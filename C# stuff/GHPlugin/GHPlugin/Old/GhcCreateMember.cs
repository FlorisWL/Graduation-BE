using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHPlugin
{
    public class GhcCreateMember : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcCreateMember()
          : base("Create Member", "Member",
              "Create a Member!",
              "GS tool", "Graphic Statics - old")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Start joint indices", "Start joint indices", "Start joint indices", GH_ParamAccess.list);
            pManager.AddIntegerParameter("End joint indices", "End joint indices", "End joint indices", GH_ParamAccess.list);
            pManager.AddPointParameter("Joints", "Joints", "Joints", GH_ParamAccess.list);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Member line", "Member line", "Member line", GH_ParamAccess.list);
            pManager.AddPointParameter("Middle point", "Middle point", "Middle point", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Known", "Known", "Known",GH_ParamAccess.item);
            pManager.AddNumberParameter("Force", "Force", "Force", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Positive force", "Positive force", "Positive force", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<int> iStartIndices = new List<int>();
            List<int> iEndIndices = new List<int>();
            List<Point3d> iJoints = new List<Point3d>();

            List<Line> oMemberLines = new List<Line>();
            List<Point3d> oMiddleJoints = new List<Point3d>();
            List<Boolean> oKnowns = new List<Boolean>();
            List<Double> oForces = new List<Double>();
            List<Boolean> oPositiveForces = new List<Boolean>();

            DA.GetDataList(0, iStartIndices);
            DA.GetDataList(1, iEndIndices);
            DA.GetDataList(2, iJoints);


            for (int i = 0; i < iStartIndices.Count; i++)
            {
                Member myMember = new Member(iStartIndices[i],iEndIndices[i],iJoints);

                oMemberLines.Add(myMember.MemberLine);
                oMiddleJoints.Add(myMember.MiddlePoint);
                oKnowns.Add(myMember.Known);
                oForces.Add(myMember.Force);
                oPositiveForces.Add(myMember.PositiveForce);
            }

            DA.SetDataList("Member line", oMemberLines);
            DA.SetDataList("Middle point", oMiddleJoints);
            DA.SetDataList("Known", oKnowns);
            DA.SetDataList("Force", oForces);
            DA.SetDataList("Positive force", oPositiveForces);
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
            get { return new Guid("ea1e0070-1258-4d07-b4e7-9463ed09d371"); }
        }
    }
}