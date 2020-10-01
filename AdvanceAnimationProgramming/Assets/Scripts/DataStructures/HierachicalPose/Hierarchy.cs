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

        public void AddNode(HierarchyNode node)
        {
            _nodes.Add(node);
        }

        public void AddNode(string name, int index, int parentIndex)
        {
            AddNode(new HierarchyNode(name, index, parentIndex));
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
