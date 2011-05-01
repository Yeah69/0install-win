﻿/*
 * Copyright 2010-2011 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using Common;
using NDesk.Options;
using ZeroInstall.Commands.Cli.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
#if !DEBUG
using Common.Storage;
using Common.Utils;
#endif

namespace ZeroInstall.Commands.Cli
{
    /// <summary>
    /// A command-line interface for Zero Install, for installing and launching applications, managing caches, etc.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static int Main(string[] args)
        {
#if !DEBUG
            // Prevent launch during update and allow instance detection
            string mutexName = AppMutex.GenerateName(Locations.InstallationBase);
            if (AppMutex.Probe(mutexName + "-update")) return 99;
            AppMutex.Create(mutexName);
            AppMutex.Create("Zero Install");
#endif

            // Automatically show help for missing args
            if (args.Length == 0) args = new[] { "--help" };

            IHandler handler = new CliHandler();
            CommandBase command;
            try { command = CommandFactory.CreateAndParse(args, handler); }
            #region Error handling
            catch (UserCancelException)
            {
                // This is reached if --help, --version or similar was used
                return 0;
            }
            catch (OptionException ex)
            {
                Log.Error(ex.Message + "\n" + Resources.TryHelp);
                return 1;
            }
            catch (IOException ex)
            {
                Log.Error(ex.Message);
                return 1;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return 1;
            }
            catch (InvalidInterfaceIDException ex)
            {
                Log.Error(ex.Message);
                return 1;
            }
            #endregion

            try { return command.Execute(); }
            #region Error handling
            catch (UserCancelException)
            {
                handler.CloseProgressUI();
                return 1;
            }
            catch (OptionException ex)
            {
                handler.CloseProgressUI();
                Log.Error(ex.Message + "\n" + Resources.TryHelp);
                return 1;
            }
            catch (WebException ex)
            {
                handler.CloseProgressUI();
                Log.Error(ex.Message);
                return 1;
            }
            catch (NotSupportedException ex)
            {
                handler.CloseProgressUI();
                Log.Error(ex.Message);
                return 1;
            }
            catch (IOException ex)
            {
                handler.CloseProgressUI();
                Log.Error(ex.Message);
                return 1;
            }
            catch (UnauthorizedAccessException ex)
            {
                handler.CloseProgressUI();
                Log.Error(ex.Message);
                return 1;
            }
            catch (DigestMismatchException ex)
            {
                handler.CloseProgressUI();
                Log.Error(ex.Message);
                //if (command.Verbosity >= 1) Log.Info("Generated manifest:\n" + ex.ActualManifest);
                return 1;
            }
            catch (InvalidInterfaceIDException ex)
            {
                handler.CloseProgressUI();
                Log.Error(ex.Message);
                return 1;
            }
            catch (SolverException ex)
            {
                handler.CloseProgressUI();
                Log.Error(ex.Message);
                return 1;
            }
            catch (ImplementationNotFoundException ex)
            {
                handler.CloseProgressUI();
                Log.Error(ex.Message);
                return 1;
            }
            catch (CommandException ex)
            {
                handler.CloseProgressUI();
                Log.Error(ex.Message);
                return 1;
            }
            catch (Win32Exception ex)
            {
                handler.CloseProgressUI();
                Log.Error(ex.Message);
                return 1;
            }
            catch (BadImageFormatException ex)
            {
                handler.CloseProgressUI();
                Log.Error(ex.Message);
                return 1;
            }
            #endregion
        }
    }
}
