namespace VoxelEngine.DataGenerators
{
	/// <summary>
	/// Represents a simple, manually defined set of voxel data for debugging purposes.
	/// </summary>
	public class SimpleDebugVoxelData : IVoxelDataGenerator
	{
		#region Public Methods
		public float[] GenerateData(int boundSize)
		{
			float[] data = new float[boundSize * boundSize * boundSize];

			data.SetVoxel(0, 0, 0, boundSize, 1);

			return data;
		}
		#endregion
	}
}
