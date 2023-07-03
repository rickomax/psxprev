using System;
using System.Collections.Generic;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;
using OpenTK;
using PSXPrev.Classes;
using Color = PSXPrev.Classes.Color;
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
        private Scene.GizmoId _hoveredGizmo;
        private EntitySelectionSource _selectionSource;
        private bool _playing;
        private bool _showUv = true;
        private bool _inAnimationTab;
        private float _lastMouseX;
        private float _lastMouseY;
        private float _curAnimationTime;
        private float _curAnimationFrame;
        private Animation _curAnimation;
        private AnimationObject _curAnimationObject;
        private AnimationFrame _curAnimationFrameObj;
        private RootEntity _selectedRootEntity;
        private ModelEntity _selectedModelEntity;
        private Vector3 _pickedPosition;

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
                var entityChildEntity = entity.ChildEntities[m];
                var modelNode = new TreeNode(entityChildEntity.EntityName);
                modelNode.Tag = entityChildEntity;
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

        public void SetAutoAttachLimbs(bool attachLimbs)
        {
            if (InvokeRequired)
            {
                var invokeAction = new Action<bool>(SetAutoAttachLimbs);
                Invoke(invokeAction, attachLimbs);
            }
            else
            {
                autoAttachLimbsToolStripMenuItem.Checked = attachLimbs;
                _scene.AutoAttach = attachLimbs;
                UpdateSelectedEntity(true);
            }
        }

        public void SelectFirstEntity()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(SelectFirstEntity));
            }
            else
            {
                if (entitiesTreeView.Nodes.Count > 0)
                {
                    // I don't think the user will necessarily want the entity checked too.
                    //entitiesTreeView.Nodes[0].Checked = true;
                    entitiesTreeView.SelectedNode = entitiesTreeView.Nodes[0];
                }
            }
        }

        public void DrawAllTexturesToVRAM()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(DrawAllTexturesToVRAM));
            }
            else
            {
                foreach (var texture in _textures)
                {
                    DrawTextureToVRAM(texture);
                }
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
            UpdateLightDirection();
        }

        private void _openTkControl_Paint(object sender, PaintEventArgs e)
        {
            if (_inAnimationTab && _curAnimation != null)
            {
                var checkedEntities = GetCheckedEntities();
                if (!_scene.AnimationBatch.SetupAnimationFrame(_curAnimationFrame, checkedEntities, _selectedRootEntity, _selectedModelEntity, true))
                {
                    _curAnimationFrame = 0f;
                    _curAnimationTime = 0f;
                }
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
            SetMaskColor(System.Drawing.Color.Black);
            SetAmbientColor(System.Drawing.Color.LightGray);
            SetBackgroundColor(System.Drawing.Color.LightSkyBlue);
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
                foreach (var animationFrame in animationObject.AnimationFrames)
                {
                    var animationFrameNode = new TreeNode("Frame " + animationFrame.Value.FrameTime);
                    animationFrameNode.Tag = animationFrame.Value;
                    animationObjectNode.Nodes.Add(animationFrameNode);
                }
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
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            Text = $@"{Text} {fileVersionInfo.FileVersion}";
        }

        private void _animateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _curAnimationTime += (float)_animateTimer.Interval;
            _curAnimationFrame = _curAnimationTime / (1f / _curAnimation.FPS);
            if (_curAnimationFrame > _curAnimation.FrameCount - 1 + 0.9999f)
            {
                _curAnimationTime = 0f;
                _curAnimationFrame = 0f;
            }
            UpdateFrameLabel();
        }

        private void UpdateFrameLabel()
        {
            animationFrameLabel.Text = string.Format("Animation Frame:{0:0.##}", _curAnimationFrame);
            animationFrameLabel.Refresh();
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

        private void SelectEntity(EntityBase entity)
        {
            _selectionSource = EntitySelectionSource.Click;
            if (entity is RootEntity rootEntity)
            {
                var rootIndex = _rootEntities.IndexOf(rootEntity);
                entitiesTreeView.SelectedNode = entitiesTreeView.Nodes[rootIndex];
            }
            else
            {
                if (entity.ParentEntity is RootEntity rootEntityFromSub)
                {
                    var rootIndex = _rootEntities.IndexOf(rootEntityFromSub);
                    var rootNode = entitiesTreeView.Nodes[rootIndex];
                    var subIndex = Array.IndexOf(rootEntityFromSub.ChildEntities, entity);
                    entitiesTreeView.SelectedNode = rootNode.Nodes[subIndex];
                }
            }
        }

        private void openTkControl_MouseEvent(MouseEventArgs e, MouseEventType eventType)
        {
            if (_inAnimationTab)
            {
                _selectedGizmo = Scene.GizmoId.None;
            }
            if (eventType == MouseEventType.Wheel)
            {
                _scene.CameraDistance -= e.Delta * MouseSensivity * _scene.CameraDistanceIncrement;
                _scene.UpdateViewMatrix();
                UpdateGizmos(_selectedGizmo, _hoveredGizmo, false);
                return;
            }
            var deltaX = e.X - _lastMouseX;
            var deltaY = e.Y - _lastMouseY;
            var mouseLeft = e.Button == MouseButtons.Left;
            var mouseMiddle = e.Button == MouseButtons.Middle;
            var mouseRight = e.Button == MouseButtons.Right;
            var selectedEntityBase = (EntityBase)_selectedRootEntity ?? _selectedModelEntity;
            var controlWidth = _openTkControl.Size.Width;
            var controlHeight = _openTkControl.Size.Height;
            _scene.UpdatePicking(e.Location.X, e.Location.Y, controlWidth, controlHeight);
            var hoveredGizmo = _scene.GetGizmoUnderPosition(selectedEntityBase);
            var selectedGizmo = _selectedGizmo;
            switch (_selectedGizmo)
            {
                case Scene.GizmoId.None:
                    if (!_inAnimationTab && mouseLeft && eventType == MouseEventType.Down)
                    {
                        _pickedPosition = _scene.GetPickedPosition(-_scene.CameraDirection);
                        if (hoveredGizmo == Scene.GizmoId.None)
                        {
                            var checkedEntities = GetCheckedEntities();
                            RootEntity rootEntity = null;
                            if (_selectedRootEntity != null)
                            {
                                rootEntity = _selectedRootEntity;
                            }
                            else if (_selectedModelEntity != null)
                            {
                                rootEntity = _selectedModelEntity.GetRootEntity();
                            }
                            var newSelectedEntity = _scene.GetEntityUnderMouse(checkedEntities, rootEntity, e.Location.X, e.Location.Y, controlWidth, controlHeight);
                            if (newSelectedEntity != null)
                            {
                                SelectEntity(newSelectedEntity);
                            }
                        }
                        else
                        {
                            selectedGizmo = hoveredGizmo;
                            _scene.ResetIntersection();
                        }
                    }
                    else
                    {
                        var hasToUpdateViewMatrix = false;
                        if (mouseRight && eventType == MouseEventType.Move)
                        {
                            _scene.CameraYaw -= deltaX * MouseSensivity;
                            _scene.CameraPitch += deltaY * MouseSensivity;
                            hasToUpdateViewMatrix = true;
                        }
                        if (mouseMiddle && eventType == MouseEventType.Move)
                        {
                            _scene.CameraX += deltaX * MouseSensivity * _scene.CameraPanIncrement;
                            _scene.CameraY += deltaY * MouseSensivity * _scene.CameraPanIncrement;
                            hasToUpdateViewMatrix = true;
                        }
                        if (hasToUpdateViewMatrix)
                        {
                            _scene.UpdateViewMatrix();
                            UpdateGizmos(_selectedGizmo, _hoveredGizmo, false);
                        }
                    }
                    break;
                case Scene.GizmoId.XMover when !_inAnimationTab:
                    if (mouseLeft && eventType == MouseEventType.Move && selectedEntityBase != null)
                    {
                        var pickedPosition = _scene.GetPickedPosition(-_scene.CameraDirection);
                        var projectedOffset = (pickedPosition - _pickedPosition).ProjectOnNormal(GeomUtils.XVector);
                        selectedEntityBase.PositionX += projectedOffset.X;
                        selectedEntityBase.PositionY += projectedOffset.Y;
                        selectedEntityBase.PositionZ += projectedOffset.Z;
                        _pickedPosition = pickedPosition;
                        UpdateSelectedEntity(false);
                    }
                    else
                    {
                        AlignSelectedEntityToGrid(selectedEntityBase);
                        selectedGizmo = Scene.GizmoId.None;
                    }
                    break;
                case Scene.GizmoId.YMover when !_inAnimationTab:
                    if (mouseLeft && eventType == MouseEventType.Move && selectedEntityBase != null)
                    {
                        var pickedPosition = _scene.GetPickedPosition(-_scene.CameraDirection);
                        var projectedOffset = (pickedPosition - _pickedPosition).ProjectOnNormal(GeomUtils.YVector);
                        selectedEntityBase.PositionX += projectedOffset.X;
                        selectedEntityBase.PositionY += projectedOffset.Y;
                        selectedEntityBase.PositionZ += projectedOffset.Z;
                        _pickedPosition = pickedPosition;
                        UpdateSelectedEntity(false);
                    }
                    else
                    {
                        AlignSelectedEntityToGrid(selectedEntityBase);
                        selectedGizmo = Scene.GizmoId.None;
                    }
                    break;
                case Scene.GizmoId.ZMover when !_inAnimationTab:
                    if (mouseLeft && eventType == MouseEventType.Move && selectedEntityBase != null)
                    {
                        var pickedPosition = _scene.GetPickedPosition(-_scene.CameraDirection);
                        var projectedOffset = (pickedPosition - _pickedPosition).ProjectOnNormal(GeomUtils.ZVector);
                        selectedEntityBase.PositionX += projectedOffset.X;
                        selectedEntityBase.PositionY += projectedOffset.Y;
                        selectedEntityBase.PositionZ += projectedOffset.Z;
                        _pickedPosition = pickedPosition;
                        UpdateSelectedEntity(false);
                    }
                    else
                    {
                        AlignSelectedEntityToGrid(selectedEntityBase);
                        selectedGizmo = Scene.GizmoId.None;
                    }
                    break;
            }
            if (selectedGizmo != _selectedGizmo || hoveredGizmo != _hoveredGizmo)
            {
                UpdateGizmos(selectedGizmo, hoveredGizmo);
            }
            _lastMouseX = e.X;
            _lastMouseY = e.Y;
        }

        private void AlignSelectedEntityToGrid(EntityBase selectedEntityBase)
        {
            if (selectedEntityBase != null)
            {
                selectedEntityBase.PositionX = AlignToGrid(selectedEntityBase.PositionX);
                selectedEntityBase.PositionY = AlignToGrid(selectedEntityBase.PositionY);
                selectedEntityBase.PositionZ = AlignToGrid(selectedEntityBase.PositionZ);
                UpdateSelectedEntity(false);
            }
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
                _selectedRootEntity = selectedNode.Tag as RootEntity;
                _selectedModelEntity = selectedNode.Tag as ModelEntity;
            }
            UpdateSelectedEntity();
        }

        private void UpdateGizmos(Scene.GizmoId selectedGizmo = Scene.GizmoId.None, Scene.GizmoId hoveredGizmo = Scene.GizmoId.None, bool updateMeshData = true)
        {
            if (updateMeshData)
            {
                _scene.GizmosMeshBatch.Reset(3);
            }
            var selectedEntityBase = (EntityBase)_selectedRootEntity ?? _selectedModelEntity;
            if (selectedEntityBase == null)
            {
                return;
            }

            var matrix = Matrix4.CreateTranslation(selectedEntityBase.Bounds3D.Center);
            var scaleMatrix = _scene.GetGizmoScaleMatrix(matrix.ExtractTranslation());
            var finalMatrix = scaleMatrix * matrix;
            _scene.GizmosMeshBatch.BindCube(finalMatrix, hoveredGizmo == Scene.GizmoId.XMover || selectedGizmo == Scene.GizmoId.XMover ? Color.White : Color.Red, Scene.XGizmoDimensions, Scene.XGizmoDimensions, 0, null, updateMeshData);
            _scene.GizmosMeshBatch.BindCube(finalMatrix, hoveredGizmo == Scene.GizmoId.YMover || selectedGizmo == Scene.GizmoId.YMover ? Color.White : Color.Green, Scene.YGizmoDimensions, Scene.YGizmoDimensions, 1, null, updateMeshData);
            _scene.GizmosMeshBatch.BindCube(finalMatrix, hoveredGizmo == Scene.GizmoId.ZMover || selectedGizmo == Scene.GizmoId.ZMover ? Color.White : Color.Blue, Scene.ZGizmoDimensions, Scene.ZGizmoDimensions, 2, null, updateMeshData);
            _selectedGizmo = selectedGizmo;
            _hoveredGizmo = hoveredGizmo;
        }

        private void UpdateSelectedEntity(bool updateMeshData = true)
        {
            _scene.BoundsBatch.Reset();
            _scene.SkeletonBatch.Reset();
            var selectedEntityBase = (EntityBase)_selectedRootEntity ?? _selectedModelEntity;
            var rootEntity = selectedEntityBase?.GetRootEntity();
            if (rootEntity != null)
            {
                rootEntity.ResetAnimationData();
                rootEntity.FixConnections();
            }
            if (selectedEntityBase != null)
            {
                selectedEntityBase.ComputeBoundsRecursively();
                modelPropertyGrid.SelectedObject = selectedEntityBase;
                var checkedEntities = GetCheckedEntities();
                _scene.BoundsBatch.SetupEntityBounds(selectedEntityBase);
                _scene.MeshBatch.SetupMultipleEntityBatch(checkedEntities, _selectedModelEntity, _selectedRootEntity, _scene.TextureBinder, updateMeshData || _scene.AutoAttach, _selectionSource == EntitySelectionSource.TreeView && _selectedModelEntity == null);
            }
            else
            {
                modelPropertyGrid.SelectedObject = null;
                _scene.MeshBatch.Reset(0);
                _selectedGizmo = Scene.GizmoId.None;
                _hoveredGizmo = Scene.GizmoId.None;
            }
            UpdateGizmos(_selectedGizmo, _hoveredGizmo, updateMeshData);
            _selectionSource = EntitySelectionSource.None;
        }

        private void UpdateSelectedAnimation()
        {
            var selectedObject = _curAnimationFrameObj ?? _curAnimationObject ?? (object)_curAnimation;
            if (selectedObject == null)
            {
                return;
            }
            if (_curAnimation != null)
            {
                _curAnimationTime = 0f;
                _curAnimationFrame = 0f;
                UpdateAnimationFPS();
                animationPlayButton.Enabled = true;
                Playing = false;
                UpdateFrameLabel();
            }
            animationPropertyGrid.SelectedObject = selectedObject;
            _scene.AnimationBatch.SetupAnimationBatch(_curAnimation);
        }

        private void UpdateAnimationFPS()
        {
            _animateTimer.Interval = 1f / 60f * (animationSpeedTrackbar.Value / 100f);
        }

        private Texture GetSelectedTexture(int? index = null)
        {
            if (!index.HasValue && texturesListView.SelectedIndices.Count == 0)
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

        private void DrawTextureToVRAM(Texture texture)
        {
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
                DrawTextureToVRAM(GetSelectedTexture(index));
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
            if (_selectedModelEntity != null)
            {
                _selectedModelEntity.TexturePage = Math.Min(31, Math.Max(0, _selectedModelEntity.TexturePage));
                _selectedModelEntity.Texture = _vramPage[_selectedModelEntity.TexturePage];
            }
            var selectedEntityBase = (EntityBase)_selectedRootEntity ?? _selectedModelEntity;
            if (selectedEntityBase != null)
            {
                selectedNode.Text = selectedEntityBase.EntityName;
                selectedEntityBase.PositionX = AlignToGrid(selectedEntityBase.PositionX);
                selectedEntityBase.PositionY = AlignToGrid(selectedEntityBase.PositionY);
                selectedEntityBase.PositionZ = AlignToGrid(selectedEntityBase.PositionZ);
            }
            UpdateSelectedEntity(false);
        }

        private float AlignToGrid(float value)
        {
            return (float)((int)(value / (float)gridSizeNumericUpDown.Value) * gridSizeNumericUpDown.Value);
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
            var checkedEntities = GetCheckedEntities();
            if (checkedEntities == null)
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
                    objExporter.Export(checkedEntities, path);
                }
                if (e.ClickedItem == miOBJVC)
                {
                    var objExporter = new ObjExporter();
                    objExporter.Export(checkedEntities, path, true);
                }
                if (e.ClickedItem == miOBJMerged)
                {
                    var objExporter = new ObjExporter();
                    objExporter.Export(checkedEntities, path, false, true);
                }
                if (e.ClickedItem == miOBJVCMerged)
                {
                    var objExporter = new ObjExporter();
                    objExporter.Export(checkedEntities, path, true, true);
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
            if (entity is ModelEntity modelEntity)// && modelEntity.HasUvs)
            {
                foreach (var triangle in modelEntity.Triangles)
                {
                    graphics.DrawLine(Black3Px, triangle.Uv[0].X * 255f, triangle.Uv[0].Y * 255f, triangle.Uv[1].X * 255f, triangle.Uv[1].Y * 255f);
                    graphics.DrawLine(Black3Px, triangle.Uv[1].X * 255f, triangle.Uv[1].Y * 255f, triangle.Uv[2].X * 255f, triangle.Uv[2].Y * 255f);
                    graphics.DrawLine(Black3Px, triangle.Uv[2].X * 255f, triangle.Uv[2].Y * 255f, triangle.Uv[0].X * 255f, triangle.Uv[0].Y * 255f);
                }
            }
            if (entity is ModelEntity modelEntity2)//&& modelEntity2.HasUvs)
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

        private void SetMaskColor(System.Drawing.Color color)
        {
            _scene.MaskColor = color;
            var bitmap = new Bitmap(16, 16);
            var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(color);
            setMaskColorToolStripMenuItem.Image = bitmap;
        }

        private void SetAmbientColor(System.Drawing.Color color)
        {
            _scene.AmbientColor = color;
            var bitmap = new Bitmap(16, 16);
            var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(color);
            setAmbientColorToolStripMenuItem.Image = bitmap;
        }

        private void SetBackgroundColor(System.Drawing.Color color)
        {
            _scene.ClearColor = new Color(color.R / 255f, color.G / 255f, color.B / 255f);
            var bitmap = new Bitmap(16, 16);
            var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(color);
            setBackgroundColorToolStripMenuItem.Image = bitmap;
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
                _curAnimationFrameObj = null;
            }
            if (selectedNode.Tag is AnimationObject animationObject)
            {
                _curAnimation = animationObject.Animation;
                _curAnimationObject = animationObject;
                _curAnimationFrameObj = null;
            }
            if (selectedNode.Tag is AnimationFrame)
            {
                _curAnimationFrameObj = (AnimationFrame)selectedNode.Tag;
                _curAnimationObject = _curAnimationFrameObj.AnimationObject;
                _curAnimation = _curAnimationFrameObj.AnimationObject.Animation;
                UpdateFrameLabel();
            }
            UpdateSelectedAnimation();
            if (_curAnimationFrameObj != null)
            {
                _curAnimationFrame = _curAnimationFrameObj.FrameTime + 0.9999f;
            }
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
                    if (checkedEntity == _selectedRootEntity)
                    {
                        continue;
                    }
                    DrawUV(checkedEntity, e.Graphics);
                }
            }
            DrawUV(_selectedRootEntity, e.Graphics);
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

        private void animationSpeedTrackbar_Scroll(object sender, EventArgs e)
        {
            UpdateAnimationFPS();
        }

        private void lightRoll_Scroll(object sender, EventArgs e)
        {
            UpdateLightDirection();
        }

        private void lightYaw_Scroll(object sender, EventArgs e)
        {
            UpdateLightDirection();
        }

        private void lightPitch_Scroll(object sender, EventArgs e)
        {
            UpdateLightDirection();
        }

        private void UpdateLightDirection()
        {
            _scene.LightRotation = new Vector3(MathHelper.DegreesToRadians((float)lightPitchNumericUpDown.Value), MathHelper.DegreesToRadians((float)lightYawNumericUpDown.Value), MathHelper.DegreesToRadians((float)lightRollNumericUpDown.Value));
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
            Program.Initialize(null);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("PSXPrev - Playstation (PSX) Files Previewer/Extractor\n" +
                            "(c) PSX Prev Contributors - 2020-2023"
                , "About"
            );
        }

        private void videoTutorialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.youtube.com/watch?v=hPDa8l3ZE6U");
        }

        private void autoAttachLimbsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _scene.AutoAttach = autoAttachLimbsToolStripMenuItem.Checked;
            UpdateSelectedEntity(true);
        }

        private void compatibilityListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://docs.google.com/spreadsheets/d/155pUzwl7CC14ssT0PJkaEA53CS1ijpOV04VitQCVBC4/edit?pli=1#gid=22642205");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void lightPitchNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateLightDirection();
        }

        private void lightYawNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateLightDirection();
        }

        private void lightRollNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateLightDirection();
        }

        private void setMaskColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    SetMaskColor(colorDialog.Color);
                }
            }
        }

        private void enableLightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _scene.LightEnabled = enableLightToolStripMenuItem.Checked;
        }


        private void setAmbientColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    SetAmbientColor(colorDialog.Color);
                }
            }
        }

        private void setBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    SetBackgroundColor(colorDialog.Color);
                }
            }
        }

        private void lightIntensityNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _scene.LightIntensity = (float)lightIntensityNumericUpDown.Value / 100f;
        }

        private void vibRibbonWireframeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void wireframeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void lineRendererToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _scene.VibRibbonWireframe = lineRendererToolStripMenuItem.Checked;
        }

        private void resetWholeModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedEntityBase = (EntityBase)_selectedRootEntity ?? _selectedModelEntity;
            if (selectedEntityBase != null)
            {
                // This could be changed to only reset the selected model and its children.
                // But that's only necessary if sub-sub-model support is ever added.
                selectedEntityBase.GetRootEntity()?.ResetTransform(true);
                UpdateSelectedEntity(true);
            }
        }

        private void resetSelectedModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedEntityBase = (EntityBase)_selectedRootEntity ?? _selectedModelEntity;
            if (selectedEntityBase != null)
            {
                selectedEntityBase.ResetTransform(false);
                UpdateSelectedEntity(true);
            }
        }
    }
}
