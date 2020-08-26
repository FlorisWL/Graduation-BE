using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHPlugin
{
    public class GhcDisplayGlobalForces : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcDisplayGlobalForces()
          : base("Display Global Forces", "Display Global",
              "Display the solved global force, form and unified diagram",
              "GS tool", "Graphic Statics")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {


            pManager.AddBooleanParameter("Display External Forces", "Display Ext. Forces", "Display the external force in the form diagram!", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Display Resultant", "Display Resultant", "Display the resultant force in the form diagram!", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Display Support Reactions", "Display Supports", "Display the support reactions in the form diagram!", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Display Global Members", "Display Global Members", "Display the virtual members of the global form diagram!", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Display Force Diagram", "Display Force Diagram", "Display the global force diagram!", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
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
            get { return new Guid("5da95b0e-d3ca-4c07-a802-00c7e77e07c2"); }
        }
    }
}