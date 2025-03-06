using Sirenix.OdinInspector;

using UnityEngine;

namespace VoxelEngine
{
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
	public class VoxelMeshChunk : MonoBehaviour
	{
		#region Private Fields
		[ShowInInspector]
		[ReadOnly]
		private Mesh _mesh;

		[ShowIf("@_showDebug")]
		[ShowInInspector]
		private bool _showDebug = false;

		private float[] _chunkData;
		private int _chunkSize;

		private MeshFilter _meshFilter;
		#endregion

		#region MonoBehaviour Methods
		private void OnDestroy()
		{
			Cleanup();
		}

		private void OnDrawGizmos()
		{
			if (_showDebug)
			{
				Gizmos.color = Color.red;

				if (_chunkData != null)
				{
					for (int x = 0; x < _chunkSize; x++)
					{
						for (int y = 0; y < _chunkSize; y++)
						{
							for (int z = 0; z < _chunkSize; z++)
							{
								if (_chunkData.IsVoxelSolid(x, y, z, _chunkSize))
								{
									Gizmos.DrawWireCube(new Vector3(x, y, z) + transform.position + Vector3.one / 2, Vector3.one);
								}
							}
						}
					}
				}
			}
		}

		private void OnValidate()
		{
			if (_meshFilter == null)
			{
				_meshFilter = GetComponent<MeshFilter>();
			}
		}
		#endregion

		#region Public Methods
		public void AssignDebugData(float[] chunkVoxelData, int chunkSize)
		{
			_chunkData = chunkVoxelData;
			_chunkSize = chunkSize;
		}

		public void AssignMesh(Mesh mesh)
		{
			_mesh = mesh;
			_meshFilter.sharedMesh = _mesh;
		}
		#endregion

		#region Private Methods
		[Button]
		private void ToggleDebug()
		{
			_showDebug = !_showDebug;
		}

		private void Cleanup()
		{
			DeleteMesh();
		}

		private void DeleteMesh()
		{
			if (_mesh != null)
			{
				if (Application.isPlaying)
				{
					Destroy(_mesh);
				}
				else
				{
					DestroyImmediate(_mesh);
				}

				_mesh = null;
			}
		}
		#endregion
	}
}
