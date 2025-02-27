using UnityEngine;

public class PerlinNoise3DGenerator : ITerrainData
{
	public int Size
	{
		get;
		set;
	}

	public int ChunkSize
	{
		get;
		set;
	}

	public float[,,] Data
	{
		get;
		set;
	}

	public bool IsValid
	{
		get
		{
			return Data != null && Data.Length > 0;
		}
	}

	public ITerrainData Clone()
	{
		PerlinNoise3DGenerator clone = new();
		clone.Size = Size;
		clone.ChunkSize = ChunkSize;
		clone.Data = Data.Clone() as float[,,];
		return clone;
	}

	public void GenerateData()
	{
		// Generate Perlin noise data
		Data = new float[Size, Size, Size];

		// Go through each position in the 3D array
		for (int x = 0; x < Size; x++)
		{
			for (int y = 0; y < Size; y++)
			{
				for (int z = 0; z < Size; z++)
				{
					Data[x, y, z] = GetPerlinNoiseValue(x, y, z);
				}
			}
		}
	}

	public float GetValue(int x, int y, int z)
	{
		bool isOutOfBounds = x < 0 || x >= Size || y < 0 || y >= Size || z < 0 || z >= Size;
		if (isOutOfBounds)
		{
			// Return 0 if out of bounds
			return 0.0f;
		}

		// Get value from the 3D array
		return Data[x, y, z];
	}

	public bool IsEmpty(int x, int y, int z)
	{
		return GetValue(x, y, z) <= 0.5f;
	}

	public bool IsSolid(int x, int y, int z)
	{
		return !IsEmpty(x, y, z);
	}

	private float GetPerlinNoiseValue(int x, int y, int z)
	{
		// Get Perlin noise value
		return Noise3D(x, y, z, 0.03f, 1.0f, 0.5f, 1, 0);
	}

	public static float Noise3D(float x, float y, float z, float frequency, float amplitude, float persistence, int octave, int seed)
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
}
