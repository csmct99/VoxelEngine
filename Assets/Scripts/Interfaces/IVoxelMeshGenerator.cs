using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Represents a mesh generator for voxel data. Does not come up with the data, simply generates the mesh and other visual
/// aspects.
/// </summary>
public interface IVoxelMeshGenerator
{
	Mesh GenerateChunk(IVoxelData data, int chunkSize, Vector3Int chunkPosition);
}
