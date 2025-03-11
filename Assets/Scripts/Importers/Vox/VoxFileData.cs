using VoxelEngine.Importers.Vox;

namespace Importers.Vox
{
	public class VoxFileData
	{
		#region Public Fields
		public VoxFileChunkContentDescriptorSize size;
		public VoxFileChunkContentDescriptorPack pack;
		public VoxFileChunkContentDescriptorRGBA rgba;
		public VoxFileChunkContentDescriptorXYZI xyzi;
		#endregion

		public VoxFileData(string path)
		{
			VoxFileReader reader = new();

			string headerCode,
				   version;
			VoxFileChunk mainChunk;

			reader.ReadVoxFile(path, out headerCode, out version, out mainChunk);

			if (headerCode != "VOX ")
			{
				throw new System.Exception($"Invalid header code: '{headerCode}'. Expected: 'VOX '. Is this file a valid .vox file?");
			}

			foreach (VoxFileChunk mainChunkChild in mainChunk.Children)
			{
				switch (mainChunkChild.ChunkID)
				{
					case "SIZE":
						size = mainChunkChild.ContentDescriptor as VoxFileChunkContentDescriptorSize;
						break;

					case "PACK":
						pack = mainChunkChild.ContentDescriptor as VoxFileChunkContentDescriptorPack;
						break;

					case "RGBA":
						rgba = mainChunkChild.ContentDescriptor as VoxFileChunkContentDescriptorRGBA;
						break;

					case "XYZI":
						xyzi = mainChunkChild.ContentDescriptor as VoxFileChunkContentDescriptorXYZI;
						break;

					default:
						break;
				}
			}
		}
	}
}
