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

        private static TMDBindingsForm _instance;

        private static object _clipboard;

        public static bool IsVisible { get; private set; }

        public TMDBindingsForm()
        {
            InitializeComponent();
        }

        private void TMDBindingForm_Load(object sender, EventArgs e)
        {
        }

        private static void AddRecursively(AnimationObject animationObject)
        {
            if (animationObject.TMDID.Count > 0)
            {
                foreach (var tmdID in animationObject.TMDID)
                {
                    if (!_currentAnimation.TMDBindings.ContainsKey(tmdID))
                    {
                        _currentAnimation.TMDBindings.Add(tmdID, tmdID - 1);
                    }
                }
            }
            foreach (var child in animationObject.Children)
            {
                AddRecursively(child);
            }
        }

        private void Reload(Animation currentAnimation)
        {
            _currentAnimation = currentAnimation;
            AddRecursively(_currentAnimation.RootAnimationObject);
            bindingPropertyGrid.SelectedObject = new DictionaryPropertyGridAdapter(_currentAnimation.TMDBindings);
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
        {;
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
