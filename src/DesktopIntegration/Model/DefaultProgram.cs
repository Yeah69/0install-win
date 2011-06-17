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
using System.Xml.Serialization;
using ZeroInstall.Model;

namespace ZeroInstall.DesktopIntegration.Model
{
    /// <summary>
    /// Makes an application a default program of some kind (e.g. default web-browser, default e-mail client, ...).
    /// </summary>
    /// <seealso cref="ZeroInstall.Model.Capabilities.DefaultProgram"/>
    [XmlType("default-program", Namespace = AppList.XmlNamespace)]
    public class DefaultProgram : DefaultAccessPoint, IEquatable<DefaultProgram>
    {
        #region Apply
        /// <inheritdoc/>
        public override void Apply(AppEntry appEntry, Feed feed, bool systemWide)
        {
            // ToDo: Implement
        }

        /// <inheritdoc/>
        public override void Unapply(AppEntry appEntry, bool systemWide)
        {
            // ToDo: Implement
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the access point in the form "DefaultProgram". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("DefaultProgram");
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override AccessPoint CloneAccessPoint()
        {
            return new DefaultProgram {Capability = Capability};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(DefaultProgram other)
        {
            if (other == null) return false;

            return base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(DefaultProgram) && Equals((DefaultProgram)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
