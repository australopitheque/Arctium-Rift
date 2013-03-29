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

using Framework.Logging;
using Framework.Logging.PacketLogging;
using Framework.Network.Packets;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using WorldServer.Game.Packets;

namespace WorldServer.Network
{
    public sealed class SelectionClass : IDisposable
    {
        public static SelectionNetwork selection;
        public Socket clientSocket;
        public Queue PacketQueue;
        byte[] DataBuffer;

        public SelectionClass()
        {
            DataBuffer = new byte[4096];
            PacketQueue = new Queue();
        }

        public void OnData()
        {
            var packet = new PacketReader(DataBuffer);

            string clientInfo = ((IPEndPoint)clientSocket.RemoteEndPoint).Address + ":" + ((IPEndPoint)clientSocket.RemoteEndPoint).Port;
            PacketLog.WritePacket(clientInfo, null, packet);

            PacketManager.InvokeHandler(ref packet, this);
        }

        public void OnConnect()
        {
            clientSocket.BeginReceive(DataBuffer, 0, DataBuffer.Length, SocketFlags.None, Receive, null);
        }

        public void Receive(IAsyncResult result)
        {
            try
            {
                var recievedBytes = clientSocket.EndReceive(result);
                if (recievedBytes != 0)
                {
                    OnData();

                    clientSocket.BeginReceive(DataBuffer, 0, DataBuffer.Length, SocketFlags.None, Receive, null);
                }
            }
            catch (Exception ex)
            {
                Log.Message(LogType.ERROR, "{0}", ex.Message);
                Log.Message();
            }
        }

        public void Send(ref PacketWriter packet)
        {
            var buffer = packet.ReadDataToSend();

            try
            {
                clientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);

                string clientInfo = ((IPEndPoint)clientSocket.RemoteEndPoint).Address + ":" + ((IPEndPoint)clientSocket.RemoteEndPoint).Port;
                PacketLog.WritePacket(clientInfo, packet);

                packet.Flush();
            }
            catch (Exception ex)
            {
                Log.Message(LogType.ERROR, "{0}", ex.Message);
                Log.Message();

                clientSocket.Close();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
