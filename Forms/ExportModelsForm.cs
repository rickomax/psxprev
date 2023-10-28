using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Collada141;
using Microsoft.WindowsAPICodePack.Dialogs;
using PSXPrev.Common;
using PSXPrev.Common.Animator;
using PSXPrev.Common.Exporters;
using PSXPrev.Forms.Utils;

namespace PSXPrev.Forms
{
    public partial class ExportModelsForm : Form
    {
        private string _format;

        private RootEntity[] _entities;
        public RootEntity[] Entities
        {
            get => _entities;
            set
            {
                _entities = value;
                var count = _entities?.Length ?? 0;
                var plural = count != 1 ? "s" : string.Empty;
                exportingModelsLabel.Text = $"Exporting {count} Model{plural}";
            }
        }

        private Animation[] _animations;
        public Animation[] Animations
        {
            get => _animations;
            set
            {
                _animations = value;
                checkedAnimationsListBox.Items.Clear();
                if (_animations != null && _animations.Length > 0)
                {
                    foreach (var animation in _animations)
                    {
                        checkedAnimationsListBox.Items.Add(animation.Name);
                    }
                }
                else
                {
                    checkedAnimationsListBox.Items.Add("Please select or check an Animation under the Animations tab");
                }
            }
        }

        public ExportModelOptions Options { get; private set; }

        public ExportModelsForm()
        {
            InitializeComponent();

            // Set default values for combo boxes
            optionModelGroupingComboBox.SelectedIndex = 0;
        }


        private void ExportModelsForm_Load(object sender, EventArgs e)
        {
            // Use the last export options
            ReadSettings(Settings.Instance, Settings.Instance.ExportModelOptions);
        }

