namespace VoxelEngine.DataGenerators
{
	/// <summary>
	/// Represents a simple, manually defined set of voxel data for debugging purposes.
	/// </summary>
	public class SimpleDebugVoxelData : IVoxelData
	{
		private IVoxelData _voxelDataImplementation;

		public int Size
		{
			get;
			set;
		}

		public int ChunkSize
		{
			get;
			set;
		}

		public float[,,] Data
		{
			get;
			set;
		}

		public IVoxelData Clone()
		{
			SimpleDebugVoxelData clonedData = new();
			clonedData.Size = Size;
			clonedData.ChunkSize = ChunkSize;
			clonedData.Data = (float[,,]) Data.Clone();

			return clonedData;
		}

		public bool IsValid
		{
			get
			{
				return Data != null;
			}
		}

		public void GenerateData()
		{
			ChunkSize = 4; // We hardcode this for simplicity
			Size = 4;

			//Manually create some data
			Data = new float[4, 4, 4]
			{
				{
					{ 0, 0, 0, 0 },
					{ 0, 0, 0, 0 },
					{ 1, 1, 1, 1 },
					{ 0, 0, 0, 0 }
				},
				{
					{ 0, 0, 0, 0 },
					{ 0, 0, 0, 0 },
					{ 1, 1, 1, 1 },
					{ 0, 0, 0, 0 }
				},
				{
					{ 0, 0, 0, 0 },
					{ 0, 0, 0, 0 },
					{ 1, 1, 1, 1 },
					{ 0, 0, 0, 0 }
				},
				{
					{ 0, 0, 0, 0 },
					{ 0, 0, 0, 0 },
					{ 1, 1, 1, 1 },
					{ 0, 0, 0, 0 }
				}
			};
		}

		public float GetValue(int x, int y, int z)
		{
			return Data[x, y, z];
		}

		public bool IsEmpty(int x, int y, int z)
		{
			if (IsOutOfBounds(x, y, z))
			{
				return true;
			}

			return Data[x, y, z] == 0;
		}

		public bool IsSolid(int x, int y, int z)
		{
			return !IsEmpty(x, y, z);
		}
		
		public bool IsOutOfBounds(int x, int y, int z)
		{
			return x < 0 || x >= Size || y < 0 || y >= Size || z < 0 || z >= Size;
		}

		public IVoxelData GetSubData(int x, int y, int z, int size)
		{
			SimpleDebugVoxelData subData = new();
			subData.Size = size;
			subData.ChunkSize = ChunkSize;
			subData.Data = new float[size, size, size];

			for (int i = 0; i < size; i++)
			{
				for (int j = 0; j < size; j++)
				{
					for (int k = 0; k < size; k++)
					{
						subData.Data[i, j, k] = Data[x + i, y + j, z + k];
					}
				}
			}

			return subData;
		}
	}
}
