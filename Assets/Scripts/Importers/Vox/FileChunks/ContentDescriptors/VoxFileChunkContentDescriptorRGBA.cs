using UnityEngine;

namespace VoxelEngine.Importers.Vox
{
	public class VoxFileChunkContentDescriptorRGBA : IVoxFileChunkContentDescriptor
	{
		#region Public Fields
		public Vector4[] Palette;
		#endregion

		#region Public Methods
		public void ReadData(byte[] data)
		{
			ReadPaletteData(data);
		}

		/*
		7. Chunk id 'RGBA' : palette
	   -------------------------------------------------------------------------------
	   # Bytes  | Type       | Value
	   -------------------------------------------------------------------------------
	   4 x 256  | int        | (R, G, B, A) : 1 byte for each component
							 | * NOTICE
							 | * color [0-254] are mapped to palette index [1-255], e.g :
							 |
							 | for ( int i = 0; i <= 254; i++ ) {
							 |     palette[i + 1] = ReadRGBA();
							 | }
	   -------------------------------------------------------------------------------
		*/

		public void ReadPaletteData(byte[] rgbaData)
		{
			Palette = new Vector4[256];

			for (int i = 0; i < rgbaData.Length; i += 4)
			{
				Palette[i / 4] = new Vector4(rgbaData[i], rgbaData[i + 1], rgbaData[i + 2], rgbaData[i + 3]);
			}
		}
		#endregion
	}
}
