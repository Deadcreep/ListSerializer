using BenchmarkDotNet.Running;
using Microsoft.Diagnostics.Tracing.Parsers.AspNet;

namespace ListSerializer
{
	public class Program
	{
		public static string FilePath => Path.Combine(Directory.GetCurrentDirectory(), "data.bytes");

		private static void Main()
		{
			//var summary = BenchmarkRunner.Run<Benchmark>();
			var listRand = Initialize(10, true);
			FileStream s = new FileStream(FilePath, FileMode.OpenOrCreate);
			listRand.Serialize(s);
			s.Position = 0;
			var newList = new ListRand();
			newList.Deserialize(s);
		}

		private static void PrintData(ListNode node)
		{
			Console.WriteLine($"has next: {node.Next != null}; " +
								$"has prev: {node.Prev != null}; " +
								$"{node.Data}; " +
								$"random data: {node.Rand?.Data};");
		}

		public static ListRand Initialize(int count, bool fixedDataLength)
		{
			Random random = new Random();
			var head = new ListNode()
			{
				Data = "HeadNode"
			};
			ListNode tail = new ListNode()
			{
				Data = "TailNode"
			};
			ListRand listRand = new ListRand()
			{
				Count = count,
				Head = head,
				Tail = tail
			};
			ListNode lastNode = head;
			var list = new List<ListNode>(count);
			list.Add(head);
			for (int i = 1; i < count - 1; i++)
			{
				var node = new ListNode()
				{
					Prev = lastNode,
					Data = fixedDataLength ? $"Node_" + i : $"Node_" + string.Join("_", Enumerable.Repeat(i, random.Next(1000)))
				};
				lastNode.Next = node;
				list.Add(node);
				lastNode = node;
			}
			lastNode.Next = tail;
			tail.Prev = lastNode;
			list.Add(tail);
			for (int i = 0; i < count; i++)
			{
				list[i].Rand = random.Next(100) < 10 ? null : list[random.Next(list.Count)];
			}
			return listRand;
		}

		public static IEnumerable<ListNode> EnumerateNodes(ListNode start)
		{
			ListNode node = start;
			while (node != null)
			{
				yield return node;
				node = node.Next;
			}
		}
	}
}