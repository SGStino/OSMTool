using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Traffic.Lofts;
using Simulation.Traffic.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Simulation.Data.Primitives;

namespace Simulation.Traffic.Test
{
    [TestClass]
    public class LoftPathTests
    {
        private float precision = 0.001f;
        public Vector3 fromString3(string bytes)
        {
            var b = bytes.Split(' ');
            return new Vector3(fromString(b[0]), fromString(b[1]), fromString(b[2]));
        }

        public float fromString(string bytes)
        {
            var data = Convert.FromBase64String(bytes);
            return BitConverter.ToSingle(data, 0);
        }

        [TestMethod]
        public void TestJShape()
        {
            var tangentStart = new Vector3(0, 0, 1);
            var tangentEnd = new Vector3(1, 0, 0);

            var start = new Vector3(0, 0, 2);
            var end = new Vector3(-1, 0, 0);

            var a = new BiArcLoftPath(start, tangentStart, end, tangentEnd);

            var points = string.Join(" ", Enumerable.Range(0, 100).Select(i => i / 100.0f).Select(i => a.GetTransformedPoint(i * a.Length, Vector3.Zero))
              .Select(t => "(" + t.X.ToString(CultureInfo.InvariantCulture) + "," + t.Z.ToString(CultureInfo.InvariantCulture) + ")"));

        }
        [TestMethod]
        public void TestReverseCShape()
        {
            // data from actual bogus case:
            var tangentStart = new Vector3(1, 0, 0);
            var tangentEnd = new Vector3(-1, 0, 0);

            var start = new Vector3(0, 0, 2);
            var end = new Vector3(0, 0, -2);

            var a = new BiArcLoftPath(start, tangentStart, end, tangentEnd);

            var p1 = a.GetTransformedPoint(a.Length / 4, Vector3.Zero);
            var p2 = a.GetTransformedPoint(a.Length / 2, Vector3.Zero);
            var p3 = a.GetTransformedPoint(a.Length / 4 * 3, Vector3.Zero);

            var points = string.Join(" ", Enumerable.Range(0, 100).Select(i => i / 100.0f).Select(i => a.GetTransformedPoint(i * a.Length, Vector3.Zero))
                .Select(t => "(" + t.X.ToString(CultureInfo.InvariantCulture) + "," + t.Z.ToString(CultureInfo.InvariantCulture) + ")"));

            var s2 = MathF.Sqrt(2);

            Assert.AreEqual(new Vector3(-s2, 0, s2).Round(precision), p1.Round(precision));
            Assert.AreEqual(new Vector3(-2, 0, 0).Round(precision), p2.Round(precision));
            Assert.AreEqual(new Vector3(-s2, 0, -s2).Round(precision), p3.Round(precision));
        }

        [TestMethod]
        public void TestReverseSShape()
        {
            // data from actual bogus case:
            var tangentStart = new Vector3(-1, 0, 0);
            var tangentEnd = new Vector3(-1, 0, 0);

            var start = new Vector3(0, 0, 2);
            var end = new Vector3(0, 0, -2);

            var a = new BiArcLoftPath(start, tangentStart, end, tangentEnd);

            var p1 = a.GetTransformedPoint(a.Length / 4, Vector3.Zero);
            var p2 = a.GetTransformedPoint(a.Length / 2, Vector3.Zero);
            var p3 = a.GetTransformedPoint(a.Length / 4 * 3, Vector3.Zero);

            var points = string.Join(" ", Enumerable.Range(0, 100).Select(i => i / 100.0f).Select(i => a.GetTransformedPoint(i * a.Length, Vector3.Zero))
                .Select(t => "(" + t.X.ToString(CultureInfo.InvariantCulture) + "," + t.Z.ToString(CultureInfo.InvariantCulture) + ")"));

            Assert.AreEqual(new Vector3(1, 0, 1), p1.Round(precision));
            Assert.AreEqual(new Vector3(0, 0, 0), p2.Round(precision));
            Assert.AreEqual(new Vector3(-1, 0, -1), p3.Round(precision));
        }

