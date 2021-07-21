using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Calandiel.Collections
{
	public struct UnmanagedDictionaryThreadSafe<TKey, TValue> :
		IDisposable
		where TValue : unmanaged
		where TKey : unmanaged, IEquatable<TKey>
	{
		private Box<int> _lock;
		private Box<UnmanagedDictionary<TKey, TValue>> collection;

		private void Lock()
		{
			unsafe
			{
				Mutex.Lock(ref *(int*)_lock.Raw());
			}
		}
		private void Unlock()
		{
			unsafe
			{
				Mutex.Unlock(ref *(int*)_lock.Raw());
			}
		}

		public void Clear()
		{
			Lock();
			var c = collection.Read();
			c.Clear();
			collection.Write(c);
			Unlock();
		}
		public bool ContainsKey(TKey key)
		{
			Lock();
			var c = collection.Read();
			var ret = c.ContainsKey(key);
			Unlock();
			return ret;
		}
		public void Set(TKey key, TValue val)
		{
			Lock();
			var c = collection.Read();
			c.Set(key, val);
			collection.Write(c);
			Unlock();
		}
		public void DeleteKey(TKey key)
		{
			Lock();
			var c = collection.Read();
			c.DeleteKey(key);
			collection.Write(c);
			Unlock();
		}
		public bool TryGetValue(TKey key, out TValue result)
		{
			Lock();
			var c = collection.Read();
			var ret = c.TryGetValue(key, out result);
			Unlock();
			return ret;
		}
		public TValue this[TKey key]
		{
			get
			{
				Lock();
				var c = collection.Read();
				var ret = c[key];
				Unlock();
				return ret;
			}
			set
			{
				Lock();
				var c = collection.Read();
				c[key] = value;
				collection.Write(c);
				Unlock();
			}
		}
		public TValue GetOrDefault(TKey key)
		{
			Lock();
			var c = collection.Read();
			var ret = c.GetOrDefault(key);
			Unlock();
			return ret;
		}
		public float LoadFactor
		{
			get
			{
				Lock();
				var c = collection.Read();
				var ret = c.LoadFactor;
				Unlock();
				return ret;
			}
		}
		public int Capacity
		{
			get
			{
				Lock();
				var c = collection.Read();
				var ret = c.Capacity;
				Unlock();
				return ret;
			}
		}
		public int Size
		{
			get
			{
				Lock();
				var c = collection.Read();
				var ret = c.Size;
				Unlock();
				return ret;
			}
		}
		public bool IsCreated
		{
			get
			{
				Lock();
				var c = collection.Read();
				var ret = c.IsCreated;
				Unlock();
				return ret;
			}
		}
		public bool TryGetAtIndex(int index, out TKey key, out TValue value)
		{
			Lock();
			var c = collection.Read();
			var ret = c.TryGetAtIndex(index, out key, out value);
			Unlock();
			return ret;
		}

		/// <summary>
		/// NOT THREAD SAFE!
		/// </summary>
		/// <param name="capacity"></param>
		public void Create(uint capacity = 4)
		{
			_lock = Box<int>.Create(0);
			collection = Box<UnmanagedDictionary<TKey, TValue>>.Create(new UnmanagedDictionary<TKey, TValue>(capacity));
		}
		/// <summary>
		/// NOT THREAD SAFE!
		/// </summary>
		public void Dispose()
		{
			_lock.Dispose();
			collection.Read().Dispose();
			collection.Dispose();
		}
	}
}