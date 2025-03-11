using UnityEngine;

namespace VoxelEngine.DataGenerators
{
	/// <summary>
	/// A simple (and flawed) implementation of a 3D noise generator using Perlin noise. Primarily used for debugging and
	/// stress testing purposes.
	/// <br/><br/>Credit to <a href="https://gist.github.com/tntmeijs/6a3b4587ff7d38a6fa63e13f9d0ac46d">github.com/tntmeijs</a>
	/// </summary>
	public class Simple3DNoiseVoxelDataGenerator : IVoxelDataGenerator
	{
		#region Properties
		public int VoxelDataSetWidth
		{
			get;
			set;
		}
		#endregion

		#region Public Methods
		public float[] GenerateData()
		{
			// Generate Perlin noise data
			float[] data = new float[VoxelDataSetWidth * VoxelDataSetWidth * VoxelDataSetWidth];

			// Go through each position in the 3D array
			for (int x = 0; x < VoxelDataSetWidth; x++)
			{
				for (int y = 0; y < VoxelDataSetWidth; y++)
				{
					for (int z = 0; z < VoxelDataSetWidth; z++)
					{
						int index = data.GetVoxelIndex(x, y, z, VoxelDataSetWidth);
						data[index] = SimplexNoise3D(x, y, z, 0.03f, 1.0f, 0.5f, 1, 0); //TODO: Expose these params in editor
					}
				}
			}

			return data;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Simple 3D noise generator using Perlin noise
		/// <br/><br/>Credit to <a href="https://gist.github.com/tntmeijs/6a3b4587ff7d38a6fa63e13f9d0ac46d">github.com/tntmeijs</a>
		/// </summary>
		private static float SimplexNoise3D(float x, float y, float z, float frequency, float amplitude, float persistence, int octave, int seed)
		{
			float noise = 0.0f;

			for (int i = 0; i < octave; ++i)
			{
				// Get all permutations of noise for each individual axis
				float noiseXY = Mathf.PerlinNoise(x * frequency + seed, y * frequency + seed) * amplitude;
				float noiseXZ = Mathf.PerlinNoise(x * frequency + seed, z * frequency + seed) * amplitude;
				float noiseYZ = Mathf.PerlinNoise(y * frequency + seed, z * frequency + seed) * amplitude;

				// Reverse of the permutations of noise for each individual axis
				float noiseYX = Mathf.PerlinNoise(y * frequency + seed, x * frequency + seed) * amplitude;
				float noiseZX = Mathf.PerlinNoise(z * frequency + seed, x * frequency + seed) * amplitude;
				float noiseZY = Mathf.PerlinNoise(z * frequency + seed, y * frequency + seed) * amplitude;

				// Use the average of the noise functions
				noise += (noiseXY + noiseXZ + noiseYZ + noiseYX + noiseZX + noiseZY) / 6.0f;

				amplitude *= persistence;
				frequency *= 2.0f;
			}

			// Use the average of all octaves
			return noise / octave;
		}
		#endregion

		public Simple3DNoiseVoxelDataGenerator(int voxelDataSetWidth)
		{
			VoxelDataSetWidth = voxelDataSetWidth;
		}
	}
}