        [TestMethod]
        public void TestSShape()
        {
            // data from actual bogus case:
            var tangentStart = new Vector3(1, 0, 0);// fromString3("AACAvw== AAAAgA== AAAAgA==");
            var tangentEnd = new Vector3(1, 0, 0);// fromString3("AACAPw== AAAAgA== AAAAgA==");

            var start = new Vector3(0, 0, 2);// fromString3("AMAuRA== AAAAAA== VVXtQg==") - new Vector3(699,0,116.6667f);
            var end = new Vector3(0, 0, -2);// fromString3("AMAuRA== AAAAAA== VVXlQg==") - new Vector3(699, 0, 116.6667f);

            var a = new BiArcLoftPath(start, tangentStart, end, tangentEnd);

            var p1 = a.GetTransformedPoint(a.Length / 4, Vector3.Zero);
            var p2 = a.GetTransformedPoint(a.Length / 2, Vector3.Zero);
            var p3 = a.GetTransformedPoint(a.Length / 4 * 3, Vector3.Zero);

            var points = string.Join(" ", Enumerable.Range(0, 100).Select(i => i / 100.0f).Select(i => a.GetTransformedPoint(i * a.Length, Vector3.Zero))
                .Select(t => "(" + t.X.ToString(CultureInfo.InvariantCulture) + "," + t.Z.ToString(CultureInfo.InvariantCulture) + ")"));

            Assert.AreEqual(new Vector3(-1, 0, 1).Round(precision), p1.Round(precision));
            Assert.AreEqual(new Vector3(0, 0, 0).Round(precision), p2.Round(precision));
            Assert.AreEqual(new Vector3(1, 0, -1).Round(precision), p3.Round(precision));
        }
        [TestMethod]
        public void TestCShape()
        {
            // data from actual bogus case:
            var tangentStart = new Vector3(-1, 0, 0);// fromString3("AACAvw== AAAAgA== AAAAgA==");
            var tangentEnd = new Vector3(1, 0, 0);// fromString3("AACAPw== AAAAgA== AAAAgA==");

            var start = new Vector3(0, 0, 2);// fromString3("AMAuRA== AAAAAA== VVXtQg==") - new Vector3(699,0,116.6667f);
            var end = new Vector3(0, 0, -2);// fromString3("AMAuRA== AAAAAA== VVXlQg==") - new Vector3(699, 0, 116.6667f);

            var a = new BiArcLoftPath(start, tangentStart, end, tangentEnd);

            var p1 = a.GetTransformedPoint(a.Length / 4, Vector3.Zero);
            var p2 = a.GetTransformedPoint(a.Length / 2, Vector3.Zero);
            var p3 = a.GetTransformedPoint(a.Length / 4 * 3, Vector3.Zero);

            var points = string.Join(" ", Enumerable.Range(0, 100).Select(i => i / 100.0f).Select(i => a.GetTransformedPoint(i * a.Length, Vector3.Zero))
                .Select(t => "(" + t.X.ToString(CultureInfo.InvariantCulture) + "," + t.Z.ToString(CultureInfo.InvariantCulture) + ")"));
            var s2 = MathF.Sqrt(2);
            Assert.AreEqual(new Vector3(s2, 0, s2).Round(precision), p1.Round(precision));
            Assert.AreEqual(new Vector3(2, 0, 0).Round(precision), p2.Round(precision));
            Assert.AreEqual(new Vector3(s2, 0, -s2).Round(precision), p3.Round(precision));
        }

        [TestMethod]
        public void TestSnapLinear()
        {
            var line = new LinearPath(Vector3.Zero, Directions3.Forward * 10);

            var point = new Vector3(15, 0, 5);

            line.SnapTo(point, out var snapped, out var distance);

            Assert.AreEqual(new Vector3(0, 0, 5), snapped);
            Assert.AreEqual(5, distance);

        }

        [TestMethod]
        public void TestBiArc()
        {
            var biArc = new BiArcLoftPath(new Vector3(-1, 0, -1), new Vector3(0, 0, -1), new Vector3(1, 0, 1), new Vector3(0, 0, -1));

            var points = string.Join(" ", Enumerable.Range(0, 100).Select(i => i / 100.0f).Select(i => biArc.GetTransformedPoint(i * biArc.Length, Vector3.Zero))
            .Select(t => "(" + t.X.ToString(CultureInfo.InvariantCulture) + "," + t.Z.ToString(CultureInfo.InvariantCulture) + ")"));


            var point1 = new Vector3(-1, 0, 0);
            var point2 = new Vector3(1, 0, 0);

            biArc.SnapTo(point1, out var snapped1, out float distance1);
            biArc.SnapTo(point2, out var snapped2, out float distance2);

            var sqrt2inv = 0.70710678818f;


            //var inc = biArc.Length / 100;
            //var items = new StringBuilder();
            //for (float i = 0; i <= biArc.Length; i += inc)
            //{
            //    var v = biArc.GetTransform(i).MultiplyPoint3x4(Vector3.Zero);
            //    items.AppendLine($"{v.X.ToString(CultureInfo.InvariantCulture)}, {v.Z.ToString(CultureInfo.InvariantCulture)}");
            //}



            var pointExpected1 = new Vector3(-sqrt2inv, 0, sqrt2inv - 1);
            Assert.AreEqual(pointExpected1.Round(0.0001f), snapped1.Round(0.0001f));
            var pointExpected2 = new Vector3(sqrt2inv, 0, 1 - sqrt2inv);
            Assert.AreEqual(pointExpected2.Round(0.0001f), snapped2.Round(0.0001f));
        }



