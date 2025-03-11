using UnityEngine;

namespace VoxelEngine.Importers.Vox
{
	public class VoxFileChunkContentDescriptorXYZI : IVoxFileChunkContentDescriptor
	{
		#region Public Fields
		/// <summary>
		/// Number of voxels in the model
		/// </summary>
		public int NumberOfVoxels;

		/// <summary>
		/// X, Y, Z, ColorIndex
		/// </summary>
		public Vector4[] VoxelData;
		#endregion

		#region Public Methods
		public bool[] FlattenData(int voxelSpaceWidth)
		{
			bool invalidData = VoxelData == null || VoxelData.Length == 0;
			if (invalidData)
			{
				return null;
			}

			bool[] flatData = new bool[voxelSpaceWidth * voxelSpaceWidth * voxelSpaceWidth];

			for (int i = 0; i < VoxelData.Length; i++)
			{
				Vector4 voxel = VoxelData[i];
				int flatIndex = (int) voxel.x + (int) voxel.y * voxelSpaceWidth + (int) voxel.z * voxelSpaceWidth * voxelSpaceWidth;
				flatData[flatIndex] = true;
			}

			return flatData;
		}

		/*
			6. Chunk id 'XYZI' : model voxels, paired with the SIZE chunk
		   -------------------------------------------------------------------------------
		   # Bytes  | Type       | Value
		   -------------------------------------------------------------------------------
		   4        | int        | numVoxels (N)
		   4 x N    | int        | (x, y, z, colorIndex) : 1 byte for each component
		   -------------------------------------------------------------------------------
		 */

		public void ReadData(byte[] data)
		{
			//First 4 bytes are the number of voxels as an int
			NumberOfVoxels = System.BitConverter.ToInt32(data, 0);

			VoxelData = new Vector4[NumberOfVoxels];

			for (int i = 0; i < NumberOfVoxels; i++)
			{
				int offset = 4 + i * 4;
				// We swap the Y and Z values here because Unity uses a different coordinate system
				VoxelData[i] = new Vector4(data[offset], data[offset + 2], data[offset + 1], data[offset + 3]);
			}
		}
		#endregion
	}
}
