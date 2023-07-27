using System;
using System.Collections.Generic;
using System.IO;
using PSXPrev.Common.Animator;

namespace PSXPrev.Common.Parsers
{
    public delegate void EntityAddedAction(RootEntity rootEntity, long offset);
    public delegate void AnimationAddedAction(Animation animation, long offset);
    public delegate void TextureAddedAction(Texture texture, long offset);

    public abstract class FileOffsetScanner : IDisposable
    {
        private readonly EntityAddedAction _entityAddedAction;
        private readonly AnimationAddedAction _animationAddedAction;
        private readonly TextureAddedAction _textureAddedAction;

        protected long _offset;

        public long? StartOffset { get; set; }
        public long? StopOffset { get; set; }
        // Next stream offset will be at the end of the last-read file.
        public bool NextOffset { get; set; }

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

            Program.Logger.WriteLine($"Scanning for {FormatName} at file: {fileTitle}");

            // Assume this is a read-only stream, and that the length won't change.
            _offset = StartOffset ?? 0;
            var length = Math.Max(_offset + 1, Math.Min(reader.BaseStream.Length, StopOffset ?? long.MaxValue));


            while (reader.BaseStream.CanRead && _offset < length)
            {
                if (Program.WaitOnScanState())
                {
                    break; // Canceled
                }

                reader.BaseStream.Seek(_offset, SeekOrigin.Begin);

                var passed = false;
                try
                {
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

                            Program.Logger.WritePositiveLine($"Found {FormatName} Model {AtOffsetString}");
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

                            Program.Logger.WritePositiveLine($"Found {FormatName} Animation {AtOffsetString}");
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

                            Program.Logger.WritePositiveLine($"Found {FormatName} Texture {AtOffsetString}");
                        }
                    }
                }
                catch (Exception exp)
                {
                    if (Program.ShowErrors)
                    {
                        Program.Logger.WriteExceptionLine(exp, $"Error scanning {FormatName} {AtOffsetString}");
                    }
                }

                _offset++; // Always increment by at least one.
                if (passed && NextOffset)
                {
                    var endPosition = reader.BaseStream.Position;
                    // Don't use this for now, because reading rogue matching data
                    // can cause large jumps in offset that skip real files.
                    //if (reader.BaseStream is FileOffsetStream fileOffsetStream)
                    //{
                    //    endPosition = fileOffsetStream.FarthestPosition;
                    //}
                    _offset = Math.Max(_offset, endPosition);
                }
            }

            Program.Logger.WriteLine($"{FormatName} - Reached file end: {fileTitle}");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }


        protected string AtOffsetString => $"at offset {_offset:X}";

        public abstract string FormatName { get; }

        protected abstract void Parse(BinaryReader reader, string fileTitle, out List<RootEntity> entities, out List<Animation> animations, out List<Texture> textures);
    }
}
