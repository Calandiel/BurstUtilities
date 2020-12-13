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

		public static void SafeWrite<T>(IntPtr ptr, T value, int index) where T : unmanaged
		{
			unsafe
			{
				var p = (T*)ptr;
				p[index] = value;
			}
		}
		public static T SafeRead<T>(IntPtr ptr, int index) where T : unmanaged
		{
			unsafe
			{
				var p = (T*)ptr;
				return p[index];
			}
		}
	}
}