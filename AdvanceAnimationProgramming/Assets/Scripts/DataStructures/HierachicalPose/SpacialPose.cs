/*
	Advanced Animation Programming
	By Jake Ruth

    SpacialPose.cs - A description of a transformation pos in space
*/

using UnityEngine;

namespace AdvAnimation
{
    public class SpacialPose
    {
        public Matrix4x4 transform;
        public Vector3 orientation;
        public Vector3 scale;
        public Vector3 translation;

        public SpacialPose()
        {
            transform = Matrix4x4.identity;
            orientation = Vector3.zero;
            scale = Vector3.one;
            translation = Vector3.zero;
        }
    }
}
