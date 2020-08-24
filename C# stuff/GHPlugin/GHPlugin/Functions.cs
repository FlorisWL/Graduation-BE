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
        public Boolean ThreeForceJoint(List<double> angles, Line mainLine, List<Vector3d> otherVectors, out List<Line> otherForceLines, out List<bool> positiveForce, out Point3d middlePoint)
        {
            double pA; double pB;
            List<Line> OtherForceLines = new List<Line>();
            Point3d MiddlePoint;
            bool Flipped = false;
            List<bool> myPositiveForce = new List<bool>();
            bool valid = true;

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

            valid = Intersection.LineLine(OtherForceLines[0], OtherForceLines[1], out pA, out pB, 0.000001, false);
            if (valid)
            {
                MiddlePoint = OtherForceLines[0].PointAt(pA);
                if (Flipped)
                {
                    OtherForceLines[0] = new Line(MiddlePoint, mainLine.From);
                    OtherForceLines[1] = new Line(mainLine.To, MiddlePoint);
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
                middlePoint = MiddlePoint;
            }
            else
            {
                otherForceLines = new List<Line>();
                positiveForce = new List<bool>();
                middlePoint = new Point3d(0,0,0);
            }

            return valid;
        }

        public void FindAngles(Line mainLine, List<Line> otherLines, out List<double> angles, out List<Vector3d> otherVectors)
        {
            Plane planeXY = new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, 1));
            Vector3d mainVector = mainLine.Direction;
            List<double> myAngles = new List<double>();
            List<Vector3d> myOtherVectors = new List<Vector3d>();

            for (int i = 0; i < otherLines.Count; i++)
            {
                myOtherVectors.Add(otherLines[i].Direction);
                myAngles.Add(Vector3d.VectorAngle(mainVector, myOtherVectors[i], planeXY));
            }

            angles = myAngles;
            otherVectors = myOtherVectors;

            return;
        }
    }
}
