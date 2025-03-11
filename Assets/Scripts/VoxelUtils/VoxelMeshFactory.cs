using UnityEngine;
using UnityEngine.Rendering;

namespace VoxelEngine
{
	public static class VoxelMeshFactory
	{
		#region Constants
		public const int ChunkSize = 32;
		#endregion

		#region Public Methods
		public static GameObject[] AssignMeshesToGameObjects(Mesh[] meshes, Vector3Int[] worldPositions, Transform parent, Material material = null)
		{
			GameObject[] gameObjects = new GameObject[meshes.Length];

			//Check if a material was assigned, if not make a new one
			if (material == null)
			{
				material = new Material(Shader.Find("Standard"));
				material.hideFlags |= HideFlags.DontSave;
			}

			// Create a game object for each mesh
			for (int i = 0; i < meshes.Length; i++)
			{
				gameObjects[i] = CreateGameObjectWithMesh(meshes[i], worldPositions[i], material, parent);
			}

			return gameObjects;
		}

		public static GameObject[] CreateGameobjectsFromMeshData(IVoxelDataGenerator voxelDataGenerator, IVoxelMeshGenerator meshGenerator, Transform parent, Material material = null)
		{
			CreateMeshesFromVoxelData(voxelDataGenerator, meshGenerator, out Mesh[] allMeshes, out Vector3Int[] meshWorldPositions);
			return AssignMeshesToGameObjects(allMeshes, meshWorldPositions, parent, material);
		}

		public static void CreateMeshesFromVoxelData(IVoxelDataGenerator voxelDataGenerator, IVoxelMeshGenerator meshGenerator, out Mesh[] allMeshes, out Vector3Int[] meshWorldPositions)
		{
			// Generate the data
			float[] voxelData = voxelDataGenerator.GenerateData();

			// Try to evenly divide the voxel space into chunks (rounding up to ensure we cover the entire space)
			int chunksPerAxis = Mathf.CeilToInt((float) voxelDataGenerator.VoxelDataSetWidth / ChunkSize);

			// Make meshes
			int totalChunks = chunksPerAxis * chunksPerAxis * chunksPerAxis;
			Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(totalChunks);
			allMeshes = new Mesh[totalChunks];
			meshWorldPositions = new Vector3Int[totalChunks];

			for (int x = 0; x < chunksPerAxis; x++)
			{
				for (int y = 0; y < chunksPerAxis; y++)
				{
					for (int z = 0; z < chunksPerAxis; z++)
					{
						int index = x + y * chunksPerAxis + z * chunksPerAxis * chunksPerAxis;
						Vector3Int chunkCoordsPosition = new(x, y, z);
						Vector3Int worldPosition = chunkCoordsPosition * ChunkSize;

						float[] chunkData = voxelData.ExtractChunkVoxels(worldPosition.x, worldPosition.y, worldPosition.z, ChunkSize, voxelDataGenerator.VoxelDataSetWidth);

						Mesh.MeshData meshData = meshDataArray[index];
						Mesh mesh = meshGenerator.WriteVoxelDataToMesh(ref meshData, chunkData, ChunkSize);

						allMeshes[index] = mesh;
						meshWorldPositions[index] = worldPosition;
					}
				}
			}

			Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, allMeshes, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontNotifyMeshUsers);

			for (int i = 0; i < allMeshes.Length; i++)
			{
				allMeshes[i].RecalculateBounds(); // For some reason I have to manually do this even when the flag is set to do it.
			}
		}
		#endregion

		#region Private Methods
		private static GameObject CreateGameObjectWithMesh(Mesh mesh, Vector3Int worldPosition, Material material, Transform parent)
		{
			GameObject go = new($"Chunk({worldPosition.x}, {worldPosition.y}, {worldPosition.z})");
			go.transform.SetParent(parent);
			go.transform.localPosition = worldPosition; //Names are a bit misleading, but this is the chunk's 'world position' is just a local offset in this context.

			go.AddComponent<MeshFilter>().sharedMesh = mesh;
			MeshRenderer renderer = go.AddComponent<MeshRenderer>();
			renderer.sharedMaterial = material;
			renderer.shadowCastingMode = ShadowCastingMode.TwoSided;

			return go;
		}
		#endregion
	}
}
