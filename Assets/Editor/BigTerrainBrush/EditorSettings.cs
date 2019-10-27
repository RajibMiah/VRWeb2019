using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public static class EditorSettings 
{
	public static EditorScriptable CreateEditorSettings()
	{

		EditorScriptable scripObj;

		scripObj = (EditorScriptable)AssetDatabase.LoadAssetAtPath(DataPath.pathStart + DataPath.pathEditorSettings + DataPath.editorSettingsName, typeof(EditorScriptable));

		if(scripObj == null)
		{			
			scripObj = ScriptableObject.CreateInstance<EditorScriptable>();			

			AssetDatabase.CreateAsset(scripObj, DataPath.pathStart + DataPath.pathEditorSettings + DataPath.editorSettingsName);
			
			EditorUtility.SetDirty( scripObj );				

			AssetDatabase.SaveAssets();
		}


		return scripObj;
	}

	public static EditorScriptable GetEditorSettings()
	{
		EditorScriptable scripObj = (EditorScriptable)AssetDatabase.LoadAssetAtPath(DataPath.pathStart + DataPath.pathEditorSettings + DataPath.editorSettingsName, typeof(EditorScriptable));
		return scripObj;
	}

}
