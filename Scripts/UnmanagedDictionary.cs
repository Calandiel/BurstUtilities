using System;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Calandiel.Collections
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedCollection]
	public struct UnmanagedDictionary<TKey, TValue> :
	IDisposable, IUnmanagedCollectionSave
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
					UnsafeUtility.Free((void*)oldPresence, Allocator.Persistent);
					UnsafeUtility.Free((void*)oldKeys, Allocator.Persistent);
					UnsafeUtility.Free((void*)oldValues, Allocator.Persistent);
				}
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
				if (Capacity == 0) ExpandAndRehash();
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
				if (Capacity == 0) { result = default; return false; }

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
					s.Append(IsSlotOccupied(i));
					s.Append("   ");
					s.Append(m_Keys[i].ToString());
					s.Append("   ");
					s.Append(m_Values[i].ToString());
					s.Append("\n");
				}
				s.Append("}");
				UnityEngine.Debug.Log(s.ToString());
			}
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write((int)this.m_Size);
			for (int i = 0; i < this.Capacity; i++)
			{
				if (this.TryGetAtIndex(i, out TKey key, out TValue val))
				{
					// key
					Utilities.SavingUtility.Save(key, writer);
					// value
					Utilities.SavingUtility.Save(val, writer);
				}
			}
		}
		public void Load(BinaryReader reader)
		{
			var len = reader.ReadInt32();
			this = new UnmanagedDictionary<TKey, TValue>((uint)len);
			for (int i = 0; i < len; i++)
			{
				object key = default(TKey);
				Utilities.SavingUtility.Load(ref key, reader);
				object value = default(TValue);
				Utilities.SavingUtility.Load(ref value, reader);
				this[(TKey)key] = (TValue)value;
			}
		}
	}
}