        [TestMethod]
        public void TestSnapArcUndershoot()
        {

            var arc = new ArcLoftPath(ArcDefinition.FromAxisAngle(Directions3.Forward, MathF.PI / 2, Vector3.Zero, Directions3.Up));


            var start = arc.Definition.GetStartPosition();
            Assert.AreEqual(Directions3.Forward, start.Round(0.0001f));
            var end = arc.Definition.GetEndPosition();
            Assert.AreEqual(Directions3.Left, end.Round(0.0001f));

            var point = new Vector3(-1, 0, 1);

            arc.SnapTo(point, out var snapped, out var distance);


            var pointExpected = new Vector3(0, 0, 1);


            Assert.AreEqual(pointExpected.Round(0.0001f), snapped.Round(0.0001f));

            var l = 0f;

            Assert.AreEqual(l, distance, 0.0001f);
        }

        [TestMethod]
        public void TestSnapArcOvershoot()
        {

            var arc = new ArcLoftPath(ArcDefinition.FromAxisAngle(Directions3.Forward, MathF.PI / 2, Vector3.Zero, Directions3.Up));


            var point = new Vector3(1, 0, -1);

            arc.SnapTo(point, out var snapped, out var distance);


            var pointExpected = new Vector3(1, 0, 0);


            Assert.AreEqual(pointExpected.Round(0.0001f), snapped.Round(0.0001f));

            var l = MathF.PI / 2;

            Assert.AreEqual(l, distance, 0.0001f);
        }

        [TestMethod]
        public void TestSnapArcQ0()
        {
            var def = ArcDefinition.FromAxisAngle(Directions3.Forward, MathF.PI / 2, Vector3.Zero, Directions3.Up);

            var arc = new ArcLoftPath(ArcDefinition.FromAxisAngle(Directions3.Forward, MathF.PI / 2, Vector3.Zero, Directions3.Up));

            var point = new Vector3(1, 0, 1);

            arc.SnapTo(point, out var snapped, out var distance);

            var sqrt2inv = 0.70710678818f;

            var pointExpected = new Vector3(sqrt2inv, 0, sqrt2inv);


            Assert.AreEqual(pointExpected.Round(0.0001f), snapped.Round(0.0001f));

            var l = 0.785398f;

            Assert.AreEqual(l, distance, 0.0001f);

        }


        [TestMethod]
        public void TestSnapArcQ1()
        {

            var arc = new ArcLoftPath(ArcDefinition.FromAxisAngle(Directions3.Left, MathF.PI / 2, Vector3.Zero, Directions3.Up));

            var point = new Vector3(-1, 0, 1);

            arc.SnapTo(point, out var snapped, out var distance);

            var sqrt2inv = 0.70710678818f;

            var pointExpected = new Vector3(-sqrt2inv, 0, sqrt2inv);


            Assert.AreEqual(pointExpected.Round(0.0001f), snapped.Round(0.0001f));

            var l = 0.785398f;

            Assert.AreEqual(l, distance, 0.0001f);

        }
        [TestMethod]
        public void TestSnapArcQ3()
        {

            var arc = new ArcLoftPath(ArcDefinition.FromAxisAngle(Directions3.Backward, MathF.PI / 2, Vector3.Zero, Directions3.Up));

            var point = new Vector3(-1, 0, -1);

            arc.SnapTo(point, out var snapped, out var distance);

            var sqrt2inv = 0.70710678818f;

            var pointExpected = new Vector3(-sqrt2inv, 0, -sqrt2inv);


            Assert.AreEqual(pointExpected.Round(0.0001f), snapped.Round(0.0001f));

            var l = 0.785398f;

            Assert.AreEqual(l, distance, 0.0001f);

        }
        [TestMethod]
        public void TestSnapArcQ4()
        {

            var arc = new ArcLoftPath(ArcDefinition.FromAxisAngle(Directions3.Right, MathF.PI / 2, Vector3.Zero, Directions3.Up));

            var point = new Vector3(1, 0, -1);

            arc.SnapTo(point, out var snapped, out var distance);

            var sqrt2inv = 0.70710678818f;

            var pointExpected = new Vector3(sqrt2inv, 0, -sqrt2inv);


            Assert.AreEqual(pointExpected.Round(0.0001f), snapped.Round(0.0001f));

            var l = 0.785398f;

            Assert.AreEqual(l, distance, 0.0001f);

        }

