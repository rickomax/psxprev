using System;
using System.Collections.Generic;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;
using OpenTK;
using Timer = System.Timers.Timer;

namespace PSXPrev
{
    public partial class PreviewForm : Form
    {
        private enum MouseEventType
        {
            Down,
            Up,
            Move,
            Wheel
        }

        private enum EntitySelectionSource
        {
            None,
            TreeView,
            Click
        }

        private const float MouseSensivity = 0.0035f;

        private static Pen Black3Px = new Pen(System.Drawing.Color.Black, 3f);
        private static Pen White1Px = new Pen(System.Drawing.Color.White, 1f);

        private Timer _animateTimer;
        private Timer _redrawTimer;
        private readonly List<Animation> _animations;
        private readonly Action<PreviewForm> _refreshAction;
        private GLControl _openTkControl;
        private Scene _scene;
        private readonly List<RootEntity> _rootEntities;
        private readonly List<Texture> _textures;
        private Texture[] _vramPage;
        private Scene.GizmoId _selectedGizmo;
        private EntitySelectionSource _selectionSource;
        private bool _playing;
        private bool _showUv = true;
        private bool _inAnimationTab;
        private float _lastMouseX;
        private float _lastMouseY;
        private int _curAnimationFrame;
        private Animation _curAnimation;
        private AnimationObject _curAnimationObject;
        private RootEntity _curEntity;
        private ModelEntity _curModel;

        private bool Playing
        {
            get => _playing;
            set
            {
                _playing = value;
                if (_playing)
                {
                    animationPlayButton.Text = "Stop Animation";
                    _animateTimer.Start();
                }
                else
                {
                    animationPlayButton.Text = "Play Animation";
                    _animateTimer.Stop();
                }
            }
        }

        public PreviewForm(Action<PreviewForm> refreshAction)
        {
            _refreshAction = refreshAction;
            _animations = new List<Animation>();
            _textures = new List<Texture>();
            _rootEntities = new List<RootEntity>();
            _vramPage = new Texture[32];
            _scene = new Scene();
            refreshAction(this);
            Toolkit.Init();
            InitializeComponent();
            SetupControls();
        }

