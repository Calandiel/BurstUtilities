using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Calandiel
{
	public struct InterlockedInt
	{
		[NativeDisableUnsafePtrRestriction]
		private unsafe long* ptr;

		public long Read()
		{
			unsafe
			{
				return System.Threading.Interlocked.Read(ref *ptr);
			}
		}
		public void Increment()
		{
			unsafe
			{
				System.Threading.Interlocked.Increment(ref *ptr);
			}
		}

		public bool IsCreated
		{
			get
			{
				unsafe
				{
					return (IntPtr)ptr != IntPtr.Zero;
				}
			}
		}

		public void Dispose()
		{
			unsafe
			{
				if (IsCreated)
				{
					UnsafeUtility.Free(ptr, Allocator.Persistent);
					ptr = null;
				}
			}
		}

		public static InterlockedInt Create()
		{
			var ii = new InterlockedInt();

			unsafe
			{
				ii.ptr = (long*)UnsafeUtility.Malloc(8, 0, Allocator.Persistent);
				*ii.ptr = 0;
			}
			return ii;
		}
	}
}