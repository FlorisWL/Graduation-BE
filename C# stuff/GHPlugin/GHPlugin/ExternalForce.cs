﻿using Rhino.Geometry;
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
        public Vector3d Direction;
        public Vector3d ForceVector;
        public double Force = 0.0;
        public Line FormLine;
        public Line ForceLine;

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



    }
}