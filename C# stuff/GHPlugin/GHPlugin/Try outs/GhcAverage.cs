using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHPlugin
{
    public class GhcAverage : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcAverage class.
        /// </summary>
        public GhcAverage()
          : base("Average of 2 numbers", "Average",
              "Compute the average of two numbers!",
              "GS tool", "try-outs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("First Number", "A", "The first number!", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Second Number", "B", "The second number!", GH_ParamAccess.item, 1.0);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Average", "avg", "The average of the the two inputs!", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double a = double.NaN;
            double b = double.NaN;

            bool successA = DA.GetData(0, ref a);
            bool successB = DA.GetData(1, ref b);

            if (successA && successB)
            {
                double avg = (a + b) / 2;
                DA.SetData(0, avg);
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Make sure both inputs are numbers!");
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.avg_icon;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("628ff584-3e5c-4675-be37-2b29287be936"); }
        }
    }
}