        private void selectFolderButton_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new CommonOpenFileDialog())
            {
                folderBrowserDialog.IsFolderPicker = true;
                folderBrowserDialog.Title = "Select a Folder to Export to";
                // Parameter name used to avoid overload resolution with WPF Window, which we don't have a reference to.
                if (folderBrowserDialog.ShowDialog(ownerWindowHandle: Handle) == CommonFileDialogResult.Ok)
                {
                    filePathTextBox.Text = folderBrowserDialog.FileName;
                }
            }
        }

        private void filePathTextBox_TextChanged(object sender, EventArgs e)
        {
            exportButton.Enabled = Directory.Exists(filePathTextBox.Text);
        }

        private void formatRadioButtons_CheckedChanged(object sender, EventArgs e)
        {
            // Only handle checked event once by seeing if the sender is checked.
            if (sender is RadioButton radioButton && radioButton.Checked)
            {
                _format = (string)radioButton.Tag;

                // We can optionally coerce the textures radio button to Single.
                // But that would be inconvenient if the user is swapping back and forth between formats.
                /*if (_format == ExportModelOptions.PLY && texturesIndividualRadioButton.Checked)
                {
                    texturesSingleRadioButton.Checked = true;
                }
                else if (!texturesIndividualRadioButton.Enabled && texturesSingleRadioButton.Checked)
                {
                    texturesIndividualRadioButton.Checked = true;
                }*/
                texturesIndividualRadioButton.Enabled = _format != ExportModelFormats.PLY && _format != ExportModelFormats.DAE;

                optionExperimentalVertexColorCheckBox.Enabled = _format == ExportModelFormats.OBJ;

                optionVertexIndexReuseCheckBox.Enabled = _format == ExportModelFormats.OBJ || _format == ExportModelFormats.DAE;

                optionHumanReadableCheckBox.Enabled = _format == ExportModelFormats.GLTF2 || _format == ExportModelFormats.DAE;

                animationsGroupBox.Enabled = _format == ExportModelFormats.GLTF2;

                animationsOffRadioButton.Checked = true; // Always turn off animations when switching formats?
            }
        }

        private void texturesRadioButtons_CheckedChanged(object sender, EventArgs e)
        {
            // Only handle checked event once by seeing if the sender is checked.
            if (sender is RadioButton radioButton && radioButton.Checked)
            {
                optionRedrawTexturesCheckBox.Enabled = !texturesOffRadioButton.Checked;
                optionShareTexturesCheckBox.Enabled = !texturesOffRadioButton.Checked;
                optionTiledTexturesCheckBox.Enabled = !texturesOffRadioButton.Checked;
            }
        }

        private void animationsRadioButtons_CheckedChanged(object sender, EventArgs e)
        {
            // Only handle checked event once by seeing if the sender is checked.
            if (sender is RadioButton radioButton && radioButton.Checked)
            {
                checkedAnimationsListBox.Enabled = !animationsOffRadioButton.Checked;
            }
        }

        private void ExportModelsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // todo: Should we always save settings?
            //WriteSettings(Settings.Instance, null); // For now, at least save UI settings (none yet).
            var options = CreateOptions();
            WriteSettings(Settings.Instance, options);
            if (Settings.ImplicitSave)
            {
                Settings.Instance.Save();
            }

            Options = options;
        }

        private ExportModelOptions CreateOptions()
        {
            var options = new ExportModelOptions
            {
                // These settings are only present for loading and saving purposes.
                Path = filePathTextBox.Text,
                Format = _format,

                ExportTextures = !texturesOffRadioButton.Checked,
                ShareTextures = optionShareTexturesCheckBox.Checked,
                TiledTextures = optionTiledTexturesCheckBox.Checked,
                RedrawTextures = optionRedrawTexturesCheckBox.Checked,
                SingleTexture = texturesSingleRadioButton.Checked,

                AttachLimbs = optionAttachLimbsCheckBox.Checked,
                ExperimentalOBJVertexColor = optionExperimentalVertexColorCheckBox.Checked,
                VertexIndexReuse = optionVertexIndexReuseCheckBox.Checked,
                ReadableFormat = optionHumanReadableCheckBox.Checked,
                StrictFloatFormat = optionStrictFloatsCheckBox.Checked,

                ExportAnimations = !animationsOffRadioButton.Checked,
            };
            switch (optionModelGroupingComboBox.SelectedIndex)
            {
                case 0:
                default:
                    options.ModelGrouping = ExportModelGrouping.Default;
                    break;
                case 1:
                    options.ModelGrouping = ExportModelGrouping.SplitSubModelsByTMDID;
                    break;
                case 2:
                    options.ModelGrouping = ExportModelGrouping.GroupAllModels;
                    break;
            }

            return options;
        }

        private void ReadSettings(Settings settings, ExportModelOptions options)
        {
            if (options == null)
            {
                options = new ExportModelOptions();
            }

            filePathTextBox.Text = options.Path ?? string.Empty;
            _format = options.Format ?? ExportModelOptions.DefaultFormat;

            // Update Format radio buttons
            foreach (var radioButton in formatGroupBox.EnumerateAllControlsOfType<RadioButton>())
            {
                var tag = (string)radioButton.Tag;
                if (tag == _format)
                {
                    radioButton.Checked = true;
                    // Force checked changed event in-case this is already the checked radio button.
                    formatRadioButtons_CheckedChanged(radioButton, EventArgs.Empty);
                    break;
                }
            }

            // Update Textures radio buttons
            if (!options.ExportTextures)
            {
                texturesOffRadioButton.Checked = true;
            }
            else if (!options.SingleTexture)
            {
                texturesIndividualRadioButton.Checked = true;
            }
            else
            {
                texturesSingleRadioButton.Checked = true;
            }
            // Force checked changed event in-case this is already the checked radio button.
            foreach (var radioButton in texturesGroupBox.EnumerateAllControlsOfType<RadioButton>())
            {
                if (radioButton.Checked)
                {
                    texturesRadioButtons_CheckedChanged(radioButton, EventArgs.Empty);
                    break;
                }
            }
            // Update Textures check boxes
            optionShareTexturesCheckBox.Checked = options.ShareTextures;
            optionTiledTexturesCheckBox.Checked = options.TiledTextures;
            optionRedrawTexturesCheckBox.Checked = options.RedrawTextures;

            // Update Options check boxes
            switch (options.ModelGrouping)
            {
                case ExportModelGrouping.Default:
                default:
                    optionModelGroupingComboBox.SelectedIndex = 0;
                    break;
                case ExportModelGrouping.SplitSubModelsByTMDID:
                    optionModelGroupingComboBox.SelectedIndex = 1;
                    break;
                case ExportModelGrouping.GroupAllModels:
                    optionModelGroupingComboBox.SelectedIndex = 2;
                    break;
            }
            optionAttachLimbsCheckBox.Checked = options.AttachLimbs;
            optionExperimentalVertexColorCheckBox.Checked = options.ExperimentalOBJVertexColor;
            optionVertexIndexReuseCheckBox.Checked = options.VertexIndexReuse;
            optionHumanReadableCheckBox.Checked = options.ReadableFormat;
            optionStrictFloatsCheckBox.Checked = options.StrictFloatFormat;

            // Update Animations radio buttons
            if (!options.ExportAnimations)
            {
                animationsOffRadioButton.Checked = true;
            }
            else
            {
                animationsOnRadioButton.Checked = true;
            }
            // Force checked changed event in-case this is already the checked radio button.
            foreach (var radioButton in animationsGroupBox.EnumerateAllControlsOfType<RadioButton>())
            {
                if (radioButton.Checked)
                {
                    animationsRadioButtons_CheckedChanged(radioButton, EventArgs.Empty);
                    break;
                }
            }
        }

        private void WriteSettings(Settings settings, ExportModelOptions options)
        {
            if (options != null)
            {
                settings.ExportModelOptions = options.Clone();
            }
        }


        public static int Export(ExportModelOptions options, RootEntity[] entities, Animation[] animations = null)
        {
            var count = 0;
            switch (options.Format)
            {
                case ExportModelFormats.OBJ:
                    var objExporter = new OBJExporter();
                    count = objExporter.Export(options, entities);
                    break;
                case ExportModelFormats.PLY:
                    var plyExporter = new PLYExporter();
                    count = plyExporter.Export(options, entities);
                    break;
                case ExportModelFormats.GLTF2:
                    var glTF2Exporter = new glTF2Exporter();
                    count = glTF2Exporter.Export(options, entities, animations);
                    break;
                case ExportModelFormats.DAE:
                    var daeExporter = new DAEExporter();
                    count = daeExporter.Export(options, entities);
                    break;
            }
            return count;
        }

        public static ExportModelOptions Show(IWin32Window owner, RootEntity[] entities, Animation[] animations = null)
        {
            using (var form = new ExportModelsForm())
            {
                form.Entities = entities;
                form.Animations = animations;
                if (form.ShowDialog(owner) == DialogResult.OK)
                {
                    return form.Options;
                }
                return null;
            }
        }
    }
}
