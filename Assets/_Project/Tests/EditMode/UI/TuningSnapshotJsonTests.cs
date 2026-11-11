using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.UI;

namespace UncannyValet.Tests.UI
{
    public class TuningSnapshotJsonTests
    {
        private static string BuildAll() => TuningSnapshotJson.Build(TuningCatalog.All, info => info.Min);

        [Test]
        public void Build_IsAJsonObjectWithEveryCatalogKey()
        {
            var json = BuildAll();
            StringAssert.StartsWith("{", json);
            StringAssert.EndsWith("}", json);
            for (var i = 0; i < TuningCatalog.All.Count; i++)
                StringAssert.Contains("\"" + TuningCatalog.All[i].Key + "\": ", json);
        }

        [Test]
        public void Build_EveryLineIsAKeyNumberPair()
        {
            var lines = BuildAll().Split('\n');
            Assert.AreEqual(TuningCatalog.All.Count + 2, lines.Length);
            var pair = new Regex("^  \"\\w+\": -?\\d+(\\.\\d+)?([eE][-+]?\\d+)?,?$");
            for (var i = 1; i < lines.Length - 1; i++) StringAssert.IsMatch(pair.ToString(), lines[i]);
        }

        [Test]
        public void Build_HasNoDuplicateKeys()
        {
            var seen = new HashSet<string>();
            foreach (Match m in Regex.Matches(BuildAll(), "\"(\\w+)\":"))
                Assert.IsTrue(seen.Add(m.Groups[1].Value), m.Groups[1].Value);
            Assert.AreEqual(TuningCatalog.All.Count, seen.Count);
        }

        [Test]
        public void Build_OrdersByEnumValue()
        {
            var last = -1;
            foreach (Match m in Regex.Matches(BuildAll(), "\"(\\w+)\":"))
            {
                var value = (int)Enum.Parse(typeof(TuningKey), m.Groups[1].Value);
                Assert.Greater(value, last);
                last = value;
            }
        }

        [Test]
        public void Build_UsesDotDecimalsUnderACommaCulture()
        {
            var old = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            try
            {
                var json = TuningSnapshotJson.Build(TuningCatalog.All, info => 1.5f);
                StringAssert.Contains(": 1.5", json);
                StringAssert.DoesNotContain("1,5", json);
            }
            finally { CultureInfo.CurrentCulture = old; }
        }

        [Test]
        public void Build_WritesWholeNumbersPlainAndNonFiniteAsNull()
        {
            var rows = new List<TuningInfo> { TuningCatalog.All[0] };
            StringAssert.Contains(": 12\n", TuningSnapshotJson.Build(rows, _ => 12f));
            StringAssert.Contains(": null\n", TuningSnapshotJson.Build(rows, _ => float.NaN));
        }
    }
}
