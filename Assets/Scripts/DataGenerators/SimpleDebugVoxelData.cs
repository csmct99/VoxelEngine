namespace VoxelEngine.DataGenerators
{
	/// <summary>
	/// Represents a simple, manually defined set of voxel data for debugging purposes.
	/// </summary>
	public class SimpleDebugVoxelData : IVoxelData
	{
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
			bool outOfBounds = x < 0 || y < 0 || z < 0 || x >= Size || y >= Size || z >= Size;
			if (outOfBounds)
			{
				return true;
			}

			return Data[x, y, z] == 0;
		}

		public bool IsSolid(int x, int y, int z)
		{
			return !IsEmpty(x, y, z);
		}
	}
}
