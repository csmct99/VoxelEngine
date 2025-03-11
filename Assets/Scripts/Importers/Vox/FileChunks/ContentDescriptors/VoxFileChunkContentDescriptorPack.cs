using System;

namespace VoxelEngine.Importers.Vox
{
	public class VoxFileChunkContentDescriptorPack : IVoxFileChunkContentDescriptor
	{
		#region Public Fields
		public int NumberOfModels;
		#endregion

		#region Public Methods
		/*
		   4. Chunk id 'PACK' : if it is absent, only one model in the file; only used for the animation in 0.98.2
		   -------------------------------------------------------------------------------
		   # Bytes  | Type       | Value
		   -------------------------------------------------------------------------------
		   4        | int        | numModels : num of SIZE and XYZI chunks
		   -------------------------------------------------------------------------------
		 */
		public void ReadData(byte[] data)
		{
			NumberOfModels = BitConverter.ToInt32(data, 0);
		}
		#endregion
	}
}
