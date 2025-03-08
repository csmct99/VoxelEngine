using System;

namespace VoxelEngine.DataGenerators
{
	public class RandomVoxelDataGenerator : IVoxelDataGenerator
	{
		public float[] GenerateData(int boundSize)
		{
			float[] data = new float[boundSize * boundSize * boundSize];
			
			for (int x = 0; x < boundSize; x++)
			{
				for (int y = 0; y < boundSize; y++)
				{
					for (int z = 0; z < boundSize; z++)
					{
						int index = data.GetVoxelIndex(x, y, z, boundSize);
						data[index] = x % 10 == 0 || y % 10 == 0 || z % 10 == 0 ? 1 : 0;
					}
				}
			}
			
			return data;
		}
	}
}
