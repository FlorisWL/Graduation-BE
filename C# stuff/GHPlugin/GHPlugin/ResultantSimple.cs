using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHPlugin
{
    class ResultantSimple
    {
        public Line ResultantLine;
        public Boolean Valid = false;

        public ResultantSimple(Point3d point, List<Line> knownForceLines, List<Line> knownForceLinesForAngle, List<Line> unknownForceLinesForAngle)
        {
            Plane planeXY = new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, -1));
            List<Line> resultantLineForces = new List<Line>();
            Point3d startPoint = point;

            //if (knownForceLines.Count==2)
            //{ 
            //double vectorAngle = Vector3d.VectorAngle(knownForceLinesForAngle[0].Direction, knownForceLinesForAngle[1].Direction, planeXY);
            //    if (vectorAngle < Math.PI)
            //        startPoint = knownForceLines[0].From;
            //    else
            //        startPoint = knownForceLines[1].From;
            //}

            double maxKnownVectorAngle;
            double minUnkownVectorAngle;

            for (int i = 0; i < knownForceLines.Count; i++)
            {
                maxKnownVectorAngle = 0;
                minUnkownVectorAngle = 10;

                for (int j = 0; j < unknownForceLinesForAngle.Count; j++)
                {
                    double unknownVectorAngle = Vector3d.VectorAngle(knownForceLinesForAngle[i].Direction, unknownForceLinesForAngle[j].Direction, planeXY);
                    if (unknownVectorAngle < minUnkownVectorAngle)
                        minUnkownVectorAngle = unknownVectorAngle;

                }

                for (int j = 0; j < knownForceLines.Count; j++)
                {
                    double knownVectorAngle = Vector3d.VectorAngle(knownForceLinesForAngle[i].Direction, knownForceLinesForAngle[j].Direction, planeXY);
                    if (knownVectorAngle > maxKnownVectorAngle)
                        maxKnownVectorAngle = knownVectorAngle;
                }

                if(minUnkownVectorAngle> maxKnownVectorAngle)
                {
                    startPoint = knownForceLines[i].From;
                    Valid = true;
                }
            }

            if (Valid)
            {
                for (int i = 0; i < knownForceLines.Count; i++)
                {
                    resultantLineForces.Add(new Line(startPoint, knownForceLines[i].Direction));
                    startPoint = resultantLineForces[i].To;
                }

                ResultantLine = new Line(resultantLineForces[0].From, resultantLineForces[resultantLineForces.Count - 1].To);
            }
        }
    }
}
