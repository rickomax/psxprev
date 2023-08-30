using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Manina.Windows.Forms;

namespace PSXPrev.Forms.Controls
{
    // Fix bugs in ImageListView and override Shift+Selection behavior to make sense.
    // If the ImageListView NuGet package ever gets an update, THEN THIS CLASS MUST BE RE-EVALUATED!
    public class ExtendedImageListView : ImageListView
    {
        private readonly static MethodInfo _method_SuspendPaint;
        private readonly static FieldInfo _field_navigationManager;
        private readonly static Type _type_ImageListViewNavigationManager;
        private readonly static PropertyInfo _prop_ShiftKey;
        private readonly static PropertyInfo _prop_ControlKey;

        static ExtendedImageListView()
        {
            _method_SuspendPaint     = typeof(ImageListView).GetMethod("SuspendPaint", BindingFlags.Instance | BindingFlags.NonPublic);
            _field_navigationManager = typeof(ImageListView).GetField("navigationManager", BindingFlags.Instance | BindingFlags.NonPublic);
            if (_field_navigationManager != null)
            {
                _type_ImageListViewNavigationManager = _field_navigationManager.FieldType;
                _prop_ShiftKey   = _type_ImageListViewNavigationManager.GetProperty("ShiftKey");
                _prop_ControlKey = _type_ImageListViewNavigationManager.GetProperty("ControlKey");
            }
        }


        private readonly object _navigationManager;
        private readonly Dictionary<ImageListViewItem, bool> _highlightedItems;

        public ExtendedImageListView()
        {
            if (_field_navigationManager != null)
            {
                _navigationManager = _field_navigationManager.GetValue(this);
            }
        }

        // Change Shift+click selection to select sequentially, instead of in a rectangular area.
        // This is useful, because the ImageListView is first and foremost a list, the fact that items are
        // shown in a grid is irrelevant.
        // This also fixes it so that Ctrl+Shift+clicking an item will always select it.
        protected override void OnMouseUp(MouseEventArgs e)
        {
            var overrideSelected = false;
            var startIndex = 0;
            var endIndex   = 0;
            HashSet<ImageListViewItem> selectedItems = null;
            ImageListViewItem focusedItem = null;

            if (MultiSelect && ModifierKeys.HasFlag(Keys.Shift))
            {
                HitTest(e.Location, out var hitTestInfo);
                if (hitTestInfo != null && hitTestInfo.ItemHit)
                {
                    var item = Items[hitTestInfo.ItemIndex];

                    // Override original multi-select behavior while shift is down
                    overrideSelected = true;
                    focusedItem = item;
                    startIndex  = item.Index;
                    endIndex    = item.Index;
                    if (Items.FocusedItem != null)
                    {
                        // When shift+clicking, the focused item is always used as the origin.
                        // Shift+clicking will never changed the focused item, even when Ctrl is held down.
                        focusedItem = Items.FocusedItem;
                        startIndex  = Math.Min(Items.FocusedItem.Index, startIndex);
                        endIndex    = Math.Max(Items.FocusedItem.Index, endIndex);
                    }

                    // If Ctrl is held down, then preserve already-selected items.
                    selectedItems = new HashSet<ImageListViewItem>();
                    if (ModifierKeys.HasFlag(Keys.Control))
                    {
                        // We can't use AddRange because SelectedItems.CopyTo throws NotSupportedException...
                        // We're using a HashSet currently, so it doesn't matter, but it's still good to know.
                        foreach (var selectedItem in SelectedItems)
                        {
                            // Only add items that aren't handled by the selection range
                            if (selectedItem.Index < startIndex || selectedItem.Index > endIndex)
                            {
                                selectedItems.Add(selectedItem);
                            }
                        }
                    }
                }
            }

            // Perform the default behavior to keep the navigation manager happy
            base.OnMouseUp(e);

            // Now override selection made by the default behavior
            if (overrideSelected)
            {
                SuspendLayout();

                ClearSelection(); // Clear selection changes made by default behavior
                SelectWhere(a => (a.Index >= startIndex && a.Index <= endIndex) || selectedItems.Contains(a));
                Items.FocusedItem = focusedItem;

                ResumeLayout(true);
            }
        }

        // Fix #231: ShiftKey/ControlKey not being reset on lost focus
        protected override void OnLostFocus(EventArgs e)
        {
            if (_navigationManager != null)
            {
                _prop_ShiftKey?.SetValue(_navigationManager, false);
                _prop_ControlKey?.SetValue(_navigationManager, false);
            }

            base.OnLostFocus(e);
        }

        // Fix #232: Missing SuspendPaint in SelectWhere
        public new void SelectWhere(Func<ImageListViewItem, bool> predicate)
        {
            if (_method_SuspendPaint != null)
            {
                _method_SuspendPaint.Invoke(this, new object[0]);

                base.SelectWhere(predicate);
            }
            else
            {
                // We failed to get the SuspendPaint method, so we're forced to do things the slow way.
                SuspendLayout();

                foreach (var item in Items.Where(predicate))
                {
                    item.Selected = true;
                }

                ResumeLayout();
            }
        }

        // Fix #232: Missing SuspendPaint in UnselectWhere
        public new void UnselectWhere(Func<ImageListViewItem, bool> predicate)
        {
            if (_method_SuspendPaint != null)
            {
                _method_SuspendPaint.Invoke(this, new object[0]);

                base.UnselectWhere(predicate);
            }
            else
            {
                // We failed to get the SuspendPaint method, so we're forced to do things the slow way.
                SuspendLayout();

                foreach (var item in Items.Where(predicate))
                {
                    item.Selected = false;
                }

                ResumeLayout();
            }
        }
    }
}
