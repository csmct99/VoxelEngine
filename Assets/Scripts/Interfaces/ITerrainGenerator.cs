using UnityEngine;

/// <summary>
/// Represents a terrain generator. Does not come up with the data, simply generates the mesh and other visual aspects of
/// the terrain.
/// </summary>
public interface ITerrainGenerator
{
	Mesh GenerateTerrain(ITerrainData data);
}