        private void EntityAdded(RootEntity entity)
        {
            foreach (var entityBase in entity.ChildEntities)
            {
                var model = (ModelEntity)entityBase;
                model.TexturePage = Math.Min(31, Math.Max(0, model.TexturePage));
                model.Texture = _vramPage[model.TexturePage];
            }
            entitiesTreeView.BeginUpdate();
            var entityNode = entitiesTreeView.Nodes.Add(entity.EntityName);
            entityNode.Tag = entity;
            for (var m = 0; m < entity.ChildEntities.Length; m++)
            {
                var modelNode = new TreeNode("Sub-Model " + (m + 1));
                modelNode.Tag = entity.ChildEntities[m];
                entityNode.Nodes.Add(modelNode);
                modelNode.HideCheckBox();
                modelNode.HideCheckBox();
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
            var animationNode = new TreeNode(animation.AnimationName);
            animationNode.Tag = animation;
            animationsTreeView.Nodes.Add(animationNode);
            AddAnimationObject(animation.RootAnimationObject, animationNode);
            animationsTreeView.EndUpdate();
        }

        public void UpdateRootEntities(List<RootEntity> entities)
        {
            foreach (var entity in entities)
            {
                if (_rootEntities.Contains(entity))
                {
                    continue;
                }
                _rootEntities.Add(entity);
                EntityAdded(entity);
            }
        }

        public void UpdateTextures(List<Texture> textures)
        {
            foreach (var texture in textures)
            {
                if (_textures.Contains(texture))
                {
                    continue;
                }
                _textures.Add(texture);
                var textureIndex = _textures.IndexOf(texture);
                TextureAdded(texture, textureIndex);
            }
        }

        public void UpdateAnimations(List<Animation> animations)
        {
            foreach (var animation in animations)
            {
                if (_animations.Contains(animation))
                {
                    continue;
                }
                _animations.Add(animation);
                AnimationAdded(animation);
            }
        }

        private System.Drawing.Color SceneBackColor
        {
            set
            {
                if (_scene == null)
                {
                    throw new Exception("Window must be initialized");
                }
                _scene.ClearColor = new Color
                (
                    value.R / 255f,
                    value.G / 255f,
                    value.B / 255f
                );
            }
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
            _openTkControl.MouseDown += delegate (object sender, MouseEventArgs e) { openTkControl_MouseEvent(e, MouseEventType.Down); };
            _openTkControl.MouseUp += delegate (object sender, MouseEventArgs e) { openTkControl_MouseEvent(e, MouseEventType.Up); };
            _openTkControl.MouseWheel += delegate (object sender, MouseEventArgs e) { openTkControl_MouseEvent(e, MouseEventType.Wheel); };
            _openTkControl.MouseMove += delegate (object sender, MouseEventArgs e) { openTkControl_MouseEvent(e, MouseEventType.Move); };
            _openTkControl.Paint += _openTkControl_Paint;
            entitiesTabPage.Controls.Add(_openTkControl);
        }

        private void _openTkControl_Paint(object sender, PaintEventArgs e)
        {
            if (_inAnimationTab && _curAnimation != null)
            {
                _scene.AnimationBatch.SetupAnimationFrame(_curAnimationFrame, _curAnimationObject, _curEntity);
            }
            _scene.Draw();
            _openTkControl.SwapBuffers();
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
            for (var index = 0; index < _textures.Count; index++)
            {
                var texture = _textures[index];
                TextureAdded(texture, index);
            }
        }

        private void SetupEntities()
        {
            foreach (var entity in _rootEntities)
            {
                EntityAdded(entity);
            }
        }

        private void SetupAnimations()
        {
            foreach (var animation in _animations)
            {
                AnimationAdded(animation);
            }
        }

        private void AddAnimationObject(AnimationObject parent, TreeNode parentNode)
        {
            var animationObjects = parent.Children;
            for (var o = 0; o < animationObjects.Count; o++)
            {
                var animationObject = animationObjects[o];
                var animationObjectNode = new TreeNode("Animation-Object " + (o + 1));
                animationObjectNode.Tag = animationObject;
                parentNode.Nodes.Add(animationObjectNode);
                animationObjectNode.HideCheckBox();
                animationObjectNode.HideCheckBox();
                //foreach (var animationFrame in animationObject.AnimationFrames)
                //{
                //    var animationFrameNode = new TreeNode("Frame " + animationFrame.Value.FrameTime);
                //    animationFrameNode.Tag = animationFrame.Value;
                //    // animationFrameNode.HideCheckBox();
                //    //animationFrameNode.HideCheckBox();
                //    animationObjectNode.Nodes.Add(animationFrameNode);
                //}
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
            _animateTimer.Elapsed += _animateTimer_Elapsed;
        }

        //private AnimationFrame GetSelectedAnimationFrame()
        //{
        //    var selectedNode = animationsTreeView.SelectedNode;
        //    if (selectedNode != null)
        //    {
        //        var selectedAnimationFrame = selectedNode.Tag as AnimationFrame;
        //        if (selectedAnimationFrame != null)
        //        {
        //            return selectedAnimationFrame;
        //        }
        //    }
        //    return null;
        //}

        private void _animateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_curAnimationFrame < _curAnimation.FrameCount)
            {
                _curAnimationFrame++;
            }
            else
            {
                _curAnimationFrame = 0;
            }
        }

        private void _redrawTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Redraw();
        }

