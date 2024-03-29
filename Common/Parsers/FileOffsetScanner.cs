﻿using System;
using System.Collections.Generic;
using System.IO;
using PSXPrev.Common.Animator;

namespace PSXPrev.Common.Parsers
{
    // Return false if the result is ignored, in which case it will be disposed of
    public delegate bool EntityAddedAction(FileOffsetScanner scanner, RootEntity rootEntity, long offset);
    public delegate bool TextureAddedAction(FileOffsetScanner scanner, Texture texture, long offset);
    public delegate bool AnimationAddedAction(FileOffsetScanner scanner, Animation animation, long offset);
    public delegate void ScannerProgressAction(FileOffsetScanner scanner, long offset);

    public abstract class FileOffsetScanner : IDisposable
    {
        private readonly EntityAddedAction _entityAddedAction;
        private readonly TextureAddedAction _textureAddedAction;
        private readonly AnimationAddedAction _animationAddedAction;

        protected long _offset;
        protected BinaryReader _reader;
        protected string _fileTitle;

        // Result lists that shoud be added to during Parse.
        protected List<RootEntity> EntityResults { get; } = new List<RootEntity>();
        protected List<Texture> TextureResults { get; } = new List<Texture>();
        protected List<Animation> AnimationResults { get; } = new List<Animation>();

        protected long MinOffsetIncrement { get; set; } = 1;

        public long? StartOffset { get; set; }
        public long? StopOffset { get; set; }
        // Next stream offset will be at the end of the last-read file.
        public bool NextOffset { get; set; }
        public long Alignment { get; set; }

        public ScannerProgressAction ProgressCallback { get; set; }
        public long BytesPerProgress { get; set; }

        public FileOffsetScanner(EntityAddedAction entityAdded = null, TextureAddedAction textureAdded = null, AnimationAddedAction animationAdded = null)
        {
            _entityAddedAction = entityAdded;
            _textureAddedAction = textureAdded;
            _animationAddedAction = animationAdded;
        }

