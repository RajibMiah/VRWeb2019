using UnityEngine;
using System.Collections;

[System.Serializable]
public class TerrainScriptable : ScriptableObject
{
	public Vector3 position = Vector3.zero;
	public Terrain.MaterialType terrainMat = Terrain.MaterialType.BuiltInLegacyDiffuse;
	public TerrainData data;	
	public string terrainName = "";
	public string terrainDataName = "";
}
