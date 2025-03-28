using UnityEngine;

namespace VoxelEngine.DataGenerators
{
	/// <summary>
	/// Represents a simple, manually defined set of voxel data for debugging purposes.
	/// </summary>
	public class SimpleDebugVoxelData : IVoxelDataGenerator
	{
		#region Properties
		public int VoxelDataSetWidth
		{
			get;
			private set;
		}
		#endregion

		#region Public Methods
		public float[] GenerateData()
		{
			VoxelDataSetWidth = 32;
			float[] data = new float[VoxelDataSetWidth * VoxelDataSetWidth * VoxelDataSetWidth];

			AddTestData(ref data, new Vector3Int(0, 0, 0), VoxelDataSetWidth);
			AddTestData(ref data, new Vector3Int(32, 0, 0), VoxelDataSetWidth);
			AddTestData(ref data, new Vector3Int(0, 32, 0), VoxelDataSetWidth);
			AddTestData(ref data, new Vector3Int(32, 0, 32), VoxelDataSetWidth);
			AddTestData(ref data, new Vector3Int(32, 32, 0), VoxelDataSetWidth);
			AddTestData(ref data, new Vector3Int(32, 32, 32), VoxelDataSetWidth);

			return data;
		}
		#endregion

		#region Private Methods
		private void AddTestData(ref float[] data, Vector3Int offset, int boundSize)
		{
			for (int z = 0; z < boundSize; z++)
			{
				for (int y = 0; y < boundSize; y++)
				{
					for (int x = 0; x < boundSize; x++)
					{
						// If the x y or z is over 16, then the voxel has a 50% chance of being solid
						float voxelValue = x + offset.x > 16 || y + offset.y > 16 || z + offset.z > 16 ? Random.value : 0;
						data.SetVoxel(x + offset.x, y + offset.y, z + offset.z, boundSize, voxelValue);
					}
				}
			}
		}
		#endregion
	}
}
