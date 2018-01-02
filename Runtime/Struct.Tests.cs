﻿#region License
//  Copyright 2015 John Källén
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion

#if DEBUG
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pytocs.runtime
{
    [TestFixture]
    public class StructTests
    {
        private byte[] hex(string str)
        {
            return Enumerable.Range(0, str.Length / 2)
                .Select(i => (byte)(hexDigit(str[i * 2]) * 16 + hexDigit(str[i * 2 + 1])))
                .ToArray();
        }

        private static int hexDigit(char ch)
        {
            if ('0' <= ch && ch <= '9')
                return ch - '0';
            else if ('A' <= ch && ch <= 'F')
                return (ch - 'A') + 10;
            else if ('a' <= ch && ch <= 'f')
                return (ch - 'a') + 10;
            else
                throw new ArgumentException($"Invalid hex digit '{ch}' (U+{(int)ch:X4}");
        }

        [Test]
        public void Struct_unpack_LEInt()
        {
            var tup = @struct.unpack<Tuple<int>>("<i", hex("78563412"));
            Assert.AreEqual(0x12345678, tup.Item1);
        }


        [Test]
        public void Struct_unpack_BEint_ushort()
        {
            var tup = @struct.unpack<Tuple<int,ushort>>(">iH", hex("12345678FCB0"));
            Assert.AreEqual(0x12345678, tup.Item1);
            Assert.AreEqual((ushort)0xFCB0, tup.Item2);
        }
    }
}
#endif
