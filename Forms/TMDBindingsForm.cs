using System;
using System.Collections.Generic;
using System.Windows.Forms;
using PSXPrev.Classes;

namespace PSXPrev.Forms
{
    public partial class TMDBindingsForm : Form
    {
        private readonly Dictionary<uint, uint> _bindings;
        private readonly AnimationObject _currentAnimation;

        private static TMDBindingsForm _instance;

        public TMDBindingsForm(Dictionary<uint, uint> bindings, AnimationObject currentAnimation)
        {
            InitializeComponent();
            _bindings = bindings;
            _currentAnimation = currentAnimation;
        }

        private void TMDBindingForm_Load(object sender, EventArgs e)
        {
            AddRecursively(_currentAnimation);
            bindingPropertyGrid.SelectedObject = new DictionaryPropertyGridAdapter(_bindings);
        }

        private void AddRecursively(AnimationObject animationObject)
        {
            if (animationObject.TMDID.Count > 0)
            {
                foreach (var tmdID in animationObject.TMDID)
                {
                    if (!_bindings.ContainsKey(tmdID))
                    {
                        _bindings.Add(tmdID, tmdID - 1);
                    }
                }
            }
            foreach (var child in animationObject.Children)
            {
                AddRecursively(child);
            }
        }

        public static void ShowTool(Dictionary<uint, uint> bindings, AnimationObject animationObject)
        {
            if (_instance == null)
            {
                _instance = new TMDBindingsForm(bindings, animationObject);
            }
            _instance.Show();
        }

        private void TMDBindingForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _instance = null;
        }
    }
}
