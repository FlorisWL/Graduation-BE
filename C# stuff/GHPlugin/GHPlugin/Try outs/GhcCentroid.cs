using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Display;
using Rhino.Geometry;

namespace GHPlugin
{
    public class GhcCentroid : GH_Component
    {

        public GhcCentroid()
          : base("Centroid of Points", "Centroid",
              "Find the centroid of a collection of points",
              "GS tool", "try-outs")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Points", "A list of points!", GH_ParamAccess.list);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Centroid", "Centroid", "The centroid of the points!", GH_ParamAccess.item);
            pManager.AddNumberParameter("Distances", "Distances", "The distances between the points and the centroid!", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> iPoints = new List<Point3d>();

            DA.GetDataList(0, iPoints);
            int countiPoints = iPoints.Count;

            Point3d oCentroid = new Point3d(0.0,0.0,0.0);
            List<double> oDistances = new List<double>();

            foreach (Point3d point in iPoints)
            {
                oCentroid += point;
            }

            oCentroid = oCentroid / countiPoints;

            foreach (Point3d point in iPoints)
            {
                oDistances.Add(oCentroid.DistanceTo(point));
            }

            DA.SetData(0, oCentroid);
            DA.SetDataList(1, oDistances);

        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.centroid_icon;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("04641481-6c5f-47d0-8f83-0ae877acb2da"); }
        }
    }
}