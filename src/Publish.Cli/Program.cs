﻿/*
 * Copyright 2010 Bastian Eicher
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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Common;
using Common.Cli;
using Common.Storage;
using NDesk.Options;
using ZeroInstall.Model;
using ZeroInstall.Publish.Arguments;
using ZeroInstall.Publish.Cli.Properties;

namespace ZeroInstall.Publish.Cli
{
    #region Enumerations
    /// <summary>
    /// An errorlevel is returned to the original caller after the application terminates, to indicate success or the reason for failure.
    /// </summary>
    public enum ErrorLevel
    {
        ///<summary>Everything is OK.</summary>
        OK = 0,

        /// <summary>The user canceled the operation.</summary>
        UserCanceled = 1,

        /// <summary>The arguments passed on the command-line were not valid.</summary>
        InvalidArguments = 2,

        /// <summary>An unknown or not supported feature was requested.</summary>
        NotSupported = 3,

        /// <summary>An IO error occurred.</summary>
        IOError = 10,

        /// <summary>An network error occurred.</summary>
        WebError = 11,

        /// <summary>A manifest digest for an implementation did not match the expected value.</summary>
        DigestMismatch = 20,

        /// <summary>A solver error occurred.</summary>
        SolverError = 21
    }
    #endregion

    /// <summary>
    /// Launches a command-line tool for editing Zero Install feed XMLs.
    /// </summary>
    public static class Program
    {
        #region Startup
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static int Main(string[] args)
        {
            // Automatically show help for missing args
            if (args.Length == 0) args = new[] { "--help" };

            ParseResults results;
            OperationMode mode;

            try { mode = ParseArgs(args, out results); }
            #region Error handling
            catch (ArgumentException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.InvalidArguments;
            }
            #endregion

            try { return Execute(mode, results); }
            #region Error hanlding
            catch (ArgumentException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            catch (FileNotFoundException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            catch (IOException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            catch (UnhandledErrorsException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            #endregion
        }
        #endregion

        #region Parse
        /// <summary>
        /// Parses command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments to be parsed.</param>
        /// <param name="results">The options detected by the parsing process.</param>
        /// <returns>The operation mode selected by the parsing process.</returns>
        /// <exception cref="ArgumentException">Throw if <paramref name="args"/> contains unknown options.</exception>
        public static OperationMode ParseArgs(IEnumerable<string> args, out ParseResults results)
        {
            #region Sanity checks
            if (args == null) throw new ArgumentNullException("args");
            #endregion

            // Prepare a structure for storing settings found in the arguments
            var mode = OperationMode.Normal;
            var parseResults = new ParseResults();

            #region Define options
            var options = new OptionSet
            {
                // Mode selection
                {"V|version", Resources.OptionVersion, unused => mode = OperationMode.Version},
                {"catalog=", Resources.OptionCatalog, catalogFile => { mode = OperationMode.Catalog; parseResults.CatalogFile = catalogFile; } },

                // Signatures
                {"x|xmlsign", Resources.OptionXmlSign, unused => parseResults.XmlSign = true},
                {"u|unsign", Resources.OptionXmlSign, unused => parseResults.Unsign = true},
                {"k|key=", Resources.OptionKey, user => parseResults.Key = user},
                {"gpg-passphrase=", Resources.OptionGnuPGPassphrase, user => parseResults.GnuPGPassphrase = user},
            };
            #endregion

            #region Help text
            options.Add("h|help|?", Resources.OptionHelp, unused =>
            {
                mode = OperationMode.Help;

                PrintUsage();
                Console.WriteLine(Resources.Options);
                options.WriteOptionDescriptions(Console.Out);
            });
            #endregion

            // Parse the arguments and call the hooked handlers
            var additionalArgs = options.Parse(args);
            try { parseResults.Feeds = ArgumentUtils.GetFiles(additionalArgs, "*.xml"); }
            #region Error handling
            catch (FileNotFoundException ex)
            {
                // Report as an invalid command-line argument
                throw new ArgumentException(ex.Message, ex);
            }
            #endregion

            // Return the now filled results structure
            results = parseResults;
            return mode;
        }
        #endregion

        #region Help
        private static void PrintUsage()
        {
            const string usage = "{0}\t{1}\n";
            Console.WriteLine(usage, Resources.Usage, Resources.UsageFeed);
        }
        #endregion

        #region Execute
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <param name="mode">The operation mode selected by the parsing process.</param>
        /// <param name="results">The parser results to be executed.</param>
        /// <returns>The error code to end the process with.</returns>
        /// <exception cref="ArgumentException">Throw if the specified feed file paths were invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a feed file is damaged.</exception>
        /// <exception cref="FileNotFoundException">Thrown if a feed file could not be found.</exception>
        /// <exception cref="IOException">Thrown if a file could not be read or written or if the GnuPG could not be launched or the feed file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to a feed file or the catalog file is not permitted.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        private static int Execute(OperationMode mode, ParseResults results)
        {
            switch (mode)
            {
                case OperationMode.Normal:
                    if (results.Feeds.Count == 0)
                    {
                        Log.Error(string.Format(Resources.MissingArguments, "0publish"));
                        return (int)ErrorLevel.InvalidArguments;
                    }

                    ModifyFeeds(results);
                    return (int)ErrorLevel.OK;

                case OperationMode.Catalog:
                    // Default to using all XML files in the current directory
                    if (results.Feeds.Count == 0)
                        results.Feeds = ArgumentUtils.GetFiles(new[] { Environment.CurrentDirectory }, "*.xml");

                    CreateCatalog(results);

                    return (int)ErrorLevel.OK;

                case OperationMode.Version:
                    Console.WriteLine(@"Zero Install Publish CLI v{0}", Assembly.GetEntryAssembly().GetName().Version);
                    return (int)ErrorLevel.OK;

                case OperationMode.Help:
                    return (int)ErrorLevel.OK;

                default:
                    Log.Error(Resources.UnknownMode);
                    return (int)ErrorLevel.NotSupported;
            }
        }
        #endregion

        //--------------------//

        #region Modify feeds
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <param name="results">The parser results to be executed.</param>
        /// <exception cref="InvalidOperationException">Thrown if the feed file is damaged.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the feed file could not be found.</exception>
        /// <exception cref="IOException">Thrown if a file could not be read or written or if the GnuPG could not be launched or the feed file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the feed file is not permitted.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        public static void ModifyFeeds(ParseResults results)
        {
            foreach (var file in results.Feeds)
            {
                bool wasSigned = false;

                var feed = Feed.Load(file.FullName);

                // ToDo: Apply modifications

                feed.Save(file.FullName);

                // Always remove existing signatures since they will become invalid if anything is changed
                FeedUtils.UnsignFeed(file.FullName);
                FeedUtils.AddStylesheet(file.FullName);

                if ((wasSigned && !results.Unsign) || results.XmlSign)
                {
                    if (string.IsNullOrEmpty(results.GnuPGPassphrase))
                        results.GnuPGPassphrase = CliUtils.ReadPassword(Resources.PleaseEnterGnuPGPassphrase);

                    FeedUtils.SignFeed(file.FullName, results.Key, results.GnuPGPassphrase);
                }
            }
        }
        #endregion

        #region Catalog
        /// <summary>
        /// Creates a <see cref="Catalog"/> from the <see cref="Feed"/>s specified in the command-line arguments.
        /// </summary>
        /// <param name="results">The parser results to be executed.</param>
        /// <exception cref="ArgumentException">Throw if the specified feed file paths were invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a feed file is damaged.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the a files could not be found.</exception>
        /// <exception cref="IOException">Thrown if a file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to a feed file or the catalog file is not permitted.</exception>
        public static void CreateCatalog(ParseResults results)
        {
            var catalog = new Catalog();

            foreach (var feed in results.Feeds)
                catalog.Feeds.Add(Feed.Load(feed.FullName));

            if (catalog.Feeds.IsEmpty) throw new FileNotFoundException(Resources.NoFeedFilesFound);

            catalog.Simplify();
            catalog.Save(results.CatalogFile);
            XmlStorage.AddStylesheet(results.CatalogFile, "catalog.xsl");
        }
        #endregion
    }
}
