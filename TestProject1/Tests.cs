using System;
using System.Collections.Generic;
using System.Linq;
using CarbonCI;
using NUnit.Framework;
using Semver;

namespace TestProject1
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void TestAlphaReleases()
        {
            var testList = new List<SemVersion>();
            SemVersion version = new SemVersion(1,5,6);
            for (int i = 0; i < 8; i++)
            {
                version = version.incrementAlpha();
                testList.Add(version);
            }

            var pass = true;
            foreach (var testObj in testList.Where(testObj => !testObj.Prerelease.Contains("alpha")))
            {
                pass = false;
            }
            Assert.IsTrue(pass);
        }
    }
}