using System;
using System.Drawing;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHPlugin
{
    public class GhcSolveMemberForces_old : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcSolveMemberForces class.
        /// </summary>
        public GhcSolveMemberForces_old()
          : base("Solve Members Forces (deprecated)", "Solve Members (depecrated)",
              "Solve all member forces! (use the 'Solve Global Forces' component first)",
              "GS tool", "Graphic Statics")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            
            pManager.AddPointParameter("Joints", "Joints", "All nodes of the structure!", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Member start indices", "Members start", "The joint indices of the start points of the members!", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Member end indices", "Members end", "The joint indices of the end points of the members!", GH_ParamAccess.list);

            pManager.AddLineParameter("Supports Form", "Supports Form", "The lines of the supports reactions in the form diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("External Forces Form", "Ext. Forces Form", "The lines of the external forces in the form diagram!", GH_ParamAccess.list);

            pManager.AddLineParameter("Supports Force", "Supports Force", "The lines of the supports reactions in the force diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("External Forces Force", "Ext. Forces Force", "The lines of the external forces in the force diagram!", GH_ParamAccess.list);

            pManager.AddBooleanParameter("Display Unified Diagram", "Display Unified Diagram", "Choose the display option of the form diagram: False = Only the wireframe structure, True = wireframe structure plus the unified digram!", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Display Force Diagram", "Display Force Diagram", "Display the overall force diagram!", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Display Joint Force Diagrams", "Display Joint Force Diagrams", "Display the individual force diagrams of each joint!", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Spacing Joint Force Diagrams", "Spacing Joint Force Diagrams", "Define the scaling factor for the spacing between the joint force diagrams, with 1.0 as default. Input is only relevant if joint force diagrams are displayed!", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Display Numerical Values", "Display Numerical Values", "Display the numerical values of the members, external forces, and support reactions in the form diagram!", GH_ParamAccess.item, false);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Supports Form", "Supports Form", "The lines of the supports reactions in the form diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("External Forces Form", "Ext. Forces Form", "The lines of the external forces in the form diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("Members Form", "Members Form", "The lines of the members in the form diagram!", GH_ParamAccess.list);

            pManager.AddLineParameter("Supports Force Joints", "Supports Force Joints", "The lines of the supports reactions in the force diagram per joint!", GH_ParamAccess.list);
            pManager.AddLineParameter("External Forces Force Joints", "Ext. Forces Force Joints", "The lines of the external forces in the force diagram per joint!", GH_ParamAccess.list);
            pManager.AddLineParameter("Members Force Joints", "Members Force Joints", "The lines of the members in the global force diagram per joint!", GH_ParamAccess.list);

            pManager.AddLineParameter("Supports Force", "Supports Force", "The lines of the supports reactions in the overall force diagram", GH_ParamAccess.list);
            pManager.AddLineParameter("External Forces Force", "Ext. Forces Force", "The lines of the external forces in the overall force diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("Members Force", "Members Force", "The lines of the members in the global overall force diagram!", GH_ParamAccess.list);


            pManager.AddBrepParameter("Members unified diagram", "Members unified diagram", "Member geometry to display for the unified diagram", GH_ParamAccess.list);
            pManager.AddColourParameter("Colour Global Members", "Colour Global Members", "The colours of global members: blue for compression, red for tension!", GH_ParamAccess.list);

            pManager.AddNumberParameter("Force Magnitudes Supports", "Force Magnitude Supports", "The magnitude of the support reactions!", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Locations Text Tag", "Locations Text Tag", "Locations as planes for the numerical force value 3D text tags", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Locations Joint Force Diagrams", "Locations Joints", "Locations as planes of the start points of the force diagram of each joint", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Functions functions = new Functions();

            List<Point3d> iJoints = new List<Point3d>();
            List<int> iMemberStartIndices = new List<int>();
            List<int> iMemberEndIndices = new List<int>();
            List<Line> iSupportLinesForm = new List<Line>();
            List<Line> iExtForceLinesForm = new List<Line>();
            List<Line> iSupportLinesForce = new List<Line>();
            List<Line> iExtForceLinesForce = new List<Line>();
            bool iDisplayUnifiedDiagram = true;
            bool iDisplayForceDiagram = true;
            bool iDisplayJointForceDiagram = true;
            double iSpacingJoinForceDiagram = 1.0;
            bool iDisplayNumericalValues = true;

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

            DA.GetDataList(0, iJoints);
            DA.GetDataList(1, iMemberStartIndices);
            DA.GetDataList(2, iMemberEndIndices);
            DA.GetDataList(3, iSupportLinesForm);
            DA.GetDataList(4, iExtForceLinesForm);
            DA.GetDataList(5, iSupportLinesForce);
            DA.GetDataList(6, iExtForceLinesForce);
            DA.GetData(7, ref iDisplayUnifiedDiagram);
            DA.GetData(8, ref iDisplayForceDiagram);
            DA.GetData(9, ref iDisplayJointForceDiagram);
            DA.GetData(10, ref iSpacingJoinForceDiagram);
            DA.GetData(11, ref iDisplayNumericalValues);

            List<Member> allMembers = new List<Member>();

            for (int i = 0; i < iMemberStartIndices.Count; i++)
            {
                allMembers.Add(new Member(iMemberStartIndices[i], iMemberEndIndices[i], iJoints));
                oMemberLinesForm.Add(allMembers[i].MemberLine);
            }

            List<SupportReaction> allSupports = new List<SupportReaction>();

            for (int i = 0; i < iSupportLinesForm.Count; i++)
            {
                allSupports.Add(new SupportReaction(iSupportLinesForm[i], iSupportLinesForce[i], iJoints));
            }

            List<ExternalForce> allExternalForces = new List<ExternalForce>();

            for (int i = 0; i < iExtForceLinesForm.Count; i++)
            {
                allExternalForces.Add(new ExternalForce(iExtForceLinesForm[i], iExtForceLinesForce[i], iJoints));
            }

            GeneralDiagram myGeneralDiagram = new GeneralDiagram(allExternalForces, allSupports, allMembers, iJoints, iSpacingJoinForceDiagram);

            for (int i = 0; i < myGeneralDiagram.AllJoints.Count; i++)
                //myGeneralDiagram.SolveForceDiagram();
                myGeneralDiagram.SolveForceDiagramJointBased();

            allMembers = myGeneralDiagram.AllMembers;

            for (int i = 0; i < allMembers.Count; i++)
            {
                oForceMagnitudes.Add(allMembers[i].Force);

                if (allMembers[i].PositiveForce == false)
                    oMemberColors.Add(Color.FromArgb(122, Color.FromName("Blue")));
                else
                    oMemberColors.Add(Color.FromArgb(122, Color.FromName("Red")));
            }

            if(iDisplayForceDiagram)
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

            oSupportLinesForm = iSupportLinesForm;
            for (int i = 0; i < allSupports.Count; i++)
            {
                oForceMagnitudes.Add(allSupports[i].Force);
            }

            oExtForceLinesForm = iExtForceLinesForm;
            for (int i = 0; i < allExternalForces.Count; i++)
            {
                oForceMagnitudes.Add(allExternalForces[i].Force);
            }

            if (iDisplayNumericalValues)
            {
                List<Plane> tempLocations = new List<Plane>();
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

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.icon_solvemembers;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("99ae43f9-0751-43d9-98c4-cd8fde23035c"); }
        }
    }
}