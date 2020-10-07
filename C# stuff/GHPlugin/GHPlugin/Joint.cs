using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHPlugin
{
    public class Joint
    {
        public Point3d JointLocation;
        public int PointIndex;
        public List<int> MemberIndices = new List<int>();
        public List<int> HalfMemberIndices = new List<int>();
        public List<int> ExternalForceIndices = new List<int>();
        public List<int> SupportReactionIndices = new List<int>();
        public Boolean JointSolved = false;
        public Boolean Relocated = false;
        public Point3d InitSolveLocation = new Point3d(0,0,0);
        public int Knowns;
        public int Unknowns;

        public Joint(Point3d jointLocation)
        {
            JointLocation = jointLocation;
        }

        public Joint(int pointIndex, List<Point3d> points)
        {
            PointIndex = pointIndex;
            JointLocation = points[pointIndex];
            InitSolveLocation.Y = pointIndex * 10;
        }

        public void FillJoint(List<HalfMember> allHalfMembers, List<ExternalForce> allExtForces, List<SupportReaction> allSupports)
        {
            for (int i = 0; i < allHalfMembers.Count; i++)
            {
                if (allHalfMembers[i].JointIndex == PointIndex)
                {
                    HalfMemberIndices.Add(i);
                    MemberIndices.Add(allHalfMembers[i].MemberIndex);
                }
            }

            for (int i = 0; i < allExtForces.Count; i++)
            {
                if (allExtForces[i].JointIndex == PointIndex)
                {
                    ExternalForceIndices.Add(i);
                }
            }

            for (int i = 0; i < allSupports.Count; i++)
            {
                if (allSupports[i].JointIndex == PointIndex)
                {
                    SupportReactionIndices.Add(i);
                }
            }

        }

        

        public void KnownsUnknowns(List<Member> allMembers)
        {
            int knowns = 0;
            int unknowns = 0;
            for (int i = 0; i < MemberIndices.Count; i++)
            {
                if (allMembers[MemberIndices[i]].Known)
                    knowns++;
                else
                    unknowns++;
            }

            for (int i = 0; i < ExternalForceIndices.Count; i++)
            {
                knowns++;
            }

            for (int i = 0; i < SupportReactionIndices.Count; i++)
            {
                knowns++;
            }
        }

    }
}
