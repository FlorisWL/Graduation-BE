using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHPlugin
{
    public class HalfMember
    {
        public int MemberIndex;
        public int JointIndex;
        public Line HalfMemberLine;

        public HalfMember(int memberIndex, Member member, Boolean start, List<Point3d> joints)
        {
            MemberIndex = memberIndex;
            if (start)
            {
                JointIndex = member.StartJointIndex;
                HalfMemberLine = new Line(member.MiddlePoint, joints[member.StartJointIndex]);
            }
            else
            {
                JointIndex = member.EndJointIndex;
                HalfMemberLine = new Line(member.MiddlePoint, joints[member.EndJointIndex]);
            }
        }
    }
}
