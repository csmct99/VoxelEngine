using System.Collections.Generic;

using UnityEditor.AssetImporters;

using UnityEngine;

using VoxelEngine.DataGenerators;

namespace VoxelEngine.Importers
{
	[ScriptedImporter(1, "vox")]
	public class VoxImporter : ScriptedImporter
	{
		#region Public Methods
		public override void OnImportAsset(AssetImportContext ctx)
		{
			Debug.Log("Importing vox file: " + ctx.assetPath);

			GameObject voxelObject = CreateVoxelObject(ctx);
		}
		#endregion

		#region Private Methods
		private GameObject CreateVoxelObject(AssetImportContext ctx)
		{
			GameObject voxelMeshGameObject = new("Voxel Mesh");

			BinaryGreedyVoxelMeshGenerator voxelMeshGenerator = new(); // Meshing
			VoxFileVoxelDataGenerator voxFileGenerator = new();        // Data
			voxFileGenerator.Path = ctx.assetPath;

			voxFileGenerator.ReadFile();
			int voxelDataWidth = Mathf.Max(voxFileGenerator.LargestDimension, 32);

			VoxelMesh voxelMesh = voxelMeshGameObject.AddComponent<VoxelMesh>(); // Meshing game object setup

			//Add a material
			// Use the unity standard shader when we have no material assigned
			Material material = new(Shader.Find("Standard"));
			ctx.AddObjectToAsset("Material", material);

			voxelMesh.MasterMaterial = material;

			// Generate the mesh
			Mesh[] meshes;

			float[] entireVoxelData = voxFileGenerator.GenerateData(voxelDataWidth);
			List<GameObject> meshGameObjects = voxelMesh.GenerateEntireMesh(voxelMeshGenerator, entireVoxelData, voxelDataWidth, 32, out meshes);

			// Add all our generated assets to the file.
			foreach (GameObject meshGameObject in meshGameObjects)
			{
				ctx.AddObjectToAsset(meshGameObject.name, meshGameObject);
			}

			for (int i = 0; i < meshes.Length; i++)
			{
				Mesh mesh = meshes[i];
				ctx.AddObjectToAsset(mesh.name + i, mesh);
			}

			// Add the main object
			ctx.AddObjectToAsset("Voxel Mesh", voxelMeshGameObject);
			ctx.SetMainObject(voxelMeshGameObject);

			return voxelMeshGameObject;
		}
		#endregion
	}
}
