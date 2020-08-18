using System;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHPlugin
{
    public class GhcCreatePyramid : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcCreatePyramid class.
        /// </summary>
        public GhcCreatePyramid()
          : base("Create Pyramid", "Pyramid",
              "Create a Pyramid!",
              "GS tool", "try-outs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Base Plane", "Base Plane", "The base plane for the pyramid!", GH_ParamAccess.item);
            pManager.AddNumberParameter("Length", "Length", "The length of the pyramid!", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "Width", "The width of the pyramid!", GH_ParamAccess.item);
            pManager.AddNumberParameter("Heigth", "Heigth", "The heigth of the pyramid!", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Edges of Pyramid", "Edges", "The edges of the pyramid!", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Plane iBasePlane = Plane.WorldXY;
            double iLength = 1.0;
            double iWidth = 1.0;
            double iHeight = 1.0;

            List<LineCurve> oDisplayLines = new List<LineCurve>();

            DA.GetData("Base Plane", ref iBasePlane);
            DA.GetData("Length", ref iLength);
            DA.GetData("Width", ref iWidth);
            DA.GetData("Heigth", ref iHeight);

            Pyramid myPyramid = new Pyramid(iBasePlane, iLength, iHeight, iWidth);
            oDisplayLines = myPyramid.ComputeEdges();

            DA.SetDataList("Edges of Pyramid", oDisplayLines);

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
            get { return new Guid("40081307-a953-4a1e-a68a-5b723bf40734"); }
        }
    }
}