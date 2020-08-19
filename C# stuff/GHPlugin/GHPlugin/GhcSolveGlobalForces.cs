using System;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using Grasshopper.Kernel;
using Rhino.Geometry;

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

            pManager.AddIntegerParameter("Force: Index", "Force: Index", "The indices in the Joints list where an external force should be applied!",GH_ParamAccess.list);
            pManager.AddNumberParameter("Force: Magnitude", "Force: Magnitude", "The magnitude of the external forces!", GH_ParamAccess.list);
            pManager.AddNumberParameter("Force: Rotation", "Force: Rotation", "The rotation in degrees of the external forces!",GH_ParamAccess.list);

            pManager.AddIntegerParameter("Support: Index", "Support: Index", "The indices in the Joints list where a support must be placed!", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Support: Horizontal Constraint", "Support: Horizontal", "Place a horizontal support at the given node!",GH_ParamAccess.list);
            pManager.AddBooleanParameter("Support: Vertical Constraint", "Support: Vertical", "Place a vertical support at the given node!",GH_ParamAccess.list);
            pManager.AddNumberParameter("Support: Rotation", "Support: Rotation", "The rotation in degrees of the base of the support!",GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Supports Form", "Supports Form", "The lines of the supports reactions in the form diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("External Forces Form", "Ext. Forces Form", "The lines of the external forces in the form diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("Resultant Form", "Resultant Form", "The line of the resultant force in the form diagram!", GH_ParamAccess.item);
            pManager.AddLineParameter("Virtual Members Form", "Virt. Members Form", "The lines of the (virtual) members in the global form diagram!", GH_ParamAccess.list);

            pManager.AddLineParameter("Supports Force", "Supports Force", "The lines of the supports reactions in the force diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("External Forces Force", "Ext. Forces Force", "The lines of the external forces in the force diagram!", GH_ParamAccess.list);
            pManager.AddLineParameter("Resultant Force", "Resultant Force", "The line of the resultant force in the force diagram!", GH_ParamAccess.item);
            pManager.AddLineParameter("Virtual Members Force", "Virt. Members Force", "The lines of the  (virtual) members in the global force diagram!", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d iStartForceDiagram = new Point3d(0, 0, 0);
            double iScalingFactor = 100.0;
            List<Point3d> iJoints = new List<Point3d>();
            List<int> iForceIndices = new List<int>();
            List<double> iForceMagnitudes = new List<double>();
            List<double> iForceRotations = new List<double>();
            List<int> iSupportIndices = new List<int>();
            List<bool> iSupportHorizontals = new List<bool>();
            List<bool> iSupportVerticals = new List<bool>();
            List<double> iSupportRotations = new List<double>();

            DA.GetData(0, ref iStartForceDiagram);
            DA.GetData(1, ref iScalingFactor);
            DA.GetDataList(2, iJoints);
            DA.GetDataList(3, iForceIndices);
            DA.GetDataList(4, iForceMagnitudes);
            DA.GetDataList(5, iForceRotations);
            DA.GetDataList(6, iSupportIndices);
            DA.GetDataList(7, iSupportHorizontals);
            DA.GetDataList(8, iSupportVerticals);
            DA.GetDataList(9, iSupportRotations);

            List<Line> oSupportLinesForm = new List<Line>();
            List<Line> oExtForceLinesForm = new List<Line>();
            Line oResultantLineForm = new Line(0, 0, 0, 1, 0, 0);
            List<Line> oVirtMemberLinesForm = new List<Line>();
            List<Line> oSupportLinesForce = new List<Line>();
            List<Line> oExtForceLinesForce = new List<Line>();
            Line oResultantLineForce = new Line(0, 0, 0, 0, 1, 0);
            List<Line> oVirtMemberLinesForce = new List<Line>();

            List<Joint> myJoints = new List<Joint>();
            List<ExternalForce> myExternalForces = new List<ExternalForce>();
            List<SupportReaction> mySupportReactions = new List<SupportReaction>();

            Vector3d zPostive = new Vector3d(0, 0, 1);
            Vector3d yPositive = new Vector3d(0, 1, 0);
            Vector3d yNegative = new Vector3d(0, -1, 0);
            Vector3d mySupportVector;

            for (int i = 0; i < iJoints.Count; i++)
                myJoints.Add(new Joint(iJoints[i]));

            for (int i = 0; i < iForceIndices.Count; i++)
            {
                ExternalForce myExternalForce = new ExternalForce(iForceIndices[i], iJoints, yNegative, iForceRotations[i], iForceMagnitudes[i]/iScalingFactor);
                myExternalForces.Add(myExternalForce);
                oExtForceLinesForm.Add(myExternalForce.FormLine);
            }

            for (int i = 0; i < iSupportIndices.Count; i++)
            {
                if (iSupportHorizontals[i])
                {
                    mySupportVector = yPositive;
                    mySupportVector.Rotate(iSupportRotations[i] / 180.0 * Math.PI, zPostive);
                    mySupportVector.Rotate(0.5 * Math.PI, zPostive);
                    SupportReaction mySupportReaction = new SupportReaction(iSupportIndices[i], iJoints, mySupportVector);
                    mySupportReactions.Add(mySupportReaction);
                    oSupportLinesForm.Add(mySupportReaction.FormLine);
                }

                if (iSupportVerticals[i])
                {
                    mySupportVector = yPositive;
                    mySupportVector.Rotate(iSupportRotations[i] / 180.0 * Math.PI, zPostive);
                    SupportReaction mySupportReaction = new SupportReaction(iSupportIndices[i], iJoints, mySupportVector);
                    mySupportReactions.Add(mySupportReaction);
                    oSupportLinesForm.Add(mySupportReaction.FormLine);
                }
            }

            Point3d startPoint = iStartForceDiagram;

            Resultant myResultant = new Resultant(iStartForceDiagram, myExternalForces);
            oResultantLineForce = myResultant.ResultantForce;
            oExtForceLinesForce = myResultant.ForceLinesForce;


            DA.SetDataList(0, oSupportLinesForm);
            DA.SetDataList(1, oExtForceLinesForm);
            DA.SetData(2, oResultantLineForm);
            DA.SetDataList(3, oVirtMemberLinesForm);
            DA.SetDataList(4, oSupportLinesForce);
            DA.SetDataList(5, oExtForceLinesForce);
            DA.SetData(6, oResultantLineForce);
            DA.SetDataList(7, oVirtMemberLinesForce);
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