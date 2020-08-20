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

        public ResultantSimple(Point3d point, List<Line> knownForceLines)
        {
            List<Line> resultantLineForces = new List<Line>();
            Point3d startPoint = point;
            for (int i = 0; i < knownForceLines.Count; i++)
            {
                resultantLineForces.Add(new Line(startPoint, knownForceLines[i].Direction));
                startPoint = resultantLineForces[i].To;
            }

            ResultantLine = new Line(resultantLineForces[0].From, resultantLineForces[resultantLineForces.Count - 1].To);
        }
    }
}
