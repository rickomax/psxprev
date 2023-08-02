using System;
using System.Windows.Forms;
using PSXPrev.Common;

namespace PSXPrev.Forms.Dialogs
{
    public partial class SelectRootEntityDialog : Form
    {
        private RootEntity[] _rootEntities;
        public RootEntity[] RootEntities
        {
            get => _rootEntities;
            set
            {
                _rootEntities = value;
                entitiesListBox.Items.Clear();
                foreach (var entity in _rootEntities)
                {
                    entitiesListBox.Items.Add(entity.EntityName ?? nameof(RootEntity));
                }
            }
        }

        public RootEntity SelectedRootEntity
        {
            get
            {
                if (entitiesListBox.SelectedIndex > -1)
                {
                    return _rootEntities[entitiesListBox.SelectedIndex];
                }
                return null;
            }
            set
            {
                if (_rootEntities != null)
                {
                    entitiesListBox.SelectedIndex = Array.IndexOf(_rootEntities, value);
                }
            }
        }

        public SelectRootEntityDialog()
        {
            InitializeComponent();
        }
    }
}
