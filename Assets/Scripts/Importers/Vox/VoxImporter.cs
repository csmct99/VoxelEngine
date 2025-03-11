using UnityEditor.AssetImporters;

using UnityEngine;

using VoxelEngine.DataGenerators;

namespace VoxelEngine.Importers
{
	[ScriptedImporter(1, "vox")]
	public class VoxImporter : ScriptedImporter
	{
		#region Private Fields
		[SerializeField]
		private Material _material;
		#endregion

		#region Public Methods
		public override void OnImportAsset(AssetImportContext ctx)
		{
			Debug.Log("Importing vox file: " + ctx.assetPath);
			CreateVoxelAssetsFromVoxFile(ctx);
		}
		#endregion

		#region Private Methods
		private GameObject CreateVoxelAssetsFromVoxFile(AssetImportContext ctx)
		{
			string fileName = System.IO.Path.GetFileNameWithoutExtension(ctx.assetPath);
			GameObject voxelMeshGameObject = new(fileName); // Main object

			BinaryGreedyVoxelMeshGenerator voxelMeshGenerator = new(); // Meshing
			VoxFileVoxelDataGenerator voxelDataFromFile = new();       // Data
			voxelDataFromFile.Path = ctx.assetPath;
			voxelDataFromFile.ReadFile();

			//Add a material
			if (_material != null)
			{
				ctx.AddObjectToAsset("Material", _material);
			}
			else
			{
				_material = new Material(Shader.Find("Standard"));
				ctx.AddObjectToAsset("Material", _material);
			}

			// Create the meshes and associated game objects
			Mesh[] meshes;
			Vector3Int[] worldPositions;
			GameObject[] gameObjects;

			VoxelMeshFactory.CreateMeshesFromVoxelData(voxelDataFromFile, voxelMeshGenerator, out meshes, out worldPositions);
			gameObjects = VoxelMeshFactory.AssignMeshesToGameObjects(meshes, worldPositions, voxelMeshGameObject.transform, _material);

			// Add all our generated assets to the import data.
			for (int i = 0; i < meshes.Length; i++)
			{
				Mesh mesh = meshes[i];

				bool noVertices = mesh.vertexCount == 0;
				if (noVertices)
				{
					continue;
				}

				ctx.AddObjectToAsset(gameObjects[i].name, gameObjects[i]);
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
