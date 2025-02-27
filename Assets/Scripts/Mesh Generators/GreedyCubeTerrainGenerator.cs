using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

public class GreedyCubeTerrainGenerator : ITerrainGenerator
{
	private int _chunkSize = 128;

	public Mesh GenerateTerrain(ITerrainData data)
	{
		_chunkSize = data.Size;
		Mesh mesh = GenerateChunk(data);
		return mesh;
	}

	private Mesh GenerateChunk(ITerrainData data)
	{
		int arraySize = 3 * 6 * 2 * _chunkSize * _chunkSize * _chunkSize; // 36 triangles per cube

		MeshBuffer buffer = new(arraySize );

		GreedyEncodeChunk(ref buffer, Vector3Int.zero, data);

		buffer.CleanArrays(); // Remove empty data from the arrays

		//Assemble the data into the mesh
		Mesh mesh = new();
		mesh.name = $"Chunk Mesh ({_chunkSize}x{_chunkSize}x{_chunkSize})";

		mesh.indexFormat = IndexFormat.UInt32; // 4B vertices allowed on 32-bit. Long term this shouldnt be needed but for early proto its useful.

		mesh.vertices = buffer.Vertices;
		mesh.triangles = buffer.Triangles;
		mesh.normals = buffer.Normals;

		mesh.hideFlags = HideFlags.DontSave;

		return mesh;
	}

	private struct Quad
	{
		public Vector3Int A; // Bottom Left
		public Vector3Int B; // Bottom Right
		public Vector3Int C; // Top Right
		public Vector3Int D; // Top Left
	}

	private struct MeshBuffer
	{
		public int DataStreamPointer;
		public Vector3[] Vertices;
		public int[] Triangles;
		public Vector3[] Normals;
		
		public MeshBuffer(int initialSize)
		{
			DataStreamPointer = 0;
			Vertices = new Vector3[initialSize];
			Triangles = new int[initialSize];
			Normals = new Vector3[initialSize];
		}

		public void CleanArrays()
		{
			Array.Resize(ref Vertices, DataStreamPointer);
			Array.Resize(ref Triangles, DataStreamPointer);
			Array.Resize(ref Normals, DataStreamPointer);
		}
	}

	private enum PlaneAxis
	{
		YZ,
		XZ,
		XY
	}

	private bool IsSolidAlongSlice(ITerrainData terrainData, PlaneAxis axis, int localX, int localY, int layerDepth)
	{
		Vector3Int terrainCoord = ConvertAxisSlicePositionToTerrainCoord(axis, localX, localY, layerDepth);
		return terrainData.IsSolid(terrainCoord.x, terrainCoord.y, terrainCoord.z);
	}

	private Vector3Int ConvertAxisSlicePositionToTerrainCoord(PlaneAxis axis, int localX, int localY, int layerDepth)
	{
		switch (axis)
		{
			case PlaneAxis.YZ:
				return new Vector3Int(layerDepth, localX, localY);

			case PlaneAxis.XZ:
				return new Vector3Int(localX, layerDepth, localY);

			case PlaneAxis.XY:
				return new Vector3Int(localX, localY, layerDepth);

			default:
				throw new ArgumentException("Invalid axis provided.", nameof(axis));
		}
	}

	private void GreedyEncodeChunk(ref MeshBuffer buffer, Vector3Int offset, ITerrainData data)
	{
		GreedyEncodePlane(ref buffer, offset, data, PlaneAxis.YZ, true);
		GreedyEncodePlane(ref buffer, offset, data, PlaneAxis.XZ, true);
		GreedyEncodePlane(ref buffer, offset, data, PlaneAxis.XY, true);

		GreedyEncodePlane(ref buffer, offset, data, PlaneAxis.YZ, false);
		GreedyEncodePlane(ref buffer, offset, data, PlaneAxis.XZ, false);
		GreedyEncodePlane(ref buffer, offset, data, PlaneAxis.XY, false);
	}