        private RootEntity[] GetCheckedEntities()
        {
            var selectedEntities = new List<RootEntity>();
            for (var i = 0; i < entitiesTreeView.Nodes.Count; i++)
            {
                var node = entitiesTreeView.Nodes[i];
                if (node.Checked)
                {
                    selectedEntities.Add(_rootEntities[i]);
                }
            }
            return selectedEntities.Count == 0 ? null : selectedEntities.ToArray();
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
            if (fbd.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            var selectedTextures = new Texture[selectedCount];
            for (var i = 0; i < selectedCount; i++)
            {
                selectedTextures[i] = _textures[selectedIndices[i]];
            }
            var exporter = new PngExporter();
            exporter.Export(selectedTextures, fbd.SelectedPath);
            MessageBox.Show("Textures exported");
        }

        private void SelectEntity(RootEntity rootEntity)
        {
            _selectionSource = EntitySelectionSource.Click;
            var rootIndex = _rootEntities.IndexOf(rootEntity);
            entitiesTreeView.SelectedNode = entitiesTreeView.Nodes[rootIndex];
        }

        private void openTkControl_MouseEvent(MouseEventArgs e, MouseEventType t)
        {
            if (t == MouseEventType.Wheel)
            {
                _scene.CameraDistance -= e.Delta * MouseSensivity * _scene.CameraDistanceIncrement;
                _scene.UpdateViewMatrix();
                UpdateGizmos(_selectedGizmo, false);
                return;
            }
            var selectedEntityBase = (EntityBase)_curEntity ?? _curModel;
            var selectedGizmo = _selectedGizmo;
            var deltaX = e.X - _lastMouseX;
            var deltaY = e.Y - _lastMouseY;
            var mouseLeft = e.Button == MouseButtons.Left;
            var mouseMiddle = e.Button == MouseButtons.Middle;
            var mouseRight = e.Button == MouseButtons.Right;
            var controlWidth = _openTkControl.Size.Width;
            var controlHeight = _openTkControl.Size.Height;
            switch (_selectedGizmo)
            {
                case Scene.GizmoId.None:
                    if (mouseLeft && t == MouseEventType.Down)
                    {
                        selectedGizmo = _scene.GetGizmoUnderPosition(e.Location.X, e.Location.Y, controlWidth, controlHeight, selectedEntityBase);
                        if (selectedGizmo == Scene.GizmoId.None)
                        {
                            var checkedEntities = GetCheckedEntities();
                            var newSelectedEntity = _scene.GetRootEntityUnderMouse(checkedEntities, _curEntity, e.Location.X, e.Location.Y, controlWidth, controlHeight);
                            if (newSelectedEntity != null && newSelectedEntity != selectedEntityBase)
                            {
                                SelectEntity(newSelectedEntity);
                            }
                        }
                    }
                    else
                    {
                        var hasToUpdateViewMatrix = false;
                        if (mouseRight && t == MouseEventType.Move)
                        {
                            _scene.CameraYaw -= deltaX * MouseSensivity;
                            _scene.CameraPitch += deltaY * MouseSensivity;
                            hasToUpdateViewMatrix = true;
                        }
                        if (mouseMiddle && t == MouseEventType.Move)
                        {
                            _scene.CameraX += deltaX * MouseSensivity * _scene.CameraPanIncrement;
                            _scene.CameraY += deltaY * MouseSensivity * _scene.CameraPanIncrement;
                            hasToUpdateViewMatrix = true;
                        }
                        if (hasToUpdateViewMatrix)
                        {
                            _scene.UpdateViewMatrix();
                            UpdateGizmos(_selectedGizmo, false);
                        }
                    }
                    break;
                case Scene.GizmoId.XMover:
                    if (mouseLeft && t == MouseEventType.Move && selectedEntityBase != null)
                    {
                        var offset = _scene.GetGizmoProjectionOffset(e.Location.X, e.Location.Y, controlWidth, controlHeight, selectedEntityBase, _scene.GetBestPlaneNormal(GeomUtils.ZVector, GeomUtils.YVector), GeomUtils.XVector);
                        selectedEntityBase.PositionX += offset.X;
                        UpdateSelectedEntity(false);
                    }
                    else
                    {
                        selectedGizmo = Scene.GizmoId.None;
                    }
                    break;
                case Scene.GizmoId.YMover:
                    if (mouseLeft && t == MouseEventType.Move && selectedEntityBase != null)
                    {
                        var offset = _scene.GetGizmoProjectionOffset(e.Location.X, e.Location.Y, controlWidth, controlHeight, selectedEntityBase, _scene.GetBestPlaneNormal(GeomUtils.ZVector, GeomUtils.XVector), GeomUtils.YVector);
                        selectedEntityBase.PositionY += offset.Y;
                        UpdateSelectedEntity(false);
                    }
                    else
                    {
                        selectedGizmo = Scene.GizmoId.None;
                    }
                    break;
                case Scene.GizmoId.ZMover:
                    if (mouseLeft && t == MouseEventType.Move && selectedEntityBase != null)
                    {
                        var offset = _scene.GetGizmoProjectionOffset(e.Location.X, e.Location.Y, controlWidth, controlHeight, selectedEntityBase, _scene.GetBestPlaneNormal(GeomUtils.YVector, GeomUtils.XVector), GeomUtils.ZVector);
                        selectedEntityBase.PositionZ += offset.Z;
                        UpdateSelectedEntity(false);
                    }
                    else
                    {
                        selectedGizmo = Scene.GizmoId.None;
                    }
                    break;
            }
            if (selectedGizmo != _selectedGizmo)
            {
                UpdateGizmos(selectedGizmo);
            }
            _lastMouseX = e.X;
            _lastMouseY = e.Y;
        }

        private void entitiesTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_selectionSource == EntitySelectionSource.None)
            {
                _selectionSource = EntitySelectionSource.TreeView;
            }
            var selectedNode = entitiesTreeView.SelectedNode;
            if (selectedNode != null)
            {
                _curEntity = selectedNode.Tag as RootEntity;
                _curModel = selectedNode.Tag as ModelEntity;
            }
            UpdateSelectedEntity();
        }

        private void UpdateGizmos(Scene.GizmoId selectedGizmo = Scene.GizmoId.None, bool updateMeshData = true)
        {
            if (updateMeshData)
            {
                _scene.GizmosMeshBatch.Reset(3);
            }
            var selectedEntityBase = (EntityBase)_curEntity ?? _curModel;
            if (selectedEntityBase == null)
            {
                return;
            }
            var matrix = selectedEntityBase.WorldMatrix;
            var scaleMatrix = _scene.GetGizmoScaleMatrix(matrix.ExtractTranslation());
            var finalMatrix = scaleMatrix * matrix;
            _scene.GizmosMeshBatch.BindCube(finalMatrix, selectedGizmo == Scene.GizmoId.XMover ? Color.White : Color.Red, Scene.XGizmoDimensions, Scene.XGizmoDimensions, 0, null, updateMeshData);
            _scene.GizmosMeshBatch.BindCube(finalMatrix, selectedGizmo == Scene.GizmoId.YMover ? Color.White : Color.Green, Scene.YGizmoDimensions, Scene.YGizmoDimensions, 1, null, updateMeshData);
            _scene.GizmosMeshBatch.BindCube(finalMatrix, selectedGizmo == Scene.GizmoId.ZMover ? Color.White : Color.Blue, Scene.ZGizmoDimensions, Scene.ZGizmoDimensions, 2, null, updateMeshData);
            _selectedGizmo = selectedGizmo;
        }

        private void UpdateSelectedEntity(bool updateMeshData = true)
        {
            _scene.BoundsBatch.Reset();
            _scene.SkeletonBatch.Reset();
            var selectedEntityBase = (EntityBase)_curEntity ?? _curModel;
            if (selectedEntityBase != null)
            {
                selectedEntityBase.ComputeBoundsRecursively();
                modelPropertyGrid.SelectedObject = selectedEntityBase;
                var checkedEntities = GetCheckedEntities();
                _scene.BoundsBatch.SetupEntityBounds(selectedEntityBase);
                _scene.MeshBatch.SetupMultipleEntityBatch(checkedEntities, _curModel, _curEntity, _scene.TextureBinder, updateMeshData, _selectionSource == EntitySelectionSource.TreeView && _curModel == null);
            }
            else
            {
                modelPropertyGrid.SelectedObject = null;
                _scene.MeshBatch.Reset(0);
                _selectedGizmo = Scene.GizmoId.None;
            }
            UpdateGizmos(_selectedGizmo, updateMeshData);
            _selectionSource = EntitySelectionSource.None;
        }

        private void UpdateSelectedAnimation()
        {
            var selectedObject = (object) _curAnimationObject ?? _curAnimation;
            if (selectedObject == null)
            {
                return;
            }
            if (_curAnimation != null)
            {
                _curAnimationFrame = 0;
                _animateTimer.Interval = 1f / _curAnimation.FPS;
                animationPlayButton.Enabled = true;
                Playing = false;
            }
            animationPropertyGrid.SelectedObject = selectedObject;
            _scene.AnimationBatch.SetupAnimationBatch(_curAnimation);
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
            return _textures[textureIndex];
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
            {
                return;
            }
            if (_curEntity != null)
            {
                selectedNode.Text = _curEntity.EntityName;
            }
            else if (_curModel != null)
            {
                _curModel.TexturePage = Math.Min(31, Math.Max(0, _curModel.TexturePage));
                _curModel.Texture = _vramPage[_curModel.TexturePage];
            }
            UpdateSelectedEntity(false);
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
            var entities = GetCheckedEntities();
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
                if (e.ClickedItem == miOBJMerged)
                {
                    var objExporter = new ObjExporter();
                    objExporter.Export(entities, path, false, true);
                }
                if (e.ClickedItem == miOBJVCMerged)
                {
                    var objExporter = new ObjExporter();
                    objExporter.Export(entities, path, true, true);
                }
                MessageBox.Show("Models exported");
            }
        }

        private void findByPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var pageIndexString = Utils.ShowDialog("Find by Page", "Type the page number");
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
            var found = 0;
            for (var i = 0; i < texturesListView.Items.Count; i++)
            {
                var item = texturesListView.Items[i];
                item.Group = null;
                var texture = _textures[i];
                if (texture.TexturePage != pageIndex)
                {
                    continue;
                }
                item.Group = texturesListView.Groups[0];
                found++;
            }
            MessageBox.Show(found > 0 ? $"Found {found} items" : "Nothing found");
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

        private void DrawUV(EntityBase entity, Graphics graphics)
        {
            if (entity == null)
            {
                return;
            }
            if (entity is ModelEntity modelEntity && modelEntity.HasUvs)
            {
                foreach (var triangle in modelEntity.Triangles)
                {
                    graphics.DrawLine(Black3Px, triangle.Uv[0].X * 255f, triangle.Uv[0].Y * 255f, triangle.Uv[1].X * 255f, triangle.Uv[1].Y * 255f);
                    graphics.DrawLine(Black3Px, triangle.Uv[1].X * 255f, triangle.Uv[1].Y * 255f, triangle.Uv[2].X * 255f, triangle.Uv[2].Y * 255f);
                    graphics.DrawLine(Black3Px, triangle.Uv[2].X * 255f, triangle.Uv[2].Y * 255f, triangle.Uv[0].X * 255f, triangle.Uv[0].Y * 255f);
                }
            }
            if (entity is ModelEntity modelEntity2 && modelEntity2.HasUvs)
            {
                foreach (var triangle in modelEntity2.Triangles)
                {
                    graphics.DrawLine(White1Px, triangle.Uv[0].X * 255f, triangle.Uv[0].Y * 255f, triangle.Uv[1].X * 255f, triangle.Uv[1].Y * 255f);
                    graphics.DrawLine(White1Px, triangle.Uv[1].X * 255f, triangle.Uv[1].Y * 255f, triangle.Uv[2].X * 255f, triangle.Uv[2].Y * 255f);
                    graphics.DrawLine(White1Px, triangle.Uv[2].X * 255f, triangle.Uv[2].Y * 255f, triangle.Uv[0].X * 255f, triangle.Uv[0].Y * 255f);
                }
            }
            if (entity.ChildEntities == null)
            {
                return;
            }
            foreach (var subEntity in entity.ChildEntities)
            {
                DrawUV(subEntity, graphics);
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
            _scene.Wireframe = wireframeToolStripMenuItem.Checked;
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
                    _openTkControl.Parent = menusTabControl.TabPages[3];
                    _openTkControl.Show();
                    UpdateSelectedAnimation();
                    break;
                default:
                    _openTkControl.Parent = this;
                    _openTkControl.Hide();
                    break;
            }
        }

        private void animationsTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var selectedNode = animationsTreeView.SelectedNode;
            if (selectedNode == null)
            {
                return;
            }
            if (selectedNode.Tag is Animation animation)
            {
                _curAnimation = animation;
                _curAnimationObject = null;
            }
            if (selectedNode.Tag is AnimationObject animationObject)
            {
                _curAnimation = animationObject.Animation;
                _curAnimationObject = animationObject;
            }
            UpdateSelectedAnimation();
        }

        private void animationPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            UpdateSelectedAnimation();
        }

        private void animationPlayButton_Click(object sender, EventArgs e)
        {
            Playing = !Playing;
        }

        public void UpdateProgress(int value, int max, bool complete, string message)
        {
            if (InvokeRequired)
            {
                var invokeAction = new Action<int, int, bool, string>(UpdateProgress);
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

        private void vramPagePictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (!_showUv)
            {
                return;
            }
            var checkedEntities = GetCheckedEntities();
            if (checkedEntities != null)
            {
                foreach (var checkedEntity in checkedEntities)
                {
                    if (checkedEntity == _curEntity)
                    {
                        continue;
                    }
                    DrawUV(checkedEntity, e.Graphics);
                }
            }
            DrawUV(_curEntity, e.Graphics);
        }

        private void entitiesTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            UpdateSelectedEntity();
        }

        private void showGizmosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _scene.ShowGizmos = showGizmosToolStripMenuItem.Checked;
        }

        private void showBoundsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _scene.ShowBounds = showBoundsToolStripMenuItem.Checked;
        }

        private void showUVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _showUv = showUVToolStripMenuItem.Checked;
            vramPagePictureBox.Refresh();
        }

        private void showSkeletonToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _scene.ShowSkeleton = showSkeletonToolStripMenuItem.Checked;
        }
    }
}