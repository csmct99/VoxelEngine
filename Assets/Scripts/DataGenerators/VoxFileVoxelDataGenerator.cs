using Importers.Vox;

using UnityEngine;

namespace VoxelEngine.DataGenerators
{
	public class VoxFileVoxelDataGenerator : IVoxelDataGenerator
	{
		#region Public Fields
		public string Path;
		#endregion

		#region Private Fields
		private VoxFileData _voxFileData;

		private Vector3Int _voxelBounds;
		#endregion

		#region Properties
		public Vector3Int VoxelBounds
		{
			get
			{
				return _voxelBounds;
			}
			set
			{
				_voxelBounds = value;
			}
		}

		public int LargestDimension
		{
			get
			{
				return Mathf.Max(_voxelBounds.x, _voxelBounds.y, _voxelBounds.z);
			}
		}
		#endregion

		#region Public Methods
		public float[] GenerateData(int boundSize)
		{
			boundSize = LargestDimension; // Since this is an imported voxel data, we really shouldnt change the size of the voxel data
			ReadFile();

			bool[] voxels = _voxFileData.xyzi.FlattenData(LargestDimension);

			//Convert bool[] to float[] //TODO: This is pretty icky, when the voxel data is more evolved, this can be abstracted better
			float[] data = new float[boundSize * boundSize * boundSize];
			for (int i = 0; i < voxels.Length; i++)
			{
				data[i] = voxels[i] ? 1 : 0;
			}

			return data;
		}

		public void ReadFile()
		{
			if (_voxFileData == null)
			{
				_voxFileData = new VoxFileData(Path);
				_voxelBounds = _voxFileData.size.ModelSize;
			}
		}
		#endregion
	}
}
