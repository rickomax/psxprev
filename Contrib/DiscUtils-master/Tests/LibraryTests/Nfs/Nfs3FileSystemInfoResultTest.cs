﻿//
// Copyright (c) 2017, Quamotion
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using DiscUtils;
using DiscUtils.Nfs;
using System;
using System.IO;
using Xunit;

namespace LibraryTests.Nfs
{
    public class Nfs3FileSystemInfoResultTest
    {
        [Fact]
        public void RoundTripTest()
        {
            Nfs3FileSystemInfoResult result = new Nfs3FileSystemInfoResult()
            {
                FileSystemInfo = new Nfs3FileSystemInfo()
                {
                    DirectoryPreferredBytes = 1,
                    FileSystemProperties = Nfs3FileSystemProperties.HardLinks,
                    MaxFileSize = 3,
                    ReadMaxBytes = 4,
                    ReadMultipleSize = 5,
                    ReadPreferredBytes = 6,
                    TimePrecision = Nfs3FileTime.Precision,
                    WriteMaxBytes = 8,
                    WriteMultipleSize = 9,
                    WritePreferredBytes = 10
                },
                PostOpAttributes = new Nfs3FileAttributes()
                {
                    AccessTime = new Nfs3FileTime(new DateTime(2017, 1, 1)),
                    BytesUsed = 2,
                    ChangeTime = new Nfs3FileTime(new DateTime(2017, 1, 3)),
                    FileId = 4,
                    FileSystemId = 5,
                    Gid = 6,
                    LinkCount = 7,
                    Mode = UnixFilePermissions.OthersExecute,
                    ModifyTime = new Nfs3FileTime(new DateTime(2017, 1, 8)),
                    RdevMajor = 9,
                    RdevMinor = 10,
                    Size = 11,
                    Type = Nfs3FileType.BlockDevice,
                    Uid = 12
                },
                Status = Nfs3Status.Ok
            };

            Nfs3FileSystemInfoResult clone = null;

            using (MemoryStream stream = new MemoryStream())
            {
                XdrDataWriter writer = new XdrDataWriter(stream);
                result.Write(writer);

                stream.Position = 0;
                XdrDataReader reader = new XdrDataReader(stream);
                clone = new Nfs3FileSystemInfoResult(reader);
            }

            Assert.Equal(result, clone);
        }
    }
}
