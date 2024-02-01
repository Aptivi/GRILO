﻿//
// GRILO  Copyright (C) 2022  Aptivi
//
// This file is part of GRILO
//
// GRILO is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// GRILO is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

#if !NETCOREAPP
using GRILO.Boot;
using System;

namespace GRILO.Bootloader.BootApps
{
    internal class BootProxy : IBootable
    {
        internal BootLoader loader = null;

        public string Title => "Boot proxy";

        public bool ShutdownRequested { get; set; }

        public void Boot(string[] args)
        {
            Console.Clear();
            loader.ProxyExecuteBootable(args);
            ShutdownRequested = loader.shutting;
        }
    }
}
#endif
