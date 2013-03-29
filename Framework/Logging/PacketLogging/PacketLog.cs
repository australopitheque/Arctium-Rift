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
using Framework.Constants.Network.Packets;
using Framework.Network.Packets;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Framework.Logging.PacketLogging
{
    public class PacketLog
    {
        static TextWriter logWriter;
        static Object syncObj = new Object();

        public static void WritePacket(string clientInfo, PacketWriter serverPacket = null, PacketReader clientPacket = null)
        {
            lock (syncObj)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();

                    if (serverPacket != null)
                    {
                        sb.AppendLine(String.Format("Client: {0}", clientInfo));
                        sb.AppendLine(String.Format("Time: {0}", DateTime.Now.ToString()));

                        if (Enum.IsDefined(typeof(ServerMessage), serverPacket.Opcode))
                        {
                            sb.AppendLine("Type: ServerMessage");
                            sb.AppendLine(String.Format("Name: {0}", Enum.GetName(typeof(ServerMessage), serverPacket.Opcode)));
                        }

                        sb.AppendLine(String.Format("Value: 0x{0:X} ({1})", serverPacket.Opcode, serverPacket.Opcode));
                        sb.AppendLine(String.Format("Length: {0}", serverPacket.Size));

                        sb.AppendLine("|----------------------------------------------------------------|");
                        sb.AppendLine("| 00  01  02  03  04  05  06  07  08  09  0A  0B  0C  0D  0E  0F |");
                        sb.AppendLine("|----------------------------------------------------------------|");
                        sb.Append("|");

                        if (serverPacket.Size != 0)
                        {
                            var data = serverPacket.ReadDataToSend().ToList();
                            data.RemoveRange(0, data.Count - (int)serverPacket.Size);

                            byte count = 0;
                            data.ForEach(b =>
                            {
                                if (b <= 0xF)
                                    sb.Append(String.Format(" 0{0:X} ", b));
                                else
                                    sb.Append(String.Format(" {0:X} ", b));

                                if (count == 15)
                                {
                                    sb.Append("|");
                                    sb.AppendLine();
                                    sb.Append("|");
                                    count = 0;
                                }
                                else
                                    count++;
                            });

                            sb.AppendLine("");
                            sb.AppendLine("|----------------------------------------------------------------|");
                        }

                        sb.AppendLine("");
                    }

                    if (clientPacket != null)
                    {
                        sb.AppendLine(String.Format("Client: {0}", clientInfo));
                        sb.AppendLine(String.Format("Time: {0}", DateTime.Now.ToString()));

                        sb.AppendLine("Type: ClientMessage");

                        if (Enum.IsDefined(typeof(ClientMessage), clientPacket.Opcode))
                            sb.AppendLine(String.Format("Name: {0}", clientPacket.Opcode));
                        else
                        {
                            sb.AppendLine(String.Format("Name: {0}", "Unknown"));

                            var fullStruct = "";
                            var counter = "";

                            while (clientPacket.BaseStream.Position <= clientPacket.Storage.Length)
                            {
                                var val = clientPacket.ReadEncoded<byte>();
                                var dataType = (DataType)(val & 7);
                                var fieldIndex = val >> 3;

                                fullStruct += dataType + "[" + fieldIndex + "] ";

                                switch (dataType)
                                {
                                    case DataType.BooleanFalse:
                                    case DataType.BooleanTrue:
                                        clientPacket.Skip(1);
                                        break;
                                    case DataType.UnsignedEncoded:
                                    case DataType.SignedEncoded:
                                        int eLength;

                                        clientPacket.ReadEncoded<ulong>(out eLength);
                                        counter += String.Format("{0}[{2}]: {1} ", dataType, eLength, fieldIndex);
                                        break;
                                    case DataType.Bits32:
                                        clientPacket.Skip(4);
                                        break;
                                    case DataType.Bits64:
                                        clientPacket.Skip(8);
                                        break;
                                    case DataType.Bytes:
                                        int arrLength = clientPacket.ReadEncoded<int>();
                                        clientPacket.Skip(arrLength);

                                        counter += String.Format("{0}[{2}]: {1} ", dataType, arrLength, fieldIndex);
                                        break;
                                    default:
                                        Log.Message(LogType.ERROR, "Tried to read unsupported data type '{0}'", dataType);
                                        break;
                                }
                            }

                            sb.AppendLine(String.Format("DataTypes: {0}", fullStruct));

                            if (counter != "")
                                sb.AppendLine(String.Format("DataLength: {0}", counter));
                        }

                        sb.AppendLine(String.Format("Value: 0x{0:X} ({1})", (ushort)clientPacket.Opcode, (ushort)clientPacket.Opcode));
                        sb.AppendLine(String.Format("Length: {0}", clientPacket.Size));

                        sb.AppendLine("|----------------------------------------------------------------|");
                        sb.AppendLine("| 00  01  02  03  04  05  06  07  08  09  0A  0B  0C  0D  0E  0F |");
                        sb.AppendLine("|----------------------------------------------------------------|");
                        sb.Append("|");

                        if (clientPacket.Size != 0)
                        {
                            var data = clientPacket.Storage;

                            byte count = 0;
                            foreach (var b in data)
                            {

                                if (b <= 0xF)
                                    sb.Append(String.Format(" 0{0:X} ", b));
                                else
                                    sb.Append(String.Format(" {0:X} ", b));

                                if (count == 15)
                                {
                                    sb.Append("|");
                                    sb.AppendLine();
                                    sb.Append("|");
                                    count = 0;
                                }
                                else
                                    count++;
                            }

                            sb.AppendLine();
                            sb.Append("|----------------------------------------------------------------|");
                        }

                        sb.AppendLine("");
                    }

                    logWriter = TextWriter.Synchronized(File.AppendText("Packet.log"));
                    logWriter.WriteLine(sb.ToString());
                    logWriter.Flush();
                }
                finally
                {
                    logWriter.Close();
                }
            }
        }
    }
}
