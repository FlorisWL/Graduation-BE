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

        public Joint(Point3d jointLocation)
        {
            JointLocation = jointLocation;
        }
    }
}
