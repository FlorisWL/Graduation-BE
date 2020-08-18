using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace GHPlugin
{
    public class MyFirstGrasshopperComponent : GH_Component
    {

        public MyFirstGrasshopperComponent()
          : base("Floris' First GH Component", "my first ghc",
              "This kind of is my first grasshopper component ever!",
              "GS tool", "try-outs")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.first_icon;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("eb42c315-e6fe-427a-a201-2dacb445f447"); }
        }
    }
}
