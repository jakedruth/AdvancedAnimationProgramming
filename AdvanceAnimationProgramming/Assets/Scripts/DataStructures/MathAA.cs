
using UnityEngine;

namespace AdvAnimation
{
    public static class MathAA
    {
        public static SpacialPose Identity()
        {
            return new SpacialPose {orientation = Vector3.zero, scale = Vector3.one, translation = Vector3.zero};
        }

        public static SpacialPose Construct(Vector3 orientation, Vector3 scale, Vector3 translation)
        {
            return new SpacialPose {orientation = orientation, scale = scale, translation = translation};
        }

        public static SpacialPose Copy(SpacialPose pose)
        {
            return new SpacialPose {orientation = pose.orientation, scale = pose.scale, translation = pose.translation};
        }

        public static SpacialPose Invert(SpacialPose pose)
        {
            pose.orientation *= -1;

            pose.scale.x = 1 / pose.scale.x;
            pose.scale.y = 1 / pose.scale.y;
            pose.scale.z = 1 / pose.scale.z;
            pose.translation *= -1;

            return pose;
        }

        public static SpacialPose Concat(SpacialPose a, SpacialPose b)
        {
            return new SpacialPose
            {
                orientation = a.orientation + b.orientation,
                scale = new Vector3(a.scale.x * b.scale.x, a.scale.y * b.scale.y, a.scale.z * b.scale.z),
                translation = a.translation + b.translation
            };
        }

        public static SpacialPose Add(this SpacialPose pose, SpacialPose other)
        {
            pose.orientation += other.orientation;

            pose.scale.x *= other.scale.x;
            pose.scale.y *= other.scale.y;
            pose.scale.z *= other.scale.z;
            pose.translation += other.translation;

            return pose;
        }

        public static SpacialPose Nearest(SpacialPose a, SpacialPose b, float u)
        {
            return u < 0.5f ? a : b;
        }

        public static SpacialPose Lerp(SpacialPose a, SpacialPose b, float u)
        {
            return new SpacialPose
            {
                orientation = (1 - u) * a.orientation + u * b.orientation,
                scale = (1 - u) * a.scale + u * b.scale,
                translation = (1 - u) * a.translation + u * b.translation
            };
        }

        public static SpacialPose Cubic(SpacialPose pP, SpacialPose p0, SpacialPose p1, SpacialPose pN, float u)
        {
            float uu = u * u;
            float uuu = uu * u;

            float qP = -uuu + 2 * uu - u;
            float q0 = +3 * uuu - 5 * uu + 2;
            float q1 = -3 * uuu + 4 * uu + u;
            float qN = uuu - uu;

            return new SpacialPose
            {
                orientation = (pP.orientation * qP + p0.orientation * q0 + p1.orientation * q1 + pN.orientation * qN) *
                              0.5f,
                scale = (pP.scale * qP + p0.scale * q0 + p1.scale * q1 + pN.scale * qN) * 0.5f,
                translation = (pP.translation * qP + p0.translation * q0 + p1.translation * q1 + pN.translation * qN) *
                              0.5f,
            };
        }
    }
}