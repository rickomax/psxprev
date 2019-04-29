using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using OpenTK;
using PSXPrev.Forms;
using Timer = System.Timers.Timer;

namespace PSXPrev
{
    public partial class PreviewForm : Form
    {
        private Timer _animateTimer;
        private List<Animation> _animations;
        private Animation _curAnimation;
        private int _curAnimationFrame;
        private Action<PreviewForm> _refreshAction;

        private bool _inAnimationTab;

        //private bool _debug;
        private float _lastMouseX;

        private float _lastMouseY;
        private GLControl _openTkControl;
        private bool _playing;
        private Timer _redrawTimer;
        private List<RootEntity> _rootEntities;

        private Scene _scene;

        //private int _selectedTriangle;
        private List<Texture> _textures;

        private Texture[] _vramPage;

        private bool _showUv = true;

        public PreviewForm(Action<PreviewForm> refreshAction, bool debug)
        {
            _refreshAction = refreshAction;

            _animations = new List<Animation>();
            _textures = new List<Texture>();
            _rootEntities = new List<RootEntity>();

            SetupCulture();
            refreshAction(this);

            SetupInternals();

            Toolkit.Init();

            InitializeComponent();
            SetupControls();
            //ResetSelectedTriangle();
        }

        private void EntityAdded(RootEntity entity)
        {
            // set textures for preview
            foreach (var entityBase in entity.ChildEntities)
            {
                var model = (ModelEntity)entityBase;
                model.Texture = _vramPage[model.TexturePage];
            }

            entitiesTreeView.BeginUpdate();
            var entityNode = entitiesTreeView.Nodes.Add(entity.EntityName);
            animationEntityComboBox.Items.Add(entity);
            for (var m = 0; m < entity.ChildEntities.Length; m++)
            {
                // var model = (ModelEntity) entity.ChildEntities[m];
                var modelNode = new TreeNode("Sub-Model " + m);
                entityNode.Nodes.Add(modelNode);
                modelNode.HideCheckBox();
                modelNode.HideCheckBox();
                //if (_debug)
                //    for (var t = 0; t < model.Triangles.Length; t++)
                //    {
                //        var triangleNode = new TreeNode("Triangle " + t);
                //        modelNode.Nodes.Add(triangleNode);
                //        triangleNode.HideCheckBox();
                //        triangleNode.HideCheckBox();
                //    }
            }
            entitiesTreeView.EndUpdate();
        }

        private void TextureAdded(Texture texture, int index)
        {
            thumbsImageList.Images.Add(texture.Bitmap);
            texturesListView.Items.Add(texture.TextureName, index);
        }

        private void AnimationAdded(Animation animation)
        {
            animationsTreeView.BeginUpdate();
            var animationNode = new TreeNode(animation.AnimationName) { Tag = animation };
            animationsTreeView.Nodes.Add(animationNode);
            AddAnimationObject(animation.RootAnimationObject, animationNode);
            animationsTreeView.EndUpdate();
        }

        public void UpdateRootEntities(List<RootEntity> entities)
        {
            for (var i = 0; i < entities.Count; ++i)
            {
                var entity = entities[i];
                if (!_rootEntities.Contains(entity))
                {
                    _rootEntities.Add(entity);
                    EntityAdded(entity);
                }
            }
        }

        public void UpdateTextures(List<Texture> textures)
        {
            for (var i = 0; i < textures.Count; ++i)
            {
                var texture = textures[i];
                if (!_textures.Contains(texture))
                {
                    _textures.Add(texture);
                    var textureIndex = _textures.IndexOf(texture);
                    TextureAdded(texture, textureIndex);
                }
            }
        }

        public void UpdateAnimations(List<Animation> animations)
        {
            for (var i = 0; i < animations.Count; ++i)
            {
                var animation = animations[i];
                if (!_animations.Contains(animation))
                {
                    _animations.Add(animation);
                    AnimationAdded(animation);
                }
            }
        }

        public System.Drawing.Color SceneBackColor
        {
            set
            {
                if (_scene == null)
                    throw new Exception("Window must be initialized");

                _scene.ClearColor = new Color
                {
                    R = value.R / 255f,
                    G = value.G / 255f,
                    B = value.B / 255f
                };
            }
        }

