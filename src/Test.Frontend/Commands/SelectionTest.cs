/*
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

using Common.Storage;
using NDesk.Options;
using NUnit.Framework;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Contains code for testing <see cref="Selection"/>.
    /// </summary>
    [TestFixture]
    public class SelectionTest : CommandBaseTest
    {
        /// <inheritdoc/>
        protected override CommandBase GetCommand()
        {
            return new Selection(Policy);
        }

        [Test(Description = "Ensures all options are parsed and handled correctly.")]
        public virtual void TestNormal()
        {
            var requirements = RequirementsTest.CreateTestRequirements();
            var selections = SelectionsTest.CreateTestSelections();

            SolverMock.ExpectAndReturn("Solve", selections, requirements, Policy, false); // First and only Solve()
            var args = new[] {"--xml", "http://0install.de/feeds/test/test1.xml", "--command=\"command name\"", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0"};
            AssertParseExecuteResult(args, selections.WriteToString(), 0);
        }

        [Test(Description = "Ensures local Selections XMLs are correctly detected and parsed.")]
        public virtual void TestImportSelections()
        {
            var selections = SelectionsTest.CreateTestSelections();
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                selections.Save(tempFile.Path);
                var args = new[] {"--xml", tempFile.Path};
                AssertParseExecuteResult(args, selections.WriteToString(), 0);
            }
        }

        [Test(Description = "Ensures invalid feed IDs are correctly detected and handled.")]
        public void TestInvalidFeedID()
        {
            // ToDo
        }

        [Test(Description = "Ensures invalid feeds are correctly detected and handled.")]
        public void TestInvalidFeed()
        {
            // ToDo
        }

        [Test(Description = "Ensures calling with no arguments raises an exception.")]
        public void TestNoArgs()
        {
            Assert.Throws<InvalidInterfaceIDException>(() => Command.Parse(new string[0]), "Should reject empty argument list");
        }

        [Test(Description = "Ensures calling with too many arguments raises an exception.")]
        public void TestTooManyArgs()
        {
            Command.Parse(new[] { "http://0install.de/feeds/test/test1.xml", "arg1" });
            Assert.Throws<OptionException>(() => Command.Execute(), "Should reject more than one argument");
        }
    }
}
