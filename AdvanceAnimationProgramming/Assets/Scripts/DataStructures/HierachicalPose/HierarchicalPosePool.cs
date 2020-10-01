/*
	Advanced Animation Programming
	By Jake Ruth

    hierarchicalPosePool.cs - A pool or group of hierarchical poses and their individual node poses.
*/

using System.Collections.Generic;

namespace AdvAnimation
{
    class HierarchicalPosePool
    {
        public Hierarchy hierarchy;
        public List<SpacialPose> spacialPosePool;
        public List<HierarchicalPose> hierarchicalPoses;
        // TODO: Chanenels
        // public List<Channels> channels
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
