using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using OpenTK;
using PSXPrev.Common;
using PSXPrev.Common.Renderer;
using PSXPrev.Common.Utils;
using PSXPrev.Forms.Utils;

namespace PSXPrev.Forms.Controls
{
    public partial class TexturePreviewer : UserControl
    {
        // Minimum scale that semi-transparency lines are shown at
        private const float MinSemiTransparencyZoom = 4.0f;

        // Width/height of a palette entry cell at 100% scale
        private const int PaletteCellSize = 8;

        private const int DefaultDropShadowThickness = 3;

        // Extra pixels to extend picture box by to include far bottom-right UV outlines
        private const int UVExtraEdge = 2;


        // Pens used for drawing UVs
        private readonly Pen _penBlack3Px = new Pen(Color.Black, 3f);
        private readonly Pen _penWhite1Px = new Pen(Color.White, 1f);
        private readonly Pen _penCyan1Px  = new Pen(Color.Cyan,  1f);

        private Pen[] _dropShadowPens = new Pen[DefaultDropShadowThickness];
        private SolidBrush _reusableBrush;
        private Pen _reusablePen;

        // States for preserving previously-drawn semi-transparency lines to reduce lag
        private readonly Bitmap[] _stpDoubleBufferBitmaps = new Bitmap[2];
        private int _stpBitmapIndex; // Index for which double buffer is the front
        // Location represents what pixel the bitmap starts at.
        // Size represents how many pixels are currently-rendered in the bitmap (we can't use bitmap dimensions).
        private Rectangle _stpRectangle;
        private bool _stpInvalidated = true;

        // States for preserving previously-drawing UV lines to reduce lag
        private Bitmap _uvBitmap;
        // Location represents what pixel the bitmap starts at (currently always 0,0).
        // Size represents how many pixels are currently-rendered in the bitmap (we can't use bitmap dimensions).
        private Rectangle _uvRectangle;
        private bool _uvInvalidated = true;


        private int PaletteWidth => _texture?.Palettes == null ? 0 : (_texture.Bpp == 4 ? 4 : 16);

        private int PaletteHeight
        {
            get
            {
                var paletteWidth = PaletteWidth;
                if (paletteWidth > 0)
                {
                    var paletteLength = _texture.Palettes[_texture.CLUTIndex].Length;
                    return (paletteLength + paletteWidth - 1) / paletteWidth;
                }
                return 0;
            }
        }

        private float PixelSize => (IsShowingPalette ? PaletteCellSize : 1) * _scale;


        [Browsable(false)]
        public bool IsShowingTexture => _texture != null;

        [Browsable(false)]
        public bool IsShowingPalette => _showPalette && _texture?.Palettes != null;

        [Browsable(false)]
        public bool IsShowingSemiTransparency => _showSemiTransparency && _texture != null && PixelSize >= MinSemiTransparencyZoom;

        [Browsable(false)]
        public bool IsShowingUVs => _showUVs && _getUVEntities != null && _texture != null && (!_texture.NeedsPacking || _texture.IsPacked);

        [Browsable(false)]
        public bool IsShowingDropShadow => _showDropShadow && _dropShadowThickness > 0 && _texture != null;

        [Browsable(false)]
        public ExtendedPanel PreviewPanel => previewPanel;


        private Texture _texture;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(null)]
        public Texture Texture
        {
            get => _texture;
            set
            {
                if (_texture != value)
                {
                    _texture = value;
                    _stpInvalidated = true;
                    _uvInvalidated = true;
                    UpdatePreviewPictureBoxSize();
                    UpdateStatusBarLabels();
                    previewPictureBox.Invalidate();
                }
            }
        }

        private float _scale = 1f;
        [Category("Behavior")]
        [Description("The display scale for the current texture or palette.")]
        [DefaultValue(1f)]
        public float Zoom
        {
            get => _scale;
            set
            {
                value = GeomMath.Clamp(value, 1.0f / 8.0f, 16.0f); // From 12.5% to 1600%
                if (_scale != value)
                {
                    _scale = value;
                    UpdateZoomLabel();
                    if (_texture != null)
                    {
                        _stpInvalidated = true;
                        _uvInvalidated = true;
                        UpdatePreviewPictureBoxSize();
                        UpdateStatusBarLabels();
                        previewPictureBox.Invalidate();
                    }
                }
            }
        }

        private bool _showPalette;
        [Category("Behavior")]
        [Description("The current texture will display its palette instead of the image (if it has one).")]
        [DefaultValue(false)]
        public bool ShowPalette
        {
            get => _showPalette;
            set
            {
                if (_showPalette != value)
                {
                    var oldIsShowingPalette = IsShowingPalette;
                    _showPalette = value;
                    if (oldIsShowingPalette != IsShowingPalette)
                    {
                        //_stpInvalidated = true; // Currently we don't cache semi-transparency lines for palettes
                        UpdatePreviewPictureBoxSize();
                        UpdateStatusBarLabels();
                        previewPictureBox.Invalidate();
                    }
                }
            }
        }

