using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Collada141;
using Microsoft.WindowsAPICodePack.Dialogs;
using PSXPrev.Common;
using PSXPrev.Common.Animator;
using PSXPrev.Common.Exporters;

namespace PSXPrev.Forms
{
    public partial class ExportModelsForm : Form
    {
        private static string _lastSelectedPath = "";
        private static string _lastFormat = "OBJ";
        private static ExportModelOptions _lastOptions = new ExportModelOptions();

        private string _format;

        private RootEntity[] _entities;
        public RootEntity[] Entities
        {
            get => _entities;
            set
            {
                _entities = value;
                exportingModelsLabel.Text = $"Exporting {_entities.Length} Models";
            }
        }

        private Animation[] _animations;
        public Animation[] Animations
        {
            get => _animations;
            set
            {
                _animations = value;
            }
        }
        public AnimationBatch AnimationBatch { get; private set; }

        public ExportModelsForm()
        {
            InitializeComponent();
            // Use the last export options
            LoadOptions(_lastSelectedPath, _lastFormat, _lastOptions);
        }

        private void LoadOptions(string selectedPath, string format, ExportModelOptions options)
        {
            fileNameTextBox.Text = selectedPath;
            _format = format;

            // Update Format radio buttons
            switch (_format)
            {
                case "OBJ":
                    formatOBJRadioButton.Checked = true;
                    break;
                case "PLY":
                    formatPLYRadioButton.Checked = true;
                    break;
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
            // Update Textures check boxes
            optionShareTexturesCheckBox.Checked = options.ShareTextures;
            optionTiledTexturesCheckBox.Checked = options.TiledTextures;
            optionRedrawTexturesCheckBox.Checked = options.RedrawTextures;

            // Update Options check boxes
            optionMergeModelsCheckBox.Checked = options.MergeEntities;
            optionAttachLimbsCheckBox.Checked = options.AttachLimbs;
            optionExperimentalVertexColorCheckBox.Checked = options.ExperimentalOBJVertexColor;
        }

        private void SaveOptions(ExportModelOptions options)
        {
            _lastSelectedPath = fileNameTextBox.Text;
            _lastFormat = _format;
            _lastOptions = options;
        }


        private void ExportModelsForm_Load(object sender, EventArgs e)
        {
            toolTip.SetToolTip(texturesIndividualRadioButton, "Required texture pages (and optionally tiled textures)\nwill be exported as individual files");
            toolTip.SetToolTip(texturesSingleRadioButton, "Required textures pages (and optionally tiled textures)\nwill be combined into a single file");
            toolTip.SetToolTip(optionRedrawTexturesCheckBox, "Models with associated textures will draw these\ntextures to the VRAM pages before exporting");
            toolTip.SetToolTip(optionShareTexturesCheckBox, "All exported models will reference the same\nexported texture files");
            toolTip.SetToolTip(optionMergeModelsCheckBox, "The geometry for all models will be merged\nand exported as a single file");
        }

        private void selectFolderButton_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new CommonOpenFileDialog())
            {
                folderBrowserDialog.IsFolderPicker = true;
                folderBrowserDialog.Title = "Select a Folder to Scan";
                // Parameter name used to avoid overload resolution with WPF Window, which we don't have a reference to.
                if (folderBrowserDialog.ShowDialog(ownerWindowHandle: Handle) == CommonFileDialogResult.Ok)
                {
                    fileNameTextBox.Text = folderBrowserDialog.FileName;
                }
            }
        }

        private void fileNameTextBox_TextChanged(object sender, EventArgs e)
        {
            exportButton.Enabled = Directory.Exists(fileNameTextBox.Text);
        }

        private void formatRadioButtons_CheckedChanged(object sender, EventArgs e)
        {
            // Only handle checked event once by seeing if the sender is checked.
            if (sender is RadioButton radioButton && radioButton.Checked)
            {
                _format = (string)radioButton.Tag;

                // We can optionally coerce the textures radio button to Single.
                // But that would be inconvenient if the user is swapping back and forth between formats.
                /*if (formatPLYRadioButton.Checked && texturesIndividualRadioButton.Checked)
                {
                    texturesSingleRadioButton.Checked = true;
                }
                else if (!texturesIndividualRadioButton.Enabled && texturesSingleRadioButton.Checked)
                {
                    texturesIndividualRadioButton.Checked = true;
                }*/
                texturesIndividualRadioButton.Enabled = !formatPLYRadioButton.Checked;

                optionExperimentalVertexColorCheckBox.Enabled = formatOBJRadioButton.Checked;

                animationsOnRadioButton.Checked = false;
                animationsOffRadioButton.Checked = true;

                animationsOffRadioButton.Enabled = animationsOnRadioButton.Enabled = _format == "glTF2";
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

        private void ExportModelsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (DialogResult != DialogResult.OK)
            {
                return;
            }

            var selectedPath = fileNameTextBox.Text;

            var options = new ExportModelOptions
            {
                MergeEntities = optionMergeModelsCheckBox.Checked,
                AttachLimbs = optionAttachLimbsCheckBox.Checked,

                ExportTextures = !texturesOffRadioButton.Checked,
                ShareTextures = optionShareTexturesCheckBox.Checked,
                TiledTextures = optionTiledTexturesCheckBox.Checked,
                RedrawTextures = optionRedrawTexturesCheckBox.Checked,
                SingleTexture = texturesSingleRadioButton.Checked,

                ExperimentalOBJVertexColor = optionExperimentalVertexColorCheckBox.Checked,

                ExportAnimations = animationsOnRadioButton.Checked
            };

            switch (_format)
            {
                case "OBJ":
                    var objExporter = new OBJExporter();
                    objExporter.Export(Entities, selectedPath, options);
                    break;
                case "PLY":
                    var plyExporter = new PLYExporter();
                    plyExporter.Export(Entities, selectedPath, options);
                    break;
                case "glTF2":
                    var glTF2Exporter = new glTF2Exporter();
                    glTF2Exporter.Export(Entities, Animations, AnimationBatch, selectedPath, options);
                    break;
            }

            SaveOptions(options);
        }


        public static bool Show(IWin32Window owner, RootEntity[] entities, Animation[] animations = null, AnimationBatch animationBatch = null)
        {
            using (var form = new ExportModelsForm())
            {
                form.Entities = entities;
                form.Animations = animations;
                form.AnimationBatch = animationBatch;
                form.checkedAnimationsListBox.Items.Clear();
                if (animations != null && animations.Length > 0)
                {
                    foreach (var animation in animations)
                    {
                        form.checkedAnimationsListBox.Items.Add(animation.AnimationName);
                    }
                }
                else
                {
                    form.checkedAnimationsListBox.Items.Add("Please select or check an Animation under the Animations tab");
                }
                return form.ShowDialog(owner) == DialogResult.OK;
            }
        }
    }
}
