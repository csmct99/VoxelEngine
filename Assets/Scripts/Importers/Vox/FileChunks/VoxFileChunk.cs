using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace VoxelEngine.Importers.Vox
{
	public class VoxFileChunk
	{
		#region Public Fields
		public string ChunkID;
		public int ChunkContentLength;
		public int ChunkChildrenLength;

		public byte[] ChunkContent;

		public bool IsChunkExcluded;

		public IVoxFileChunkContentDescriptor ContentDescriptor;

		public List<VoxFileChunk> Children;
		#endregion

		#region Private Fields
		private static Dictionary<string, bool> s_includedChunks = new()
		{
			{
				"MAIN", true
			}, // The top level chunk, the parent chunk.
			{
				"PACK", true
			}, // The number of models in the file.
			{
				"SIZE", true
			}, // The dimensions of the XYZI data that follows.
			{
				"XYZI", true
			}, // This contains actual voxel data.
			{
				"RGBA", true
			}, // A colour palette.
			{
				"NOTE", true
			}, // Note added by the model author.

			//Exlcude
			{
				"rCAM", false
			}, // Virtual camera information?
			{
				"rOBJ", false
			}, // Object information?
			{
				"IMAP", false
			}, // Binary data. Seems like it could be colour/palette remapping
			{
				"nTRN", false
			}, // Transformation of XYZI chunk.
			{
				"nGRP", false
			}, // Grouping information?
			{
				"nSHP", false
			}, // Shape information?
			{
				"LAYR", false
			}, // Layer names and/or information
			{
				"MATL", false
			}, // Material properties
			{
				"MATT", false
			} // Deprecated material properties
		};
		#endregion

		#region Private Methods
		private void AssignChunkContentDescriptor()
		{
			switch (ChunkID)
			{
				case "MAIN":
					ContentDescriptor = new VoxFileChunkContentDescriptorGeneric();
					break;

				case "PACK":
					ContentDescriptor = new VoxFileChunkContentDescriptorPack();
					break;

				case "SIZE":
					ContentDescriptor = new VoxFileChunkContentDescriptorSize();
					break;

				case "XYZI":
					ContentDescriptor = new VoxFileChunkContentDescriptorXYZI();
					break;

				case "RGBA":
					ContentDescriptor = new VoxFileChunkContentDescriptorRGBA();
					break;

				case "NOTE":
					ContentDescriptor = new VoxFileChunkContentDescriptorGeneric();
					break;

				default:
					Debug.LogWarning("Unhandled content descriptor for chunk: " + ChunkID);
					ContentDescriptor = new VoxFileChunkContentDescriptorGeneric();
					break;
			}

			ContentDescriptor.ReadData(ChunkContent);
		}
		#endregion

		public VoxFileChunk(Stream stream, long readPosition = 0)
		{
			stream.Seek(readPosition, SeekOrigin.Begin);
			byte[] chunkID = new byte[4];
			stream.Read(chunkID, 0, 4);
			ChunkID = System.Text.Encoding.UTF8.GetString(chunkID);

			byte[] contentLength = new byte[4];
			stream.Read(contentLength, 0, 4);
			ChunkContentLength = BitConverter.ToInt32(contentLength, 0);

			byte[] childrenLength = new byte[4];
			stream.Read(childrenLength, 0, 4);
			ChunkChildrenLength = BitConverter.ToInt32(childrenLength, 0);

			// Check if this chunk of data is on the ignore list, if so dont bother continuing.
			bool hasKey = s_includedChunks.ContainsKey(ChunkID);
			bool shouldKeepChunk = true; //if its not in the exlcude list, well keep the chunk.
			if (hasKey)
			{
				shouldKeepChunk = s_includedChunks[ChunkID];
			}

			IsChunkExcluded = !shouldKeepChunk;

			// If we dont want this chunk, stop reading and skip the stream reader
			if (IsChunkExcluded)
			{
				ChunkContent = null;
				Children = null;

				//Move the stream reader as if we had read the chunk content.
				int offset = ChunkChildrenLength + ChunkContentLength;
				stream.Seek(offset, SeekOrigin.Current); // Move to the next chunk start position.
			}

			// Binary streams
			ChunkContent = new byte[ChunkContentLength];
			stream.Read(ChunkContent, 0, ChunkContentLength);

			Children = new List<VoxFileChunk>();
			int startingPoint = (int) stream.Position;
			while ((int) stream.Position - startingPoint < ChunkChildrenLength)
			{
				int currentPosition = (int) stream.Position;
				VoxFileChunk child = new(stream, currentPosition);

				if (!child.IsChunkExcluded)
				{
					Children.Add(child);
				}
			}

			AssignChunkContentDescriptor();
		}
	}
}
