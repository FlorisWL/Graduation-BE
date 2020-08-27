using System;
using System.Drawing;
using Rhino.Display;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Windows.Forms.VisualStyles;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace GHPlugin
{
    public class GhcSolveGlobalForces : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcSolveGlobalForces class.
        /// </summary>
        public GhcSolveGlobalForces()
          : base("Solve Global Forces", "Solve Global",
              "Solve the global force diagram!",
              "GS tool", "Graphic Statics")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Start Force Diagram", "Start Force", "A point depicting the start location of the force diagram!", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scaling Factor", "Scaling Factor", "A scaling factor for the force diagram [Force/Length]!", GH_ParamAccess.item);

            pManager.AddPointParameter("Joints", "Joints", "All nodes of the structure!", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Member start indices", "Members start", "The joint indices of the start points of the members!", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Member end indices", "Members end", "The joint indices of the end points of the members!", GH_ParamAccess.list);

            pManager.AddIntegerParameter("Force: Index", "Force: Index", "The indices in the Joints list where an external force should be applied!",GH_ParamAccess.list);
            pManager.AddNumberParameter("Force: Magnitude", "Force: Magnitude", "The magnitude of the external forces!", GH_ParamAccess.list);
            pManager.AddNumberParameter("Force: Rotation", "Force: Rotation", "The rotation in degrees of the external forces!",GH_ParamAccess.list,0);

            pManager.AddIntegerParameter("Support: Index", "Support: Index", "The indices in the Joints list where a support must be placed!", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Support: Horizontal Constraint", "Support: Horizontal", "Place a horizontal support at the given node!",GH_ParamAccess.list);
            pManager.AddBooleanParameter("Support: Vertical Constraint", "Support: Vertical", "Place a vertical support at the given node!",GH_ParamAccess.list);
            pManager.AddNumberParameter("Support: Rotation", "Support: Rotation", "The rotation in degrees of the base of the support!",GH_ParamAccess.list,0);

            pManager.AddIntegerParameter("Display option", "Display option", "Choose the display options: 0 = Design as a whole, 1 = Global form diagram, 2 = Global unified diagram!", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("Display Force Diagram", "Display Force Diagram", "Display the global force diagram!", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Display Numerical Values", "Display Numerical Values", "Display the numerical values of the vectors in the form diagram!", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Supports Form", "Supports Form", "The lines of the supports reactions in the form diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("External Forces Form", "Ext. Forces Form", "The lines of the external forces in the form diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("Resultant Form", "Resultant Form", "The line of the resultant force in the form diagram!", GH_ParamAccess.item);
            pManager.AddLineParameter("Members Form", "Members Form", "The lines of the members in the global form diagram. Can be either the general members or the virtual global members, depending on the chosen display option!", GH_ParamAccess.list);

            pManager.AddLineParameter("Supports Force", "Supports Force", "The lines of the supports reactions in the force diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("External Forces Force", "Ext. Forces Force", "The lines of the external forces in the force diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("Resultant Force", "Resultant Force", "The line of the resultant force in the force diagram!", GH_ParamAccess.item);
            pManager.AddLineParameter("Virtual Members Force", "Virt. Members Force", "The lines of the  (virtual) members in the global force diagram!", GH_ParamAccess.list);

            pManager.AddBrepParameter("Members unified diagram", "Members unified diagram", "Member geometry to display for the unified diagram", GH_ParamAccess.list);
            pManager.AddColourParameter("Colour Global Members", "Colour Global Members", "The colours of global members: blue for compression, red for tension!", GH_ParamAccess.list);

            pManager.AddNumberParameter("Force Magnitudes Supports", "Force Magnitude Supports", "The magnitude of the support reactions!", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Locations text tag", "Locations text tag", "Locations as planes for the numerical force value 3D text tags", GH_ParamAccess.list);
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
            int iDisplayOption = 0;
            bool iDisplayForceDiagram = true;
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
            DA.GetData(12, ref iDisplayOption);
            DA.GetData(13, ref iDisplayForceDiagram);
            DA.GetData(14, ref iDisplayNumericalValues);

            if ((iSupportRotations.Count != iSupportIndices.Count) && (iSupportRotations.Count == 1))
                iSupportRotations = functions.CreateDefaultList(iSupportIndices.Count,iSupportRotations[0]);

            if ((iForceRotations.Count != iForceIndices.Count) && (iForceRotations.Count == 1))
                iForceRotations = functions.CreateDefaultList(iForceIndices.Count, iForceRotations[0]);

            Point3d averagePointJoints = functions.CenterPoint(iJoints);
            averagePointJoints.Y += 4;
            List<Line> oSupportLinesForm = new List<Line>();
            List<Line> oExtForceLinesForm = new List<Line>();
            Line oResultantLineForm;
            List<Line> oMemberLinesForm = new List<Line>();
            List<Line> oSupportLinesForce = new List<Line>();
            List<Line> oExtForceLinesForce;
            Line oResultantLineForce;
            List<Line> oVirtMemberLinesForce = new List<Line>();
            List<Double> oForceMagnitudes = new List<Double>();
            List<Plane> oLocationsForceTextTags = new List<Plane>();

            List<Line> initialSupportLinesForm = new List<Line>();
            List<Joint> myJoints = new List<Joint>();
            List<ExternalForce> myExternalForces = new List<ExternalForce>();
            List<SupportReaction> mySupportReactions = new List<SupportReaction>();
            //double myScalingFactorUnified = 1.5 / (Enumerable.Sum(iForceMagnitudes) / iScalingFactor);
            double ratio = 1.0 / 3.0;
            double myScalingFactorUnified = ratio / 3.0;

            Vector3d zPostive = new Vector3d(0, 0, 1);
            Vector3d yPositive = new Vector3d(0, 1, 0);
            Vector3d yNegative = new Vector3d(0, -1, 0);
            Vector3d mySupportVector;

            for (int i = 0; i < iJoints.Count; i++)
                myJoints.Add(new Joint(iJoints[i]));

            for (int i = 0; i < iForceIndices.Count; i++)
            {
                ExternalForce myExternalForce = new ExternalForce(iForceIndices[i], iJoints, yNegative, iForceRotations[i], iForceMagnitudes[i]/iScalingFactor);
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

            Resultant myResultant = new Resultant(iStartForceDiagram, myExternalForces, ratio, averagePointJoints);
            oResultantLineForce = myResultant.ResultantForce;
            oResultantLineForm = myResultant.ResultantForm;
            oExtForceLinesForce = myResultant.ForceLinesForce;

            GlobalDiagram myGlobalDiagram = new GlobalDiagram(myResultant, mySupportReactions);
            List<Member> myVirtMembers = myGlobalDiagram.Members();
            List<HalfMember> myHalfMembers = myGlobalDiagram.HalfMembersForm;
            myResultant.ResultantForAngle(myGlobalDiagram);



            for (int i = 0; i < myVirtMembers.Count; i++)
                oMemberLinesForm.Add(myVirtMembers[i].MemberLine);

            for (int i = 0; i < mySupportReactions.Count; i++)
                mySupportReactions[i].SupportLineForAngle(myHalfMembers);

            for (int i = 0; i < myGlobalDiagram.GlobalJoints.Count ; i++)
            {
                myGlobalDiagram.SolveForceDiagram(myVirtMembers);
            }

            for (int i = 0; i < myVirtMembers.Count; i++)
                oVirtMemberLinesForce.Add(myVirtMembers[i].ForceLine);

            for (int i = 0; i < mySupportReactions.Count; i++)
            {
                oSupportLinesForce.Add(mySupportReactions[i].ForceLine);
                double supportLengthForm = mySupportReactions[i].Force * ratio;
                double extendValue = supportLengthForm - 1.0;
                mySupportReactions[i].FormLine.Extend(extendValue, 0);
                initialSupportLinesForm[i] = mySupportReactions[i].FormLine;
                if (mySupportReactions[i].PositiveForce)
                    mySupportReactions[i].FormLine.Flip();

                oSupportLinesForm.Add(mySupportReactions[i].FormLine);
            }

            List<Line> formLinesForTextTag = new List<Line>();

            for (int i = 0; i < mySupportReactions.Count; i++)
            {
                formLinesForTextTag.Add(initialSupportLinesForm[i]);
                oForceMagnitudes.Add(Math.Round(mySupportReactions[i].Force * iScalingFactor));

            }
            for (int i = 0; i < myExternalForces.Count; i++)
            {
                formLinesForTextTag.Add(oExtForceLinesForm[i]);
                oForceMagnitudes.Add(Math.Round(myExternalForces[i].Force * iScalingFactor));
            }
            formLinesForTextTag.Add(oResultantLineForm);
            oForceMagnitudes.Add(Math.Round(myResultant.Force * iScalingFactor));
            functions.DisplayNumericalValues(formLinesForTextTag, out oLocationsForceTextTags);


            List<Brep> oDisplayBreps = new List<Brep>();
            List<Color> oMemberColors = new List<Color>();
            List<Line> memberLinesForm = new List<Line>();
            List<Member> allMembers = new List<Member>();
            
            for (int i = 0; i < iMemberStartIndices.Count; i++)
            {
                allMembers.Add(new Member(iMemberStartIndices[i], iMemberEndIndices[i], iJoints));
                memberLinesForm.Add(allMembers[i].MemberLine);
                oMemberColors.Add(Color.FromName("Black"));
            }

            if (iDisplayOption == 0)
            {
                oMemberLinesForm = memberLinesForm;
                oVirtMemberLinesForce = new List<Line>();
            }
            if (iDisplayOption == 1)
            {
                functions.DisplayColors(myVirtMembers, out oMemberColors);

            }
            if (iDisplayOption == 2)
            {
                functions.DisplayRectangles(myVirtMembers, myScalingFactorUnified, out oDisplayBreps);
                functions.DisplayColors(myVirtMembers, out oMemberColors);
            }

            if (iDisplayForceDiagram == false)
            {
                oExtForceLinesForce = new List<Line>();
                oResultantLineForce = new Line();
                oSupportLinesForce = new List<Line>();
                oVirtMemberLinesForce = new List<Line>();
            }

            if(iDisplayNumericalValues == false)
            {
                oLocationsForceTextTags = new List<Plane>();
            }

            DA.SetDataList(0, oSupportLinesForm);
            DA.SetDataList(1, oExtForceLinesForm);
            DA.SetData(2, oResultantLineForm);
            DA.SetDataList(3, oMemberLinesForm);
            DA.SetDataList(4, oSupportLinesForce);
            DA.SetDataList(5, oExtForceLinesForce);
            DA.SetData(6, oResultantLineForce);
            DA.SetDataList(7, oVirtMemberLinesForce);
            DA.SetDataList(8, oDisplayBreps);
            DA.SetDataList(9, oMemberColors);
            DA.SetDataList(10, oForceMagnitudes);
            DA.SetDataList(11, oLocationsForceTextTags);
        }

        private void CustomDisplay(bool v)
        {
            throw new NotImplementedException();
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("b5add907-3392-4eb7-9e1a-b2f95e02ab36"); }
        }
    }
}