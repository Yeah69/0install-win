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
using NUnit.Framework;
using NUnit.Mocks;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Contains test methods for <see cref="Selections"/>.
    /// </summary>
    [TestFixture]
    public class SelectionsTest
    {
        #region Helpers
        /// <summary>
        /// Creates a <see cref="Selections"/> with two implementations, one using the other as a runner plus a number of bindings.
        /// </summary>
        public static Selections CreateTestSelections()
        {
            return new Selections
            {
                InterfaceID = "http://0install.de/feeds/test/test1.xml",
                Implementations = {ImplementationSelectionTest.CreateTestImplementation1(), ImplementationSelectionTest.CreateTestImplementation2()},
                Commands = {CommandTest.CreateTestCommand1(), CommandTest.CreateTestCommand2()}
            };
        }
        #endregion

        [Test(Description = "Ensures that the class is correctly serialized and deserialized.")]
        public void TestSaveLoad()
        {
            var selections1 = CreateTestSelections();

            // Serialize and deserialize data
            string data = selections1.WriteToString();
            var selections2 = Selections.LoadFromString(data);

            // Ensure data stayed the same
            Assert.AreEqual(selections1, selections2, "Serialized objects should be equal.");
            Assert.AreEqual(selections1.GetHashCode(), selections2.GetHashCode(), "Serialized objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(selections1, selections2), "Serialized objects should not return the same reference.");
        }

        [Test(Description = "Ensures that the class can be correctly cloned.")]
        public void TestClone()
        {
            var selections1 = CreateTestSelections();
            var selections2 = selections1.CloneSelections();

            // Ensure data stayed the same
            Assert.AreEqual(selections1, selections2, "Cloned objects should be equal.");
            Assert.AreEqual(selections1.GetHashCode(), selections2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(selections1, selections2), "Cloning should not return the same reference.");
        }

        [Test(Description = "Ensures that Selections.ListUncachedImplementations() correctly finds Implementations not cached in a store")]
        public void TestListUncachedImplementations()
        {
            var cacheMock = new DynamicMock("MockCache", typeof(IFeedCache));
            var storeMock = new DynamicMock("StoreCache", typeof(IStore));

            var feed = FeedTest.CreateTestFeed();
            var selections = CreateTestSelections();

            // Pretend the first implementation isn't cached but the second is
            cacheMock.ExpectAndReturn("GetFeed", feed, new Uri("http://0install.de/feeds/test/sub1.xml"));
            storeMock.ExpectAndReturn("Contains", false, selections.Implementations[0].ManifestDigest);
            storeMock.ExpectAndReturn("Contains", true, selections.Implementations[1].ManifestDigest);

            var implementations = selections.ListUncachedImplementations((IStore)storeMock.MockInstance, (IFeedCache)cacheMock.MockInstance);

            // Only the first implementation should be listed as uncached
            CollectionAssert.AreEquivalent(new[] {feed.Elements[0]}, implementations);
        }
    }
}
