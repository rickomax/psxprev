using System;
using System.Drawing;
using System.IO;

namespace PSXPrev.Classes
{
    public class TIMParser
    {
        private long _offset;
        private readonly Action<Texture, long> _textureAddedAction;

        public TIMParser(Action<Texture, long> textureAdded)
        {
            _textureAddedAction = textureAdded;
        }

        public void LookForTim(BinaryReader reader, string fileTitle)
        {
            if (reader == null)
            {
                throw (new Exception("File must be opened"));
            }

            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            //var textures = new List<Texture>();

            while (reader.BaseStream.CanRead)
            {
                var passed = false;
                try
                {
                    var id = reader.ReadUInt16();
                    if (id == 0x10)
                    {
                        var version = reader.ReadUInt16();
                        if (version == 0x00)
                        {
                            var texture = ParseTim(reader);
                            if (texture != null)
                            {
                                texture.TextureName = string.Format("{0}{1:x}", fileTitle, _offset > 0 ? "_" + _offset : string.Empty);
                                //textures.Add(texture);
                                _textureAddedAction(texture, _offset);
                                Program.Logger.WritePositiveLine("Found TIM Image at offset {0:X}", _offset);
                                _offset = reader.BaseStream.Position;
                                passed = true;
                            }
                        }
                    }

                }
                catch (Exception exp)
                {
                    //if (Program.Debug)
                    //{
                    //    Program.Logger.WriteLine(exp);
                    //}
                }
                if (!passed)
                {
                    if (++_offset > reader.BaseStream.Length)
                    {
                        Program.Logger.WriteLine($"TIM - Reached file end: {fileTitle}");
                        return;
                    }
                    reader.BaseStream.Seek(_offset, SeekOrigin.Begin);
                }
            }
        }

        private Texture ParseTim(BinaryReader reader)
        {
            Texture texture = null;
            Bitmap bitmap;

            var palette = new System.Drawing.Color[] { };

            var flag = reader.ReadUInt32();
            var pmode = (flag & 0x7);
            if (pmode > 4)
            {
                return null;
            }

            var cf = (flag & 0x8) >> 3;
            if (cf > 1)
            {
                return null;
            }

            if (pmode < 2 && cf != 1)
            {
                return null;
            }

            if (cf == 1)
            {
                var clutBnum = reader.ReadUInt32();
                var clutDx = reader.ReadUInt16();
                var clutDy = reader.ReadUInt16();
                var clutWidth = reader.ReadUInt16();
                var clutHeight = reader.ReadUInt16();
                palette = ReadPalette(reader, pmode, clutWidth, clutHeight, false);
            }
            var imgBnum = reader.ReadUInt32();
            var imgDx = reader.ReadUInt16();
            var imgDy = reader.ReadUInt16();
            var imgWidth = reader.ReadUInt16();
            var imgHeight = reader.ReadUInt16();
            texture = ReadTexture(reader, imgWidth, imgHeight, imgDx, imgDy, pmode, palette, false);
            return texture;
        }

