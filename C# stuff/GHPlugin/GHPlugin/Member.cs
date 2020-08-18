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
        public Line MemberLine;
        public Point3d StartJoint;
        public Point3d EndJoint;
        public Point3d MiddlePoint;
        public Boolean Known = false;
        public Double Force = 0.0;
        public Boolean PositiveForce = true;

        public Member(Point3d startJoint, Point3d endJoint)
        {
            StartJoint = startJoint;
            EndJoint = endJoint;
            MiddlePoint = (startJoint + endJoint) / 2.0;
            MemberLine = new Line(startJoint, endJoint);
        }

        public Member(Line line)
        {
            StartJoint = line.From;
            EndJoint = line.To;
            MiddlePoint = (line.From + line.To) / 2.0;
            MemberLine = line;
        }
    }
}
