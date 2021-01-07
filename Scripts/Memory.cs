using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections.LowLevel.Unsafe;

namespace Calandiel
{
	public static class Memory
	{
		public static IntPtr SafeMalloc(long size, int alignment)
		{
			unsafe
			{
				return (IntPtr)UnsafeUtility.Malloc(size, alignment, Unity.Collections.Allocator.Persistent);
			}
		}
		public static void SafeFree(IntPtr ptr)
		{
			unsafe
			{
				UnsafeUtility.Free((void*)ptr, Unity.Collections.Allocator.Persistent);
			}
		}
		//UnsafeUtility.WriteArrayElement<UnmanagedDictionary<Vector3Int, int>>(worldTilettesPtr, 0, new UnmanagedDictionary<Vector3Int, int>(1000));
		public static void SafeWriteStruct<T>(IntPtr ptr, T value, int index) where T : struct
		{
			unsafe
			{
				UnsafeUtility.WriteArrayElement<T>(ptr.ToPointer(), index, value);
			}
		}
		public static T SafeReadStruct<T>(IntPtr ptr, int index) where T : struct
		{
			unsafe
			{
				return UnsafeUtility.ReadArrayElement<T>(ptr.ToPointer(), index);
			}
		}

		public static void SafeWriteUnmanaged<T>(IntPtr ptr, T value, int index) where T : unmanaged
		{
			unsafe
			{
				var p = (T*)ptr;
				p[index] = value;
			}
		}
		public static T SafeReadUnmanaged<T>(IntPtr ptr, int index) where T : unmanaged
		{
			unsafe
			{
				var p = (T*)ptr;
				return p[index];
			}
		}
	}
}