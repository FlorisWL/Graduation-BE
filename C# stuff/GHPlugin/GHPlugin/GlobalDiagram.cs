using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHPlugin
{
    public class GlobalDiagram
    {
        public Resultant ForceResultant;
        public List<SupportReaction> AllSupportReactions;
        public List<Line> MemberLinesForm = new List<Line>();
        public List<Line> MemberLinesForce = new List<Line>();
        public List<HalfMember> HalfMembersForm = new List<HalfMember>();
        public List<Point3d> GlobalJoints = new List<Point3d>();
        public Point3d dummyPoint = new Point3d(0,0,0);

        public GlobalDiagram(Resultant forceResultant, List<SupportReaction> allSupportReactions)
        {
            ForceResultant = forceResultant;
            AllSupportReactions = allSupportReactions;
            List<Point3d> globalJoints = new List<Point3d>();

            Point3d topPoint = forceResultant.ResultantForm.To;
            Point3d supportPoint0 = allSupportReactions[0].FormLine.To;
            Point3d supportPoint1;

            if (allSupportReactions[1].FormLine.To == allSupportReactions[0].FormLine.To)
                supportPoint1 = allSupportReactions[2].FormLine.To;
            else
                supportPoint1 = allSupportReactions[1].FormLine.To;

            globalJoints.Add(topPoint);
            globalJoints.Add(supportPoint0);
            globalJoints.Add(supportPoint1);
            GlobalJoints = globalJoints;

            for (int i = 0; i<GlobalJoints.Count;i++)
            {
                for (int j = 0; j < GlobalJoints.Count; j++)
                {
                    if (GlobalJoints[i] == allSupportReactions[j].Joint)
                    {
                        allSupportReactions[j].GlobalJointIndex = i;
                    }

                }

            }
        }

        public List<Member> Members()
        {
            List<int> startIntegers = new List<int>() { 0, 0, 1 };
            List<int> endIntegers = new List<int>() { 1, 2, 2 };

            List<Member> globalMembers = new List<Member>();

            for (int i = 0; i < startIntegers.Count; i++)
            {
                globalMembers.Add(new Member(startIntegers[i], endIntegers[i], GlobalJoints));
                MemberLinesForm.Add(globalMembers[i].MemberLine);
            }

            for (int i = 0; i < globalMembers.Count; i++)
            {
                HalfMembersForm.Add(new HalfMember(i, globalMembers[i], true, GlobalJoints));
                HalfMembersForm.Add(new HalfMember(i, globalMembers[i], false, GlobalJoints));
            }

            return globalMembers;
        }

        

        public void SolveForceDiagram(List<Member> globalMembers)
        {
            Functions functions = new Functions();

            int unknowns;
            int knowns;

            List<int> halfMemberIndices;
            List<int> correspondingHalfMemberIndices;
            List<int> supportIndices;
            List<int> correspondingSupportIndices;
            List<Line> unknownForceLines;
            List<Line> knownForceLines;
            List<Line> knownForceLinesForAngles;
            List<Line> unknownForceLinesForAngle;
            Line knownForceLine;

            List<double> myAngles;
            List<Vector3d> myUnkownVectors;
            List<Boolean> myPostiveForces;
            Point3d myMiddlePoint;

            for (int i = 0; i < GlobalJoints.Count; i++)
            {
                unknowns = 0;
                knowns = 0;

                halfMemberIndices = new List<int>();  //Indices of myHalfMembers in unknownForceLines
                correspondingHalfMemberIndices = new List<int>();  //corresponding indices in myHalfMembers
                supportIndices = new List<int>();
                correspondingSupportIndices = new List<int>();
                unknownForceLines = new List<Line>();
                knownForceLines = new List<Line>();
                knownForceLinesForAngles = new List<Line>();
                unknownForceLinesForAngle = new List<Line>();

                Line knownForceLineForAngle;
                Line forceLineTemp;

                for (int j = 0; j < HalfMembersForm.Count; j++)
                    if (i == HalfMembersForm[j].JointIndex)
                    {
                        if (globalMembers[HalfMembersForm[j].MemberIndex].Known == false)
                        {
                            unknowns += 1;
                            halfMemberIndices.Add(unknownForceLines.Count);
                            correspondingHalfMemberIndices.Add(j);
                            unknownForceLines.Add(HalfMembersForm[j].HalfMemberLine);
                            unknownForceLinesForAngle.Add(HalfMembersForm[j].HalfMemberLine);
                        }
                        else
                        {
                            knowns += 1;
                            forceLineTemp = globalMembers[HalfMembersForm[j].MemberIndex].ForceLine;

                            if (Vector3d.Multiply(forceLineTemp.Direction, HalfMembersForm[j].HalfMemberLine.Direction) > 0.0)
                            {
                                if (globalMembers[HalfMembersForm[j].MemberIndex].PositiveForce == true)
                                    forceLineTemp.Flip();
                            }
                            else
                            {
                                if (globalMembers[HalfMembersForm[j].MemberIndex].PositiveForce == false)
                                    forceLineTemp.Flip();
                            }
                            knownForceLines.Add(forceLineTemp);
                            knownForceLinesForAngles.Add(HalfMembersForm[j].HalfMemberLine);
                        }
                    }

                for (int j = 0; j < AllSupportReactions.Count; j++)
                    if (GlobalJoints[i].DistanceToSquared(AllSupportReactions[j].Joint) < 0.00001)
                    {
                        if (AllSupportReactions[j].Known == false)
                        {
                            unknowns += 1;
                            supportIndices.Add(unknownForceLines.Count);
                            correspondingSupportIndices.Add(j);
                            unknownForceLines.Add(AllSupportReactions[j].FormLine);
                            unknownForceLinesForAngle.Add(AllSupportReactions[j].FormLineForAngle);
                        }
                        else
                        {
                            knowns += 1;
                            knownForceLines.Add(AllSupportReactions[i].ForceLine);
                            knownForceLinesForAngles.Add(AllSupportReactions[i].ForceLine);
                        }
                    }

                if (GlobalJoints[i].DistanceToSquared(ForceResultant.ResultantForm.To) < 0.00001)
                {
                    knowns += 1;
                    knownForceLines.Add(ForceResultant.ResultantForce);
                    knownForceLinesForAngles.Add(ForceResultant.ResultantForceForAngle);
                }

                if ((unknowns < 3) && (unknowns > 0))
                {
                    if (knowns > 1)
                    {
                        //startpoint (here dummypoint) for ResultantSimple gets overruled if knownForceLines.Count == 2, which should be the case here.
                        ResultantSimple myResultantSimple = new ResultantSimple(dummyPoint, knownForceLines, knownForceLinesForAngles);
                        knownForceLine = myResultantSimple.ResultantLine;
                        
                    }
                    else
                        knownForceLine = knownForceLines[0];

                    knownForceLineForAngle = knownForceLinesForAngles[0];
                    functions.FindAngles(knownForceLineForAngle, unknownForceLinesForAngle, unknownForceLines, out myAngles, out myUnkownVectors);
                    bool valid = functions.ThreeForceJoint(myAngles, knownForceLine, myUnkownVectors, out unknownForceLines, out myPostiveForces, out myMiddlePoint);

                    if (valid)
                    {
                        for (int j = 0; j < halfMemberIndices.Count; j++)
                        {
                            globalMembers[HalfMembersForm[correspondingHalfMemberIndices[j]].MemberIndex].ForceLine = unknownForceLines[halfMemberIndices[j]];
                            globalMembers[HalfMembersForm[correspondingHalfMemberIndices[j]].MemberIndex].Force = unknownForceLines[halfMemberIndices[j]].Length;
                            globalMembers[HalfMembersForm[correspondingHalfMemberIndices[j]].MemberIndex].PositiveForce = myPostiveForces[halfMemberIndices[j]];
                            globalMembers[HalfMembersForm[correspondingHalfMemberIndices[j]].MemberIndex].Known = true;
                            MemberLinesForce.Add(unknownForceLines[halfMemberIndices[j]]);
                        }

                        for (int j = 0; j < supportIndices.Count; j++)
                        {
                            AllSupportReactions[correspondingSupportIndices[j]].ForceLine = unknownForceLines[supportIndices[j]];
                            AllSupportReactions[correspondingSupportIndices[j]].Force = unknownForceLines[supportIndices[j]].Length;
                            AllSupportReactions[correspondingSupportIndices[j]].PositiveForce = myPostiveForces[supportIndices[j]];
                            AllSupportReactions[correspondingSupportIndices[j]].Known = true;
                        }
                    }
                    else
                        //message: Print("Error: system is unstable! Try changing the support reactions.");

                    return;

                }

            }
        }
    }
}
