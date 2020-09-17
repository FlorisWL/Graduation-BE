using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHPlugin
{
    public class GeneralDiagram
    {
        public List<ExternalForce> AllExternalForces;
        public List<SupportReaction> AllSupportReactions;
        public List<Member> AllMembers;
        public List<HalfMember> allHalfMembers = new List<HalfMember>();
        public List<Point3d> AllJoints;
        public Point3d dummyPoint = new Point3d(0,0,0);

        public GeneralDiagram(
            List<ExternalForce> allExternalForces, List<SupportReaction> allSupportReactions, 
            List<Member> allMembers, List<Point3d> allJoints)
        {
            AllExternalForces = allExternalForces;
            AllSupportReactions = allSupportReactions;
            AllMembers = allMembers;
            AllJoints = allJoints;

            for (int i = 0; i < allMembers.Count; i++)
            {
                allHalfMembers.Add(new HalfMember(i, allMembers[i], true, AllJoints));
                allHalfMembers.Add(new HalfMember(i, allMembers[i], false, AllJoints));
            }
        }
       

        public void SolveForceDiagram()
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

            for (int i = 0; i < AllJoints.Count; i++)
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

                for (int j = 0; j < allHalfMembers.Count; j++)
                    if (i == allHalfMembers[j].JointIndex)
                    {
                        if (AllMembers[allHalfMembers[j].MemberIndex].Known == false)
                        {
                            unknowns += 1;
                            halfMemberIndices.Add(unknownForceLines.Count);
                            correspondingHalfMemberIndices.Add(j);
                            unknownForceLines.Add(allHalfMembers[j].HalfMemberLine);
                            unknownForceLinesForAngle.Add(allHalfMembers[j].HalfMemberLine);
                        }
                        else
                        {
                            knowns += 1;
                            forceLineTemp = AllMembers[allHalfMembers[j].MemberIndex].ForceLine;

                            if (Vector3d.Multiply(forceLineTemp.Direction, allHalfMembers[j].HalfMemberLine.Direction) > 0.0)
                            {
                                if (AllMembers[allHalfMembers[j].MemberIndex].PositiveForce == true)
                                    forceLineTemp.Flip();
                            }
                            else
                            {
                                if (AllMembers[allHalfMembers[j].MemberIndex].PositiveForce == false)
                                    forceLineTemp.Flip();
                            }
                            knownForceLines.Add(forceLineTemp);
                            knownForceLinesForAngles.Add(allHalfMembers[j].HalfMemberLine);
                        }
                    }

                for (int j = 0; j < AllSupportReactions.Count; j++)
                    if (i == AllSupportReactions[j].JointIndex)
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
                            knownForceLines.Add(AllSupportReactions[j].ForceLine);
                            knownForceLinesForAngles.Add(AllSupportReactions[j].ForceLineForAngle);
                        }
                    }

                for (int j = 0; j < AllExternalForces.Count; j++)
                {
                    if (i == AllExternalForces[j].JointIndex)
                    {
                        knowns += 1;
                        knownForceLines.Add(AllExternalForces[j].ForceLine);
                        knownForceLinesForAngles.Add(AllExternalForces[j].ForceLineForAngle);
                    }

                }

                if ((unknowns < 3) && (unknowns > 0))
                {
                    if (knowns > 1)
                    {
                        //startpoint (here dummypoint) for ResultantSimple gets overruled if knownForceLines.Count == 2, which should be the case here.
                        ResultantSimple myResultantSimple = new ResultantSimple(dummyPoint, knownForceLines, knownForceLinesForAngles, unknownForceLinesForAngle);
                        if (myResultantSimple.Valid == false)
                            break;
                        else
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
                            AllMembers[allHalfMembers[correspondingHalfMemberIndices[j]].MemberIndex].ForceLine = unknownForceLines[halfMemberIndices[j]];
                            AllMembers[allHalfMembers[correspondingHalfMemberIndices[j]].MemberIndex].Force = unknownForceLines[halfMemberIndices[j]].Length;
                            AllMembers[allHalfMembers[correspondingHalfMemberIndices[j]].MemberIndex].PositiveForce = myPostiveForces[halfMemberIndices[j]];
                            AllMembers[allHalfMembers[correspondingHalfMemberIndices[j]].MemberIndex].Known = true;
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
