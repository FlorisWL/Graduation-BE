using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHPlugin
{
    public class GhcMovingParticle : GH_Component
    {
        public GhcMovingParticle()
          : base("Moving Particle", "Moving Particle",
              "Create a moving particle! (try-out with persistent data)",
              "GS tool", "try-outs")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Reset", "Reset", "Reset the movement!", GH_ParamAccess.item);
            pManager.AddVectorParameter("Velocity", "Velocity", "The velocity of the moving particle!", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Particle", "Particle", "The moving particle!", GH_ParamAccess.item);
        }

        Point3d currentPosition;


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iReset = false;
            DA.GetData("Reset", ref iReset);

            if (iReset)
                currentPosition = new Point3d(0.0, 0.0, 0.0);
            else
            {
                Vector3d iVelocity = new Vector3d(0.0, 0.0, 0.0);
                DA.GetData("Velocity", ref iVelocity);
                currentPosition += iVelocity;
            }

            DA.SetData("Particle", currentPosition);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.movingparticle_icon;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("fb757d27-e882-4eba-8094-379f90d75c1d"); }
        }
    }
}