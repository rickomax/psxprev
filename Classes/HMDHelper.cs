using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PSXPrev.Classes
{
    public static class HMDHelper
    {
        public static bool TryGetAnimInterpolationType(uint interpAlgo, out InterpolationType interpType)
        {
            switch (interpAlgo)
            {
                case  0: // None
                    interpType = InterpolationType.None;
                    return true;
                case  1: // Linear
                case  9: // Linear(*)
                    interpType = InterpolationType.Linear;
                    return true;
                case  2: // Bezier Curve
                case 10: // Bezier Curve(*)
                    //interpType = InterpolationType.Bezier;
                    //return true;
                    break; // Not supported yet
                case  3: // B-Spline
                case 11: // B-Spline(*)
                    //interpType = InterpolationType.BSpline;
                    //return true;
                    break; // Not supported yet
                case  4: // Beta-Spline
                    //interpType = InterpolationType.BetaSpline;
                    //return true;
                    break; // Not supported yet
            }
            interpType = InterpolationType.None;
            return false;
        }

        public static bool TryGetAnimRotationOrder(uint rotOrder, out RotationOrder rotOrderType)
        {
            switch (rotOrder)
            {
                case 0: // XYZ
                    rotOrderType = RotationOrder.XYZ;
                    return true;
                case 1: // XYZ
                    rotOrderType = RotationOrder.XZY;
                    return true;
                case 2: // YXZ
                    rotOrderType = RotationOrder.YXZ;
                    return true;
                case 3: // YZX
                    rotOrderType = RotationOrder.YZX;
                    return true;
                case 4: // ZXY
                    rotOrderType = RotationOrder.ZXY;
                    return true;
                case 5: // ZYX
                    rotOrderType = RotationOrder.ZYX;
                    return true;
            }
            rotOrderType = RotationOrder.None;
            return false;
        }

        private static Vector3[] AppendToCurve(Vector3[] curve, Vector3 v)
        {
            if (curve == null)
            {
                return new Vector3[1] { v }; // First element in curve.
            }
            Array.Resize(ref curve, curve.Length + 1);
            curve[curve.Length - 1] = v;
            return curve;
        }

        public static bool ReadAnimPacket(BinaryReader reader, uint animationType, AnimationFrame animationFrame, AnimationFrame lastAnimationFrame)
        {
            var tgt = ((animationType >> 16) & 0xf); // Update: 0-Coordinate, 1-General
            var cat = ((animationType >> 20) & 0x7); // Category: 0-Standard update driver
            var ini = ((animationType >> 23) & 0x1) == 1; // Scan: 0-Off, 1-On

            if (tgt == 0) // Coordinate update
            {
                // Interpolation: 0-None, 1-Linear, 2-Bezier, 3-BSpline, 4-BetaSpline[*], 9-Linear(*), 10-Bezier(*), 11-BSpline(*)
                // Scale: No packet information available for anything other than linear.
                // [*] Scale only, no packet information available. Assumed to be same as B-Spline.
                // (*) Scale: single value for xyz, Translation: int16 values, Rotation: not supported.
                var transAlgo = (animationType >>  0) & 0xf;
                var rotAlgo   = (animationType >>  4) & 0xf;
                var scaleAlgo = (animationType >>  8) & 0xf;
                var rotOrder  = (animationType >> 12) & 0xf; // 0-XYZ, 1-XZY, 2-YXZ, 3-YZX, 4-ZXY, 5-ZYX

                var transShort  = (transAlgo >= 9 && transAlgo <= 11);
                var scaleSingle = (scaleAlgo >= 9 && scaleAlgo <= 11);
                // Bezier curve packets store 3 vectors for each algorithm.
                var transCount = (transAlgo == 2 || transAlgo == 10 ? 3 : 1);
                var rotCount   = (rotAlgo   == 2                    ? 3 : 1);
                var scaleCount = (scaleAlgo == 2 || scaleAlgo == 10 ? 3 : 1);
                Vector3[] t = (transAlgo != 0 ? new Vector3[transCount] : null);
                Vector3[] r = (rotAlgo   != 0 ? new Vector3[rotCount]   : null);
                Vector3[] s = (scaleAlgo != 0 ? new Vector3[scaleCount] : null);
                if (transAlgo != 0)
                {
                    for (var i = 0; i < transCount; i++)
                    {
                        t[i].X = (transShort ? reader.ReadInt16() : reader.ReadInt32());
                        t[i].Y = (transShort ? reader.ReadInt16() : reader.ReadInt32());
                        t[i].Z = (transShort ? reader.ReadInt16() : reader.ReadInt32());
                    }
                }
                if (rotAlgo != 0)
                {
                    for (var i = 0; i < rotCount; i++)
                    {
                        // 4096 == 360 degrees
                        r[i].X = (float)(reader.ReadInt16() / 4096.0 * (Math.PI * 2.0));
                        r[i].Y = (float)(reader.ReadInt16() / 4096.0 * (Math.PI * 2.0));
                        r[i].Z = (float)(reader.ReadInt16() / 4096.0 * (Math.PI * 2.0));
                    }
                }
                if (scaleAlgo != 0)
                {
                    for (var i = 0; i < scaleCount; i++)
                    {
                        s[i].X = reader.ReadInt16() / 4096f;
                        if (!scaleSingle)
                        {
                            s[i].Y = reader.ReadInt16() / 4096f;
                            s[i].Z = reader.ReadInt16() / 4096f;
                        }
                        else
                        {
                            s[i].Y = s[i].X;
                            s[i].Z = s[i].X;
                        }
                    }
                }
                // Note: Packets may end in 2-byte padding, but we don't need to read that.


                if (TryGetAnimInterpolationType(transAlgo, out var transType))
                {
                    animationFrame.TranslationType = transType;

                    switch (transType)
                    {
                        case InterpolationType.Linear:
                            animationFrame.Translation = t[0];
                            break;
                        case InterpolationType.Bezier:
                            //animationFrame.CurveTranslations = t;
                            //break;
                            return false; // Only linear is currently supported.
                        case InterpolationType.BSpline:
                            //animationFrame.CurveTranslations = AppendToCurve(animationFrame.CurveTranslations, t[0]);
                            //break;
                            return false; // Only linear is currently supported.
                        case InterpolationType.BetaSpline:
                            return false; // Invalid translation type.
                    }

                    if (transType != InterpolationType.None && lastAnimationFrame != null)
                    {
                        lastAnimationFrame.FinalTranslation = t[0];
                    }
                }
                else
                {
                    return false; // Invalid translation interpolation type.
                }

                if (TryGetAnimInterpolationType(scaleAlgo, out var scaleType))
                {
                    animationFrame.ScaleType = scaleType;

                    switch (scaleType)
                    {
                        case InterpolationType.Linear:
                            animationFrame.Scale = s[0];
                            break;
                        case InterpolationType.Bezier:
                            //animationFrame.CurveScales = s;
                            //break;
                            return false; // Only linear is currently supported.
                        case InterpolationType.BSpline:
                        case InterpolationType.BetaSpline:
                            //animationFrame.CurveScales = AppendToCurve(animationFrame.CurveScales, s[0]);
                            //break;
                            return false; // Only linear is currently supported.
                    }

                    if (scaleType != InterpolationType.None && lastAnimationFrame != null)
                    {
                        lastAnimationFrame.FinalScale = s[0];
                    }
                }
                else
                {
                    return false; // Invalid scale interpolation type.
                }

                if (TryGetAnimInterpolationType(rotAlgo, out var rotType))
                {
                    animationFrame.RotationType = rotType;

                    switch (rotType)
                    {
                        case InterpolationType.Linear:
                            animationFrame.EulerRotation = r[0];
                            break;
                        case InterpolationType.Bezier:
                            //animationFrame.CurveEulerRotations = r;
                            //break;
                            return false; // Only linear is currently supported.
                        case InterpolationType.BSpline:
                            //animationFrame.CurveEulerRotations = AppendToCurve(animationFrame.CurveEulerRotations, r[0]);
                            //break;
                            return false; // Only linear is currently supported.
                        case InterpolationType.BetaSpline:
                            return false; // Invalid rotation type.
                    }

                    if (rotType != InterpolationType.None && lastAnimationFrame != null)
                    {
                        lastAnimationFrame.FinalEulerRotation = r[0];
                    }

                    if (rotType == InterpolationType.None)
                    {
                        animationFrame.RotationOrder = RotationOrder.None;
                    }
                    else if (TryGetAnimRotationOrder(rotOrder, out var rotOrderType))
                    {
                        animationFrame.RotationOrder = rotOrderType;
                    }
                    else
                    {
                        return false; // Invalid rotation order.
                    }
                }
                else
                {
                    return false; // Invalid rotation interpolation type.
                }

                return true;
            }
            else if (tgt == 1) // General update (vertices or normals)
            {
                var length = (animationType >> 0) & 0xf; // Length: 0-32bit, 1-16bit, 2-8bit
                var write  = (animationType >> 4) & 0xf; // Write area: bits represent each unit to write to.
                var algo   = (animationType >> 8) & 0xf; // Interpolation: 1-Linear, 2-Bezier, 3-BSpline
                // Length: 16bit, Write: 0b1010 - Would write to 0x2 and 0x6
                // Not supported. In the future we can add support for updating vertices and normals.

                return false;
            }
            else
            {
                return false; // Invalid TGT
            }
        }

        public static Vector3 Interpolate(InterpolationType interpType, Vector3? src, Vector3[] curve, Vector3? dst, float delta)
        {
            switch (interpType)
            {
                case InterpolationType.Linear:
                    return GeomUtils.InterpolateVector(src.Value, dst ?? src.Value, delta);
                case InterpolationType.Bezier:
                case InterpolationType.BSpline:
                case InterpolationType.BetaSpline:
                    if (curve == null || curve.Length < 3)
                    {
                        break; // Invalid curve array
                    }
                    // todo: Using curve[2] instead of dst isn't really a solution...
                    //curve[0], curve[1], curve[2], dst ?? curve[2], delta
                    break; // Not supported yet
            }
            return Vector3.Zero;
        }

        public static void PrintAnimInstructions(BinaryReader reader, uint ctrlTop, uint paramTop, long offset, Dictionary<uint, List<Tuple<uint, uint>>> tmdidStarts = null)
        {
            // Read instructions.
            var position = reader.BaseStream.Position;
            reader.BaseStream.Seek(offset + ctrlTop, SeekOrigin.Begin);

            var instructionCount = (paramTop - ctrlTop) / 4;
            var instructions = new uint[instructionCount];
            for (var i = 0; i < instructionCount; i++)
            {
                instructions[i] = reader.ReadUInt32();
            }
            reader.BaseStream.Seek(position, SeekOrigin.Begin);

            // Find instructions that are referenced by jumps.
            var xrefs = new Dictionary<uint, List<uint>>();
            for (uint i = 0; i < instructionCount; i++)
            {
                var descriptor = instructions[i];
                var descriptorType = (descriptor >> 30) & 0x3;
                if (descriptorType == 0x2)
                {
                    var seqIndex = (descriptor) & 0xffff; // Next control descriptor to jump to.
                    if (!xrefs.TryGetValue(seqIndex, out var xrefList))
                    {
                        xrefList = new List<uint>();
                        xrefs.Add(seqIndex, xrefList);
                    }
                    xrefList.Add(i);
                }
            }

            // Print starting Indices#StreamIDs for each TMDID object.
            if (tmdidStarts != null)
            {
                Console.ForegroundColor = ConsoleColor.White;
                foreach (var pair in tmdidStarts.OrderBy(p => p.Key))
                {
                    var arrayStr = string.Join(", ", pair.Value.Select(t => $"{t.Item1}#{t.Item2}"));
                    Console.WriteLine($"TMDID {pair.Key,2}: [{arrayStr}]");
                }
                Console.ResetColor();
            }

            // Format padding information.
            var digits = instructionCount.ToString().Length;
            var addrLen = digits + 2;
            var opLen = 7;
            var argLen = 20;
            var xrefStart = addrLen + opLen + argLen;

            // Print instructions.
            for (uint i = 0; i < instructionCount; i++)
            {
                // Writer to track line length for padding purposes.
                var lineLength = 0;
                void Write(ConsoleColor color, string text, int pad = 0, bool padLeft = false)
                {
                    if (pad > 0)
                    {
                        text = (padLeft ? text.PadLeft(pad) : text.PadRight(pad));
                    }
                    Console.ForegroundColor = color;
                    Console.Write(text);
                    Console.ResetColor();
                    lineLength += text.Length;
                }

                var descriptor = instructions[i];
                var descriptorType = (descriptor >> 30) & 0x3;

                // Format: " N: "
                var addrColor = (xrefs.ContainsKey(i) ? ConsoleColor.DarkCyan : ConsoleColor.DarkGray);
                Write(addrColor, $"{i}: ", addrLen, true);

                if ((descriptorType & 0x2) == 0x0) // Normal
                {
                    var paramIndex  = (descriptor >>  0) & 0xffff; // Index to parameter data for key frame referred to by sequence descriptor.
                    var nextTFrame  = (descriptor >> 16) & 0xff; // Frame number of next sequence descriptor (int).
                    var interpIndex = (descriptor >> 24) & 0x7f; // Index into interpolation table. Specifies function to be used.
                    
                    // TFrame==0 has special meaning
                    var tfColor = (nextTFrame == 0 ? ConsoleColor.Red : ConsoleColor.Green);
                    //var tfColor = (nextTFrame == 0 ? ConsoleColor.DarkYellow : ConsoleColor.Green);
                    //var tfColor = ConsoleColor.Gray;

                    // Format: "norm    p:N     t:N   f:N"
                    Write(ConsoleColor.White, "norm", opLen);
                    Write(ConsoleColor.Gray, $" p:{paramIndex,-5}"); // +8
                    Write(tfColor, $" t:{nextTFrame,-3}"); // +6
                    Write(ConsoleColor.Gray, $" f:{interpIndex,-2}"); // +5
                }
                else if (descriptorType == 0x2) // Jump
                {
                    var seqIndex = (descriptor >>  0) & 0xffff; // Next control descriptor to jump to.
                    var cnd      = (descriptor >> 16) & 0x7f; // Stream ID conditional jump.
                    var dst      = (descriptor >> 23) & 0x7f; // Stream ID destination of jump.

                    // Format: "cnd     @N     #N   >#N"
                    // Format: "jmpd    @N          >#N"
                    // Format: "jmp     @N"
                    if (cnd != 0)
                    {
                        Write(ConsoleColor.Blue, "cnd", opLen); // Jump if condition and set SID.
                    }
                    else if (dst != 0)
                    {
                        Write(ConsoleColor.Blue, "jmpd", opLen); // Jump and set SID.
                    }
                    else if (dst == 0)
                    {
                        Write(ConsoleColor.Blue, "jmp", opLen); // Jump without setting SID.
                    }

                    Write(ConsoleColor.Cyan, $" @{seqIndex,-5}"); // +7
                    if (cnd != 0)
                    {
                        var cnd_sid = (cnd == 127 ? 0 : cnd);
                        Write(ConsoleColor.Yellow, $" #{cnd_sid,-3}"); // +5
                    }
                    if (cnd != 0 || dst != 0)
                    {
                        Write(ConsoleColor.Yellow, $" >#{dst,-3}"); // +6
                    }
                }
                else if (descriptorType == 0x3) // Control
                {
                    var code = (descriptor >> 23) & 0x7f;
                    var p1   = (descriptor >> 16) & 0x7f;
                    var p2   = (descriptor >>  0) & 0xffff;
                    if (code == 1) // End
                    {
                        // Format: "end"
                        // Format: "endif         #N"
                        if (p1 == 0)
                        {
                            Write(ConsoleColor.Magenta, "end", opLen);
                        }
                        else if (p1 != 0)
                        {
                            Write(ConsoleColor.Magenta, "endif", opLen);
                            var cnd_sid = (p1 == 127 ? 0 : p1);
                            Write(ConsoleColor.Yellow, "", 6); // +6 (align with cnd instructions by skipping jump target)
                            Write(ConsoleColor.Yellow, $" #{cnd_sid,-3}"); // +5
                        }
                    }
                    else if (code == 2) // Work
                    {
                        // Format: "work    p:N"
                        // Format: "work    p:N     p1:N"
                        Write(ConsoleColor.Magenta, "work", opLen);
                        Write(ConsoleColor.Gray, $" p:{p2,-5}"); // +8
                        if (p1 != 127) // Should always be 127
                        {
                            Write(ConsoleColor.DarkGray, $" p1:{p1,-3}"); // +7
                        }
                    }
                    else // Unknown code
                    {
                        // Format: "code.XX p1:0xXX p2:0xXXXX"
                        Write(ConsoleColor.Red, $"code.{code:x02}", opLen);
                        Write(ConsoleColor.Gray, $" p1:0x{p1:x02}"); // +8
                        Write(ConsoleColor.Gray, $" p2:0x{p2:x04}"); // +10
                    }
                }
                
                if (xrefs.TryGetValue(i, out var xrefList))
                {
                    // Format: " [N] [N]"
                    Write(ConsoleColor.DarkCyan, "", (xrefStart - lineLength)); // Pad
                    foreach (var xref in xrefList)
                    {
                        Write(ConsoleColor.DarkCyan, $" [{xref}]");
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
