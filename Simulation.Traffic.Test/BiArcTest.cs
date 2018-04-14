using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Traffic.Lofts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Simulation.Traffic.Test
{
    [TestClass]
    public class BiArcTest
    {
        private static Regex regex = new Regex(@"\[\s*(\S+)\s+(\S+)\s+(\S+)\s*]\s+\[\s*(\S+)\s+(\S+)\s+(\S+)\s*]\s+\[\s*(\S+)\s+(\S+)\s+(\S+)\s*]\s+\[\s*(\S+)\s+(\S+)\s+(\S+)\s*]\s+\[\s*(\S+)\s+(\S+)\s+(\S+)\s*]\s+(\S+)\s*(\S+)");
        private static ArcDefinition Arc(string pyData)
        {
            var m = regex.Match(pyData);

            var data = new float[17];
            for (int i = 0; i < data.Length; i++)
                data[i] = (float)double.Parse(m.Groups[i + 1].Value, CultureInfo.InvariantCulture);

            return new ArcDefinition(
                 center: new Vector3(data[0], data[1], data[2]),
                 e1: new Vector3(data[3], data[4], data[5]),
                 e2: new Vector3(data[6], data[7], data[8]),
                 endDir: new Vector3(data[9], data[10], data[11]),
                 axis: new Vector3(data[12], data[13], data[14]),
                 r: data[15],
                 theta: data[16]
                 );
        }

        private Vector3[][] dataSets = new Vector3[][]
        {
            new Vector3[]
            {
                new Vector3(0,0,0),
                new Vector3(1,1,0),
                new Vector3(-2,1,2),
                new Vector3(3,2,0),
                new Vector3(-1, -2, 3)
            }
        };

        private (ArcDefinition, ArcDefinition)[] arcs = new(ArcDefinition, ArcDefinition)[]
        {
            (Arc("[ 1.1169564 -1.1169564 -1.4892752] [-0.51449576  0.51449576  0.68599434] [7.07106781e-01 7.07106781e-01 2.22044605e-16] [-0.10608095  0.79326522  0.59956411] [-0.48507125  0.48507125 -0.72760688] 2.170973007924428 0.2537035435685886"),
            Arc("[ 0.86051018  0.80072882 -0.03985424] [ 0.10608095 -0.79326522 -0.59956411] [ 0.86801654  0.36801654 -0.33333333] [0.56591646 0.80845208 0.16169042] [ 0.48507125 -0.48507125  0.72760688] 0.2464848308371891 1.158072269381141")),
            (Arc("[-1.38006955 -1.79443368 -0.98285271] [0.62634298 0.73538772 0.25864912] [-0.66666667  0.33333333  0.66666667] [-0.16002788  0.70681881  0.68905605] [ 0.40404211 -0.58999474  0.69903947] 3.7999460705526467 0.4650353668821312"),
            Arc("[-1.97687437  0.95374875  1.81592885] [-0.0590622  -0.32588988 -0.94356104] [-0.9006337  -0.39027346  0.19116896] [-0.12095204  0.24190409  0.96273206] [-0.43054687  0.86109373 -0.27045699] 0.19119666079169817 1.4708473987914097")),
            (Arc("[-2.56245511  2.12491022 -3.42789315] [ 0.10094863 -0.20189725  0.97419037] [ 8.94427191e-01  4.47213595e-01 -2.08166817e-16] [0.65953208 0.13836596 0.73883171] [-0.43567118  0.87134236  0.22572799] 5.571696579178248 0.35496623942942773"),
            Arc("[1.92891608 1.88542085 0.72734648] [-0.62831377  0.77739023 -0.02976968] [ 0.61254229  0.47076253 -0.6349603 ] [ 0.82406138  0.08815393 -0.55959961] [-0.47959748 -0.41718949 -0.77197098] 1.2997623050856755 1.0090743773239252")),
            (Arc("[1.05235448 1.6104709  0.77905821] [ 0.91287093  0.18257419 -0.36514837] [ 0.30151134 -0.90453403  0.30151134] [ 0.39336036 -0.88109733  0.26255497] [-0.27524094 -0.38533732 -0.88077101] 2.133538771287758 0.7341333452554724"),
            Arc("[ 5.26288606 -7.82079998  3.5894481 ] [-0.39336036  0.88109733 -0.26255497] [-0.87721722 -0.27419453  0.39409049] [-0.73075202  0.67916953 -0.06877666] [0.27524094 0.38533732 0.88077101] 8.570466924611276 0.22097308694951703"))
        };

        [DataRow(0)]
        [DataTestMethod]
        public void TestCase(int set)
        {
            var points = dataSets[set];
            var n = points.Length;

            var tangents = new Vector3[n];
            tangents[0] = points[1] - points[0];

            for (int i = 1; i < n - 1; i++)
                tangents[i] = points[i + 1] - points[i - 1];

            tangents[n - 1] = points[n - 1] - points[n - 2];

            for (int i = 0; i < n; i++)
                tangents[i] = Vector3.Normalize(tangents[i]);


            for (int i = 1; i < n; i++)
            {
                var parameters = new BiArcParameters(points[i - 1], tangents[i - 1], points[i], tangents[i]);
                var (arc1, arc2) = BiArcGenerator.Form1(parameters, 1.0f);

                var (pyArc1, pyArc2) = arcs[i-1];

                check(pyArc1, arc1);
                check(pyArc2, arc2);
            }
        }

        private void check(ArcDefinition pyArc2, ArcDefinition arc2)
        {
            check(pyArc2.Center, arc2.Center);
            check(pyArc2.E1, arc2.E1);
            check(pyArc2.E2, arc2.E2);
            check(pyArc2.Axis, arc2.Axis);
            check(pyArc2.EndDir, arc2.EndDir);
            Assert.AreEqual(pyArc2.R, arc2.R, delta);
            Assert.AreEqual(pyArc2.Theta, arc2.Theta, delta);
        }
        private const float delta = 0.00001f;
        private void check(Vector3 center1, Vector3 center2)
        {
            Assert.AreEqual(center1.X, center2.X, delta);
            Assert.AreEqual(center1.Y, center2.Y, delta);
            Assert.AreEqual(center1.Z, center2.Z, delta);
        }
    }
}
