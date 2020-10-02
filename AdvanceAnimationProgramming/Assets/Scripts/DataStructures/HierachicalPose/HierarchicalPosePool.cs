/*
	Advanced Animation Programming
	By Jake Ruth

    hierarchicalPosePool.cs - A pool or group of hierarchical poses and their individual node poses.
*/

using System.Collections.Generic;

namespace AdvAnimation
{
    enum SpatialPoseChannel
    {
        // identity (no channels)
        POSE_CHANNEL_NONE,

        // orientation
        POSE_CHANNEL_ORIENT_X = 0x0001,
        POSE_CHANNEL_ORIENT_Y = 0x0002,
        POSE_CHANNEL_ORIENT_Z = 0x0004,
        POSE_CHANNEL_ORIENT_XY = POSE_CHANNEL_ORIENT_X | POSE_CHANNEL_ORIENT_Y,
        POSE_CHANNEL_ORIENT_YZ = POSE_CHANNEL_ORIENT_Y | POSE_CHANNEL_ORIENT_Z,
        POSE_CHANNEL_ORIENT_ZX = POSE_CHANNEL_ORIENT_Z | POSE_CHANNEL_ORIENT_X,
        POSE_CHANNEL_ORIENT_XYZ = POSE_CHANNEL_ORIENT_XY | POSE_CHANNEL_ORIENT_Z,

        // scale
        POSE_CHANNEL_SCALE_X = 0x0010,
        POSE_CHANNEL_SCALE_Y = 0x0020,
        POSE_CHANNEL_SCALE_Z = 0x0040,
        POSE_CHANNEL_SCALE_XY = POSE_CHANNEL_SCALE_X | POSE_CHANNEL_SCALE_Y,
        POSE_CHANNEL_SCALE_YZ = POSE_CHANNEL_SCALE_Y | POSE_CHANNEL_SCALE_Z,
        POSE_CHANNEL_SCALE_ZX = POSE_CHANNEL_SCALE_Z | POSE_CHANNEL_SCALE_X,
        POSE_CHANNEL_SCALE_XYZ = POSE_CHANNEL_SCALE_XY | POSE_CHANNEL_SCALE_Z,

        // translation
        POSE_CHANNEL_TRANSLATE_X = 0x0100,
        POSE_CHANNEL_TRANSLATE_Y = 0x0200,
        POSE_CHANNEL_TRANSLATE_Z = 0x0400,
        POSE_CHANNEL_TRANSLATE_XY = POSE_CHANNEL_TRANSLATE_X | POSE_CHANNEL_TRANSLATE_Y,
        POSE_CHANNEL_TRANSLATE_YZ = POSE_CHANNEL_TRANSLATE_Y | POSE_CHANNEL_TRANSLATE_Z,
        POSE_CHANNEL_TRANSLATE_ZX = POSE_CHANNEL_TRANSLATE_Z | POSE_CHANNEL_TRANSLATE_X,
        POSE_CHANNEL_TRANSLATE_XYZ = POSE_CHANNEL_TRANSLATE_XY | POSE_CHANNEL_TRANSLATE_Z,

        // all
        POSE_CHANNEL_ALL = POSE_CHANNEL_ORIENT_XYZ | POSE_CHANNEL_SCALE_XYZ | POSE_CHANNEL_TRANSLATE_XYZ,
    };

    class HierarchicalPosePool
    {
        public Hierarchy hierarchy;
        public List<SpacialPose> spacialPosePool;
        public List<HierarchicalPose> hierarchicalPoses;
        public List<SpatialPoseChannel> channels;
        public int eulerOrder; // Flag for for the pool that describes the concatenation order of orientation channels

        public int HierarchicalPoseCount
        {
            get { return hierarchicalPoses.Count; }
        }

        public int SpacialPoseCount
        {
            get { return spacialPosePool.Count; }
        }
    }
}
