namespace Calandiel.Collections
{
	public struct Array2D<T> where T : unmanaged
	{
		private UnmanagedArray<T> data;

		private int x;
		private int y;

		public int LengthX { get => x; }
		public int LengthY { get => y; }

		public Array2D(int x, int y)
		{
			this.x = x;
			this.y = y;
			data = new UnmanagedArray<T>((uint)(x * y));
		}

		public void Dispose()
		{
			x = 0;
			y = 0;
			data.Dispose();
		}

		public T this[int x, int y]
		{
			get => data[x + y * (int)this.x];
			set => data[x + y * (int)this.x] = value;
		}
	}
}