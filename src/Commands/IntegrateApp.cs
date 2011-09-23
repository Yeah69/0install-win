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
using System.Diagnostics.CodeAnalysis;
using Common.Storage;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Injector;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Add an application to the application list (if missing) and integrate it into the desktop environment.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 types only need to be disposed when using snapshots")]
    [CLSCompliant(false)]
    public sealed class IntegrateApp : AppCommand
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public const string Name = "integrate-app";
        #endregion

        #region Variables
        /// <summary>A list of all <see cref="AccessPoint"/> categories to be added to the already applied ones.</summary>
        private readonly C5.ICollection<string> _addCategories = new C5.LinkedList<string>();

        /// <summary>A list of all <see cref="AccessPoint"/> categories to be removed from the already applied ones.</summary>
        private readonly C5.ICollection<string> _removeCategories = new C5.LinkedList<string>();
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionIntegrateApp; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public IntegrateApp(Policy policy) : base(policy)
        {
            string categoryList = StringUtils.Concatenate(CategoryIntegrationManager.Categories, ", ");

            Options.Add("a|add=", Resources.OptionAppAdd + "\n" + Resources.OptionAppCategory + categoryList + "\n" + string.Format(Resources.OptionAppImplicitCategory, CapabilityRegistration.CategoryName), category =>
            {
                category = category.ToLower();
                if (!CategoryIntegrationManager.Categories.Contains(category)) throw new OptionException(string.Format(Resources.UnknownCategory, category), "add");
                _addCategories.Add(category);
            });
            Options.Add("x|remove=", Resources.OptionAppRemove + "\n" + Resources.OptionAppCategory + categoryList, category =>
            {
                category = category.ToLower();
                if (!CategoryIntegrationManager.Categories.Contains(category)) throw new OptionException(string.Format(Resources.UnknownCategory, category), "remove");
                _removeCategories.Add(category);
            });
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            if (Locations.IsPortable) throw new NotSupportedException(Resources.NotAvailableInPortableMode);

            return base.Execute();
        }

        /// <inheritdoc/>
        protected override int ExecuteHelper(CategoryIntegrationManager integrationManager, string interfaceID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            #endregion

            var appEntry = GetAppEntry(integrationManager, interfaceID);

            // If user specified no specific integration options show UI
            if (_addCategories.IsEmpty && _removeCategories.IsEmpty)
            {
                Policy.Handler.ShowIntegrateApp(integrationManager, appEntry, GetFeed(interfaceID));
                return 0;
            }

            if (!_removeCategories.IsEmpty)
                integrationManager.RemoveAccessPointCategories(appEntry, _removeCategories);

            if (!_addCategories.IsEmpty)
            {
                try
                {
                    integrationManager.AddAccessPointCategories(appEntry, GetFeed(interfaceID), _addCategories, Policy.Handler);
                }
                    #region Error handling
                catch (InvalidOperationException ex)
                {
                    // Show a "failed to comply" message (but not in batch mode, since it is too unimportant)
                    if (!Policy.Handler.Batch) Policy.Handler.Output(Resources.AppList, ex.Message);
                    return 2;
                }
                #endregion

                // Show a "integration complete" message with application name (but not in batch mode, since it is too unimportant)
                if (!Policy.Handler.Batch) Policy.Handler.Output(Resources.DesktopIntegration, string.Format(Resources.DesktopIntegrationDone, appEntry.Name));
                return 0;
            }

            // Show a "integration complete" message without application name (but not in batch mode, since it is too unimportant)
            if (!Policy.Handler.Batch) Policy.Handler.Output(Resources.DesktopIntegration, string.Format(Resources.DesktopIntegrationDone, appEntry.Name));
            return 0;
        }
        #endregion

        #region Helpers
        private AppEntry GetAppEntry(IIntegrationManager integrationManager, string interfaceID)
        {
            // ToDo: Reduce need for feed loading
            var feed = GetFeed(interfaceID);

            try
            {
                var appEntry = integrationManager.AppList.GetEntry(interfaceID);

                if (!appEntry.CapabilityLists.UnsequencedEquals(feed.CapabilityLists))
                {
                    if (Policy.Handler.AskQuestion("Update stuff?", "Stuff updated"))
                        integrationManager.UpdateApp(appEntry, feed);
                }

                return appEntry;
            }
            catch (KeyNotFoundException)
            {
                // Automatically add missing AppEntry
                return integrationManager.AddApp(interfaceID, feed);
            }
        }
        #endregion
    }
}
