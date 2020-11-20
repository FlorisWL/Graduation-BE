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
        public List<Joint> AllJoints = new List<Joint>();
        public Point3d StartPoint = new Point3d(0,0,0);
        public Functions MyFunctions = new Functions();

        public GeneralDiagram(
            List<ExternalForce> allExternalForces, List<SupportReaction> allSupportReactions, 
            List<Member> allMembers, List<Point3d> allPoints, double scaleFactorSpacing)
        {
            AllExternalForces = allExternalForces;
            AllSupportReactions = allSupportReactions;
            AllMembers = allMembers;

            StartPoint = allExternalForces[0].ForceLine.From;

            for (int i = 0; i < allMembers.Count; i++)
            {
                AllHalfMembers.Add(new HalfMember(i, allMembers[i], true, allPoints));
                AllHalfMembers[AllHalfMembers.Count - 1].OtherHalfMemberIndex = (2 * i + 1);
                AllHalfMembers.Add(new HalfMember(i, allMembers[i], false, allPoints));
                AllHalfMembers[AllHalfMembers.Count - 1].OtherHalfMemberIndex = (2 * i);
            }

            for (int i = 0; i < allPoints.Count; i++)
            {
                AllJoints.Add(new Joint(i, allPoints, scaleFactorSpacing));
                AllJoints[i].FillJoint(AllHalfMembers, AllExternalForces, allSupportReactions);
            }

            for (int i = 0; i < AllJoints.Count; i++)
                AllJoints[i].SetExternalForcesForAngle(AllHalfMembers, AllExternalForces, AllSupportReactions);
        }

        public List<int> KnownUnknownLines(Joint joint, ref List<Line> oKnownForceLines, ref List<Line> oUnknownForceLines, ref List<Line> oKnownForceLinesForAngles, ref List<int> oIndices, ref List<int> oCorrespondingIndices)
        {
            List<Line> knownForceLines = new List<Line>();
            List<Line> unknownForceLines = new List<Line>();
            List<Line> knownForceLinesForAngles = new List<Line>();

            Line flippedForceLineJoint;

            List<int> indexKnownHalfMembers = new List<int>();
            List<int> indexKnownHalfMembersCorresponding = new List<int>();
            List<int> indexUnknownHalfMembers = new List<int>();
            List<int> indexUnknownHalfMembersCorresponding = new List<int>();
            List<int> indexSupports = new List<int>();
            List<int> indexSupportsCorresponding = new List<int>();
            List<int> indexExtForces = new List<int>();
            List<int> indexExtForcesCorresponding = new List<int>();

            List<int> distribution = new List<int>();
            for (int i = 0; i < 4; i++)
                distribution.Add(0);

            for (int i = 0; i < joint.HalfMemberIndices.Count; i++)
            {
                if (AllMembers[joint.MemberIndices[i]].Known)
                {
                    indexKnownHalfMembers.Add(joint.HalfMemberIndices[i]);
                    indexKnownHalfMembersCorresponding.Add(knownForceLines.Count);
                    flippedForceLineJoint = AllHalfMembers[AllHalfMembers[joint.HalfMemberIndices[i]].OtherHalfMemberIndex].ForceLineJoint; flippedForceLineJoint.Flip();
                    knownForceLines.Add(flippedForceLineJoint);
                    knownForceLinesForAngles.Add(AllHalfMembers[joint.HalfMemberIndices[i]].FormLine);
                    distribution[0]++;
                }

                else
                {
                    indexUnknownHalfMembers.Add(joint.HalfMemberIndices[i]);
                    indexUnknownHalfMembersCorresponding.Add(unknownForceLines.Count);
                    unknownForceLines.Add(AllHalfMembers[joint.HalfMemberIndices[i]].FormLine);
                    distribution[3]++;
                }
            }

            for (int i = 0; i < joint.ExternalForceIndices.Count; i++)
            {
                indexSupports.Add(joint.ExternalForceIndices[i]);
                indexSupportsCorresponding.Add(knownForceLines.Count);
                knownForceLines.Add(AllExternalForces[joint.ExternalForceIndices[i]].ForceLine);
                knownForceLinesForAngles.Add(AllExternalForces[joint.ExternalForceIndices[i]].ForceLineForAngle);
                distribution[2]++;
            }

            for (int i = 0; i < joint.SupportReactionIndices.Count; i++)
            {
                indexExtForces.Add(joint.SupportReactionIndices[i]);
                indexExtForcesCorresponding.Add(knownForceLines.Count);
                knownForceLines.Add(AllSupportReactions[joint.SupportReactionIndices[i]].ForceLine);
                knownForceLinesForAngles.Add(AllSupportReactions[joint.SupportReactionIndices[i]].ForceLineForAngle);
                distribution[1]++;
            }

            oIndices = new List<int>();
            for (int i = 0; i < indexKnownHalfMembers.Count; i++)
                oIndices.Add(indexKnownHalfMembers[i]);
            for (int i = 0; i < indexSupports.Count; i++)
                oIndices.Add(indexSupports[i]);
            for (int i = 0; i < indexExtForces.Count; i++)
                oIndices.Add(indexExtForces[i]);
            for (int i = 0; i < indexUnknownHalfMembers.Count; i++)
                oIndices.Add(indexUnknownHalfMembers[i]);

            oCorrespondingIndices = new List<int>();
            for (int i = 0; i < indexKnownHalfMembersCorresponding.Count; i++)
                oCorrespondingIndices.Add(indexKnownHalfMembersCorresponding[i]);
            for (int i = 0; i < indexSupportsCorresponding.Count; i++)
                oCorrespondingIndices.Add(indexSupportsCorresponding[i]);
            for (int i = 0; i < indexExtForcesCorresponding.Count; i++)
                oCorrespondingIndices.Add(indexExtForcesCorresponding[i]);
            for (int i = 0; i < indexUnknownHalfMembersCorresponding.Count; i++)
                oCorrespondingIndices.Add(indexUnknownHalfMembersCorresponding[i]);

            oKnownForceLines = knownForceLines;
            oUnknownForceLines = unknownForceLines;
            oKnownForceLinesForAngles = knownForceLinesForAngles;

            

            return distribution;
        }


        public void SetFormLinesOrder(List<Line> iKnownForceLinesForAngles, List<Line> iUnknownForceLines, ref List<int> oKnownForceLinesOrder, ref List<int> oUnknownForceLinesOrder)
        {
            List<double> myAnglesKnownLines = new List<double>();
            List<double> myAnglesUnknownLines = new List<double>();
            oKnownForceLinesOrder = new List<int>(new int[iKnownForceLinesForAngles.Count]);
            oUnknownForceLinesOrder = new List<int>(new int[iUnknownForceLines.Count]);

            Plane planeXY = new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, -1));
            Vector3d positiveY = new Vector3d(0, 1, 0);

            for (int i = 0; i < iKnownForceLinesForAngles.Count; i++)
            {
                myAnglesKnownLines.Add(Vector3d.VectorAngle(positiveY, iKnownForceLinesForAngles[i].Direction, planeXY));
            }

            for (int i = 0; i < iUnknownForceLines.Count; i++)
            {
                myAnglesUnknownLines.Add(Vector3d.VectorAngle(positiveY, iUnknownForceLines[i].Direction, planeXY));
            }

            if (iKnownForceLinesForAngles.Count != 0)
            {
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
            else
            {
                for (int i = 0; i < (iUnknownForceLines.Count + iKnownForceLinesForAngles.Count); i++)
                {
                    if (myAnglesUnknownLines.Count != 0)
                    {
                        int _1 = myAnglesUnknownLines.IndexOf(myAnglesUnknownLines.Min());
                        oUnknownForceLinesOrder[_1] = i;
                        myAnglesUnknownLines[_1] = 400;
                    }
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

        public void SetInRightOrder(List<int> iKnownForceLinesOrder, List<int> iUnknownForceLinesOrder, ref List<Line> oKnownForceLines, ref List<Line> oUnknownForceLines, Joint iJoint)
        {
            //List<Line> newKnownForceLines = new List<Line>();
            //List<Line> newUnknownForceLines = new List<Line>();
            Line newLine;
            bool myBreak;
            Point3d startPoint = iJoint.InitSolveLocation;

            for (int j = 0; j < (iKnownForceLinesOrder.Count + iUnknownForceLinesOrder.Count); j++)
            {
                myBreak = false;

                for (int i = 0; i < (iKnownForceLinesOrder.Count); i++)
                {
                    if (j == iKnownForceLinesOrder[i])
                    {
                        newLine = new Line(startPoint, oKnownForceLines[i].Direction);
                        oKnownForceLines[i] = newLine;
                        startPoint = newLine.To;

                        myBreak = true;
                        break;
                    }
                }

                for (int i = 0; i < (iUnknownForceLinesOrder.Count); i++)
                {
                    if (myBreak) break;

                    if (j == iUnknownForceLinesOrder[i])
                    {
                        newLine = new Line(startPoint, oUnknownForceLines[i].Direction);
                        oUnknownForceLines[i] = newLine;
                        startPoint = newLine.To;

                        break;
                    }
                }
            }
                
            //oKnownForceLines = newKnownForceLines;
            //oUnknownForceLines = newUnknownForceLines;
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

            List<int> indexKnownHalfMembers; ;
            List<int> indexKnownHalfMembersCorresponding; ;
            List<int> indexUnknownHalfMembers; ;
            List<int> indexUnknownHalfMembersCorresponding; ;
            List<int> indexSupports;
            List<int> indexSupportsCorresponding;
            List<int> indexExtForces;
            List<int> indexExtForcesCorresponding;

            for (int i = 0; i < AllJoints.Count; i++)
            {
                if(AllJoints[i].JointSolved == false)
                {
                    AllJoints[i].KnownsUnknowns(AllMembers);
                    if ((AllJoints[i].Unknowns < 3) && (AllJoints[i].Unknowns > -1))
                    {
                        indexKnownHalfMembers = new List<int>();
                        indexKnownHalfMembersCorresponding = new List<int>();
                        indexUnknownHalfMembers = new List<int>();
                        indexUnknownHalfMembersCorresponding = new List<int>();
                        indexSupports = new List<int>();
                        indexSupportsCorresponding = new List<int>();
                        indexExtForces = new List<int>();
                        indexExtForcesCorresponding = new List<int>();

                        List<int> myDistribtution = KnownUnknownLines(AllJoints[i], ref myKnownForceLines, ref myUnknownForceLines, ref myKnownForceLinesForAngles, ref myIndices, ref myCorrespondingIndices);
                        SetFormLinesOrder(myKnownForceLinesForAngles, myUnknownForceLines, ref myKnownForceLinesOrder, ref myUnkownForceLinesOrder);
                        CalculateFormDiagramJoint(myKnownForceLines, myUnknownForceLines, AllJoints[i], ref mySolvedKnownForceLines, ref mySolvedUnknownForceLines);
                        SetInRightOrder(myKnownForceLinesOrder, myUnkownForceLinesOrder, ref mySolvedKnownForceLines, ref mySolvedUnknownForceLines, AllJoints[i]);

                        for (int j = 0; j < myIndices.Count; j++)
                        {
                            if (j < myDistribtution[0])
                            {
                                indexKnownHalfMembers.Add(myIndices[j]);
                                indexKnownHalfMembersCorresponding.Add(myCorrespondingIndices[j]);
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
                                indexUnknownHalfMembers.Add(myIndices[j]);
                                indexUnknownHalfMembersCorresponding.Add(myCorrespondingIndices[j]);
                            }
                        }

                        for (int j = 0; j < indexUnknownHalfMembers.Count; j++)
                        {
                            AllHalfMembers[indexUnknownHalfMembers[j]].ForceLineJoint = mySolvedUnknownForceLines[indexUnknownHalfMembersCorresponding[j]];
                            AllMembers[AllHalfMembers[indexUnknownHalfMembers[j]].MemberIndex].Known = true;
                            AllMembers[AllHalfMembers[indexUnknownHalfMembers[j]].MemberIndex].Force = AllHalfMembers[indexUnknownHalfMembers[j]].ForceLineJoint.Length;
                            if (Vector3d.Multiply(myUnknownForceLines[indexUnknownHalfMembersCorresponding[j]].Direction, mySolvedUnknownForceLines[indexUnknownHalfMembersCorresponding[j]].Direction) > 0.0)
                                AllMembers[AllHalfMembers[indexUnknownHalfMembers[j]].MemberIndex].PositiveForce = false;
                            else
                                AllMembers[AllHalfMembers[indexUnknownHalfMembers[j]].MemberIndex].PositiveForce = true;
                        }

                        for (int j = 0; j < indexKnownHalfMembers.Count; j++)
                            AllHalfMembers[indexKnownHalfMembers[j]].ForceLineJoint = mySolvedKnownForceLines[indexKnownHalfMembersCorresponding[j]];

                        for (int j = 0; j < indexSupports.Count; j++)
                            AllSupportReactions[indexSupports[j]].ForceLineJoint = mySolvedKnownForceLines[indexSupportsCorresponding[j]];

                        for (int j = 0; j < indexExtForces.Count; j++)
                            AllExternalForces[indexExtForces[j]].ForceLineJoint = mySolvedKnownForceLines[indexExtForcesCorresponding[j]];

                        AllJoints[i].JointSolved = true;
                    }
                }
            }
        }

        public List<Line> ForceLinesJoint()
        {
            List<Line> oMemberLinesForce = new List<Line>();

            for (int i = 0; i < AllHalfMembers.Count; i++)
                oMemberLinesForce.Add(AllHalfMembers[i].ForceLineJoint);

            return oMemberLinesForce;
        }

        
        public void CreateOverallForceDiagram()
        {
            //create overall force diagram
            int solvedJoints = 0;
            Vector3d tranformationVector = new Vector3d(StartPoint);
            tranformationVector -= new Vector3d(AllJoints[0].InitSolveLocation);

            Transform transformationMatrix = Transform.Translation(tranformationVector);
            Line tempLine;

            for (int k = 0; k < AllJoints[0].HalfMemberIndices.Count; k++)
            {
                tempLine = AllHalfMembers[AllJoints[0].HalfMemberIndices[k]].ForceLineJoint;
                tempLine.Transform(transformationMatrix);
                AllMembers[AllHalfMembers[AllJoints[0].HalfMemberIndices[k]].MemberIndex].ForceLine = tempLine;
            }

            for (int k = 0; k < AllJoints[0].ExternalForceIndices.Count; k++)
            {
                tempLine = AllExternalForces[AllJoints[0].ExternalForceIndices[k]].ForceLineJoint;
                tempLine.Transform(transformationMatrix);
                AllExternalForces[AllJoints[0].ExternalForceIndices[k]].ForceLine = tempLine;
            }

            for (int k = 0; k < AllJoints[0].SupportReactionIndices.Count; k++)
            {
                tempLine = AllSupportReactions[AllJoints[0].SupportReactionIndices[k]].ForceLineJoint;
                tempLine.Transform(transformationMatrix);
                AllSupportReactions[AllJoints[0].SupportReactionIndices[k]].ForceLine = tempLine;
            }

            AllJoints[0].Relocated = true;
            solvedJoints++;
            for (int l = 0; l < (AllJoints.Count); l++)
            {
                for (int i = 0; i < (AllJoints.Count); i++)
                {
                    if (AllJoints[i].Relocated == false)
                    {
                        for (int j = 0; j < AllJoints[i].HalfMemberIndices.Count; j++)
                        {
                            if ((AllHalfMembers[AllHalfMembers[AllJoints[i].HalfMemberIndices[j]].OtherHalfMemberIndex].MemberIndex == AllHalfMembers[AllJoints[i].HalfMemberIndices[j]].MemberIndex)
                                && (AllJoints[AllHalfMembers[AllHalfMembers[AllJoints[i].HalfMemberIndices[j]].OtherHalfMemberIndex].JointIndex].Relocated))
                            {
                                tranformationVector = new Vector3d(AllMembers[AllHalfMembers[AllHalfMembers[AllJoints[i].HalfMemberIndices[j]].OtherHalfMemberIndex].MemberIndex].ForceLine.To);
                                tranformationVector -= new Vector3d(AllHalfMembers[AllJoints[i].HalfMemberIndices[j]].ForceLineJoint.From);
                                transformationMatrix = Transform.Translation(tranformationVector);

                                for (int k = 0; k < AllJoints[i].HalfMemberIndices.Count; k++)
                                {
                                    if (AllMembers[AllHalfMembers[AllJoints[i].HalfMemberIndices[k]].MemberIndex].ForceLine.Length <= 0)
                                    {
                                        tempLine = AllHalfMembers[AllJoints[i].HalfMemberIndices[k]].ForceLineJoint;
                                        tempLine.Transform(transformationMatrix);
                                        AllMembers[AllHalfMembers[AllJoints[i].HalfMemberIndices[k]].MemberIndex].ForceLine = tempLine;
                                    }
                                }

                                for (int k = 0; k < AllJoints[i].ExternalForceIndices.Count; k++)
                                {
                                    tempLine = AllExternalForces[AllJoints[i].ExternalForceIndices[k]].ForceLineJoint;
                                    tempLine.Transform(transformationMatrix);
                                    AllExternalForces[AllJoints[i].ExternalForceIndices[k]].ForceLine = tempLine;
                                }

                                for (int k = 0; k < AllJoints[i].SupportReactionIndices.Count; k++)
                                {
                                    tempLine = AllSupportReactions[AllJoints[i].SupportReactionIndices[k]].ForceLineJoint;
                                    tempLine.Transform(transformationMatrix);
                                    AllSupportReactions[AllJoints[i].SupportReactionIndices[k]].ForceLine = tempLine;
                                }

                                AllJoints[i].Relocated = true;
                                solvedJoints++;
                                break;
                            }
                        }
                    }
                    if (solvedJoints == AllJoints.Count)
                        break;
                }
                if (solvedJoints == AllJoints.Count)
                    break;
            }

        }
        

    }
}