        private void SetupCulture()
        {
            var customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = customCulture;
        }

        private void SetupInternals()
        {
            _vramPage = new Texture[32];
            //_debug = debug;
            _scene = new Scene();
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
            _openTkControl.Paint += _openTkControl_Paint;
            entitiesTabPage.Controls.Add(_openTkControl);
        }

        private void _openTkControl_Paint(object sender, PaintEventArgs e)
        {
            if (_inAnimationTab && _curAnimation != null)
            {
                _scene.AnimationBatch.SetupAnimationFrame(_curAnimationFrame);
            }
            _scene.Draw();
            _openTkControl.SwapBuffers();
        }

        //private void ResetSelectedTriangle()
        //{
        //    _selectedTriangle = int.MaxValue;
        //}

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
            for (var index = 0; index < _textures.Count; index++)
            {
                var texture = _textures[index];
                TextureAdded(texture, index);
            }
        }

        private void SetupEntities()
        {
            for (var e = 0; e < _rootEntities.Count; e++)
            {
                var entity = _rootEntities[e];
                EntityAdded(entity);
            }
        }

        private void SetupAnimations()
        {
            for (var a = 0; a < _animations.Count; a++)
            {
                var animation = _animations[a];
                AnimationAdded(animation);
            }
        }

        private void AddAnimationObject(AnimationObject parent, TreeNode parentNode)
        {
            var animationObjects = parent.Children;
            for (var o = 0; o < animationObjects.Count; o++)
            {
                var animationObject = animationObjects[o];
                var animationObjectNode = new TreeNode("Animation-Object " + o) { Tag = animationObject };
                parentNode.Nodes.Add(animationObjectNode);
                animationObjectNode.HideCheckBox();
                animationObjectNode.HideCheckBox();
                AddAnimationObject(animationObject, animationObjectNode);
            }
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
        }

        private void previewForm_Load(object sender, EventArgs e)
        {
            _redrawTimer = new Timer();
            _redrawTimer.Interval = 1f / 60f;
            _redrawTimer.Elapsed += _redrawTimer_Elapsed;
            _redrawTimer.Start();

            _animateTimer = new Timer();
            _animateTimer.Interval = 1f / 60f;
            _animateTimer.Elapsed += _animateTimer_Elapsed;
        }

