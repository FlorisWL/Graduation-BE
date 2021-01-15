using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace GHPlugin
{
    public class Member
    {
        public int StartJointIndex;
        public int EndJointIndex;
        public Point3d MiddlePoint;
        public Line FormLine;
        public Line ForceLine = new Line();
        public Double Force = 0.0;
        public Boolean PositiveForce = true;
        public Boolean Known = false;

        public Member(int startJointIndex, int endJointIndex, List<Point3d> joints)
        {
            StartJointIndex = startJointIndex;
            EndJointIndex = endJointIndex;
            MiddlePoint = (joints[startJointIndex] + joints[endJointIndex]) / 2.0;
            FormLine = new Line(joints[startJointIndex], joints[endJointIndex]);
        }
    }
}