        public static System.Drawing.Color[] ReadPalette(BinaryReader reader, uint pmode, uint clutWidth, uint clutHeight, bool allowOutOfBounds)
        {
            if (clutWidth == 0 || clutHeight == 0 || clutWidth > 256 || clutHeight > 256)
            {
                return null;
            }

            var count = clutWidth * clutHeight;
            System.Drawing.Color[] palette = null;
            // We should probably allocate the full 16clut or 256clut in-case an image pixel has bad data.
            switch (pmode)
            {
                case 0:
                    palette = new System.Drawing.Color[16];
                    break;
                case 1:
                    palette = new System.Drawing.Color[256];
                    break;
            }
            if (palette != null)
            {
                for (var c = 0; c < palette.Length; c++)
                {
                    System.Drawing.Color color;
                    if (c >= count)
                    {
                        // Use default masking black as fallback color.
                        color = System.Drawing.Color.FromArgb(255, 0, 0, 0);
                    }
                    else
                    {
                        // HMD: Support models with invalid image data, but valid model data.
                        if (allowOutOfBounds && reader.BaseStream.Position + 2 > reader.BaseStream.Length)
                            break;

                        var clut = reader.ReadUInt16();
                        var r = (clut & 0x1F);
                        var g = (clut & 0x3E0) >> 5;
                        var b = (clut & 0x7C00) >> 10;
                        var stpBit = ((clut & 0x8000) >> 15) == 1; // Semi-transparency: 0-Off, 1-On
                        // HMD: Semi-transparency
                        // Note: stpMode is defined on a per polygon type basis, so we can't apply this alpha until rendering.
                        //       We could choose to create secondary versions of textures with the semi-transparency alpha applied.
                        var a = 255;
                        //var black = (r == 0 && g == 0 && b == 0);
                        //if (!stpBit && black)
                        //{
                        //    a = 0;
                        //}
                        //else if (stpBit && stpMode)
                        //{
                        //    a = 127;
                        //}

                        color = System.Drawing.Color.FromArgb(a, r * 8, g * 8, b * 8);
                    }
                    palette[c] = color;
                }
            }
            return palette;
        }

