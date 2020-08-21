using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHPlugin
{
    public class CreateExternalForce : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public CreateExternalForce()
          : base("Create External Force", "Ext. Force",
              "Create External Force!",
              "GS tool", "Graphic Statics")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Joint indices", "Joint indices", "Joint indices", GH_ParamAccess.list);
            pManager.AddPointParameter("Joints", "Joints", "Joints", GH_ParamAccess.list);
            pManager.AddVectorParameter("Base Vector", "Base Vector", "Base Vector", GH_ParamAccess.item);
            pManager.AddNumberParameter("Rotation", "Rotation", "Rotationi in degrees", GH_ParamAccess.list);
            pManager.AddNumberParameter("Magnitude", "Magnitude", "Magnitude", GH_ParamAccess.list);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Joint index", "Joint index", "Joint index", GH_ParamAccess.list);
            pManager.AddVectorParameter("Direction", "Direction", "Direction", GH_ParamAccess.list);
            pManager.AddNumberParameter("Force", "Force", "Force", GH_ParamAccess.list);
            pManager.AddLineParameter("Form line", "Form line", "Form line", GH_ParamAccess.list);
            pManager.AddLineParameter("Force line", "Force line", "Force line", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<int> iJointIndices = new List<int>();
            List<Point3d> iJoints = new List<Point3d>();
            Vector3d iBaseVector = new Vector3d(0,-1,0);
            List<double> iRotation = new List<double>();
            List<double> iMagnitude = new List<double>();

            List<int> oJointIndices = new List<int>();
            List<Vector3d> oDirections = new List<Vector3d>();
            List<double> oForces = new List<double>();
            List<Line> oFormLines = new List<Line>();
            List<Line> oForceLines = new List<Line>();

            DA.GetDataList(0, iJointIndices);
            DA.GetDataList(1, iJoints);
            DA.GetData(2, ref iBaseVector);
            DA.GetDataList(3, iRotation);
            DA.GetDataList(4, iMagnitude);


            for (int i = 0; i < iJointIndices.Count; i++)
            {
                ExternalForce myExternalForce = new ExternalForce(
                    iJointIndices[i], iJoints, iBaseVector, iRotation[i], iMagnitude[i]
                    );

                oJointIndices.Add(myExternalForce.JointIndex);
                oDirections.Add(myExternalForce.Direction);
                oForces.Add(myExternalForce.Force);
                oFormLines.Add(myExternalForce.FormLine);
                oForceLines.Add(myExternalForce.ForceLine);
            }

            DA.SetDataList(0, oJointIndices);
            DA.SetDataList(1, oDirections);
            DA.SetDataList(2, oForces);
            DA.SetDataList(3, oFormLines);
            DA.SetDataList(4, oForceLines);
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
            get { return new Guid("{846609C1-0555-4C9C-B635-8246212F77FF}"); }
        }
    }
}