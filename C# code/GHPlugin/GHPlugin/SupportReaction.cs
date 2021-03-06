﻿using Rhino.Geometry;
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
        public int GlobalJointIndex;
        public Point3d Joint;
        public Line FormLine;
        public Line FormLineForAngle;
        public Line ForceLine;
        public Line ForceLineJoint;
        public Line ForceLineForAngle;
        public Vector3d Direction;
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
        
        public SupportReaction(Line supportLineForm, Line supportLineForce, List<Point3d> joints)
        {
            FormLine = supportLineForm;
            ForceLineForAngle = supportLineForce;
            ForceLine = supportLineForce;
            Direction = supportLineForce.Direction;
            Direction.Unitize();
            Force = supportLineForce.Length;
            Known = true;

            for (int i = 0; i < joints.Count; i++)
            {
                if (supportLineForm.To == joints[i])
                {
                    PositiveForce = true;
                    Joint = joints[i];
                    JointIndex = i;
                }
                if (supportLineForm.From == joints[i])
                {
                    PositiveForce = false;
                    Joint = joints[i];
                    JointIndex = i;
                }
            }
        }

        public void SupportLineForAngleGeneral(List<HalfMember> halfMembers)
        {
            Boolean flip = false;
            Plane planeXY = new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, 1));
            Line SupportFlipped = FormLine;
            SupportFlipped.Flip();
            List<Vector3d> m0m1 = new List<Vector3d>();

            for (int i = 0; i < halfMembers.Count; i++)
            {
                if (halfMembers[i].JointIndex == GlobalJointIndex)
                    m0m1.Add(halfMembers[i].FormLine.Direction);
            }

            for (int i = 0; i < m0m1.Count; i++)
            {

            }

            Vector3d m0 = m0m1[0];
            Vector3d m1 = m0m1[m0m1.Count - 1];

            if (Vector3d.VectorAngle(m0, m1, planeXY) < Math.PI)
            {
                if (Vector3d.VectorAngle(m0, FormLine.Direction, planeXY) < Vector3d.VectorAngle(m0, m1, planeXY))
                    flip = true;
            }
            else
            {
                if (Vector3d.VectorAngle(m1, FormLine.Direction, planeXY) < Vector3d.VectorAngle(m1, m0, planeXY))
                    flip = true;
            }

            FormLineForAngle = FormLine;
            if (flip)
                FormLineForAngle.Flip();

            return;
        }

        public void SupportLineForAngle(List<HalfMember> halfMembers)
        {
            Boolean flip = false;
            Plane planeXY = new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, 1));
            Line SupportFlipped = FormLine;
            SupportFlipped.Flip();
            List<Vector3d> m0m1 = new List<Vector3d>();

            for(int i = 0; i < halfMembers.Count; i++)
            {
                if (halfMembers[i].JointIndex == GlobalJointIndex)
                    m0m1.Add(halfMembers[i].FormLine.Direction);
            }

            Vector3d m0 = m0m1[0];
            Vector3d m1 = m0m1[m0m1.Count - 1];

            if (Vector3d.VectorAngle(m0, m1, planeXY) < Math.PI)
            {
                if (Vector3d.VectorAngle(m0, FormLine.Direction, planeXY) < Vector3d.VectorAngle(m0, m1, planeXY))
                    flip = true;
            }
            else
            {
                if (Vector3d.VectorAngle(m1, FormLine.Direction, planeXY) < Vector3d.VectorAngle(m1, m0, planeXY))
                    flip = true;
            }

            FormLineForAngle = FormLine;
            if (flip)
                FormLineForAngle.Flip();

            return;
        }
    }
}
