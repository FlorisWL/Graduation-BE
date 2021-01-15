using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace GHPlugin
{
    public class ExternalForce
    {
        public int JointIndex;
        public Line FormLine;
        public Line ForceLine;
        public Line ForceLineJoint;
        public Line ForceLineForAngle;
        public Vector3d ForceVector;
        public Vector3d Direction;
        public double Force = 0.0;



        public ExternalForce(int jointIndex, List<Point3d> joints, Vector3d forceVector)
        {
            JointIndex = jointIndex;
            ForceVector = forceVector;
            Force = forceVector.SquareLength;
            forceVector.Unitize();
            Direction = forceVector;
            forceVector.Reverse();
            Line formLine = new Line(joints[jointIndex], forceVector);
            formLine.Flip();
            FormLine = formLine;
        }

        public ExternalForce(
            int jointIndex, List<Point3d> joints, Vector3d baseVector, 
            double rotation, double magnitude
            )
        {
            JointIndex = jointIndex;
            Force = magnitude;
            baseVector.Unitize();
            baseVector.Rotate(rotation / 180.0 * Math.PI, new Vector3d(0,0,1));
            Direction = baseVector;
            ForceVector = baseVector * magnitude;
            baseVector.Reverse();
            Line formLine = new Line(joints[jointIndex], baseVector);
            formLine.Flip();
            FormLine = formLine;
        }

        public ExternalForce(Line formLine, Line forceLine, List<Point3d> joints)
        {
            FormLine = formLine;
            ForceLineForAngle = forceLine;
            ForceLine = forceLine;
            Force = ForceLine.Length;
            ForceVector = ForceLine.Direction;
            Direction = ForceLine.Direction;
            Direction.Unitize();

            for (int i = 0; i < joints.Count; i++)
            {
                if (formLine.To == joints[i])
                {
                    JointIndex = i;
                    break;
                }
            }
        }

        public void LengthenFormLines(double ratio)
        {
            double forceLengthForm = Force * ratio;
            double extendValue = forceLengthForm - 1.0;
            FormLine.Extend(extendValue, 0);
        }

    }
}
