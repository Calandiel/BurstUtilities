namespace Calandiel.Collections
{
	public struct Array3D<T> where T : unmanaged
	{
		private UnmanagedArray<T> data;

		private int x;
		private int y;
		private int z;

		public int LengthX { get => x; }
		public int LengthY { get => y; }
		public int LengthZ { get => z; }

		public Array3D(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			data = new UnmanagedArray<T>((uint)(x * y * z));
		}

		public void Dispose()
		{
			x = 0;
			y = 0;
			z = 0;
			data.Dispose();
		}

		public T this[int x, int y, int z]
		{
			get => data[x + z * (int)this.x + y * (int)this.x * (int)this.z];
			set => data[x + z * (int)this.x + y * (int)this.x * (int)this.z] = value;
		}
	}
}