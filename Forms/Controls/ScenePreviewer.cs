using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using PSXPrev.Common;
using PSXPrev.Common.Renderer;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using GdiPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace PSXPrev.Forms.Controls
{
    public partial class ScenePreviewer : UserControl
    {
        [Browsable(false)]
        public int ViewportWidth => _glControl?.ClientSize.Width ?? 0;

        [Browsable(false)]
        public int ViewportHeight => _glControl?.ClientSize.Height ?? 0;

        // True if this ScenePreviewer is currently the parent of the GLControl
        [Browsable(false)]
        public bool IsCurrent => _glControl?.Parent == this;

        private GLControl _glControl;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(null)]
        public GLControl GLControl
        {
            get => _glControl;
            set
            {
                if (_glControl != value)
                {
                    if (_glControl != null)
                    {
                        // Unhook GLControl events here
                        _glControl.Resize -= glControl_Resize;
                        if (IsCurrent)
                        {
                            // If we currently have ownership of the old GLControl, then unparent it.
                            _glControl.Parent = null;
                        }
                    }

                    _glControl = value;

                    if (_glControl != null)
                    {
                        // Hook GLControl events here
                        _glControl.Resize += glControl_Resize;
                        if (_glControl.Parent == null)
                        {
                            // If no ScenePreviewer has taken ownership of GLControl, then take it for ourselves.
                            MakeCurrent();
                        }
                    }
                }
            }
        }

        private Scene _scene;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(null)]
        public Scene Scene
        {
            get => _scene;
            set
            {
                if (_scene != value)
                {
                    if (_scene != null)
                    {
                        // Unhook Scene events here
                        _scene.CameraChanged -= scene_CameraChanged;
                    }

                    _scene = value;

                    if (_scene != null)
                    {
                        // Hook Scene events here
                        _scene.CameraChanged += scene_CameraChanged;
                    }
                    if (IsCurrent)
                    {
                        UpdateStatusBarLabels();
                    }
                }
            }
        }

        // Note that we can't store the property in statusStrip.Visible, since the getter returns actual visibility.
        private bool _showStatusBar = true;
        [Category("Appearance")]
        [Description("Shows or hides the status bar at the bottom of the control.")]
        [DefaultValue(true)]
        public bool ShowStatusBar
        {
            get => _showStatusBar;
            set
            {
                if (_showStatusBar != value)
                {
                    _showStatusBar = value;
                    if (_showStatusBar)
                    {
                        UpdateStatusBarLabels();
                    }
                    statusStrip.Visible = value;
                }
            }
        }


        public ScenePreviewer()
        {
            // PictureBox control is placed as dock fill behind the GLControl to prevent
            // flicker when changing GLControl's parent. This supposedly works because of
            // PictureBox's DoubleBuffered properties.
            // Note that it's not necessary to assign Image, and the Paint event is never even triggered :D
            InitializeComponent();

            // Setup labels
            UpdateStatusBarLabels();

            // Give focus to the main control when we click anywhere in the user control (except the scrollbars)
            for (var i = 0; i < statusStrip.Items.Count; i++)
            {
                var item = statusStrip.Items[i];
                if (item is ToolStripStatusLabel || item is ToolStripProgressBar)
                {
                    item.MouseDown += statusStrip_MouseDown;
                }
            }
        }


        // Invalidate the GLControl so that it can repaint
        public void InvalidateScene()
        {
            if (IsCurrent)
            {
                _glControl.Invalidate();
            }
        }

        // Creates a bitmap containing the contents of the scene preview
        public Bitmap CreateBitmap()
        {
            if (!IsCurrent) //if (_glControl != null)
            {
                return null;
            }
            var width  = ViewportWidth;
            var height = ViewportHeight;
            if (width <= 0 || height <= 0)
            {
                return null;
            }

            var rect = new Rectangle(0, 0, width, height);
            var bitmap = new Bitmap(width, height, GdiPixelFormat.Format32bppPArgb);
            try
            {
                var bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                try
                {
                    // These values are modified by TextureBinder, so change them back to default.
                    // (Maybe TextureBinder should restore these when done...)
                    GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
                    GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
                    GL.ReadPixels(0, 0, bmpData.Width, bmpData.Height, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
                }
                finally
                {
                    bitmap.UnlockBits(bmpData);
                }
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                return bitmap;
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }
        }

        // Take ownership of GLControl and assign its parent to this control
        public void MakeCurrent()
        {
            if (!IsCurrent && _glControl != null)
            {
                SuspendLayout();
                _glControl.Parent = this;
                _glControl.BackColor = BackColor;
                _glControl.BringToFront(); // Make sure we don't fill dock space used by status bar
                UpdateStatusBarLabels();
                ResumeLayout();
            }
        }


        private void glControl_Resize(object sender, EventArgs e)
        {
            if (IsCurrent)
            {
                _scene.Resize(ViewportWidth, ViewportHeight);
            }
        }

        private void scene_CameraChanged(object sender, EventArgs e)
        {
            if (IsCurrent)
            {
                UpdateStatusBarLabels();
            }
        }

        private void statusStrip_MouseDown(object sender, MouseEventArgs e)
        {
            // Clicking anywhere in the user control besides the scroll bars should give focus.
            // Note: Change this if we're attaching any buttons or interactable controls to the status bar.
            if (IsCurrent)
            {
                _glControl.Focus();
            }
        }


        // Divert focus to the GLControl and treat it as if this control is focused
        public override bool Focused => base.ContainsFocus;

        protected override void OnEnter(EventArgs e)
        {
            if (IsCurrent && base.Focused)
            {
                _glControl.Focus();
            }
            base.OnEnter(e);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            if (IsCurrent)
            {
                _glControl.BackColor = BackColor;
            }
            base.OnBackColorChanged(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            // Note that Visible getter is not the same as its setter.
            // This returns whether the control is actually visible, not if we have it set to visible.
            if (Visible)
            {
                // Take ownership of GLControl when this control is made visible
                MakeCurrent();
            }
            base.OnVisibleChanged(e);
        }


        private void UpdateStatusBarLabels()
        {
            if (!_showStatusBar)
            {
                return;
            }

            Vector3? target = null;
            float? yaw = null, pitch = null, distance = null;

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                //return;
            }
            else if (_scene != null)
            {
                target = _scene.CameraPositionTarget;
                // Floor angles so that we don't show 360 (rounded up, when wrapping should show 0)
                // Use negated angles to match light rotation angles.
                yaw = (float)Math.Floor(GeomMath.PositiveModulus(-_scene.CameraYaw * GeomMath.Rad2Deg, 360f));
                // Flip pitch 180 so that it shows as -90 to 90.
                pitch = (float)Math.Floor(GeomMath.PositiveModulus(-_scene.CameraPitch * GeomMath.Rad2Deg + 180f, 360f) - 180f);
                distance = _scene.CameraDistance;
            }

            // Pad with Unicode space that's the same width as a digit/letter
            string Pad(float? value, int pad = 1)
            {
                return (value?.ToString("0") ?? string.Empty).PadLeft(pad, '\u2007');
            }
            // I tried padding the '-' with '\u2008', but the widths don't match. So padding isn't perfect.
            // '\u2004' is a bit closer than '\u2007'.
            string MPad(float? value, int pad = 1)
            {
                if (value.HasValue && value < 0f)
                {
                    return Pad(value, pad);
                }
                else
                {
                    return Pad(value, pad - 1).PadLeft(pad, '\u2004');// '\u2008');
                }
            }

            var C = target.HasValue ? "," : "\u2008"; // Unicode space that's the same width as punctuation

            statusStrip.SuspendLayout();
            camPositionStatusLabel.Text = $"Camera:  {MPad(target?.X, 6)}{C} {MPad(target?.Y, 6)}{C} {MPad(target?.Z, 6)}";
            camRotationStatusLabel.Text = $"Rotation:  {Pad(yaw, 3)}{C} {MPad(pitch, 3)}";
            camDistanceStatusLabel.Text = $"Distance:  {distance:0.0}";
            statusStrip.ResumeLayout();
            statusStrip.Refresh(); // Refresh needed so that labels update at a reasonable pace
        }
    }
}
