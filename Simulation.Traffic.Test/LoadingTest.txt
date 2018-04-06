using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Traffic.Lofts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Traffic.Test
{
    [TestClass]
    public class LoadingTest
    {
        [TestMethod]
        public async Task TestLoading()
        {
            var file = "mapOutput.json";

            using (var stream = File.OpenRead(file))
            {
                var roadManager = new RoadManager();

                var loader = new Traffic.IO.RoadsReader(roadManager, stream, false);

                await Task.Run((Action)loader.ReadAll);

            }
        }
        [TestMethod]
        public async Task TestPaths()
        {
            var file = "mapOutput.json";
            var roadManager = new RoadManager();

            using (var stream = File.OpenRead(file))
            {

                var loader = new Traffic.IO.RoadsReader(roadManager, stream, false);

                await Task.Run((Action)loader.ReadAll); 
            }
 
        }

        private void IsValid(ILoftPath value)
        {
            Assert.IsTrue(value.Length > 0, "length > 0");
            Assert.IsFalse(float.IsNaN(value.Length), "length is NaN");
            Assert.IsFalse(float.IsInfinity(value.Length), "length is Infinity");

            if (value is ArcLoftPath)
                IsValid(value as ArcLoftPath);
            if (value is BiArcLoftPath)
                IsValid(value as BiArcLoftPath);
        }

        private void IsValid(BiArcLoftPath value)
        {
            IsValid(value.Arc1);
            IsValid(value.Arc2); 
        }
        private void IsValid(ArcLoftPath value)
        {
            Assert.IsTrue(value.Radius > 0, "radius > 0");
            Assert.IsFalse(float.IsNaN(value.Radius), "length is NaN");
            Assert.IsFalse(float.IsInfinity(value.Radius), "length is Infinity");
        }
    }
}
