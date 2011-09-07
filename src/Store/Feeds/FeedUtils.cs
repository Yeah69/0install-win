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
using System.Collections.Generic;
using System.IO;
using Common;
using Common.Cli;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Provides utiltity methods for managing <see cref="Feed"/>s.
    /// </summary>
    public static class FeedUtils
    {
        /// <summary>
        /// Loads all <see cref="Feed"/>s stored in <see cref="IFeedCache"/> into memory.
        /// </summary>
        /// <param name="cache">The <see cref="IFeedCache"/> to load <see cref="Feed"/>s from.</param>
        /// <returns>The parsed <see cref="Feed"/>s.</returns>
        public static IEnumerable<Feed> GetFeeds(IFeedCache cache)
        {
            #region Sanity checks
            if (cache == null) throw new ArgumentNullException("cache");
            #endregion

            var feeds = new LinkedList<Feed>();
            foreach (string id in cache.ListAll())
            {
                try
                {
                    var feed = cache.GetFeed(id);
                    feed.Simplify();
                    feeds.AddLast(feed);
                }
                #region Error handling
                catch (IOException ex) { Log.Error(ex.Message); }
                catch (UnauthorizedAccessException ex) { Log.Error(ex.Message); }
                catch (InvalidDataException ex) { Log.Error(ex.Message); }
                #endregion
            }
            return feeds;
        }

        /// <summary>
        /// Determines which signatures a feed is signed with.
        /// </summary>
        /// <param name="openPgp">The OpenPGP-compatible system used to validate the signatures.</param>
        /// <param name="data">The feed data containing an embedded signature.</param>
        /// <returns>A list of signatures found, both valid and invalid.</returns>
        /// <exception cref="IOException">Thrown if the OpenPGP implementation could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        public static OpenPgpSignature[] GetSignatures(IOpenPgp openPgp, Stream data)
        {
            // ToDo: Split data into pureData and signature
            Stream pureData = null;
            byte[] signature = null;

            return openPgp.Verify(pureData, signature);
        }
    }
}