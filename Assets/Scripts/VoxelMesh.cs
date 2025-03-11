using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

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

		#region Properties
		public Material MasterMaterial
		{
			get
			{
				return _masterMaterial;
			}
			set
			{
				_masterMaterial = value;
			}
		}
		#endregion

		#region MonoBehaviour Methods
		private void OnDestroy()
		{
			Cleanup();
		}
		#endregion

		#region Public Methods
		public List<GameObject> GenerateEntireMesh(IVoxelMeshGenerator meshGenerator, float[] voxelData, int voxelDataWidth, int chunkSize)
		{
			return GenerateEntireMesh(meshGenerator, voxelData, voxelDataWidth, chunkSize, out _);
		}

		public List<GameObject> GenerateEntireMesh(IVoxelMeshGenerator meshGenerator, float[] voxelData, int voxelDataWidth, int chunkSize, out Mesh[] meshes)
		{
			DeleteMeshChunks();

			// Try to evenly divide the voxel space into chunks (rounding up to ensure we cover the entire space)
			int chunksPerAxis = Mathf.CeilToInt((float) voxelDataWidth / chunkSize);

			// Make meshes
			int totalChunks = chunksPerAxis * chunksPerAxis * chunksPerAxis;

			Mesh.MeshDataArray meshDataArray;
			meshDataArray = Mesh.AllocateWritableMeshData(totalChunks);

			Mesh[] meshArray = new Mesh[totalChunks];

			for (int x = 0; x < chunksPerAxis; x++)
			{
				for (int y = 0; y < chunksPerAxis; y++)
				{
					for (int z = 0; z < chunksPerAxis; z++)
					{
						int index = x + y * chunksPerAxis + z * chunksPerAxis * chunksPerAxis;
						Vector3Int chunkCoordsPosition = new(x, y, z);
						Vector3Int worldPosition = chunkCoordsPosition * chunkSize;

						float[] chunkData = voxelData.ExtractChunkVoxels(worldPosition.x, worldPosition.y, worldPosition.z, chunkSize, voxelDataWidth);
						VoxelMeshChunk voxelChunkMesh = CreateMeshChunk($"Chunk {chunkCoordsPosition} ({worldPosition})", worldPosition);

						Mesh.MeshData meshData = meshDataArray[index];
						Mesh mesh = meshGenerator.WriteVoxelDataToMesh(ref meshData, chunkData, chunkSize);
						meshArray[index] = mesh;

						voxelChunkMesh.AssignMesh(mesh);
						voxelChunkMesh.AssignDebugData(chunkData, chunkSize);
					}
				}
			}

			//TODO: When we send the mesh update to each renderer, that prob has some overhead. We should try do the write mesh data and then assign the mesh.
			Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, meshArray, MeshUpdateFlags.DontValidateIndices);

			for (int i = 0; i < _meshChunks.Count; i++)
			{
				meshArray[i].RecalculateBounds();
			}

			meshes = meshArray;
			return new List<GameObject>(_meshChunks.ConvertAll(chunk => chunk.gameObject));
		}

		public void WipeMesh()
		{
			DeleteMeshChunks();
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
