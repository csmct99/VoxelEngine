/// <summary>
/// Common interface for all terrain data.
/// This represents creating data for the terrain, not the terrain itself
/// </summary>
public interface ITerrainData
{
	public int Size
	{
		get;
		set;
	}

	public int ChunkSize
	{
		get;
		set;
	}

	public float[,,] Data
	{
		get;
		set;
	}

	public ITerrainData Clone();

	public bool IsValid
	{
		get;
	}

	public void GenerateData();

	public float GetValue(int x, int y, int z);

	public bool IsEmpty(int x, int y, int z);
	public bool IsSolid(int x, int y, int z);
}
