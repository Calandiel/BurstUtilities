using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Calandiel.Collections;
using UnityEngine;

namespace Gem.Data
{
	[System.Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct ID : System.IEquatable<ID>
	{
		public int Index;
		public uint Version;

		public ID(int INDEX, uint VERSION)
		{
			this.Index = INDEX;
			this.Version = VERSION;
		}

		public override string ToString()
		{
			return Index.ToString();
		}

		public Vector2 ToVector()
		{
			return new Vector2(Index, Version);
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}
		public bool Equals(ID other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			int hashCode = -561076678;
			hashCode = hashCode * -1521134295 + Index.GetHashCode();
			hashCode = hashCode * -1521134295 + Version.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(ID a, ID b)
		{
			return a.Index == b.Index && a.Version == b.Version;
		}
		public static bool operator !=(ID a, ID b)
		{
			return a.Index != b.Index || a.Version != b.Version;
		}
	}
}
