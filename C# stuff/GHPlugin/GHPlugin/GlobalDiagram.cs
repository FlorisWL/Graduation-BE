using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHPlugin
{
    class GlobalDiagram
    {
        Resultant ForceResultant;
        List<SupportReaction> AllSupportReactions;
        List<Line> MemberLinesForm = new List<Line>();
        List<Line> MemberLinesForce = new List<Line>();
        List<Point3d> GlobalJoints = new List<Point3d>();

        public GlobalDiagram(Resultant forceResultant, List<SupportReaction> allSupportReactions)
        {
            ForceResultant = forceResultant;
            AllSupportReactions = allSupportReactions;

            Point3d topPoint = forceResultant.ResultantForm.To;
            Point3d supportPoint0 = allSupportReactions[0].FormLine.To;
            Point3d supportPoint1;

            if (allSupportReactions[1].FormLine.To == allSupportReactions[0].FormLine.To)
                supportPoint1 = allSupportReactions[2].FormLine.To;
            else
                supportPoint1 = allSupportReactions[1].FormLine.To;
            
            GlobalJoints.Add(topPoint);
            GlobalJoints.Add(supportPoint0);
            GlobalJoints.Add(supportPoint1);
        }

        public List<Member> Members()
        {
            List<Point3d> myPoints = GlobalJoints;
            List<int> startIntegers = new List<int>() { 0, 0, 1 };
            List<int> endIntegers = new List<int>() { 1, 2, 2 };

            List<Member> globalMembers = new List<Member>();

            for (int i = 0; i < startIntegers.Count; i++)
            {
                globalMembers.Add(new Member(startIntegers[i], endIntegers[i], GlobalJoints));
            }
            return globalMembers;
        }

        public List<Member> SolveForceDiagram(List<Member> memberList)
        {
            List<Point3d> myPoints = GlobalJoints;
            List<Joint> myJoints = new List<Joint>();
            for (int i = 0; i < myPoints.Count; i++)
            {
                myJoints.Add(new Joint(GlobalJoints[i]));
            }
            List<Member> myMembers = new List<Member>();
            for(int i = 0; i<myJoints.Count; i++)
            {
                //for (int j = 0; j < memberList.Count; j++)
                //    if (i == memberList[j].StartJointIndex)
                    
            }

            return myMembers;
        }
    }
}
