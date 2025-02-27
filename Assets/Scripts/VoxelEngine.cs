using System.Diagnostics;

using Sirenix.OdinInspector;

using UnityEngine;

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
		private int _size = 32;

		[SerializeField]
		private bool _showDebug = true;

		private ITerrainGenerator _terrainGenerator;
		private ITerrainData _terrainData;

		[SerializeField]
		private MeshFilter _meshFilter;
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
			DrawDebugTerrainData(_terrainData);
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
			_terrainGenerator = new GreedyCubeTerrainGenerator();
			_terrainData = new PerlinNoise3DGenerator();

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
			_terrainData = null;
			_terrainGenerator = null;

			if (_meshFilter.sharedMesh != null)
			{
				if (Application.isPlaying)
				{
					Destroy(_meshFilter.sharedMesh);
				}
				else
				{
					DestroyImmediate(_meshFilter.sharedMesh);
				}

				_meshFilter.sharedMesh = null;
			}
		}

		private void GenerateTerrain()
		{
			_terrainData.Size = _size;
			_terrainData.GenerateData();

			_meshFilter.sharedMesh = _terrainGenerator.GenerateTerrain(_terrainData);
		}

		private void DrawDebugTerrainData(ITerrainData data)
		{
			if (data == null || !data.IsValid)
			{
				return;
			}

			DrawDebugAxis(new Vector3(-1, 0, -1), 20f);

			// Draw the bounding box
			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(new Vector3(data.Size / 2, data.Size / 2, data.Size / 2), Vector3.one * data.Size);

			//If the data is bigger than 32^3, we don't want to draw all the cubes
			bool tooBig = data.Size > 32;
			if (tooBig || !_showDebug)
			{
				return;
			}

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
