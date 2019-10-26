using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public static class Presets 
{
	public static PresetsScriptable CreatePresets()
	{
		
		PresetsScriptable scripObj;

		scripObj = (PresetsScriptable)AssetDatabase.LoadAssetAtPath(DataPath.pathStart + DataPath.pathPresets + DataPath.presetsName, typeof(PresetsScriptable));

		if(scripObj == null)
		{			
			scripObj = ScriptableObject.CreateInstance<PresetsScriptable>();			

			AssetDatabase.CreateAsset(scripObj, DataPath.pathStart + DataPath.pathPresets + DataPath.presetsName);

			scripObj.presetsNames.Add("Preset_Default");
			EditorUtility.SetDirty( scripObj );				

			AssetDatabase.SaveAssets();
		}
		

		return scripObj;
	}

	public static PresetsScriptable GetPresets()
	{
		PresetsScriptable scripObj = (PresetsScriptable)AssetDatabase.LoadAssetAtPath(DataPath.pathStart + DataPath.pathPresets + DataPath.presetsName, typeof(PresetsScriptable));
		return scripObj;
	}

}
