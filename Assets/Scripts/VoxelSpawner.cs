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

		[Button("Wipe Mesh")]
		public void WipeMesh()
		{
			Cleanup();
		}
		#endregion

		#region Private Methods
		[Button("Regenerate")]
		private void StartFromEditor()
		{
			// Cleanup the old data and generate new ones
			Cleanup();
			_voxelDataGenerator = new Simple3DNoiseVoxelDataGenerator(_voxelDataWidth);
			_voxelMeshGenerator = new BinaryGreedyVoxelMeshGenerator();

			Generate();
		}

		private void Cleanup()
		{
			_voxelMeshGenerator = null;
			_voxelDataGenerator = null;
		}
		#endregion
	}
}
