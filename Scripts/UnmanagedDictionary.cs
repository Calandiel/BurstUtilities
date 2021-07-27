using System;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Calandiel.Internal;

namespace Calandiel.Collections
{
	[StructLayout(LayoutKind.Sequential)]
	public struct UnmanagedDictionary<TKey, TValue> :
	IDisposable
	where TValue : unmanaged//, IEquatable<TValue>
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
				//UnityEngine.Debug.Log($"EXPAND AND REHASH FROM: {m_Capacity}, INTO: {2 * m_Capacity + 3}");
				var oldPresence = m_KeyPresentBuffer;
				var oldKeys = m_Keys;
				var oldValues = m_Values;

				var oldCapacity = m_Capacity;
				var newCapacity = m_Capacity * 2 + 3;

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

				if (oldCapacity > 0)
				{
					//UnityEngine.Debug.Log($"Reshash clear: {((IntPtr)oldPresence).ToInt64()} {((IntPtr)oldKeys).ToInt64()} {((IntPtr)oldValues).ToInt64()}");
					UnsafeUtility.Free((void*)oldPresence, Allocator.Persistent);
					UnsafeUtility.Free((void*)oldKeys, Allocator.Persistent);
					UnsafeUtility.Free((void*)oldValues, Allocator.Persistent);
				}
			}
		}

		public void Clear()
		{
			unsafe
			{
				m_Size = 0;
				if (m_Capacity > 0)
				{
					for (int i = 0; i < (1 + m_Capacity / 8); i++)
					{
						m_KeyPresentBuffer[i] = new Bitmask() { data = 0 };
					}
				}
			}
		}
		public bool ContainsKey(TKey key)
		{
			unsafe
			{
				if (m_Capacity > 0)
				{
					int hash = Hash(key);
					int index = 0;
					while (true)
					{
						var pos = hash + index;
						if (pos >= m_Capacity) return false;

						if (IsSlotOccupied(pos) == true)
						{
							var localKey = m_Keys[pos];
							if (key.Equals(localKey))
							{
								return true;
							}
						}
						else
							return false;
						index++;
					}
				}
				else
					return false;
			}
		}
		public void Set(TKey key, TValue val)
		{
			unsafe
			{
				if (Capacity == 0) ExpandAndRehash();
				// check for high load factors -- we can't let it get too high or our linear scheme will get very inefficient :<
				if (LoadFactor > 0.65f) ExpandAndRehash();

				var index = Hash(key);
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
						// If the bucket isn't open nor used by us, check the next one (linear probing)
						index++;
						if (index >= m_Capacity)
						{
							ExpandAndRehash();
							Set(key, val);
							return;
						}
					}
				}
			}
		}
		public void DeleteKey(TKey key)
		{
			unsafe
			{
				int hash = Hash(key);

				int offset = 0;
				while (true)
				{
					var pos = hash + offset;
					if(pos >= m_Capacity) throw new Exception("CAN'T DELETE: KEY IS MISSING FROM THE DICTIONARY!");

					var localKey = m_Keys[pos];
					if (IsSlotOccupied(pos) == true)
					{
						if (key.Equals(localKey))
						{
							// If we managed to find the key to delete, delete it.
							m_Keys[pos] = default;
							m_Size--;
							int slotToEmpty = pos; // the index we want to clear at the very end

							// Since we use linear probing, we need to "shift down" all remaining entries in the probe.
							int j = 1; // offset
							while (true)
							{
								var next = hash + j;
								if (next >= m_Capacity)
								{
									SetSlot(slotToEmpty, false);
									return;
								}

								if(IsSlotOccupied(next))
								{
									var nextKey = m_Keys[next];

									var desiredPosition = Hash(nextKey);
									if(desiredPosition <= slotToEmpty)
									{
										m_Keys[slotToEmpty] = nextKey;
										m_Values[slotToEmpty] = m_Values[next];
										slotToEmpty = next;
									}
								}
								else
								{
									SetSlot(slotToEmpty, false);
									return;
								}
								j++;
							}
						}
						else
						{
							// The key is not present on this slot, keep probing further
							offset++;
						}
					}
					else
					{
						throw new Exception("CAN'T DELETE: KEY IS MISSING FROM THE DICTIONARY!");
					}
				}
			}
		}
		public bool TryGetValue(TKey key, out TValue result)
		{
			unsafe
			{
				TValue* ptr;
				if(TryGetPtr(key, out ptr))
				{
					result = *ptr;
					return true;
				}
				else
				{
					result = default;
					return false;
				}
			}
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
		public unsafe bool TryGetPtr(TKey key, out TValue* ptr)
		{
			unsafe
			{
				if (Capacity == 0) { ptr = default; return false; }

				ptr = default;
				var hash = Hash(key);

				int i = 0;
				while (true)
				{
					var pos = hash + i;
					if (pos >= m_Capacity) return false;

					if (IsSlotOccupied(pos) == true)
					{
						var posKey = m_Keys[pos];
						if (posKey.Equals(key))
						{
							ptr = m_Values + pos;
							return true;
						}
					}
					else
						return false;
					i++;
				}
			}
		}
		public TValue GetOrDefault(TKey key)
		{
			if (!TryGetValue(key, out TValue owo))
				return default;
			else
				return owo;
		}

		public float LoadFactor { get { return m_Size / (float)Capacity; } }
		public int Capacity { get { return (int)m_Capacity; } }
		public int Size { get { return (int)m_Size; } }
		public bool IsCreated
		{
			get { unsafe { return (IntPtr)m_Capacity != IntPtr.Zero; } }
		}

		public void Dispose()
		{
			unsafe
			{
				if (m_Capacity > 0)
				{
					UnsafeUtility.Free((void*)m_KeyPresentBuffer, Allocator.Persistent);
					UnsafeUtility.Free((void*)m_Keys, Allocator.Persistent);
					UnsafeUtility.Free((void*)m_Values, Allocator.Persistent);
					m_Capacity = 0;
					m_Size = 0;
				}
			}
		}
		public bool TryGetAtIndex(int index, out TKey key, out TValue value)
		{
			unsafe
			{
				var pos = index;
				if (IsSlotOccupied(pos) == true)
				{
					key = m_Keys[pos];
					value = m_Values[pos];
					return true;
				}
				else
				{
					key = default;
					value = default;
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
					s.Append(i);
					s.Append(":   ");
					s.Append(IsSlotOccupied(i));
					s.Append("   ");
					s.Append(m_Keys[i].ToString());
					s.Append("   ");
					s.Append(m_Values[i].ToString());
					s.Append(" (");
					s.Append(Hash(m_Keys[i]));
					s.Append(")");
					s.Append("\n");
				}
				s.Append("}");
				UnityEngine.Debug.Log(s.ToString());
			}
		}
		public void QuickDebug()
		{
			unsafe
			{
				var s = new System.Text.StringBuilder();

				s.Append("{ ");
				for (int i = 0; i < m_Capacity; i++)
				{
					if (IsSlotOccupied(i))
					{
						s.Append(m_Keys[i].ToString());
						s.Append(" -- ");
						s.Append(m_Values[i].ToString());
						s.Append(", ");
					}
				}
				s.Append(" }");
				UnityEngine.Debug.Log(s.ToString());
			}
		}

		#region SLOT
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
		#endregion

		#region HASH
		// our own mod cuz '%' is dumb
		private int HashMod(int a, int m) => (a % m + m) % m;
		private int Hash(TKey i)
		{
			return HashMod((int)Internal.Hash.pcg_hash((uint)i.GetHashCode()), (int)m_Capacity);
		}
		#endregion}

	}
}