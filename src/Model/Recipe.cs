﻿/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// A recipe is a list of <see cref="IRecipeStep"/>s used to create an <see cref="Model.Implementation"/> directory.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    [XmlRoot("recipe", Namespace = Feed.XmlNamespace), XmlType("recipe", Namespace = Feed.XmlNamespace)]
    public sealed class Recipe : RetrievalMethod, IEquatable<Recipe>
    {
        #region Properties
        // Preserve order
        private readonly C5.ArrayList<IRecipeStep> _steps = new C5.ArrayList<IRecipeStep>();

        /// <summary>
        /// An ordered list of <see cref="IRecipeStep"/>s to execute.
        /// </summary>
        [Description("An ordered list of archives to extract.")]
        [XmlIgnore]
        public C5.ArrayList<IRecipeStep> Steps { get { return _steps; } }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Steps"/>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Used for XML serialization.")]
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement(typeof(Archive)), XmlElement(typeof(SingleFile)), XmlElement(typeof(RenameStep)), XmlElement(typeof(RemoveStep))]
        public object[] StepsArray
        {
            get
            {
                // ReSharper disable CoVariantArrayConversion
                return _steps.ToArray();
                // ReSharper restore CoVariantArrayConversion
            }
            set
            {
                _steps.Clear();
                if (value != null && value.Length > 0) _steps.AddAll(value.OfType<IRecipeStep>());
            }
        }

        /// <summary>
        /// Indicates whether this recipe contains steps of unknown type and therefore can not be processed.
        /// </summary>
        [Description("Indicates whether this recipe contains steps of unknown type and therefore can not be processed.")]
        [XmlIgnore]
        public bool ContainsUnknownSteps { get { return UnknownElements != null && UnknownElements.Length > 0; } }
        #endregion

        //--------------------//

        #region Normalize
        /// <summary>
        /// Call <see cref="RetrievalMethod.Normalize"/> on all contained <see cref="IRecipeStep"/>s.
        /// </summary>
        /// <remarks>This method should be called to prepare a <see cref="Feed"/> for solver processing.
        /// It should not be called if you plan on serializing the feed again since it will may loose some of its structure.</remarks>
        public override void Normalize()
        {
            // Simplify recipe steps and rebuild list to update sequenced hash value
            var newSteps = new IRecipeStep[Steps.Count];
            int i = 0;
            foreach (var step in Steps)
            {
                step.Normalize();
                newSteps[i++] = step;
            }
            Steps.Clear();
            Steps.AddAll(newSteps);
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the archive in the form "Archive: Location (MimeType, Size + StartOffset) => Extract". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("Recipe: {0} Archives", Steps.Count);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Recipe"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Recipe"/>.</returns>
        public override RetrievalMethod Clone()
        {
            var recipe = new Recipe {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements};
            foreach (var step in Steps)
                recipe.Steps.Add(step.CloneRecipeStep());

            return recipe;
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Recipe other)
        {
            if (other == null) return false;
            return base.Equals(other) && Steps.SequencedEquals(other.Steps);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Recipe && Equals((Recipe)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Steps.GetSequencedHashCode();
            }
        }
        #endregion
    }
}
