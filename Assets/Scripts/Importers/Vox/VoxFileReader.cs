using System;
using System.IO;

namespace VoxelEngine.Importers.Vox
{
	/// <summary>
	/// https://paulbourke.net/dataformats/vox/
	/// </summary>
	public class VoxFileReader
	{
		#region Structs
		private struct FileHeader
		{
			public string Header;
			public int Version;

			public FileHeader(Stream stream)
			{
				byte[] header = new byte[4];
				stream.Read(header, 0, 4);
				Header = System.Text.Encoding.UTF8.GetString(header);

				byte[] version = new byte[4];
				stream.Read(version, 0, 4);
				Version = BitConverter.ToInt32(version, 0);
			}
		}
		#endregion

		#region Public Methods
		public void ReadVoxFile(string path, out string headerCode, out string version, out VoxFileChunk mainChunk)
		{
			mainChunk = null;
			headerCode = null;
			version = null;

			bool valid = IsPathValid(path);
			if (!valid)
			{
				throw new ArgumentException("Invalid path: " + path);
			}

			Stream stream = null;

			try
			{
				// Open the file and read it as binary;
				stream = new FileStream(path, FileMode.Open);

				// Read the header
				FileHeader header = new(stream);
				version = header.Version.ToString();
				headerCode = header.Header;

				// We know the rest of the file is made up of chunks, there is one main chunk that contains all the other chunks
				mainChunk = new VoxFileChunk(stream, 8);
			}
			finally
			{
				if (stream != null)
				{
					stream.Close();
					stream.Dispose();
				}
			}
		}
		#endregion

		#region Private Methods
		private bool IsPathValid(string path)
		{
			bool endsInVox = path.EndsWith(".vox");
			bool exists = File.Exists(path);

			return endsInVox && exists;
		}
		#endregion
	}
}
