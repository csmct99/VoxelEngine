using UnityEditor;

using UnityEngine;

namespace VoxelEngine.Attributes
{
	// A custom readonly attribute is needed so that I can use this in the importer UI without a custom serializer
	public class ReadOnlyAttribute : PropertyAttribute { }
}
