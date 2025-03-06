using System;

using UnityEngine;
using UnityEngine.Rendering;

public class SimpleCubeVoxelMeshGenerator : IVoxelMeshGenerator
{
	private int _voxelDataWidth = 128;

	public Mesh GenerateVoxelMesh(float[] data, int voxelDataWidth, Vector3Int position)
	{
		_voxelDataWidth = voxelDataWidth;
		Mesh mesh = GenerateMesh(data);
		return mesh;
	}

	private Mesh GenerateMesh(float[] data)
	{
		int maxTriangles = 6 * 2 * 3 * _voxelDataWidth * _voxelDataWidth * _voxelDataWidth; // 36 triangles per cube
		int[] triangles = new int[maxTriangles];

		int maxVertices = 6 * 2 * 3 * _voxelDataWidth * _voxelDataWidth * _voxelDataWidth; // 36 vertices per cube
		Vector3[] vertices = new Vector3[maxVertices];

		int maxNormals = 6 * 2 * 3 * _voxelDataWidth * _voxelDataWidth * _voxelDataWidth;
		Vector3[] normals = new Vector3[maxNormals];

		int meshDataStreamPointer = 0;

		for (int x = 0; x < _voxelDataWidth; x++)
		{
			for (int y = 0; y < _voxelDataWidth; y++)
			{
				for (int z = 0; z < _voxelDataWidth; z++)
				{
					if (data.IsVoxelEmpty(x, y, z, _voxelDataWidth))
					{
						continue;
					}

					EncodeCube(ref meshDataStreamPointer, new Vector3Int(x, y, z), ref vertices, ref triangles, ref normals, data);
				}
			}
		}

		// Trim the arrays to the correct size
		Array.Resize(ref vertices, meshDataStreamPointer);
		Array.Resize(ref triangles, meshDataStreamPointer);
		Array.Resize(ref normals, meshDataStreamPointer);


		//Assemble the data into the mesh
		Mesh mesh = new();
		mesh.name = "Chunk Mesh";

		mesh.indexFormat = IndexFormat.UInt32; // 4B vertices allowed on 32-bit. Long term this shouldnt be needed but for early proto its useful.

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;

		mesh.hideFlags = HideFlags.DontSave;

		return mesh;
	}

	/// <summary>
	/// Adds a cube to the vertices and triangles arrays
	/// </summary>
	/// <param name="meshDataStreamPointer"></param>
	/// <param name="position"></param>
	/// <param name="vertices"></param>
	/// <param name="triangles"></param>
	private void EncodeCube(ref int meshDataStreamPointer, Vector3Int position, ref Vector3[] vertices, ref int[] triangles, ref Vector3[] normals, float[] voxelData)
	{
		// +Y (Top)
		if (voxelData.IsVoxelEmpty(position.x, position.y + 1, position.z, _voxelDataWidth))
		{
			EncodeTriangle(ref meshDataStreamPointer, ref vertices, ref triangles, ref normals, position, new Vector3Int(1, 1, 1), new Vector3Int(1, 1, 0), new Vector3Int(0, 1, 0));

			EncodeTriangle(ref meshDataStreamPointer, ref vertices, ref triangles, ref normals, position, new Vector3Int(0, 1, 1), new Vector3Int(1, 1, 1), new Vector3Int(0, 1, 0));
		}

		// -Y (Bottom)
		if (voxelData.IsVoxelEmpty(position.x, position.y - 1, position.z, _voxelDataWidth))
		{
			EncodeTriangle(ref meshDataStreamPointer, ref vertices, ref triangles, ref normals, position, new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(1, 0, 1));

			EncodeTriangle(ref meshDataStreamPointer, ref vertices, ref triangles, ref normals, position, new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 1), new Vector3Int(0, 0, 1));
		}

		// +X (Right)
		if (voxelData.IsVoxelEmpty(position.x + 1, position.y, position.z, _voxelDataWidth))
		{
			EncodeTriangle(ref meshDataStreamPointer, ref vertices, ref triangles, ref normals, position, new Vector3Int(1, 0, 0), new Vector3Int(1, 1, 0), new Vector3Int(1, 1, 1));

			EncodeTriangle(ref meshDataStreamPointer, ref vertices, ref triangles, ref normals, position, new Vector3Int(1, 0, 0), new Vector3Int(1, 1, 1), new Vector3Int(1, 0, 1));
		}

		// -X (Left)
		if (voxelData.IsVoxelEmpty(position.x - 1, position.y, position.z, _voxelDataWidth))
		{
			EncodeTriangle(ref meshDataStreamPointer, ref vertices, ref triangles, ref normals, position, new Vector3Int(0, 1, 1), new Vector3Int(0, 1, 0), new Vector3Int(0, 0, 0));

			EncodeTriangle(ref meshDataStreamPointer, ref vertices, ref triangles, ref normals, position, new Vector3Int(0, 0, 1), new Vector3Int(0, 1, 1), new Vector3Int(0, 0, 0));
		}

		// +Z (Front)
		if (voxelData.IsVoxelEmpty(position.x, position.y, position.z + 1, _voxelDataWidth))
		{
			EncodeTriangle(ref meshDataStreamPointer, ref vertices, ref triangles, ref normals, position, new Vector3Int(0, 0, 1), new Vector3Int(1, 0, 1), new Vector3Int(1, 1, 1));

			EncodeTriangle(ref meshDataStreamPointer, ref vertices, ref triangles, ref normals, position, new Vector3Int(0, 0, 1), new Vector3Int(1, 1, 1), new Vector3Int(0, 1, 1));
		}

		// -Z (Back)
		if (voxelData.IsVoxelEmpty(position.x, position.y, position.z - 1, _voxelDataWidth))
		{
			EncodeTriangle(ref meshDataStreamPointer, ref vertices, ref triangles, ref normals, position, new Vector3Int(1, 1, 0), new Vector3Int(1, 0, 0), new Vector3Int(0, 0, 0));

			EncodeTriangle(ref meshDataStreamPointer, ref vertices, ref triangles, ref normals, position, new Vector3Int(0, 1, 0), new Vector3Int(1, 1, 0), new Vector3Int(0, 0, 0));
		}
	}

	private void EncodeTriangle(ref int dataStreamPointer, ref Vector3[] vertices, ref int[] triangles, ref Vector3[] normals, Vector3Int offset, Vector3Int vertexA, Vector3Int vertexB, Vector3Int vertexC)
	{
		vertices[dataStreamPointer + 0] = vertexA + offset;
		vertices[dataStreamPointer + 1] = vertexB + offset;
		vertices[dataStreamPointer + 2] = vertexC + offset;

		triangles[dataStreamPointer + 0] = dataStreamPointer + 0;
		triangles[dataStreamPointer + 1] = dataStreamPointer + 1;
		triangles[dataStreamPointer + 2] = dataStreamPointer + 2;

		Vector3 normal = Vector3.Cross(vertexB - vertexA, vertexC - vertexA).normalized;
		normals[dataStreamPointer + 0] = normal;
		normals[dataStreamPointer + 1] = normal;
		normals[dataStreamPointer + 2] = normal;

		dataStreamPointer += 3; // Move to the next triangle position
	}
}
