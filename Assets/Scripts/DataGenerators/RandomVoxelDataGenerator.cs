namespace VoxelEngine.DataGenerators
{
	public class RandomVoxelDataGenerator : IVoxelDataGenerator
	{
		#region Properties
		public int VoxelDataSetWidth
		{
			get;
			set;
		}
		#endregion

		#region Public Methods
		public float[] GenerateData()
		{
			float[] data = new float[VoxelDataSetWidth * VoxelDataSetWidth * VoxelDataSetWidth];

			for (int x = 0; x < VoxelDataSetWidth; x++)
			{
				for (int y = 0; y < VoxelDataSetWidth; y++)
				{
					for (int z = 0; z < VoxelDataSetWidth; z++)
					{
						int index = data.GetVoxelIndex(x, y, z, VoxelDataSetWidth);
						data[index] = x % 10 == 0 || y % 10 == 0 || z % 10 == 0 ? 1 : 0;
					}
				}
			}

			return data;
		}
		#endregion
	}
}
