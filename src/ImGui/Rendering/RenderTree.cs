using System;
using System.Collections.Generic;
using ImGui.Layout;
using ImGui.Common.Primitive;

namespace ImGui.Rendering
{
    internal class RenderTree
    {
        public Node Root { get; }

        private Node currentContainer;
        public Node CurrentContainer
        {
            get
            {
                return currentContainer ?? Root;
            }
            set
            {
                currentContainer = value;
            }
        }

        public RenderTree(int rootId, Point position, Size size)
        {
            Root = new Node();
            Root.Id = rootId;
            Root.Rect = new Rect(position, size);
            Root.AttachLayoutGroup(true);
        }

        public Node GetNode(int id)
        {
            return Root.GetNodeById(id);
        }

        public void Foreach(Action<Node> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException();
            }

            foreach (var node in Root.Children)
            {
                action(node);
                if (node.Children!=null && node.Children.Count != 0)
                {
                    node.Foreach(action);
                }
            }
        }
    }
}
