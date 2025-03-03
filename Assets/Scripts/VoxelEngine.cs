using System.Diagnostics;

using Sirenix.OdinInspector;

using UnityEngine;

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

		[SerializeField]
		[PropertyRange("@_chunkSize", 2048)]
		private int _voxelSpaceSize = 32;

		[SerializeField]
		[PropertyRange(4, "@_voxelSpaceSize")]
		private int _chunkSize = 16;

		[SerializeField]
		private bool _showDebug = true;

		private IVoxelMeshGenerator _voxelMeshGenerator;
		private IVoxelData _voxelData;

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
			DrawDebugTerrainData(_voxelData);
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
			_voxelMeshGenerator = new GreedyCubeVoxelMeshGenerator();
			_voxelData = new Simple3DNoiseVoxelDataGenerator();

			// Generate the terrain
			GenerateTerrain();

			// Stop measuring time
			generationTime.Stop();
			_lastGenerationTime = generationTime.ElapsedMilliseconds;
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
			_voxelData.Size = _voxelSpaceSize;
			_voxelData.GenerateData();
			_voxelMesh.GenerateEntireMesh(_voxelMeshGenerator, _voxelData, _chunkSize);
		}

		private void DrawDebugTerrainData(IVoxelData data)
		{
			if (data == null || !data.IsValid)
			{
				return;
			}

			// Draw the bounding box
			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(new Vector3(data.Size / 2, data.Size / 2, data.Size / 2), Vector3.one * data.Size);

			//If the data is bigger than 32^3, we don't want to draw all the cubes
			bool tooBig = data.Size > 32;
			if (tooBig || !_showDebug)
			{
				return;
			}

			DrawDebugAxis(new Vector3(-1, 0, -1), 20f);

			// Draw the terrain
			for (int x = 0; x < data.Size; x++)
			{
				for (int y = 0; y < data.Size; y++)
				{
					for (int z = 0; z < data.Size; z++)
					{
						if (data.IsEmpty(x, y, z))
						{
							continue;
						}

						float value = data.GetValue(x, y, z);
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
