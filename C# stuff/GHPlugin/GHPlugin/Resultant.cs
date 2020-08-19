using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHPlugin
{
    class Resultant
    {
        public Point3d StartPointForce;
        public List<ExternalForce> ExternalForces;
        public List<Line> ForceLinesForce;
        public Line ResultantForce;
        public Line ResultantForm;


        public Resultant(Point3d startPointForce, List<ExternalForce> externalForces)
        {
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

        }
    }
}
