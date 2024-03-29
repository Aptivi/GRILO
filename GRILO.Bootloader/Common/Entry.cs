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

using System;
using System.Reflection;
using Terminaux.Writer.ConsoleWriters;
using Terminaux.Sequences.Builder;
using Terminaux.Inputs;
using SpecProbe.Platform;
using GRILO.Bootloader.Boot.Apps;
using GRILO.Bootloader.Boot.Diagnostics;
using GRILO.Bootloader.Common.Configuration;
using GRILO.Bootloader.Boot.Style;
using Terminaux.ResizeListener;
using Terminaux.Base;
using Terminaux.Base.Buffered;
using Terminaux.Reader;
using Terminaux.Colors;

namespace GRILO.Bootloader.Common
{
    internal class Entry
    {
        internal static string griloVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private static bool isOnAlternativeBuffer = false;
        private static bool isOnMainLoop = false;

        static void Main()
        {
            try
            {
                // Preload bootloader
                ConsoleWrapper.CursorVisible = false;
                TextWriterColor.Write("Starting GRILO v{0}...", griloVersion);

                // Populate GRILO folders (if any)
                ConfigPaths.MakePaths();

                // Populate custom boot styles
                BootStyleManager.PopulateCustomBootStyles();
                DiagnosticsWriter.WriteDiag(DiagnosticsLevel.Info, "Custom boot styles read successfully.");

                // Populate the bootable apps list
                BootManager.PopulateBootApps();
                DiagnosticsWriter.WriteDiag(DiagnosticsLevel.Info, "Bootable apps read successfully.");

                // Switch to alternative buffer
                if (!PlatformHelper.IsOnWindows())
                {
                    TextWriterRaw.WriteRaw(VtSequenceBuilderTools.BuildVtSequence(VtSequenceSpecificTypes.EscSaveCursor), false);
                    TextWriterRaw.WriteRaw(VtSequenceBuilderTools.BuildVtSequence(VtSequenceSpecificTypes.CsiSetMode, "?47"), false);
                    isOnAlternativeBuffer = true;
                }

                // Read the configuration (or make one if not found)
                Config.ReadConfig();
                DiagnosticsWriter.WriteDiag(DiagnosticsLevel.Info, "Config read successfully.");

                // Run the console resize listener
                ConsoleResizeListener.StartResizeListener();

                // Set the foreground and the background color
                ColorTools.AllowForeground = true;
                ColorTools.AllowBackground = true;

                // Now, enter the main loop.
                isOnMainLoop = true;
                BootloaderMain.MainLoop();
            }
            catch (Exception ex)
            {
                // Failed trying to preload the bootloader or failure in the bootloader (after preloading)
                if (isOnMainLoop)
                {
                    DiagnosticsWriter.WriteDiag(DiagnosticsLevel.Error, "Preload bootloader failed: {0}", ex.Message);
                    TextWriterColor.Write("Failed to preload bootloader: {0}", ex.Message);
                }
                else
                {
                    DiagnosticsWriter.WriteDiag(DiagnosticsLevel.Error, "Bootloader has failed: {0}", ex.Message);
                    TextWriterColor.Write("GRILO has experienced an internal error: {0}", ex.Message);
                }

                DiagnosticsWriter.WriteDiag(DiagnosticsLevel.Error, "Stack trace:\n{0}", ex.StackTrace);
                TextWriterColor.Write(ex.StackTrace);
                TextWriterColor.Write("Press any key to exit.");
                TermReader.ReadKey();
            }
            finally
            {
                if (isOnAlternativeBuffer)
                {
                    TextWriterRaw.WriteRaw(VtSequenceBuilderTools.BuildVtSequence(VtSequenceSpecificTypes.CsiEraseInDisplay), false);
                    TextWriterRaw.WriteRaw(VtSequenceBuilderTools.BuildVtSequence(VtSequenceSpecificTypes.CsiResetMode, "?47"), false);
                    TextWriterRaw.WriteRaw(VtSequenceBuilderTools.BuildVtSequence(VtSequenceSpecificTypes.EscRestoreCursor), false);
                }

                // Set the foreground and the background color
                ColorTools.AllowForeground = false;
                ColorTools.AllowBackground = false;
                ColorTools.LoadBack();
                ConsoleWrapper.CursorVisible = true;
            }
        }
    }
}
