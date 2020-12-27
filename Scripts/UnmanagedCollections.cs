using System;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Calandiel.Collections
{
	public class UnmanagedCollectionAttribute : Attribute
	{
	}
	public interface IUnmanagedCollectionSave
	{
		public void Save(System.IO.BinaryWriter writer);
		public void Load(System.IO.BinaryReader reader);
	}


	public struct SafePtr<T> : IEquatable<SafePtr<T>> where T : unmanaged
	{
		public long val;
		public bool Equals(SafePtr<T> other) => this.val == other.val;
		public unsafe static explicit operator T*(SafePtr<T> ptr) => (T*)(new IntPtr(ptr.val)).ToPointer();
		public unsafe static explicit operator SafePtr<T>(T* ptr) => new SafePtr<T>() { val = ((IntPtr)ptr).ToInt64() };
	}

	public struct Box<T> : IDisposable where T : unmanaged
	{
		[NativeDisableUnsafePtrRestriction] private unsafe T* ptr;

		public static Box<T> FromSafePtr(SafePtr<T> data)
		{
			unsafe
			{
				var box = new Box<T>();
				box.ptr = (T*)(new IntPtr(data.val).ToPointer());
				return box;
			}
		}
		public static Box<T> Create(T data)
		{
			unsafe
			{
				var box = new Box<T>();
				box.ptr = (T*)UnsafeUtility.Malloc(sizeof(T), UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
				*box.ptr = data;
				return box;
			}
		}
		public void Dispose()
		{
			unsafe
			{
				UnsafeUtility.Free((void*)ptr, Allocator.Persistent);
			}
		}
		public T Read() { unsafe { return *ptr; } }
		public void Write(T data) { unsafe { *ptr = data; } }
		public unsafe T* Raw() { unsafe { return ptr; } }
	}

	#region EXTENSIONS
	public static class ListExtensions
	{
		/// <summary>
		/// Checks whether or not the collection contains an item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool Contains<T>(this UnmanagedList<T> list, T item) where T : unmanaged, IEquatable<T>
		{
			for (int i = 0; i < list.Length; i++)
				if (list[i].Equals(item))
					return true;
			return false;
		}
		/// <summary>
		/// Checks whether or not the collection contains an item.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="index">This value is set to the index at which item is first found. If the list doesn't contain item, it's not modified.</param>
		/// <returns></returns>
		public static bool Contains<T>(this UnmanagedList<T> list, T item, ref int index) where T : unmanaged, IEquatable<T>
		{
			for (int i = 0; i < list.Length; i++)
				if (list[i].Equals(item))
				{
					index = i;
					return true;
				}
			return false;
		}
		public static void Remove<T>(ref this UnmanagedList<T> list, T item) where T : unmanaged, IEquatable<T>
		{
			int i = 0;
			if (list.Contains(item, ref i))
				list.RemoveAt(i);
			else
				throw new System.Exception("The item is not present in the collection!");
		}
	}

	public static class ArrayExtensions
	{
		/// <summary>
		/// Checks whether or not the collection contains an item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool Contains<T>(this UnmanagedArray<T> list, T item) where T : unmanaged, IEquatable<T>
		{
			for (int i = 0; i < list.Length; i++)
				if (list[i].Equals(item))
					return true;
			return false;
		}
		/// <summary>
		/// Checks whether or not the collection contains an item.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="index">This value is set to the index at which item is first found. If the list doesn't contain item, it's not modified.</param>
		/// <returns></returns>
		public static bool Contains<T>(this UnmanagedArray<T> list, T item, ref int index) where T : unmanaged, IEquatable<T>
		{
			for (int i = 0; i < list.Length; i++)
				if (list[i].Equals(item))
				{
					index = i;
					return true;
				}
			return false;
		}
	}

	public static class StackExtensions
	{

	}

	public static class QueueExtensions
	{

	}
	#endregion

	#region COLLECTIONS
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedCollection]
	public struct UnmanagedList<T> : IDisposable, IUnmanagedCollectionSave where T : unmanaged
	{
		[NativeDisableUnsafePtrRestriction]
		private unsafe void* m_Buffer;
		private int m_Length;
		private uint m_Capacity;
		public unsafe T* BasePtr { get { return (T*)m_Buffer; } }


		public UnmanagedList(uint defaultCapacity = 1)
		{
			unsafe
			{
				var length = defaultCapacity;
				if (length < 1) length = 1;

				long totalSize = UnsafeUtility.SizeOf<T>() * length;

				m_Buffer = UnsafeUtility.Malloc(totalSize, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
				UnsafeUtility.MemClear(m_Buffer, totalSize);

				m_Capacity = length;
				m_Length = 0;
			}
		}

		public void Add(T item)
		{
			unsafe
			{
				if (m_Length == m_Capacity)
				{
					if (m_Capacity > 0)
					{
						long previousCapacity = UnsafeUtility.SizeOf<T>() * m_Capacity;
						long newCapacity = UnsafeUtility.SizeOf<T>() * (m_Capacity * 2);

						void* newBuffer = UnsafeUtility.Malloc(newCapacity, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
						UnsafeUtility.MemCpy(newBuffer, m_Buffer, previousCapacity);
						UnsafeUtility.Free(m_Buffer, Allocator.Persistent);
						m_Buffer = newBuffer;
						m_Capacity = m_Capacity * 2;
					}
					else
					{
						m_Capacity = 1;
						m_Buffer = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>() * 1, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
						UnsafeUtility.MemClear(m_Buffer, UnsafeUtility.SizeOf<T>() * 1);
					}
				}

				m_Length++;
				UnsafeUtility.WriteArrayElement<T>(m_Buffer, m_Length - 1, item);
			}
		}

		/// <summary>
		/// Removes an element at a given index.
		/// NOTE: it *doesn't* remove the element equal to the index
		/// </summary>
		/// <param name="index"></param>
		public void RemoveAt(int index)
		{
			unsafe
			{
				if (index != m_Length - 1)
				{
					long indexAdress = UnsafeUtility.SizeOf<T>() * index;
					long nextAdress = UnsafeUtility.SizeOf<T>() * (index + 1);
					long movedCapacity = UnsafeUtility.SizeOf<T>() * (m_Capacity - index - 1);

					if (movedCapacity > 0)
					{
						UnsafeUtility.MemMove((byte*)m_Buffer + indexAdress, (byte*)m_Buffer + nextAdress, movedCapacity);
					}
					m_Length--;
				}
				else
				{
					m_Length--;
				}
			}
		}
		/// <summary>
		/// Completely clears the collection.
		/// </summary>
		public void Clear()
		{
			m_Length = 0;
		}

		public T this[int index]
		{
			get
			{
				unsafe
				{
					// Read the element from the allocated native memory
					return UnsafeUtility.ReadArrayElement<T>(m_Buffer, index);
				}
			}

			set
			{
				unsafe
				{
					// Writes value to the allocated native memory
					UnsafeUtility.WriteArrayElement<T>(m_Buffer, index, value);
				}
			}
		}

		public int Length { get { return m_Length; } }
		public int Capacity { get { return (int)m_Capacity; } }

		public bool IsCreated
		{
			get { unsafe { return (IntPtr)m_Buffer != IntPtr.Zero; } }
		}
		public void Dispose()
		{
			unsafe
			{
				if (m_Capacity > 0)
				{
					UnsafeUtility.Free(m_Buffer, Allocator.Persistent);
					m_Buffer = IntPtr.Zero.ToPointer();
					m_Length = 0;
					m_Capacity = 0;
				}
			}
		}

		public T[] ToArray()
		{
			var o = new T[this.Length];
			for (int i = 0; i < this.Length; i++)
				o[i] = this[i];
			return o;
		}
		public static UnmanagedList<T> FromArray(T[] arr)
		{
			var o = new UnmanagedList<T>((uint)arr.Length);
			for (int i = 0; i < arr.Length; i++)
				o.Add(arr[i]);
			return o;
		}


		public void Debug()
		{
			unsafe
			{
				var s = new System.Text.StringBuilder();

				s.Append(m_Length);
				s.Append(" -- {");
				for (int i = 0; i < m_Length; i++)
				{
					s.Append(this[i]);
					s.Append(", ");
				}
				s.Append("}");
				UnityEngine.Debug.Log(s.ToString());
			}
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write((int)this.Length);
			for (int i = 0; i < this.Length; i++)
			{
				Utilities.SavingUtility.Save(this[i], writer);
			}
		}

		public void Load(BinaryReader reader)
		{
			var len = reader.ReadInt32();
			for (int i = 0; i < len; i++)
			{
				object t = default(T);
				Utilities.SavingUtility.Load(ref t, reader);
				this.Add((T)t);
			}
		}
	}
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedCollection]
	public struct UnmanagedArray<T> : IDisposable, IUnmanagedCollectionSave where T : unmanaged
	{
		[NativeDisableUnsafePtrRestriction]
		public unsafe void* m_Buffer;
		public int m_Length;
		public unsafe T* BasePtr { get { return (T*)m_Buffer; } }

		public UnmanagedArray(uint defaultCapacity = 1)
		{
			unsafe
			{
				var length = defaultCapacity;
				if (length < 1) length = 1;

				long totalSize = UnsafeUtility.SizeOf<T>() * length;

				m_Buffer = UnsafeUtility.Malloc(totalSize, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
				UnsafeUtility.MemClear(m_Buffer, totalSize);

				m_Length = (int)length;
			}
		}

		public T this[int index]
		{
			get
			{
				unsafe
				{
					//return (T*)m_Buffer;
					// Read the element from the allocated native memory
					return UnsafeUtility.ReadArrayElement<T>(m_Buffer, index);
				}
			}
			set
			{
				unsafe
				{
					// Writes value to the allocated native memory
					UnsafeUtility.WriteArrayElement<T>(m_Buffer, index, value);
				}
			}
		}

		public int Length { get { return m_Length; } }

		public bool IsCreated
		{
			get { unsafe { return (IntPtr)m_Buffer != IntPtr.Zero; } }
		}
		public void Dispose()
		{
			unsafe
			{
				if (m_Length > 0)
				{
					UnsafeUtility.Free(m_Buffer, Allocator.Persistent);
					m_Buffer = IntPtr.Zero.ToPointer();
					m_Length = 0;
				}
			}
		}
		public T[] ToArray()
		{
			var o = new T[this.Length];
			for (int i = 0; i < this.Length; i++)
				o[i] = this[i];
			return o;
		}
		public static UnmanagedArray<T> FromArray(T[] arr)
		{
			var o = new UnmanagedArray<T>((uint)arr.Length);
			for (int i = 0; i < arr.Length; i++)
				o[i] = arr[i];
			return o;
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write((int)this.Length);
			for (int i = 0; i < this.Length; i++)
			{
				Utilities.SavingUtility.Save(this[i], writer);
			}
		}
		public void Load(BinaryReader reader)
		{
			var len = reader.ReadInt32();
			this = new UnmanagedArray<T>((uint)len);
			for (int i = 0; i < len; i++)
			{
				object t = default(T);
				Utilities.SavingUtility.Load(ref t, reader);
				this[i] = (T)t;
			}
		}
	}
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedCollection]
	public struct UnmanagedStack<T> : IDisposable where T : unmanaged
	{
		[NativeDisableUnsafePtrRestriction]
		public unsafe void* m_Buffer;
		public int m_Length;
		public uint m_Capacity;
		public unsafe T* BasePtr { get { return (T*)m_Buffer; } }

		public UnmanagedStack(uint defaultCapacity = 1)
		{
			unsafe
			{
				var length = defaultCapacity;
				if (length < 1) length = 1;

				long totalSize = UnsafeUtility.SizeOf<T>() * length;

				m_Buffer = UnsafeUtility.Malloc(totalSize, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
				UnsafeUtility.MemClear(m_Buffer, totalSize);

				m_Capacity = length;
				m_Length = 0;
			}
		}

		public void Push(T item)
		{
			unsafe
			{
				if (m_Length == m_Capacity)
				{
					if (m_Capacity > 0)
					{
						long previousCapacity = UnsafeUtility.SizeOf<T>() * m_Capacity;
						long newCapacity = UnsafeUtility.SizeOf<T>() * (m_Capacity * 2);

						void* newBuffer = UnsafeUtility.Malloc(newCapacity, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
						UnsafeUtility.MemCpy(newBuffer, m_Buffer, previousCapacity);
						UnsafeUtility.Free(m_Buffer, Allocator.Persistent);
						m_Buffer = newBuffer;
						m_Capacity = m_Capacity * 2;
					}
					else
					{
						m_Capacity = 1;
						m_Buffer = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>() * 1, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
						UnsafeUtility.MemClear(m_Buffer, UnsafeUtility.SizeOf<T>() * 1);
					}
				}

				m_Length++;
				UnsafeUtility.WriteArrayElement<T>(m_Buffer, m_Length - 1, item);
			}
		}
		public T Pop()
		{
			var o = Peek();
			m_Length--;
			return o;
		}
		public T Peek()
		{
			return this[Length - 1];
		}
		/// <summary>
		/// Removes an element at a given index.
		/// </summary>
		/// <param name="index"></param>
		private void RemoveAt(int index)
		{
			unsafe
			{
				if (index != m_Length - 1)
				{
					long indexAdress = UnsafeUtility.SizeOf<T>() * index;
					long nextAdress = UnsafeUtility.SizeOf<T>() * (index + 1);
					long movedCapacity = UnsafeUtility.SizeOf<T>() * (m_Capacity - index - 1);

					if (movedCapacity > 0)
					{
						UnsafeUtility.MemMove((byte*)m_Buffer + indexAdress, (byte*)m_Buffer + nextAdress, movedCapacity);
					}
					m_Length--;
				}
				else
				{
					m_Length--;
				}
			}
		}

		public void Clear()
		{
			m_Length = 0;
		}
		private T this[int index]
		{
			get
			{
				unsafe
				{
					// Read the element from the allocated native memory
					return UnsafeUtility.ReadArrayElement<T>(m_Buffer, index);
				}
			}

			set
			{
				unsafe
				{
					// Writes value to the allocated native memory
					UnsafeUtility.WriteArrayElement<T>(m_Buffer, index, value);
				}
			}
		}
		public int Length { get { return m_Length; } }
		public bool IsCreated
		{
			get { unsafe { return (IntPtr)m_Buffer != IntPtr.Zero; } }
		}
		public void Dispose()
		{
			unsafe
			{
				if (m_Capacity > 0)
				{
					UnsafeUtility.Free(m_Buffer, Allocator.Persistent);
					m_Buffer = IntPtr.Zero.ToPointer();
					m_Length = 0;
					m_Capacity = 0;
				}
			}
		}
		public T[] ToArray()
		{
			var o = new T[this.Length];
			for (int i = 0; i < this.Length; i++)
				o[i] = this[i];
			return o;
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write((int)this.Length);
			for (int i = 0; i < this.Length; i++)
			{
				Utilities.SavingUtility.Save(this[i], writer);
			}
		}
		public void Load(BinaryReader reader)
		{
			var len = reader.ReadInt32();
			for (int i = 0; i < len; i++)
			{
				object t = default(T);
				Utilities.SavingUtility.Load(ref t, reader);
				this.Push((T)t);
			}
		}
	}
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedCollection]
	public struct UnmanagedQueue<T> : IDisposable where T : unmanaged
	{
		[NativeDisableUnsafePtrRestriction]
		private unsafe void* m_Buffer;
		private uint m_Capacity;
		private int m_First;
		private int m_Size;
		public unsafe T* BasePtr { get { return (T*)m_Buffer; } }

		public UnmanagedQueue(uint defaultCapacity = 1)
		{
			unsafe
			{
				var length = defaultCapacity;
				if (length < 1) length = 1;

				long totalSize = UnsafeUtility.SizeOf<T>() * length;

				m_Buffer = UnsafeUtility.Malloc(totalSize, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
				UnsafeUtility.MemClear(m_Buffer, totalSize);

				m_Capacity = length;
				m_First = 0;
				m_Size = 0;
			}
		}

		public void Enqueue(T item)
		{
			unsafe
			{
				// Expand buffer if necessary
				if (Length == m_Capacity)
				{
					if (m_Capacity > 0)
					{
						var size = UnsafeUtility.SizeOf<T>();
						long newCapacity = size * (m_Capacity * 2);
						void* startingPointer = (void*)((byte*)m_Buffer + m_First * size);

						void* newBuffer = UnsafeUtility.Malloc(newCapacity, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);


						// All of the buffer is on the left side
						if (m_First + m_Size < m_Capacity)
						{
							UnsafeUtility.MemCpy(newBuffer, startingPointer, m_Size * size);
						}
						// Buffer is split into two
						else
						{
							UnsafeUtility.MemCpy(newBuffer, startingPointer, (m_Capacity - m_First) * size);
							void* offsetNewBuffer = (void*)((byte*)newBuffer + (m_Capacity - m_First) * size);
							UnsafeUtility.MemCpy(offsetNewBuffer, m_Buffer, (m_Size - m_Capacity + m_First) * size);
						}
						UnsafeUtility.Free(m_Buffer, Allocator.Persistent);
						m_Buffer = newBuffer;
						m_Capacity *= 2;

						m_First = 0;
					}
					else
					{
						m_Capacity = 1;
						m_Buffer = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>() * 1, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
						UnsafeUtility.MemClear(m_Buffer, UnsafeUtility.SizeOf<T>() * 1);
					}
				}

				this[m_First + m_Size] = item;
				m_Size++;
			}
		}
		/// <summary>
		/// Removes an element at a given index.
		/// NOTE: it *doesn't* remove the element equal to the index
		/// </summary>
		/// <param name="index"></param>
		public T Dequeue()
		{
			unsafe
			{
				if (m_Size > 0)
				{
					var v = this[m_First];
					m_First = GetIndex(m_First + 1);
					m_Size--;
					return v;
				}
				else
					throw new System.Exception("QUEUE IS EMPTY!");
			}
		}

		public T Peek()
		{
			return this[m_First];
		}
		/// <summary>
		/// Completely clears the collection.
		/// </summary>
		public void Clear()
		{
			m_First = 0;
			m_Size = 0;
		}
		private T this[int index]
		{
			get
			{
				unsafe
				{
					index = GetIndex(index);
					// Read the element from the allocated native memory
					return UnsafeUtility.ReadArrayElement<T>(m_Buffer, index);
				}
			}

			set
			{
				unsafe
				{
					index = GetIndex(index);
					// Writes value to the allocated native memory
					UnsafeUtility.WriteArrayElement<T>(m_Buffer, index, value);
				}
			}
		}
		private int GetIndex(int index) => (int)(index % m_Capacity);
		public int Length { get { return m_Size; } }
		public int Capacity { get { return (int)m_Capacity; } }
		public bool IsCreated
		{
			get { unsafe { return (IntPtr)m_Buffer != IntPtr.Zero; } }
		}
		public void Dispose()
		{
			unsafe
			{
				if (m_Capacity > 0)
				{
					UnsafeUtility.Free(m_Buffer, Allocator.Persistent);
					m_Buffer = IntPtr.Zero.ToPointer();
					m_First = 0;
					m_Size = 0;
					m_Capacity = 0;
				}
			}
		}


		/// <summary>
		/// Checks whether or not the collection contains an item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Contains<V>(V item) where V : unmanaged, IEquatable<T>
		{
			for (int i = m_First; i < Length; i++)
				if (item.Equals(this[i]))
					return true;
			return false;
		}

	}
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedCollection]
	public struct UnmanagedDictionary<TKey, TValue> :
		IDisposable
		where TValue : unmanaged, IEquatable<TValue>
		where TKey : unmanaged, IEquatable<TKey>
	{
		[NativeDisableUnsafePtrRestriction]
		public unsafe Bitmask* m_KeyPresentBuffer;
		[NativeDisableUnsafePtrRestriction]
		public unsafe TKey* m_Keys;
		[NativeDisableUnsafePtrRestriction]
		public unsafe TValue* m_Values;
		private uint m_Size;
		private uint m_Capacity;

		public const int ProbeLength = 20; // how many buckets do we check before we assume that there is no place for the item

		public UnmanagedDictionary(uint defaultCapacity)
		{
			unsafe
			{
				if (defaultCapacity < 1) defaultCapacity = 1;

				m_KeyPresentBuffer = (Bitmask*)UnsafeUtility.Malloc(1 + defaultCapacity / 8, UnsafeUtility.AlignOf<byte>(), Allocator.Persistent);
				UnsafeUtility.MemSet((void*)m_KeyPresentBuffer, 0, 1 + defaultCapacity / 8);
				m_Keys = (TKey*)UnsafeUtility.Malloc(sizeof(TKey) * defaultCapacity, UnsafeUtility.AlignOf<TKey>(), Allocator.Persistent);
				m_Values = (TValue*)UnsafeUtility.Malloc(sizeof(TValue) * defaultCapacity, UnsafeUtility.AlignOf<TValue>(), Allocator.Persistent);
				m_Size = 0;
				m_Capacity = defaultCapacity;
			}
		}

		// expands the thing uwu
		private void ExpandAndRehash()
		{
			unsafe
			{
				var oldPresence = m_KeyPresentBuffer;
				var oldKeys = m_Keys;
				var oldValues = m_Values;

				var oldCapacity = m_Capacity;
				var newCapacity = m_Capacity * 2 + 1;
				m_KeyPresentBuffer = (Bitmask*)UnsafeUtility.Malloc(1 + newCapacity / 8, UnsafeUtility.AlignOf<byte>(), Allocator.Persistent);
				UnsafeUtility.MemSet(m_KeyPresentBuffer, 0, 1 + newCapacity / 8);
				m_Keys = (TKey*)UnsafeUtility.Malloc(sizeof(TKey) * newCapacity, UnsafeUtility.AlignOf<TKey>(), Allocator.Persistent);
				m_Values = (TValue*)UnsafeUtility.Malloc(sizeof(TValue) * newCapacity, UnsafeUtility.AlignOf<TValue>(), Allocator.Persistent);
				m_Capacity = newCapacity;
				m_Size = 0;
				for (int i = 0; i < oldCapacity; i++)
				{
					if (IsSlotOccupiedOnBuffer(i, oldPresence) == true)
					{
						var k = oldKeys[i];
						var v = oldValues[i];
						Set(k, v);
					}
				}
				UnsafeUtility.Free((void*)oldPresence, Allocator.Persistent);
				UnsafeUtility.Free((void*)oldKeys, Allocator.Persistent);
				UnsafeUtility.Free((void*)oldValues, Allocator.Persistent);
			}
		}

		private bool IsSlotOccupied(int slotID)
		{
			unsafe
			{
				return IsSlotOccupiedOnBuffer(slotID, m_KeyPresentBuffer);
			}
		}
		private unsafe bool IsSlotOccupiedOnBuffer(int slotID, Bitmask* buffer)
		{
			var byteID = slotID / 8;
			var dataID = slotID - byteID * 8;
			var data = buffer[byteID];
			return data.Get((byte)dataID);
		}
		private void SetSlot(int slotID, bool value)
		{
			unsafe
			{
				var byteID = slotID / 8;
				var dataID = slotID - byteID * 8;
				var data = m_KeyPresentBuffer[byteID];
				data.Set((byte)dataID, value);
				m_KeyPresentBuffer[byteID] = data;
			}
		}

		public void Clear()
		{
			unsafe
			{
				m_Size = 0;
				for (int i = 0; i < (1 + m_Capacity / 8); i++)
				{
					m_KeyPresentBuffer[i] = new Bitmask() { data = 0 };
				}
			}
		}
		public bool ContainsKey(TKey key)
		{
			unsafe
			{
				int hash = Hash(key);

				for (int i = 0; i <= ProbeLength; i++)
				{
					var pos = Mod(hash + i, (int)m_Capacity);
					var localKey = m_Keys[pos];

					if (IsSlotOccupied(pos) == true)
					{
						if (key.Equals(localKey))
						{
							return true;
						}
					}
					else
						break;
				}
			}
			return false;
		}

		// our own mod cuz '%' is dumb
		private int Mod(int a, int m) => (a % m + m) % m;
		private int Hash(TKey i) => Mod((int)wang_hash((uint)i.GetHashCode()), (int)m_Capacity);
		uint wang_hash(uint seed)
		{
			seed = (seed ^ 61) ^ (seed >> 16);
			seed *= 9;
			seed = seed ^ (seed >> 4);
			seed *= 0x27d4eb2d;
			seed = seed ^ (seed >> 15);
			return seed;
		}
		public void Set(TKey key, TValue val)
		{
			unsafe
			{
				// check for high load factors -- we can't let it get too high or our linear scheme will get very inefficient :<
				if (LoadFactor > 0.7f) ExpandAndRehash();

				var index = Hash(key);
				int fails = 0;
				while (true)
				{
					var Key = m_Keys[index];
					if (IsSlotOccupied(index) == false)
					{
						// If the slot isn't occupied, we're writing a new value
						m_Size++;
						SetSlot(index, true);
						m_Keys[index] = key;
						m_Values[index] = val;
						return;
					}
					else if (Key.Equals(key))
					{
						// If the slot is occupied and the keys are equal, write and return (this is the place we need to write)
						m_Keys[index] = key;
						m_Values[index] = val;
						return;
					}
					else
					{
						// if the bucket isn't open nor used by us, check the next one (linear probing)
						index = Mod(index + 1, (int)m_Capacity);
						// if we fail too many times, we should probably rehash.
						// we will do it by recursively calling the function and returning early
						fails++;
						if (fails == ProbeLength)
						{
							ExpandAndRehash();
							Set(key, val);
							return;
						}
					}
				}
			}
		}
		public void DeleteValue(TValue val)
		{
			unsafe
			{
				for (int i = 0; i < m_Capacity; i++)
				{
					if (IsSlotOccupied(i))
					{
						var v = m_Values[i];
						if (v.Equals(val))
						{
							var key = m_Keys[i];
							DeleteKey(key);
						}
					}
				}
			}
			throw new IndexOutOfRangeException("This value was missing from the dictionary!");
		}
		public void DeleteKey(TKey key)
		{
			unsafe
			{
				int hash = Hash(key);

				for (int i = 0; i <= ProbeLength; i++)
				{
					var pos = Mod(hash + i, (int)m_Capacity);
					var localKey = m_Keys[pos];

					if (IsSlotOccupied(pos) == true)
					{
						if (key.Equals(localKey))
						{
							// If we managed to find the key to delete, delete it.
							m_Keys[pos] = default;
							m_Size--;
							int finalIndex = pos;
							// Since we use linear probing, we need to "shift down" all remaining entries in the probe.
							for (int j = i + 1; j <= ProbeLength; j++)
							{
								var curr = Mod(hash + j, (int)m_Capacity);
								var prev = Mod(hash + j - 1, (int)m_Capacity);

								if (IsSlotOccupied(curr) == true)
								{
									if (Hash(m_Keys[curr]) == hash)
									{
										// "shit down"
										m_Keys[prev] = m_Keys[curr];
									}
									else
									{
										finalIndex = prev;
										break;
									}
								}
								else
								{
									finalIndex = prev;
									break;
								}
							}
							SetSlot(finalIndex, false);
							return;
						}
					}
					else
						break;
				}
			}
			throw new IndexOutOfRangeException($"This key ({key}) was missing from the dictionary!");
		}
		public bool TryGetValue(TKey key, out TValue result)
		{
			unsafe
			{
				result = default;
				var hash = Hash(key);

				for (int i = 0; i < ProbeLength; i++)
				{
					var pos = Mod(hash + i, (int)m_Capacity);

					var posKey = m_Keys[pos];
					if (IsSlotOccupied(pos) == true)
					{
						if (posKey.Equals(key))
						{
							result = m_Values[pos];
							return true;
						}
					}
					else
						break;
				}
			}
			return false;
		}
		public TValue this[TKey key]
		{
			get
			{
				if (!TryGetValue(key, out TValue owo)) throw new Exception("KEY IS MISSING FROM THE DICTIONARY!");
				return owo;
			}
			set { Set(key, value); }
		}

		public float LoadFactor { get { return m_Size / (float)Capacity; } }
		public int Capacity { get { return (int)m_Capacity; } }
		public bool IsCreated
		{
			get { unsafe { return (IntPtr)m_Capacity != IntPtr.Zero; } }
		}

		public void Dispose()
		{
			unsafe
			{
				m_Capacity = 0;
				m_Size = 0;
				UnsafeUtility.Free((void*)m_KeyPresentBuffer, Allocator.Persistent);
				UnsafeUtility.Free((void*)m_Keys, Allocator.Persistent);
				UnsafeUtility.Free((void*)m_Capacity, Allocator.Persistent);
			}
		}
		public bool TryGetAtIndex(int index, out TValue result)
		{
			unsafe
			{
				var pos = index;
				if (IsSlotOccupied(pos) == true)
				{
					result = m_Values[pos];
					return true;
				}
				else
				{
					result = default;
					return false;
				}
			}
		}

		public void Debug()
		{
			unsafe
			{
				var s = new System.Text.StringBuilder();

				s.Append(m_Size);
				s.Append("\n{\n");
				for (int i = 0; i < m_Capacity; i++)
				{
					s.Append(IsSlotOccupied(i));
					s.Append("   ");
					s.Append(m_Keys[i]);
					s.Append("   ");
					s.Append(m_Values[i]);
					s.Append("\n");
				}
				s.Append("}");
				UnityEngine.Debug.Log(s.ToString());
			}
		}
	}
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedCollection]
	public struct UnmanagedHashSet<TKey> :
		IDisposable
		where TKey : unmanaged, IEquatable<TKey>
	{
		[NativeDisableUnsafePtrRestriction]
		public unsafe Bitmask* m_KeyPresentBuffer;
		[NativeDisableUnsafePtrRestriction]
		public unsafe TKey* m_Keys;
		private uint m_Size;
		private uint m_Capacity;
		public const int ProbeLength = 20; // how many buckets do we check before we assume that there is no place for the item

		public UnmanagedHashSet(uint defaultCapacity)
		{
			unsafe
			{
				if (defaultCapacity < 1) defaultCapacity = 1;

				m_KeyPresentBuffer = (Bitmask*)UnsafeUtility.Malloc(1 + defaultCapacity / 8, UnsafeUtility.AlignOf<byte>(), Allocator.Persistent);
				UnsafeUtility.MemSet((void*)m_KeyPresentBuffer, 0, 1 + defaultCapacity / 8);
				m_Keys = (TKey*)UnsafeUtility.Malloc(sizeof(TKey) * defaultCapacity, UnsafeUtility.AlignOf<TKey>(), Allocator.Persistent);
				m_Size = 0;
				m_Capacity = defaultCapacity;
			}
		}

		// expands the thing uwu
		private void ExpandAndRehash()
		{
			unsafe
			{
				var oldPresence = m_KeyPresentBuffer;
				var oldKeys = m_Keys;
				
				var oldCapacity = m_Capacity;
				var newCapacity = m_Capacity * 2 + 1;
				m_KeyPresentBuffer = (Bitmask*)UnsafeUtility.Malloc(1 + newCapacity / 8, UnsafeUtility.AlignOf<byte>(), Allocator.Persistent);
				UnsafeUtility.MemSet(m_KeyPresentBuffer, 0, 1 + newCapacity / 8);
				m_Keys = (TKey*)UnsafeUtility.Malloc(sizeof(TKey) * newCapacity, UnsafeUtility.AlignOf<TKey>(), Allocator.Persistent);
				m_Capacity = newCapacity;
				m_Size = 0;
				for (int i = 0; i < oldCapacity; i++)
				{
					if (IsSlotOccupiedOnBuffer(i, oldPresence) == true)
					{
						var k = oldKeys[i];
						Add(k);
					}
				}
				UnsafeUtility.Free((void*)oldPresence, Allocator.Persistent);
				UnsafeUtility.Free((void*)oldKeys, Allocator.Persistent);
			}
		}

		private bool IsSlotOccupied(int slotID)
		{
			unsafe
			{
				return IsSlotOccupiedOnBuffer(slotID, m_KeyPresentBuffer);
			}
		}
		private unsafe bool IsSlotOccupiedOnBuffer(int slotID, Bitmask* buffer)
		{
			var byteID = slotID / 8;
			var dataID = slotID - byteID * 8;
			var data = buffer[byteID];
			return data.Get((byte)dataID);
		}
		private void SetSlot(int slotID, bool value)
		{
			unsafe
			{
				var byteID = slotID / 8;
				var dataID = slotID - byteID * 8;
				var data = m_KeyPresentBuffer[byteID];
				data.Set((byte)dataID, value);
				m_KeyPresentBuffer[byteID] = data;
			}
		}

		public void Clear()
		{
			unsafe
			{
				m_Size = 0;
				for (int i = 0; i < (1 + m_Capacity / 8); i++)
				{
					m_KeyPresentBuffer[i] = new Bitmask() { data = 0 };
				}
			}
		}
		public bool ContainsKey(TKey key)
		{
			unsafe
			{
				int hash = Hash(key);

				for (int i = 0; i <= ProbeLength; i++)
				{
					var pos = Mod(hash + i, (int)m_Capacity);
					var localKey = m_Keys[pos];

					if (IsSlotOccupied(pos) == true)
					{
						if (key.Equals(localKey))
						{
							return true;
						}
					}
					else
						break;
				}
			}
			return false;
		}

		// our own mod cuz '%' is dumb
		private int Mod(int a, int m) => (a % m + m) % m;
		private int Hash(TKey i) => Mod((int)wang_hash((uint)i.GetHashCode()), (int)m_Capacity);
		uint wang_hash(uint seed)
		{
			seed = (seed ^ 61) ^ (seed >> 16);
			seed *= 9;
			seed = seed ^ (seed >> 4);
			seed *= 0x27d4eb2d;
			seed = seed ^ (seed >> 15);
			return seed;
		}
		public void Add(TKey key)
		{
			unsafe
			{
				// check for high load factors -- we can't let it get too high or our linear scheme will get very inefficient :<
				if (LoadFactor > 0.7f) ExpandAndRehash();

				var index = Hash(key);
				int fails = 0;
				while (true)
				{
					var Key = m_Keys[index];
					if (IsSlotOccupied(index) == false)
					{
						// If the slot isn't occupied, we're writing a new value
						m_Size++;
						SetSlot(index, true);
						m_Keys[index] = key;
						return;
					}
					else if (Key.Equals(key))
					{
						// If the slot is occupied and the keys are equal, the value was added in the past. Nothing to be done.
						return;
					}
					else
					{
						// if the bucket isn't open nor used by us, check the next one (linear probing)
						index = Mod(index + 1, (int)m_Capacity);
						// if we fail too many times, we should probably rehash.
						// we will do it by recursively calling the function and returning early
						fails++;
						if (fails == ProbeLength)
						{
							ExpandAndRehash();
							Add(key);
							return;
						}
					}
				}
			}
		}
		public void Remove(TKey key)
		{
			unsafe
			{
				int hash = Hash(key);

				for (int i = 0; i <= ProbeLength; i++)
				{
					var pos = Mod(hash + i, (int)m_Capacity);
					var localKey = m_Keys[pos];

					if (IsSlotOccupied(pos) == true)
					{
						if (key.Equals(localKey))
						{
							// If we managed to find the key to delete, delete it.
							m_Keys[pos] = default;
							m_Size--;
							int finalIndex = pos;
							// Since we use linear probing, we need to "shift down" all remaining entries in the probe.
							for (int j = i + 1; j <= ProbeLength; j++)
							{
								var curr = Mod(hash + j, (int)m_Capacity);
								var prev = Mod(hash + j - 1, (int)m_Capacity);

								if (IsSlotOccupied(curr) == true)
								{
									if (Hash(m_Keys[curr]) == hash)
									{
										// "shit down"
										m_Keys[prev] = m_Keys[curr];
									}
									else
									{
										finalIndex = prev;
										break;
									}
								}
								else
								{
									finalIndex = prev;
									break;
								}
							}
							SetSlot(finalIndex, false);
							return;
						}
					}
					else
						break;
				}
			}
			throw new IndexOutOfRangeException($"This key ({key}) was missing from the dictionary!");
		}

		public bool TryGetAtIndex(int index, out TKey result)
		{
			unsafe
			{
				var pos = index;
				//var posKey = m_Keys[pos];
				if (IsSlotOccupied(pos) == true)
				{
					result = m_Keys[pos];
					return true;
				}
				else
				{
					result = default;
					return false;
				}
			}
		}

		public float LoadFactor { get { return m_Size / (float)Capacity; } }
		public int Capacity { get { return (int)m_Capacity; } }
		public bool IsCreated
		{
			get { unsafe { return (IntPtr)m_Capacity != IntPtr.Zero; } }
		}

		public void Dispose()
		{
			unsafe
			{
				m_Capacity = 0;
				m_Size = 0;
				UnsafeUtility.Free((void*)m_KeyPresentBuffer, Allocator.Persistent);
				UnsafeUtility.Free((void*)m_Keys, Allocator.Persistent);
				UnsafeUtility.Free((void*)m_Capacity, Allocator.Persistent);
			}
		}

		public void Debug()
		{
			unsafe
			{
				var s = new System.Text.StringBuilder();

				s.Append(m_Size);
				s.Append("\n{\n");
				for (int i = 0; i < m_Capacity; i++)
				{
					s.Append(IsSlotOccupied(i));
					s.Append("   ");
					s.Append(m_Keys[i]);
					s.Append("\n");
				}
				s.Append("}");
				UnityEngine.Debug.Log(s.ToString());
			}
		}
	}
	#endregion
}