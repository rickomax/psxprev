using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using PSXPrev.Common.Animator;

namespace PSXPrev.Common.Parsers
{
    public class TIMParser : FileOffsetScanner
    {
        public TIMParser(TextureAddedAction textureAdded)
            : base(textureAdded: textureAdded)
        {
        }

        public override string FormatName => "TIM";

        protected override void Parse(BinaryReader reader, string fileTitle, out List<RootEntity> entities, out List<Animation> animations, out List<Texture> textures)
        {
            entities = null;
            animations = null;
            textures = null;

            var id = reader.ReadUInt16();
            if (id == 0x10)
            {
                var version = reader.ReadUInt16();
                if (Program.IgnoreTimVersion || version == 0x00)
                {
                    var texture = ParseTim(reader);
                    if (texture != null)
                    {
                        textures = new List<Texture> { texture };
                    }
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

            bool[] semiTransparentPalette = null;
            if (cf == 1)
            {
                var clutBnum = reader.ReadUInt32();
                var clutDx = reader.ReadUInt16();
                var clutDy = reader.ReadUInt16();
                var clutWidth = reader.ReadUInt16();
                var clutHeight = reader.ReadUInt16();
                palette = ReadPalette(reader, pmode, clutWidth, clutHeight, out semiTransparentPalette, false);
            }
            var imgBnum = reader.ReadUInt32();
            var imgDx = reader.ReadUInt16();
            var imgDy = reader.ReadUInt16();
            var imgWidth = reader.ReadUInt16();
            var imgHeight = reader.ReadUInt16();
            texture = ReadTexture(reader, imgWidth, imgHeight, imgDx, imgDy, pmode, palette, semiTransparentPalette, false);
            return texture;
        }

        public static System.Drawing.Color[] ReadPalette(BinaryReader reader, uint pmode, uint clutWidth, uint clutHeight, out bool[] semiTransparentPalette, bool allowOutOfBounds)
        {
            semiTransparentPalette = null;

            if (clutWidth == 0 || clutHeight == 0 || clutWidth > 256 || clutHeight > 256)
            {
                return null;
            }

            // HMD: Support models with invalid image data, but valid model data.
            if (allowOutOfBounds && (clutWidth * clutHeight * 2) + reader.BaseStream.Position > reader.BaseStream.Length)
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
                        var clut = reader.ReadUInt16();
                        var r = (clut & 0x1F);
                        var g = (clut & 0x3E0) >> 5;
                        var b = (clut & 0x7C00) >> 10;
                        var stpBit = ((clut & 0x8000) >> 15) == 1; // Semi-transparency: 0-Off, 1-On
                        var a = 255;

                        // Note: stpMode (not stpBit) is defined on a per polygon basis. We can't apply alpha now, only during rendering.
                        if (stpBit)
                        {
                            if (semiTransparentPalette == null)
                            {
                                semiTransparentPalette = new bool[palette.Length];
                            }
                            semiTransparentPalette[c] = true;
                        }
                        else if (r == 0 && g == 0 && b == 0)
                        {
                            a = 0; // Transparent when black and !stpBit
                        }

                        color = System.Drawing.Color.FromArgb(a, r * 8, g * 8, b * 8);
                    }
                    palette[c] = color;
                }
            }
            return palette;
        }

