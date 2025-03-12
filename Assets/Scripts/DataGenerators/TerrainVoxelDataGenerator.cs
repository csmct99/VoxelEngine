using System.Diagnostics;

using Unity.Collections;
using Unity.Jobs;

using UnityEngine;

using Debug = UnityEngine.Debug;

namespace VoxelEngine.DataGenerators
{
	public class TerrainVoxelDataGenerator : IVoxelDataGenerator
	{
		
		public TerrainVoxelDataGenerator(int voxelDataSetWidth)
		{
			VoxelDataSetWidth = voxelDataSetWidth;
		}
		
		public struct NoiseGeneratorJob : IJobParallelFor
		{
			// We are allowing writing to any point in the array.
			// I dont care which thread writes when, the index ensures they dont access the same areas of memory.
			[NativeDisableParallelForRestriction] 
			public NativeArray<float> noiseData; // Flat 3D array of noise data
			public int width; // Width of the 3D array

			//The index is the depth (z) of the noise. Each job will be responsible for a single depth layer
			public void Execute(int index) 
			{
				//Create the noise generator object
				FastNoiseLite fastNoise = new FastNoiseLite();
				fastNoise.SetSeed(133742069);
				
				fastNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
				fastNoise.SetFrequency(0.015f);
				
				fastNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
				fastNoise.SetFractalOctaves(3);
				fastNoise.SetFractalLacunarity(2.0f);
				fastNoise.SetFractalGain(0.5f);
				
				for (int x = 0; x < width; x++)
				{
					for (int y = 0; y < width; y++)
					{
						int flatIndex = index * width * width + y * width + x;
						float value = fastNoise.GetNoise(x, y, index);
						
						float heightAsPercent = (y / (float)width);
						float v = Mathf.Lerp(1, 0.15f, Mathf.Clamp(heightAsPercent + 0.25f, 0, 1));
						
						value *= v;
						
						noiseData[flatIndex] = 1 - heightAsPercent + value;
					}
				}
			}
			
			public void Cleanup()
			{
				noiseData.Dispose();
			}
		}
		
		public float[] GenerateData()
		{
			NoiseGeneratorJob job = new NoiseGeneratorJob();
			job.width = VoxelDataSetWidth;
			job.noiseData = new NativeArray<float>(VoxelDataSetWidth * VoxelDataSetWidth * VoxelDataSetWidth, Allocator.TempJob);
			
			JobHandle handle = job.Schedule(VoxelDataSetWidth, 1);
			
			handle.Complete();
			
			float[] data = new float[VoxelDataSetWidth * VoxelDataSetWidth * VoxelDataSetWidth];
			job.noiseData.CopyTo(data);
			
			job.Cleanup();
			return data;
		}

		public int VoxelDataSetWidth
		{
			get;
		}
	}
}
