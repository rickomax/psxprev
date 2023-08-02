﻿using System;
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
        private string _format;

        private RootEntity[] _entities;
        public RootEntity[] Entities
        {
            get => _entities;
            set
            {
                _entities = value;
                var plural = _entities.Length != 1 ? "s" : string.Empty;
                exportingModelsLabel.Text = $"Exporting {_entities.Length} Model{plural}";
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
                        checkedAnimationsListBox.Items.Add(animation.AnimationName);
                    }
                }
                else
                {
                    checkedAnimationsListBox.Items.Add("Please select or check an Animation under the Animations tab");
                }
            }
        }
        public AnimationBatch AnimationBatch { get; private set; }

        public ExportModelsForm()
        {
            InitializeComponent();
        }


        private void ExportModelsForm_Load(object sender, EventArgs e)
        {
            toolTip.SetToolTip(texturesIndividualRadioButton, "Required texture pages (and optionally tiled textures)\nwill be exported as individual files");
            toolTip.SetToolTip(texturesSingleRadioButton, "Required textures pages (and optionally tiled textures)\nwill be combined into a single file");
            toolTip.SetToolTip(optionRedrawTexturesCheckBox, "Models with associated textures will draw these\ntextures to the VRAM pages before exporting");
            toolTip.SetToolTip(optionShareTexturesCheckBox, "All exported models will reference the same\nexported texture files");
            toolTip.SetToolTip(optionMergeModelsCheckBox, "The geometry for all models will be merged\nand exported as a single file");

            // Use the last export options
            ReadSettings(Settings.Instance.ExportModelOptions);

            // Debugging: Instantly export with specified settings.
            //{
            //    filePathTextBox.Text = @"";
            //    formatGLTF2RadioButton.Checked = true;
            //    animationsOnRadioButton.Checked = true;
            //    optionRedrawTexturesCheckBox.Checked = true;
            //    DialogResult = DialogResult.OK;
            //}
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
                texturesIndividualRadioButton.Enabled = _format != ExportModelOptions.PLY;

                optionMergeModelsCheckBox.Enabled = _format != ExportModelOptions.GLTF2;
                optionExperimentalVertexColorCheckBox.Enabled = _format == ExportModelOptions.OBJ;

                animationsGroupBox.Enabled = _format == ExportModelOptions.GLTF2;

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

            var selectedPath = filePathTextBox.Text;

            var options = new ExportModelOptions
            {
                // These settings are only present for loading and saving purposes.
                Path = selectedPath,
                Format = _format,

                ExportTextures = !texturesOffRadioButton.Checked,
                ShareTextures = optionShareTexturesCheckBox.Checked,
                TiledTextures = optionTiledTexturesCheckBox.Checked,
                RedrawTextures = optionRedrawTexturesCheckBox.Checked,
                SingleTexture = texturesSingleRadioButton.Checked,

                MergeEntities = optionMergeModelsCheckBox.Checked,
                AttachLimbs = optionAttachLimbsCheckBox.Checked,
                ExperimentalOBJVertexColor = optionExperimentalVertexColorCheckBox.Checked,

                ExportAnimations = !animationsOffRadioButton.Checked,
            };

            WriteSettings(options);

            switch (_format)
            {
                case ExportModelOptions.OBJ:
                    var objExporter = new OBJExporter();
                    objExporter.Export(Entities, selectedPath, options);
                    break;
                case ExportModelOptions.PLY:
                    var plyExporter = new PLYExporter();
                    plyExporter.Export(Entities, selectedPath, options);
                    break;
                case ExportModelOptions.GLTF2:
                    var glTF2Exporter = new glTF2Exporter();
                    glTF2Exporter.Export(Entities, Animations, AnimationBatch, selectedPath, options);
                    break;
            }
        }

        private void ReadSettings(ExportModelOptions options)
        {
            if (options == null)
            {
                options = new ExportModelOptions();
            }

            filePathTextBox.Text = options.Path ?? string.Empty;
            _format = options.Format ?? ExportModelOptions.DefaultFormat;

            // Update Format radio buttons
            foreach (var control in formatGroupBox.Controls)
            {
                if (control is RadioButton radioButton)
                {
                    var tag = (string)radioButton.Tag;
                    if (string.Equals(tag, _format, StringComparison.InvariantCultureIgnoreCase))
                    {
                        radioButton.Checked = true;
                        // Force checked changed event in-case this is already the checked radio button.
                        formatRadioButtons_CheckedChanged(radioButton, EventArgs.Empty);
                        break;
                    }
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
            foreach (var control in texturesGroupBox.Controls)
            {
                if (control is RadioButton radioButton && radioButton.Checked)
                {
                    // Force checked changed event in-case this is already the checked radio button.
                    texturesRadioButtons_CheckedChanged(radioButton, EventArgs.Empty);
                    break;
                }
            }
            // Update Textures check boxes
            optionShareTexturesCheckBox.Checked = options.ShareTextures;
            optionTiledTexturesCheckBox.Checked = options.TiledTextures;
            optionRedrawTexturesCheckBox.Checked = options.RedrawTextures;

            // Update Options check boxes
            optionMergeModelsCheckBox.Checked = options.MergeEntities;
            optionAttachLimbsCheckBox.Checked = options.AttachLimbs;
            optionExperimentalVertexColorCheckBox.Checked = options.ExperimentalOBJVertexColor;

            // Update Animations radio buttons
            if (!options.ExportAnimations)
            {
                animationsOffRadioButton.Checked = true;
            }
            else
            {
                animationsOnRadioButton.Checked = true;
            }
            foreach (var control in animationsGroupBox.Controls)
            {
                if (control is RadioButton radioButton && radioButton.Checked)
                {
                    // Force checked changed event in-case this is already the checked radio button.
                    animationsRadioButtons_CheckedChanged(radioButton, EventArgs.Empty);
                    break;
                }
            }
        }

        private void WriteSettings(ExportModelOptions options)
        {
            Settings.Instance.ExportModelOptions = options.Clone();
        }


        public static bool Show(IWin32Window owner, RootEntity[] entities, Animation[] animations = null, AnimationBatch animationBatch = null)
        {
            using (var form = new ExportModelsForm())
            {
                form.Entities = entities;
                form.Animations = animations;
                form.AnimationBatch = animationBatch;
                return form.ShowDialog(owner) == DialogResult.OK;
            }
        }
    }
}