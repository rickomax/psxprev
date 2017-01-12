using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenTK;
using PSXPrev.Classes.Entities;
using PSXPrev.Classes.Exporters;
using PSXPrev.Classes.Extensions;
using PSXPrev.Classes.Mesh;
using PSXPrev.Classes.Scene;
using PSXPrev.Classes.Texture;
using PSXPrev.Classes.Utils;
using Color = PSXPrev.Classes.Color.Color;

namespace PSXPrev
{
    public partial class PreviewForm : Form
    {
        public enum SelectionType
        {
            None,
            Entity,
            Model,
            Triangle
        }

        private float _lastMouseX;
        private float _lastMouseY;
        private GLControl _openTkControl;
        private Scene _scene;
        private int _selectedTriangle;
        private Texture[] _vramPage;

        public PreviewForm()
        {
            SetupCulture();
            SetupInternals();
            InitializeComponent();
            SetupControls();
        }

        public System.Drawing.Color SceneBackColor
        {
            set
            {
                if (_scene == null)
                {
                    throw new Exception("Window must be initialized");
                }

                _scene.ClearColor = new Color
                {
                    R = value.R / 256f,
                    G = value.G / 256f,
                    B = value.B / 256f
                };
            }
        }

        private void SetupCulture()
        {
            var customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
        }

        private void SetupInternals()
        {
            _vramPage = new Texture[32];
            _scene = new Scene();
            Toolkit.Init();
        }

        private void SetupControls()
        {
            _openTkControl = new GLControl
            {
                BackColor = System.Drawing.Color.Black,
                Location = new Point(180, 3),
                Name = "openTKControl",
                Size = new Size(800, 600),
                TabIndex = 15,
                VSync = true
            };
            _openTkControl.Load += openTKControl_Load;
            _openTkControl.MouseWheel += openTkControl_MouseWhell;
            _openTkControl.MouseMove += openTkControl_MouseEvent;

            entitiesTabPage.Controls.Add(_openTkControl);
        }

        private void SetupScene()
        {
            _scene.Initialise(Width, Height);
        }

        private void SetupColors()
        {
            SceneBackColor = System.Drawing.Color.LightSkyBlue;
        }

        private void SetupTextures()
        {
            var textures = Program.AllTextures;
            for (var index = 0; index < textures.Count; index++)
            {
                var texture = textures[index];
                thumbsImageList.Images.Add(texture.Bitmap);
                texturesListView.Items.Add(texture.TextureName, index);
            }
        }

        private void SetupEntities()
        {
            entitiesTreeView.BeginUpdate();
            var entities = Program.AllEntities;
            foreach (var entity in entities)
            {
                var entityNode = new TreeNode(entity.EntityName) { Tag = entity };
                entitiesTreeView.Nodes.Add(entityNode);
                for (var m = 0; m < entity.ChildEntities.Count; m++)
                {
                    var model = entity.ChildEntities[m];
                    var modelNode = new TreeNode("Sub-Model " + m) { Tag = model };
                    entityNode.Nodes.Add(modelNode);
                    modelNode.HideCheckBox();
                    modelNode.HideCheckBox();
                    if (Program.Debug)
                    {
                        for (var t = 0; t < model.Triangles.Count; t++)
                        {
                            var triangle = model.Triangles[t];
                            var triangleNode = new TreeNode("Triangle " + t) { Tag = triangle };
                            modelNode.Nodes.Add(triangleNode);
                            triangleNode.HideCheckBox();
                            triangleNode.HideCheckBox();
                        }
                    }
                }
            }
            entitiesTreeView.EndUpdate();
        }

        private void SetupVram()
        {
            for (var i = 0; i < 32; i++)
            {
                var texture = new Texture(256, 256, 0, 0, 32, i);
                var textureBitmap = texture.Bitmap;
                var graphics = Graphics.FromImage(textureBitmap);
                graphics.Clear(System.Drawing.Color.White);
                _vramPage[i] = texture;
                _scene.UpdateTexture(textureBitmap, i);
            }
            var entities = Program.AllEntities;
            foreach (var entity in entities)
            {
                foreach (var entityBase in entity.ChildEntities)
                {
                    entityBase.Texture = _vramPage[entityBase.TexturePage];
                }
            }
        }