        public void ScanFile(BinaryReader reader, string fileTitle)
        {
            _reader = reader ?? throw new Exception("File must be opened");
            _fileTitle = fileTitle;

            Program.Logger.WriteLine($"Scanning for {FormatName} at file: {fileTitle}");

            // Assume this is a read-only stream, and that the length won't change.
            var start = Math.Max(0, StartOffset ?? 0);
            var lastProgress = start;
            _offset = start;
            // Ensure stop offset is always at least StartOffset + 1.
            var length = Math.Min(Math.Max((StopOffset ?? long.MaxValue), (_offset + 1)), reader.BaseStream.Length);
            var alignment = Math.Max(MinAlignment, Alignment);


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
                    EntityResults.Clear();
                    TextureResults.Clear();
                    AnimationResults.Clear();
                    MinOffsetIncrement = 1;

                    Parse(reader);

                    if (EntityResults.Count > 0 || TextureResults.Count > 0 || AnimationResults.Count > 0)
                    {
                        var offsetPostfix = (_offset > 0 ? $"_{_offset:X}" : string.Empty);
                        var name = $"{fileTitle}{offsetPostfix}";

                        var resultIndex = 0;
                        foreach (var entity in EntityResults)
                        {
                            if (entity == null)
                            {
                                continue;
                            }
                            passed = true;
                            entity.Name = name;
                            entity.FormatName = CombineFormatName(FormatName, entity.FormatName);
                            entity.FileOffset = _offset;
                            entity.ResultIndex = resultIndex++;
                            if (_entityAddedAction(this, entity, _offset))
                            {
                                Program.Logger.WritePositiveLine($"Found {FormatName} Model {AtOffsetString}");
                            }
                        }

                        // Enumerate textures differently so that we know whether to dispose of them or not.
                        resultIndex = 0;
                        while (TextureResults.Count > 0)
                        {
                            var texture = TextureResults[0]; // Pop (but remove later on)

                            if (texture == null)
                            {
                                TextureResults.RemoveAt(0);
                                continue;
                            }
                            passed = true;
                            texture.Name = name;
                            texture.FormatName = CombineFormatName(FormatName, texture.FormatName);
                            texture.FileOffset = _offset;
                            texture.ResultIndex = resultIndex++;
                            if (_textureAddedAction(this, texture, _offset))
                            {
                                Program.Logger.WritePositiveLine($"Found {FormatName} Texture {AtOffsetString}");
                            }
                            else
                            {
                                texture.Dispose();
                                foreach (var entity in EntityResults)
                                {
                                    entity.OwnedTextures.Remove(texture);
                                }
                            }

                            TextureResults.RemoveAt(0); // We should no longer dispose of this during an exception
                        }

                        resultIndex = 0;
                        foreach (var animation in AnimationResults)
                        {
                            if (animation == null)
                            {
                                continue;
                            }
                            passed = true;
                            animation.Name = name;
                            animation.FormatName = CombineFormatName(FormatName, animation.FormatName);
                            animation.FileOffset = _offset;
                            animation.ResultIndex = resultIndex++;
                            if (_animationAddedAction(this, animation, _offset))
                            {
                                Program.Logger.WritePositiveLine($"Found {FormatName} Animation {AtOffsetString}");
                            }
                        }
                    }
                }
                catch (Exception exp)
                {
                    MinOffsetIncrement = 1; // Change this back on error

                    if (Program.ShowErrors)
                    {
                        Program.Logger.WriteExceptionLine(exp, $"Error scanning {FormatName} {AtOffsetString}");
                    }
                    // todo: Maybe we want to support keeping textures even if we failed to read the rest of the file?
                    // Make sure to dispose of textures that we never got to add.
                    foreach (var leakedTexture in TextureResults)
                    {
                        leakedTexture.Dispose();
                        // Remove owned leaked textures from root entities.
                        // This is important in-case the entity was successfully added.
                        foreach (var entity in EntityResults)
                        {
                            entity.OwnedTextures.Remove(leakedTexture);
                        }
                    }
                }

                _offset += Math.Max(1, MinOffsetIncrement); // Always increment by at least one.
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
                if (alignment > 1)
                {
                    // todo: Should we align based on start offset?
                    //_offset = start + ((_offset - start) + alignment - 1) / alignment * alignment;
                    _offset = (_offset + alignment - 1) / alignment * alignment;
                }
                if (BytesPerProgress > 0 && (_offset - lastProgress) >= BytesPerProgress)
                {
                    // todo: Should we align progress? Or maybe exclude bytes skipped by endPosition...?
                    lastProgress = _offset;
                    //lastProgress = _offset / BytesPerProgress * BytesPerProgress;
                    ProgressCallback?.Invoke(this, _offset);
                }
            }

            Program.Logger.WriteLine($"{FormatName} - Reached file end: {fileTitle}");

            _reader = null;
            _fileTitle = null;
            EntityResults.Clear();
            TextureResults.Clear();
            AnimationResults.Clear();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }


        public string AtOffsetString => $"at offset {_offset:X}";

        public abstract string FormatName { get; }

        public virtual long MinAlignment => 1;

        // Parse the file format at the given offset. Results should be added to
        // the following lists: EntityResults, TextureResults, AnimationResults.
        protected abstract void Parse(BinaryReader reader);


        protected static string AbsoluteFormatName(string formatName)
        {
            if (!string.IsNullOrEmpty(formatName) && formatName[0] == '/')
            {
                return formatName; // Already prefixed with a forward slash
            }
            return $"/{formatName}";
        }

        protected static string CombineFormatName(string formatName, string subFormatName)
        {
            if (!string.IsNullOrEmpty(subFormatName))
            {
                if (subFormatName[0] == '/')
                {
                    // Prefix with a forward slash to override the format name completely
                    return subFormatName.Substring(1);
                }
                else
                {
                    // BaseFormat/SubFormat
                    return $"{formatName}/{subFormatName}";
                }
            }
            return formatName;
        }
    }
}
