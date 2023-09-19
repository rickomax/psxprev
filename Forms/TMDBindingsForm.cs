using System;
using System.Collections.Generic;
using System.Windows.Forms;
using PSXPrev.Common;
using PSXPrev.Common.Animator;
using PSXPrev.Forms.Utils;

namespace PSXPrev.Forms
{
    public partial class TMDBindingsForm : Form
    {
        private static Animation _currentAnimation;
        private readonly Dictionary<object, string> _tmdidObjectNames = new Dictionary<object, string>();

        private static TMDBindingsForm _instance;

        private static object _clipboard;

        public static bool IsVisible { get; private set; }

        public TMDBindingsForm()
        {
            InitializeComponent();
        }

        private void AddRecursively(AnimationObject animationObject)
        {
            foreach (var tmdID in animationObject.TMDID)
            {
                if (!_currentAnimation.TMDBindings.ContainsKey(tmdID))
                {
                    _currentAnimation.TMDBindings.Add(tmdID, tmdID);
                }
                if (!string.IsNullOrEmpty(animationObject.ObjectName) && !_tmdidObjectNames.ContainsKey(tmdID))
                {
                    _tmdidObjectNames.Add(tmdID, animationObject.ObjectName);
                }
            }
            foreach (var child in animationObject.Children)
            {
                AddRecursively(child);
            }
        }

        private static int KeyOrderer(object a, object b)
        {
            return ((IComparable)a).CompareTo(b);
        }

        private string KeyDisplayName(object key)
        {
            var namePostfix = string.Empty;
            if (_tmdidObjectNames.TryGetValue(key, out var objectName))
            {
                namePostfix = $" {objectName}";
            }
            return $"ID {key}{namePostfix}";
        }

        private static string KeyDescription(object key)
        {
            return $"Change what animation TMD ID {key} maps to in the model's TMD IDs.";
        }

        private void Reload(Animation currentAnimation)
        {
            _currentAnimation = currentAnimation;
            _tmdidObjectNames.Clear();
            AddRecursively(_currentAnimation.RootAnimationObject);
            var d = new DictionaryPropertyGridAdapter(_currentAnimation.TMDBindings,
                                                      keyOrderer: KeyOrderer,
                                                      keyDisplayName: KeyDisplayName,
                                                      keyDescription: KeyDescription);
            bindingPropertyGrid.SelectedObject = d;
        }

        public static void ShowTool(IWin32Window owner, Animation currentAnimation)
        {
            if (_instance == null)
            {
                _instance = new TMDBindingsForm();
                _instance.Show(owner);
            }
            _instance.Reload(currentAnimation);
            IsVisible = true;
        }

        public static void CloseTool()
        {
            if (_instance != null)
            {
                _instance.Close();
            }
        }

        private void TMDBindingForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _instance = null;
            IsVisible = false;
        }

        private void copyBindingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _clipboard = _currentAnimation.TMDBindings;
        }

        private void pasteBindingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_clipboard is Dictionary<uint, uint> dataObject)
            {
                _currentAnimation.TMDBindings.Clear();
                foreach (var kvp in dataObject)
                {
                    _currentAnimation.TMDBindings.Add(kvp.Key, kvp.Value);
                }
                _instance.bindingPropertyGrid.Refresh();
            }
        }

    }
}