        private void previewForm_Load(object sender, EventArgs e)
        {
        }

        private List<RootEntity> GetSelectectedEntities()
        {
            //var entities = Program.AllEntities;
            var selectedEntities = new List<RootEntity>();
            for (var i = 0; i < entitiesTreeView.Nodes.Count; i++)
            {
                var node = entitiesTreeView.Nodes[i];
                if (node.Checked)
                {
                    var entity = (RootEntity) node.Tag;
                    selectedEntities.Add(entity);
                }
            }
            if (selectedEntities.Count == 0)
            {
                return null;
            }
            return selectedEntities;
        }

        private DialogResult ShowEntityFolderSelect(out string path)
        {
            var fbd = new FolderBrowserDialog { Description = "Select the output folder" };
            var result = fbd.ShowDialog();
            path = fbd.SelectedPath;
            return result;
        }

        private void exportEntityButton_Click(object sender, EventArgs e)
        {
            cmsModelExport.Show(exportEntityButton, exportEntityButton.Width, 0);
        }

        private void exportBitmapButton_Click(object sender, EventArgs e)
        {
            var selectedIndices = texturesListView.SelectedIndices;
            var selectedCount = selectedIndices.Count;
            if (selectedCount == 0)
            {
                MessageBox.Show("Select the textures to export first");
                return;
            }
            var fbd = new FolderBrowserDialog { Description = "Select the output folder" };
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                var textures = Program.AllTextures;
                var selectedTextures = new Texture[selectedCount];
                for (var i = 0; i < selectedCount; i++)
                {
                    selectedTextures[i] = textures[selectedIndices[i]];
                }
                var exporter = new PngExporter();
                exporter.Export(selectedTextures, fbd.SelectedPath);
                MessageBox.Show("Textures exported");
            }
        }

        private void openTkControl_MouseWhell(object sender, MouseEventArgs e)
        {
            _scene.CameraDistance -= e.Delta * Scene.MouseSensivity * _scene.CameraDistanceIncrement;
        }

