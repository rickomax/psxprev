using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace PSXPrev.Common.Renderer
{
    public class VRAM : IReadOnlyList<Texture>, IDisposable
    {
        public const int PageCount = 32;
        public const int PageSize = 256;
        public const int PackAlign = 8;
        public const int PackBlocks = PageSize / PackAlign;
        private const int PageSemiTransparencyX = PageSize;

        public static readonly System.Drawing.Color DefaultBackgroundColor = System.Drawing.Color.White;


        private readonly Scene _scene;
        private readonly Texture[] _vramPages = new Texture[PageCount];
        // Pages that require a scene update.
        private readonly bool[] _modifiedPages = new bool[PageCount];
        // Pages that have textures drawn to them (not reset unless cleared).
        private readonly bool[] _usedPages = new bool[PageCount];

        private readonly bool[,,] _packedPageBlocks = new bool[PageCount, PackBlocks, PackBlocks]; // [Page,X,Y]
        private readonly int[] _freePageBlocks = new int[PageCount];

        public System.Drawing.Color BackgroundColor { get; set; } = DefaultBackgroundColor;

        public bool Initialized { get; private set; }

        public VRAM(Scene scene)
        {
            _scene = scene;
        }

        public Texture this[uint index] => _vramPages[index];
        public Texture this[int index] => _vramPages[index];

        public int Count => PageCount;

        public IEnumerator<Texture> GetEnumerator()
        {
            return ((IReadOnlyList<Texture>)_vramPages).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Texture[] ToArray()
        {
            return (Texture[])_vramPages.Clone();
        }

        public void Dispose()
        {
            if (Initialized)
            {
                for (var i = 0; i < PageCount; i++)
                {
                    _vramPages[i]?.Dispose();
                    _vramPages[i] = null;
                    _modifiedPages[i] = false;
                    _usedPages[i] = false;
                }
                ClearAllPagePacking();
                Initialized = false;
            }
        }

        public void Initialize(bool suppressUpdate = false)
        {
            if (Initialized)
            {
                return;
            }
            for (var i = 0; i < PageCount; i++)
            {
                if (_vramPages[i] == null)
                {
                    // X coordinates [0,256) store texture data.
                    // X coordinates [256,512) store semi-transparency information for textures.
                    _vramPages[i] = new Texture(PageSize * 2, PageSize, 0, 0, 32, i, 0, null, null, true); // Is VRAM page
                    _vramPages[i].Name = $"VRAM[{i}]";
                    ClearPage(i, suppressUpdate);
                }
            }
            Initialized = true;
        }

        public void AssignModelTextures(RootEntity rootEntity)
        {
            foreach (ModelEntity model in rootEntity.ChildEntities)
            {
                AssignModelTextures(model);
            }
        }

        public void AssignModelTextures(ModelEntity model)
        {
            if (model.IsTextured && ContainsPage(model.TexturePage))
            {
                model.Texture = _vramPages[model.TexturePage];
            }
            else
            {
                model.Texture = null;
            }
        }

        // Gets if a page has had at least one texture drawn to it.
        public bool IsPageUsed(uint index) => IsPageUsed((int)index);

        public bool IsPageUsed(int index)
        {
            return _usedPages[index];
        }

        // Returns true if the index is a valid VRAM texture page number.
        public bool ContainsPage(uint index) => ContainsPage((int)index);

        public bool ContainsPage(int index)
        {
            return index >= 0 && index < PageCount;
        }

        public void ClearPagePacking(uint index) => ClearPagePacking((int)index);

        public void ClearPagePacking(int index)
        {
            for (var px = 0; px < PackBlocks; px++)
            {
                for (var py = 0; py < PackBlocks; py++)
                {
                    _packedPageBlocks[index, px, py] = false;
                }
            }
            _freePageBlocks[index] = PackBlocks * PackBlocks;
        }

        public void ClearAllPagePacking()
        {
            for (var i = 0; i < PageCount; i++)
            {
                ClearPagePacking(i);
            }
        }

        // Update page textures in the scene.
        public bool UpdatePage(uint index, bool force = false) => UpdatePage((int)index, force);

        public bool UpdatePage(int index, bool force = false)
        {
            if (force || _modifiedPages[index])
            {
                // Support using VRAM even if we have no Scene.
                _scene?.TextureBinder.UpdateTexture(_vramPages[index].Bitmap, index);
                _modifiedPages[index] = false;
                return true;
            }
            return false;
        }

        public bool UpdateAllPages()
        {
            var anyUpdated = false;
            for (var i = 0; i < PageCount; i++)
            {
                anyUpdated |= UpdatePage(i, false); // Only update page if modified.
            }
            return anyUpdated;
        }

        // Clear page textures to background color.
        public void ClearPage(uint index, bool suppressUpdate = false) => ClearPage((int)index, suppressUpdate);

        public void ClearPage(int index, bool suppressUpdate = false)
        {
            using (var graphics = Graphics.FromImage(_vramPages[index].Bitmap))
            {
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                // Clear texture data to background color.
                graphics.Clear(BackgroundColor);

                // Clear semi-transparent information to its default.
                using (var brush = new SolidBrush(Texture.NoSemiTransparentFlag))
                {
                    graphics.FillRectangle(brush, PageSemiTransparencyX, 0, PageSize, PageSize);
                }
            }

            for (var px = 0; px < PackBlocks; px++)
            {
                for (var py = 0; py < PackBlocks; py++)
                {
                    _packedPageBlocks[index, px, py] = false;
                }
            }
            _freePageBlocks[index] = PackBlocks * PackBlocks;

            _usedPages[index] = false;
            if (suppressUpdate)
            {
                _modifiedPages[index] = true;
            }
            else
            {
                UpdatePage(index, true);
            }
        }

        public void ClearAllPages(bool suppressUpdate = false)
        {
            for (var i = 0; i < PageCount; i++)
            {
                ClearPage(i, suppressUpdate);
            }
        }

        // Draw texture onto page.
        public void DrawTexture(Texture texture, bool suppressUpdate = false)
        {
            var index = ClampTexturePage(texture.TexturePage);
            DrawTexture(_vramPages[index], texture);

            GetTexturePackBounds(texture, out var startX, out var startY, out var endX, out var endY);
            for (var px = startX; px <= endX; px++)
            {
                for (var py = startY; py <= endY; py++)
                {
                    if (!_packedPageBlocks[index, px, py])
                    {
                        _packedPageBlocks[index, px, py] = true;
                        _freePageBlocks[index]--;
                    }
                }
            }

            _usedPages[index] = true;
            if (suppressUpdate)
            {
                _modifiedPages[index] = true;
            }
            else
            {
                UpdatePage(index, true);
            }
        }

        // Frees up packing space used by this texture
        public void RemoveTexturePacking(Texture texture)
        {
            var index = ClampTexturePage(texture.TexturePage);

            GetTexturePackBounds(texture, out var startX, out var startY, out var endX, out var endY);
            for (var px = startX; px <= endX; px++)
            {
                for (var py = startY; py <= endY; py++)
                {
                    if (_packedPageBlocks[index, px, py])
                    {
                        _packedPageBlocks[index, px, py] = false;
                        _freePageBlocks[index]++;
                    }
                }
            }
        }

        // Finds an unused area of VRAM to pack this texture into
        public bool FindPackLocation(Texture texture, out int page, out int x, out int y)
        {
            var packWidth  = Math.Max(1, (texture.Width  + PackAlign - 1) / PackAlign);
            var packHeight = Math.Max(1, (texture.Height + PackAlign - 1) / PackAlign);
            var packBlocks = packWidth * packHeight;
            var endX = PackBlocks - packWidth;
            var endY = PackBlocks - packHeight;
            for (var i = 0; i < PageCount; i++)
            {
                if (_freePageBlocks[i] < packBlocks)
                {
                    continue;
                }
                for (var px = 0; px <= endX; px++)
                {
                    for (var py = 0; py <= endY; py++)
                    {
                        var obstructed = false;
                        for (var tx = 0; !obstructed && tx < packWidth; tx++)
                        {
                            for (var ty = 0; !obstructed && ty < packHeight; ty++)
                            {
                                if (_packedPageBlocks[i, px + tx, py + ty])
                                {
                                    obstructed = true;
                                }
                            }
                        }
                        if (!obstructed)
                        {
                            page = i;
                            x = px * PackAlign;
                            y = py * PackAlign;
                            return true;
                        }
                    }
                }
            }
            page = 0;
            x = 0;
            y = 0;
            return false;
        }

        private static void GetTexturePackBounds(Texture texture, out int startX, out int startY, out int endX, out int endY)
        {
            startX = Math.Max(0, texture.X / PackAlign);
            startY = Math.Max(0, texture.Y / PackAlign);
            endX = Math.Min(PackBlocks - 1, (texture.X + texture.Width  - 1) / PackAlign);
            endY = Math.Min(PackBlocks - 1, (texture.Y + texture.Height - 1) / PackAlign);
        }


        public static void DrawTexture(Texture vramTexture, Texture texture)
        {
            var textureX = texture.X;
            var textureY = texture.Y;
            var textureWidth = texture.Width;
            var textureHeight = texture.Height;
            var textureBitmap = texture.Bitmap;
            var textureSemiTransparentMap = texture.SemiTransparentMap;
            using (var graphics = Graphics.FromImage(vramTexture.Bitmap))
            {
                // Use SourceCopy to overwrite image alpha with alpha stored in textures.
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                // Draw the actual texture to VRAM.
                // Clip drawing region so we don't draw over semi-transparent information.
                graphics.SetClip(new Rectangle(0, 0, PageSize, PageSize));
                graphics.DrawImage(textureBitmap, textureX, textureY, textureWidth, textureHeight);

                // Draw semi-transparent information to VRAM in X coordinates [256,512).
                graphics.SetClip(new Rectangle(PageSemiTransparencyX, 0, PageSize, PageSize));
                if (textureSemiTransparentMap != null)
                {
                    graphics.DrawImage(textureSemiTransparentMap, PageSemiTransparencyX + textureX, textureY, textureWidth, textureHeight);
                }
                else
                {
                    using (var brush = new SolidBrush(Texture.NoSemiTransparentFlag))
                    {
                        graphics.FillRectangle(brush, PageSemiTransparencyX + textureX, textureY, textureWidth, textureHeight);
                    }
                }
                graphics.ResetClip();
            }
        }

        // Returns a bitmap of the VRAM texture page without the semi-transparency section.
        // Must dispose of Bitmap after use.
        public static Bitmap ConvertTexture(Texture texture, bool semiTransparency)
        {
            var stpX = semiTransparency ? PageSemiTransparencyX : 0;
            var srcRect = new Rectangle(stpX, 0, PageSize, PageSize);

            var bitmap = new Bitmap(PageSize, PageSize);
            try
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                    graphics.DrawImage(texture.Bitmap, 0, 0, srcRect, GraphicsUnit.Pixel);
                }
                return bitmap;
            }
            catch
            {
                bitmap?.Dispose();
                throw;
            }
        }

        public static Bitmap ConvertTiledTexture(Texture texture, Rectangle srcRect, int repeatX, int repeatY, int? fullWidth, int? fullHeight, bool semiTransparency)
        {
            var stpX = semiTransparency ? PageSemiTransparencyX : 0;
            srcRect.X += stpX;

            if (!fullWidth.HasValue)
            {
                fullWidth  = repeatX * srcRect.Width;
            }
            if (!fullHeight.HasValue)
            {
                fullHeight = repeatY * srcRect.Height;
            }

            var bitmap = new Bitmap(fullWidth.Value, fullHeight.Value);
            try
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                    // Full size might be larger than tiled size.
                    graphics.Clear(DefaultBackgroundColor);

                    for (var ry = 0; ry < repeatY; ry++)
                    {
                        for (var rx = 0; rx < repeatX; rx++)
                        {
                            var x = rx * srcRect.Width;
                            var y = ry * srcRect.Height;
                            graphics.DrawImage(texture.Bitmap, x, y, srcRect, GraphicsUnit.Pixel);
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

        // Draw individual textures into 256x256 cells.
        public static Bitmap ConvertSingleTexture(IEnumerable<Texture> textures, int countX, int countY, bool semiTransparency)
        {
            var packedTextures = new List<Texture[]>();
            foreach (var texture in textures)
            {
                packedTextures.Add(new Texture[1] { texture });
            }
            return ConvertSingleTexture(packedTextures, countX, countY, semiTransparency);
        }

        // Pack textures into 256x256 cells. The inner enumerable of textures will all be drawn to the same cell,
        // and the Texture X,Y determines where in the cell that texture is drawn.
        public static Bitmap ConvertSingleTexture(IEnumerable<IEnumerable<Texture>> packedTextures, int countX, int countY, bool semiTransparency)
        {
            var bitmap = new Bitmap(countX * PageSize, countY * PageSize);
            try
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                    // Clear the image, because there may be cells that we don't fill in.
                    if (!semiTransparency)
                    {
                        graphics.Clear(DefaultBackgroundColor);
                    }
                    else
                    {
                        graphics.Clear(Texture.NoSemiTransparentFlag);
                    }

                    var i = 0;
                    foreach (var cell in packedTextures)
                    {
                        var x = (i % countX) * PageSize;
                        var y = (i / countX) * PageSize;
                        i++;

                        foreach (var texture in cell)
                        {
                            graphics.SetClip(new Rectangle(x, y, PageSize, PageSize));
                            if (texture.IsVRAMPage)
                            {
                                var stpX = semiTransparency ? PageSemiTransparencyX : 0;
                                var srcRect = new Rectangle(stpX, 0, PageSize, PageSize);
                                graphics.DrawImage(texture.Bitmap, x, y, srcRect, GraphicsUnit.Pixel);
                            }
                            else
                            {
                                var textureBitmap = semiTransparency ? texture.SemiTransparentMap : texture.Bitmap;
                                // Texture may not have semi-transparent map
                                if (textureBitmap != null)
                                {
                                    graphics.DrawImage(textureBitmap, x + texture.X, y + texture.Y);
                                    // Packed boundary debugging:
                                    //graphics.DrawRectangle(Pens.Red, new Rectangle(x + texture.X, y + texture.Y, texture.Width, texture.Height));
                                }
                            }
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

        // Explicitly define uint overload to properly clamp values above int.MaxValue.
        public static uint ClampTexturePage(uint index)
        {
            return GeomMath.Clamp(index, 0, (PageCount - 1));
        }

        public static int ClampTexturePage(int index)
        {
            return GeomMath.Clamp(index, 0, (PageCount - 1));
        }

        public static int ClampTextureX(int x)
        {
            return GeomMath.Clamp(x, 0, (PageSize - 1));
        }

        public static int ClampTextureY(int y)
        {
            return GeomMath.Clamp(y, 0, (PageSize - 1));
        }
    }
}
