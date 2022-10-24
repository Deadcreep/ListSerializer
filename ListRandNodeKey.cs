using System.Text;

namespace ListSerializer
{
	public class ListRandNodeKey : ListRand
	{
		public override void Serialize(FileStream s)
		{
			Dictionary<ListNode, (int index, byte[] data)> dict =
					new Dictionary<ListNode, (int index, byte[] data)>(this.Count);

			var totalSize = (this.Count * (4 * 2)) + 4;

			ListNode node = this.Head;
			for (int i = 0; i < this.Count; i++)
			{
				var bData = Encoding.ASCII.GetBytes(node.Data);
				dict.Add(node, new(i, bData));
				totalSize += bData.Length;
				node = node.Next;
			}

			byte[] array = new byte[totalSize];
			int p = 0;
			var bCount = BitConverter.GetBytes(this.Count);

			CopyArray(ref p, ref array, bCount);

			node = Head;
			while (node != null)
			{
				var info = dict[node];
				byte[] bIndexOfRand;
				if (node.Rand == null)
				{
					bIndexOfRand = BitConverter.GetBytes(-1);
				}
				else
				{
					var indexOfRand = dict[node.Rand].index;
					bIndexOfRand = BitConverter.GetBytes(indexOfRand);
				}
				CopyArray(ref p, ref array, bIndexOfRand);
				var bDataLength = BitConverter.GetBytes(info.data.Length);
				CopyArray(ref p, ref array, bDataLength);
				CopyArray(ref p, ref array, info.data);
				node = node.Next;
			}



			s.Write(array);

			static void CopyArray(ref int p, ref byte[] dest, in byte[] from)
			{
				for (int i = 0; i < from.Length; i++, p++)
				{
					dest[p] = from[i];
				}
			}
		}

		public override void Deserialize(FileStream s)
		{
			int INT_SIZE = 4;
			var bCount = new byte[INT_SIZE];
			s.Read(bCount, 0, bCount.Length);
			this.Count = BitConverter.ToInt32(bCount);
			List<ListNode> list = new List<ListNode>(this.Count);
			int[] indeces = new int[this.Count];

			for (int i = 0; i < this.Count; i++)
			{
				ListNode node = new ListNode();
				var bIndexOfRand = new byte[INT_SIZE];
				s.Read(bIndexOfRand, 0, bIndexOfRand.Length);

				var indexOfRand = BitConverter.ToInt32(bIndexOfRand);
				indeces[i] = indexOfRand;
				var bLength = new byte[INT_SIZE];
				s.Read(bLength, 0, bLength.Length);
				var dataLength = BitConverter.ToInt32(bLength);
				var bData = new byte[dataLength];
				s.Read(bData, 0, dataLength);
				string data = Encoding.ASCII.GetString(bData);
				node.Data = data;
				list.Add(node);
			}
			for (int i = 1; i < list.Count - 1; i++)
			{
				var node = list[i];
				node.Prev = list[i - 1];
				node.Next = list[i + 1];
				if (indeces[i] != -1)
				{
					node.Rand = list[indeces[i]];
				}
			}
			this.Head = list[0];
			this.Head.Next = list[1];
			this.Head.Rand = indeces[0] != -1 ? list[indeces[0]] : null;

			this.Tail = list[list.Count - 1];
			this.Tail.Rand = indeces[list.Count - 1] != -1 ? list[indeces[list.Count - 1]] : null;
			this.Tail.Prev = list[list.Count - 2];
		}
	}
}