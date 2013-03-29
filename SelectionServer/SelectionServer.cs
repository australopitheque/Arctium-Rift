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

﻿using Framework.Console;
using Framework.Console.Commands;
using Framework.Logging;
﻿using System;
﻿using WorldServer.Game.Packets;
﻿using WorldServer.Network;

namespace WorldServer
{
    class WorldServer
    {
        static void Main()
        {
            Log.ServerType = "SelectionServer";

            Log.Message(LogType.INIT, "___________________________________________");
            Log.Message(LogType.INIT, "    __                                     ");
            Log.Message(LogType.INIT, "    / |                     ,              ");
            Log.Message(LogType.INIT, "---/__|---)__----__--_/_--------------_--_-");
            Log.Message(LogType.INIT, "  /   |  /   ) /   ' /    /   /   /  / /  )");
            Log.Message(LogType.INIT, "_/____|_/_____(___ _(_ __/___(___(__/_/__/_");
            Log.Message(LogType.INIT, "____________________RIFT___________________");
            Log.Message();

            Log.Message(LogType.NORMAL, "Starting Arctium Rift SelectionServer...");

            SelectionClass.selection = new SelectionNetwork();

            if (SelectionClass.selection.Start("127.0.0.1", 6520))
            {
                SelectionClass.selection.AcceptConnectionThread();
                Log.Message(LogType.NORMAL, "SelectionServer listening on {0} port {1}.", "127.0.0.1", 6520);
                Log.Message(LogType.NORMAL, "SelectionServer successfully started!");

                PacketManager.DefineOpcodeHandler();
            }
            else
            {
                Log.Message(LogType.ERROR, "Server couldn't be started: ");
            }

            Log.Message(LogType.NORMAL, "Total Memory: {0} Kilobytes", GC.GetTotalMemory(false) / 1024);

            CommandDefinitions.Initialize();
            CommandManager.InitCommands();
        }
    }
}
