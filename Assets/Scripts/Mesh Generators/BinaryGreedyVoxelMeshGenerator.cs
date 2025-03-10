using System;

using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Rendering;

using VoxelEngine;

public class BinaryGreedyVoxelMeshGenerator : IVoxelMeshGenerator
{
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
			MeshData.SetVertexBufferParams(DataStreamPointer, new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0), //TODO: Reconsider these data types, this is way more than I need
				new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1), new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 2));
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
		_voxelDataWidth = 32; // In binary data, the width is always 32 for int32

		return WriteToMesh(data, ref meshData);
	}
	#endregion

	#region Private Methods
	private Mesh WriteToMesh(float[] data, ref Mesh.MeshData meshData)
	{
		//TODO: Initializing such a potentially huge data set is not ideal. This should be done in a more dynamic way. Maybe I swap to using Lists in the end.
		int arraySize = 3 * 6 * 2 * _voxelDataWidth * _voxelDataWidth * _voxelDataWidth; // 36 triangles per cube

		meshData.SetIndexBufferParams(arraySize, IndexFormat.UInt32);
		meshData.SetVertexBufferParams(arraySize, new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0), //TODO: Reconsider these data types, this is way more than I need
			new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1), new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 2));

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
	/// Convert a local sliced position on a plane to voxel coordinates
	/// </summary>
	/// <returns> The voxel coordinates </returns>
	/// <exception cref="ArgumentException"> Thrown when an invalid axis is provided </exception>
	private Vector3Int ConvertAxisSlicePositionToVoxelCoord(PlaneAxis axis, int localX, int localY, int layerDepth)
	{
		return axis switch
		{
			PlaneAxis.YZ => new Vector3Int(layerDepth, localY, localX),
			PlaneAxis.XZ => new Vector3Int(localX, layerDepth, localY),
			PlaneAxis.XY => new Vector3Int(localX, localY, layerDepth),
			_ => throw new ArgumentException("Invalid axis provided.", nameof(axis))
		};
	}

	/// <summary>
	/// Returns the voxel data as a 2D grid of true/false values that represent the visibility of a face. If true, a face
	/// should be drawn here.
	/// The data is stored in a 32 bit integer array, where each bit represents a voxel's face visibility along the given axis.
	/// A given int should be interpreted as binary. For example, 0b_0000_0000_0000_0000_0000_0000_0000_0111 would mean only
	/// the first 3 faces are visible.
	/// The first bit represents the first voxel in the column, the second bit represents the second voxel in the column, and
	/// so on.
	/// The intent is for this data to be used to do efficent binary greedy meshing.
	/// </summary>
	/// <param name="voxelData"> The voxel data to convert </param>
	/// <param name="axis"> The axis to convert the data along </param>
	/// <param name="depth"> The depth of the slice to convert </param>
	/// <returns> The binary data </returns>
	/// <exception cref="ArgumentException"></exception>
	private uint[] ConvertVoxelSliceToBinaryFaceSlice(float[] voxelData, PlaneAxis axis, int depth, int depthDirection = 1)
	{
		const int sliceSize = 32; // Leaving this here to help with later potential optimizations (LODs mainly)

		if (voxelData.Length != 32768) // 32 * 32 * 32
		{
			throw new ArgumentException("Voxel data must be 32x32x32", nameof(voxelData));
		}

		uint[] facesBinaryData = new uint[sliceSize];

		// Traverse the local slice space
		for (int localX = 0; localX < sliceSize; localX++) //We currently only allow 32x32x32 voxel data.
		{
			for (int localY = 0; localY < sliceSize; localY++)
			{
				Vector3Int voxelCoord = ConvertAxisSlicePositionToVoxelCoord(axis, localX, localY, depth);
				Vector3Int nextVoxelCoord = ConvertAxisSlicePositionToVoxelCoord(axis, localX, localY, depth + depthDirection);
				bool isVoxelSolid = voxelData.IsVoxelSolid(voxelCoord.x, voxelCoord.y, voxelCoord.z, sliceSize);
				bool isNextVoxelSolid = voxelData.IsVoxelSolid(nextVoxelCoord.x, nextVoxelCoord.y, nextVoxelCoord.z, sliceSize);

				bool isFaceVisible = isVoxelSolid && !isNextVoxelSolid;

				uint bitToPush = isFaceVisible ? 0b_0001u : 0b_0000u;

				/*
				 * we are pushing 0b_0001 or 0b_0000 to the binary data at the localY offset
				 * If it was 0b_0101, and we are pushing 0b_0001 at the 2nd bit, it will become 0b_0111
				 */
				facesBinaryData[localX] |= bitToPush << localY;
			}
		}

		return facesBinaryData;
	}

	/// <summary>
	/// Using the greedy meshing algorithm, encode a chunk of voxels into the mesh buffer. All Top, Bottom, Right, Left, Front,
	/// Back faces.
	/// </summary>
	private void GreedyEncodeVoxelMesh(ref MeshBuffer buffer, float[] data)
	{
		//Encode each face into the mesh buffer
		BinaryGreedyEncodePlane(ref buffer, Vector3Int.zero, data, PlaneAxis.YZ, true); // +X (Right)
		BinaryGreedyEncodePlane(ref buffer, Vector3Int.zero, data, PlaneAxis.XZ, true); // +Y (Top)
		BinaryGreedyEncodePlane(ref buffer, Vector3Int.zero, data, PlaneAxis.XY, true); // +Z (Front)

		BinaryGreedyEncodePlane(ref buffer, Vector3Int.zero, data, PlaneAxis.YZ, false); // -X (Left)
		BinaryGreedyEncodePlane(ref buffer, Vector3Int.zero, data, PlaneAxis.XZ, false); // -Y (Bottom)
		BinaryGreedyEncodePlane(ref buffer, Vector3Int.zero, data, PlaneAxis.XY, false); // -Z (Back)
	}

	/// <summary>
	/// Using the binary greedy meshing algorithm, encode a plane of voxels into the mesh buffer
	/// </summary>
	/// <param name="buffer"> The mesh buffer to encode the plane into </param>
	/// <param name="offset"> The offset to apply to the plane vertices </param>
	/// <param name="chunkData"> The voxel data to encode </param>
	/// <param name="axis"> The axis to encode the plane along </param>
	/// <param name="forward"> Should the plane be encoded in the forward direction? </param>
	private void BinaryGreedyEncodePlane(ref MeshBuffer buffer, Vector3Int offset, float[] chunkData, PlaneAxis axis, bool forward)
	{
		for (int depth = 0; depth < _voxelDataWidth; depth++)
		{
			uint[] facesBinaryData = ConvertVoxelSliceToBinaryFaceSlice(chunkData, axis, depth, forward ? 1 : -1);

			for (int localX = 0; localX < _voxelDataWidth; localX++) //Row
			{
				int localY = 0;
				while (localY < _voxelDataWidth) //Column
				{
					// Example: 0b_...0000_1100
					// We skip the end 2 bits and save to y that we have skipped these bits.
					localY += math.tzcnt(facesBinaryData[localX] >> localY); // Skip empty voxels

					bool hitEnd = localY >= _voxelDataWidth;
					if (hitEnd)
					{
						break;
					}

					// Find the height of the current set of 1s by flipping them to 0s and counting the trailing zeros
					int height = math.tzcnt(~(facesBinaryData[localX] >> localY)); // Find the height of the current face

					// Create a mask to extract the bits we are interested in. The mask is a series of 1s that is "height" long. Ex: 0b_...0000_0111 for height 3
					uint maskNoOffset = height >= 32 ? uint.MaxValue : uint.MaxValue >> (32 - height);

					// Shift the mask to the correct position 
					uint maskOffset = maskNoOffset << localY; //0b_...0111_0000 for height 3 at y 4

					int width = 1;
					while (localX + width < _voxelDataWidth)
					{
						// Check if the next column has the same height
						//Get the column data but only starting at the bits we are interested in
						uint nextColumn = facesBinaryData[localX + width] >> localY; // Example: 0b_...0111_1100 -> 0b_...????_0111
						uint nextColumnMasked = nextColumn & maskNoOffset;           // Example: 0b_...????_0111 & 0b_...0000_0111 = 0b_...0000_0111  // '?' can be 1 or 0 but since we are doing an AND operation, it will be 0 since the mask is 0 

						// If the this 'next' column doesnt matches the mask, we cant extend the width to this column
						bool nextColumnMatches = nextColumnMasked == maskNoOffset; // Example: 0b_...0000_0111 == 0b_...0000_0111
						if (!nextColumnMatches)
						{
							break;
						}

						// We can extend the width to this column
						// Start by removing these bits from the next column
						facesBinaryData[localX + width] &= ~maskOffset; // The inverse of the mask AND'd with the next column will remove the bits we just used

						// Try another column
						width++;
					}

					int xSize = width;
					int ySize = height;

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

					localY += height;
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
