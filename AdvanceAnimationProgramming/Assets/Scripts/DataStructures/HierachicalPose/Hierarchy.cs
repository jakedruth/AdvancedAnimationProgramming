/*
	Advanced Animation Programming
	By Jake Ruth

    Hierarchy.cs - class to hold Hierarchy Nodes
*/

using System.Collections.Generic;

namespace AdvAnimation
{
    class Hierarchy
    {
        private readonly List<HierarchyNode> _nodes;

        public int Count
        {
            get { return _nodes.Count; }
        }

        public Hierarchy(params HierarchyNode[] nodes)
        {
            _nodes = new List<HierarchyNode>();
            _nodes.AddRange(nodes);
        }

        public int AddNode(string name, int parentIndex)
        {
            HierarchyNode node = new HierarchyNode(name, Count, parentIndex);
            _nodes.Add(node);
            return node.index;
        }

        public HierarchyNode this[int i]
        {
            get { return _nodes[i]; }
            set { _nodes[i] = value; }
        }

        public HierarchyNode this[string name]
        {
            get
            {
                //return _nodes.Find(n => n.name == name);

                for (int i = 0; i < _nodes.Count; i++)
                {
                    if (_nodes[i].name == name)
                        return _nodes[i];
                }

                return null;
            }
        }
    }
}
