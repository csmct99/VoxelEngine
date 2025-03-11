using UnityEngine;

namespace VoxelEngine.Importers.Vox
{
	public class VoxFileChunkContentDescriptorSize : IVoxFileChunkContentDescriptor
	{
		#region Public Fields
		public Vector3Int ModelSize;
		#endregion

		#region Public Methods
		/*
		 5. Chunk id 'SIZE' : model size
		   -------------------------------------------------------------------------------
		   # Bytes  | Type       | Value
		   -------------------------------------------------------------------------------
		   4        | int        | size x
		   4        | int        | size y
		   4        | int        | size z : gravity direction
		   -------------------------------------------------------------------------------
		 */
		public void ReadData(byte[] data)
		{
			ModelSize = new Vector3Int((int) System.BitConverter.ToInt32(data, 0), (int) System.BitConverter.ToInt32(data, 4), (int) System.BitConverter.ToInt32(data, 8));
		}
		#endregion
	}
}
