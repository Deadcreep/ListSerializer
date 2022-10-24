using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace ListSerializer
{
	public class ListNode
	{
		public ListNode Prev;
		public ListNode Next;
		public ListNode Rand;
		public string Data;
	}

	public class ListRand
	{
		public ListNode Head;
		public ListNode Tail;

		public int Count;

		public virtual void Serialize(FileStream s)
		{
			unsafe
			{
				Dictionary<IntPtr, (IntPtr ptrOfRand, int index, byte[] data)> dict =
						new Dictionary<IntPtr, (IntPtr ptrOfRand, int index, byte[] data)>(this.Count);

				ListNode node = this.Head;
				var totalSize = (this.Count * (4 * 2)) + 4;
				for (int i = 0; i < this.Count; i++)
				{
					TypedReference tr = __makeref(node);
					IntPtr ptr = **(IntPtr**)(&tr);

					TypedReference trRand = __makeref(node.Rand);
					IntPtr ptrRand = **(IntPtr**)(&trRand);

					var bData = Encoding.ASCII.GetBytes(node.Data);
					dict.Add(ptr, new(ptrRand, i, bData));
					totalSize += bData.Length;
					node = node.Next;
				}
				var intPtr = Marshal.AllocHGlobal(totalSize);
				byte* p = (byte*)intPtr.ToPointer();

				var bCount = BitConverter.GetBytes(this.Count);
				p = MovePointer(p, bCount);

				node = this.Head;
				for (int i = 0; i < this.Count; i++)
				{
					
					TypedReference tr = __makeref(node);
					IntPtr ptr = **(IntPtr**)(&tr);

					int indexOfRand = -1;
					if (node.Rand != null)
					{
						if (ReferenceEquals(node, node.Rand))
						{
							indexOfRand = dict[ptr].index;
						}
						else
						{
							TypedReference trRand = __makeref(node.Rand);
							IntPtr ptrRand = **(IntPtr**)(&trRand);							
							indexOfRand = dict[ptrRand].index;
						}
					}
					var bIndexOfRand = BitConverter.GetBytes(indexOfRand);

					p = MovePointer(p, bIndexOfRand);
					var info = dict[ptr];
					var bData = info.data;
					var bLength = BitConverter.GetBytes(bData.Length);
					p = MovePointer(p, bLength);
					p = MovePointer(p, bData);
					node = node.Next;
				}

				var bb = new byte[totalSize];

				Marshal.Copy(intPtr, bb, 0, totalSize);
				s.Write(bb);
				Marshal.FreeHGlobal(intPtr);

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				static unsafe byte* MovePointer(byte* p, byte[] array)
				{
					for (int i = 0; i < array.Length; i++, p++)
					{
						*p = array[i];
					}
					return p;
				}
			}
		}

		public virtual void Deserialize(FileStream s)
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