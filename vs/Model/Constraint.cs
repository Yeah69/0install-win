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
using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Restrict the set of versions from which the injector may choose an <see cref="Implementation"/>. 
    /// </summary>
    [TypeConverter(typeof(ConstraintConverter))]
    public struct Constraint : IEquatable<Constraint>
    {
        #region Properties
        /// <summary>
        /// This is the lowest-numbered version that can be chosen.
        /// </summary>
        [Description("This is the lowest-numbered version that can be chosen.")]
        [XmlAttribute("not-before")]
        public string NotBeforeVersion { get; set; }

        /// <summary>
        /// This version and all later versions are unsuitable.
        /// </summary>
        [Description("This version and all later versions are unsuitable.")]
        [XmlAttribute("before")]
        public string BeforeVersion { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new constraint sturcture with pre-set values.
        /// </summary>
        /// <param name="notBeforeVersion">This is the lowest-numbered version that can be chosen.</param>
        /// <param name="beforeVersion">This version and all later versions are unsuitable.</param>
        public Constraint(string notBeforeVersion, string beforeVersion) : this()
        {
            NotBeforeVersion = notBeforeVersion;
            BeforeVersion = beforeVersion;
        }
        #endregion

        //--------------------//

        #region Conversion
        public override string ToString()
        {
            return string.Format("{0}  =< Ver < {1}", NotBeforeVersion, BeforeVersion);
        }
        #endregion

        #region Equality
        public bool Equals(Constraint other)
        {
            return other.NotBeforeVersion == NotBeforeVersion && other.BeforeVersion == BeforeVersion;
        }

        public static bool operator ==(Constraint left, Constraint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Constraint left, Constraint right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return obj.GetType() == typeof(Constraint) && Equals((Constraint)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((NotBeforeVersion != null ? NotBeforeVersion.GetHashCode() : 0) * 397) ^ (BeforeVersion != null ? BeforeVersion.GetHashCode() : 0);
            }
        }
        #endregion
    }
}
