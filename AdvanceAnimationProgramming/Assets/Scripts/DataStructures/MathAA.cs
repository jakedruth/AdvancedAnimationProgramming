/*
	Advanced Animation Programming
	By Jake Ruth

    MathAA.cs - Similar to Mathf for floats, MathAA is math for Advanced Animation poses
*/

using UnityEngine;

namespace AdvAnimation
{
    public static class MathAA
    {
        public static SpacialPose Identity()
        {
            return new SpacialPose {orientation = Vector3.zero, scale = Vector3.one, translation = Vector3.zero};
        }

        public static HierarchicalPose HierarchicalPoseIdentity()
        {
            return new HierarchicalPose {rootPose = Identity()};
        }

        public static SpacialPose Construct(Vector3 orientation, Vector3 scale, Vector3 translation)
        {
            return new SpacialPose {orientation = orientation, scale = scale, translation = translation};
        }

        public static SpacialPose Copy(SpacialPose pose)
        {
            return new SpacialPose {orientation = pose.orientation, scale = pose.scale, translation = pose.translation};
        }

        public static SpacialPose Clone(this SpacialPose pose)
        {
            return Copy(pose);
        }

        public static HierarchicalPose Copy(HierarchicalPose hierarchicalPose)
        {
            return new HierarchicalPose {rootPose = Copy(hierarchicalPose.rootPose)};
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

        public static SpacialPose Negate(this SpacialPose pose)
        {
            return Invert(pose);
        }

        public static HierarchicalPose Invert(HierarchicalPose hierarchicalPose)
        {
            hierarchicalPose.rootPose = Invert(hierarchicalPose.rootPose);
            return hierarchicalPose;
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

        public static HierarchicalPose Concat(HierarchicalPose a, HierarchicalPose b)
        {
            return new HierarchicalPose
            {
                rootPose = Concat(a.rootPose, b.rootPose)
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

        public static HierarchicalPose Add(this HierarchicalPose hierarchicalPose, HierarchicalPose other)
        {
            hierarchicalPose.rootPose.Add(other.rootPose);
            return hierarchicalPose;
        }

        public static SpacialPose Nearest(SpacialPose a, SpacialPose b, float u)
        {
            return u < 0.5f ? a.Clone() : b.Clone();
        }

        public static HierarchicalPose Nearest(HierarchicalPose a, HierarchicalPose b, float u)
        {
            return u < 0.5f ? Copy(a) : Copy(b);
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

        public static HierarchicalPose Lerp(HierarchicalPose a, HierarchicalPose b, float u)
        {
            return new HierarchicalPose
            {
                rootPose = Lerp(a.rootPose, b.rootPose, u)
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
                orientation = (pP.orientation * qP + p0.orientation * q0 + p1.orientation * q1 + pN.orientation * qN) * 0.5f,
                scale = (pP.scale * qP + p0.scale * q0 + p1.scale * q1 + pN.scale * qN) * 0.5f,
                translation = (pP.translation * qP + p0.translation * q0 + p1.translation * q1 + pN.translation * qN) * 0.5f,
            };
        }

        public static HierarchicalPose Cubic(HierarchicalPose pP, HierarchicalPose p0, HierarchicalPose p1, HierarchicalPose pN, float u)
        {
            return new HierarchicalPose
            {
                rootPose = Cubic(pP.rootPose, p0.rootPose, p1.rootPose, pN.rootPose, u)
            };
        }

        public static SpacialPose DeConcat(SpacialPose a, SpacialPose b)
        {
            return Concat(a, Invert(b));
        }

        public static HierarchicalPose DeConcat(HierarchicalPose a, HierarchicalPose b)
        {
            return Concat(a, Invert(b));
        }

        public static SpacialPose Subtract(this SpacialPose pose, SpacialPose other)
        {
            return pose.Add(other.Negate());
        }

        public static HierarchicalPose Subtract(this HierarchicalPose pose, HierarchicalPose other)
        {
            return pose.Add(Invert(other));
        }

        public static SpacialPose Scale(SpacialPose pose, float u)
        {
            return Lerp(Identity(), pose, u);
        }

        public static HierarchicalPose Scale(HierarchicalPose pose, float u)
        {
            return Lerp(HierarchicalPoseIdentity(), pose, u);
        }

        public static SpacialPose TriangularBlend(SpacialPose p0, SpacialPose p1, SpacialPose p2, float u1, float u2)
        {
            float u0 = 1 - u1 - u2;
            return Scale(p0, u0).Add(Scale(p1, u1)).Add(Scale(p2, u2));
        }

        public static HierarchicalPose TriangularBlend(HierarchicalPose p0, HierarchicalPose p1, HierarchicalPose p2,
            float u1, float u2)
        {
            float u0 = 1 - u1 - u2;
            return Scale(p0, u0).Add(Scale(p1, u1)).Add(Scale(p2, u2));
        }

        public static SpacialPose BiNearest(SpacialPose p00, SpacialPose p01, SpacialPose p10, SpacialPose p11,
            float u0, float u1, float u)
        {
            return Nearest(Nearest(p00, p01, u0), Nearest(p10, p11, u1), u);
        }

        public static HierarchicalPose BiNearest(HierarchicalPose p00, HierarchicalPose p01, HierarchicalPose p10, HierarchicalPose p11,
            float u0, float u1, float u)
        {
            return Nearest(Nearest(p00, p01, u0), Nearest(p10, p11, u1), u);
        }

        public static SpacialPose BiLerp(SpacialPose p00, SpacialPose p01, SpacialPose p10, SpacialPose p11,
            float u0, float u1, float u)
        {
            return Lerp(Lerp(p00, p01, u0), Lerp(p10, p11, u1), u);
        }

        public static HierarchicalPose BiLerp(HierarchicalPose p00, HierarchicalPose p01, HierarchicalPose p10, HierarchicalPose p11,
            float u0, float u1, float u)
        {
            return Lerp(Lerp(p00, p01, u0), Lerp(p10, p11, u1), u);
        }

        public static SpacialPose BiCubic(
            SpacialPose pap, SpacialPose pa0, SpacialPose pa1, SpacialPose pan,
            SpacialPose pbp, SpacialPose pb0, SpacialPose pb1, SpacialPose pbn,
            SpacialPose pcp, SpacialPose pc0, SpacialPose pc1, SpacialPose pcn,
            SpacialPose pdp, SpacialPose pd0, SpacialPose pd1, SpacialPose pdn,
            float ua, float ub, float uc, float ud, float u)
        {
            return Cubic(
                Cubic(pap, pa0, pa1, pan, ua),
                Cubic(pbp, pb0, pb1, pbn, ub),
                Cubic(pcp, pc0, pc1, pcn, uc),
                Cubic(pdp, pd0, pd1, pdn, ud),
                u);
        }

        public static HierarchicalPose BiCubic(
            HierarchicalPose pap, HierarchicalPose pa0, HierarchicalPose pa1, HierarchicalPose pan,
            HierarchicalPose pbp, HierarchicalPose pb0, HierarchicalPose pb1, HierarchicalPose pbn,
            HierarchicalPose pcp, HierarchicalPose pc0, HierarchicalPose pc1, HierarchicalPose pcn,
            HierarchicalPose pdp, HierarchicalPose pd0, HierarchicalPose pd1, HierarchicalPose pdn,
            float ua, float ub, float uc, float ud, float u)
        {
            return Cubic(
                Cubic(pap, pa0, pa1, pan, ua),
                Cubic(pbp, pb0, pb1, pbn, ub),
                Cubic(pcp, pc0, pc1, pcn, uc),
                Cubic(pdp, pd0, pd1, pdn, ud),
                u);
        }

        public static SpacialPose LerpCompositeOne(SpacialPose a, SpacialPose b, float u)
        {


            return Concat(a, Scale(DeConcat(b, a), u));
        }

        public static SpacialPose LerpCompositeTwo(SpacialPose a, SpacialPose b, float u)
        {
            return Scale(a, 1 - u).Add(Scale(b, u));
        }
    }
}