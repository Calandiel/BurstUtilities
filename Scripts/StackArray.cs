using Unity.Collections.LowLevel.Unsafe;

namespace Calandiel.Collections
{
	struct StackArray8<T> where T : unmanaged
	{
		private unsafe fixed byte data[bufferSize];
		private const int bufferSize = 8;
		public int Length { get => UnsafeUtility.SizeOf<T>() / bufferSize; }

		public T this[int i]
		{
			get
			{
				unsafe
				{
					fixed (byte* ptr = data)
					{
						var T_ptr = (T*)ptr;
						return T_ptr[i];
					}
				}
			}
			set
			{
				unsafe
				{
					fixed (byte* ptr = data)
					{
						var T_ptr = (T*)ptr;
						T_ptr[i] = value;
					}
				}
			}
		}
	}
	struct StackArray16<T> where T : unmanaged
	{
		private unsafe fixed byte data[bufferSize];
		private const int bufferSize = 16;
		public int Length { get => UnsafeUtility.SizeOf<T>() / bufferSize; }

		public T this[int i]
		{
			get
			{
				unsafe
				{
					fixed (byte* ptr = data)
					{
						var T_ptr = (T*)ptr;
						return T_ptr[i];
					}
				}
			}
			set
			{
				unsafe
				{
					fixed (byte* ptr = data)
					{
						var T_ptr = (T*)ptr;
						T_ptr[i] = value;
					}
				}
			}
		}
	}
	struct StackArray32<T> where T : unmanaged
	{
		private unsafe fixed byte data[bufferSize];
		private const int bufferSize = 32;
		public int Length { get => UnsafeUtility.SizeOf<T>() / bufferSize; }

		public T this[int i]
		{
			get
			{
				unsafe
				{
					fixed (byte* ptr = data)
					{
						var T_ptr = (T*)ptr;
						return T_ptr[i];
					}
				}
			}
			set
			{
				unsafe
				{
					fixed (byte* ptr = data)
					{
						var T_ptr = (T*)ptr;
						T_ptr[i] = value;
					}
				}
			}
		}
	}
	struct StackArray64<T> where T : unmanaged
	{
		private unsafe fixed byte data[bufferSize];
		private const int bufferSize = 64;
		public int Length { get => UnsafeUtility.SizeOf<T>() / bufferSize; }

		public T this[int i]
		{
			get
			{
				unsafe
				{
					fixed (byte* ptr = data)
					{
						var T_ptr = (T*)ptr;
						return T_ptr[i];
					}
				}
			}
			set
			{
				unsafe
				{
					fixed (byte* ptr = data)
					{
						var T_ptr = (T*)ptr;
						T_ptr[i] = value;
					}
				}
			}
		}
	}
	struct StackArray128<T> where T : unmanaged
	{
		private unsafe fixed byte data[bufferSize];
		private const int bufferSize = 128;
		public int Length { get => UnsafeUtility.SizeOf<T>() / bufferSize; }

		public T this[int i]
		{
			get
			{
				unsafe
				{
					fixed (byte* ptr = data)
					{
						var T_ptr = (T*)ptr;
						return T_ptr[i];
					}
				}
			}
			set
			{
				unsafe
				{
					fixed (byte* ptr = data)
					{
						var T_ptr = (T*)ptr;
						T_ptr[i] = value;
					}
				}
			}
		}
	}
	struct StackArray256<T> where T : unmanaged
	{
		private unsafe fixed byte data[bufferSize];
		private const int bufferSize = 256;
		public int Length { get => UnsafeUtility.SizeOf<T>() / bufferSize; }

		public T this[int i]
		{
			get
			{
				unsafe
				{
					fixed (byte* ptr = data)
					{
						var T_ptr = (T*)ptr;
						return T_ptr[i];
					}
				}
			}
			set
			{
				unsafe
				{
					fixed (byte* ptr = data)
					{
						var T_ptr = (T*)ptr;
						T_ptr[i] = value;
					}
				}
			}
		}
	}
	struct StackArray512<T> where T : unmanaged
	{
		private unsafe fixed byte data[bufferSize];
		private const int bufferSize = 512;
		public int Length { get => UnsafeUtility.SizeOf<T>() / bufferSize; }

		public T this[int i]
		{
			get
			{
				unsafe
				{
					fixed (byte* ptr = data)
					{
						var T_ptr = (T*)ptr;
						return T_ptr[i];
					}
				}
			}
			set
			{
				unsafe
				{
					fixed (byte* ptr = data)
					{
						var T_ptr = (T*)ptr;
						T_ptr[i] = value;
					}
				}
			}
		}
	}
	struct StackArray1024<T> where T : unmanaged
	{
		private unsafe fixed byte data[bufferSize];
		private const int bufferSize = 1024;
		public int Length { get => UnsafeUtility.SizeOf<T>() / bufferSize; }

		public T this[int i]
		{
			get
			{
				unsafe
				{
					fixed (byte* ptr = data)
					{
						var T_ptr = (T*)ptr;
						return T_ptr[i];
					}
				}
			}
			set
			{
				unsafe
				{
					fixed (byte* ptr = data)
					{
						var T_ptr = (T*)ptr;
						T_ptr[i] = value;
					}
				}
			}
		}
	}
}