        private void _animateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_curAnimationFrame < _curAnimation.FrameCount)
                _curAnimationFrame++;
            else
                _curAnimationFrame = 0;
            //animationTrackBar.Value = _curAnimationFrame;
        }

        private void _redrawTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Redraw();
        }

        private RootEntity[] GetSelectectedEntities()
        {
            var selectedEntities = new List<RootEntity>();
            for (var i = 0; i < entitiesTreeView.Nodes.Count; i++)
            {
                var node = entitiesTreeView.Nodes[i];
                if (node.Checked)
                    selectedEntities.Add(_rootEntities[i]);
            }
            if (selectedEntities.Count == 0)
                return null;
            return selectedEntities.ToArray();
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
                var selectedTextures = new Texture[selectedCount];
                for (var i = 0; i < selectedCount; i++)
                    selectedTextures[i] = _textures[selectedIndices[i]];
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
            var node = e.Node;
            var nodeIndex = node.Index;
            var nodeLevel = node.Level;
            SelectModelOrEntity(nodeIndex, nodeLevel, true);
        }

        private object SelectModelOrEntity(int nodeIndex, int nodeLevel, bool focus)
        {
            //_scene.LineBatch.Reset();
            ModelEntity model;
            switch (nodeLevel)
            {
                case 0:
                    var entity = GetSelectedEntity(nodeIndex);
                    //ResetSelectedTriangle();
                    _scene.MeshBatch.SetupEntityBatch(entity, focus);
                    modelPropertyGrid.SelectedObject = entity;
                    return entity;
                case 1:
                    model = GetSelectedModel();
                    //ResetSelectedTriangle();
                    _scene.MeshBatch.SetupModelBatch(model, focus);
                    modelPropertyGrid.SelectedObject = model;
                    return model;
                case 2:
                    model = GetSelectedModel(true);
                    //_selectedTriangle = (ushort) nodeIndex;
                    _scene.MeshBatch.SetupModelBatch(model);
                    var triangle = model.Triangles[nodeIndex];
                    modelPropertyGrid.SelectedObject = triangle;
                    return model;
            }
            return null;
        }

        private Texture GetSelectedTexture(int? index = null)
        {
            if (texturesListView.SelectedIndices.Count == 0)
                return null;
            var textureIndex = index ?? texturesListView.SelectedIndices[0];
            if (textureIndex < 0)
                return null;
            return _textures[textureIndex];
        }

        private RootEntity GetSelectedEntity(int? entityIndex = null)
        {
            int index;
            if (entitiesTreeView.SelectedNode == null)
                return null;
            if (entityIndex == null)
                index = entitiesTreeView.SelectedNode.Index;
            else
                index = (int)entityIndex;
            if (index < 0)
                return null;
            var entity = _rootEntities[index];
            return entity;
        }

        private ModelEntity GetSelectedModel(bool fromTriangle = false)
        {
            var selectedNode = entitiesTreeView.SelectedNode;
            if (selectedNode == null)
            {
                return null;
            }
            var parentNode = selectedNode.Parent;
            int childIndex;
            int parentIndex;
            if (!fromTriangle)
            {
                childIndex = selectedNode.Index;
                parentIndex = parentNode.Index;
            }
            else
            {
                childIndex = parentNode.Index;
                parentIndex = parentNode.Parent.Index;
            }
            if (childIndex < 0)
                return null;
            var model = (ModelEntity)_rootEntities[parentIndex].ChildEntities[childIndex];
            model.TexturePage = Math.Min(31, Math.Max(0, model.TexturePage));
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
            var selectedNode = entitiesTreeView.SelectedNode;
            if (selectedNode == null)
                return;
            var nodeLevel = selectedNode.Level;
            var nodeIndex = selectedNode.Index;
            if (nodeLevel == 0)
            {
                var entity = (RootEntity)SelectModelOrEntity(nodeIndex, nodeLevel, false);
                selectedNode.Text = entity.EntityName;
            }
        }

        private void texturePropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            var selectedNodes = texturesListView.SelectedItems;
            if (selectedNodes.Count == 0)
                return;
            var selectedNode = selectedNodes[0];
            if (selectedNode == null)
                return;
            var texture = GetSelectedTexture();
            if (texture == null)
                return;
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
                if (e.ClickedItem == miDAE)
                {
                    var daeExporter = new DaeExporter();
                    daeExporter.Export(entities, path);
                }
                MessageBox.Show("Models exported");
            }
        }

        private void findByPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var pageIndexString = Utils.ShowDialog("Find by Page", "Type the page number");
            int pageIndex;
            if (string.IsNullOrEmpty(pageIndexString))
                return;
            if (!int.TryParse(pageIndexString, out pageIndex))
            {
                MessageBox.Show("Invalid page number");
                return;
            }
            var found = 0;
            for (var i = 0; i < texturesListView.Items.Count; i++)
            {
                var item = texturesListView.Items[i];
                item.Group = null;
                var texture = _textures[i];
                if (texture.TexturePage == pageIndex)
                {
                    item.Group = texturesListView.Groups[0];
                    found++;
                }
            }
            if (found > 0)
                MessageBox.Show(string.Format("Found {0} itens", found));
            else
                MessageBox.Show("Nothing found");
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
                return;
            var bitmap = texture.Bitmap;
            texturePreviewPictureBox.Image = bitmap;
            texturePreviewPictureBox.Refresh();
            texturePropertyGrid.SelectedObject = texture;
        }

        private void DrawUV(EntityBase entity, Graphics graphics)
        {
            if (entity == null)
            {
                return;
            }
            var modelEntity = entity as ModelEntity;
            if (modelEntity != null && modelEntity.HasUvs)
            {
                foreach (var triangle in modelEntity.Triangles)
                {
                    graphics.DrawLine(Pens.Green, triangle.Uv[0].X * 255f, triangle.Uv[0].Y * 255f, triangle.Uv[1].X * 255f, triangle.Uv[1].Y * 255f);
                    graphics.DrawLine(Pens.Green, triangle.Uv[1].X * 255f, triangle.Uv[1].Y * 255f, triangle.Uv[2].X * 255f, triangle.Uv[2].Y * 255f);
                    graphics.DrawLine(Pens.Green, triangle.Uv[2].X * 255f, triangle.Uv[2].Y * 255f, triangle.Uv[0].X * 255f, triangle.Uv[0].Y * 255f);
                }
            }
            if (entity.ChildEntities != null)
            {
                foreach (var subEntity in entity.ChildEntities)
                {
                    DrawUV(subEntity, graphics);
                }
            }
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

        private void wireframeToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _scene.WireFrame = wireframeToolStripMenuItem.Checked;
        }

        private void clearAllPagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < 32; i++)
                ClearPage(i);
            MessageBox.Show("Pages cleared");
        }

        private void openTKControl_Load(object sender, EventArgs e)
        {
            SetupScene();
            SetupColors();
            SetupVram();
            SetupEntities();
            SetupTextures();
            SetupAnimations();
        }

        private void Redraw()
        {
            _openTkControl.Invalidate();
        }

        private void menusTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (menusTabControl.SelectedTab.TabIndex)
            {
                case 0:
                    _inAnimationTab = false;
                    animationsTreeView.SelectedNode = null;
                    _openTkControl.Parent = menusTabControl.TabPages[0];
                    _openTkControl.Show();
                    break;
                case 3:
                    _inAnimationTab = true;
                    entitiesTreeView.SelectedNode = null;
                    _openTkControl.Parent = menusTabControl.TabPages[3];
                    _openTkControl.Show();
                    break;
                default:
                    _openTkControl.Parent = this;
                    _openTkControl.Hide();
                    break;
            }
        }

        private void animationsTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SelectAnimationOrObject();
        }

        private void SelectAnimationOrObject()
        {
            var selectedNode = animationsTreeView.SelectedNode;
            if (selectedNode == null)
            {
                _curAnimation = null;
                _curAnimationFrame = 0;
                animationPlayButton.Enabled = false;
                return;
            }
            Animation animation;
            var result = selectedNode.Tag;
            animationPropertyGrid.SelectedObject = result;
            if (animationsTreeView.SelectedNode.Level == 0)
            {
                animation = (Animation)result;
            }
            else
            {
                var animationObject = (AnimationObject)selectedNode.Tag;
                animation = animationObject.Animation;
                UnselectAllAnimationObjects(animation.RootAnimationObject);
                animationObject.IsSelected = true;
            }
            _curAnimation = animation;
            _curAnimationFrame = 0;
            animationPlayButton.Enabled = true;
            _scene.AnimationBatch.SetupAnimationBatch(_curAnimation);
        }

        private void UnselectAllAnimationObjects(AnimationObject rootAnimationObject)
        {
            foreach (var animationObject in rootAnimationObject.Children)
            {
                animationObject.IsSelected = false;
                UnselectAllAnimationObjects(animationObject);
            }
        }

        private void animationPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            SelectAnimationOrObject();
        }

        private void animationPlayButton_Click(object sender, EventArgs e)
        {
            if (_playing)
            {
                _animateTimer.Stop();
                _playing = false;
            }
            else
            {
                _animateTimer.Start();
                _playing = true;
            }
        }

        public void UpdateProgress(int value, int max, bool complete, string message)
        {
            if (InvokeRequired)
            {
                var invokeAction = new Action<int, int, bool, string>((a, b, c, d) =>
                    UpdateProgress(a, b, c, d)
                );

                Invoke(invokeAction, value, max, complete, message);
            }
            else
            {
                toolStripProgressBar1.Minimum = 0;
                toolStripProgressBar1.Maximum = max;
                toolStripProgressBar1.Value = value;
                toolStripProgressBar1.Enabled = !complete;
                toolStripStatusLabel1.Text = message;
            }
        }

        public void ReloadItems()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ReloadItems));
            }
            else
            {
                _refreshAction(this);

                Redraw();
            }
        }

        private void showUVCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _showUv = showUVCheckBox.Checked;
            vramPagePictureBox.Refresh();
        }

        private void vramPagePictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (_showUv)
            {
                DrawUV(GetSelectedEntity(), e.Graphics);
            }
        }
    }
}