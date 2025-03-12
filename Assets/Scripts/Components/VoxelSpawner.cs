using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.Serialization;

using VoxelEngine;
using VoxelEngine.DataGenerators;

namespace DefaultNamespace
{
	public class VoxelSpawner : MonoBehaviour
	{
		#region Private Fields
		[FormerlySerializedAs("_voxelSpaceSize")]
		[SerializeField]
		[Tooltip("For the data generators that support it, this is the width of the voxel data set to be generated.")]
		[PropertyRange(32, 2048)]
		private int _voxelDataWidth = 32;

		private IVoxelMeshGenerator _voxelMeshGenerator;
		private IVoxelDataGenerator _voxelDataGenerator;

		[SerializeField]
		private Material _material;
		#endregion

		#region Public Methods
		public void Generate()
		{
			// Generate the mesh and put it in the scene
			VoxelMeshFactory.CreateGameobjectsFromMeshData(_voxelDataGenerator, _voxelMeshGenerator, transform, _material);
		}

		[Button("Regenerate")]
		public void StartFromEditor()
		{
			// Cleanup the old data and generate new ones
			Cleanup();
			DestroyChildren();
			
			_voxelDataGenerator = new TerrainVoxelDataGenerator(_voxelDataWidth);
			_voxelMeshGenerator = new BinaryGreedyVoxelMeshGenerator();

			Generate();
		}
		
		[Button("Wipe Mesh")]
		public void WipeMesh()
		{
			DestroyChildren();
			Cleanup();
		}
		#endregion

		#region Private Methods
		private void DestroyChildren()
		{
			Transform[] children = transform.GetComponentsInChildren<Transform>();
			for (int i = 1; i < children.Length; i++) //Skip the first one as its likely the parent
			{
				DestroyImmediate(children[i].gameObject);
			}
		}

		private void Cleanup()
		{
			_voxelMeshGenerator = null;
			_voxelDataGenerator = null;
		}
		#endregion
	}
}