	private void GreedyEncodePlane(ref MeshBuffer buffer, Vector3Int offset, ITerrainData data, PlaneAxis axis, bool forward)
	{
		for (int depth = 0; depth < _chunkSize; depth++)
		{
			ITerrainData terrainDataCopy = data.Clone();

			for (int localX = 0; localX < _chunkSize; localX++)
			{
				for (int localY = 0; localY < _chunkSize; localY++)
				{
					bool isVoxelSolid = IsSolidAlongSlice(terrainDataCopy, axis, localX, localY, depth);
					bool isNextVoxelSolid = IsSolidAlongSlice(terrainDataCopy, axis, localX, localY, depth + (forward ? 1 : -1));
					bool shouldRenderThisVoxel = isVoxelSolid && !isNextVoxelSolid;

					if (!shouldRenderThisVoxel)
					{
						continue;
					}

					// We know we want to render this voxel, but how far can we spread it?
					int maxXFill = 1; // This voxel is valid therefore we can travel at least 1 voxel

					for (int travelX = localX + 1; travelX < _chunkSize; travelX++)
					{
						bool isTravelVoxelSolid = IsSolidAlongSlice(terrainDataCopy, axis, travelX, localY, depth);
						bool isNextTravelVoxelSolid = IsSolidAlongSlice(terrainDataCopy, axis, travelX, localY, depth + (forward ? 1 : -1));
						bool canTravel = isTravelVoxelSolid && !isNextTravelVoxelSolid;

						if (!canTravel)
						{
							break;
						}

						// We can travel here, mark it and remove it from the copy data so we don't draw it again
						maxXFill++;
						Vector3Int travelVoxel = ConvertAxisSlicePositionToTerrainCoord(axis, travelX, localY, depth);
						terrainDataCopy.Data[travelVoxel.x, travelVoxel.y, travelVoxel.z] = 0.0f;
					}

					//Now that we know how far along the X axis we can travel, we can try and spread along the Y axis
					int maxYFill = 1;
					for (int travelY = localY + 1; travelY < _chunkSize; travelY++)
					{
						bool didBreakOut = false;
						List<Vector3Int> travelledVoxels = new();

						for (int travelX = localX; travelX < localX + maxXFill; travelX++)
						{
							bool isTravelVoxelSolid = IsSolidAlongSlice(terrainDataCopy, axis, travelX, travelY, depth);
							bool isNextTravelVoxelSolid = IsSolidAlongSlice(terrainDataCopy, axis, travelX, travelY, depth + (forward ? 1 : -1));
							bool canTravel = isTravelVoxelSolid && !isNextTravelVoxelSolid;

							if (!canTravel)
							{
								didBreakOut = true;
								break;
							}

							travelledVoxels.Add(ConvertAxisSlicePositionToTerrainCoord(axis, travelX, travelY, depth));
						}

						if (didBreakOut)
						{
							break;
						}

						// We can travel here, mark it and remove it from the copy data so we don't draw it again
						foreach (Vector3Int voxel in travelledVoxels)
						{
							terrainDataCopy.Data[voxel.x, voxel.y, voxel.z] = 0.0f;
						}

						maxYFill++;
					}

					int xSize = maxXFill;
					int ySize = maxYFill;

					// Encode Quads
					switch (axis)
					{
						case PlaneAxis.XY: // Front / Back 
							EncodeQuad(ref buffer, offset + Vector3Int.forward * (forward ? 1 : 0), new Quad
							{
								A = new Vector3Int(localX, localY, depth),
								B = new Vector3Int(localX + xSize, localY, depth),
								C = new Vector3Int(localX + xSize, localY + ySize, depth),
								D = new Vector3Int(localX, localY + ySize, depth)
							}, forward);
							break;

						case PlaneAxis.YZ: // Right / Left
							EncodeQuad(ref buffer, offset + Vector3Int.right * (forward ? 1 : 0), new Quad
							{
								A = new Vector3Int(depth, localY, localX),
								B = new Vector3Int(depth, localY, localX + xSize),
								C = new Vector3Int(depth, localY + ySize, localX + xSize),
								D = new Vector3Int(depth, localY + ySize, localX)
							}, !forward);
							break;

						case PlaneAxis.XZ: // Top / Bottom

							// Top
							EncodeQuad(ref buffer, offset + Vector3Int.up * (forward ? 1 : 0), new Quad
							{
								A = new Vector3Int(localX, depth, localY),
								B = new Vector3Int(localX + xSize, depth, localY),
								C = new Vector3Int(localX + xSize, depth, localY + ySize),
								D = new Vector3Int(localX, depth, localY + ySize)
							}, !forward);
							break;
					}
				}
			}
		}
	}

	private void EncodeQuad(ref MeshBuffer buffer, Vector3Int offset, Quad quad, bool reverseDirection = false)
	{
		if (reverseDirection)
		{
			EncodeTriangle(ref buffer, offset, quad.A, quad.B, quad.C);
			EncodeTriangle(ref buffer, offset, quad.A, quad.C, quad.D);
			return;
		}

		EncodeTriangle(ref buffer, offset, quad.C, quad.B, quad.A);
		EncodeTriangle(ref buffer, offset, quad.D, quad.C, quad.A);
	}

	private void EncodeTriangle(ref MeshBuffer buffer, Vector3Int offset, Vector3Int vertexA, Vector3Int vertexB, Vector3Int vertexC)
	{
		buffer.Vertices[buffer.DataStreamPointer + 0] = vertexA + offset;
		buffer.Vertices[buffer.DataStreamPointer + 1] = vertexB + offset;
		buffer.Vertices[buffer.DataStreamPointer + 2] = vertexC + offset;

		buffer.Triangles[buffer.DataStreamPointer + 0] = buffer.DataStreamPointer + 0;
		buffer.Triangles[buffer.DataStreamPointer + 1] = buffer.DataStreamPointer + 1;
		buffer.Triangles[buffer.DataStreamPointer + 2] = buffer.DataStreamPointer + 2;

		Vector3 normal = Vector3.Cross(vertexB - vertexA, vertexC - vertexA).normalized; //TODO: This can be precalculated
		buffer.Normals[buffer.DataStreamPointer + 0] = normal;
		buffer.Normals[buffer.DataStreamPointer + 1] = normal;
		buffer.Normals[buffer.DataStreamPointer + 2] = normal;

		buffer.DataStreamPointer += 3; // Move to the next triangle position
	}
}