        [TestMethod]
        public void TestSnapArc180Forward()
        {

            var arc = new ArcLoftPath(ArcDefinition.FromAxisAngle(Directions3.Right, MathF.PI, Vector3.Zero, Directions3.Up));

            var point = new Vector3(0, 0, -2);

            arc.SnapTo(point, out var snapped, out var distance);


            var pointExpected = new Vector3(0, 0, -1);


            Assert.AreEqual(pointExpected.Round(0.0001f), snapped.Round(0.0001f));

            var l = MathF.PI / 2;

            Assert.AreEqual(l, distance, 0.0001f);

        }
        [TestMethod]
        public void TestSnapArc180Backward()
        {

            var arc = new ArcLoftPath(ArcDefinition.FromAxisAngle(Directions3.Right, -MathF.PI, Vector3.Zero, Directions3.Up));

            var point = new Vector3(0, 0, 2);

            arc.SnapTo(point, out var snapped, out var distance);


            var pointExpected = new Vector3(0, 0, 1);


            Assert.AreEqual(pointExpected.Round(0.0001f), snapped.Round(0.0001f));

            var l = MathF.PI / 2;

            Assert.AreEqual(l, distance, 0.0001f);

        }

        [TestMethod]
        public void TestSnapSweepForward()
        {
            var arc = new ArcLoftPath(ArcDefinition.FromAxisAngle(Directions3.Right, MathF.PI, Vector3.Zero, Directions3.Up));

            int count = 18;
            float increment = MathF.PI / count;

            for (float i = 0; i <= MathF.PI; i += increment)
            {
                var x = MathF.Cos(i);
                var z = -MathF.Sin(i);

                var theoreticalPoint = arc.GetTransformedPoint(i, Vector3.Zero);

                var pointInside = new Vector3(x, 0, z) * 0.5f;
                var pointAt = new Vector3(x, 0, z);
                var pointOutside = new Vector3(x, 0, z) * 1.5f;

                Assert.AreEqual(theoreticalPoint.Round(0.0001f), pointAt.Round(0.0001f));

                arc.SnapTo(pointInside, out var snapped1, out float distance1);
                arc.SnapTo(pointAt, out var snapped2, out float distance2);
                arc.SnapTo(pointOutside, out var snapped3, out float distance3);

                Assert.AreEqual(pointAt.Round(0.001f), snapped1.Round(0.001f));
                Assert.AreEqual(pointAt.Round(0.001f), snapped2.Round(0.001f));
                Assert.AreEqual(pointAt.Round(0.001f), snapped3.Round(0.001f));
                Assert.AreEqual(i, distance1, 0.001f);
                Assert.AreEqual(i, distance2, 0.001f);
                Assert.AreEqual(i, distance3, 0.001f);
            }
        }
        [TestMethod]
        public void TestSnapSweepBack()
        {
            var arc = new ArcLoftPath(ArcDefinition.FromAxisAngle(Directions3.Right, -MathF.PI, Vector3.Zero, Directions3.Up));

            int count = 18;
            float increment = MathF.PI / count;

            for (float i = 0; i <= MathF.PI; i += increment)
            {
                var x = MathF.Cos(-i);
                var z = -MathF.Sin(-i);

                var theoreticalPoint = arc.GetTransformedPoint(i, Vector3.Zero);

                var pointInside = new Vector3(x, 0, z) * 0.5f;
                var pointAt = new Vector3(x, 0, z);
                var pointOutside = new Vector3(x, 0, z) * 1.5f;

                Assert.AreEqual(theoreticalPoint.Round(0.0001f), pointAt.Round(0.0001f));

                arc.SnapTo(pointInside, out var snapped1, out float distance1);
                arc.SnapTo(pointAt, out var snapped2, out float distance2);
                arc.SnapTo(pointOutside, out var snapped3, out float distance3);

                Assert.AreEqual(pointAt.Round(0.001f), snapped1.Round(0.001f));
                Assert.AreEqual(pointAt.Round(0.001f), snapped2.Round(0.001f));
                Assert.AreEqual(pointAt.Round(0.001f), snapped3.Round(0.001f));
                Assert.AreEqual(i, distance1, 0.001f);
                Assert.AreEqual(i, distance2, 0.001f);
                Assert.AreEqual(i, distance3, 0.001f);
            }
        }




