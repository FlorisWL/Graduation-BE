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
        public Point3d InitSolveLocation = new Point3d(0,-10,0);
        public int Knowns;
        public int Unknowns;

        public Joint(Point3d jointLocation)
        {
            JointLocation = jointLocation;
        }

        public Joint(int pointIndex, List<Point3d> points, double iScaleFactorSpacing)
        {
            PointIndex = pointIndex;
            JointLocation = points[pointIndex];
            if(pointIndex%2 == 1)
            {
                InitSolveLocation.Y += -10 * iScaleFactorSpacing;
            }
            InitSolveLocation.X += pointIndex * (12.5 * 0.5) * iScaleFactorSpacing;
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

        public void SetExternalForcesForAngle(List<HalfMember> iHalfMembers, List<ExternalForce> iExternalForces)
        {
            if(ExternalForceIndices.Count > 0)
            {
                Plane planeXY = new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, -1));
                List<double> anglesBetweenMembers = new List<double>();
                double minAngle;
                double tempAngle;

                for (int i = 0; i < HalfMemberIndices.Count; i++)
                {
                    minAngle = 360;
                    for (int j = 0; j < HalfMemberIndices.Count; j++)
                        if (i != j)
                        {
                            tempAngle = Vector3d.VectorAngle(iHalfMembers[i].HalfMemberLine.Direction, iHalfMembers[i].HalfMemberLine.Direction, planeXY);
                            if (tempAngle < minAngle) minAngle = tempAngle;
                        }
                    anglesBetweenMembers.Add(minAngle);
                }
                double _1 = anglesBetweenMembers.Max();
                int indexMaxAngle = anglesBetweenMembers.IndexOf(_1);
                Line newLine = iHalfMembers[indexMaxAngle].HalfMemberLine;
                Transform rotationTransformation = Transform.Rotation(_1 * 0.5, newLine.To);
                newLine.Transform(rotationTransformation);

                for (int i = 0; i < ExternalForceIndices.Count; i++)
                    iExternalForces[ExternalForceIndices[i]].ForceLineForAngle = newLine;
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

            Knowns = knowns;
            Unknowns = unknowns;
        }

    }
}
