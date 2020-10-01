/*
	Advanced Animation Programming
	By Jake Ruth

    HierarchyNode.cs - class to represent a decoupled node of data in a hierarchy
*/

using System;

namespace AdvAnimation
{
    public class HierarchyNode
    {
        public string name;
        public int index;
        public int parentIndex;

        public HierarchyNode(string name, int index, int parentIndex)
        {
            this.name = name;
            this.index = index;
            this.parentIndex = parentIndex;

            if (this.parentIndex > this.index)
                throw new Exception("Parent Index must be higher up on the tree (a lower value) than the index");
        }
    }
}