        [TestMethod]
        public void TestPositiveArc()
        {
            var start = new Vector3(-1, 0, 0);
            var end = new Vector3(0, 0, -1);
            var startDirection = new Vector3(0, 0, -1);
            var endDirection = new Vector3(1, 0, 0);

            var halfDirection = (startDirection + endDirection).Normalized();

            var arc = new ArcLoftPath(ArcDefinition.FromAxisAngle(start, MathF.PI / 2, Vector3.Zero, Directions3.Up));

            Assert.AreEqual(MathF.PI / 2, arc.Length);
            var startTransform = arc.GetTransform(0);

            var endTransform = arc.GetTransform(arc.Length);

            var startForward = startTransform.MultiplyVector(Directions3.Forward);
            var endForward = endTransform.MultiplyVector(Directions3.Forward);

            var startPosition = startTransform.MultiplyPoint3x4(Vector3.Zero);
            var endPosition = endTransform.MultiplyPoint3x4(Vector3.Zero);

            Assert.AreEqual(start, startPosition.Round(0.00001f));
            Assert.AreEqual(end, endPosition.Round(0.00001f));

            Assert.AreEqual(startDirection, startForward.Round(0.00001f));
            Assert.AreEqual(endDirection, endForward.Round(0.00001f));

            var halfTransform = arc.GetTransform(arc.Length / 2);



            Assert.AreEqual(halfDirection.Round(0.00001f), halfTransform.MultiplyVector(Directions3.Forward).Round(0.00001f));

        }

        [TestMethod]
        public void TestNegativeArc()
        {
            var start = new Vector3(-1, 0, 0);
            var startDirection = new Vector3(0, 0, 1);
            var endDirection = new Vector3(1, 0, 0);
            var halfDirection = (startDirection + endDirection).Normalized();

            var arc = new ArcLoftPath(ArcDefinition.FromAxisAngle(start, -MathF.PI / 2, Vector3.Zero, Directions3.Up));

            Assert.AreEqual(MathF.PI / 2, arc.Length);
            var startTransform = arc.GetTransform(0);

            var endTransform = arc.GetTransform(arc.Length);

            var startForward = startTransform.MultiplyVector(Directions3.Forward);
            var endForward = endTransform.MultiplyVector(Directions3.Forward);

            var startPosition = startTransform.MultiplyPoint3x4(Vector3.Zero);
            var endPosition = endTransform.MultiplyPoint3x4(Vector3.Zero);

            Assert.AreEqual(start, startPosition.Round(0.00001f));
            Assert.AreEqual(startDirection, endPosition.Round(0.00001f));

            Assert.AreEqual(startDirection, startForward.Round(0.00001f));
            Assert.AreEqual(endDirection, endForward.Round(0.00001f));

            var halfTransform = arc.GetTransform(arc.Length / 2);



            Assert.AreEqual(halfDirection.Round(0.00001f), halfTransform.MultiplyVector(Directions3.Forward).Round(0.00001f));

        }



        [TestMethod]
        public void TestLinearX()
        {
            var linearLoftPath = new LinearPath(Vector3.Zero, new Vector3(1, 0, 0));

            var transformStart = linearLoftPath.GetTransform(0);
            var transformEnd = linearLoftPath.GetTransform(1);

            var forwardStart = transformStart.MultiplyVector(Directions3.Forward);

            var forwardEnd = transformEnd.MultiplyVector(Directions3.Forward);

            Assert.AreEqual(Directions3.Right, forwardStart);
            Assert.AreEqual(Directions3.Right, forwardEnd);


            var rightStart = transformStart.MultiplyVector(Directions3.Right);

            var rightEnd = transformEnd.MultiplyVector(Directions3.Right);
            Assert.AreEqual(Directions3.Backward, rightStart);
            Assert.AreEqual(Directions3.Backward, rightEnd);
        }


        [TestMethod]
        public void TestLinearZ()
        {
            var linearLoftPath = new LinearPath(Vector3.Zero, new Vector3(0, 0, 1));

            var transformStart = linearLoftPath.GetTransform(0);
            var transformEnd = linearLoftPath.GetTransform(1);

            var forwardStart = transformStart.MultiplyVector(Directions3.Forward);

            var forwardEnd = transformEnd.MultiplyVector(Directions3.Forward);

            Assert.AreEqual(new Vector3(0, 0, 1), forwardStart);
            Assert.AreEqual(new Vector3(0, 0, 1), forwardEnd);


            var rightStart = transformStart.MultiplyVector(Directions3.Right);

            var rightEnd = transformEnd.MultiplyVector(Directions3.Right);
            Assert.AreEqual(new Vector3(-1, 0, 0), rightStart);
            Assert.AreEqual(new Vector3(-1, 0, 0), rightEnd);
        }
    }
}
