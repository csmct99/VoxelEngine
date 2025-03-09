public static class VoxelExtensions
{
	#region Public Methods
	public static float[] ExtractChunkVoxels(this float[] voxels, int x, int y, int z, int chunkSize, int voxelDataSize)
	{
		float[] subData = new float[chunkSize * chunkSize * chunkSize];

		for (int subX = 0; subX < chunkSize; subX++)
		{
			for (int subY = 0; subY < chunkSize; subY++)
			{
				for (int subZ = 0; subZ < chunkSize; subZ++)
				{
					subData[voxels.GetVoxelIndex(subX, subY, subZ, chunkSize)] = voxels.GetVoxel(x + subX, y + subY, z + subZ, voxelDataSize);
				}
			}
		}

		return subData;
	}

	public static float GetVoxel(this float[] voxels, int x, int y, int z, int size)
	{
		if (voxels.IsVoxelOutOfBounds(x, y, z, size))
		{
			return 0;
		}

		return voxels[voxels.GetVoxelIndex(x, y, z, size)];
	}

	public static int GetVoxelIndex(this float[] voxels, int x, int y, int z, int size)
	{
		return x + y * size + z * size * size;
	}

	public static bool IsVoxelEmpty(this float[] voxels, int x, int y, int z, int size)
	{
		return !voxels.IsVoxelSolid(x, y, z, size);
	}

	public static bool IsVoxelOutOfBounds(this float[] voxels, int x, int y, int z, int size)
	{
		return x < 0 || y < 0 || z < 0 || x >= size || y >= size || z >= size;
	}

	public static bool IsVoxelSolid(this float[] voxels, int x, int y, int z, int size)
	{
		if (voxels.IsVoxelOutOfBounds(x, y, z, size))
		{
			return false;
		}

		return voxels[voxels.GetVoxelIndex(x, y, z, size)] > 0.5f;
	}

	public static void SetVoxel(this float[] voxels, int x, int y, int z, int size, float value)
	{
		if (voxels.IsVoxelOutOfBounds(x, y, z, size))
		{
			return;
		}

		voxels[voxels.GetVoxelIndex(x, y, z, size)] = value;
	}
	#endregion
}
