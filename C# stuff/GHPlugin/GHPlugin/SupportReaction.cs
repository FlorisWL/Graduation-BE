using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace GHPlugin
{
    public class SupportReaction
    {
        public int JointIndex;
        public Point3d Joint;
        public Vector3d Direction;
        public Line FormLine;
        public Line ForceLine;
        public double Force = 0.0;
        public Boolean PositiveForce = true;
        public Boolean Known = false;

        public SupportReaction(int jointIndex, List<Point3d> joints, Vector3d supportVector)
        {
            JointIndex = jointIndex;
            Joint = joints[jointIndex];
            supportVector.Unitize();
            Direction = supportVector;
            supportVector.Reverse();
            Line formLine = new Line(joints[jointIndex], supportVector);
            formLine.Flip();
            FormLine = formLine;
        }
    }
}
