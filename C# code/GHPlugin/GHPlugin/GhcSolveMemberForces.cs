using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHPlugin
{
    public class GhcSolveMemberForces : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcSolveGlobalForces class.
        /// </summary>
        public GhcSolveMemberForces()
          : base("Solve Member Forces", "Solve Members",
              "Solve all forces in the structure!",
              "GSDesign", "2D Graphic Statics")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Start Force Diagram", "Start Force", "A point depicting the start location of the overall force diagram!", GH_ParamAccess.item, new Point3d(0, 0, 0));
            pManager.AddNumberParameter("Scaling Factor", "Scaling Factor", "A scaling factor for the force diagram [Force/Length]!", GH_ParamAccess.item, 100);

            pManager.AddPointParameter("Joints", "Joints", "All nodes of the structure!", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Member Start Indices", "Members Start", "The joint indices of the start points of the members!", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Member End Indices", "Members End", "The joint indices of the end points of the members!", GH_ParamAccess.list);

            pManager.AddIntegerParameter("Force: Indices", "Force: Indices", "The indices in the Joints list where an external force should be applied!",GH_ParamAccess.list);
            pManager.AddNumberParameter("Force: Magnitudes", "Force: Magnitude", "The magnitude of the external forces!", GH_ParamAccess.list);
            pManager.AddNumberParameter("Force: Rotations", "Force: Rotation", "The rotation in degrees of the external forces!",GH_ParamAccess.list,0);

            pManager.AddIntegerParameter("Support: Indices", "Support: Indices", "The indices in the Joints list where a support must be placed!", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Support: Horizontal Constraints", "Support: Horizontal", "Place a horizontal support at the given node!",GH_ParamAccess.list);
            pManager.AddBooleanParameter("Support: Vertical Constraints", "Support: Vertical", "Place a vertical support at the given node!",GH_ParamAccess.list);
            pManager.AddNumberParameter("Support: Rotations", "Support: Rotation", "The rotation in degrees of the base of the support!",GH_ParamAccess.list,0);

            pManager.AddBooleanParameter("Display Unified Diagram", "Display Unified Diagram", "Choose the display option of the form diagram: False = Only the wireframe structure, True = wireframe structure plus the unified digram!", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Display Force Diagram", "Display Force Diagram", "Display the overall force diagram!", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Display Joint Force Diagrams", "Display Joint Force Diagrams", "Display the individual force diagrams of each joint!", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Spacing Joint Force Diagrams", "Spacing Joint Force Diagrams", "Define the scaling factor for the spacing between the joint force diagrams, with 1.0 as default. Input is only relevant if joint force diagrams are displayed!", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Display Numerical Values", "Display Numerical Values", "Display the numerical values of the members, external forces, and support reactions in the form diagram!", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Supports Form", "Supports Form", "The lines of the supports reactions in the form diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("External Forces Form", "Ext. Forces Form", "The lines of the external forces in the form diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("Members Form", "Members Form", "The lines of the members in the form diagram!", GH_ParamAccess.list);

            pManager.AddLineParameter("Supports Force Joints", "Supports Force Joints", "The lines of the supports reactions in the force diagram per joint!", GH_ParamAccess.list);
            pManager.AddLineParameter("External Forces Force Joints", "Ext. Forces Force Joints", "The lines of the external forces in the force diagram per joint!", GH_ParamAccess.list);
            pManager.AddLineParameter("Members Force Joints", "Members Force Joints", "The lines of the members in the global force diagram per joint!", GH_ParamAccess.list);

            pManager.AddLineParameter("Supports Force", "Supports Force", "The lines of the supports reactions in the overall force diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("External Forces Force", "Ext. Forces Force", "The lines of the external forces in the overall force diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("Members Force", "Members Force", "The lines of the members in the global overall force diagram!", GH_ParamAccess.list);

            pManager.AddBrepParameter("Members Unified Diagram", "Members Unified Diagram", "Member geometry to display for the unified diagram!", GH_ParamAccess.list);
            pManager.AddColourParameter("Color Members", "Color Members", "The colors of all the members: blue for compression, red for tension!", GH_ParamAccess.list);

            pManager.AddNumberParameter("Force Magnitudes", "Force Magnitude", "The magnitude of forces in all members, supports and external forces, in that order!", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Locations Text Tags", "Locations Text Tags", "Locations as planes for the numerical force value 3D text tags!", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Locations Joint Force Diagrams", "Locations Joints", "Locations as planes of the start points of the force diagram of each joint!", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Functions functions = new Functions();

            Point3d iStartForceDiagram = new Point3d(0, 0, 0);
            double iScalingFactor = 100.0;
            List<Point3d> iJoints = new List<Point3d>();
            List<int> iMemberStartIndices = new List<int>();
            List<int> iMemberEndIndices = new List<int>();
            List<int> iForceIndices = new List<int>();
            List<double> iForceMagnitudes = new List<double>();
            List<double> iForceRotations = new List<double>();
            List<int> iSupportIndices = new List<int>();
            List<bool> iSupportHorizontals = new List<bool>();
            List<bool> iSupportVerticals = new List<bool>();
            List<double> iSupportRotations = new List<double>();
            bool iDisplayUnifiedDiagram = true;
            bool iDisplayForceDiagram = true;
            bool iDisplayJointForceDiagram = true;
            double iSpacingJoinForceDiagram = 1.0;
            bool iDisplayNumericalValues = true;

            DA.GetData(0, ref iStartForceDiagram);
            DA.GetData(1, ref iScalingFactor);
            DA.GetDataList(2, iJoints);
            DA.GetDataList(3, iMemberStartIndices);
            DA.GetDataList(4, iMemberEndIndices);
            DA.GetDataList(5, iForceIndices);
            DA.GetDataList(6, iForceMagnitudes);
            DA.GetDataList(7, iForceRotations);
            DA.GetDataList(8, iSupportIndices);
            DA.GetDataList(9, iSupportHorizontals);
            DA.GetDataList(10, iSupportVerticals);
            DA.GetDataList(11, iSupportRotations);
            DA.GetData(12, ref iDisplayUnifiedDiagram);
            DA.GetData(13, ref iDisplayForceDiagram);
            DA.GetData(14, ref iDisplayJointForceDiagram);
            DA.GetData(15, ref iSpacingJoinForceDiagram);
            DA.GetData(16, ref iDisplayNumericalValues);

            if ((iSupportRotations.Count != iSupportIndices.Count) && (iSupportRotations.Count == 1))
                iSupportRotations = functions.CreateDefaultList(iSupportIndices.Count,iSupportRotations[0]);

            if ((iForceRotations.Count != iForceIndices.Count) && (iForceRotations.Count == 1))
                iForceRotations = functions.CreateDefaultList(iForceIndices.Count, iForceRotations[0]);

            List<Line> ioSupportLinesForce = new List<Line>();

            List<Line> oSupportLinesForm = new List<Line>();
            List<Line> oExtForceLinesForm = new List<Line>();
            List<Line> oMemberLinesForm = new List<Line>();
            List<Line> oSupportLinesForceJoint = new List<Line>();
            List<Line> oExtForceLinesForceJoint = new List<Line>();
            List<Line> oMemberLinesForceJoint = new List<Line>();
            List<Line> oSupportLinesForce = new List<Line>();
            List<Line> oExtForceLinesForce = new List<Line>();
            List<Line> oMemberLinesForce = new List<Line>();
            List<Brep> oDisplayBreps = new List<Brep>();
            List<Color> oMemberColors = new List<Color>();
            List<Double> oForceMagnitudes = new List<Double>();
            List<Plane> oLocationsForceTextTags = new List<Plane>();
            List<Point3d> oLocationsJointForceDiagrams = new List<Point3d>();

            List<Line> initialSupportLinesForm = new List<Line>();
            List<Joint> myJoints = new List<Joint>();
            List<ExternalForce> myExternalForces = new List<ExternalForce>();
            List<SupportReaction> mySupportReactions = new List<SupportReaction>();
            double ratio = 1.0 / 3.0;
            Vector3d zPostive = new Vector3d(0, 0, 1);
            Vector3d yPositive = new Vector3d(0, 1, 0);
            Vector3d yNegative = new Vector3d(0, -1, 0);
            Vector3d mySupportVector;

            List<int> extForceInputLengths = new List<int>();
            extForceInputLengths.Add(iForceIndices.Count);
            extForceInputLengths.Add(iForceMagnitudes.Count);
            extForceInputLengths.Add(iForceRotations.Count);
            List<int> supportInputLengths = new List<int>();
            supportInputLengths.Add(iSupportIndices.Count);
            supportInputLengths.Add(iSupportHorizontals.Count);
            supportInputLengths.Add(iSupportVerticals.Count);
            supportInputLengths.Add(iSupportRotations.Count);
            List<int> memberInputLengths = new List<int>();
            memberInputLengths.Add(iMemberStartIndices.Count);
            memberInputLengths.Add(iMemberEndIndices.Count);
            List<int> allMaxIndices = new List<int>();
            allMaxIndices.Add(iMemberStartIndices.Max());
            allMaxIndices.Add(iMemberEndIndices.Max());
            allMaxIndices.Add(iForceIndices.Max());
            allMaxIndices.Add(iSupportIndices.Max());

            bool noErrors = true;

            List<Member> allMembers = new List<Member>();
            for (int i = 0; i < iMemberStartIndices.Count; i++)
            {
                allMembers.Add(new Member(iMemberStartIndices[i], iMemberEndIndices[i], iJoints));
                oMemberLinesForm.Add(allMembers[i].FormLine);
            }

            if (functions.MatchingInputs(supportInputLengths) == false)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Support related inputs don't have matching list lengths!");
                noErrors = false;
            }
            if (functions.MatchingInputs(extForceInputLengths) == false)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "External force related inputs don't have matching list lengths!");
                noErrors = false;
            }
            if (functions.MatchingInputs(memberInputLengths) == false)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Member related inputs don't have matching list lengths!");
                noErrors = false;
            }
            if (functions.IndicesInBounds(iJoints.Count, allMaxIndices) == false)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "One or more of the point indices of the members, forces and/or supports inputs are out of bounds!");
                noErrors = false;
            }


            if (noErrors)
            {

                for (int i = 0; i < iJoints.Count; i++)
                    myJoints.Add(new Joint(iJoints[i]));

                for (int i = 0; i < iForceIndices.Count; i++)
                {
                    ExternalForce myExternalForce = new ExternalForce(iForceIndices[i], iJoints, yNegative, iForceRotations[i], iForceMagnitudes[i] / iScalingFactor);
                    myExternalForce.LengthenFormLines(ratio);
                    myExternalForces.Add(myExternalForce);
                    oExtForceLinesForm.Add(myExternalForce.FormLine);
                }

                for (int i = 0; i < iSupportIndices.Count; i++)
                {
                    if (iSupportHorizontals[i])
                    {
                        mySupportVector = yPositive;
                        mySupportVector.Rotate(iSupportRotations[i] / 180.0 * Math.PI, zPostive);
                        mySupportVector.Rotate(-0.5 * Math.PI, zPostive);
                        SupportReaction mySupportReaction = new SupportReaction(iSupportIndices[i], iJoints, mySupportVector);
                        mySupportReactions.Add(mySupportReaction);
                        initialSupportLinesForm.Add(mySupportReaction.FormLine);
                    }

                    if (iSupportVerticals[i])
                    {
                        mySupportVector = yPositive;
                        mySupportVector.Rotate(iSupportRotations[i] / 180.0 * Math.PI, zPostive);
                        SupportReaction mySupportReaction = new SupportReaction(iSupportIndices[i], iJoints, mySupportVector);
                        mySupportReactions.Add(mySupportReaction);
                        initialSupportLinesForm.Add(mySupportReaction.FormLine);
                    }
                }

                for (int i = 0; i < mySupportReactions.Count; i++)
                {

                    if ((mySupportReactions[i].JointIndex != mySupportReactions[(i + 1) % mySupportReactions.Count].JointIndex) && (mySupportReactions[i].JointIndex != mySupportReactions[(i + 2) % mySupportReactions.Count].JointIndex))
                    {
                        if (mySupportReactions[i].FormLine.DistanceTo(mySupportReactions[(i + 1) % 3].Joint, false) == 0)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "unstable structure: two support reactions act in the same line!");
                            noErrors = false;
                            break;
                        }
                    }
                }

                if (iSupportIndices.Count != 2)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "invalid structure: only designs with exactly two supports are supported in the current version of this tool!");
                    noErrors = false;
                }

                int degreeOfExtDeterminacy = functions.ExternalStaticDeterminacy(mySupportReactions.Count, 3);
                if (degreeOfExtDeterminacy < 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "global structure is unstable!");
                    noErrors = false;
                }
                else if (degreeOfExtDeterminacy > 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "global structure is statically indeterminate!");
                    noErrors = false;
                }

                if (noErrors)
                {

                    Point3d averagePointJoints = functions.CenterPoint(iJoints);
                    averagePointJoints.Y += 4;
                    Resultant myResultant = new Resultant(iStartForceDiagram, myExternalForces, ratio, averagePointJoints);
                    GlobalDiagram myGlobalDiagram = new GlobalDiagram(myResultant, mySupportReactions);
                    List<Member> myVirtMembers = myGlobalDiagram.Members();
                    List<HalfMember> myHalfMembers = myGlobalDiagram.GlobalHalfMembers;

                    for (int i = 0; i < mySupportReactions.Count; i++)
                        mySupportReactions[i].SupportLineForAngle(myHalfMembers);

                    for (int i = 0; i < myGlobalDiagram.GlobalJoints.Count; i++)
                        myGlobalDiagram.SolveForceDiagram(myVirtMembers);

                    for (int i = 0; i < mySupportReactions.Count; i++)
                    {
                        ioSupportLinesForce.Add(mySupportReactions[i].ForceLine);
                        double supportLengthForm = mySupportReactions[i].Force * ratio;
                        double extendValue = supportLengthForm - 1.0;
                        mySupportReactions[i].FormLine.Extend(extendValue, 0);
                        initialSupportLinesForm[i] = mySupportReactions[i].FormLine;
                        if (mySupportReactions[i].PositiveForce)
                            mySupportReactions[i].FormLine.Flip();

                        oSupportLinesForm.Add(mySupportReactions[i].FormLine);
                    }

                    List<Line> iExtForceLinesForce = myResultant.ForceLinesForce;

                    //Solve Member Forces

                    int degreeOfIntDeterminacy = functions.InternalStaticDeterminacy(iMemberStartIndices.Count, mySupportReactions.Count, iJoints.Count);
                    if (degreeOfIntDeterminacy < 0)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "internal truss structure is unstable!");
                        oMemberColors.Add(Color.FromName("Black"));
                    }
                    else if (degreeOfIntDeterminacy > 0)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "internal truss structure is statically indeterminate!");
                        oMemberColors.Add(Color.FromName("Black"));
                    }
                    else
                    {

                        List<SupportReaction> allSupports = new List<SupportReaction>();

                        for (int i = 0; i < oSupportLinesForm.Count; i++)
                        {
                            allSupports.Add(new SupportReaction(oSupportLinesForm[i], ioSupportLinesForce[i], iJoints));
                        }

                        List<ExternalForce> allExternalForces = new List<ExternalForce>();

                        for (int i = 0; i < oExtForceLinesForm.Count; i++)
                        {
                            allExternalForces.Add(new ExternalForce(oExtForceLinesForm[i], iExtForceLinesForce[i], iJoints));
                        }

                        GeneralDiagram myGeneralDiagram = new GeneralDiagram(allExternalForces, allSupports, allMembers, iJoints, iSpacingJoinForceDiagram);

                        for (int i = 0; i < myGeneralDiagram.AllJoints.Count; i++)
                            //myGeneralDiagram.SolveForceDiagram();
                            myGeneralDiagram.SolveForceDiagramJointBased();

                        allMembers = myGeneralDiagram.AllMembers;

                        for (int i = 0; i < allMembers.Count; i++)
                        {
                            oForceMagnitudes.Add(allMembers[i].Force * iScalingFactor);

                            if (allMembers[i].PositiveForce == false)
                                oMemberColors.Add(Color.FromArgb(122, Color.FromName("Blue")));
                            else
                                oMemberColors.Add(Color.FromArgb(122, Color.FromName("Red")));
                        }

                        if (iDisplayForceDiagram)
                        {
                            myGeneralDiagram.CreateOverallForceDiagram();

                            for (int i = 0; i < allMembers.Count; i++)
                                oMemberLinesForce.Add(allMembers[i].ForceLine);

                            for (int i = 0; i < allSupports.Count; i++)
                                oSupportLinesForce.Add(allSupports[i].ForceLine);

                            for (int i = 0; i < allExternalForces.Count; i++)
                                oExtForceLinesForce.Add(allExternalForces[i].ForceLine);
                        }

                        if (iDisplayJointForceDiagram)
                        {
                            oMemberLinesForceJoint = myGeneralDiagram.ForceLinesJoint();

                            for (int i = 0; i < myGeneralDiagram.AllJoints.Count; i++)
                                oLocationsJointForceDiagrams.Add(myGeneralDiagram.AllJoints[i].InitSolveLocation);

                            for (int i = 0; i < allSupports.Count; i++)
                                oSupportLinesForceJoint.Add(allSupports[i].ForceLineJoint);

                            for (int i = 0; i < allExternalForces.Count; i++)
                                oExtForceLinesForceJoint.Add(allExternalForces[i].ForceLineJoint);
                        }

                        if (iDisplayUnifiedDiagram)
                        {
                            double myScalingFactor = 1.0 / 9.0;
                            functions.DisplayRectangles(allMembers, myScalingFactor, out oDisplayBreps);
                        }

                        for (int i = 0; i < allSupports.Count; i++)
                        {
                            oForceMagnitudes.Add(allSupports[i].Force * iScalingFactor);
                        }

                        for (int i = 0; i < allExternalForces.Count; i++)
                        {
                            oForceMagnitudes.Add(allExternalForces[i].Force * iScalingFactor);
                        }

                        if (iDisplayNumericalValues)
                        {
                            List<Plane> tempLocations;
                            functions.DisplayNumericalValues(oMemberLinesForm, out tempLocations);
                            for (int i = 0; i < tempLocations.Count; i++)
                                oLocationsForceTextTags.Add(tempLocations[i]);
                            functions.DisplayNumericalValues(oSupportLinesForm, out tempLocations);
                            for (int i = 0; i < tempLocations.Count; i++)
                                oLocationsForceTextTags.Add(tempLocations[i]);
                            functions.DisplayNumericalValues(oExtForceLinesForm, out tempLocations);
                            for (int i = 0; i < tempLocations.Count; i++)
                                oLocationsForceTextTags.Add(tempLocations[i]);
                        }
                    }
                }
            }

            DA.SetDataList(0, oSupportLinesForm);
            DA.SetDataList(1, oExtForceLinesForm);
            DA.SetDataList(2, oMemberLinesForm);
            DA.SetDataList(3, oSupportLinesForceJoint);
            DA.SetDataList(4, oExtForceLinesForceJoint);
            DA.SetDataList(5, oMemberLinesForceJoint);
            DA.SetDataList(6, oSupportLinesForce);
            DA.SetDataList(7, oExtForceLinesForce);
            DA.SetDataList(8, oMemberLinesForce);
            DA.SetDataList(9, oDisplayBreps);
            DA.SetDataList(10, oMemberColors);
            DA.SetDataList(11, oForceMagnitudes);
            DA.SetDataList(12, oLocationsForceTextTags);
            DA.SetDataList(13, oLocationsJointForceDiagrams);
        }

        private void CustomDisplay(bool v)
        {
            throw new NotImplementedException();
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.icon_solvemembers;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{63E9D66D-AA33-4D47-A454-4C17D2741E5A}"); }
        }
    }
}