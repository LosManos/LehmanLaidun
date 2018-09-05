using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LehmanLaidun.FileSystem
{
    // https://stackoverflow.com/a/2012855/521554
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public class Node<T>
	{
        public LinkedList<Node<T>> Children { get; }

        public T Data { get; }

        public Node(T data)
		{
			Data = data;
			Children = new LinkedList<Node<T>>();
		}

		public Node<T> AddChild(T data)
		{
            var tree = new Node<T>(data);
            AddChild(tree);
            return tree;
		}

        public void AddChild(Node<T> tree)
        {
            Children.AddLast(tree);
        }

		public Node<T> GetChild(int i)
		{
			foreach (var n in Children)
				if (--i == 0)
					return n;
			return null;
		}

		public void Traverse(Node<T> node, Action<T> visitor)
		{
			visitor(node.Data);
			foreach (var kid in node.Children)
				Traverse(kid, visitor);
		}

		public static Node<T> Parse(string inputString, Func<string,T>transformer)
		{
            var lines = inputString.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            //var head = lines.First();
            //var tail = lines.Skip(1);
            var root = new Node<T>(transformer("root"));
            var x = Parse(
                root,
                lines,
                transformer, 
                0).Item1;
            return root;
		}


		private static (Node<T>,IEnumerable<string>) Parse(Node<T> localRoot, IEnumerable<string> lines, Func<string,T>transformer, int depth)
        {
            Node<T> latest = null;

            while (lines.Any())
            {
                var head = lines.First();
                var tail = lines.Skip(1);
                if (DepthOf(head) == depth)
                {
                    latest = localRoot.AddChild(transformer(head.Trim()));
                }
                else if (DepthOf(head) > depth)
                {
                    if (latest == null) throw new Exception($"Variable [{nameof(latest)}] should not be null.");
                    (latest, tail) = Parse(latest, lines, transformer, depth + 1);
                }
                else
                {
                    return (localRoot, tail);
                }
                lines = tail;
            }
            return (localRoot,lines);
        }

        private static int DepthOf(string line)
        {
            return line.TakeWhile(Char.IsWhiteSpace).Count();
        }

#if DEBUG
        private string DebuggerDisplay()
        {
            return
                $"Data:{Data}, ChildrenCkount:{(Children??new LinkedList<Node<T>>()).Count()}, Type:{GetType()}";
        }
    }
#endif
}