        private bool _showSemiTransparency;
        [Category("Behavior")]
        [Description("Semi-transparency and transparency will be shown via diagonal lines.")]
        [DefaultValue(false)]
        public bool ShowSemiTransparency
        {
            get => _showSemiTransparency;
            set
            {
                if (_showSemiTransparency != value)
                {
                    var oldIsShowingSemiTransparency = IsShowingSemiTransparency;
                    _showSemiTransparency = value;
                    if (oldIsShowingSemiTransparency != IsShowingSemiTransparency)
                    {
                        previewPictureBox.Invalidate();
                    }
                }
            }
        }

        private bool _showUVs;
        [Category("Behavior")]
        [Description("Entity UVs will be drawn over the texture. Must assign " + nameof(GetUVEntities) + ".")]
        [DefaultValue(false)]
        public bool ShowUVs
        {
            get => _showUVs;
            set
            {
                if (_showUVs != value)
                {
                    var oldIsShowingUVs = IsShowingUVs;
                    _showUVs = value;
                    if (oldIsShowingUVs != IsShowingUVs)
                    {
                        previewPictureBox.Invalidate();
                    }
                }
            }
        }

        private Func<IEnumerable<EntityBase>> _getUVEntities;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(null)]
        public Func<IEnumerable<EntityBase>> GetUVEntities
        {
            get => _getUVEntities;
            set
            {
                if (_getUVEntities != value)
                {
                    var oldIsShowingUVs = IsShowingUVs;
                    _getUVEntities = value;
                    _uvInvalidated = true;
                    if (oldIsShowingUVs || IsShowingUVs) // Update if showing is changed, or if showing at all
                    {
                        previewPictureBox.Invalidate();
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
                    statusStrip.Visible = value;
                }
            }
        }

        private bool _showDropShadow = true;
        [Category("Appearance")]
        [Description("Shows a drop shadow at the bottom-right borders of the texture.")]
        [DefaultValue(true)]
        public bool ShowDropShadow
        {
            get => _showDropShadow;
            set
            {
                if (_showDropShadow != value)
                {
                    var oldIsShowingDropShadow = IsShowingDropShadow;
                    _showDropShadow = value;
                    if (oldIsShowingDropShadow != IsShowingDropShadow)
                    {
                        UpdatePreviewPictureBoxSize(); // Size accounts for extra size of drop shadow
                        previewPictureBox.Invalidate();
                    }
                }
            }
        }

        private int _dropShadowThickness = DefaultDropShadowThickness;
        [Category("Appearance")]
        [Description("How many pixels the drop shadow draws out by.")]
        [DefaultValue(DefaultDropShadowThickness)]
        public int DropShadowThickness
        {
            get => _dropShadowThickness;
            set
            {
                value = GeomMath.Clamp(value, 0, 128);
                if (_dropShadowThickness != value)
                {
                    var oldIsShowingDropShadow = IsShowingDropShadow;
                    // Clear cached drop shadow pens
                    for (var i = 0; i < _dropShadowPens.Length; i++)
                    {
                        _dropShadowPens[i]?.Dispose();
                        _dropShadowPens[i] = null; // Set to null in-case an exception occurs
                    }
                    _dropShadowPens = new Pen[value];
                    _dropShadowThickness = value;
                    if (oldIsShowingDropShadow || IsShowingDropShadow) // Update if showing is changed, or if showing at all
                    {
                        UpdatePreviewPictureBoxSize(); // Size accounts for extra size of drop shadow
                        previewPictureBox.Invalidate();
                    }
                }
            }
        }

        private int _dropShadowIntensity = 100;
        [Category("Appearance")]
        [Description("Alpha level of the darkest part of the drop shadow (between 1 and 255).")]
        [DefaultValue(100)]
        public int DropShadowIntensity
        {
            get => _dropShadowIntensity;
            set
            {
                value = GeomMath.Clamp(value, 1, 255);
                if (_dropShadowIntensity != value)
                {
                    // Clear cached drop shadow pens
                    for (var i = 0; i < _dropShadowPens.Length; i++)
                    {
                        _dropShadowPens[i]?.Dispose();
                        _dropShadowPens[i] = null;
                    }
                    _dropShadowIntensity = value;
                    if (IsShowingDropShadow)
                    {
                        previewPictureBox.Invalidate();
                    }
                }
            }
        }


        public TexturePreviewer()
        {
            // anchorPanel exists solely because anchoring zoomLabel to the UserControl doesn't work.
            // Currently we don't use the old zoomLabel, but we'll keep anchorPanel until we're sure we
            // don't want to switch back.
            // Even though we don't assign the Image property, using a PictureBox for the preview image
            // is important, because it won't flicker when resizing.
            InitializeComponent();

            // Add events that are marked as unbrowsable, and don't show up in the designer.
            Disposed += userControl_Disposed;

            // Setup labels
            UpdateZoomLabel();
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


        // Invalidate changes to the texture data, palette, or UVs
        public void InvalidateTexture()
        {
            if (_texture != null)
            {
                _stpInvalidated = true;
                _uvInvalidated = true;
                previewPictureBox.Invalidate();
            }
        }

        // Invalidate changes to the just the UV data or selected UV entities
        public void InvalidateUVs()
        {
            if (_texture != null)
            {
                _uvInvalidated = true;
                if (IsShowingUVs)
                {
                    previewPictureBox.Invalidate();
                }
            }
        }

        // Creates a bitmap containing the full view of the previewer. Returns null if Texture is null
        public Bitmap CreateBitmap()
        {
            if (_texture == null)
            {
                return null;
            }

            GetPreviewSize(true, out var width, out var height);
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            try
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    graphics.CompositingMode = CompositingMode.SourceOver;

                    if (IsShowingPalette)
                    {
                        DrawPalette(graphics);
                    }
                    else
                    {
                        graphics.Clear(Color.Transparent);

                        DrawTextureImage(graphics);

                        if (IsShowingSemiTransparency)
                        {
                            // Bypass the normal semi-transparency line caching and draw directly to the image
                            DrawSemiTransparencyLines(graphics);
                        }

                        if (IsShowingUVs)
                        {
                            DrawUVs(graphics);
                        }
                    }
                }
                return bitmap;
            }
            catch
            {
                bitmap?.Dispose();
                throw;
            }
        }


        private void userControl_Disposed(object sender, EventArgs e)
        {
            for (var i = 0; i < _stpDoubleBufferBitmaps.Length; i++)
            {
                _stpDoubleBufferBitmaps[i]?.Dispose();
                _stpDoubleBufferBitmaps[i] = null;
            }
            _uvBitmap?.Dispose();
            _uvBitmap = null;
            for (var i = 0; i < _dropShadowPens.Length; i++)
            {
                _dropShadowPens[i]?.Dispose();
                _dropShadowPens[i] = null;
            }
            _reusableBrush?.Dispose();
            _reusableBrush = null;
            _reusablePen?.Dispose();
            _reusablePen = null;
            _penBlack3Px.Dispose();
            _penWhite1Px.Dispose();
            _penCyan1Px.Dispose();
        }

        private void previewPanel_MouseWheelEx(object sender, MouseEventArgs e)
        {
            if (e is HandledMouseEventArgs handledArgs)
            {
                handledArgs.Handled = true;
            }

            var oldZoom = Zoom;
            var newZoom = oldZoom;
            if (e.Delta > 0)
            {
                newZoom *= 2f;
            }
            else if (e.Delta < 0)
            {
                newZoom /= 2f;
            }
            if ((oldZoom < 1f && newZoom > 1f) || (oldZoom > 1f && newZoom < 1f))
            {
                newZoom = 1f; // Always stop at 100% if we pass by it
            }
            Zoom = newZoom;
        }

        private void previewPictureBox_MouseLeave(object sender, EventArgs e)
        {
            UpdateStatusBarLabels(new Point(-1, -1)); // Outside of picture box
        }

        private void previewPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateStatusBarLabels(e.Location);
        }

        private void previewPictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }
            if (_texture == null || previewPictureBox.Width <= 0 || previewPictureBox.Height <= 0)
            {
                return;
            }

            if (IsShowingDropShadow)
            {
                DrawDropShadow(e.Graphics);
            }

            if (IsShowingPalette)
            {
                DrawPalette(e.Graphics, e.Graphics.ClipBounds);
            }
            else
            {
                DrawTexture(e.Graphics, e.Graphics.ClipBounds);
            }

            // Reset drawing mode back to default.
            e.Graphics.InterpolationMode = InterpolationMode.Default;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;
            e.Graphics.SmoothingMode = SmoothingMode.Default;
        }

        private void statusStrip_MouseDown(object sender, MouseEventArgs e)
        {
            // Clicking anywhere in the user control besides the scroll bars should give focus.
            // Note: Change this if we're attaching any buttons or interactable controls to the status bar.
            previewPanel.Focus();
        }


        // Divert focus to the previewPanel and treat it as if this control is focused
        public override bool Focused => base.ContainsFocus;

        protected override void OnEnter(EventArgs e)
        {
            if (base.Focused)
            {
                previewPanel.Focus();
            }
            base.OnEnter(e);
        }


        private Point GetPreviewMousePosition(bool scaled)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return new Point(-1, -1); // Outside of picture box
            }

            var mousePos = previewPictureBox.PointToClient(MousePosition);
            if (!scaled)
            {
                // Use Floor to ensure negative values stay negative
                mousePos.X = (int)Math.Floor(mousePos.X / PixelSize);
                mousePos.Y = (int)Math.Floor(mousePos.Y / PixelSize);
            }
            return mousePos;
        }

        private bool GetPreviewSize(bool scaled, out int width, out int height)
        {
            if (IsShowingPalette)
            {
                width  = PaletteWidth;
                height = PaletteHeight;
            }
            else
            {
                width  = _texture?.Width  ?? 0;
                height = _texture?.Height ?? 0;
            }
            if (scaled)
            {
                width  = (int)(width  * PixelSize);
                height = (int)(height * PixelSize);
            }
            return width > 0 && height > 0;
        }

        private void UpdatePreviewPictureBoxSize()
        {
            GetPreviewSize(true, out var width, out var height);
            // +UVExtraEdge pixels for the bottom-right edge of UV outlines
            var extraSize = Math.Max(UVExtraEdge, (IsShowingDropShadow ? _dropShadowThickness : 0));

            previewPictureBox.SuspendLayout();
            previewPictureBox.Width  = width  + (width  != 0 ? extraSize : 0);
            previewPictureBox.Height = height + (height != 0 ? extraSize : 0);
            previewPictureBox.ResumeLayout();
        }

        private void UpdateZoomLabel()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                //return;
            }

            zoomLabel.Text = $"{_scale:P0}"; // Percent format
            zoomStatusLabel.Text = $"Zoom:  {_scale:P0}"; // Percent format
        }

        private void UpdateStatusBarLabels(Point? mousePosition = null)
        {
            if (!mousePosition.HasValue)
            {
                mousePosition = GetPreviewMousePosition(true);
            }

            Point? position = null;
            Color? color = null;
            bool stp = false, transparent = false;
            int? paletteIndex = null;

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                //return;
            }
            else if (_texture != null && previewPictureBox.ClientRectangle.Contains(mousePosition.Value))
            {
                // Use Floor to ensure negative values stay negative
                var x = (int)Math.Floor(mousePosition.Value.X / PixelSize);
                var y = (int)Math.Floor(mousePosition.Value.Y / PixelSize);

                // Note that we still need to check for out-of-bounds because ClientRectangle
                // will include the extra padding we add on the bottom-right sides.

                if (IsShowingPalette)
                {
                    var palette = _texture.Palettes?[_texture.CLUTIndex];
                    var origPalette = _texture.OriginalPalettes?[_texture.CLUTIndex] ?? palette;
                    var index = y * PaletteWidth + x;
                    if (x >= 0 && y >= 0 && x < PaletteWidth && y < PaletteHeight && index < palette.Length)
                    {
                        var paletteColor = palette[index];
                        position = new Point(x, y);
                        color = TexturePalette.ToColor(origPalette[index], noTransparent: true);
                        stp = TexturePalette.GetStp(paletteColor);
                        transparent = paletteColor == TexturePalette.Transparent;
                        paletteIndex = index;
                    }
                }
                else
                {
                    if (x >= 0 && y >= 0 && x < _texture.Width && y < _texture.Height)
                    {
                        position = new Point(x, y);
                        color = _texture.GetPixel(x, y, out stp, out transparent, out paletteIndex);
                    }
                }
            }

            // Pad with Unicode space that's the same width as a digit/letter
            string Pad(int? value, int pad = 3)
            {
                return value.ToString().PadLeft(pad, '\u2007');
            }

            var C = position.HasValue ? "," : "\u2008"; // Unicode space that's the same width as punctuation
            var hasPalette = _texture?.HasPalette ?? false;

            // Position:  ###, ###
            // Color:  ###, ###, ###
            // T / S /
            // Index:  ### /
            statusStrip.SuspendLayout();
            positionStatusLabel.Text = $"Position:  {Pad(position?.X)}{C} {Pad(position?.Y)}";
            colorStatusLabel.Text = $"Color:  {Pad(color?.R)}{C} {Pad(color?.G)}{C} {Pad(color?.B)}";
            transparencyStatusLabel.Text = (transparent ? "T" : (stp ? "S" : "\u2007"));
            paletteIndexStatusLabel.Text = hasPalette ? $"Index:  {Pad(paletteIndex)}" : string.Empty;
            statusStrip.ResumeLayout();
        }


        private void DrawDropShadow(Graphics graphics)
        {
            graphics.PixelOffsetMode = PixelOffsetMode.None;
            graphics.SmoothingMode = SmoothingMode.None;

            const bool RoundedCorner = true;

            GetPreviewSize(true, out var width, out var height);
            for (var i = 0; i < _dropShadowThickness; i++)
            {
                var pen = _dropShadowPens[i];
                if (pen == null)
                {
                    var alpha = (_dropShadowThickness - i) * ((float)_dropShadowIntensity / _dropShadowThickness);
                    _dropShadowPens[i] = pen = new Pen(Color.FromArgb((int)alpha, 0, 0, 0), 1f);
                }
                // Draw horizontal then vertical shadow line
                var x = width  + i;
                var y = height + i;
                graphics.DrawLine(pen, 0, y, x - (RoundedCorner ? 1 : 0), y);
                graphics.DrawLine(pen, x, 0, x, y - 1); // -1 to avoid overlapping at corner
                if (RoundedCorner && i > 0)
                {
                    graphics.FillRectangle(pen.Brush, x - 1, y - 1, 1, 1);
                }
            }

            graphics.PixelOffsetMode = PixelOffsetMode.Default;
            graphics.SmoothingMode = SmoothingMode.Default;
        }

        private void DrawPalette(Graphics graphics, RectangleF? clipBounds = null)
        {
            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            graphics.PixelOffsetMode = PixelOffsetMode.None;
            graphics.SmoothingMode = SmoothingMode.None;

            var paletteWidth  = PaletteWidth;
            var paletteHeight = PaletteHeight;
            if (!GetClipRect(clipBounds, previewPanel, paletteWidth, paletteHeight, PixelSize, out var clipRect, out var viewRect))
            {
                return; // Nothing to draw
            }
            // We're not drawing to a cached region bitmap (unlike stp), so we don't need to offset the location
            var location = Point.Empty;// viewRect.Location;

            var backColor = BackColor;
            var palette = _texture.Palettes[_texture.CLUTIndex];
            var origPalette = _texture.OriginalPalettes?[_texture.CLUTIndex] ?? palette;
            var cellSize = Math.Max(1, (int)PixelSize);
            var showingSemiTransparency = IsShowingSemiTransparency;

            for (var y = clipRect.Top; y < clipRect.Bottom; y++)
            {
                for (var x = clipRect.Left; x < clipRect.Right; x++)
                {
                    var index = y * paletteWidth + x;
                    if (index >= palette.Length)
                    {
                        // Break out of both loops, since any x/y higher than this will also be outside the palette bounds
                        y = clipRect.Bottom;
                        break;
                    }
                    var color = palette[index];
                    var solidColor = TexturePalette.ToColor(origPalette[index], noTransparent: true);
                    var transparent = color == TexturePalette.Transparent;
                    var xx = ((x - location.X) * cellSize);
                    var yy = ((y - location.Y) * cellSize);

                    if (!transparent || _texture.OriginalPalettes != null)
                    {
                        var brush = ReuseBrush(ref _reusableBrush, solidColor);
                        graphics.FillRectangle(brush, new Rectangle(xx, yy, cellSize, cellSize));
                    }

                    if (showingSemiTransparency && (transparent || TexturePalette.GetStp(color)))
                    {
                        if (transparent && _texture.OriginalPalettes == null)
                        {
                            // We don't have the original unmasked color, default to showing behind the palette
                            solidColor = backColor;
                        }
                        DrawSemiTransparencyLine(graphics, xx, yy, cellSize, cellSize, solidColor, transparent);
                    }
                }
            }
        }

        private void DrawTexture(Graphics graphics, RectangleF? clipBounds = null)
        {
            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

            DrawTextureImage(graphics);

            if (IsShowingSemiTransparency)
            {
                DrawSemiTransparencyCached(graphics, clipBounds);
            }

            if (IsShowingUVs)
            {
                DrawUVsCached(graphics, clipBounds);
            }
        }

        private void DrawTextureImage(Graphics graphics)
        {
            GetPreviewSize(true, out var width, out var height);
            var dstRect = new Rectangle(0, 0, width, height);
            var srcRect = new Rectangle(0, 0, _texture.Width, _texture.Height);

            // Despite what it sounds like, we want Half. Otherwise we end up drawing half a pixel back.
            graphics.PixelOffsetMode = PixelOffsetMode.Half;
            graphics.DrawImage(_texture.Bitmap, dstRect, srcRect, GraphicsUnit.Pixel);
        }

        private void DrawSemiTransparencyCached(Graphics graphics, RectangleF? clipBounds = null)
        {
            var stpOverlayBitmap = UpdateSemiTransparencyCache(clipBounds);

            if (stpOverlayBitmap != null)
            {
                var dstX = (int)(_stpRectangle.X * _scale);
                var dstY = (int)(_stpRectangle.Y * _scale);
                var srcRect = new Rectangle(0,
                                            0,
                                            (int)(_stpRectangle.Width  * _scale),
                                            (int)(_stpRectangle.Height * _scale));

                // Despite what it sounds like, we want Half. Otherwise we end up drawing half a pixel back.
                graphics.PixelOffsetMode = PixelOffsetMode.Half;
                graphics.DrawImage(stpOverlayBitmap, dstX, dstY, srcRect, GraphicsUnit.Pixel);
            }
        }

        private Bitmap UpdateSemiTransparencyCache(RectangleF? clipBounds = null)
        {
            var imageWidth  = _texture.Width;
            var imageHeight = _texture.Height;
            if (!GetClipRect(clipBounds, previewPanel, imageWidth, imageHeight, _scale, out var clipRect, out var viewRect))
            {
                return null; // Nothing to draw
            }
            var width  = (int)(viewRect.Width  * _scale);
            var height = (int)(viewRect.Height * _scale);
            var preserveRect = _stpRectangle;
            preserveRect.Intersect(viewRect);

            var needsUpdate = true;
            if (_stpInvalidated)
            {
                _stpRectangle = preserveRect = Rectangle.Empty;
            }
            else if (!_stpRectangle.Contains(viewRect))
            {
                // Swap which bitmap is used to draw the lines, and use the old one to draw onto the new one
                _stpBitmapIndex = 1 - _stpBitmapIndex;
            }
            else
            {
                // All visible lines are already rendered, nothing to update
                needsUpdate = false;
            }

            var front = _stpDoubleBufferBitmaps[_stpBitmapIndex];
            var back  = _stpDoubleBufferBitmaps[1 - _stpBitmapIndex];
            if (needsUpdate)
            {
                if (front == null || front.Width < width || front.Height < height)
                {
                    var newWidth  = Math.Max(1, Math.Max(width,  front?.Width  ?? 0));
                    var newHeight = Math.Max(1, Math.Max(height, front?.Height ?? 0));
                    front?.Dispose(); // Make sure to dispose of after we get the old dimensions

                    // Format32bppPArgb is MUCH MUCH faster to render to than Format32bppArgb
                    front = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppPArgb);
                    _stpDoubleBufferBitmaps[_stpBitmapIndex] = front;
                }

                using (var graphics = Graphics.FromImage(front))
                {
                    graphics.Clear(Color.Transparent);

                    graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

                    if (!preserveRect.IsEmpty && back != null)
                    {
                        // Offset destination by the change in initial X,Y
                        var dstX = (int)((_stpRectangle.X - viewRect.X) * _scale);
                        var dstY = (int)((_stpRectangle.Y - viewRect.Y) * _scale);
                        var srcRect = new Rectangle(0,
                                                    0,
                                                    (int)(_stpRectangle.Width  * _scale),
                                                    (int)(_stpRectangle.Height * _scale));

                        // Despite what it sounds like, we want Half. Otherwise we end up drawing half a pixel back.
                        graphics.PixelOffsetMode = PixelOffsetMode.Half;
                        graphics.DrawImage(back, dstX, dstY, srcRect, GraphicsUnit.Pixel);
                    }

                    DrawSemiTransparencyLines(graphics, clipRect, viewRect.Location, preserveRect);
                }
                _stpRectangle = viewRect;
                _stpInvalidated = false;
            }

            return front;
        }

        private void DrawSemiTransparencyLines(Graphics graphics, Rectangle? optionalClipRect = null, Point location = default(Point), Rectangle preserveRect = default(Rectangle))
        {
            graphics.PixelOffsetMode = PixelOffsetMode.None;
            graphics.SmoothingMode = SmoothingMode.None;

            var backColor = BackColor;
            _texture.Lock(); // Lock the texture once for all accesses to the pixel data
            // Preserve the last-used pen to avoid creating a new one when the color hasn't changed
            Pen pen = null;
            try
            {
                var clipRect = optionalClipRect ?? new Rectangle(0, 0, _texture.Width, _texture.Height);

                for (var y = clipRect.Top; y < clipRect.Bottom; y++)
                {
                    for (var x = clipRect.Left; x < clipRect.Right; x++)
                    {
                        if (preserveRect.Contains(x, y))
                        {
                            x = preserveRect.Right - 1; // Skip to end of preserve rect width
                            continue; // Already drawn
                        }
                        var xx = ((x - location.X) * _scale);
                        var yy = ((y - location.Y) * _scale);
                        var solidColor = _texture.GetPixel(x, y, out var stp, out var transparent, out var paletteIndex);
                        if (transparent)
                        {
                            solidColor = backColor;
                        }
                        if (transparent || stp)
                        {
                            DrawSemiTransparencyLine(graphics, xx, yy, _scale, _scale, solidColor, transparent);
                        }
                    }
                }
            }
            finally
            {
                _texture.Unlock();
                pen?.Dispose();
            }
        }

        private void DrawSemiTransparencyLine(Graphics graphics, float x, float y, float width, float height, Color backColor, bool transparent)
        {
            // Get a color that's easily visible against the pixel background by using delta values for each channel.
            // This is similar to inverting a color (255 - c), but doesn't have the drawback of being less visible
            // the closer you are to 128.
            // If we want to optimize this, we can do: (channel ^ 0x80), which both adds 128 and wraps around 256.
            var penColor = Color.FromArgb((backColor.R + 128) % 256,
                                          (backColor.G + 128) % 256,
                                          (backColor.B + 128) % 256);

            var pen = ReusePen(ref _reusablePen, penColor);
            // The -1's here are used for the higher of the two components to prevent the line from drawing one pixel off/too far.
            if (transparent)
            {
                // Draw transparent line from top-right to bottom-left
                graphics.DrawLine(pen, x + width - 1, y, x, y + height - 1); // -1 for x1 and y2 to fix line being offset by one
            }
            else
            {
                // Draw semi-transparent line from top-left to bottom-right
                graphics.DrawLine(pen, x, y, x + width - 1, y + height - 1); // -1 for x2 and y2 to fix line drawing one extra pixel in the bottom-right
            }
        }


        private void DrawUVsCached(Graphics graphics, RectangleF? clipBounds = null)
        {
            var uvOverlayBitmap = UpdateUVsCache(clipBounds);

            if (uvOverlayBitmap != null)
            {
                // Currently we always cache and draw the entire set of UV lines, and dstX,dstY are always 0,0.
                // They would be non-zero if we were only caching the viewable area of the texture.
                var dstX = (int)(_uvRectangle.X * _scale);
                var dstY = (int)(_uvRectangle.Y * _scale);
                // +UVExtraEdge pixels for the bottom-right edge of UV outlines
                var srcRect = new Rectangle(0,
                                            0,
                                            (int)(_uvRectangle.Width  * _scale) + UVExtraEdge,
                                            (int)(_uvRectangle.Height * _scale) + UVExtraEdge);

                // Despite what it sounds like, we want Half. Otherwise we end up drawing half a pixel back.
                graphics.PixelOffsetMode = PixelOffsetMode.Half;
                graphics.DrawImage(uvOverlayBitmap, dstX, dstY, srcRect, GraphicsUnit.Pixel);
            }
        }

        private Bitmap UpdateUVsCache(RectangleF? clipBounds = null)
        {
            // To optimize drawing all UV lines in one go, we limit the size of the overlay bitmap to the size of a VRAM page.
            // If the texture is larger than this, then we clip everything past that, since it holds no meaning.
            // Additionally, if the texture position causes the texture size to end outside the VRAM page, we clip that too.
            var endX = Math.Min(VRAM.PageSize, (_texture.X + _texture.Width));
            var endY = Math.Min(VRAM.PageSize, (_texture.Y + _texture.Height));
            var vramWidth  = (endX - _texture.X);
            var vramHeight = (endY - _texture.Y);
            // No need to check clip rect, since we always cache the entire set of UV lines at once.
            /*if (!GetClipRect(clipBounds, previewPanel, vramWidth, vramHeight, _scale, out var clipRect, out var viewRect))
            {
                return null; // Nothing to draw
            }*/

            var width  = (int)(vramWidth  * _scale);
            var height = (int)(vramHeight * _scale);
            if (width <= 0 || height <= 0)
            {
                _uvRectangle = Rectangle.Empty;
                return null; // Nothing to draw
            }
            // +UVExtraEdge pixels for the bottom-right edge of UV outlines
            width  += UVExtraEdge;
            height += UVExtraEdge;

            if (_uvBitmap == null || _uvBitmap.Width < width || _uvBitmap.Height < height)
            {
                var newWidth  = Math.Max(1, Math.Max(width,  _uvBitmap?.Width  ?? 0));
                var newHeight = Math.Max(1, Math.Max(height, _uvBitmap?.Height ?? 0));
                _uvBitmap?.Dispose(); // Make sure to dispose of after we get the old dimensions

                // Format32bppPArgb is MUCH MUCH faster to render to than Format32bppArgb
                _uvBitmap = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppPArgb);
            }
            else if (!_uvInvalidated)
            {
                return _uvBitmap; // Everything is already cached
            }

            using (var graphics = Graphics.FromImage(_uvBitmap))
            {
                graphics.Clear(Color.Transparent);

                // Set the clip bounds to the area we're drawing to.
                // We want to limit expensive draw operations to only what will be visible.
                graphics.SetClip(new Rectangle(0, 0, width, height));

                DrawUVs(graphics);

                _uvRectangle = new Rectangle(0, 0, vramWidth, vramHeight);
                _uvInvalidated = false;
            }

            return _uvBitmap;
        }

        private void DrawUVs(Graphics graphics)
        {
            // todo: If we want smoother lines, then we can turn on Anti-aliasing.
            // Note that PixelOffsetMode needs to be changed back to None first.
            // Also note that diagonal lines will be a bit thicker than normal.
            graphics.PixelOffsetMode = PixelOffsetMode.None;
            graphics.SmoothingMode = SmoothingMode.None;
            //graphics.SmoothingMode = SmoothingMode.AntiAlias;
            foreach (var entity in _getUVEntities())
            {
                DrawEntityUVs(entity, graphics);
            }

            graphics.PixelOffsetMode = PixelOffsetMode.Default;
            graphics.SmoothingMode = SmoothingMode.Default;
        }

        private void DrawEntityUVs(EntityBase entity, Graphics graphics)
        {
            // Don't draw UVs for this model unless it uses the same texture page that we're on.
            if (entity is ModelEntity model && model.IsTextured && !model.MissingTexture && model.TexturePage == _texture.TexturePage)
            {
                var uvConverter = model.TextureLookup;

                // Draw all black outlines before inner fill lines, so that black outline edges don't overlap fill lines.
                foreach (var triangle in model.Triangles)
                {
                    if (triangle.IsTiled)
                    {
                        // Triangle.Uv is useless when tiled, so draw the TiledUv area instead.
                        DrawTiledAreaUVs(graphics, _penBlack3Px, triangle.TiledUv, uvConverter);
                    }
                    else
                    {
                        DrawTriangleUVs(graphics, _penBlack3Px, triangle.Uv, uvConverter);
                    }
                }

                foreach (var triangle in model.Triangles)
                {
                    if (triangle.IsTiled)
                    {
                        // Different color for tiled area.
                        DrawTiledAreaUVs(graphics, _penCyan1Px, triangle.TiledUv, uvConverter);
                    }
                    else
                    {
                        DrawTriangleUVs(graphics, _penWhite1Px, triangle.Uv, uvConverter);
                    }
                }
            }

            if (entity?.ChildEntities != null)
            {
                foreach (var child in entity.ChildEntities)
                {
                    DrawEntityUVs(child, graphics);
                }
            }
        }

        private void DrawTriangleUVs(Graphics graphics, Pen pen, Vector2[] uvs, IUVConverter uvConverter)
        {
            var x = _texture.X * _scale; // Support for displaying UVs on normal textures
            var y = _texture.Y * _scale;
            var scalar = GeomMath.UVScalar * _scale;
            var uvLast = uvs[uvs.Length - 1];
            if (uvConverter != null)
            {
                uvLast = uvConverter.ConvertUV(uvLast, false);
            }
            for (var i = 0; i < uvs.Length; i++)
            {
                var uv = uvs[i];
                if (uvConverter != null)
                {
                    uv = uvConverter.ConvertUV(uv, false);
                }
                graphics.DrawLine(pen, (uvLast.X * scalar) - x, (uvLast.Y * scalar) - y, (uv.X * scalar) - x, (uv.Y * scalar) - y);
                uvLast = uv;
            }
        }

        private void DrawTiledAreaUVs(Graphics graphics, Pen pen, TiledUV tiledUv, IUVConverter uvConverter)
        {
            var x = _texture.X * _scale; // Support for displaying UVs on normal textures
            var y = _texture.Y * _scale;
            var scalar = GeomMath.UVScalar * _scale;
            var tiledArea = tiledUv.Area;
            if (uvConverter != null)
            {
                tiledArea = uvConverter.ConvertTiledArea(tiledArea);
            }
            graphics.DrawRectangle(pen, (tiledArea.X * scalar) - x, (tiledArea.Y * scalar) - y, (tiledArea.Z * scalar), (tiledArea.W * scalar));
        }


        private static SolidBrush ReuseBrush(ref SolidBrush brush, Color color)
        {
            // NEVER use Color equality, because it also checks stupid things like name.
            if (brush == null)
            {
                brush = new SolidBrush(color);
            }
            else if (!brush.Color.EqualsRgb(color))//.ToArgb() != color.ToArgb())
            {
                brush.Color = color; // We can just change the brush color instead of creating a new brush
            }
            return brush;
        }

        private static Pen ReusePen(ref Pen pen, Color color)
        {
            // NEVER use Color equality, because it also checks stupid things like name.
            if (pen == null)
            {
                pen = new Pen(color, 1f);
            }
            else if (!pen.Color.EqualsRgb(color))//.ToArgb() != color.ToArgb())
            {
                pen.Color = color; // We can just change the pen color instead of creating a new pen
            }
            return pen;
        }

        private static bool GetClipRect(RectangleF? clipBounds, Panel panel, int width, int height, float scale, out Rectangle clipRect, out Rectangle viewRect)
        {
            if (clipBounds.HasValue)
            {
                var startX = Math.Max(0, (int)Math.Floor(clipBounds.Value.Left / scale));
                var startY = Math.Max(0, (int)Math.Floor(clipBounds.Value.Top  / scale));
                var endX = Math.Min(width,  (int)Math.Ceiling(clipBounds.Value.Right  / scale));
                var endY = Math.Min(height, (int)Math.Ceiling(clipBounds.Value.Bottom / scale));
                clipRect = new Rectangle(startX, startY, endX - startX, endY - startY);

                var scrollRect = panel.GetAutoScrollRectangle();
                startX = Math.Max(0, (int)Math.Floor(scrollRect.Left / scale));
                startY = Math.Max(0, (int)Math.Floor(scrollRect.Top  / scale));
                endX = Math.Min(width,  (int)Math.Ceiling(scrollRect.Right  / scale));
                endY = Math.Min(height, (int)Math.Ceiling(scrollRect.Bottom / scale));
                viewRect = new Rectangle(startX, startY, endX - startX, endY - startY);
            }
            else
            {
                clipRect = viewRect = new Rectangle(0, 0, width, height);
            }

            return /*clipRect.Width > 0 && clipRect.Height > 0 &&*/ viewRect.Width > 0 && viewRect.Height > 0;
        }
    }
}
