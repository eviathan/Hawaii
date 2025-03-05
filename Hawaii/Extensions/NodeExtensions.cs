using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hawaii.Extensions
{
    public static class NodeExtensions
    {
        public static IEnumerable<Node> TraverseDepthFirst(this Node node)
        {
            yield return node;

            foreach (var child in node.Children)
                foreach (var descendant in TraverseDepthFirst(child))
                    yield return descendant;
        }
    }
}
