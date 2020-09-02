using Grasshopper.Kernel.Types;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace GHPlugin
{
    public class Functions
    {
        public List<double> CreateDefaultList(int listLength, double value)
        {
            List<double> defaultList = new List<double>();

            for (int i = 0; i < listLength; i++)
                defaultList.Add(value);

            return defaultList;
        }
        
        public Boolean ThreeForceJoint(List<double> angles, Line mainLine, List<Vector3d> otherVectors, out List<Line> otherForceLines, out List<bool> positiveForce, out Point3d middlePoint)
        {
            double pA; double pB;
            List<Line> OtherForceLines = new List<Line>();
            Point3d MiddlePoint;
            bool Flipped = false;
            List<bool> myPositiveForce = new List<bool>();
            bool valid;

            if (angles[0] > angles[1])
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

        public void FindAngles(Line mainLine, List<Line> otherLinesForAngle, List<Line> otherLines, out List<double> angles, out List<Vector3d> otherVectors)
        {
            Plane planeXY = new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, 1));
            Vector3d mainVector = mainLine.Direction;
            List<double> myAngles = new List<double>();
            List<Vector3d> myOtherVectors = new List<Vector3d>();

            for (int i = 0; i < otherLines.Count; i++)
            {
                myOtherVectors.Add(otherLines[i].Direction);
                myAngles.Add(Vector3d.VectorAngle(mainVector, otherLinesForAngle[i].Direction, planeXY));
            }

            angles = myAngles;
            otherVectors = myOtherVectors;

            return;
        }
    
        public void DisplayRectangles(List<Member> members, double scalingFactorUnified, out List<Brep> oBreps)
        {
            Rectangle3d rectangle;
            List<Brep> breps = new List<Brep>();
            Vector3d normal1;
            Vector3d normal2;
            Point3d point1;
            Point3d point2;
            
            Vector3d zPostive = new Vector3d(0, 0, 1);
            double thickness;
            double angle;

            for (int i = 0; i < members.Count; i++)
            {
                thickness = members[i].Force * scalingFactorUnified;
                //thickness = 1;
                point1 = members[i].MemberLine.From;
                point2 = members[i].MemberLine.To;
                normal1 = members[i].MemberLine.Direction;
                normal1.Unitize();
                normal1 = normal2 = normal1* (thickness*0.5);
                normal1.Rotate(0.5 * Math.PI, zPostive);
                normal2.Rotate(-0.5 * Math.PI, zPostive);

                point1 = Point3d.Add(point1, normal1);
                point2 = Point3d.Add(point2, normal2);

                Plane planeXY = new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, 1));
                angle = Vector3d.VectorAngle(new Vector3d(1, 0, 0), members[i].MemberLine.Direction, planeXY);
                planeXY.Rotate(angle, new Vector3d(0, 0, 1));

                rectangle = new Rectangle3d(planeXY, point1, point2);
                PolylineCurve curveRectangle = rectangle.ToPolyline().ToPolylineCurve();
                breps.Add(Brep.CreatePlanarBreps(curveRectangle, 0.000001)[0]);
            }

            oBreps = breps;

            return;
        }
    
        public void DisplayColors(List<Member> members, out List<Color> oColors)
        {
            List<Color> colors = new List<Color>();

            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].PositiveForce == false)
                    colors.Add(Color.FromArgb(122, Color.FromName("Blue")));
                else
                    colors.Add(Color.FromArgb(122, Color.FromName("Red")));
            }

            oColors = colors;

            return;
        }
        public Point3d CenterPoint(List<Point3d> joints)
        {
            Point3d centerPoint = new Point3d(0,0,0);
            for (int i = 0; i < joints.Count; i++)
            {
                centerPoint += joints[i];
            }
            centerPoint = centerPoint / joints.Count;
            return centerPoint;
        }
        
        public void DisplayNumericalValues(List<Line> formLines, out List<Plane> locations)
        {
            Point3d startPoint;
            Plane centerPlane;
            Vector3d transformationVector;
            double angle;
            Functions functions = new Functions();
            List<Plane> myLocations = new List<Plane>();
            Vector3d zPositive = new Vector3d(0, 0, 1);
            Plane planeXY = new Plane(new Point3d(0, 0, 0), zPositive);

            for (int i = 0; i < formLines.Count; i++)
            {
                startPoint = (3*formLines[i].From + 1*formLines[i].To)/4.0;
                transformationVector = formLines[i].Direction;
                transformationVector.Unitize();
                transformationVector = transformationVector * 0.3;
                transformationVector.Rotate(0.5 * Math.PI, zPositive);
                startPoint = Point3d.Add(startPoint, transformationVector);
                centerPlane = new Plane(startPoint, new Vector3d(0, 0, 1));

                angle = Vector3d.VectorAngle(new Vector3d(1, 0, 0), formLines[i].Direction, planeXY);
                if (((angle > 0.5 * Math.PI) && (angle <= 1.5 * Math.PI)) || ((angle < -0.5 * Math.PI) && (angle >= -1.5 * Math.PI)))
                    angle += Math.PI;

                centerPlane.Rotate(angle, new Vector3d(0, 0, 1));

                myLocations.Add(centerPlane);
            }

            locations = myLocations;
        }

    }
}
