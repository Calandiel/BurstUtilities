using Calandiel.Collections;
using Unity.Mathematics;

namespace Calandiel.Collections
{
	public struct Octree
	{
		#region PUBLIC INTERFACE AND PRIVATE DATA
		private UnmanagedHashSet<uint> data;
		private uint levels;
		public uint Levels { get => levels; }
		public bool this[int x, int y, int z]
		{
			get => Get((uint)x, (uint)y, (uint)z);
			set => Set((uint)x, (uint)y, (uint)z, value);
		}
		#endregion

		#region MORTON CURVE
		private uint NodeFromCoords(uint x, uint y, uint z)
		{
			// x -- left/right
			// z -- back/front
			// y -- up/down
			x = (x | (x << 16)) & 0x030000FF;
			x = (x | (x << 8)) & 0x0300F00F;
			x = (x | (x << 4)) & 0x030C30C3;
			x = (x | (x << 2)) & 0x09249249;

			y = (y | (y << 16)) & 0x030000FF;
			y = (y | (y << 8)) & 0x0300F00F;
			y = (y | (y << 4)) & 0x030C30C3;
			y = (y | (y << 2)) & 0x09249249;

			z = (z | (z << 16)) & 0x030000FF;
			z = (z | (z << 8)) & 0x0300F00F;
			z = (z | (z << 4)) & 0x030C30C3;
			z = (z | (z << 2)) & 0x09249249;

			return ((uint)1 << 31) | (x << 1) | (y << 2) | z;
		}
		private uint3 CoordsFromNode(uint node)
		{
			var level = Level(node);
			while (level != levels)
			{
				node = Child(node, (byte)Children.BottomLeftBack);
				level++;
			}

			uint mask = 0b0000_1001_0010_0100_1001_0010_0100_1001;
			uint zn = node & mask;
			uint xn = (node >> 1) & mask;
			uint yn = (node >> 2) & mask;

			zn = (zn | (zn >> 2)) & 0x030C30C3;
			zn = (zn | (zn >> 4)) & 0x0300F00F;
			zn = (zn | (zn >> 8)) & 0x030000FF;
			zn = (zn | (zn >> 16)) & 0x0000FFFF;

			yn = (yn | (yn >> 2)) & 0x030C30C3;
			yn = (yn | (yn >> 4)) & 0x0300F00F;
			yn = (yn | (yn >> 8)) & 0x030000FF;
			yn = (yn | (yn >> 16)) & 0x0000FFFF;

			xn = (xn | (xn >> 2)) & 0x030C30C3;
			xn = (xn | (xn >> 4)) & 0x0300F00F;
			xn = (xn | (xn >> 8)) & 0x030000FF;
			xn = (xn | (xn >> 16)) & 0x0000FFFF;

			return new uint3(xn, yn, zn);
		}
		#endregion

		#region GET/SET
		private bool Get(uint x, uint y, uint z) => InnerGet(x, y, z, out uint _, out uint _, out uint _);
		private bool InnerGet(uint x, uint y, uint z, out uint nodeAtPos, out uint trueNode, out uint trueNodeLevel)
		{
			nodeAtPos = NodeFromCoords(x, y, z);
			trueNode = 0;
			trueNodeLevel = 0;
			var node = nodeAtPos;
			int level = (int)levels;

			do
			{
				if (data.ContainsKey(node))
				{
					trueNode = node;
					trueNodeLevel = (uint)level;
					return true;
				}
				else
				{
					node = Parent(node);
					level--;
				}
			} while (level >= 0);
			return false;
		}
		private void Set(uint x, uint y, uint z, bool value)
		{
			var val = InnerGet(x, y, z, out var node, out var trueNode, out var trueNodeLevel);
			if (val == value)
				return;
			else
			{
				if (value)
				{
					int level = (int)levels;
					// adding
					// since we're here, right now the value at this position is false
					// we need to check our parents children
					// in order to determine if we can merge
					// if we can, we need to recurse upwards
					// if we can't, we just write our node
					do
					{
						var parent = Parent(node);
						bool canMerge = true;

						// Determine children of our parent that aren't the same as the node we're currently inspecting
						StackBasedArrayUInt7 children = default;
						for (uint i = 0; i < 8; i++)
						{
							var child = Child(parent, i);
							if (child != node)
								children[i] = child;
						}
						// Determine if a merge is possible.
						for (uint i = 0; i < 7; i++)
						{
							var child = children[i];
							if (!data.ContainsKey(child))
							{
								canMerge = false;
								break;
							}
						}
						if (canMerge)
						{
							// If a merge is possible, remove all children
							for (uint i = 0; i < 7; i++)
							{
								var child = children[i];
								data.Remove(child);
							}
							if (data.ContainsKey(node))
								data.Remove(node);
							// After the merge, we can set the parent to true
							data.Add(parent);
						}
						else
						{
							// If we can't merge, just add ourselves
							data.Add(node);
							return;
						}
						node = parent;
						level--;
					} while (level > 0);
				}
				else
				{
					int deltaLevel = (int)(levels - trueNodeLevel);
					do
					{
						data.Remove(trueNode);
						for (uint i = 0; i < 8; i++)
							data.Add(Child(trueNode, i));
						deltaLevel--;

						trueNode = node >> (int)(3 * deltaLevel);
					} while (deltaLevel > 0);
					data.Remove(node);
				}
			}
		}
		#endregion

		#region NODE ID MANIPULATIONS
		private uint Parent(uint node) => node >> 3;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		/// <param name="child_id">A value from 0 (inclusive) to 8 (exclusive).</param>
		/// <returns>ID of the child node for the corresponding child id</returns>
		private uint Child(uint node, uint child_id) => (node << 3) | child_id;
		#endregion

		#region LEVEL AND EDGE SIZES
		/// <summary>
		/// Returns the child node in which this node is located.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="pos"></param>
		/// <returns></returns>
		private uint Level(uint node)
		{
			uint ldz = (uint)math.lzcnt(node);
			uint overallLevel = (31 - ldz) / 3;
			return overallLevel - (10 - levels);
		}
		private uint EdgeSize(uint node)
		{
			uint lvl = Level(node);
			return ((uint)1) << (int)(levels - lvl);
		}
		private uint MaxSize()
		{
			var node = NodeFromCoords(0, 0, 0); // this node is always "in"
			while (Level(node) != 0) node = Parent(node);
			return EdgeSize(node);
		}
		public int Edge { get => (int)MaxSize(); }
		#endregion

		#region MEMORY MANAGEMENT
		public void Init(uint levels)
		{
			if (levels > 10) throw new System.Exception("32-bit linear octrees can only support volumes up to 1024 blocks big! Decrease the max octree level!");
			this.levels = levels;
			data = new UnmanagedHashSet<uint>(10);
		}

		public void Dispose()
		{
			this.levels = 0;
			data.Dispose();
		}
		#endregion

		#region DATA TYPES
		private enum Children : byte
		{
			BottomLeftBack = 0,  // 000
			BottomLeftFront = 1,   // 001
			BottomRightBack = 2, // 010
			BottomRightFront = 3,  // 011
			TopLeftBack = 4,     // 100
			TopLeftFront = 5,      // 101
			TopRightBack = 6,    // 110
			TopRightFront = 7,     // 111
		}
		#endregion
	}
}