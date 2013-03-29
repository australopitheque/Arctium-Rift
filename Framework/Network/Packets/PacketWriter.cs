/*
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
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;

namespace Framework.Network.Packets
{
    public class PacketWriter : BinaryWriter
    {
        public ServerMessage Opcode { get; set; }
        public uint Size { get; set; }

        public PacketWriter() : base(new MemoryStream()) { }
        public PacketWriter(ServerMessage message, bool isWorldPacket = false) : base(new MemoryStream())
        {
            WritePacketHeader(message);
        }

        protected void WritePacketHeader(ServerMessage opcode)
        {
            Opcode = opcode;

            WriteEncoded((ushort)Opcode);

            Size = (uint)BaseStream.Length;
        }

        public byte[] ReadDataToSend()
        {
            byte[] data = new byte[BaseStream.Length];

            Seek(0, SeekOrigin.Begin);

            for (int i = 0; i < BaseStream.Length; i++)
                data[i] = (byte)BaseStream.ReadByte();

            Size = (uint)data.Length - Size;

            Seek(0, SeekOrigin.Begin);

            WriteEncoded(data.Length);
            WriteBytes(data);

            Seek(0, SeekOrigin.Begin);

            data = new byte[BaseStream.Length];

            for (int i = 0; i < BaseStream.Length; i++)
                data[i] = (byte)BaseStream.ReadByte();

            return data;
        }

        public void WriteEncodedData(int type, int index)
        {
            var value = index;
            value <<= 3;
            value |= (byte)(type & 7);

            WriteEncoded(value);
        }

        public void WriteEncoded(long data)
        {
            var val = data;
            var repeat = true;

            for (int i = 0; repeat; i++)
            {
                var tmp = (byte)(val & 0x7F);

                val = val >> 7;
                repeat = val != 0;

                if (repeat)
                    tmp += 128;

                WriteUInt8(tmp);
            }
        }

        public void WriteInt8(sbyte data)
        {
            base.Write(data);
        }

        public void WriteInt16(short data)
        {
            base.Write(data);
        }

        public void WriteInt32(int data)
        {
            base.Write(data);
        }

        public void WriteInt64(long data)
        {
            base.Write(data);
        }

        public void WriteUInt8(byte data)
        {
            base.Write(data);
        }

        public void WriteUInt16(ushort data)
        {
            base.Write(data);
        }

        public void WriteUInt32(uint data)
        {
            base.Write(data);
        }

        public void WriteUInt64(ulong data)
        {
            base.Write(data);
        }

        public void WriteFloat(float data)
        {
            base.Write(data);
        }

        public void WriteDouble(double data)
        {
            base.Write(data);
        }

        public void WriteCString(string data)
        {
            byte[] sBytes = UTF8Encoding.UTF8.GetBytes(data);

            WriteBytes(sBytes);
            WriteUInt8(0);
        }

        public void WriteString(string data)
        {
            byte[] sBytes = UTF8Encoding.UTF8.GetBytes(data);

            if (sBytes.Length == 0)
                sBytes = new byte[1];

            WriteBytes(sBytes);
        }

        public void WriteUnixTime()
        {
            DateTime baseDate = new DateTime(1970, 1, 1);
            DateTime currentDate = DateTime.Now;
            TimeSpan ts = currentDate - baseDate;

            WriteUInt32(Convert.ToUInt32(ts.TotalSeconds));
        }

        public void WriteBytes(byte[] data, int count = 0)
        {
            if (count == 0)
                base.Write(data);
            else
                base.Write(data, 0, count);
        }

        public void WriteBitArray(BitArray buffer, int Len)
        {
            byte[] bufferarray = new byte[Convert.ToByte((buffer.Length + 8) / 8) + 1];
            buffer.CopyTo(bufferarray, 0);

            WriteBytes(bufferarray.ToArray(), Len);
        }

        public byte[] ToBytes()
        {
            using (var ms = new MemoryStream())
            {
                BaseStream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public void WriteUInt32Pos(uint data, int pos)
        {
            Seek(pos, SeekOrigin.Begin);

            WriteUInt32(data);

            Seek((int)BaseStream.Length - 1, SeekOrigin.Begin);
        }
    }
}
