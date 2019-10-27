using UnityEngine;
using System.Collections;
using UnityEditor;

public class CreateTerrainScriptable : MonoBehaviour {

	public static TerrainScriptable CreateTerrain(int index, string presetName)
	{
		TerrainScriptable scripObj = ScriptableObject.CreateInstance<TerrainScriptable>();

		AssetDatabase.CreateAsset(scripObj, DataPath.pathStart + DataPath.pathScriptableTerrain + "Terrain" + "_" + presetName + "_" + (index+1).ToString() + ".asset");
		AssetDatabase.SaveAssets();

		return scripObj;
	}

	public static TerrainScriptable GetTerrainScriptable(string name)
	{		
		TerrainScriptable scripObj = (TerrainScriptable)AssetDatabase.LoadAssetAtPath(DataPath.pathStart + DataPath.pathScriptableTerrain + name + ".asset", typeof(TerrainScriptable));
		return scripObj;
	}
}