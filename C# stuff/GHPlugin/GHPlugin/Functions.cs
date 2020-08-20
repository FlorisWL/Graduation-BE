using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace GHPlugin
{
    public class Functions
    {
        public void ThreeForceJoint(List<double> angles, Line mainLine, List<Vector3d> otherVectors, out List<Line> otherForceLines, out List<bool> positiveForce)
        {
            double pA; double pB;
            List<Line> OtherForceLines = new List<Line>();
            Point3d MiddlePoint;
            bool Flipped = false;
            List<bool> myPositiveForce = new List<bool>();

            if (angles[0] < angles[1])
            {
                OtherForceLines.Add(new Line(mainLine.To, otherVectors[0]));
                OtherForceLines.Add(new Line(mainLine.From, otherVectors[1]));
            }
            else
            {
                OtherForceLines.Add(new Line(mainLine.From, otherVectors[0]));
                OtherForceLines.Add(new Line(mainLine.To, otherVectors[1]));
                Flipped = true;
            }

            Intersection.LineLine(OtherForceLines[0], OtherForceLines[1], out pA, out pB);
            MiddlePoint = OtherForceLines[0].PointAt(pA);
            if (Flipped)
            {
                OtherForceLines[0] = new Line(MiddlePoint, mainLine.From);
                OtherForceLines[1] = new Line( mainLine.To, MiddlePoint);
            }
            else
            {
                OtherForceLines[0] = new Line(mainLine.To, MiddlePoint);
                OtherForceLines[1] = new Line(MiddlePoint, mainLine.From);
            }
            otherForceLines = OtherForceLines;

            for (int i = 0; i < OtherForceLines.Count; i++)
            {
                if (Vector3d.Multiply(OtherForceLines[i].Direction, otherVectors[i]) > 0.0)
                    myPositiveForce.Add(false);
                else
                    myPositiveForce.Add(true);
            }
            positiveForce = myPositiveForce;

            return;
        }
    }
}
