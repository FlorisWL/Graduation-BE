using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHPlugin
{
    public class Resultant
    {
        public Point3d StartPointForce;
        public List<ExternalForce> ExternalForces;
        public List<Line> ForceLinesForce;
        public Line ResultantForce;
        public double Force = 0.0;
        public Line ResultantForceForAngle;
        public Point3d PointForm;
        public Line ResultantForm;


        public Resultant(Point3d startPointForce, List<ExternalForce> externalForces, double ratio, Point3d centerPoint)
        {
            Vector3d zPostive = new Vector3d(0, 0, 1); 
            StartPointForce = startPointForce;
            ExternalForces = externalForces;
            List<Line> forceLinesForce = new List<Line>();
            Point3d startPoint = startPointForce;

            for (int i = 0; i < externalForces.Count; i++)
            {
                forceLinesForce.Add(new Line(startPoint, externalForces[i].ForceVector));
                startPoint = forceLinesForce[i].To;
            }

            ForceLinesForce = forceLinesForce;
            ResultantForce = new Line(forceLinesForce[0].From, forceLinesForce[forceLinesForce.Count - 1].To);
            Force = ResultantForce.Length;
            Vector3d resultantVector = ResultantForce.Direction;

            Vector3d arbitraryVector = resultantVector / 1.0;
            arbitraryVector.Rotate(1.5, zPostive);
            Point3d arbitraryPoint = startPoint + arbitraryVector;
            PointForm = arbitraryPoint;
            

            List<Line> globalForceLines2 = new List<Line>();
            List<Line> globalForceLines1 = new List<Line>();
            List<Vector3d> globalForceVectors2 = new List<Vector3d>();
            List<Vector3d> globalForceVectors1 = new List<Vector3d>();

            globalForceLines2.Add(new Line(ResultantForce.To, arbitraryPoint));
            globalForceLines2.Add(new Line(arbitraryPoint, ResultantForce.From));
            globalForceVectors2.Add(new Vector3d(globalForceLines2[0].Direction));
            globalForceVectors2.Add(new Vector3d(globalForceLines2[1].Direction));

            for (int i = 0; i < externalForces.Count - 1; i++)
            {
                globalForceLines1.Add(new Line(forceLinesForce[i].To, arbitraryPoint));
                globalForceVectors1.Add(new Vector3d(globalForceLines1[i].Direction));
            }

            //Translation to form diagram:
            Line lineA = new Line(externalForces[0].FormLine.To, globalForceVectors1[0]);
            Line lineB;
            double pA;
            double pB;
            Point3d parameterPoint;
            Point3d resultantPoint;

            for (int i = 0; i < externalForces.Count - 1; i++)
            {
                lineB = externalForces[i+1].FormLine;
                Intersection.LineLine(lineA, lineB, out pA, out pB);
                parameterPoint = lineA.PointAt(pA);
                if (i < externalForces.Count - 2)
                {
                    lineA = new Line(parameterPoint, globalForceVectors1[i + 1]);
                }
                else
                {
                    lineA = new Line(parameterPoint, globalForceVectors2[0]);
                    lineB = new Line(externalForces[0].FormLine.To, globalForceVectors2[1]);
                    Intersection.LineLine(lineA, lineB, out pA, out pB);
                    resultantPoint = lineA.PointAt(pA);
                    resultantVector.Unitize();
                    resultantVector.Reverse();
                    Line resultantLine = new Line(resultantPoint, resultantVector);
                    resultantPoint = resultantLine.ClosestPoint(centerPoint, false);
                    resultantLine = new Line(resultantPoint, resultantVector);
                    resultantLine.Flip();
                    double forceLengthForm = Force * ratio;
                    double extendValue = forceLengthForm - 1.0;
                    resultantLine.Extend(extendValue, 0);
                    ResultantForm = resultantLine;
                }
            }
        }

        public void ResultantForAngle(GlobalDiagram globaldiagram)
        {
            Boolean flip = false;
            Plane planeXY = new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, 1));
            Line resultantFlipped = ResultantForm;
            resultantFlipped.Flip();
            Vector3d m0 = globaldiagram.MemberLinesForm[0].Direction;
            Vector3d m1 = globaldiagram.MemberLinesForm[1].Direction;

            if (Vector3d.VectorAngle(m0,m1, planeXY) < Math.PI)
            {
                if (Vector3d.VectorAngle(m0, resultantFlipped.Direction, planeXY) < Vector3d.VectorAngle(m0, m1, planeXY))
                    flip = true;
            }
            else
            {
                if (Vector3d.VectorAngle(m1, resultantFlipped.Direction, planeXY) < Vector3d.VectorAngle(m1, m0, planeXY))
                    flip = true;
            }

            ResultantForceForAngle = ResultantForce;
            if (flip)
                ResultantForceForAngle.Flip();
            return;
        }
    }
}
