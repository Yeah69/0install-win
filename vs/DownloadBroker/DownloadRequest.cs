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

using System.Collections.Generic;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.DownloadBroker
{
    /// <summary>
    /// Handles the download of one or more <see cref="Implementation"/>s into an <see cref="IImplementationProvider"/>.
    /// </summary>
    public class DownloadRequest
    {
        #region Constructor
        /// <summary>
        /// Creates a new download request.
        /// </summary>
        /// <param name="implementations">The <see cref="Implementation"/>s to be downloaded.</param>
        /// <param name="provider">The location to store the downloaded and unpacked <see cref="Implementation"/>s in.</param>
        public DownloadRequest(IEnumerable<Implementation> implementations, IImplementationProvider provider)
        {
            // ToDo: Implement
        }
        #endregion

        //--------------------//

        #region Run
        public void RunSync()
        {
            // ToDo: Implement
        }
        #endregion
    }
}
