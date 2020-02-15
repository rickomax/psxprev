namespace PSXPrev.Classes
{
   public class PXLParser {
   // {
   //     private readonly Logger _logger;
   //
   //     public PXLParser(Logger logger)
   //     {
   //         _logger = logger;
   //     }
   //
   //     public Texture[] LookForPxl(BinaryReader reader)
   //     {
   //         if (reader == null)
   //         {
   //             throw (new Exception("File must be opened"));
   //         }
   //
   //         reader.BaseStream.Seek(0, SeekOrigin.Begin);
   //
   //         var bitmaps = new List<Texture>();
   //
   //         long checkOffset = 0;
   //
   //         while (reader.BaseStream.CanRead)
   //         {
   //             try
   //             {
   //                 checkOffset = reader.BaseStream.Position;
   //                 var header = reader.ReadUInt32();
   //                 var id = header & 0xFF;
   //                 if (id == 0x11)
   //                 {
   //                     var version = (header & 0xFF00) >> 8;
   //                     if (version == 0x00)
   //                     {
   //                         Texture bitmap = ParsePxl(reader);
   //                         if (bitmap != null)
   //                         {
   //                             bitmaps.Add(bitmap);
   //                             _Program.Logger.WriteLine("Found PXL Image at offset {0:X}", checkOffset);
   //                         }
   //                     }
   //                 }
   //             }
   //             catch (Exception exp)
   //             {
   //                 if (exp is EndOfStreamException)
   //                 {
   //                     break;
   //                 }
   //                 _Program.Logger.WriteLine(exp);
   //             }
   //             reader.BaseStream.Seek(checkOffset + 1, SeekOrigin.Begin);
   //         }
   //
   //         return bitmaps.ToArray();
   //     }
   //
   //     private Texture ParsePxl(BinaryReader reader)
   //     {
   //         Texture bitmap = null;
   //
   //         var pmode = reader.ReadUInt32();
   //         if (pmode > 1)
   //         {
   //             return null;
   //         }
   //
   //         var imgBnum = reader.ReadUInt32();
   //         var imgDx = reader.ReadUInt16();
   //         var imgDy = reader.ReadUInt16();
   //         var imgWidth = reader.ReadUInt16();
   //         var imgHeight = reader.ReadUInt16();
   //
   //         if (imgWidth == 0 || imgHeight == 0 || imgWidth > 2048 || imgHeight > 2048)
   //         {
   //             return null;
   //         }
   //
   //         switch (pmode)
   //         {
   //             case 0: // 4-bit CLUT
   //                 bitmap = new Texture(imgWidth*4, imgHeight);
   //
   //                 for (var y = 0; y < imgHeight; y++)
   //                 {
   //                     for (var x = 0; x < imgWidth; x++)
   //                     {
   //                         var color = reader.ReadUInt16();
   //                         var index1 = (color & 0xF);
   //                         var index2 = (color & 0xF0) >> 4;
   //                         var index3 = (color & 0xF00) >> 8;
   //                         var index4 = (color & 0xF000) >> 12;
   //
   //                         var color1 = System.Drawing.Color.FromArgb(index1, index1, index1);
   //                         var color2 = System.Drawing.Color.FromArgb(index2, index2, index2);
   //                         var color3 = System.Drawing.Color.FromArgb(index3, index3, index3);
   //                         var color4 = System.Drawing.Color.FromArgb(index4, index4, index4);
   //
   //                         bitmap.SetPixel(x*4, y, color1);
   //                         bitmap.SetPixel((x*4) + 1, y, color2);
   //                         bitmap.SetPixel((x*4) + 2, y, color3);
   //                         bitmap.SetPixel((x*4) + 3, y, color4);
   //                     }
   //                 }
   //                 break;
   //             case 1: // 8-bit CLUT
   //                 bitmap = new Texture(imgWidth*8, imgHeight);
   //
   //                 for (var y = 0; y < imgHeight; y++)
   //                 {
   //                     for (var x = 0; x < imgWidth; x++)
   //                     {
   //                         var color = reader.ReadUInt16();
   //                         var index1 = (color & 0xFF);
   //                         var index2 = (color & 0xFF00) >> 8;
   //
   //                         var color1 = System.Drawing.Color.FromArgb(index1, index1, index1);
   //                         var color2 = System.Drawing.Color.FromArgb(index2, index2, index2);
   //
   //                         bitmap.SetPixel(x*2, y, color1);
   //                         bitmap.SetPixel((x*2) + 1, y, color2);
   //                     }
   //                 }
   //                 break;
   //         }
   //
   //         return bitmap;
   //     }
    }
}  
