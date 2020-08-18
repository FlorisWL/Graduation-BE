﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

namespace GHPlugin
{
    class Pyramid
    {
        public Plane BasePlane = Plane.WorldXY;
        public double Length = 1.0;
        public double Width = 1.0;
        public double Height = 1.0;


        public Pyramid(double length, double height, double width)
        {
            Length = length;
            Width = width;
            Height = height;
        }


        public Pyramid(Plane basePlane, double length, double height, double width)
        {
            BasePlane = basePlane;
            Length = length;
            Width = width;
            Height = height;
        }

        public List<LineCurve> ComputeEdges()
        {
            Point3d A = BasePlane.Origin + BasePlane.XAxis * Length * 0.5 + BasePlane.YAxis * Width * 0.5;
            Point3d B = BasePlane.Origin - BasePlane.XAxis * Length * 0.5 + BasePlane.YAxis * Width * 0.5;
            Point3d C = BasePlane.Origin - BasePlane.XAxis * Length * 0.5 - BasePlane.YAxis * Width * 0.5;
            Point3d D = BasePlane.Origin + BasePlane.XAxis * Length * 0.5 - BasePlane.YAxis * Width * 0.5;
            Point3d M = BasePlane.Origin + BasePlane.ZAxis * Height;

            List<LineCurve> displayLines = new List<LineCurve>();

            displayLines.Add(new LineCurve(A, B));
            displayLines.Add(new LineCurve(B, C));
            displayLines.Add(new LineCurve(C, D));
            displayLines.Add(new LineCurve(D, A));

            displayLines.Add(new LineCurve(A, M));
            displayLines.Add(new LineCurve(B, M));
            displayLines.Add(new LineCurve(C, M));
            displayLines.Add(new LineCurve(D, M));

            return displayLines;
        }
    }
}
