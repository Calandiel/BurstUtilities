using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Calandiel
{
	//an example with a pointer:
	// Mutex.Lock(ref *(int*)m_Buffer);
	public struct Mutex
	{
		public static void Lock(ref int r)
		{
			unsafe
			{
				while (1 == Interlocked.Exchange(ref r, 1))
				{
					// do nothing, we're just waiting for the lock to free
				}
			}
		}
		public static void Unlock(ref int r)
		{
			unsafe
			{
				Interlocked.Exchange(ref r, 0);
			}
		}
	}

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