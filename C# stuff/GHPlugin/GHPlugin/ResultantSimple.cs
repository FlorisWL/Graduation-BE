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

        public ResultantSimple(Point3d point, List<Line> knownForceLines, List<Line> knownForceLinesForAngles)
        {
            Plane planeXY = new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, 1));
            List<Line> resultantLineForces = new List<Line>();
            Point3d startPoint = point;
            if (knownForceLines.Count==2)
            { 
            double vectorAngle = Vector3d.VectorAngle(knownForceLinesForAngles[0].Direction, knownForceLinesForAngles[1].Direction, planeXY);
                if (vectorAngle < Math.PI)
                    startPoint = knownForceLines[0].From;
                else
                    startPoint = knownForceLines[1].From;
            }

            
            for (int i = 0; i < knownForceLines.Count; i++)
            {
                resultantLineForces.Add(new Line(startPoint, knownForceLines[i].Direction));
                startPoint = resultantLineForces[i].To;
            }

            ResultantLine = new Line(resultantLineForces[0].From, resultantLineForces[resultantLineForces.Count - 1].To);
        }
    }
}
