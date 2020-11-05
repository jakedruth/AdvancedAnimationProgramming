﻿/*
	Advanced Animation Programming
	By Jake Ruth

    hierarchicalState.cs - The complete pose information for a given hierarchy. 
*/

namespace AdvAnimation
{
    class HierarchicalState
    {
        public Hierarchy hierarchy = new Hierarchy();
        //public HierarchicalPose samplePose;
        //public HierarchicalPose localSpacePose;
        //public HierarchicalPose objectSpacePose;

        public void SolveForwardKinematics()
        {
            SolveForwardKinematics(this, 0, hierarchy.Count);
        }

        public void SolveForwardKinematics(int firstIndex, int nodeCount)
        {
            SolveForwardKinematics(this, firstIndex, nodeCount);
        }

        public static void SolveForwardKinematics(HierarchicalState state)
        {
            SolveForwardKinematics(state, 0, state.hierarchy.Count);
        }

        public static void SolveForwardKinematics(HierarchicalState state, int firstIndex, int nodeCount)
        {
            // TODO: implement forward kinematics algorithm
            //  - for all nodes starting at first index
            //      - if node is not root (has parent node)
            //			- object matrix = parent object matrix * local matrix
            //		- else
            //			- copy local matrix to object matrix
        }
    }
}
