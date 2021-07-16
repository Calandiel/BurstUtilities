using System.Runtime.CompilerServices;
using UnityEngine;
namespace Calandiel
{
	public interface IBitmask
	{
		void Set(byte index, bool value);
		void SetTrue(byte index);
		void SetFalse(byte index);
		bool Get(byte index);
	}
	[System.Serializable]
	public struct Bitmask : IBitmask
	{
		public byte data;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(byte index, bool value)
		{
			if (value)
				SetTrue(index);
			else
				SetFalse(index);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetTrue(byte index)
		{
			data = (byte)(data | (1 << index));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetFalse(byte index)
		{
			data = (byte)(data & ~(1 << index));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Get(byte index)
		{
			return 1 == ((data & (1 << index)) >> index);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Print() => Debug.Log(System.Convert.ToString(data, 2).PadLeft(8, '0'));
	}
}