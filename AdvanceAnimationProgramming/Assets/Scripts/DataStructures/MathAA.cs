
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

    }
}