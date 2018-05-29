using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;


namespace PSXPrev
{
    public class TIMParser
    {
        private long _offset;
        private Action<Texture, long> entityAddedAction;

        public TIMParser(Action<Texture, long> entityAdded)
        {
            entityAddedAction = entityAdded;
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
                try
                {
                    _offset = reader.BaseStream.Position;
                    var id = reader.ReadUInt32();
                    if (id == 0x10)
                    {
                        var foundTextures = ParseRETim(reader);
                        if (foundTextures != null)
                        {
                            for (var t = 0; t < foundTextures.Length; t++)
                            {
                                var texture = foundTextures[t];
                                texture.TextureName = string.Format("{0}{1:x}", fileTitle, _offset > 0 ? "_" + _offset + "_" + t : string.Empty);
                                texture.TexturePage = t;
                                //textures.Add(texture);
                                entityAddedAction(texture, reader.BaseStream.Position);
                                Program.Logger.WriteLine("Found TIM Image at offset {0:X}", _offset);
                            }
                        }
                    }
                }
                catch (Exception exp)
                {
                    if (exp is EndOfStreamException)
                    {
                        break;
                    }
                    Program.Logger.WriteLine(exp);
                }
                reader.BaseStream.Seek(_offset + 1, SeekOrigin.Begin);
            }
        }