        public static Texture ReadTexture(BinaryReader reader, ushort imgWidth, ushort imgHeight, ushort imgDx, ushort imgDy, uint pmode, System.Drawing.Color[] palette, bool allowOutOfBounds)
        {
            Texture texture = null;
            Bitmap bitmap;

            if (imgWidth == 0 || imgHeight == 0 || imgWidth > Program.MaxTIMResolution || imgHeight > Program.MaxTIMResolution)
            {
                return null;
            }

            int texturePage = imgDx / 64;
            if (texturePage > 16)
            {
                return null;
            }

            int textureOffset = texturePage * 64;

            int texturePageY = imgDy / 255;
            if (texturePageY > 2)
            {
                return null;
            }

            int textureOffsetY = texturePageY * 256;

            int finalTexturePage = (texturePageY * 16) + texturePage;

            int textureX;
            int textureY;
            int textureWidth;
            ushort textureHeight;
            int textureBpp;

            switch (pmode)
            {
                case 0: //4bpp
                    textureX = (imgDx - textureOffset) * 4;
                    textureY = (imgDy - textureOffsetY);
                    textureWidth = imgWidth * 4;
                    textureHeight = imgHeight;
                    textureBpp = 4;
                    texture = new Texture(textureWidth, textureHeight, textureX, textureY, textureBpp, finalTexturePage);
                    bitmap = texture.Bitmap;
                    for (var y = 0; y < imgHeight; y++)
                    {
                        for (var x = 0; x < imgWidth; x++)
                        {
                            // HMD: Support models with invalid image data, but valid model data.
                            if (allowOutOfBounds && reader.BaseStream.Position + 2 > reader.BaseStream.Length)
                                break;

                            var color = reader.ReadUInt16();
                            var index1 = (color & 0xF);
                            var index2 = (color & 0xF0) >> 4;
                            var index3 = (color & 0xF00) >> 8;
                            var index4 = (color & 0xF000) >> 12;

                            if (palette == null || index1 >= palette.Length || index2 >= palette.Length || index3 >= palette.Length || index4 >= palette.Length)
                            {
                                return texture;
                            }

                            var color1 = palette[index1];
                            var color2 = palette[index2];
                            var color3 = palette[index3];
                            var color4 = palette[index4];

                            bitmap.SetPixel(x * 4, y, color1);
                            bitmap.SetPixel((x * 4) + 1, y, color2);
                            bitmap.SetPixel((x * 4) + 2, y, color3);
                            bitmap.SetPixel((x * 4) + 3, y, color4);
                        }
                    }

                    break;
                case 1: //8bpp
                    texturePage = imgDx / 64;
                    textureOffset = texturePage * 64;
                    textureX = (imgDx - textureOffset) * 2;
                    textureY = (imgDy - textureOffsetY);
                    textureWidth = imgWidth * 2;
                    textureHeight = imgHeight;
                    textureBpp = 8;
                    texture = new Texture(textureWidth, textureHeight, textureX, textureY, textureBpp, finalTexturePage);
                    bitmap = texture.Bitmap;

                    for (var y = 0; y < imgHeight; y++)
                    {
                        for (var x = 0; x < imgWidth; x++)
                        {
                            // HMD: Support models with invalid image data, but valid model data.
                            if (allowOutOfBounds && reader.BaseStream.Position + 2 > reader.BaseStream.Length)
                                break;

                            var color = reader.ReadUInt16();
                            var index1 = (color & 0xFF);
                            var index2 = (color & 0xFF00) >> 8;

                            if (palette == null || index1 >= palette.Length || index2 >= palette.Length)
                            {
                                return texture;
                            }

                            var color1 = palette[index1];
                            var color2 = palette[index2];

                            bitmap.SetPixel(x * 2, y, color1);
                            bitmap.SetPixel((x * 2) + 1, y, color2);
                        }
                    }

                    break;
                case 2: //16bpp
                    textureX = (imgDx - textureOffset);
                    textureY = (imgDy - textureOffsetY);
                    textureWidth = imgWidth;
                    textureHeight = imgHeight;
                    textureBpp = 16;
                    texture = new Texture(textureWidth, textureHeight, textureX, textureY, textureBpp, finalTexturePage);
                    bitmap = texture.Bitmap;

                    for (var y = 0; y < imgHeight; y++)
                    {
                        for (var x = 0; x < imgWidth; x++)
                        {
                            // HMD: Support models with invalid image data, but valid model data. (HMD has no 2 pmode)
                            if (allowOutOfBounds && reader.BaseStream.Position + 2 > reader.BaseStream.Length)
                                break;

                            var data1 = reader.ReadUInt16();
                            var r0 = (data1 & 0x1F);
                            var g0 = (data1 & 0x3E0) >> 5;
                            var b0 = (data1 & 0x7C00) >> 10;
                            var stpBit = ((data1 & 0x8000) >> 11) == 1; // Likely same semi-transparency bit seen in ReadPalette.

                            var color1 = System.Drawing.Color.FromArgb(255, r0 * 8, g0 * 8, b0 * 8);

                            bitmap.SetPixel(x, y, color1);
                        }
                    }

                    break;
                case 3: //24bpp
                    textureX = (imgDx - textureOffset);
                    textureY = (imgDy - textureOffsetY);
                    textureWidth = imgWidth;
                    textureHeight = imgHeight;
                    textureBpp = 24;
                    texture = new Texture(textureWidth, textureHeight, textureX, textureY, textureBpp, finalTexturePage);
                    bitmap = texture.Bitmap;

                    for (var y = 0; y < imgHeight; y++)
                    {
                        for (var x = 0; x < imgWidth - 1; x++)
                        {
                            // HMD: Support models with invalid image data, but valid model data.
                            if (allowOutOfBounds && reader.BaseStream.Position + 4 > reader.BaseStream.Length)
                                break;

                            var data1 = reader.ReadUInt16();
                            var r0 = (data1 & 0xFF);
                            var g0 = (data1 & 0xFF00) >> 8;

                            var data2 = reader.ReadUInt16();
                            var b0 = (data2 & 0xFF);
                            var r1 = (data2 & 0xFF00) >> 8;

                            var data3 = reader.ReadUInt16();
                            var g1 = (data3 & 0xFF);
                            var b1 = (data3 & 0xFF00) >> 8;

                            var color1 = System.Drawing.Color.FromArgb(255, r0, g0, b0);
                            var color2 = System.Drawing.Color.FromArgb(255, r1, g1, b1);

                            bitmap.SetPixel((x * 2), y, color1);
                            bitmap.SetPixel((x * 2) + 1, y, color2);
                        }
                    }

                    break;
                case 4:
                    break;
            }

            return texture;
        }
    }
}
