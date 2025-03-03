using System.Collections.Generic;

using UnityEngine;

namespace VoxelEngine
{
	public class VoxelMesh : MonoBehaviour
	{
		#region Private Fields
		[SerializeField]
		private List<VoxelMeshChunk> _meshChunks = new();

		[SerializeField]
		private Material _masterMaterial;
		#endregion

		#region MonoBehaviour Methods
		private void OnDestroy()
		{
			Cleanup();
		}
		#endregion

		#region Public Methods
		public void GenerateEntireMesh(IVoxelMeshGenerator meshGenerator, IVoxelData voxelData, int chunkSize)
		{
			DeleteMeshChunks();
			
			// Try to evenly divide the voxel space into chunks (rounding up to ensure we cover the entire space)
			int chunksPerAxis = Mathf.CeilToInt((float)voxelData.Size / chunkSize);
			
			for (int x = 0; x < chunksPerAxis; x++)
			{
				for (int y = 0; y < chunksPerAxis; y++)
				{
					for (int z = 0; z < chunksPerAxis; z++)
					{
						Vector3Int chunkCoordsPosition = new(x, y, z);
						Vector3Int worldPosition = chunkCoordsPosition * chunkSize;
						
						VoxelMeshChunk chunk = CreateMeshChunk($"Chunk {chunkCoordsPosition} ({worldPosition})", worldPosition);
						chunk.AssignMesh(meshGenerator.GenerateChunk(voxelData, chunkSize, worldPosition));
						chunk.AssignDebugData(voxelData.GetSubData(worldPosition.x, worldPosition.y, worldPosition.z, chunkSize));
					}
				}
			}
		}
		#endregion

		#region Private Methods
		private void Cleanup()
		{
			DeleteMeshChunks();
		}

		private void DeleteMeshChunks()
		{
			foreach (VoxelMeshChunk meshChunk in _meshChunks)
			{
				if (Application.isPlaying)
				{
					Destroy(meshChunk.gameObject);
				}
				else
				{
					DestroyImmediate(meshChunk.gameObject);
				}
			}

			_meshChunks.Clear();
		}

		private VoxelMeshChunk CreateMeshChunk(string name, Vector3Int worldPosition)
		{
			GameObject meshChunkGameObject = new(name);
			meshChunkGameObject.transform.SetParent(transform);
			meshChunkGameObject.transform.localRotation = Quaternion.identity;
			meshChunkGameObject.transform.localScale = Vector3.one;
			meshChunkGameObject.transform.position = worldPosition;

			meshChunkGameObject.AddComponent<MeshFilter>();
			meshChunkGameObject.AddComponent<MeshRenderer>().sharedMaterial = _masterMaterial;
			VoxelMeshChunk voxelMeshChunk = meshChunkGameObject.AddComponent<VoxelMeshChunk>();
			
			_meshChunks.Add(voxelMeshChunk);
			
			return voxelMeshChunk;
		}
		#endregion
	}
}
