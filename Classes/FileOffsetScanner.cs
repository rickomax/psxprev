using System;
using System.Collections.Generic;
using System.IO;

namespace PSXPrev.Classes
{
    public delegate void EntityAddedAction(RootEntity rootEntity, long offset);
    public delegate void AnimationAddedAction(Animation animation, long offset);
    public delegate void TextureAddedAction(Texture texture, long offset);

    public abstract class FileOffsetScanner
    {
        protected long _offset;
        private readonly EntityAddedAction _entityAddedAction;
        private readonly AnimationAddedAction _animationAddedAction;
        private readonly TextureAddedAction _textureAddedAction;

        public FileOffsetScanner(EntityAddedAction entityAdded = null, AnimationAddedAction animationAdded = null, TextureAddedAction textureAdded = null)
        {
            _entityAddedAction = entityAdded;
            _animationAddedAction = animationAdded;
            _textureAddedAction = textureAdded;
        }

        public void ScanFile(BinaryReader reader, string fileTitle)
        {
            if (reader == null)
            {
                throw new Exception("File must be opened");
            }

            Program.Logger.WriteLine($"Scanning for {FormatName} at file {fileTitle}");


            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            
            while (reader.BaseStream.CanRead)
            {
                var passed = false;
                try
                {
                    _offset = reader.BaseStream.Position;

                    Parse(reader, fileTitle, out var entities, out var animations, out var textures);

                    var offsetPostfix = (_offset > 0 ? $"_{_offset:X}" : string.Empty);
                    var name = $"{fileTitle}{offsetPostfix}";
                        
                    if (entities != null)
                    {
                        foreach (var entity in entities)
                        {
                            if (entity == null)
                            {
                                continue;
                            }
                            passed = true;
                            entity.EntityName = name;
                            _entityAddedAction(entity, _offset);

                            Program.Logger.WritePositiveLine($"Found {FormatName} Model at offset {_offset:X}");
                        }
                    }
                    if (animations != null)
                    {
                        foreach (var animation in animations)
                        {
                            if (animation == null)
                            {
                                continue;
                            }
                            passed = true;
                            animation.AnimationName = name;
                            _animationAddedAction(animation, _offset);

                            Program.Logger.WritePositiveLine($"Found {FormatName} Animation at offset {_offset:X}");
                        }
                    }
                    if (textures != null)
                    {
                        foreach (var texture in textures)
                        {
                            if (texture == null)
                            {
                                continue;
                            }
                            passed = true;
                            texture.TextureName = name;
                            _textureAddedAction(texture, _offset);

                            Program.Logger.WritePositiveLine($"Found {FormatName} Image at offset {_offset:X}");
                        }
                    }
                }
                catch (Exception exp)
                {
                    //if (Program.Debug)
                    //{
                    //    Program.Logger.WriteErrorLine(exp);
                    //}
                }

                if (!passed)
                {
                    if (Program.NoOffset || ++_offset > reader.BaseStream.Length)
                    {
                        Program.Logger.WriteLine($"{FormatName} - Reached file end: {fileTitle}");
                        return;
                    }
                    reader.BaseStream.Seek(_offset, SeekOrigin.Begin);
                }
            }
        }

        public abstract string FormatName { get; }

        protected abstract void Parse(BinaryReader reader, string fileTitle, out List<RootEntity> entities, out List<Animation> animations, out List<Texture> textures);
    }
}