        public static Texture ReadTexture(BinaryReader reader, ushort imgWidth, ushort imgHeight, ushort imgDx, ushort imgDy, uint pmode, System.Drawing.Color[] palette, bool[] semiTransparentPalette, bool allowOutOfBounds)
        {
            Texture texture = null;
            Bitmap bitmap = null;
            Bitmap semiTransparentMap = null;

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

                    // HMD: Support models with invalid image data, but valid model data.
                    if (allowOutOfBounds && (textureWidth * textureHeight / 2) + reader.BaseStream.Position > reader.BaseStream.Length)
                    {
                        break;
                    }
                    if (palette == null)
                    {
                        break;
                    }

                    texture = new Texture(textureWidth, textureHeight, textureX, textureY, textureBpp, finalTexturePage);
                    bitmap = texture.Bitmap;
                    if (semiTransparentPalette != null)
                    {
                        semiTransparentMap = texture.SetupSemiTransparentMap();
                    }

                    for (var y = 0; y < imgHeight; y++)
                    {
                        for (var x = 0; x < imgWidth; x++)
                        {
                            var data1 = reader.ReadUInt16();
                            var index1 = (data1 & 0xF);
                            var index2 = (data1 & 0xF0) >> 4;
                            var index3 = (data1 & 0xF00) >> 8;
                            var index4 = (data1 & 0xF000) >> 12;

                            if (palette == null || index1 >= palette.Length || index2 >= palette.Length || index3 >= palette.Length || index4 >= palette.Length)
                            {
                                return texture;
                            }

                            var color1 = palette[index1];
                            var color2 = palette[index2];
                            var color3 = palette[index3];
                            var color4 = palette[index4];

                            bitmap.SetPixel((x * 4) + 0, y, color1);
                            bitmap.SetPixel((x * 4) + 1, y, color2);
                            bitmap.SetPixel((x * 4) + 2, y, color3);
                            bitmap.SetPixel((x * 4) + 3, y, color4);

                            if (semiTransparentPalette != null)
                            {
                                if (semiTransparentPalette[index1])
                                {
                                    semiTransparentMap.SetPixel((x * 4) + 0, y, Texture.SemiTransparentFlag);
                                }
                                if (semiTransparentPalette[index2])
                                {
                                    semiTransparentMap.SetPixel((x * 4) + 1, y, Texture.SemiTransparentFlag);
                                }
                                if (semiTransparentPalette[index3])
                                {
                                    semiTransparentMap.SetPixel((x * 4) + 2, y, Texture.SemiTransparentFlag);
                                }
                                if (semiTransparentPalette[index4])
                                {
                                    semiTransparentMap.SetPixel((x * 4) + 3, y, Texture.SemiTransparentFlag);
                                }
                            }
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

                    // HMD: Support models with invalid image data, but valid model data.
                    if (allowOutOfBounds && (textureWidth * textureHeight) + reader.BaseStream.Position > reader.BaseStream.Length)
                    {
                        break;
                    }
                    if (palette == null)
                    {
                        break;
                    }

                    texture = new Texture(textureWidth, textureHeight, textureX, textureY, textureBpp, finalTexturePage);
                    bitmap = texture.Bitmap;
                    if (semiTransparentPalette != null)
                    {
                        semiTransparentMap = texture.SetupSemiTransparentMap();
                    }

                    for (var y = 0; y < imgHeight; y++)
                    {
                        for (var x = 0; x < imgWidth; x++)
                        {
                            var data1 = reader.ReadUInt16();
                            var index1 = (data1 & 0xFF);
                            var index2 = (data1 & 0xFF00) >> 8;

                            if (palette == null || index1 >= palette.Length || index2 >= palette.Length)
                            {
                                return texture;
                            }

                            var color1 = palette[index1];
                            var color2 = palette[index2];

                            bitmap.SetPixel((x * 2) + 0, y, color1);
                            bitmap.SetPixel((x * 2) + 1, y, color2);

                            if (semiTransparentPalette != null)
                            {
                                if (semiTransparentPalette[index1])
                                {
                                    semiTransparentMap.SetPixel((x * 2) + 0, y, Texture.SemiTransparentFlag);
                                }
                                if (semiTransparentPalette[index2])
                                {
                                    semiTransparentMap.SetPixel((x * 2) + 1, y, Texture.SemiTransparentFlag);
                                }
                            }
                        }
                    }

                    break;
                case 2: //16bpp
                    textureX = (imgDx - textureOffset);
                    textureY = (imgDy - textureOffsetY);
                    textureWidth = imgWidth;
                    textureHeight = imgHeight;
                    textureBpp = 16;

                    // HMD: Support models with invalid image data, but valid model data.
                    if (allowOutOfBounds && (textureWidth * textureHeight * 2) + reader.BaseStream.Position > reader.BaseStream.Length)
                    {
                        break;
                    }

                    texture = new Texture(textureWidth, textureHeight, textureX, textureY, textureBpp, finalTexturePage);
                    bitmap = texture.Bitmap;

                    for (var y = 0; y < imgHeight; y++)
                    {
                        for (var x = 0; x < imgWidth; x++)
                        {
                            var data1 = reader.ReadUInt16();
                            var r0 = (data1 & 0x1F);
                            var g0 = (data1 & 0x3E0) >> 5;
                            var b0 = (data1 & 0x7C00) >> 10;
                            var stpBit = ((data1 & 0x8000) >> 15) == 1; // Semi-transparency: 0-Off, 1-On
                            var a0 = 255;

                            // Note: stpMode (not stpBit) is defined on a per polygon basis. We can't apply alpha now, only during rendering.
                            if (stpBit)
                            {
                                if (semiTransparentMap == null)
                                {
                                    semiTransparentMap = texture.SetupSemiTransparentMap();
                                }
                                semiTransparentMap.SetPixel(x, y, Texture.SemiTransparentFlag);
                            }
                            else if (r0 == 0 && g0 == 0 && b0 == 0)
                            {
                                a0 = 0; // Transparent when black and !stpBit
                            }

                            var color1 = System.Drawing.Color.FromArgb(a0, r0 * 8, g0 * 8, b0 * 8);

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

                    if (imgWidth % 2 != 0)
                    {
                        if (Program.Debug)
                        {
                            Program.Logger.WriteLine("24bpp texture has odd-numbered width, unsure how to handle row padding.");
                        }
                    }
                    // HMD: Support models with invalid image data, but valid model data.
                    if (allowOutOfBounds && (textureWidth * textureHeight * 3) + reader.BaseStream.Position > reader.BaseStream.Length)
                    {
                        break;
                    }

                    texture = new Texture(textureWidth, textureHeight, textureX, textureY, textureBpp, finalTexturePage);
                    bitmap = texture.Bitmap;

                    for (var y = 0; y < imgHeight; y++)
                    {
                        for (var x = 0; x < imgWidth; x++)
                        {
                            var r0 = reader.ReadByte();
                            var g0 = reader.ReadByte();
                            var b0 = reader.ReadByte();

                            var color1 = System.Drawing.Color.FromArgb(255, r0, g0, b0);

                            bitmap.SetPixel(x, y, color1);
                        }
                        // todo: Is there padding at the end of rows?
                        //       It's probably padding to 2-bytes if there is any, rather than 4-bytes.
                    }

                    break;
                case 4:
                    break;
            }

            return texture;
        }
    }
}
