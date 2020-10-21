using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace GHPlugin
{
    public class GhcFitnessFunction : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GhcFitnessFunction()
          : base("Compute Fitness Function", "Fitness Function",
              "Compute a basic fitness function based on the total load path SUM(F*L), which could be used as a target for optimization",
              "GSDesign", "2D Graphic Statics")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Members Form", "Members Form", "The lines of the members in the form diagram!", GH_ParamAccess.list);
            pManager.AddNumberParameter("Force Magnitudes", "Force Magnitude", "The magnitude of forces in all members, supports and external forces, in that order!", GH_ParamAccess.list);
            pManager.AddColourParameter("Colour Members", "Colour Members", "The colours of all the members: blue for compression, red for tension!", GH_ParamAccess.list);

            pManager.AddCurveParameter("Free Areas", "Free Areas", "Curves depicting areas that are supposed to stay free of structural elements!", GH_ParamAccess.list);
            pManager.AddNumberParameter("Penalty Factor Compression", "Penalty Compression", "A penalty factor for all members in compression. A higher penalty should result in a favourability of tensile structures!", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Penalty Factor Tension", "Penalty Tension", "A penalty factor for all members in tension. A higher penalty should result in a favourability of compressive structures!", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Max Length Compression Element", "Max Length Compression", "A value that sets the max length for any compressive element, any design containing compressive elements with longer lengths are disregarded!", GH_ParamAccess.item, 0.0);

            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Fitness", "Fitness", "The manipulated fitness value, taking into consideration all extra penalties and constraints on top of the formula for the total load path!", GH_ParamAccess.item);
            pManager.AddNumberParameter("Fitness Unweighted", "Fitness Unweighted", "The pure fitness value using only the formula SUM(F*L) for the theoretical total load path!", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Line> iMembersForm = new List<Line>();
            List<double> iForceMagnitudes = new List<double>();
            List<Color> iMemberColors = new List<Color>();
            List<Curve> iFreeAreas = new List<Curve>();
            double iPenaltyCompression = 1.0;
            double iPenaltyTension = 1.0;
            double iMaxLengthCompression = 0.0;

            double oFitnessManipulated;
            double oFitnessPure; ;

            DA.GetDataList(0,iMembersForm);
            DA.GetDataList(1,iForceMagnitudes);
            DA.GetDataList(2,iMemberColors);
            DA.GetDataList(3,iFreeAreas);
            DA.GetData(4,ref iPenaltyCompression);
            DA.GetData(5,ref iPenaltyTension);
            DA.GetData(6,ref iMaxLengthCompression);

            List<double> memberLengths = new List<double>();
            List<double> memberForces = new List<double>();
            List<double> memberLengthsXForces = new List<double>();
            List<double> memberLengthsXForcesWeighted = new List<double>();
            bool MaxCompressionLengthExceeded = false;
            bool FreeAreasBreached = false;

            int i = 0;
            foreach (Line element in iMembersForm)
            {
                memberLengths.Add(iMembersForm[i].Length);
                memberForces.Add(iForceMagnitudes[i]);
                memberLengthsXForces.Add(memberLengths[i] * memberForces[i]);
                i++;
            }

            oFitnessPure = memberLengthsXForces.Sum();

            i = 0;
            foreach (Color element in iMemberColors)
            {
                if (iMemberColors[i].R > 0.5)
                    memberLengthsXForcesWeighted.Add(memberLengthsXForces[i] * iPenaltyTension);
                else
                {
                    memberLengthsXForcesWeighted.Add(memberLengthsXForces[i] * iPenaltyCompression);
                    if (iMaxLengthCompression > 0)
                        if (memberLengths[i] > iMaxLengthCompression)
                            MaxCompressionLengthExceeded = true;
                }
                i++;
            }

            double tol = 1e-9;
            Transform scaleTransform;
            Point3d centerPoint;
            if(iFreeAreas.Count != 0)
            {
                for (int j = 0; j < iFreeAreas.Count; j++)
                {
                    centerPoint = AreaMassProperties.Compute(iFreeAreas[j]).Centroid;
                    scaleTransform = Transform.Scale(centerPoint, 0.9999);
                    iFreeAreas[j].Transform(scaleTransform);
                    for (i = 0; i < iMembersForm.Count; i++)
                    {
                        Curve curve = iMembersForm[i].ToNurbsCurve();
                        bool curveCollision = Curve.PlanarCurveCollision(iFreeAreas[j], curve, new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, -1)), tol);
                        if (curveCollision)
                        {
                            FreeAreasBreached = true;
                            goto End;
                        }
                    }
                }
            }

            End:

            oFitnessManipulated = memberLengthsXForcesWeighted.Sum();

            if ((FreeAreasBreached) || (MaxCompressionLengthExceeded))
                oFitnessManipulated *= 1e6;


            DA.SetData(0, oFitnessManipulated);
            DA.SetData(1, oFitnessPure);
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
                return Properties.Resources.icon_computefitness;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("79d3d724-8e5a-4f29-88e3-0a0215e721c1"); }
        }
    }
}