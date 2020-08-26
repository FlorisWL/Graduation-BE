using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHPlugin
{
    public class GhcCreateSupportReaction : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcCreateSupportReaction class.
        /// </summary>
        public GhcCreateSupportReaction()
          : base("Create Support Reaction", "Support Reaction",
              "Create Support Reaction!",
              "GS tool", "Graphic Statics - old")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Joint indices", "Joint indices", "Joint indices", GH_ParamAccess.list);
            pManager.AddPointParameter("Joints", "Joints", "Joints", GH_ParamAccess.list);
            pManager.AddVectorParameter("Base Vertical Vector", "Base Vertical Vector", "Base Vertical Vector", GH_ParamAccess.item);
            pManager.AddNumberParameter("Rotation", "Rotation", "Rotationi in degrees", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Horizontal Constraint", "Horizontal Constraint", "Horizontal Constraint", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Vertical Constraint", "Vertical Constraint", "Vertical Constraint", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Joint index", "Joint index", "Joint index", GH_ParamAccess.list);
            pManager.AddVectorParameter("Direction", "Direction", "Direction", GH_ParamAccess.list);
            pManager.AddLineParameter("Form line", "Form line", "Form line", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Known", "Known", "Known", GH_ParamAccess.item);
            pManager.AddNumberParameter("Force", "Force", "Force", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Positive force", "Positive force", "Positive force", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<int> iJointIndices = new List<int>();
            List<Point3d> iJoints = new List<Point3d>();
            Vector3d iBaseYVector = new Vector3d(0, 1, 0);
            List<double> iRotation = new List<double>();
            List<Boolean> iHorizontalConstraint = new List<Boolean>();
            List<Boolean> iVerticalConstraint = new List<Boolean>();

            List<int> oJointIndices = new List<int>();
            List<Vector3d> oDirections = new List<Vector3d>();
            List<Line> oFormLines = new List<Line>();
            List<Boolean> oKnowns = new List<Boolean>();
            List<Double> oForces = new List<Double>();
            List<Boolean> oPositiveForces = new List<Boolean>();

            DA.GetDataList(0, iJointIndices);
            DA.GetDataList(1, iJoints);
            DA.GetData(2, ref iBaseYVector);
            DA.GetDataList(3, iRotation);
            DA.GetDataList(4, iHorizontalConstraint);
            DA.GetDataList(5, iVerticalConstraint);

            Vector3d mySupportVector;
            Vector3d z = new Vector3d(0, 0, 1);

            for (int i = 0; i < iJointIndices.Count; i++)
            {
                

                if (iHorizontalConstraint[i])
                {
                    mySupportVector = iBaseYVector;
                    mySupportVector.Unitize();
                    mySupportVector.Rotate(iRotation[i] / 180.0 * Math.PI, z);
                    mySupportVector.Rotate(0.5*Math.PI, z);
                    SupportReaction mySupportReaction = new SupportReaction(
                    iJointIndices[i], iJoints, mySupportVector
                    );

                    oJointIndices.Add(mySupportReaction.JointIndex);
                    oDirections.Add(mySupportReaction.Direction);
                    oFormLines.Add(mySupportReaction.FormLine);
                    oKnowns.Add(mySupportReaction.Known);
                    oForces.Add(mySupportReaction.Force);
                    oPositiveForces.Add(mySupportReaction.PositiveForce);
                }

                if (iVerticalConstraint[i])
                {
                    mySupportVector = iBaseYVector;
                    mySupportVector.Unitize();
                    mySupportVector.Rotate(iRotation[i] / 180.0 * Math.PI, z);
                    SupportReaction mySupportReaction = new SupportReaction(
                    iJointIndices[i], iJoints, mySupportVector
                    );

                    oJointIndices.Add(mySupportReaction.JointIndex);
                    oDirections.Add(mySupportReaction.Direction);
                    oFormLines.Add(mySupportReaction.FormLine);
                    oKnowns.Add(mySupportReaction.Known);
                    oForces.Add(mySupportReaction.Force);
                    oPositiveForces.Add(mySupportReaction.PositiveForce);
                }
            }

            DA.SetDataList(0, oJointIndices);
            DA.SetDataList(1, oDirections);
            DA.SetDataList(2, oFormLines);
            DA.SetDataList(3, oKnowns);
            DA.SetDataList(4, oForces);
            DA.SetDataList(5, oPositiveForces);
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("94da64b3-10dd-409d-accf-64c85473c669"); }
        }
    }
}