        private void openTkControl_MouseEvent(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _scene.CameraYaw -= (e.X - _lastMouseX) * Scene.MouseSensivity;
                _scene.CameraPitch += (e.Y - _lastMouseY) * Scene.MouseSensivity;
            }
            _lastMouseX = e.X;
            _lastMouseY = e.Y;
        }

        private void entitiesTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            object selection;
            TreeNode selectedNode;
            var selectionType = SetupModelOrEntity(true, out selection, out selectedNode);
            ResetSelection(selectionType);
            modelPropertyGrid.SelectedObject = selection;
            switch (selectionType)
            {
                case SelectionType.Triangle:
                    var triangle = GetSelectedTriangle();
                    _selectedTriangle = (ushort)triangle.Index;
                    break;
            }
        }

        private void ResetSelection(SelectionType selectionType)
        {
            if (selectionType != SelectionType.Entity && selectionType != SelectionType.Model)
            {
                entitiesTreeView.SelectedNode = null;
                entitiesTreeView.SelectedNode = null;
                modelPropertyGrid.SelectedObject = null;
            }
            _selectedTriangle = int.MaxValue;
            _scene.LineBatch.Reset();
        }

        private SelectionType SetupModelOrEntity(bool focus, out object selectedModelOrEntity, out TreeNode selectedNode)
        {
            selectedModelOrEntity = null;
            selectedNode = entitiesTreeView.SelectedNode;
            if (selectedNode == null)
            {
                return SelectionType.None;
            }
            ModelEntity model;
            var nodeLevel = selectedNode.Level;
            switch (nodeLevel)
            {
                case 0:
                    var entity = GetSelectedEntity();
                    SetupEntityBatch(entity, focus);
                    selectedModelOrEntity = entity;
                    return SelectionType.Entity;
                case 1:
                    model = GetSelectedModel();
                    SetupModelBatch(model, focus);
                    selectedModelOrEntity = model;
                    return SelectionType.Model;
                case 2:
                    model = GetSelectedModel();
                    SetupModelBatch(model, false);
                    selectedModelOrEntity = model;
                    return SelectionType.Triangle;
            }
            return SelectionType.None;
        }

        private void SetupModelBatch(ModelEntity model, bool focus)
        {
            _scene.MeshBatch.SetupModelBatch(model, focus);
        }

        private void SetupEntityBatch(RootEntity entity, bool focus)
        {
            _scene.MeshBatch.SetupEntityBatch(entity, focus);
        }

        private Triangle GetSelectedTriangle()
        {
            var selectedNode = entitiesTreeView.SelectedNode;
            if (selectedNode == null)
            {
                return null;
            }
            var triangle = (Triangle)selectedNode.Tag;
            return triangle;
        }

        private void UpdateModelProperties(ModelEntity model)
        {
            model.TexturePage = Math.Min(31, Math.Max(0, model.TexturePage));
        }

        private Texture GetSelectedTexture(int? index = null)
        {
            if (texturesListView.SelectedIndices.Count == 0)
            {
                return null;
            }
            var textureIndex = index ?? texturesListView.SelectedIndices[0];
            if (textureIndex < 0)
            {
                return null;
            }
            var textures = Program.AllTextures;
            return textures[textureIndex];
        }

        private RootEntity GetSelectedEntity()
        {
            var selectedNode = entitiesTreeView.SelectedNode;
            if (selectedNode == null)
            {
                return null;
            }
            var entity = (RootEntity)selectedNode.Tag;
            return entity;
        }

        private ModelEntity GetSelectedModel()
        {
            var selectedNode = entitiesTreeView.SelectedNode;
            if (selectedNode == null)
            {
                return null;
            }
            var model = (ModelEntity)selectedNode.Tag; //_rootEntities[parentIndex].ChildEntities[childIndex];
            return model;
        }

        private void drawToVRAMButton_Click(object sender, EventArgs e)
        {
            var selectedIndices = texturesListView.SelectedIndices;
            if (selectedIndices.Count == 0)
            {
                MessageBox.Show("Select the textures to draw to VRAM first");
                return;
            }
            foreach (int index in texturesListView.SelectedIndices)
            {
                var texture = GetSelectedTexture(index);
                var texturePage = texture.TexturePage;
                var textureX = texture.X;
                var textureY = texture.Y;
                var textureBitmap = texture.Bitmap;
                var textureWidth = textureBitmap.Width;
                var textureHeight = textureBitmap.Height;
                var vramPageBitmap = _vramPage[texturePage].Bitmap;
                var vramPageGraphics = Graphics.FromImage(vramPageBitmap);
                vramPageGraphics.DrawImage(textureBitmap, textureX, textureY, textureWidth, textureHeight);
                _scene.UpdateTexture(vramPageBitmap, texturePage);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var index = vramComboBox.SelectedIndex;
            if (index > -1)
            {
                vramPagePictureBox.Image = _vramPage[index].Bitmap;
            }
        }

        private void modelPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            object selection;
            TreeNode selectedNode;
            var selectionType = SetupModelOrEntity(false, out selection, out selectedNode);
            switch (selectionType)
            {
                case SelectionType.Entity:
                    var entity = (RootEntity)selection;
                    selectedNode.Text = entity.EntityName;
                    break;
                case SelectionType.Model:
                    var model = (ModelEntity)selection;
                    UpdateModelProperties(model);
                    break;
            }
        }

        private void texturePropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            var selectedNodes = texturesListView.SelectedItems;
            if (selectedNodes.Count == 0)
            {
                return;
            }
            var selectedNode = selectedNodes[0];
            if (selectedNode == null)
            {
                return;
            }
            var texture = GetSelectedTexture();
            if (texture == null)
            {
                return;
            }
            texture.X = Math.Min(255, Math.Max(0, texture.X));
            texture.Y = Math.Min(255, Math.Max(0, texture.Y));
            texture.TexturePage = Math.Min(31, Math.Max(0, texture.TexturePage));
            selectedNode.Text = texture.TextureName;
        }

        private void btnClearPage_Click(object sender, EventArgs e)
        {
            var index = vramComboBox.SelectedIndex;
            if (index <= -1)
            {
                MessageBox.Show("Select a page first");
                return;
            }
            ClearPage(index);
            MessageBox.Show("Page cleared");
        }

        private void ClearPage(int index)
        {
            var bitmap = _vramPage[index].Bitmap;
            var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(System.Drawing.Color.White);
            vramPagePictureBox.Image = bitmap;
            _scene.UpdateTexture(bitmap, index);
        }

        private void cmsModelExport_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var entities = GetSelectectedEntities();
            if (entities == null)
            {
                MessageBox.Show("Check the models to export first");
                return;
            }
            string path;
            if (ShowEntityFolderSelect(out path) == DialogResult.OK)
            {
                if (e.ClickedItem == miOBJ)
                {
                    var objExporter = new ObjExporter();
                    objExporter.Export(entities, path);
                }
                if (e.ClickedItem == miOBJVC)
                {
                    var objExporter = new ObjExporter();
                    objExporter.Export(entities, path, true);
                }
                if (e.ClickedItem == miPLY)
                {
                    var plyExporter = new PlyExporter();
                    plyExporter.Export(entities, path);
                }
                MessageBox.Show("Models exported");
            }
        }

        private void findByPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var pageIndexString = DialogUtils.ShowDialog("Find by Page", "Type the page number");
            int pageIndex;
            if (string.IsNullOrEmpty(pageIndexString))
            {
                return;
            }
            if (!int.TryParse(pageIndexString, out pageIndex))
            {
                MessageBox.Show("Invalid page number");
                return;
            }
            var textures = Program.AllTextures;
            var found = 0;
            for (var i = 0; i < texturesListView.Items.Count; i++)
            {
                var item = texturesListView.Items[i];
                item.Group = null;
                var texture = textures[i];
                if (texture.TexturePage == pageIndex)
                {
                    item.Group = texturesListView.Groups[0];
                    found++;
                }
            }
            if (found > 0)
            {
                MessageBox.Show(string.Format("Found {0} itens", found));
            }
            else
            {
                MessageBox.Show(string.Format("Nothing found"));
            }
        }

        private void texturesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (texturesListView.SelectedIndices.Count == 0 || texturesListView.SelectedIndices.Count > 1)
            {
                texturePropertyGrid.SelectedObject = null;
                return;
            }
            var texture = GetSelectedTexture();
            if (texture == null)
            {
                return;
            }
            var bitmap = texture.Bitmap;
            texturePreviewPictureBox.Image = bitmap;
            texturePreviewPictureBox.Refresh();
            texturePropertyGrid.SelectedObject = texture;
        }

        private void clearSearchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < texturesListView.Items.Count; i++)
            {
                var item = texturesListView.Items[i];
                item.Group = null;
            }
            MessageBox.Show("Results cleared");
        }
        
        private void clearAllPagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < 32; i++)
            {
                ClearPage(i);
            }
            MessageBox.Show("Pages cleared");
        }

        private void openTKControl_Load(object sender, EventArgs e)
        {
            SetupScene();
            SetupColors();
            SetupEntities();
            SetupTextures();
            SetupVram();
            ResetSelection(SelectionType.None);
            EnableRedraw(true);
        }

        private void EnableRedraw(bool b)
        {
            redrawTimer.Enabled = b;
        }

        private void Redraw()
        {
            _openTkControl.MakeCurrent();
            _scene.Draw(_selectedTriangle);
            _openTkControl.SwapBuffers();
        }

        private void redrawTimer_Tick(object sender, EventArgs e)
        {
            Redraw();
        }

        private void menusTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (menusTabControl.SelectedTab.TabIndex)
            {
                case 0:
                    _openTkControl.Parent = menusTabControl.TabPages[0];
                    _openTkControl.Show();
                    break;
                case 3:
                    _openTkControl.Parent = menusTabControl.TabPages[3];
                    _openTkControl.Show();
                    break;
                default:
                    _openTkControl.Parent = this;
                    _openTkControl.Hide();
                    break;
            }
        }
        
        private void wireframeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _scene.WireFrame = wireframeToolStripMenuItem.Checked;
        }
    }
}