        private Texture[] ParseRETim(BinaryReader reader)
        {
            //Texture texture;
            //Bitmap bitmap;

            var flag = reader.ReadUInt32();
            var offset = reader.ReadUInt32();
            var clutDx = reader.ReadUInt16();
            var clutDy = reader.ReadUInt16();
            var colorCount = reader.ReadUInt16();
            if (colorCount > 256)
            {
                return null;
            }
            var paletteCount = reader.ReadUInt16();
            if (paletteCount > 256)
            {
                return null;
            }

            var palettes = new System.Drawing.Color[paletteCount][];
            var numTextures = paletteCount == 0 ? 1 : paletteCount;
            var textures = new Texture[numTextures];
            //var usingPalette = new System.Drawing.Color[] { };

            if (flag == 0x08 || flag == 0x09)
            {
                for (var p = 0; p < paletteCount; p++)
                {
                    System.Drawing.Color[] palette;
                    if (flag == 0x08)
                    {
                        // 4-bit CLUT
                        palette = new System.Drawing.Color[colorCount];
                        for (var c = 0; c < colorCount; c++)
                        {
                            var clut = reader.ReadUInt16();
                            var r = (clut & 0x1F);
                            var g = (clut & 0x3E0) >> 5;
                            var b = (clut & 0x7C00) >> 10;
                            var a = (clut & 0x8000) >> 15;
                            var color = System.Drawing.Color.FromArgb(255, r * 8, g * 8, b * 8);
                            palette[c] = color;
                        }
                    }
                    else
                    { // 8-bit CLUT
                        palette = new System.Drawing.Color[colorCount];
                        for (var c = 0; c < colorCount; c++)
                        {
                            var clut = reader.ReadUInt16();
                            var r = (clut & 0x1F);
                            var g = (clut & 0x3E0) >> 5;
                            var b = (clut & 0x7C00) >> 10;
                            var a = (clut & 0x8000) >> 15;
                            var color = System.Drawing.Color.FromArgb(255, r * 8, g * 8, b * 8);
                            palette[c] = color;
                        }
                    }
                    palettes[p] = palette;
                }
            }
            //else if (flag != 0x02)
            //{
            //    return null;
            //}

            reader.BaseStream.Position = _offset + offset + 16;

            var imgWidth = reader.ReadUInt16();
            var imgHeight = reader.ReadUInt16();

            if (imgWidth == 0 || imgHeight == 0 || imgWidth > 2000 || imgHeight > 2000)
            {
                return null;
            }

            //reader.BaseStream.Position = _offset + offset;

            int textureWidth;
            ushort textureHeight;
            int textureBpp;
            int textureDivision;
            int textureOffset;
            int xOffset;
            Bitmap bitmap;

            if (flag == 0x08)
            {
                //4bpp
                textureWidth = imgWidth * 4;
                textureHeight = imgHeight;
                textureBpp = 4;
                for (var i = 0; i < numTextures; i++)
                {
                    textures[i] = new Texture(textureWidth / numTextures, textureHeight, 0, 0, textureBpp, 0);
                }
                textureDivision = imgWidth / paletteCount;

                for (var y = 0; y < imgHeight; y++)
                {
                    for (var x = 0; x < imgWidth; x++)
                    {
                        var paletteOffset = x > 0 ? x / textureDivision : 0;
                        var color = reader.ReadUInt16();
                        var index1 = (color & 0xF);
                        var index2 = (color & 0xF0) >> 4;
                        var index3 = (color & 0xF00) >> 8;
                        var index4 = (color & 0xF000) >> 12;

                        if (index1 >= palettes[paletteOffset].Length || index2 >= palettes[paletteOffset].Length ||
                            index3 >= palettes[paletteOffset].Length || index4 >= palettes[paletteOffset].Length)
                        {
                            return null;
                        }

                        var color1 = palettes[paletteOffset][index1];
                        var color2 = palettes[paletteOffset][index2];
                        var color3 = palettes[paletteOffset][index3];
                        var color4 = palettes[paletteOffset][index4];

                        bitmap = textures[paletteOffset].Bitmap;
                        textureOffset = paletteOffset*textureDivision*4;
                        xOffset = x*4;

                        bitmap.SetPixel(xOffset - textureOffset, y, color1);
                        bitmap.SetPixel(xOffset + 1 - textureOffset, y, color2);
                        bitmap.SetPixel(xOffset + 2 - textureOffset, y, color3);
                        bitmap.SetPixel(xOffset + 3 - textureOffset, y, color4);
                    }
                }
            }
            else if (flag == 0x09)
            {
                //8bpp 
                textureWidth = imgWidth * 2;
                textureHeight = imgHeight;
                textureBpp = 8;
                for (var i = 0; i < numTextures; i++)
                {
                    textures[i] = new Texture(textureWidth / numTextures, textureHeight, 0, 0, textureBpp, 0);
                }
                textureDivision = imgWidth / paletteCount;

                for (var y = 0; y < imgHeight; y++)
                {
                    for (var x = 0; x < imgWidth; x++)
                    {
                        var paletteOffset = x > 0 ? x / textureDivision : 0;
                        var color = reader.ReadUInt16();
                        var index1 = (color & 0xFF);
                        var index2 = (color & 0xFF00) >> 8;

                        if (index1 >= palettes[paletteOffset].Length || index2 >= palettes[paletteOffset].Length)
                        {
                            return null;
                        }

                        var color1 = palettes[paletteOffset][index1];
                        var color2 = palettes[paletteOffset][index2];

                        bitmap = textures[paletteOffset].Bitmap;
                        textureOffset = paletteOffset * textureDivision * 2;
                        xOffset = x * 2;

                        bitmap.SetPixel(xOffset - textureOffset, y, color1);
                        bitmap.SetPixel(xOffset + 1 - textureOffset, y, color2);
                    }
                }
            }
            else
            {
                //16bpp
                textureWidth = imgWidth;
                textureHeight = imgHeight;
                textureBpp = 16;
                textures[0] = new Texture(textureWidth, textureHeight, 0, 0, textureBpp, 0);
                bitmap = textures[0].Bitmap;

                for (var y = 0; y < imgHeight; y++)
                {
                    for (var x = 0; x < imgWidth; x++)
                    {
                        var data1 = reader.ReadUInt16();
                        var r0 = (data1 & 0x1F);
                        var g0 = (data1 & 0x3E0) >> 5;
                        var b0 = (data1 & 0x7C00) >> 10;
                        var a0 = (data1 & 0x8000) >> 11;

                        var color1 = System.Drawing.Color.FromArgb(255, r0 * 8, g0 * 8, b0 * 8);

                        bitmap.SetPixel(x, y, color1);
                    }
                }
            }

            return textures;
        }
    }
}
