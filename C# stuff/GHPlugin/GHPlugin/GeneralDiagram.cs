using Rhino.Geometry;
using Rhino.Geometry.Intersect;
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
        public List<HalfMember> AllHalfMembers = new List<HalfMember>();
        public List<Point3d> AllPoints;
        public List<Joint> AllJoints = new List<Joint>();
        public Point3d DummyPoint = new Point3d(0,0,0);
        public Functions MyFunctions = new Functions();

        public GeneralDiagram(
            List<ExternalForce> allExternalForces, List<SupportReaction> allSupportReactions, 
            List<Member> allMembers, List<Point3d> allPoints)
        {
            AllExternalForces = allExternalForces;
            AllSupportReactions = allSupportReactions;
            AllMembers = allMembers;
            AllPoints = allPoints;

            for (int i = 0; i < allMembers.Count; i++)
            {
                AllHalfMembers.Add(new HalfMember(i, allMembers[i], true, AllPoints));
                AllHalfMembers.Add(new HalfMember(i, allMembers[i], false, AllPoints));
            }

            for (int i = 0; i < allPoints.Count; i++)
            {
                AllJoints.Add(new Joint(i, allPoints));
                AllJoints[i].FillJoint(AllHalfMembers, AllExternalForces, allSupportReactions);
            }
        }

        public List<int> KnownUnknownLines(Joint joint, ref List<Line> oKnownForceLines, ref List<Line> oUnknownForceLines, ref List<Line> oKnownForceLinesForAngles, ref List<int> oIndices, ref List<int> oCorrespondingIndices)
        {
            List<Line> knownForceLines = new List<Line>();
            List<Line> unknownForceLines = new List<Line>();

            Line flippedForceLineJoint;

            List<int> indexKnownMembers = new List<int>();
            List<int> indexKnownMembersCorresponding = new List<int>();
            List<int> indexUnknownMembers = new List<int>();
            List<int> indexUnknownMembersCorresponding = new List<int>();
            List<int> indexSupports = new List<int>();
            List<int> indexSupportsCorresponding = new List<int>();
            List<int> indexExtForces = new List<int>();
            List<int> indexExtForcesCorresponding = new List<int>();

            List<int> distribution = new List<int>();
            for (int i = 0; i < 4; i++)
                distribution.Add(0);

            for (int i = 0; i < joint.MemberIndices.Count; i++)
            {
                if (AllMembers[joint.MemberIndices[i]].Known)
                {
                    indexKnownMembers.Add(joint.MemberIndices[i]);
                    indexKnownMembersCorresponding.Add(knownForceLines.Count);
                    flippedForceLineJoint = AllMembers[joint.MemberIndices[i]].ForceLineJoint1; flippedForceLineJoint.Flip();
                    knownForceLines.Add(flippedForceLineJoint);
                    distribution[0]++;
                }

                else
                {
                    indexUnknownMembers.Add(joint.MemberIndices[i]);
                    indexUnknownMembersCorresponding.Add(unknownForceLines.Count);
                    unknownForceLines.Add(AllHalfMembers[joint.HalfMemberIndices[i]].HalfMemberLine);
                    distribution[3]++;
                }
            }

            for (int i = 0; i < joint.ExternalForceIndices.Count; i++)
            {
                indexSupports.Add(joint.ExternalForceIndices[i]);
                indexSupportsCorresponding.Add(knownForceLines.Count);
                knownForceLines.Add(AllExternalForces[joint.ExternalForceIndices[i]].ForceLine);
                distribution[2]++;
            }

            for (int i = 0; i < joint.SupportReactionIndices.Count; i++)
            {
                indexExtForces.Add(joint.SupportReactionIndices[i]);
                indexExtForcesCorresponding.Add(knownForceLines.Count);
                knownForceLines.Add(AllSupportReactions[joint.SupportReactionIndices[i]].ForceLine);
                distribution[1]++;
            }

            oIndices = new List<int>();
            for (int i = 0; i < indexKnownMembers.Count; i++)
                oIndices.Add(indexKnownMembers[i]);
            for (int i = 0; i < indexSupports.Count; i++)
                oIndices.Add(indexSupports[i]);
            for (int i = 0; i < indexExtForces.Count; i++)
                oIndices.Add(indexExtForces[i]);
            for (int i = 0; i < indexUnknownMembers.Count; i++)
                oIndices.Add(indexUnknownMembers[i]);

            oCorrespondingIndices = new List<int>();
            for (int i = 0; i < indexKnownMembersCorresponding.Count; i++)
                oCorrespondingIndices.Add(indexKnownMembersCorresponding[i]);
            for (int i = 0; i < indexSupportsCorresponding.Count; i++)
                oCorrespondingIndices.Add(indexSupportsCorresponding[i]);
            for (int i = 0; i < indexExtForcesCorresponding.Count; i++)
                oCorrespondingIndices.Add(indexExtForcesCorresponding[i]);
            for (int i = 0; i < indexUnknownMembersCorresponding.Count; i++)
                oCorrespondingIndices.Add(indexUnknownMembersCorresponding[i]);

            oKnownForceLines = knownForceLines;
            oUnknownForceLines = unknownForceLines;
            oKnownForceLinesForAngles = knownForceLines;

            return distribution;
        }

        public void SetFormLinesOrder(List<Line> iKnownForceLinesForAngles, List<Line> iUnknownForceLines, ref List<int> oKnownForceLinesOrder, ref List<int> oUnknownForceLinesOrder)
        {
            List<double> myAnglesKnownLines = new List<double>();
            List<double> myAnglesUnknownLines = new List<double>();
            oKnownForceLinesOrder = new List<int>(new int[iKnownForceLinesForAngles.Count]);
            oUnknownForceLinesOrder = new List<int>(new int[iUnknownForceLines.Count]);

            Plane planeXY = new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, 1));
            Vector3d positiveY = new Vector3d(0, 1, 0);

            for (int i = 0; i < iKnownForceLinesForAngles.Count; i++)
            {
                myAnglesKnownLines.Add(Vector3d.VectorAngle(positiveY, iKnownForceLinesForAngles[i].Direction, planeXY));
            }

            for (int i = 0; i < iUnknownForceLines.Count; i++)
            {
                myAnglesUnknownLines.Add(Vector3d.VectorAngle(positiveY, iUnknownForceLines[i].Direction, planeXY));
            }

            for (int i = 0; i < (iUnknownForceLines.Count + iKnownForceLinesForAngles.Count); i++)
            {
                if (myAnglesUnknownLines.Count == 0)
                {
                    int _1 = myAnglesKnownLines.IndexOf(myAnglesKnownLines.Min());
                    oKnownForceLinesOrder[_1] = i;
                    myAnglesKnownLines[_1] = 400;
                }
                else if (myAnglesKnownLines.Min() < myAnglesUnknownLines.Min())
                {
                    int _1 = myAnglesKnownLines.IndexOf(myAnglesKnownLines.Min());
                    oKnownForceLinesOrder[_1] = i;
                    myAnglesKnownLines[_1] = 400;
                }
                else
                {
                    int _1 = myAnglesUnknownLines.IndexOf(myAnglesUnknownLines.Min());
                    oUnknownForceLinesOrder[_1] = i;
                    myAnglesUnknownLines[_1] = 400;
                }
            }
        }

        public void CalculateFormDiagramJoint(List<Line> iKnownForceLines, List<Line> iUnknownForceLines, Joint iJoint, ref List<Line> oSolvedKnownForceLines, ref List<Line> oSolvedUnknownForceLines)
        {
            oSolvedKnownForceLines = new List<Line>();
            oSolvedUnknownForceLines = new List<Line>();

            Point3d OGstartPoint = iJoint.InitSolveLocation;
            Point3d startPoint = OGstartPoint;
            bool valid; double pA; double pB; Point3d intersectionPoint;

            for (int i = 0; i < iKnownForceLines.Count; i++)
            {
                oSolvedKnownForceLines.Add(new Line(startPoint, iKnownForceLines[i].Direction));
                startPoint = oSolvedKnownForceLines[i].To;
            }

            if (iUnknownForceLines.Count == 1)
            {
                oSolvedUnknownForceLines.Add(new Line(startPoint, OGstartPoint));
            }
            else if (iUnknownForceLines.Count == 2)
            {
                oSolvedUnknownForceLines.Add(new Line(startPoint, iUnknownForceLines[0].Direction));
                oSolvedUnknownForceLines.Add(new Line(OGstartPoint, iUnknownForceLines[1].Direction));
                valid = Intersection.LineLine(oSolvedUnknownForceLines[0], oSolvedUnknownForceLines[1], out pA, out pB);
                if (valid)
                {
                    intersectionPoint = oSolvedUnknownForceLines[0].PointAt(pA);
                    oSolvedUnknownForceLines[0] = new Line(startPoint, intersectionPoint);
                    oSolvedUnknownForceLines[1] = new Line(intersectionPoint, OGstartPoint);
                }
            }
        }

        public void SolveForceDiagramJointBased()
        {
            List<Line> myKnownForceLines = new List<Line>();
            List<Line> myKnownForceLinesForAngles = new List<Line>();
            List<Line> mySolvedKnownForceLines = new List<Line>();
            List<int> myKnownForceLinesOrder = new List<int>();
            List<Line> myUnknownForceLines = new List<Line>();
            List<Line> mySolvedUnknownForceLines = new List<Line>();
            List<int> myUnkownForceLinesOrder = new List<int>();
            List<int> myCorrespondingIndices = new List<int>();
            List<int> myIndices = new List<int>();

            List<int> indexKnownMembers; ;
            List<int> indexKnownMembersCorresponding; ;
            List<int> indexUnknownMembers; ;
            List<int> indexUnknownMembersCorresponding; ;
            List<int> indexSupports;
            List<int> indexSupportsCorresponding;
            List<int> indexExtForces;
            List<int> indexExtForcesCorresponding;

            for (int i = 0; i < AllJoints.Count; i++)
            {
                if(i == 4)
                {
                    int test = 1;
                }
                if(AllJoints[i].JointSolved == false)
                {
                    AllJoints[i].KnownsUnknowns(AllMembers);
                    if ((AllJoints[i].Unknowns < 3) && (AllJoints[i].Unknowns > -1))
                    {
                        indexKnownMembers = new List<int>();
                        indexKnownMembersCorresponding = new List<int>();
                        indexUnknownMembers = new List<int>();
                        indexUnknownMembersCorresponding = new List<int>();
                        indexSupports = new List<int>();
                        indexSupportsCorresponding = new List<int>();
                        indexExtForces = new List<int>();
                        indexExtForcesCorresponding = new List<int>();

                        List<int> myDistribtution = KnownUnknownLines(AllJoints[i], ref myKnownForceLines, ref myUnknownForceLines, ref myKnownForceLinesForAngles, ref myIndices, ref myCorrespondingIndices);
                        SetFormLinesOrder(myKnownForceLinesForAngles, myUnknownForceLines, ref myKnownForceLinesOrder, ref myUnkownForceLinesOrder);
                        CalculateFormDiagramJoint(myKnownForceLines, myUnknownForceLines, AllJoints[i], ref mySolvedKnownForceLines, ref mySolvedUnknownForceLines);

                        for (int j = 0; j < myIndices.Count; j++)
                        {
                            if (j < myDistribtution[0])
                            {
                                indexKnownMembers.Add(myIndices[j]);
                                indexKnownMembersCorresponding.Add(myCorrespondingIndices[j]);
                            }
                            else if (j < myDistribtution[1] + myDistribtution[0])
                            {
                                indexSupports.Add(myIndices[j]);
                                indexSupportsCorresponding.Add(myCorrespondingIndices[j]);
                            }
                            else if (j < myDistribtution[2] + myDistribtution[1] + myDistribtution[0])
                            {
                                indexExtForces.Add(myIndices[j]);
                                indexExtForcesCorresponding.Add(myCorrespondingIndices[j]);
                            }
                            else
                            {
                                indexUnknownMembers.Add(myIndices[j]);
                                indexUnknownMembersCorresponding.Add(myCorrespondingIndices[j]);
                            }
                        }

                        for (int j = 0; j < indexUnknownMembers.Count; j++)
                        {
                            AllMembers[indexUnknownMembers[j]].ForceLineJoint1 = mySolvedUnknownForceLines[indexUnknownMembersCorresponding[j]];
                            AllMembers[indexUnknownMembers[j]].Known = true;
                            AllMembers[indexUnknownMembers[j]].Force = AllMembers[indexUnknownMembers[j]].ForceLineJoint1.Length;
                            if (Vector3d.Multiply(myUnknownForceLines[indexUnknownMembersCorresponding[j]].Direction, mySolvedUnknownForceLines[indexUnknownMembersCorresponding[j]].Direction) > 0.0)
                                AllMembers[indexUnknownMembers[j]].PositiveForce = false;
                            else
                                AllMembers[indexUnknownMembers[j]].PositiveForce = true;
                        }

                        for (int j = 0; j < indexKnownMembers.Count; j++)
                            AllMembers[indexKnownMembers[j]].ForceLineJoint2 = mySolvedKnownForceLines[indexKnownMembersCorresponding[j]];

                        for (int j = 0; j < indexSupports.Count; j++)
                            AllSupportReactions[indexSupports[j]].ForceLineJoint = mySolvedKnownForceLines[indexSupportsCorresponding[j]];

                        for (int j = 0; j < indexExtForces.Count; j++)
                            AllExternalForces[indexExtForces[j]].ForceLineJoint = mySolvedKnownForceLines[indexExtForcesCorresponding[j]];

                        AllJoints[i].JointSolved = true;
                    }
                }
            }
        }

        public void SolveSingleUnknownJoint()
        {
            
        }

        public void SolveForceDiagram()
        {
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

            for (int i = 0; i < AllPoints.Count; i++)
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

                for (int j = 0; j < AllHalfMembers.Count; j++)
                    if (i == AllHalfMembers[j].JointIndex)
                    {
                        if (AllMembers[AllHalfMembers[j].MemberIndex].Known == false)
                        {
                            unknowns += 1;
                            halfMemberIndices.Add(unknownForceLines.Count);
                            correspondingHalfMemberIndices.Add(j);
                            unknownForceLines.Add(AllHalfMembers[j].HalfMemberLine);
                            unknownForceLinesForAngle.Add(AllHalfMembers[j].HalfMemberLine);
                        }
                        else
                        {
                            knowns += 1;
                            forceLineTemp = AllMembers[AllHalfMembers[j].MemberIndex].ForceLine;

                            if (Vector3d.Multiply(forceLineTemp.Direction, AllHalfMembers[j].HalfMemberLine.Direction) > 0.0)
                            {
                                if (AllMembers[AllHalfMembers[j].MemberIndex].PositiveForce == true)
                                    forceLineTemp.Flip();
                            }
                            else
                            {
                                if (AllMembers[AllHalfMembers[j].MemberIndex].PositiveForce == false)
                                    forceLineTemp.Flip();
                            }
                            knownForceLines.Add(forceLineTemp);
                            knownForceLinesForAngles.Add(AllHalfMembers[j].HalfMemberLine);
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
                        ResultantSimple myResultantSimple = new ResultantSimple(DummyPoint, knownForceLines, knownForceLinesForAngles, unknownForceLinesForAngle);
                        if (myResultantSimple.Valid == false)
                            break;
                        else
                            knownForceLine = myResultantSimple.ResultantLine;
                        
                    }
                    else
                        knownForceLine = knownForceLines[0];

                    knownForceLineForAngle = knownForceLinesForAngles[0];
                    MyFunctions.FindAngles(knownForceLineForAngle, unknownForceLinesForAngle, unknownForceLines, out myAngles, out myUnkownVectors);
                    bool valid = MyFunctions.ThreeForceJoint(myAngles, knownForceLine, myUnkownVectors, out unknownForceLines, out myPostiveForces, out myMiddlePoint);

                    if (valid)
                    {
                        for (int j = 0; j < halfMemberIndices.Count; j++)
                        {
                            AllMembers[AllHalfMembers[correspondingHalfMemberIndices[j]].MemberIndex].ForceLine = unknownForceLines[halfMemberIndices[j]];
                            AllMembers[AllHalfMembers[correspondingHalfMemberIndices[j]].MemberIndex].Force = unknownForceLines[halfMemberIndices[j]].Length;
                            AllMembers[AllHalfMembers[correspondingHalfMemberIndices[j]].MemberIndex].PositiveForce = myPostiveForces[halfMemberIndices[j]];
                            AllMembers[AllHalfMembers[correspondingHalfMemberIndices[j]].MemberIndex].Known = true;
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
