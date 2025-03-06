using System.Diagnostics;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.Serialization;

using VoxelEngine;
using VoxelEngine.DataGenerators;

namespace DefaultNamespace
{
	public class VoxelEngine : MonoBehaviour
	{
		#region Private Fields
		[ShowInInspector]
		[ReadOnly]
		[LabelText("Last Generation (ms)")] // We dont want to save this but we want to see it in the inspector
		private float _lastGenerationTime;

		[FormerlySerializedAs("_voxelSpaceSize")]
		[SerializeField]
		[PropertyRange("@_chunkSize", 2048)]
		private int _voxelDataWidth = 32;

		[SerializeField]
		[PropertyRange(4, "@_voxelDataWidth")]
		private int _chunkSize = 16;

		[SerializeField]
		private bool _showDebug = true;

		private IVoxelMeshGenerator _voxelMeshGenerator;
		private IVoxelDataGenerator _voxelDataGenerator;
		private float[] _voxelData;

		[SerializeField]
		[Required]
		private VoxelMesh _voxelMesh;
		#endregion

		#region MonoBehaviour Methods
		private void Start()
		{
			Restart();
		}

		private void OnDestroy()
		{
			Cleanup();
		}

		private void OnDrawGizmos()
		{
			DrawDebugTerrainData(_voxelData, _voxelDataWidth);
		}
		#endregion

		#region Public Methods
		[Button("Regenerate")]
		public void Restart()
		{
			// Start measuring time
			_lastGenerationTime = 0;
			Stopwatch generationTime = new();
			generationTime.Start();

			// Cleanup the old data and generate new ones
			Cleanup();
			_voxelDataGenerator = new Simple3DNoiseVoxelDataGenerator();
			_voxelMeshGenerator = new GreedyCubeVoxelMeshGenerator();

			// Generate the terrain
			GenerateTerrain();

			// Stop measuring time
			generationTime.Stop();
			_lastGenerationTime = generationTime.ElapsedMilliseconds;
		}

		[Button("Wipe Mesh")]
		public void WipeMesh()
		{
			_voxelMesh.WipeMesh();
			Cleanup();
		}
		#endregion

		#region Private Methods
		private void Cleanup()
		{
			_voxelData = null;
			_voxelMeshGenerator = null;
		}

		private void GenerateTerrain()
		{
			_voxelData = _voxelDataGenerator.GenerateData(_voxelDataWidth);
			_voxelMesh.GenerateEntireMesh(_voxelMeshGenerator, _voxelData, _voxelDataWidth, _chunkSize);
		}

		private void DrawDebugTerrainData(float[] voxelData, int size)
		{
			if (voxelData == null)
			{
				return;
			}

			// Draw the bounding box
			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(new Vector3(size / 2, size / 2, size / 2), Vector3.one * size);

			//If the data is bigger than 32^3, we don't want to draw all the cubes
			bool tooBig = size > 32;
			if (tooBig || !_showDebug)
			{
				return;
			}

			DrawDebugAxis(new Vector3(-1, 0, -1), 20f);

			// Draw the terrain
			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y < size; y++)
				{
					for (int z = 0; z < size; z++)
					{
						if (voxelData.IsVoxelEmpty(x, y, z, size))
						{
							continue;
						}

						float value = voxelData.GetVoxel(x, y, z, size);
						Gizmos.color = new Color(0.1f, 0.1f, 0.1f, Mathf.Clamp01(value)); // Gray scale

						float cubeScale = 0.95f;
						Gizmos.DrawCube(new Vector3(x, y, z) * 1 + Vector3.one * cubeScale / 2, Vector3.one * cubeScale);
					}
				}
			}
		}

		private void DrawDebugAxis(Vector3 position, float size)
		{
			// Draw XYZ colored axis
			// X
			Gizmos.color = Color.red;
			Gizmos.DrawLine(position, position + Vector3.right * size);

			// Y
			Gizmos.color = Color.green;
			Gizmos.DrawLine(position, position + Vector3.up * size);

			// Z
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(position, position + Vector3.forward * size);
		}
		#endregion
	}
}
