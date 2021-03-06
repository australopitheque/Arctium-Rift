﻿/*
 * Copyright (C) 2012-2013 Arctium Rift <http://arctium.org>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using Framework.Constants.Network.Messages;
using Framework.Constants.Network.Packets;
using Framework.Helper;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Framework.Network.Packets
{
    public class PacketReader : BinaryReader
    {
        public ClientMessage Opcode { get; set; }
        public ushort Size { get; set; }
        public byte[] Storage { get; set; }

        public PacketReader(byte[] data) : base(new MemoryStream(data))
        {
            int sizeLength;
            int opcodeLength;

            Size = (ushort)(ReadEncoded<ushort>(out sizeLength));
            Opcode = (ClientMessage)ReadEncoded<ushort>(out opcodeLength);

            Size -= (ushort)(opcodeLength + sizeLength);

            Storage = new byte[Size];
            Array.Copy(data, sizeLength + opcodeLength, Storage, 0, Size);
        }

        public T Read<T>()
        {
            return BinaryReaderExtensions.Read<T>(this);
        }

        public T ReadEncoded<T>()
        {
            int count;

            return ReadEncoded<T>(out count);
        }

        public T ReadEncoded<T>(out int count)
        {
            count = 0;

            var val = 0ul;
            var repeat = true;

            for (int i = 0; repeat; i++)
            {
                var tmp = Read<byte>();
                repeat = (tmp & 0x80) != 0;
                val |= ((uint)tmp & 0x7F) << i * 7;

                count = i + 1;
            }

            return (T)Convert.ChangeType(val, (typeof(T)));
        }

        public string ReadCString()
        {
            StringBuilder tmpString = new StringBuilder();
            char tmpChar = base.ReadChar();
            char tmpEndChar = Convert.ToChar(Encoding.UTF8.GetString(new byte[] { 0 }), CultureInfo.InvariantCulture);

            while (tmpChar != tmpEndChar)
            {
                tmpString.Append(tmpChar);
                tmpChar = base.ReadChar();
            }

            return tmpString.ToString();
        }

        public string ReadString(uint count)
        {
            byte[] stringArray = ReadBytes(count);
            return Encoding.UTF8.GetString(stringArray);
        }

        public byte[] ReadBytes(uint count)
        {
            return base.ReadBytes((int)count);
        }

        public string ReadStringFromBytes(uint count)
        {
            byte[] stringArray = ReadBytes(count);
            Array.Reverse(stringArray);

            return UTF8Encoding.UTF8.GetString(stringArray);
        }

        public string ReadIPAddress()
        {
            byte[] ip = new byte[4];

            for (int i = 0; i < 4; ++i)
                ip[i] = this.Read<byte>();

            return ip[0] + "." + ip[1] + "." + ip[2] + "." + ip[3];
        }

        public void Skip(int count)
        {
            base.BaseStream.Position += count;
        }
    }
}
