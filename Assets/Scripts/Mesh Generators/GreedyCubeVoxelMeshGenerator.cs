using System;
using System.Collections.Generic;

using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;

public class GreedyCubeVoxelMeshGenerator : IVoxelMeshGenerator
{
	#region Enumerations
	private enum PlaneAxis
	{
		/// <summary>
		/// Left / Right
		/// </summary>
		YZ,

		/// <summary>
		/// Top / Bottom
		/// </summary>
		XZ,

		/// <summary>
		/// Front / Back
		/// </summary>
		XY
	}
	#endregion

	#region Private Fields
	private int _voxelDataWidth = 32;
	#endregion

	#region Structs
	private struct FaceID
	{
		public const int Top = 0;
		public const int Bottom = 1;
		public const int Right = 2;
		public const int Left = 3;
		public const int Front = 4;
		public const int Back = 5;
	}

	private struct MeshBuffer
	{
		public int DataStreamPointer;
		public Mesh.MeshData MeshData;

		public MeshBuffer(Mesh.MeshData meshData)
		{
			DataStreamPointer = 0;
			MeshData = meshData;
		}

		public void ResizeArrays()
		{
			MeshData.SetIndexBufferParams(DataStreamPointer, IndexFormat.UInt32);
			MeshData.SetVertexBufferParams(DataStreamPointer, 
				new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0), //TODO: Reconsider these data types, this is way more than I need
				new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1),
				new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 2)
			);
		}
	}

	private struct Quad
	{
		public Vector3Int A; // Bottom Left
		public Vector3Int B; // Bottom Right
		public Vector3Int C; // Top Right
		public Vector3Int D; // Top Left
	}
	#endregion

	#region Public Methods
	public Mesh WriteVoxelDataToMesh(ref Mesh.MeshData meshData, float[] data, int voxelDataWidth)
	{
		_voxelDataWidth = voxelDataWidth;
		return WriteToMesh(data, ref meshData);
	}
	#endregion

	#region Private Methods
	private Mesh WriteToMesh(float[] data, ref Mesh.MeshData meshData)
	{
		//TODO: Initializing such a potentially huge data set is not ideal. This should be done in a more dynamic way. Maybe I swap to using Lists in the end.
		int arraySize = 3 * 6 * 2 * _voxelDataWidth * _voxelDataWidth * _voxelDataWidth; // 36 triangles per cube

		meshData.SetIndexBufferParams(arraySize, IndexFormat.UInt32);
		meshData.SetVertexBufferParams(arraySize, 
			new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0), //TODO: Reconsider these data types, this is way more than I need
			new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 2)
		);
		
		MeshBuffer buffer = new(meshData);
		GreedyEncodeVoxelMesh(ref buffer, data);

		// Finalize data
		buffer.ResizeArrays(); // Remove empty data from the arrays
		buffer.MeshData.subMeshCount = 1;
		buffer.MeshData.SetSubMesh(0, new SubMeshDescriptor(0, buffer.DataStreamPointer, MeshTopology.Triangles));

		//Assemble the data into the mesh
		Mesh mesh = new();
		mesh.name = $"Chunk Mesh ({_voxelDataWidth}x{_voxelDataWidth}x{_voxelDataWidth})";
		mesh.indexFormat = IndexFormat.UInt16;
		mesh.hideFlags = HideFlags.DontSave; // Dont save this to the scene, its a temporary mesh meant to be manually managed

		return mesh;
	}

	/// <summary>
	/// Check if a voxel is <see cref="IVoxelData.IsSolid(int, int, int)">solid</see> along a slice of a plane
	/// </summary>
	private bool IsSolidAlongSlice(float[] voxelData, PlaneAxis axis, int localX, int localY, int layerDepth)
	{
		Vector3Int terrainCoord = ConvertAxisSlicePositionToTerrainCoord(axis, localX, localY, layerDepth);
		return voxelData.IsVoxelSolid(terrainCoord.x, terrainCoord.y, terrainCoord.z, _voxelDataWidth);
	}

	/// <summary>
	/// Convert a local sliced position on a plane to voxel coordinates
	/// </summary>
	/// <returns> The voxel coordinates </returns>
	/// <exception cref="ArgumentException"> Thrown when an invalid axis is provided </exception>
	private Vector3Int ConvertAxisSlicePositionToTerrainCoord(PlaneAxis axis, int localX, int localY, int layerDepth)
	{
		switch (axis)
		{
			case PlaneAxis.YZ:
				return new Vector3Int(layerDepth, localY, localX);

			case PlaneAxis.XZ:
				return new Vector3Int(localX, layerDepth, localY);

			case PlaneAxis.XY:
				return new Vector3Int(localX, localY, layerDepth);

			default:
				throw new ArgumentException("Invalid axis provided.", nameof(axis));
		}
	}

	/// <summary>
	/// Using the greedy meshing algorithm, encode a chunk of voxels into the mesh buffer. All Top, Bottom, Right, Left, Front,
	/// Back faces.
	/// </summary>
	private void GreedyEncodeVoxelMesh(ref MeshBuffer buffer, float[] data)
	{
		//Encode each face into the mesh buffer
		GreedyEncodePlane(ref buffer, Vector3Int.zero, data, PlaneAxis.YZ, true);
		GreedyEncodePlane(ref buffer, Vector3Int.zero, data, PlaneAxis.XZ, true);
		GreedyEncodePlane(ref buffer, Vector3Int.zero, data, PlaneAxis.XY, true);

		GreedyEncodePlane(ref buffer, Vector3Int.zero, data, PlaneAxis.YZ, false);
		GreedyEncodePlane(ref buffer, Vector3Int.zero, data, PlaneAxis.XZ, false);
		GreedyEncodePlane(ref buffer, Vector3Int.zero, data, PlaneAxis.XY, false);
	}

	/// <summary>
	/// Using the greedy meshing algorithm, encode a plane of voxels into the mesh buffer
	/// </summary>
	/// <param name="buffer"> The mesh buffer to encode the plane into </param>
	/// <param name="offset"> The offset to apply to the plane vertices </param>
	/// <param name="chunkData"> The voxel data to encode </param>
	/// <param name="axis"> The axis to encode the plane along </param>
	/// <param name="forward"> Should the plane be encoded in the forward direction? </param>
	private void GreedyEncodePlane(ref MeshBuffer buffer, Vector3Int offset, float[] chunkData, PlaneAxis axis, bool forward)
	{
		for (int depth = 0; depth < _voxelDataWidth; depth++)
		{
			float[] chunkDataCopy = new float[chunkData.Length];
			Array.Copy(chunkData, chunkDataCopy, chunkData.Length);

			for (int localX = 0; localX < _voxelDataWidth; localX++)
			{
				for (int localY = 0; localY < _voxelDataWidth; localY++)
				{
					bool isVoxelSolid = IsSolidAlongSlice(chunkDataCopy, axis, localX, localY, depth);
					bool isNextVoxelSolid = IsSolidAlongSlice(chunkDataCopy, axis, localX, localY, depth + (forward ? 1 : -1));
					bool shouldRenderThisVoxel = isVoxelSolid && !isNextVoxelSolid;

					if (!shouldRenderThisVoxel)
					{
						continue;
					}

					// We know we want to render this voxel, but how far can we spread it?
					int maxXFill = 1; // This voxel is valid therefore we can travel at least 1 voxel

					for (int travelX = localX + 1; travelX < _voxelDataWidth; travelX++)
					{
						bool isTravelVoxelSolid = IsSolidAlongSlice(chunkDataCopy, axis, travelX, localY, depth);
						bool isNextTravelVoxelSolid = IsSolidAlongSlice(chunkDataCopy, axis, travelX, localY, depth + (forward ? 1 : -1));
						bool canTravel = isTravelVoxelSolid && !isNextTravelVoxelSolid;

						if (!canTravel)
						{
							break;
						}

						// We can travel here, mark it and remove it from the copy data so we don't draw it again
						maxXFill++;
						Vector3Int travelVoxel = ConvertAxisSlicePositionToTerrainCoord(axis, travelX, localY, depth);
						chunkDataCopy.SetVoxel(travelVoxel.x, travelVoxel.y, travelVoxel.z, _voxelDataWidth, 0.0f);
					}

					//Now that we know how far along the X axis we can travel, we can try and spread along the Y axis
					int maxYFill = 1;
					for (int travelY = localY + 1; travelY < _voxelDataWidth; travelY++)
					{
						bool didBreakOut = false;
						List<Vector3Int> travelledVoxels = new();

						for (int travelX = localX; travelX < localX + maxXFill; travelX++)
						{
							bool isTravelVoxelSolid = IsSolidAlongSlice(chunkDataCopy, axis, travelX, travelY, depth);
							bool isNextTravelVoxelSolid = IsSolidAlongSlice(chunkDataCopy, axis, travelX, travelY, depth + (forward ? 1 : -1));
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
							chunkDataCopy.SetVoxel(voxel.x, voxel.y, voxel.z, _voxelDataWidth, 0.0f);
						}

						maxYFill++;
					}

					int xSize = maxXFill;
					int ySize = maxYFill;

					int faceID = 0;

					// Encode Quads
					switch (axis)
					{
						case PlaneAxis.XY: // Front / Back 

							faceID = forward ? FaceID.Front : FaceID.Back;

							EncodeQuad(ref buffer, offset + Vector3Int.forward * (forward ? 1 : 0), new Quad
							{
								A = new Vector3Int(localX, localY, depth),
								B = new Vector3Int(localX + xSize, localY, depth),
								C = new Vector3Int(localX + xSize, localY + ySize, depth),
								D = new Vector3Int(localX, localY + ySize, depth)
							}, faceID, forward);
							break;

						case PlaneAxis.YZ: // Right / Left

							faceID = forward ? FaceID.Right : FaceID.Left;

							EncodeQuad(ref buffer, offset + Vector3Int.right * (forward ? 1 : 0), new Quad
							{
								A = new Vector3Int(depth, localY, localX),
								B = new Vector3Int(depth, localY, localX + xSize),
								C = new Vector3Int(depth, localY + ySize, localX + xSize),
								D = new Vector3Int(depth, localY + ySize, localX)
							}, faceID, !forward);
							break;

						case PlaneAxis.XZ: // Top / Bottom

							faceID = forward ? FaceID.Top : FaceID.Bottom;

							// Top
							EncodeQuad(ref buffer, offset + Vector3Int.up * (forward ? 1 : 0), new Quad
							{
								A = new Vector3Int(localX, depth, localY),
								B = new Vector3Int(localX + xSize, depth, localY),
								C = new Vector3Int(localX + xSize, depth, localY + ySize),
								D = new Vector3Int(localX, depth, localY + ySize)
							}, faceID, !forward);
							break;
					}
				}
			}
		}
	}

	/// <summary>
	/// Encodes a quad into the mesh buffer
	/// </summary>
	/// <param name="buffer"> The mesh buffer to encode the quad into </param>
	/// <param name="offset"> The offset to apply to the quad vertices </param>
	/// <param name="quad"> The quad to encode </param>
	/// <param name="reverseDirection">
	/// Should the quad be encoded in reverse order (CW/CCW)? Useful to avoid remapping
	/// vertices in a seperate call
	/// </param>
	private void EncodeQuad(ref MeshBuffer buffer, Vector3Int offset, Quad quad, int faceID, bool reverseDirection = false)
	{
		if (reverseDirection)
		{
			EncodeTriangle(ref buffer, faceID, offset, quad.A, quad.B, quad.C);
			EncodeTriangle(ref buffer, faceID, offset, quad.A, quad.C, quad.D);
			return;
		}

		EncodeTriangle(ref buffer, faceID, offset, quad.C, quad.B, quad.A);
		EncodeTriangle(ref buffer, faceID, offset, quad.D, quad.C, quad.A);
	}

	/// <summary>
	/// Encodes a triangle into the mesh buffer. Verticies, Normals and Triangles are all updated.
	/// </summary>
	/// <param name="buffer"> The mesh buffer to encode the triangle into </param>
	/// <param name="offset"> The offset to apply to the triangle vertices </param>
	/// <param name="vertexA"> The first vertex of the triangle (CW) </param>
	/// <param name="vertexB"> The second vertex of the triangle (CW) </param>
	/// <param name="vertexC"> The third vertex of the triangle (CW) </param>
	private void EncodeTriangle(ref MeshBuffer buffer, int faceDir, Vector3Int offset, Vector3Int vertexA, Vector3Int vertexB, Vector3Int vertexC)
	{
		//Positions
		NativeArray<Vector3> positions = buffer.MeshData.GetVertexData<Vector3>(0);
		positions[buffer.DataStreamPointer + 0] = vertexA + offset;
		positions[buffer.DataStreamPointer + 1] = vertexB + offset;
		positions[buffer.DataStreamPointer + 2] = vertexC + offset;

		//Triangles (Index order) just sequential
		NativeArray<int> indexBuffer = buffer.MeshData.GetIndexData<int>();
		indexBuffer[buffer.DataStreamPointer + 0] = buffer.DataStreamPointer + 0;
		indexBuffer[buffer.DataStreamPointer + 1] = buffer.DataStreamPointer + 1;
		indexBuffer[buffer.DataStreamPointer + 2] = buffer.DataStreamPointer + 2;

		NativeArray<Vector3> normals = buffer.MeshData.GetVertexData<Vector3>(1);
		Vector3 normal = Vector3.Cross(vertexB - vertexA, vertexC - vertexA).normalized; //TODO: This can be precalculated, no need to do this math every time
		normals[buffer.DataStreamPointer + 0] = normal;
		normals[buffer.DataStreamPointer + 1] = normal;
		normals[buffer.DataStreamPointer + 2] = normal;

		// We also want to encode the face index // TODO: We can surely optimize all this extra data going in
		NativeArray<Vector2> uv0 = buffer.MeshData.GetVertexData<Vector2>(2);
		uv0[buffer.DataStreamPointer + 0] = new Vector2(faceDir, faceDir);
		uv0[buffer.DataStreamPointer + 1] = new Vector2(faceDir, faceDir);
		uv0[buffer.DataStreamPointer + 2] = new Vector2(faceDir, faceDir);

		buffer.DataStreamPointer += 3; // Move to the next triangle position
	}
	#endregion
}
