/// <summary>
/// Common interface for all terrain data.
/// This represents creating data for the terrain, not the terrain itself
/// </summary>
public interface IVoxelData
{
	public int Size
	{
		get;
		set;
	}

	public float[,,] Data
	{
		get;
		set;
	}

	public IVoxelData Clone();

	/// <summary>
	/// Whether or not the data contained has been created and is generally ready for use
	/// </summary>
	public bool IsValid
	{
		get;
	}

	public void GenerateData();

	public float GetValue(int x, int y, int z);

	public bool IsEmpty(int x, int y, int z);

	public bool IsSolid(int x, int y, int z);
	
	public bool IsOutOfBounds(int x, int y, int z);
	
	public IVoxelData GetSubData(int x, int y, int z, int size);
}
