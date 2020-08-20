using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHPlugin
{
    class Joint
    {
        public Point3d JointLocation; 
        public List<int> MemberIndices = new List<int>();
        public List<int> ExternalForceIndices = new List<int>();
        public List<int> SupportReactionIndices = new List<int>();
        public Boolean Solved = false;

        public Joint(Point3d jointLocation)
        {
            JointLocation = jointLocation;
        }

        public void FindAngles(Line mainLine, List<Line> otherLines, out List<double> angles, out List<Vector3d> otherVectors)
        {
            Point3d jointPointForm = JointLocation;
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
