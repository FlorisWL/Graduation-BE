using System;
using System.Drawing;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHPlugin
{
    public class GhcSolveMemberForces : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcSolveMemberForces class.
        /// </summary>
        public GhcSolveMemberForces()
          : base("Solve Members Forces", "Solve Members",
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

            pManager.AddIntegerParameter("Display option", "Display option", "Choose the display options: 0 = Design as a whole, 1 = Global form diagram, 2 = Global unified diagram!", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("Display Force Diagram", "Display Force Diagram", "Display the global force diagram!", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Display Numerical Values", "Display Numerical Values", "Display the numerical values of the vectors in the form diagram!", GH_ParamAccess.item, false);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Supports Form", "Supports Form", "The lines of the supports reactions in the form diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("External Forces Form", "Ext. Forces Form", "The lines of the external forces in the form diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("Members Form", "Members Form", "The lines of the members in the form diagram!", GH_ParamAccess.list);

            pManager.AddLineParameter("Supports Force", "Supports Force", "The lines of the supports reactions in the force diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("External Forces Force", "Ext. Forces Force", "The lines of the external forces in the force diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("Members Force", "Members Force", "The lines of the members in the global force diagram!", GH_ParamAccess.list);

            pManager.AddBrepParameter("Members unified diagram", "Members unified diagram", "Member geometry to display for the unified diagram", GH_ParamAccess.list);
            pManager.AddColourParameter("Colour Global Members", "Colour Global Members", "The colours of global members: blue for compression, red for tension!", GH_ParamAccess.list);

            pManager.AddNumberParameter("Force Magnitudes Supports", "Force Magnitude Supports", "The magnitude of the support reactions!", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Locations text tag", "Locations text tag", "Locations as planes for the numerical force value 3D text tags", GH_ParamAccess.list);
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
            int iDisplayOption = 0;
            bool iDisplayForceDiagram = true;
            bool iDisplayNumericalValues = true;

            List<Line> oSupportLinesForm = new List<Line>();
            List<Line> oExtForceLinesForm = new List<Line>();
            List<Line> oMemberLinesForm = new List<Line>();
            List<Line> oSupportLinesForce = new List<Line>();
            List<Line> oExtForceLinesForce = new List<Line>();
            List<Line> oMemberLinesForce = new List<Line>();
            List<Brep> oDisplayBreps = new List<Brep>();
            List<Color> oMemberColors = new List<Color>();
            List<Double> oForceMagnitudes = new List<Double>();
            List<Plane> oLocationsForceTextTags = new List<Plane>();

            DA.GetDataList(0, iJoints);
            DA.GetDataList(1, iMemberStartIndices);
            DA.GetDataList(2, iMemberEndIndices);
            DA.GetDataList(3, iSupportLinesForm);
            DA.GetDataList(4, iExtForceLinesForm);
            DA.GetDataList(5, iSupportLinesForce);
            DA.GetDataList(6, iExtForceLinesForce);
            DA.GetData(7, ref iDisplayOption);
            DA.GetData(8, ref iDisplayForceDiagram);
            DA.GetData(9, ref iDisplayNumericalValues);

            List<Member> allMembers = new List<Member>();

            for (int i = 0; i < iMemberStartIndices.Count; i++)
            {
                allMembers.Add(new Member(iMemberStartIndices[i], iMemberEndIndices[i], iJoints));
                oMemberLinesForm.Add(allMembers[i].MemberLine);
                oMemberColors.Add(Color.FromName("Black"));
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

            GeneralDiagram myGeneralDiagram = new GeneralDiagram(allExternalForces, allSupports, allMembers, iJoints);
            myGeneralDiagram.SolveForceDiagram();
            allMembers = myGeneralDiagram.AllMembers;

            for (int i = 0; i < allMembers.Count; i++)
            {
                oMemberLinesForm.Add(allMembers[i].MemberLine);
                oMemberLinesForce.Add(allMembers[i].ForceLine);
                if (allMembers[i].PositiveForce == false)
                    oMemberColors.Add(Color.FromArgb(122, Color.FromName("Blue")));
                else
                    oMemberColors.Add(Color.FromArgb(122, Color.FromName("Red")));
            }

            oSupportLinesForm = iSupportLinesForm;
            oSupportLinesForce = iSupportLinesForce;
            oExtForceLinesForm = iExtForceLinesForm;
            oExtForceLinesForce = iExtForceLinesForce;


            DA.SetDataList(0, oSupportLinesForm);
            DA.SetDataList(1, oExtForceLinesForm);
            DA.SetDataList(2, oMemberLinesForm);
            DA.SetDataList(3, oSupportLinesForce);
            DA.SetDataList(4, oExtForceLinesForce);
            DA.SetDataList(5, oMemberLinesForce);
            DA.SetDataList(6, oDisplayBreps);
            DA.SetDataList(7, oMemberColors);
            DA.SetDataList(8, oForceMagnitudes);
            DA.SetDataList(9, oLocationsForceTextTags);
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