using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


public class TerrainToolboxEditor : EditorWindow 
{
	[MenuItem ("Window/BigTerrainBrush")]

	#region Start

	static void Init () 
	{		
		TerrainToolboxEditor window = (TerrainToolboxEditor)EditorWindow.GetWindow<TerrainToolboxEditor>( false, "TerrainToolBox" );
		window.Show();	
				
	}
	
	void OnEnable()
	{
		icon_Brush1 = (Texture)Resources.Load("TerrainToolbox/Buttons/Brush1");	
		icon_Brush2 = (Texture)Resources.Load("TerrainToolbox/Buttons/Brush2");	
		icon_Brush3 = (Texture)Resources.Load("TerrainToolbox/Buttons/Brush3");	
		icon_Brush4 = (Texture)Resources.Load("TerrainToolbox/Buttons/Brush4");	
		icon_Brush5 = (Texture)Resources.Load("TerrainToolbox/Buttons/Brush5");	
		
		SceneView.onSceneGUIDelegate += this.OnSceneGUI;		
		EditorApplication.update += Update;			
	}

	void OnDisable() 
	{
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		EditorApplication.update += Update;
	}
	#endregion
	
	#region Variables
	
	//Terrains
	public Transform terrainsParent;
	List<GameObject> terrains = new List<GameObject>();
	
	//Field
	public bool showField = false;
	public Vector3 fieldStart = Vector3.zero;
	public Vector3 fieldEnd = new Vector3(500,0,500);
	
	
	//Terrain Data
	float terrainRes = 0;
	float terrainSize = 0;
	float stepSize = 0;											//Size between each point in the field. its based on terrainSize and Res, because stepSize is the measuring unit in whole editor, all Terrains need to have same size and resolution for this unit to work
	Vector2 fieldIndexSize = Vector2.zero;						//How many stepSizes are in the whole field, basicly amount of X and Y steps in whole field
	float terrainPosY = 0.0f;
	float terrainHeightSize = 0.0f;
	
	//Terrain Editor
	private Dictionary<Vector2, BasePoint> basePointsNew = new Dictionary<Vector2, BasePoint>();		//points which represnt terrain heights data in 3d space, there is as many BasePoints as all of the terrain heights in all terrains		
	private Dictionary<Vector2, SculptPoint> sculptPoints = new Dictionary<Vector2, SculptPoint>();		//Sculpt points are created and averaged based on basePoints, their amount is reduced by reduceDensity	
	private List<Dictionary<Vector2, SculptPoint>> sculptPointsHistory = new List<Dictionary<Vector2, SculptPoint>>();

	private Dictionary<Vector2, float> basePointsDifs = new Dictionary<Vector2, float>();	

	private List<Dictionary<Vector2, float>> sculptPointsDeltas = new List<Dictionary<Vector2, float>>(); 	
	
	private List<Vector2> sculptPointsIndexes = new List<Vector2>();	
	List<int> terrainsToEdit = new List<int>();													//make sure we are not updating all terrains GO, only the ones which have data
	float reduceDenisty = 4.0f;
	int reduceDensityInt = 3;	
	
	
	//Mesh Generation
	GameObject meshTerrain;
	MeshFilter meshFilter;
	Mesh mesh;
	Mesh meshCollider;
	Material meshMat;	
	int rectSize = 250;
	float vertCount = 65.0f;
	int targetVertCount = 10000;
	bool convertToMesh = false;

	//List holding all meshes which were generated
	private List<Mesh> meshes = new List<Mesh>();		//way to access it is for example if limit is 5000 and vert index would be 6000 is by 6000 / 5000 = floorToInt => 1 ( index of mesh, of the list ), then do 6000 % 5000 => 1000, thats index in that list for the specific vertex
	private List<List<Vector3>> meshVertsLists = new List<List<Vector3>>();		//All vertices which are needed for each mesh. There is list of those because each mesh has its one list of vertices	
	List<List<int>> meshTrianglesLists = new List<List<int>>();
	List<Dictionary<Vector2, int>> trianglVerts = new List<Dictionary<Vector2, int>>();	
	List<GameObject> sculptGOs = new List<GameObject>();
	List<MeshCollider> meshColliders = new List<MeshCollider>();
	private List<int> meshIndexes = new List<int>();

	float alphaValue = 0.5f;

	enum InterpolationMode
	{
		Smooth,
		Normal,
		Linear
	};
	InterpolationMode interpolationMode = InterpolationMode.Smooth;

	//UI
	float windowWidth = 0.0f;
	float buttonMainSize = 75.0f;
	float buttonSubSize = 70.0f;
	float amountOfButtons = 2.0f;	
	Color originalUIColor;
	
	int selectedMenu = 0;
	
	enum UiMainMode
	{
		CreateTerrains,
		TerrainEditor
	};
	UiMainMode mode = UiMainMode.TerrainEditor;

	Texture icon_Brush1;
	Texture icon_Brush2;
	Texture icon_Brush3;
	Texture icon_Brush4;
	Texture icon_Brush5;

	
	//UI TerrainEditor
	int selectedEditorSubMode = 0;
	
	enum TerrainEditorSubMode
	{
		Settings,
		Edit,
		Spline
	};
	TerrainEditorSubMode terrainSubMode = TerrainEditorSubMode.Settings;
	
	
	//Creator
	int selectedEditorSubModeCreator = 0;

	enum TerrainEditorSubModeCreator
	{
		Settings,
		EditTerrains,
		Presets
	};
	TerrainEditorSubModeCreator terrainSubModeCreator = TerrainEditorSubModeCreator.Settings;
	
	enum TerrainMaterials
	{
		Standard,
		LegacyDiffuse,
		LegacySpecular
	};
	TerrainMaterials terrainMat = TerrainMaterials.LegacyDiffuse;
	
	
	//Settings
	bool disabled = false;
	
	//Brush
	enum BrushType
	{
		Height,
		Smooth,
		SetHeight,
		Flatten
	};
	
	BrushType brushType = BrushType.Height;
	int selectedBrush = 0;
	
	Vector3 finalHitPositionAdjust = Vector3.zero;
	Vector2 indexPositionAdjust = Vector2.zero;
	Vector3 pointAdjust = Vector3.zero;
	Vector2 fieldIndexAdjust = Vector2.zero;
	
	bool mouseDown = false;
	Event sceneEvent;
	Vector2 mousePos;
	float brushSize = 200.0f;
	float brushIntensity = 5.0f;
	float brushIntenstityOne = 1.0f;	//0-1
	float smoothAllIntensity = 1.0f;
	int brushInterpolation = 0;
	Vector3 hitPoint;
	bool autoUpdateMeshCollider = true;
	bool terrainsVisible = true;
	bool sculptVisible = true;
	GameObject sculptMeshParentRef;
	Projector projectorComponent;
	float maxSculptHeight = 0.0f;		//limit sculpting height based on terrains values
	float minSculptHeight = 0.0f;		//limit sculpting height based on terrains values
	float brushDirection = 1.0f;			//scultp down
	bool destroyedProjector = false;
	float setHeightValue = 0.0f;
	bool brushDrag = false;
	string brushDragText = "FREE";
	string[] brushDragTexts = new string[2]{"DRAG", "FREE"};
	bool brushIsRound = true;
	string brushRoundText = "ROUND";
	string[] brushRoundTexts = new string[2]{"ROUND", "SQUARE"};
	float brushOutline = 10.0f;
	bool keepHighResBrush = true;	

	AnimationCurve curve = AnimationCurve.EaseInOut(0,0,1,1); //(0,0,10,10);
	AnimationCurve curveSmooth = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
	
	int undoIndex = 0;

	public Vector2 scrollPosition = Vector2.zero;
	public float requiredUIHeight = 1300;

	//Edit Terrains
	SplatPrototype[] splatPrototypes = new SplatPrototype[]{};
	Texture2D[] terrainTextures = new Texture2D[4]{null, null, null, null};	

	float[] textureSizesX = new float[4]{0,0,0,0};
	float[] textureSizesY = new float[4]{0,0,0,0};
	float[] textureOffsetsX = new float[4]{0,0,0,0};
	float[] textureOffsetsY = new float[4]{0,0,0,0};

	//Pixel Error, Base map distance, cast shadows, Material, Thickness, Details Draw, Control Texture Resolution, Base Texture Resolution
	float terrainData_PixelError = 0;
	float terrainData_BasemapDistance = 0;
	bool terrainData_CastShadows = false;
	Terrain.MaterialType terrainData_Material = 0;
	float terrainData_Thickness = 0;
	bool terrainData_DrawDetails = false;
	int terrainData_ControlTextureRes = 0;
	int terrainData_BaseTextureRes = 0;



	GameObject myProjector;	
	
	//Smoothing
	int smoothingIterations = 1;

	//System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
	Vector3 TestPoint;

	
	#endregion

	#region PresetsVars

	PresetsScriptable presetsObject;
	List<string> presets = new List<string>();
	int presetsIndex = 0;	
	string newPresetName = "";

	#endregion

	#region Splines

	//List<Vector3> splinePoints = new List<Vector3>();
	//List<GameObject> handles = new List<GameObject>();

	SplineObject spline;

	#endregion

	#region EditTerrain
	int defaultTerrainSize = 0;
	int heighmapResolution = 0;
	int defaultTerrainHeight = 0;
	float terrainThickness = 0.0f;
	Vector2 gridDimensions = Vector2.zero;
	Vector3 startPositionGrid = Vector3.zero;
	int nameIndex = 0;

	float defaultPixelError = 5.0f;	//slider 1-200
	int defaultBaseMapDistance = 1000;	//slider 0-2000
	bool defaultCastShadows = false;
	bool defaultProbes = false;
	bool defaultDrawDetail = false;
	int defaultDetailRes = 512;
	int defaultResPerPatch = 8;
	int defaultBaseTextureRes = 1024;
	#endregion



	void Update()
	{
		if(terrainSubMode == TerrainEditorSubMode.Edit && mouseDown && !disabled)
		{
			Brush(ref hitPoint);
			UpdateMeshVerts();
			UpdateMeshes();					
		}
	}	
	
	
	#region GUI Functions
	
	void OnGUI()
	{	
		windowWidth = position.width;
		originalUIColor = GUI.backgroundColor;

		GUILayout.Space(30);

		float width = this.position.width;
		float height = this.position.height;

		scrollPosition = GUI.BeginScrollView(new Rect(0, 0, width, height), scrollPosition, new Rect(0, 0, width - 20, requiredUIHeight), false, false);

		MainUIButtons();
		GuiMode();	

		GUI.EndScrollView();

		SceneView.RepaintAll();
	}
	
	
	void MainUIButtons()
	{
		EditorGUI.BeginChangeCheck();

		GUILayout.BeginHorizontal();
		GUILayout.Space(windowWidth / 2.0f - (buttonMainSize * amountOfButtons / 2.0f));
		selectedMenu = GUILayout.Toolbar(selectedMenu, new string[2]{"Create\nTerrains", "Terrain\nEditor"}, GUILayout.Width(buttonMainSize * amountOfButtons), GUILayout.Height(buttonMainSize)/*, GUILayout.Space(windowWidth / 2.0f - (buttonMainSize * amountOfButtons / 2.0f))*/);
		mode = (UiMainMode)selectedMenu;
		GUILayout.EndHorizontal();

		if(EditorGUI.EndChangeCheck())
		{
			if(mode == UiMainMode.CreateTerrains)
			{
				if(terrainsParent != null)
				{
					terrainsParent.gameObject.SetActive(true);
					terrainsVisible = true;
					//disabled = true;
					HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

					if(sculptMeshParentRef != null)
					{
						sculptMeshParentRef.gameObject.SetActive(false);
						sculptVisible = false;
					}
				}
			}
			else
			{
				//disabled = false;
			}
		}

		GUILayout.Space(30);		
	}
	
	//Main UI Toolbar, main mode selection
	void GuiMode()
	{
		switch(mode)
		{
			case UiMainMode.CreateTerrains:
				ShowCreateTerrainsMain();				
				break;
			case UiMainMode.TerrainEditor:			
				ShowEditorMain();
				break;
			default:
			break;
		}		
	}
	
	//User is in Terrain Editor mode, select submode
	void GuiModeTerrainEditor()
	{
		switch(terrainSubMode)
		{
		case TerrainEditorSubMode.Settings:
			ShowEditorSettings();
			break;
		case TerrainEditorSubMode.Edit:
			ShowBrushSettings();			
			break;
		case TerrainEditorSubMode.Spline:	
			ShowSplineSettings();
			break;
		}
	}

	void ShowEditorMain()
	{		
		GUILayout.BeginHorizontal();
		GUILayout.Space(windowWidth / 2.0f - (buttonSubSize * 2.0f / 2.0f));
		selectedEditorSubMode = GUILayout.Toolbar(selectedEditorSubMode, new string[2]{"Settings", "Edit"}, GUILayout.Width(buttonSubSize * 2), GUILayout.Height(buttonSubSize * 0.8f));
		terrainSubMode = (TerrainEditorSubMode)selectedEditorSubMode;
		GUILayout.EndHorizontal();
		
		GuiModeTerrainEditor();			
	}
	
	void ShowEditorSettings()
	{
		GUILayout.Space(20);

		terrainsParent = (Transform)EditorGUILayout.ObjectField("PARENT", terrainsParent, typeof(Transform), true);	
		
		GUILayout.Space(20);
		
		//SHOW FIELD
		GUILayout.BeginHorizontal();
		float centerButton = windowWidth / 2.0f - (170.0f / 2.0f);
		GUILayout.Space(centerButton);
		GUI.backgroundColor = (showField) ? Color.green : Color.red;

		string hideString = showField ? "FIELD VISIBLE" : "FIELD HIDDEN";
		if(GUILayout.Button(hideString, GUILayout.Width(170), GUILayout.Height(50)))
		{
			showField = !showField;			
		}
		GUILayout.EndHorizontal();
		GUI.backgroundColor = originalUIColor;	
		
		GUILayout.Space(30);


		convertToMesh = EditorGUILayout.Toggle("Vert Target", convertToMesh);
		GUILayout.Space(5);

		if(!convertToMesh)
		{
			reduceDensityInt = EditorGUILayout.IntSlider("REDUCE DENSITY", reduceDensityInt, 1, 30);//EditorGUILayout.IntField("Density Reduction", reduceDensityInt);
		}
		else
		{
			targetVertCount = EditorGUILayout.IntField("Target Vert Count", targetVertCount);

			int originalVertCount = heighmapResolution * heighmapResolution;
			float reduceTarget = (float)originalVertCount / (float)(targetVertCount*1.5f);
			reduceDensityInt = (int)reduceTarget;
		}

		reduceDenisty = (float)reduceDensityInt;
		GUILayout.Space(5);
		interpolationMode = (InterpolationMode)EditorGUILayout.EnumPopup("Interpolation Mode", interpolationMode);
		if(interpolationMode == InterpolationMode.Normal)
		{
			GUILayout.Space(5);
			alphaValue = EditorGUILayout.Slider("Interpolation", alphaValue, 0.0f, 1.0f);
		}
		GUILayout.Space(5);



		EditorGUI.BeginChangeCheck();
		vertCount = EditorGUILayout.Slider("Verts mesh size", vertCount, 0.1f, 65.0f);

		if(EditorGUI.EndChangeCheck())
		{
			float tempVertCount = vertCount * 1000.0f;
			float tempRectCount = Mathf.Sqrt(tempVertCount);
			int rectCountInt = Mathf.FloorToInt(tempRectCount);
			rectSize = rectCountInt;
		}

		GUILayout.Space(10);


		GUILayout.Space(30);
		GUILayout.BeginHorizontal();
		float center = windowWidth / 2.0f - (170.0f / 2.0f);
		float posLeft = windowWidth / 2.0f - 200.0f;
		float posRight = 60;

		GUILayout.Space(posLeft);
		if(GUILayout.Button("PRE-PROCESS", GUILayout.Width(170), GUILayout.Height(50)))
		{
			if(terrainsParent == null)
			{
				EditorUtility.DisplayDialog("PARENT OBJECT MISSING", "Please assign parent object first", "OK");
			}
			else
			{
				InitializeEditor();
			}
		}

		GUILayout.Space(posRight);
		if(GUILayout.Button("EDIT", GUILayout.Width(170), GUILayout.Height(50)))
		{
			if(terrainsParent == null)
			{
				EditorUtility.DisplayDialog("PARENT OBJECT MISSING", "Please assign parent object first", "OK");
			}
			else
			{
				EditTerrain();

				terrainsParent.gameObject.SetActive(false);
				terrainsVisible = false;
				sculptMeshParentRef.gameObject.SetActive(true);
				sculptVisible = true;
				showField = false;	
			}
		}
		GUILayout.EndHorizontal();

		GUILayout.Space(50);



		GUILayout.BeginHorizontal();
		GUILayout.Space(center);
		if(GUILayout.Button("PROJECT BACK", GUILayout.Width(170), GUILayout.Height(50)))
		{
			if(terrainsParent == null)
			{
				EditorUtility.DisplayDialog("PARENT OBJECT MISSING", "Please assign parent object first", "OK");
			}
			else
			{
				//890,750
				//230,214
				CalculateSculptDeltas();
				//480 - 189
				ProjectBackToBasePoints();
				//240
				ProjectBackToTerrain();

				terrainsParent.gameObject.SetActive(true);
				terrainsVisible = true;
				sculptMeshParentRef.gameObject.SetActive(false);
				sculptVisible = false;
			}
		}
		GUILayout.EndHorizontal();

		GUILayout.Space(50);
		GUILayout.BeginHorizontal();
		float cen = windowWidth / 2.0f - (100.0f / 2.0f);
		GUILayout.Space(cen);
		if(GUILayout.Button("DELETE", GUILayout.Width(100), GUILayout.Height(30)))
		{
			Delete();
		}
		GUILayout.EndHorizontal();
	}
	
	void ShowBrushSettings()
	{
		GUILayout.Space(50);	
		
		float brushButtonSize = 70.0f;
		int amountOfBrushButtons = System.Enum.GetValues(typeof(BrushType)).Length;

		GUILayout.BeginHorizontal();
		GUILayout.Space(windowWidth / 2.0f - (brushButtonSize * amountOfBrushButtons / 2.0f));
		selectedBrush = GUILayout.Toolbar(selectedBrush, new string[4]{"Height", "Smooth", "SetHeight", "Flatten"}, GUILayout.Width(brushButtonSize * amountOfBrushButtons), GUILayout.Height(brushButtonSize));
		brushType = (BrushType)selectedBrush;
		GUILayout.EndHorizontal();
				
		GUILayout.Space(20);		



		EditorGUI.BeginChangeCheck();

		brushSize = EditorGUILayout.FloatField("Brush Size", brushSize);

		if(EditorGUI.EndChangeCheck())
		{
			brushSize = Mathf.Clamp(brushSize, 1.0f, 10000.0f);

			if(projectorComponent != null)
			{
				projectorComponent.orthographicSize = brushSize + brushOutline;
			}
		}

			
		GUILayout.Space(10);
		
		if(brushType == BrushType.Height)
		{			
			brushIntensity = EditorGUILayout.Slider("Brush Intensity", brushIntensity, 0.0f, 100.0f);
		}
		else if(brushType == BrushType.Smooth || brushType == BrushType.SetHeight || brushType == BrushType.Flatten)
		{		
			brushIntenstityOne = EditorGUILayout.Slider("Brush Intensity", brushIntenstityOne, 0.0f, 1.0f);
		}

				
		if(brushType == BrushType.SetHeight)
		{		
			setHeightValue = EditorGUILayout.Slider("Set Height", setHeightValue, 0.0f, terrainHeightSize);
									
			GUILayout.Space(20);
			
			GUILayout.BeginHorizontal();
			float centerButton1 = windowWidth / 2.0f - (170.0f / 2.0f);
			GUILayout.Space(centerButton1);
			if(GUILayout.Button("Set Height All", GUILayout.Width(170), GUILayout.Height(50)))
			{
				SetHeight();
			}
			GUILayout.EndHorizontal();
		}


		//Keep High Res
		GUILayout.Space(20);
		GUILayout.BeginHorizontal();
		float centerButton4 = windowWidth / 2.0f - (170.0f / 2.0f);
		GUILayout.Space(centerButton4);

		GUI.backgroundColor = (keepHighResBrush) ? Color.green : Color.red;

		string hideString2 = keepHighResBrush ? "DETAILS ON" : "DETAILS OFF";
		if(GUILayout.Button(hideString2, GUILayout.Width(170), GUILayout.Height(50)))
		{
			keepHighResBrush = !keepHighResBrush;
		}
		GUILayout.EndHorizontal();
		GUI.backgroundColor = originalUIColor;


		GUILayout.Space(20);
		GUILayout.BeginHorizontal();
		GUILayout.Space(windowWidth / 2.0f - (180.0f / 2.0f));
		if(GUILayout.Button (brushRoundText, GUILayout.Width (80), GUILayout.Height (40))) 
		{
			brushIsRound = !brushIsRound;
			brushRoundText = (brushIsRound) ? brushRoundTexts[0] : brushRoundTexts[1];
			ProjectorBrush();
		}

		GUILayout.Space(20);

		if(GUILayout.Button (brushDragText, GUILayout.Width (80), GUILayout.Height (40))) 
		{
			brushDrag = !brushDrag;
			brushDragText = (brushDrag) ? brushDragTexts[0] : brushDragTexts[1];
		}

		GUILayout.EndHorizontal();

				

		GUILayout.Space(20);	
		
		int typeButtonSize = 50;

		GUILayout.BeginHorizontal();	
		GUILayout.Space(windowWidth / 2.0f - (typeButtonSize * 5 / 2.0f));

		EditorGUI.BeginChangeCheck();
		brushInterpolation = GUILayout.Toolbar(brushInterpolation, new Texture[5]{icon_Brush1, icon_Brush2, icon_Brush3, icon_Brush4, icon_Brush5}, GUILayout.Width(typeButtonSize * 5), GUILayout.Height(typeButtonSize)); //Toolbar(brushInterpolation, new string[5]{"1", "2", "3", "4", "5"}, GUILayout.Width(typeButtonSize * 5), GUILayout.Height(typeButtonSize));
		if (EditorGUI.EndChangeCheck())
		{
			switch(brushInterpolation)
			{
			case 0:
				curve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
				break;
			case 1:
				curve.keys = new Keyframe[2]{new Keyframe(0.0f, 0.0f, 0.0f, 2.0f), new Keyframe(1.0f, 1.0f, 0.0f, 0.0f)}; 
				break;
			case 2:
				curve.keys = new Keyframe[2]{new Keyframe(0.0f, 0.0f, 0.0f, 0.0f), new Keyframe(1.0f, 1.0f, 2.0f, 0.0f)};
				break;
			case 3:
				curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
				break;
			case 4:
				curve.keys = new Keyframe[3]{new Keyframe(0.0f, 0.0f, 0.0f, 0.0f), new Keyframe(0.0f, 1.0f, 0.0f, 0.0f), new Keyframe(1.0f, 1.0f, 0.0f, 0.0f)};
				break;
			}
		}

		GUILayout.EndHorizontal();	


		GUILayout.Space(20);
		GUILayout.BeginHorizontal();	
		GUILayout.Space(windowWidth / 2.0f - (80.0f / 2.0f));				
		curve = EditorGUILayout.CurveField("", curve, GUILayout.Width(80), GUILayout.Height(40));
		GUILayout.EndHorizontal();
		
		//Terrain Visible
		GUILayout.Space(50);
		GUILayout.BeginHorizontal();
		float centerButton = windowWidth / 2.0f - (170.0f / 2.0f);
		GUILayout.Space(centerButton);
		
		GUI.backgroundColor = (terrainsVisible) ? Color.green : Color.red;
		
		string hideString = terrainsVisible ? "TERRAIN VISIBLE" : "TERRAIN HIDDEN";
		if(GUILayout.Button(hideString, GUILayout.Width(170), GUILayout.Height(50)))
		{
			terrainsVisible = !terrainsVisible;
			terrainsParent.gameObject.SetActive(terrainsVisible);
		}
		GUILayout.EndHorizontal();
		GUI.backgroundColor = originalUIColor;
		
		
		//Sculpt Visible
		GUILayout.Space(20);
		GUILayout.BeginHorizontal();		
		GUILayout.Space(centerButton);

		GUI.backgroundColor = (sculptVisible) ? Color.green : Color.red;

		string hideStringSculpt = sculptVisible ? "SCULPT VISIBLE" : "SCULPT HIDDEN";
		if(GUILayout.Button(hideStringSculpt, GUILayout.Width(170), GUILayout.Height(50)))
		{
			sculptVisible = !sculptVisible;
			sculptMeshParentRef.gameObject.SetActive(sculptVisible);
		}
		GUILayout.EndHorizontal();
		GUI.backgroundColor = originalUIColor;
		
			
		//Update Mesh colliders
		GUILayout.Space(20);		
		GUILayout.BeginHorizontal();
		
		float posCenter = windowWidth / 2.0f - (170.0f / 2.0f);
		float posLeft = windowWidth / 2.0f - 200.0f;
		float posRight = 60;
		
		if(autoUpdateMeshCollider)
		{
			GUILayout.Space(posCenter);
		}
		else
		{
			GUILayout.Space(posLeft);
		}
		
		GUI.backgroundColor = (autoUpdateMeshCollider) ? Color.green : Color.red;
		
		string onOff = autoUpdateMeshCollider ? "ON" : "OFF";
		if(GUILayout.Button("AUTO UPDATE\nMESH COLLIDER\n\n" + onOff, GUILayout.Width(170), GUILayout.Height(70)))
		{
			autoUpdateMeshCollider = !autoUpdateMeshCollider;
		}		
		
		
		if(!autoUpdateMeshCollider)
		{
			GUILayout.Space(posRight);
			GUI.backgroundColor = Color.green;
			
			if(GUILayout.Button("UPDATE MESH COLLIDER", GUILayout.Width(170), GUILayout.Height(70)))
			{
				ResetColliders();
			}	
		}		
		
		GUILayout.EndHorizontal();
		GUI.backgroundColor = originalUIColor;
		
		GUILayout.Space(30);		
		smoothingIterations = EditorGUILayout.IntField("Smooth Iterations", smoothingIterations);
		smoothingIterations = Mathf.Clamp(smoothingIterations, 1, 50);
		GUILayout.Space(10);
		smoothAllIntensity = EditorGUILayout.Slider("Smooth Intensity", smoothAllIntensity, 0.0f, 1.0f);
		GUILayout.Space(10);


		GUILayout.BeginHorizontal();
		GUILayout.Space(posCenter);
		GUI.backgroundColor = Color.green;

		if(GUILayout.Button("SMOOTH ALL", GUILayout.Width(170), GUILayout.Height(50)))
		{
			SmoothSculptIterations();
		}
		GUILayout.EndHorizontal();
		GUI.backgroundColor = originalUIColor;
		
		GUILayout.Space(30);
	
				
		
		
		///////////////
		
		if(undoIndex >= sculptPointsDeltas.Count - 1)
		{
			GUI.backgroundColor = Color.grey;
		}
		else
		{
			GUI.backgroundColor = originalUIColor;
		}
		
		//Undo works in a way where latest state is undoIndex = 0, as undo goes more into history, its index is increased i.e. 1,2,3 etc.
		GUILayout.Space(50);
		GUILayout.BeginHorizontal();
		GUILayout.Space(posLeft);
		
		if(GUILayout.Button("UNDO", GUILayout.Width(170), GUILayout.Height(40)))
		{
			undoIndex++;							
			
			if(undoIndex > sculptPointsDeltas.Count - 1)
			{
				undoIndex = sculptPointsDeltas.Count - 1;
			}
			else
			{				
				foreach(KeyValuePair<Vector2, float> tempPair in sculptPointsDeltas[undoIndex])
				{
					Vector3 tempPos = sculptPoints[tempPair.Key].position;
					sculptPoints[tempPair.Key].position =  new Vector3(tempPos.x, tempPos.y - tempPair.Value, tempPos.z);										
				}

				UpdateMeshVertsAll();
				UpdateMeshesAll();
				ResetCollidersEditing();
			}
			
			
		}
		
		if(undoIndex <= 0)
		{
			GUI.backgroundColor = Color.grey;
		}
		else
		{
			GUI.backgroundColor = originalUIColor;
		}
		
		
		GUILayout.Space(posRight);
		
		if(GUILayout.Button("REDO", GUILayout.Width(170), GUILayout.Height(40)))
		{		
			if(undoIndex >= 0 && sculptPointsDeltas.Count > 0)
			{				
				foreach(KeyValuePair<Vector2, float> tempPair in sculptPointsDeltas[undoIndex])
				{
					Vector3 tempPos = sculptPoints[tempPair.Key].position;
					sculptPoints[tempPair.Key].position =  new Vector3(tempPos.x, tempPos.y + tempPair.Value, tempPos.z);										
				}

				UpdateMeshVertsAll();
				UpdateMeshesAll();
				ResetCollidersEditing();
			}

			undoIndex--;
			
			if(undoIndex < 0)
			{
				undoIndex = 0;
			}
		}
		
		GUILayout.EndHorizontal();
		GUI.backgroundColor = originalUIColor;
		
	}
	
	void ShowSplineSettings()
	{
		GUILayout.Space(50);

		GUILayout.BeginHorizontal();
		float centerButtonAddPoint = windowWidth / 2.0f - (170.0f / 2.0f);
		GUILayout.Space(centerButtonAddPoint);
		if(GUILayout.Button("Add Point", GUILayout.Width(170), GUILayout.Height(50)))
		{
			AddSplinePoint();
		}
		GUILayout.EndHorizontal();
	}
	
	
	void ShowCreateTerrainsMain()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(windowWidth / 2.0f - (buttonSubSize * 3.0f / 2.0f));
		selectedEditorSubModeCreator = GUILayout.Toolbar(selectedEditorSubModeCreator, new string[3]{"Settings", "Edit", "Presets"}, GUILayout.Width(buttonSubSize * 3.0f), GUILayout.Height(buttonSubSize * 0.8f));
		terrainSubModeCreator = (TerrainEditorSubModeCreator)selectedEditorSubModeCreator;
		GUILayout.EndHorizontal();

		GuiModeTerrainCreator();	
	}
	
	//User is in Terrain Editor mode, select submode
	void GuiModeTerrainCreator()
	{
		switch(terrainSubModeCreator)
		{
		case TerrainEditorSubModeCreator.Settings:
			ShowCreatorSettings();			
			break;
		case TerrainEditorSubModeCreator.EditTerrains:
			ShowEditTerrainsData();			
			break;
		case TerrainEditorSubModeCreator.Presets:
			ShowPresetsSettings();			
			break;
			default:
			break;
		}
	}
	


	
	void ShowCreatorSettings()
	{
		GUILayout.Space(20);		
		gridDimensions = EditorGUILayout.Vector2Field("GRID DIMENSIONS", gridDimensions);
		GUILayout.Space(10);		
		startPositionGrid = EditorGUILayout.Vector3Field("START POSITION", startPositionGrid);
		
		
		GUILayout.Space(30);
		terrainsParent = (Transform)EditorGUILayout.ObjectField("PARENT", terrainsParent, typeof(Transform), true);		

		GUILayout.Space(20);		
		defaultTerrainSize = EditorGUILayout.IntField("Terrain Size", defaultTerrainSize);					
		defaultTerrainHeight = EditorGUILayout.IntField("Terrain Height", defaultTerrainHeight);
		heighmapResolution = EditorGUILayout.IntField("Heighmap Resolution", heighmapResolution);				
		terrainThickness = EditorGUILayout.FloatField("Terrain Thickness", terrainThickness);			
		terrainMat = (TerrainMaterials)EditorGUILayout.EnumPopup("Terrain Material", terrainMat);

		defaultPixelError = EditorGUILayout.Slider("Pixel Error", defaultPixelError, 1.0f, 200.0f);
		defaultBaseMapDistance = EditorGUILayout.IntSlider("Base Map Distance", defaultBaseMapDistance, 0, 2000);
		defaultCastShadows = EditorGUILayout.Toggle("Cast Shadows", defaultCastShadows);
		defaultProbes = EditorGUILayout.Toggle("Probes On", defaultProbes);
		defaultDrawDetail = EditorGUILayout.Toggle("Draw Details", defaultDrawDetail);
		defaultDetailRes = EditorGUILayout.IntField("Detail Resolution", defaultDetailRes);
		defaultResPerPatch = EditorGUILayout.IntField("Detail Resolution Per Patch", defaultResPerPatch);
		defaultBaseTextureRes = EditorGUILayout.IntField("Base Texture Resolution", defaultBaseTextureRes);


		GUILayout.Space(20);		
		nameIndex = EditorGUILayout.IntField("Name Index", nameIndex);
		
		
		GUILayout.Space(40);		
		float posCenter = windowWidth / 2.0f - (170.0f / 2.0f);	
		GUI.backgroundColor = Color.green;

		GUILayout.BeginHorizontal();
		GUILayout.Space(posCenter);
		if(GUILayout.Button("CREATE TERRAINS", GUILayout.Width(170), GUILayout.Height(50)))
		{
			if(terrainsParent == null)
			{
				EditorUtility.DisplayDialog("PARENT OBJECT MISSING", "Please assign parent object first", "OK");
			}
			else
			{
				CreateTerrain();
			}
		}	
		GUILayout.EndHorizontal();
		GUI.backgroundColor = originalUIColor;

		GUILayout.Space(20);	


		GUILayout.BeginHorizontal();
		GUILayout.Space(posCenter);
		if(GUILayout.Button("SET NEIGHBOURS", GUILayout.Width(170), GUILayout.Height(50)))
		{
			if(terrainsParent == null)
			{
				EditorUtility.DisplayDialog("PARENT OBJECT MISSING", "Please assign parent object first", "OK");
			}
			else
			{
				SetNeighbours();
			}
		}	
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Space(posCenter);
		if(GUILayout.Button("DISABLED " + ((disabled) ? "ON" : "OFF"), GUILayout.Width(170), GUILayout.Height(50)))
		{
			disabled = !disabled;
		}	
		GUILayout.EndHorizontal();



	}

	#endregion

	#region EditTerrainData

	void ShowEditTerrainsData()
	{
		requiredUIHeight = 800;

		float posCenter = windowWidth / 2.0f - (170.0f / 2.0f);	

		GUILayout.Space(50);

		GUILayout.BeginHorizontal();

		for(int i = 0; i < 4; i++)
		{
			float pos = 0;	

			if(i == 0)
			{
				pos += posCenter - (52);
			}

			GUILayout.Space(pos);	

			terrainTextures[i] = (Texture2D)EditorGUILayout.ObjectField(/*Texture" + (i+1).ToString()*/"", terrainTextures[i], typeof(Texture2D), false, GUILayout.Height(60), GUILayout.Width(65));
		}

		GUILayout.EndHorizontal();

		GUILayout.Space(20);	

		GUILayout.BeginHorizontal();
		GUILayout.Label("Size X", GUILayout.Width(50));
		for(int i = 0; i < 4; i++)
		{
			float pos = 0;
			if(i == 0)
			{
				pos += posCenter - (104);
			}

			GUILayout.Space(pos);	
			textureSizesX[i] = EditorGUILayout.FloatField(textureSizesX[i], GUILayout.Width(65));
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Size Y", GUILayout.Width(50));
		for(int i = 0; i < 4; i++)
		{
			float pos = 0;
			if(i == 0)
			{
				pos += posCenter - (104);
			}

			GUILayout.Space(pos);	
			textureSizesY[i] = EditorGUILayout.FloatField(textureSizesY[i], GUILayout.Width(65));
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Offset X", GUILayout.Width(50));
		for(int i = 0; i < 4; i++)
		{
			float pos = 0;
			if(i == 0)
			{
				pos += posCenter - (104);
			}

			GUILayout.Space(pos);	
			textureOffsetsX[i] = EditorGUILayout.FloatField(textureOffsetsX[i], GUILayout.Width(65));
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Offset Y", GUILayout.Width(50));
		for(int i = 0; i < 4; i++)
		{
			float pos = 0;
			if(i == 0)
			{
				pos += posCenter - (104);
			}

			GUILayout.Space(pos);	
			textureOffsetsY[i] = EditorGUILayout.FloatField(textureOffsetsY[i], GUILayout.Width(65));
		}
		GUILayout.EndHorizontal();

		GUILayout.Space(30);

		terrainData_PixelError = EditorGUILayout.FloatField("Pixel Error", terrainData_PixelError);
		terrainData_BasemapDistance = EditorGUILayout.FloatField("Basemap Distance", terrainData_BasemapDistance);
		terrainData_CastShadows = EditorGUILayout.Toggle("Cast Shadows", terrainData_CastShadows);

		//terrainData_Material

		terrainData_Thickness = EditorGUILayout.FloatField("Thickness", terrainData_Thickness);
		terrainData_DrawDetails = EditorGUILayout.Toggle("Draw Details", terrainData_DrawDetails);
		terrainData_ControlTextureRes = EditorGUILayout.IntField("Control Tex Resolution", terrainData_ControlTextureRes);
		terrainData_BaseTextureRes = EditorGUILayout.IntField("Base Tex Resolution", terrainData_BaseTextureRes);	

		GUILayout.Space(50);

		GUILayout.BeginHorizontal();
		GUILayout.Space(posCenter);
		if(GUILayout.Button("LOAD DATA", GUILayout.Width(170), GUILayout.Height(50)))
		{
			if(terrainsParent == null)
			{
				EditorUtility.DisplayDialog("PARENT OBJECT MISSING", "Please assign parent object first", "OK");
			}
			else
			{
				GetTerrainsData();
			}
		}
		GUILayout.EndHorizontal();

		GUILayout.Space(20);

		GUILayout.BeginHorizontal();
		GUI.backgroundColor = Color.green;
		GUILayout.Space(posCenter);
		if(GUILayout.Button("APPLY CHANGES", GUILayout.Width(170), GUILayout.Height(50)))
		{
			if(terrainsParent == null)
			{
				EditorUtility.DisplayDialog("PARENT OBJECT MISSING", "Please assign parent object first", "OK");
			}
			else
			{
				ApplyDataChanges();
			}
		}
		GUI.backgroundColor = originalUIColor;
		GUILayout.EndHorizontal();

	}



	void GetTerrainsData()
	{
		GetTerrains();

		splatPrototypes = terrains[0].gameObject.GetComponent<Terrain>().terrainData.splatPrototypes;

		for(int i = 0; i < 4; i++)
		{
			terrainTextures[i] = null;
		}

		for(int i = 0; i < 4; i++)
		{
			if(splatPrototypes.Length > i)
			{
				terrainTextures[i] = splatPrototypes[i].texture;

				textureSizesX[i] = splatPrototypes[i].tileSize.x;
				textureSizesY[i] = splatPrototypes[i].tileSize.y;
				textureOffsetsX[i] = splatPrototypes[i].tileOffset.x;
				textureOffsetsY[i] = splatPrototypes[i].tileOffset.y;
			}
			else
			{
				textureSizesX[i] = 15;
				textureSizesY[i] = 15;
				textureOffsetsX[i] = 0;
				textureOffsetsY[i] = 0;
			}
		}

		Terrain tempData = terrains[0].gameObject.GetComponent<Terrain>();

		terrainData_PixelError = tempData.heightmapPixelError;
		terrainData_BasemapDistance = tempData.basemapDistance;
		terrainData_CastShadows = tempData.castShadows;
		terrainData_Material = tempData.materialType;
		terrainData_Thickness = tempData.terrainData.thickness;
		terrainData_DrawDetails = tempData.drawTreesAndFoliage;
	}

	void ApplyDataChanges()
	{
		foreach(GameObject terrain in terrains)
		{			
			SplatPrototype[] tempSplatPrototype = new SplatPrototype[4];

			for(int j = 0; j < 4; j++)
			{
				if(terrainTextures[j] != null)
				{
					tempSplatPrototype[j] = new SplatPrototype(); 
					tempSplatPrototype[j].texture = terrainTextures[j]; 

					tempSplatPrototype[j].tileSize = new Vector2(textureSizesX[j], textureSizesY[j]); 
					tempSplatPrototype[j].tileOffset = new Vector2(textureOffsetsX[j], textureOffsetsY[j]); 
				}
				else
				{					
					tempSplatPrototype[j] = new SplatPrototype(); 
					tempSplatPrototype[j].texture = terrainTextures[0]; 

					tempSplatPrototype[j].tileSize = new Vector2(15,15); 
					tempSplatPrototype[j].tileOffset = new Vector2(0,0); 				
				}
			}

			terrain.GetComponent<Terrain>().terrainData.splatPrototypes = tempSplatPrototype;
			Terrain tempTerrain = terrain.GetComponent<Terrain>();


			tempTerrain.heightmapPixelError = terrainData_PixelError;
			tempTerrain.basemapDistance = terrainData_BasemapDistance;
			tempTerrain.castShadows = terrainData_CastShadows;
			tempTerrain.materialType = terrainData_Material;
			tempTerrain.terrainData.thickness = terrainData_Thickness;
			tempTerrain.drawTreesAndFoliage = terrainData_DrawDetails;
		}
	}

	#endregion
					
	#region Init
	
	//At the start of initialize, clean up everything
	void ResetAll()
	{
		basePointsDifs.Clear();
		basePointsNew.Clear();
		sculptPoints.Clear();
		sculptPointsHistory.Clear();
		terrains.Clear();
		terrainsToEdit.Clear();
		undoIndex = 0;
		sculptPointsDeltas.Clear();
		
		meshes.Clear();
		meshVertsLists.Clear();
		meshTrianglesLists.Clear();
		trianglVerts.Clear();
		sculptGOs.Clear();
		meshColliders.Clear();

		if(sculptMeshParentRef != null)
		{
			DestroyImmediate(sculptMeshParentRef.gameObject);
		}	

	}

	void Delete()
	{
		//Delete terrains in parent object
		List<GameObject> children = new List<GameObject>();

		foreach(Transform temp in terrainsParent.transform)
		{			
			children.Add(temp.gameObject);
		}

		foreach(GameObject temp in children)
		{
			DestroyImmediate(temp.gameObject);
		}

		ResetAll();
	}	
	
	
	void InitializeEditor()
	{
		ResetAll();
		GetTerrains();
		SetFieldValues();
		CreateBasePoints();
		CreateSculptPoints();
		//InitializeSpline();
	}
	
	void GetTerrains()
	{
		foreach(Transform temp in terrainsParent)
		{
			terrains.Add(temp.gameObject);
		}
	}
	
	void SetFieldValues()
	{
		terrainRes = terrains[0].GetComponent<Terrain>().terrainData.heightmapResolution - 1;	
		terrainSize = terrains[0].GetComponent<Terrain>().terrainData.size.x;		

		stepSize = terrainSize / terrainRes;
		fieldIndexSize = new Vector2( (fieldEnd.x - fieldStart.x) / stepSize, (fieldEnd.z - fieldStart.z) / stepSize);	
		
		terrainPosY = terrains[0].transform.position.y;
	}
	
	#endregion
	
	#region Terrain Editor
	
	//Points which represent each terrain height value as 3d space point. These points are used to create sculpt points from which meshes are created and base points are also used to project data from meshes back onto terrains, because BasePoint holds access to the terrainData so it can be
	//assigned back
	void CreateBasePoints()
	{
		for(int i = 0; i < terrains.Count; i++)
		{
			Terrain curTerrain = terrains[i].GetComponent<Terrain>();
									
			float res = (float)curTerrain.terrainData.heightmapResolution;		//this makes points distributed properly over terrain ( otherwise they are slightly off )			
			float terrainHeight = curTerrain.terrainData.size.y;
			terrainHeightSize = terrainHeight;
			Vector2 terrainSize = new Vector2(curTerrain.terrainData.size.x, curTerrain.terrainData.size.z);			
			
			float[,] heights = curTerrain.terrainData.GetHeights(0, 0, (int)res, (int)res);	// make res bigger again to cover actual all points

			Vector3 terrainDataSize = curTerrain.terrainData.size;
			float terrainDataRes = curTerrain.terrainData.heightmapResolution;
			
			//Set min and max terrain sculpt height
			maxSculptHeight = terrainHeight;//curTerrain.transform.position.y + terrainHeight;
			minSculptHeight = 0f;//curTerrain.transform.position.y;
			

			//take terrain base position, remove field start position to make it as 0,0
			Vector2 startIndex = new Vector2( (terrains[i].transform.position.x - fieldStart.x) , (terrains[i].transform.position.z - fieldStart.z) );						
									
			//Make sure the position is actualy multiplication of the stepSize and not some random number because the terrain is offseted. Manualy impossible to align, since it would have to be aligned to the stepsize perfectly	
			
			//If field start is inside of terrain, then that terrain startIndex is negative, (modulus in negative number works differeint 1.7 % 0.5 = 0.2, -1.7 % 0.5 = 0.3 ( 2.0 - 1.7 )) , we offset the start by one stepSize to make the start position full size of terrain
			//if terrain1 starts at y = 10, terrain2 must start at y=60 if their size is 50 ( startindex.y )
			float modX = 0.0f;			
			float startX = 0.0f;

			if(startIndex.x < 0.0f)
			{
				modX = (startIndex.x % stepSize);
				startX = startIndex.x - modX - stepSize;
			}
			else
			{
				modX = (startIndex.x % stepSize);
				startX = startIndex.x - modX;
			}
			
			float modY = 0.0f;			
			float startY = 0.0f;
			
			if(startIndex.y < 0.0f)
			{
				modY = (startIndex.y % stepSize);
				startY = startIndex.y - modY - stepSize;
			}
			else
			{
				modY = (startIndex.y % stepSize);
				startY = startIndex.y - modY;
			}				
						
			startIndex = new Vector2(startX, startY);									
			
			//Ignore terrain if its out of the field
			Vector3 terrainPos = terrains[i].transform.position;
			Vector3 terainPosEnd = new Vector3(terrainPos.x + terrainSize.x, terrainPos.y, terrainPos.z + terrainSize.y);	//position in world where terrain ends. top right corner. we need to check if field start is in the terrain, not if the start of terrain is outside, because field can be in the terrain, then we need to create points there as well
						
			if(terrainPos.x > fieldEnd.x || terainPosEnd.x < fieldStart.x || terrainPos.z > fieldEnd.z || terainPosEnd.z < fieldStart.z)
			{				
				continue;	
			}

			for(int x = 0; x < (int)res; x++)
			{				
				for(int y = 0; y < (int)res; y++)
				{					
					//the position needs to get cleaned up so that meshpoints can be found. This solves problem when terrain position is for example 1.6, it makes sure the position is at stepSize multiplication
					float posX = terrainPos.x + stepSize * (x);
					float posZ = terrainPos.z + stepSize * (y);

					Vector2 vectorYX = new Vector2(y, x);

					BasePoint tempPoint = new BasePoint();
					Vector3 pos = new Vector3(posX, heights[y,x] * terrainHeight /*+ terrainPos.y*/, posZ);
					tempPoint.position = pos;
					tempPoint.originalPosition = pos;
					tempPoint.terrainIndex = i;
					tempPoint.terrainIndexes.Add(i);
					tempPoint.heightsIndex = vectorYX ;
					tempPoint.heightsIndexes.Add( vectorYX );
					tempPoint.terrainSize = terrainDataSize;
					tempPoint.terrainRes = terrainDataRes;
													
					//Calculate actual index postion based on the x and y indexes
					Vector2 pointIndex = new Vector2(startIndex.x + x * stepSize /*+ Random.Range(0,10000)*/, startIndex.y + y * stepSize);					

					//Terrains are aligned exactly to each other, the border points are at same spot, so there is conflict because both points can be at same position in dictionary. We take the first one, and second is out of luck. This doesnt matter
					//because these borders will be smoothed anyway
					bool isOccupied = basePointsNew.ContainsKey(pointIndex);
															
					if(!isOccupied)
					{
						basePointsNew.Add(pointIndex, tempPoint);
					}
					else
					{						
						basePointsNew[pointIndex].terrainIndexes.Add(i);	
						basePointsNew[pointIndex].heightsIndexes.Add(vectorYX);	
					}
				}				
			}

			terrainsToEdit.Add(i);			
		}
	}
	
	//Sculpt points are created based on Base Points, their amount is basePoints reduced by reduceDenisty. These points are used for creating sculpting mesh points
	void CreateSculptPoints()
	{		
		//Based on density, create either same amount of points as Base Points or less, then can adjust density at any time to add more points, max is the actual amount of points in BasePoints
		//amount of base points is basicly divided by this number. if its 2 for example, each 2 points are approximated to 1, if 3, then 3 points are approxiamted to 1						
		float stepSizeSculpt = reduceDenisty * stepSize;

		//Loop through all possible points in the field
		for(int x = 0; x < fieldIndexSize.x; x+= (int)reduceDenisty)
		{
			for(int y = 0; y < fieldIndexSize.y; y+= (int)reduceDenisty)
			{
				float xIndex = x*stepSize;
				float yIndex = y*stepSize;

				Vector2 indexVector = new Vector2(xIndex, yIndex);

				if(basePointsNew.ContainsKey(indexVector) && !sculptPoints.ContainsKey(indexVector))
				{					
					Vector3 tempPos = basePointsNew[indexVector].position;

					SculptPoint tempSculptPoint = new SculptPoint();
					tempSculptPoint.position = tempPos;
					tempSculptPoint.originalPosition = tempPos;
					tempSculptPoint.deltaPosition = tempPos;

					sculptPoints.Add(indexVector, tempSculptPoint);
					ExpandSculptPoint(indexVector, stepSizeSculpt);
				}					
			}			
		}
			
		sculptPointsDeltas.Add(new Dictionary<Vector2, float>());
	}

	//All direction to which we can expand ( right, top, left, topright etc )
	Vector2[] directions2 = new Vector2[]{new Vector2(1.0f,0.0f), new Vector2(-1.0f,0.0f), new Vector2(0.0f,1.0f), new Vector2(0.0f,-1.0f), new Vector2(1.0f,1.0f), new Vector2(-1.0f,1.0f), new Vector2(1.0f,-1.0f), new Vector2(-1.0f,-1.0f)};

	//expand ScuptPoint to nearest basePoint at distance of sculptPoints, if its not there, create arbitrary sculpt point
	void ExpandSculptPoint(Vector2 index, float stepSizeSculptExpand)
	{	
		//go through each direction, setup new index for basePoint based on the direction, check if the point exists, if it does and it isnt already in sculptPoints, add it as new Point, if it isnt in sculpt points but basePoint doesnt exist, expand with arbitrary point
		for(int i = 0; i < directions2.Length; i++)
		{
			Vector2 newIndex = new Vector2(index.x + (directions2[i].x * stepSizeSculptExpand), index.y + (directions2[i].y * stepSizeSculptExpand));

			if(!sculptPoints.ContainsKey(newIndex))
			{
				if(basePointsNew.ContainsKey(newIndex))
				{
					
				}
				else
				{
					Vector3 existingSculptPointPos = sculptPoints[index].position;
					SculptPoint tempSculptPoint = new SculptPoint();
					Vector3 tempPos = new Vector3(existingSculptPointPos.x + (directions2[i].x * stepSizeSculptExpand), existingSculptPointPos.y, existingSculptPointPos.z + (directions2[i].y * stepSizeSculptExpand));

					tempSculptPoint.position = tempPos;
					tempSculptPoint.originalPosition = tempPos;
					tempSculptPoint.deltaPosition = tempPos;

					sculptPoints.Add(newIndex, tempSculptPoint);
				}
			}
		}
	}			

	
	#endregion
	
	#region Edit Terrain
	
	void EditTerrain()
	{
		//11400
		//2900
		CreateMeshPointsAndVerts();	
		//2500
		CreateTriangles();
		//0
		InitMeshes();
		//0
		CreateMesh();
		//5200
		ResetColliders();

		//160
		sculptVisible = true;
		keepHighResBrush = true;
		Resources.UnloadUnusedAssets();
		System.GC.Collect();		
		terrainsVisible = terrainsParent.gameObject.activeSelf;
	}


	void CreateMeshPointsAndVerts()
	{

		//How many meshes will be used
		int horizontalSize = Mathf.CeilToInt(fieldIndexSize.x / reduceDenisty / rectSize);
		int verticalSize = Mathf.CeilToInt(fieldIndexSize.y / reduceDenisty / rectSize);

		float stepSizeSculpt = reduceDenisty * stepSize;	

		//go through each possible terrain and then check if that point ( index ) exists in sculpt points already. If it does, create mesh point. Mesh point holds all data, like index, meshIndex, world position etc.
		for(int indexVertical = 0; indexVertical < verticalSize; indexVertical++)		
		{
			for(int indexHorizontal = 0; indexHorizontal < horizontalSize; indexHorizontal++)
			{
				int index = 0;
				int meshIndex = (indexVertical * horizontalSize) + indexHorizontal;	
				meshVertsLists.Add(new List<Vector3>());
				trianglVerts.Add(new Dictionary<Vector2, int>());


				//Rect size is amount of points on x and y that is maxium amount for Unity mesh ( 250x250 )
				for(int y = 0; y <= rectSize; y++)
				{
					for(int x = 0; x <= rectSize; x++)
					{
						//There is -stepSizeSculpt because when sculptPoints are expanded, the index goes to negative
						float xIndex = x*stepSizeSculpt + (indexHorizontal * rectSize * stepSizeSculpt) - stepSizeSculpt;
						float yIndex = y*stepSizeSculpt + (indexVertical * rectSize * stepSizeSculpt) - stepSizeSculpt;			

						Vector2 indexVector = new Vector2(xIndex, yIndex);

						if(sculptPoints.ContainsKey(indexVector))
						{
							MeshPoint meshPoint = new MeshPoint();
							meshPoint.index = index;
													
							meshPoint.meshIndex = meshIndex;
							meshPoint.stepIndex = indexVector;
							meshPoint.position = sculptPoints[indexVector].position;
							//Debug.Log(meshPoint.position);


							meshVertsLists[meshIndex].Add(meshPoint.position);
							trianglVerts[meshIndex].Add(indexVector, index);

							//Assignt to sculpt point meshIndex and index, so that when updating meshVerts, its easy to access the correct vertex at correct mesh index
							SculptPoint tempSculptPoint = sculptPoints[indexVector];
							tempSculptPoint.meshData.Add(new Vector2(meshIndex, index));


							index++;
						}					
					}									
				}							
			}
		}

	}


	//Again go through all terrains and their possible points. Then create triangle indicies for meshes
	void CreateTriangles()
	{
		float stepSizeSculpt = reduceDenisty * stepSize;

		int horizontalSize = Mathf.CeilToInt(fieldIndexSize.x / reduceDenisty / rectSize);
		int verticalSize = Mathf.CeilToInt(fieldIndexSize.y / reduceDenisty / rectSize);


		for(int indexVertical = 0; indexVertical < verticalSize; indexVertical++)		
		{
			for(int indexHorizontal = 0; indexHorizontal < horizontalSize; indexHorizontal++)
			{				
				int meshIndex = (indexVertical * horizontalSize) + indexHorizontal;		//Calculate mesh index based on the indexes
				meshTrianglesLists.Add(new List<int>());								//Always have to add triangle even if it would be empty, so that indexes correspond with meshIndexes

				if(meshVertsLists[meshIndex].Count == 0)								//Skip, if this mesh has 0 vertices
				{					
					continue;
				}

				for(int y = 0; y <= rectSize; y++)
				{
					for(int x = 0; x <= rectSize; x++)
					{
						//There is -stepSizeSculpt because when sculptPoints are expanded, the index goes to negative
						float xIndex = x*stepSizeSculpt + (indexHorizontal * rectSize * stepSizeSculpt) - stepSizeSculpt;
						float yIndex = y*stepSizeSculpt + (indexVertical * rectSize * stepSizeSculpt) - stepSizeSculpt;					

						Vector2 stepIndex = new Vector2(xIndex, yIndex);

						if(trianglVerts[meshIndex].ContainsKey(stepIndex))			//triangleVerts is list of Dictionaries, look for correct vertex index based on meshIndex and stepIndex as key in dictionary
						{
							int baseIndex = trianglVerts[meshIndex][stepIndex];		//We need to make sure if this specific vert has all of the adjacents verts ( right, topright, top ) to make sure two triangles can be build from this position. if it doesnt have all of them, skip it

							//Right
							Vector2 rightKey = new Vector2(stepIndex.x + stepSizeSculpt, stepIndex.y);
							int rightIndex = 0;

							if(trianglVerts[meshIndex].ContainsKey(rightKey))
							{
								rightIndex = trianglVerts[meshIndex][rightKey];
							}
							else
							{	continue; }

							//Top
							Vector2 topKey = new Vector2(stepIndex.x, stepIndex.y + stepSizeSculpt);
							int topIndex = 0;

							if(trianglVerts[meshIndex].ContainsKey(topKey))
							{
								topIndex = trianglVerts[meshIndex][topKey];
							}
							else
							{	continue; }

							//Right
							Vector2 topRightKey = new Vector2(stepIndex.x + stepSizeSculpt, stepIndex.y + stepSizeSculpt);
							int topRigthIndex = 0;

							if(trianglVerts[meshIndex].ContainsKey(topRightKey))
							{
								topRigthIndex = trianglVerts[meshIndex][topRightKey];
							}
							else
							{	continue; }



							//We know all of the adjacents verts are avaiable, we build two triangles from them. these are used to create the mesh
							meshTrianglesLists[meshIndex].Add(baseIndex);
							meshTrianglesLists[meshIndex].Add(topRigthIndex);
							meshTrianglesLists[meshIndex].Add(rightIndex);

							meshTrianglesLists[meshIndex].Add(baseIndex);
							meshTrianglesLists[meshIndex].Add(topIndex);
							meshTrianglesLists[meshIndex].Add(topRigthIndex);

						}		

					}
				}

			}
		}			
	}
	
	
	void InitMeshes()
	{				
		meshMat = Resources.Load("TerrainToolbox/SculptMat") as Material;
		GameObject sculptMeshParent = new GameObject();
		sculptMeshParent.name = "Sculpt_Parent";
		sculptMeshParent.transform.position = new Vector3(0, terrainsParent.transform.position.y, 0); // Vector3.zero;//terrainsParent.transform.position; // Vector3.zero; new Vector3(0, terrainsParent.transform.position.y, 0);
				
		//Store access to the sculpt mesh, so it can be deleted and there are no duplicates even when quiting Unity
		EditorScriptable editorObject = EditorSettings.CreateEditorSettings();
		if(editorObject.sculptMesh != null)
		{
			GameObject temp = editorObject.sculptMesh;
			DestroyImmediate(temp.gameObject);
		}
		
		editorObject.sculptMesh = (GameObject)sculptMeshParent;
		EditorUtility.SetDirty(editorObject);
		AssetDatabase.SaveAssets();
						
		sculptMeshParentRef = sculptMeshParent;
		
		for(int i = 0; i < meshVertsLists.Count; i++)
		{		
			Mesh tempMesh = new Mesh();
			tempMesh.MarkDynamic();

			if(meshVertsLists[i].Count != 0)
			{
				meshTerrain = new GameObject();
				meshTerrain.transform.localPosition = Vector3.zero;
				meshTerrain.transform.localRotation =  Quaternion.Euler(0, 0, 0);
				meshTerrain.transform.localScale = Vector3.one;	
				meshTerrain.AddComponent(typeof(MeshRenderer));
				meshTerrain.AddComponent(typeof(MeshFilter));	
				MeshFilter tempFilter = meshTerrain.GetComponent<MeshFilter>();

				tempFilter.sharedMesh = tempMesh; //SharedMesh
				tempMesh.name = "Mesh" + i.ToString();					
				meshTerrain.GetComponent<Renderer>().material = meshMat;			

				meshTerrain.name = "Sculpt Mesh" +i.ToString();
				meshTerrain.AddComponent<MeshCollider>();				
				meshTerrain.transform.parent = sculptMeshParent.transform;
				meshTerrain.transform.localPosition = Vector3.zero;
				sculptGOs.Add(meshTerrain);			
							
			}

			meshes.Add(tempMesh);				
			meshColliders.Add(meshTerrain.GetComponent<MeshCollider>());
		}	
	}
		
	
	void UpdateMeshes()
	{			
		int meshIndex = 0;
		
		for(int i = 0; i < meshIndexes.Count; i++)
		{	
			meshIndex = meshIndexes[i];
			
			//check if the mesh has some triangles, if not and is empty, skip it
			if(meshTrianglesLists[meshIndex].Count != 0)
			{				
				meshes[meshIndex].vertices = meshVertsLists[meshIndex].ToArray();								
				meshes[meshIndex].RecalculateNormals();									
			}			
		}

		//System.GC.Collect();
	}
	
	
	void UpdateMeshesAll()
	{		
		for(int i = 0; i < meshes.Count; i++)
		{			
			//check if the mesh has some triangles, if not and is empty, skip it
			if(meshTrianglesLists[i].Count != 0)
			{					
				meshes[i].vertices = meshVertsLists[i].ToArray();
				meshes[i].RecalculateNormals();					
			}			
		}
	}
	
		
	void CreateMesh()
	{		
		for(int i = 0; i < meshVertsLists.Count; i++)
		{	
			//check if the mesh has some triangles, if not and is empty, skip it
			if(meshTrianglesLists[i].Count != 0)
			{		
				meshes[i].Clear();
				meshes[i].vertices = meshVertsLists[i].ToArray();
				meshes[i].triangles = meshTrianglesLists[i].ToArray();

				meshes[i].RecalculateNormals();
				meshes[i].RecalculateBounds();			
			}			
		}		
	}
	
	void ResetColliders()
	{		
		foreach(GameObject temp in sculptGOs)
		{
			temp.GetComponent<MeshCollider>().enabled = false;
			temp.GetComponent<MeshCollider>().enabled = true;
		}
	}
	
	void ResetCollidersEditing()
	{		
		for(int i = 0; i < meshIndexes.Count; i++)
		{			
			meshColliders[meshIndexes[i]].enabled = false;
			meshColliders[meshIndexes[i]].enabled = true;
		}
	}	
	
	void DisableColliders()
	{
		for(int i = 0; i < meshIndexes.Count; i++)
		{			
			meshColliders[meshIndexes[i]].enabled = false;		
		}
	}	

	
	#endregion
	
	#region Project Back

	//Take interpolated start points, remove those from original points to find out difference. then apply this difference when points are projected back at the end

	void AdjustSculptPoints()
	{
		//to find the index in sculpt index, points got stored at index based on reduceDenisty, so we must use same step to get to the same index

		float prevPointOriginal = 0.0f;
		float prevPointNew = 0.0f;
		float totalOriginal = 0.0f;
		float totalNew = 0.0f;

		bool firstPoint = true;

		for(int y = 0; y < fieldIndexSize.y; y++)
		{
			for(int x = 0; x < fieldIndexSize.x; x++)
			{	
				if(x == 0)
				{
					firstPoint = true;
				}

				Vector2 index = new Vector2(x * stepSize, y * stepSize);

				if(sculptPoints.ContainsKey(index))
				{
					
					Vector3 posOriginal = sculptPoints[index].originalPosition; //originalSculptPoints[index].position;
					Vector3 posNew = sculptPoints[index].position;

					if(firstPoint)
					{
						prevPointOriginal = posOriginal.y;
						prevPointNew = posNew.y;
						totalOriginal = 0.0f;
						totalNew = 0.0f;

						firstPoint = false;
					}

					float deltaOriginal = posOriginal.y - prevPointOriginal;
					float deltaNew = posNew.y - prevPointNew;

					totalOriginal += deltaOriginal;
					totalNew += deltaNew;

					float delta = posNew.y - posOriginal.y;

					sculptPoints[index].position = new Vector3(posNew.x, posOriginal.y + delta, posNew.z);

					prevPointOriginal = posOriginal.y;
					prevPointNew = posNew.y;	

				}				
			}			
		}	
	}


	void  CalculateSculptDeltas()
	{		
		int offsetIndex = (int)reduceDenisty + 1;			

		for(int y = -offsetIndex; y < fieldIndexSize.y +offsetIndex; y++)
		{
			for(int x = -offsetIndex; x < fieldIndexSize.x +offsetIndex; x++)
			{
				Vector2 index = new Vector2(x * stepSize, y * stepSize);

				if(sculptPoints.ContainsKey(index))
				{
					Vector3 originalY = sculptPoints[index].originalPosition;
					Vector3 finalY = sculptPoints[index].position;

					Vector3 delta = new Vector3(originalY.x, finalY.y - originalY.y, originalY.z);
					sculptPoints[index].deltaPosition = delta;
				}
			}
		}
	}
	

	//Take data from sculpt points and project it back to virtual points, interpolate between sculpt points to create missing points for the base points ( there is less sculpt points then there is base points )
	void ProjectBackToBasePoints()///*bool keepHighRes,*/ bool isStart)//ref Dictionary<Vector2, BasePoint> basePointsTemp, bool phase1)
	{
		float stepSizeSculpt = reduceDenisty * stepSize;
		bool keepHighResSculptPoint = true;

		Vector3 A0 = Vector3.zero;
		Vector3 A1 = Vector3.zero;
		Vector3 A2 = Vector3.zero;
		Vector3 A3 = Vector3.zero;
		
		Vector3 B0 = Vector3.zero;
		Vector3 B1 = Vector3.zero;
		Vector3 B2 = Vector3.zero;		
		Vector3 B3 = Vector3.zero;		
		
		Vector3 C0 = Vector3.zero;
		Vector3 C1 = Vector3.zero;
		Vector3 C2 = Vector3.zero;		
		Vector3 C3 = Vector3.zero;		
		
		Vector3 D0 = Vector3.zero;
		Vector3 D1 = Vector3.zero;;
		Vector3 D2 = Vector3.zero;		
		Vector3 D3 = Vector3.zero;
		
		Vector3 finalA = Vector3.zero;
		Vector3 finalB = Vector3.zero;
		Vector3 finalC = Vector3.zero;
		Vector3 finalD = Vector3.zero;
		Vector3 finalPoint = Vector3.zero;		


		//Go through all points in basePointsNew and assign them new Y values
		for(int y = 0; y < (fieldIndexSize.y); y++) //fieldIndexSize.y
		{			
			for(int x = 0; x < (fieldIndexSize.x); x++)
			{	
				//actual index of the basePoints, stepSize is smaller than stepSizeSculpt ( or at max can be same ) so we are going through points which dont exist in sculptPoints
				Vector2 index = new Vector2(x * stepSize , y * stepSize);				

				//We need to use custom scutlpIndex for the points, because if using normal index. when point was on new Y line ( y+1 ) the sculpt Point it would be looking at wouldnt be the closest one from ( y-1 ) but at the one from the last x, which was probably at then of the terrain on X, this way, index is cleaned
				//up for sculpt point to always be the closest one, the points are not selected withing the square where it needs to interpolate, instead, it goes from x = 0 to last x, row by row, so basicly losing sight of the sculpt point when going to next row on Y, thus, it needs to get
				//selected properly again, to the previous lines where the sculpt point was
				Vector2 sculptPointsIndex = new Vector2(index.x, index.y - index.y % stepSizeSculpt);

				//If sculpt point exists tho, find next sculpt point on X and Y and XY, those will be used for interpolating and finding actualy Y which will be smoothed for the basePoints
				if(sculptPoints.ContainsKey(sculptPointsIndex))
				{		
					float index1 = sculptPointsIndex.x - stepSizeSculpt;
					float index3 = sculptPointsIndex.x + stepSizeSculpt;
					float index4 = sculptPointsIndex.x + 2*stepSizeSculpt;

					keepHighResSculptPoint = sculptPoints[sculptPointsIndex].keepHighRes;

					if(keepHighResSculptPoint)
					{
						A1 = sculptPoints[sculptPointsIndex].deltaPosition;		//origin point				
					}
					else
					{
						A1 = sculptPoints[sculptPointsIndex].position;
					}

					float tempSize = stepSizeSculpt;
					Vector2 tempIndex = sculptPointsIndex;

					A0 = sculptPointGetY(new Vector2(index1, sculptPointsIndex.y), 									A1, new Vector3(-1.0f, 0.0f, 0.0f), tempSize, tempIndex, keepHighResSculptPoint);																						
					A2 = sculptPointGetY(new Vector2(index3, sculptPointsIndex.y),									A1, new Vector3(1.0f, 0.0f, 0.0f), tempSize, tempIndex, keepHighResSculptPoint);  						//Point on X+1				//sculptPoints[new Vector2(index.x + stepSizeSculpt, index.y)].y;
					A3 = sculptPointGetY(new Vector2(index4, sculptPointsIndex.y), 									A1, new Vector3(2.0f, 0.0f, 0.0f), tempSize, tempIndex, keepHighResSculptPoint);//, new Vector2(index3, sculptPointsIndex.y));

					B0 = sculptPointGetY(new Vector2(index1, sculptPointsIndex.y + stepSizeSculpt), 				A1, new Vector3(-1.0f, 0.0f, 1.0f), tempSize, tempIndex, keepHighResSculptPoint);
					B1 = sculptPointGetY(new Vector2(sculptPointsIndex.x, sculptPointsIndex.y + stepSizeSculpt), 	A1, new Vector3(0.0f, 0.0f, 1.0f), tempSize, tempIndex, keepHighResSculptPoint);  						//Point on Y+1  			//sculptPoints[new Vector2(index.x, index.y + stepSizeSculpt)].y;
					B2 = sculptPointGetY(new Vector2(index3, sculptPointsIndex.y + stepSizeSculpt), 				A1, new Vector3(1.0f, 0.0f, 1.0f), tempSize, tempIndex, keepHighResSculptPoint);   	//Points on X+1 and Y+1     //sculptPoints[new Vector2(index.x + stepSizeSculpt, index.y + stepSizeSculpt)].y;					
					B3 = sculptPointGetY(new Vector2(index4, sculptPointsIndex.y + stepSizeSculpt), 				A1, new Vector3(2.0f, 0.0f, 1.0f), tempSize, tempIndex, keepHighResSculptPoint);//, new Vector2(index3, sculptPointsIndex.y + stepSizeSculpt)); 

					C0 = sculptPointGetY(new Vector2(index1, sculptPointsIndex.y + 2*stepSizeSculpt), 				A1, new Vector3(-1.0f, 0.0f, 2.0f), tempSize, tempIndex, keepHighResSculptPoint);
					C1 = sculptPointGetY(new Vector2(sculptPointsIndex.x, sculptPointsIndex.y + 2*stepSizeSculpt), 	A1, new Vector3(0.0f, 0.0f, 2.0f), tempSize, tempIndex, keepHighResSculptPoint); 						
					C2 = sculptPointGetY(new Vector2(index3, sculptPointsIndex.y + 2*stepSizeSculpt), 				A1, new Vector3(1.0f, 0.0f, 2.0f), tempSize, tempIndex, keepHighResSculptPoint);  					
					C3 = sculptPointGetY(new Vector2(index4, sculptPointsIndex.y + 2*stepSizeSculpt), 				A1, new Vector3(2.0f, 0.0f, 2.0f), tempSize, tempIndex, keepHighResSculptPoint);//, new Vector2(index3, sculptPointsIndex.y + 2*stepSizeSculpt));

					D0 = sculptPointGetY(new Vector2(index1, sculptPointsIndex.y - stepSizeSculpt), 				A1, new Vector3(-1.0f, 0.0f, -1.0f), tempSize, tempIndex, keepHighResSculptPoint);
					D1 = sculptPointGetY(new Vector2(sculptPointsIndex.x, sculptPointsIndex.y - stepSizeSculpt), 	A1, new Vector3(0.0f, 0.0f, -1.0f), tempSize, tempIndex, keepHighResSculptPoint); 						
					D2 = sculptPointGetY(new Vector2(index3, sculptPointsIndex.y - stepSizeSculpt), 				A1, new Vector3(1.0f, 0.0f, -1.0f), tempSize, tempIndex, keepHighResSculptPoint);   				
					D3 = sculptPointGetY(new Vector2(index4, sculptPointsIndex.y - stepSizeSculpt), 				A1, new Vector3(2.0f, 0.0f, -1.0f), tempSize, tempIndex, keepHighResSculptPoint);//, new Vector2(index3, sculptPointsIndex.y - stepSizeSculpt)); 

				}

				//if the basePoint doesnt exist, we cant project it back, since there is nothing to project back to. This check happens after assinging sculpt points A1,A2 etc. because of those outer borders. This check would prevent any sculpt points out of the base points to be used
				if(!basePointsNew.ContainsKey(new Vector2(index.x, index.y))){ continue; }


				//This is actual value for interpolations, if stepSize is 0.1 and stepSizeSculpt is 0.5 for example, then we get something like 4.1 % 0.5 => 0.1, this is relative distance from one sculpt point to the other
				//so then we do 0.1 / 0.5 => 0.02. This is value we can use for interpolation
				float interpolateX = ((x * stepSize) % stepSizeSculpt) / stepSizeSculpt;
				float interpolateY = ((y * stepSize) % stepSizeSculpt) / stepSizeSculpt;				
								
				float tx = interpolateX;
				float ty = interpolateY;

				if(interpolationMode == InterpolationMode.Normal || interpolationMode == InterpolationMode.Smooth)
				{
					finalA = CalculatePoint(A0, A1, A2, A3, tx, ty);
					finalB = CalculatePoint(B0, B1, B2, B3, tx, ty);
					finalC = CalculatePoint(C0, C1, C2, C3, tx, ty);
					finalD = CalculatePoint(D0, D1, D2, D3, tx, ty);

					finalPoint = CalculatePoint(finalD, finalA, finalB, finalC, ty, ty);
				}
				else
				{
					finalPoint = CalculatePoint(A1, A2, B1, B2, tx, ty);
				}


				Vector3 pos = basePointsNew[new Vector2(index.x, index.y)].position;
				Vector3 posOrig = basePointsNew[index].originalPosition;


				if(keepHighResSculptPoint)
				{
					basePointsNew[new Vector2(index.x, index.y)].position = new Vector3(pos.x, posOrig.y + finalPoint.y, pos.z);
				}
				else
				{
					basePointsNew[new Vector2(index.x, index.y)].position = new Vector3(pos.x, finalPoint.y, pos.z);
				}
			}
		}
	}	

						
	Vector3 CalculatePoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, float ty)
	{
		if(interpolationMode == InterpolationMode.Linear)
		{
			//Linearly interpolate to find how far actualy this current point is
			Vector3 interpolateXresultA = t * p1 + (1.0f - t) * p0;
			Vector3 interpolateXresultB = t * p3 + (1.0f - t) * p2;	

			//The interpolat points were bont on X, X to X+1 and then X to X+1 on Y+1. which means there has to be interpolation between these two on Y as well
			Vector3 interpolateAtoB = ty * interpolateXresultB + ((1.0f - ty) * interpolateXresultA);	

			return interpolateAtoB; 
		}
		else if(interpolationMode == InterpolationMode.Normal)
		{
			float t0 = 0.0f;
			float t1 = GetT(t0, p0, p1);
			float t2 = GetT(t1, p1, p2);
			float t3 = GetT(t2, p2, p3);

			t = t1 + ((t2-t1) * t);

			Vector3 A1 = (t1-t)/(t1-t0)*p0 + (t-t0)/(t1-t0)*p1;
			Vector3 A2 = (t2-t)/(t2-t1)*p1 + (t-t1)/(t2-t1)*p2;
			Vector3 A3 = (t3-t)/(t3-t2)*p2 + (t-t2)/(t3-t2)*p3;

			Vector3 B1 = (t2-t)/(t2-t0)*A1 + (t-t0)/(t2-t0)*A2;
			Vector3 B2 = (t3-t)/(t3-t1)*A2 + (t-t1)/(t3-t1)*A3;

			Vector3 C = (t2-t)/(t2-t1)*B1 + (t-t1)/(t2-t1)*B2;

			return C;
		}
		else
		{			
			return 
				0.5f * ((2 * p1) +
					(-p0 + p2) * t +
					(2*p0 - 5*p1 + 4*p2 - p3) * t*t +
					(-p0 + 3*p1 - 3*p2 + p3) * t*t*t );
		}
			
	}	


	float GetT(float t, Vector3 p0, Vector3 p1)
	{		
		float a = Mathf.Pow((p1.x-p0.x), 2.0f) + Mathf.Pow((p1.y-p0.y), 2.0f) + Mathf.Pow((p1.z-p0.z), 2.0f);
		float b = Mathf.Pow(a, 0.5f);
		float c = Mathf.Pow(b, alphaValue);

		return (c + t);
	}
	


	Vector3 sculptPointGetY(Vector2 index, Vector3 originPos, Vector3 direction, float size, Vector2 pointsIndex, bool keepDetails)
	{
		if(sculptPoints.ContainsKey(index))
		{
			if(keepDetails)
			{
				return sculptPoints[index].deltaPosition;
			}
			else
			{
				return sculptPoints[index].position;	
			}
		}
		else
		{
			if(keepDetails)
			{
				if(sculptPoints.ContainsKey(new Vector2(pointsIndex.x + (direction.x*size), pointsIndex.y + (direction.z*size))))
				{
					return sculptPoints[new Vector2(pointsIndex.x + (direction.x*size), pointsIndex.y + (direction.z*size))].deltaPosition;
				}
				else
				{				
					if(sculptPoints.ContainsKey(new Vector2(pointsIndex.x + (direction.x*size), pointsIndex.y + (direction.z*size))))
					{
						return sculptPoints[new Vector2(pointsIndex.x + (direction.x*size), pointsIndex.y + (direction.z*size))].deltaPosition;
					}
					else
					{
						return originPos + (direction * size);
					}
				}
			}
			else
			{
				if(sculptPoints.ContainsKey(new Vector2(pointsIndex.x + (direction.x*size), pointsIndex.y + (direction.z*size))))
				{
					return sculptPoints[new Vector2(pointsIndex.x + (direction.x*size), pointsIndex.y + (direction.z*size))].position;
				}
				else
				{				
					if(sculptPoints.ContainsKey(new Vector2(pointsIndex.x + (direction.x*size), pointsIndex.y + (direction.z*size))))
					{
						return sculptPoints[new Vector2(pointsIndex.x + (direction.x*size), pointsIndex.y + (direction.z*size))].position;
					}
					else
					{
						return originPos + (direction * size);
					}
				}
			}
		}	
	}

	
	//Use basePoints and apply the data to actual terrain heights
	void ProjectBackToTerrain()
	{
		for(int i = 0; i < terrains.Count; i++)
		{
			bool canEdit = false;

			foreach(int terrainIndex in terrainsToEdit)
			{
				if(i == terrainIndex)
				{
					canEdit = true;
				}
			}

			if(!canEdit){ continue; }			

			TerrainData data = terrains[i].GetComponent<Terrain>().terrainData;	
			float[,] tempHeights = data.GetHeights(0, 0, data.heightmapResolution, data.heightmapResolution);			

			foreach(KeyValuePair<Vector2, BasePoint> temp in basePointsNew)
			{
				BasePoint tempPoint = temp.Value;

				for(int t = 0; t < tempPoint.terrainIndexes.Count; t++)
				{
					if(tempPoint.terrainIndexes[t] == i)
					{
						tempHeights[(int)tempPoint.heightsIndexes[t].x, (int)tempPoint.heightsIndexes[t].y] = tempPoint.position.y / tempPoint.terrainSize.y;		
						//Debug.Log(tempPoint.position.y.ToString() + " " + tempPoint.terrainSize.y.ToString());

					}
				}
			}

			data.SetHeights(0,0, tempHeights);
			terrains[i].GetComponent<Terrain>().terrainData = data;
			
			terrains[i].gameObject.SetActive(false);
			terrains[i].gameObject.SetActive(true);
		}
	}
	
	#endregion
	
	#region Creator
	
	void CreateTerrain()
	{		
		int index = nameIndex;
		
		for(int y = 0; y < gridDimensions.y; y++)
		{		
			for(int x = 0; x < gridDimensions.x; x++)
			{			
				
				//Create terrain data, plug those into instantiated terrain object, these are data existing only in project panel
				TerrainData terrainData = new TerrainData ();			
				terrainData.heightmapResolution = heighmapResolution;			
				terrainData.size = new Vector3 (defaultTerrainSize, defaultTerrainHeight, defaultTerrainSize);
		
				terrainData.heightmapResolution = heighmapResolution;
				terrainData.thickness = terrainThickness;	

				terrainData.SetDetailResolution(defaultDetailRes, defaultResPerPatch);
				terrainData.baseMapResolution = defaultBaseTextureRes;

				AssetDatabase.CreateAsset(terrainData, DataPath.pathStart + DataPath.pathTerrainData + "Terrain_" + index.ToString() + ".asset");
								
				//Create terrain GameObject existing in the scene
				GameObject terrain = (GameObject)Terrain.CreateTerrainGameObject(terrainData);				
				Terrain terrainComponent = terrain.GetComponent<Terrain>();		
				
				switch(terrainMat)
				{
					case TerrainMaterials.Standard:
					terrainComponent.materialType = Terrain.MaterialType.BuiltInStandard;
						break;
					case TerrainMaterials.LegacyDiffuse:
					terrainComponent.materialType = Terrain.MaterialType.BuiltInLegacyDiffuse;
						break;
					case TerrainMaterials.LegacySpecular:
					terrainComponent.materialType = Terrain.MaterialType.BuiltInLegacySpecular;
						break;
					default:
					terrainComponent.materialType = Terrain.MaterialType.BuiltInStandard;
						break;
				}	

				terrainComponent.heightmapPixelError = defaultPixelError;
				terrainComponent.basemapDistance = defaultBaseMapDistance;
				terrainComponent.castShadows = defaultCastShadows;
				terrainComponent.drawTreesAndFoliage = defaultDrawDetail;


				if(defaultProbes)
				{
					terrainComponent.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
				}
				else
				{
					terrainComponent.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;	
				}

				
				terrain.name = "Terrain_" + index.ToString();
				terrain.transform.parent = terrainsParent.transform;
				terrain.transform.localPosition = new Vector3(x * defaultTerrainSize + startPositionGrid.x, 0, y * defaultTerrainSize + startPositionGrid.z);						
				
				
				index++;
			}
		}
	}

	void SetNeighbours()
	{
		foreach(Transform temp in terrainsParent.transform)
		{
			if(!temp.gameObject.GetComponent<TerrainNeighbour>())
			{
				temp.gameObject.AddComponent<TerrainNeighbour>();
			}

			temp.gameObject.GetComponent<TerrainNeighbour>().SetNeighbours();
		}
	}

	#endregion
	
	#region Presets


	void ShowPresetsSettings()
	{
		float posCenter = windowWidth / 2.0f - (160.0f / 2.0f);	

		GUILayout.Space(50);

		GUILayout.BeginHorizontal();
		GUILayout.Space(posCenter);
		if(GUILayout.Button("LOAD PRESETS", GUILayout.Width(170), GUILayout.Height(40)))
		{
			LoadPresets();			
		}	
		GUILayout.EndHorizontal();

		GUILayout.Space(20);
		presetsIndex = EditorGUILayout.Popup("Select Preset", presetsIndex, presets.ToArray(), GUILayout.Width(350));

		GUILayout.Space(20);

		GUILayout.Label("New Preset Name");
		newPresetName = GUILayout.TextField(newPresetName);

		GUILayout.Space(20);


		posCenter = windowWidth / 2.0f - (170.0f / 2.0f);	

		GUILayout.BeginHorizontal();
		GUILayout.Space(posCenter);
		if(GUILayout.Button("CREATE NEW PRESET", GUILayout.Width(170), GUILayout.Height(50)))
		{
			CreatePreset(newPresetName);			
		}	
		GUILayout.EndHorizontal();

		GUILayout.Space(20);

		GUILayout.BeginHorizontal();
		GUILayout.Space(posCenter);
		if(GUILayout.Button("DELETE SELECTED PRESET", GUILayout.Width(170), GUILayout.Height(50)))
		{
			if(EditorUtility.DisplayDialog("DELETE SELECTED PRESET", "This action will delete all terrain data associated with this preset. \n\nAre you sure you want to proceed ?", "NO", "YES"))
			{
				//Debug.Log("1");
			}
			else
			{
				DeletePreset();	
			}			
		}	
		GUILayout.EndHorizontal();

		GUILayout.Space(50);

		float posLeft = windowWidth / 2.0f - 200.0f;
		float posRight = 60;

		GUILayout.BeginHorizontal();
		GUILayout.Space(posLeft);
		if(GUILayout.Button("SAVE TO PRESET", GUILayout.Width(170), GUILayout.Height(50)))
		{
			SavePreset();			
		}	

		GUILayout.Space(posRight);
		if(GUILayout.Button("LOAD FROM PRESET", GUILayout.Width(170), GUILayout.Height(50)))
		{
			LoadPreset();			
		}	
		GUILayout.EndHorizontal();

	}


	void CreatePreset(string name)
	{	
		presetsObject.presetsNames.Add("Preset_" + name);
		LoadPresets();

		presetsIndex = presets.Count-1;
	}

	void LoadPresets()
	{
		presetsObject = Presets.CreatePresets();

		presets.Clear();

		for(int i = 0; i < presetsObject.presetsNames.Count; i++)
		{
			presets.Add(presetsObject.presetsNames[i]);	
		}

		EditorUtility.SetDirty(presetsObject);
		AssetDatabase.SaveAssets();
	}

	void DeletePreset()
	{
		string[] presetTerrainObjects = AssetDatabase.FindAssets(presets[presetsIndex]);		

		foreach(string temp in presetTerrainObjects)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(temp);	
			if(assetPath.Contains("TerrainData")){continue;}

			TerrainScriptable tempScriptable = AssetDatabase.LoadAssetAtPath<TerrainScriptable>(assetPath);

			string terrainDataPath = DataPath.pathStart + DataPath.pathTerrainData + tempScriptable.terrainDataName + ".asset";

			//Delete Terrain Scriptable
			AssetDatabase.DeleteAsset(assetPath);	
			//Delete Terrain Data
			AssetDatabase.DeleteAsset(terrainDataPath);	
		}

		presetsObject.presetsNames.Remove(presets[presetsIndex]);
		LoadPresets();

		presetsIndex = 0;

	}

	//Go through existing terrains in the terrainsParent, and save their terrain data with the preset name ( save position as well )
	void SavePreset()
	{
		//get preset name
		string tempPreset = presets[presetsIndex];

		//Create TerrainObjects, save date of each terrain, save the terrainObject name with the preset
		int index = 0;

		foreach(Transform tempTerrain in terrainsParent.transform)
		{	
			TerrainScriptable terObj = CreateTerrainScriptable.CreateTerrain(index, presets[presetsIndex]);						
			terObj.name = "Terrain" + "_" + tempPreset + "_" + (index).ToString();					//This is name of sriptable object in project panel, through this name the correct terrains for preste is found and loaded	

			TerrainData tempData = tempTerrain.gameObject.GetComponent<Terrain>().terrainData;	
			string dataName = "TerrainData" + "_" + tempPreset + "_" + (index).ToString();
			AssetDatabase.RenameAsset(DataPath.pathStart + DataPath.pathTerrainData + tempData.name + ".asset", dataName);

			terObj.position = tempTerrain.position;				
			terObj.data = tempData;									
			terObj.terrainName = "Terrain" + "_" + tempPreset + "_" + (index).ToString();
			terObj.terrainMat = tempTerrain.gameObject.GetComponent<Terrain>().materialType;
			terObj.terrainDataName = dataName;	

			EditorUtility.SetDirty( terObj );
			EditorUtility.SetDirty( tempData );
			index++;
		}
	}

	void LoadPreset()
	{
		string[] presetTerrainObjects = AssetDatabase.FindAssets(presets[presetsIndex]);	

		foreach(string temp in presetTerrainObjects)
		{			
			string assetPath = AssetDatabase.GUIDToAssetPath(temp);	

			if(assetPath.Contains("TerrainData")){continue;}


			TerrainScriptable tempScriptable = AssetDatabase.LoadAssetAtPath<TerrainScriptable>(assetPath);
			TerrainData tempData = AssetDatabase.LoadAssetAtPath<TerrainData>(DataPath.pathStart + DataPath.pathTerrainData + tempScriptable.terrainDataName + ".asset");

			GameObject terrain = (GameObject)Terrain.CreateTerrainGameObject(tempData); 

			terrain.name = tempScriptable.terrainName;
			terrain.transform.parent = terrainsParent.transform;
			terrain.transform.position = tempScriptable.position;		
			terrain.gameObject.GetComponent<Terrain>().materialType = tempScriptable.terrainMat;
		}		
	}
	
	#endregion

	#region Brush	
	
	void Brush(ref Vector3 hitPosition)
	{
		//to find the index in sculpt index, points got stored at index based on reduceDenisty, so we must use same step to get to the same index
		float stepSizeSculpt = reduceDenisty * stepSize;
	

		finalHitPositionAdjust = new Vector3(hitPosition.x - fieldStart.x, hitPosition.y /*+ fieldStart.y*/, hitPosition.z - fieldStart.z); //hitPosition.y - fieldStart.y


		//clean up the position for the grid step sizes
		indexPositionAdjust = new Vector2(finalHitPositionAdjust.x - (finalHitPositionAdjust.x % stepSizeSculpt), finalHitPositionAdjust.z - (finalHitPositionAdjust.z % stepSizeSculpt));


		//check if point exists
		bool exists = sculptPoints.ContainsKey(indexPositionAdjust);
		int rangeIndex = Mathf.FloorToInt(brushSize / stepSizeSculpt); //range in stepSizes, convert how many steps is the size in world units		

		sculptPointsIndexes.Clear();

		if(exists)
		{
			float yValue = 0.0f;
			float finalValue = 0.0f;		
			float distance = 0.0f;
			SculptPoint tempPointAdjust = new SculptPoint();

			for(int y = -rangeIndex; y < rangeIndex; y++)
			{
				for(int x = -rangeIndex; x < rangeIndex; x++)
				{
					fieldIndexAdjust = new Vector2(indexPositionAdjust.x + (x*stepSizeSculpt), indexPositionAdjust.y + (y*stepSizeSculpt));	

					if(sculptPoints.TryGetValue(fieldIndexAdjust, out tempPointAdjust))
					{							

						//Pythagors, measure distance of point to the center
						distance = Mathf.Sqrt(x*x + y*y);

						if(brushIsRound)
						{
							//round brush, only adjust height, if its in circle radius
							if(distance >= rangeIndex){ continue; }
						}	

						float t = 0.0f;
						float curveY = 0.0f;

						if(brushIsRound)
						{
							//get point along the curve in UI
							t = (1.0f - distance / rangeIndex);
						}
						else
						{
							if(Mathf.Abs(x) > Mathf.Abs(y))
							{
								t = 1.0f - (float)Mathf.Abs(x) / (float)rangeIndex;
							}
							else
							{
								t = 1.0f - (float)Mathf.Abs(y) / (float)rangeIndex;
							}
						}


						
						if(brushType == BrushType.Height || brushType == BrushType.SetHeight || brushType == BrushType.Flatten)
						{
							curveY = curve.Evaluate(t);						
						}
						
						if(brushType == BrushType.Smooth)
						{
							curveY = curveSmooth.Evaluate(t);		
						}
												
						
						pointAdjust = tempPointAdjust.position;
						//Debug.Log(pointAdjust);

						Vector3 sculptPointOriginal = sculptPoints[fieldIndexAdjust].originalPosition; 

						//calculate delta which will be added
						yValue = brushIntensity * brushDirection * curveY;
						finalValue = pointAdjust.y + yValue;	


						if(brushType == BrushType.Height)
						{
							yValue = (finalValue > maxSculptHeight) ? (maxSculptHeight-pointAdjust.y) : yValue;		
							yValue = (finalValue < minSculptHeight) ? (-pointAdjust.y) : yValue;		
							sculptPoints[fieldIndexAdjust].position = new Vector3(pointAdjust.x, pointAdjust.y + yValue, pointAdjust.z);

							//Debug.Log(maxSculptHeight.ToString() + " " + minSculptHeight.ToString() + "  " + finalValue.ToString());
							//Debug.Log(yValue.ToString() + " " + pointAdjust.y.ToString());

							if(keepHighResBrush)
							{sculptPoints[fieldIndexAdjust].keepHighRes = true;}
							else
							{sculptPoints[fieldIndexAdjust].keepHighRes = false;}
						}
						else if(brushType == BrushType.Smooth)
						{							
							float smoothedValue = GetSmoothValue(tempPointAdjust, fieldIndexAdjust);
							float originalY = tempPointAdjust.position.y;
							yValue = originalY - smoothedValue;
							yValue *= curveY * brushIntenstityOne;
							float final = originalY - yValue;
							
							final = (final > maxSculptHeight) ? (maxSculptHeight-pointAdjust.y) : final;		
							final = (final < minSculptHeight) ? (-pointAdjust.y) : final;	
							
							sculptPoints[fieldIndexAdjust].position = new Vector3(pointAdjust.x, final, pointAdjust.z);	
							yValue *= -1.0f;

							//Smoothing never stores details
							sculptPoints[fieldIndexAdjust].keepHighRes = false;
						}						
						else if(brushType == BrushType.SetHeight)
						{								
							if(keepHighResBrush)
							{
								float finalHeight = (setHeightValue) + sculptPointOriginal.y;
								yValue = (setHeightValue * (brushIntenstityOne * 0.04f * curveY));
								float finalValueY = yValue + sculptPoints[fieldIndexAdjust].position.y ;

								//Checks for undo
								if((finalValueY)  > finalHeight)
								{
									float over = finalValueY - finalHeight;
									yValue -= over;

									/*
									if(yValue < 0.0f)
									{
										yValue = 0.0f;
									}*/
								}

								finalValueY = Mathf.Clamp(finalValueY, minSculptHeight, finalHeight);

								sculptPoints[fieldIndexAdjust].position = new Vector3(pointAdjust.x, finalValueY, pointAdjust.z);
								sculptPoints[fieldIndexAdjust].keepHighRes = true;

							}
							else
							{
								yValue = (setHeightValue - pointAdjust.y) * brushIntenstityOne * curveY;
								sculptPoints[fieldIndexAdjust].position = new Vector3(pointAdjust.x, pointAdjust.y + yValue, pointAdjust.z);
								sculptPoints[fieldIndexAdjust].keepHighRes = false;
							}

							
							if(keepHighResBrush)
							{sculptPoints[fieldIndexAdjust].keepHighRes = true;}
							else
							{sculptPoints[fieldIndexAdjust].keepHighRes = false;}
						}
						else if(brushType == BrushType.Flatten)
						{		
							//Get Center Point, thats reference for flattening every other point
							//Get delta between center point and current point. Adjust this delta down to 0 if its suppoed totaly flattened, ie points are set to be same y Postion as center if to be totaly flattened

							SculptPoint centerPoint = new SculptPoint();					
							Vector2 center = new Vector2(indexPositionAdjust.x + (1*stepSizeSculpt), indexPositionAdjust.y + (1*stepSizeSculpt));					

							if(sculptPoints.TryGetValue(center, out centerPoint))
							{
								float deltaY = centerPoint.position.y - pointAdjust.y;
								float flattenValue = deltaY * curveY;
								flattenValue *= brushIntenstityOne * brushDirection;

								yValue = flattenValue;

								yValue = ((pointAdjust.y + yValue) > maxSculptHeight) ? (maxSculptHeight-pointAdjust.y) : yValue;		
								yValue = ((pointAdjust.y + yValue) < minSculptHeight) ? (-pointAdjust.y) : yValue;	
										
								sculptPoints[fieldIndexAdjust].position = new Vector3(pointAdjust.x, pointAdjust.y + yValue, pointAdjust.z);
								sculptPoints[fieldIndexAdjust].keepHighRes = false;
							}
						}




						if(!sculptPointsDeltas[0].ContainsKey(fieldIndexAdjust))
						{
							sculptPointsDeltas[0].Add(fieldIndexAdjust, 0.0f); 	
						}

						sculptPointsDeltas[0][fieldIndexAdjust] += yValue;


						//Used to only update the points which were affected, not all points
						sculptPointsIndexes.Add(fieldIndexAdjust);											
					}
				}
			}
		}
	}
		

	
	//Assign updated positions from sculpt points to mesh vertices. This is only thing which needs to happen to update the mesh
	void UpdateMeshVerts()
	{
		//Go through all existing sculpt points and assign to meshverts updated position from sculpt point. We find the mesh verts through meshPoints dict which holds index of the vert in meshVerts
		List<Vector2> meshDataUpdate = new List<Vector2>();
		meshIndexes.Clear();
		SculptPoint tempPoint;		
		int meshIndex = 0;
		
		//foreach(KeyValuePair<Vector2, SculptPoint> tempPoint in sculptPoints)
		foreach(Vector2 index in sculptPointsIndexes)
		{
			tempPoint = sculptPoints[index];			
			meshDataUpdate = tempPoint.meshData;

			for(int i = 0; i < meshDataUpdate.Count; i++)
			{
				meshIndex = (int)meshDataUpdate[i].x;
				
				if(trianglVerts[meshIndex].ContainsKey(index))
				{
					meshVertsLists[meshIndex][(int)meshDataUpdate[i].y] = tempPoint.position;
					
					if(!meshIndexes.Contains(meshIndex))
					{
						meshIndexes.Add(meshIndex);
					}
				}
			}
		}	
		
	}
	
	void UpdateMeshVertsAll()
	{
		float posTotalY = 0;		
		
		foreach(KeyValuePair<Vector2, SculptPoint> tempPoint in sculptPoints)		
		{
			List<Vector2> meshData = tempPoint.Value.meshData;

			for(int i = 0; i < meshData.Count; i++)
			{
				if(trianglVerts[(int)meshData[i].x].ContainsKey(tempPoint.Key))
				{
					meshVertsLists[(int)meshData[i].x][(int)meshData[i].y] = tempPoint.Value.position;
					posTotalY += tempPoint.Value.position.y;
				}
			}
		}			
	}
		
	void SetHeight()
	{		
		//to find the index in sculpt index, points got stored at index based on reduceDenisty, so we must use same step to get to the same index
		float stepSizeSculpt = reduceDenisty * stepSize;	

			
		for(int y = 0; y < fieldIndexSize.y; y++)
		{
			for(int x = 0; x < fieldIndexSize.x; x++)
			{					
				Vector2 index = new Vector2(x * stepSizeSculpt, y * stepSizeSculpt);
								
				if(sculptPoints.ContainsKey(index))
				{	
					float tempYHeight = setHeightValue - sculptPoints[index].position.y;
					
					sculptPoints[index].position = new Vector3(sculptPoints[index].position.x, sculptPoints[index].position.y + tempYHeight, sculptPoints[index].position.z);					
					
					if(!sculptPointsDeltas[0].ContainsKey(index))
					{
						sculptPointsDeltas[0].Add(index, 0.0f); 	
					}

					sculptPointsDeltas[0][index] += tempYHeight;

					//Used to only update the points which were affected, not all points
					sculptPointsIndexes.Add(index);		
				}				
			}			
		}		

		UpdateMeshVerts();
		UpdateMeshes();
		ResetCollidersEditing();
		
	
		sculptPointsDeltas.Insert(0, new Dictionary<Vector2, float>());
		undoIndex = 0;
	}	
		
	void CreateProjector()
	{
		destroyedProjector = false;
		
		myProjector = new GameObject();
		myProjector.name = "TerrainToolbox_Projector";
		myProjector.AddComponent<Projector>();
		myProjector.transform.localEulerAngles = new Vector3(90,0,0);
		
		projectorComponent = myProjector.GetComponent<Projector>();
		projectorComponent.farClipPlane = 2000.0f;
		projectorComponent.orthographic = true;
		projectorComponent.orthographicSize = brushSize + brushOutline;		

		ProjectorBrush();

		myProjector.hideFlags = HideFlags.HideInHierarchy;
	}
	
	void DestroyProjector()
	{		
		if(myProjector != null)
		{
			DestroyImmediate(myProjector.gameObject);
			myProjector = null;
		}
		
		GameObject projectorTemp = GameObject.Find("TerrainToolbox_Projector");
		
		if(projectorTemp != null)
		{
			DestroyImmediate(projectorTemp.gameObject);	
		}
	}

	void ProjectorBrush()
	{
		if(brushIsRound)
		{
			projectorComponent.material = (Material)Resources.Load("TerrainToolbox/Brush/ProjectorMaterialRound"); 
		}
		else
		{
			projectorComponent.material = (Material)Resources.Load("TerrainToolbox/Brush/ProjectorMaterialSquare"); 
		}
	}
	
	#endregion

	#region Splines

	void InitializeSpline()
	{
		if(spline != null)
		{			
			Debug.Log("has Parent");

			DestroyImmediate(spline.parent);

			foreach(GameObject tempPoint in spline.splinePoints)
			{
				DestroyImmediate(tempPoint);
			}
		}

		spline = new SplineObject();
		spline.parent = new GameObject();
		spline.parent.name = "SplineParent";

		spline.splinePoints = new List<GameObject>();

		Vector3 point1 = new Vector3(0, terrainsParent.transform.position.y, 0);
		Vector3 point2 = new Vector3(50, terrainsParent.transform.position.y, 50);

		GameObject splinePoint1 = new GameObject();
		splinePoint1.name = "SplinePoint_1";
		splinePoint1.transform.position = point1;

		GameObject splinePoint2 = new GameObject();
		splinePoint2.name = "SplinePoint_2";
		splinePoint2.transform.position = point2;

		spline.splinePoints.Add(splinePoint1);
		spline.splinePoints.Add(splinePoint2);
	}

	void AddSplinePoint()
	{

	}

	#endregion

	#region Smoothing
	
	void SmoothSculptIterations()
	{
		for(int i = 0; i < smoothingIterations; i++)
		{
			SmoothSculptPoints();	
		}
	}
	
	void SmoothSculptPoints()
	{		
		float stepSizeSculpt = reduceDenisty * stepSize;
		
		//Loop through all possible points in the field
		for(int x = 0; x < fieldIndexSize.x; x++)
		{
			for(int y = 0; y < fieldIndexSize.y; y++)
			{				
				float xIndex = x*stepSizeSculpt;
				float yIndex = y*stepSizeSculpt;
				
				SculptPoint basePoint;
				float sumY = 0.0f;
				int pointsAmount = 1;

				if(sculptPoints.ContainsKey(new Vector2(xIndex, yIndex)))
				{					
					basePoint = sculptPoints[new Vector2(xIndex, yIndex)];
					sumY += basePoint.position.y;
					
					//Right Point
					xIndex = (x+1)*stepSizeSculpt;
					yIndex = y*stepSizeSculpt;
					SculptPoint rightPoint;
					if(sculptPoints.ContainsKey(new Vector2(xIndex, yIndex)))
					{
						rightPoint = sculptPoints[new Vector2(xIndex, yIndex)];
						sumY += rightPoint.position.y;	
						pointsAmount++;
					}
					
					//TopRight Point
					xIndex = (x+1)*stepSizeSculpt;
					yIndex = (y+1)*stepSizeSculpt;
					SculptPoint topRightPoint;
					if(sculptPoints.ContainsKey(new Vector2(xIndex, yIndex)))
					{
						topRightPoint = sculptPoints[new Vector2(xIndex, yIndex)];
						sumY += topRightPoint.position.y;	
						pointsAmount++;
					}
					
					
					//Top Point
					xIndex = (x)*stepSizeSculpt;
					yIndex = (y+1)*stepSizeSculpt;
					SculptPoint topPoint;
					if(sculptPoints.ContainsKey(new Vector2(xIndex, yIndex)))
					{
						topPoint = sculptPoints[new Vector2(xIndex, yIndex)];
						sumY += topPoint.position.y;	
						pointsAmount++;
					}
					
					//TopLeft Point
					xIndex = (x-1)*stepSizeSculpt;
					yIndex = (y+1)*stepSizeSculpt;
					SculptPoint topLeftPoint;
					if(sculptPoints.ContainsKey(new Vector2(xIndex, yIndex)))
					{
						topLeftPoint = sculptPoints[new Vector2(xIndex, yIndex)];
						sumY += topLeftPoint.position.y;	
						pointsAmount++;
					}
					
					//Left Point
					xIndex = (x-1)*stepSizeSculpt;
					yIndex = y*stepSizeSculpt;
					SculptPoint leftPoint;
					if(sculptPoints.ContainsKey(new Vector2(xIndex, yIndex)))
					{
						leftPoint = sculptPoints[new Vector2(xIndex, yIndex)];
						sumY += leftPoint.position.y;	
						pointsAmount++;
					}
					
					//Bottom Left Point
					xIndex = (x-1)*stepSizeSculpt;
					yIndex = (y-1)*stepSizeSculpt;
					SculptPoint bottomLeftPoint;
					if(sculptPoints.ContainsKey(new Vector2(xIndex, yIndex)))
					{
						bottomLeftPoint = sculptPoints[new Vector2(xIndex, yIndex)];
						sumY += bottomLeftPoint.position.y;	
						pointsAmount++;
					}
					
					//Bottom Point
					xIndex = (x)*stepSizeSculpt;
					yIndex = (y-1)*stepSizeSculpt;
					SculptPoint bottomPoint;
					if(sculptPoints.ContainsKey(new Vector2(xIndex, yIndex)))
					{
						bottomPoint = sculptPoints[new Vector2(xIndex, yIndex)];
						sumY += bottomPoint.position.y;	
						pointsAmount++;
					}
					
					//Bottom Right Point
					xIndex = (x+1)*stepSizeSculpt;
					yIndex = (y-1)*stepSizeSculpt;
					SculptPoint bottomRightPoint;
					if(sculptPoints.ContainsKey(new Vector2(xIndex, yIndex)))
					{
						bottomRightPoint = sculptPoints[new Vector2(xIndex, yIndex)];
						sumY += bottomRightPoint.position.y;	
						pointsAmount++;
					}
					
															
					xIndex = x*stepSizeSculpt;
					yIndex = y*stepSizeSculpt;					
					
					Vector3 temp = sculptPoints[new Vector2(xIndex, yIndex)].position;
					//Apply intensity
					float newValue = (sumY / pointsAmount);
					float original = temp.y;
					float dif = (original - newValue) * smoothAllIntensity;
					float final = original - dif;
					
					temp = new Vector3(temp.x, final, temp.z);
					
					sculptPoints[new Vector2(xIndex, yIndex)].position = temp;//(sumVector / pointsAmount);
					sculptPoints[new Vector2(xIndex, yIndex)].keepHighRes = false;
				}									
			}			
		}
		
		UpdateMeshVertsAll();
		UpdateMeshesAll();
	}
	
	float GetSmoothValue(SculptPoint smoothPoint, Vector2 index)
	{
		float stepSizeSculpt = reduceDenisty * stepSize;		
		
		float xIndex = index.x;
		float yIndex = index.y;

		SculptPoint basePoint = smoothPoint;
		float sumY = 0.0f;
		sumY += basePoint.position.y;
		int pointsAmount = 1;
			

		//Right Point
		xIndex = index.x + stepSizeSculpt;
		yIndex = index.y;
		SculptPoint rightPoint;
		if(sculptPoints.ContainsKey(new Vector2(xIndex, yIndex)))
		{
			rightPoint = sculptPoints[new Vector2(xIndex, yIndex)];
			sumY += rightPoint.position.y;	
			pointsAmount++;
		}

		//TopRight Point			
		xIndex = index.x + stepSizeSculpt;
		yIndex = index.y + stepSizeSculpt;
		SculptPoint topRightPoint;
		if(sculptPoints.ContainsKey(new Vector2(xIndex, yIndex)))
		{
			topRightPoint = sculptPoints[new Vector2(xIndex, yIndex)];
			sumY += topRightPoint.position.y;	
			pointsAmount++;
		}


		//Top Point
		xIndex = index.x;
		yIndex = index.y + stepSizeSculpt;
		SculptPoint topPoint;
		if(sculptPoints.ContainsKey(new Vector2(xIndex, yIndex)))
		{
			topPoint = sculptPoints[new Vector2(xIndex, yIndex)];
			sumY += topPoint.position.y;	
			pointsAmount++;
		}

		//TopLeft Point
		xIndex = index.x - stepSizeSculpt;
		yIndex = index.y + stepSizeSculpt;
		SculptPoint topLeftPoint;
		if(sculptPoints.ContainsKey(new Vector2(xIndex, yIndex)))
		{
			topLeftPoint = sculptPoints[new Vector2(xIndex, yIndex)];
			sumY += topLeftPoint.position.y;	
			pointsAmount++;
		}

		//Left Point
		xIndex = index.x - stepSizeSculpt;
		yIndex = index.y;
		SculptPoint leftPoint;
		if(sculptPoints.ContainsKey(new Vector2(xIndex, yIndex)))
		{
			leftPoint = sculptPoints[new Vector2(xIndex, yIndex)];
			sumY += leftPoint.position.y;	
			pointsAmount++;
		}

		//Bottom Left Point
		xIndex = index.x - stepSizeSculpt;
		yIndex = index.y - stepSizeSculpt;
		SculptPoint bottomLeftPoint;
		if(sculptPoints.ContainsKey(new Vector2(xIndex, yIndex)))
		{
			bottomLeftPoint = sculptPoints[new Vector2(xIndex, yIndex)];
			sumY += bottomLeftPoint.position.y;	
			pointsAmount++;
		}

		//Bottom Point
		xIndex = index.x;
		yIndex = index.y - stepSizeSculpt;
		SculptPoint bottomPoint;
		if(sculptPoints.ContainsKey(new Vector2(xIndex, yIndex)))
		{
			bottomPoint = sculptPoints[new Vector2(xIndex, yIndex)];
			sumY += bottomPoint.position.y;	
			pointsAmount++;
		}

		//Bottom Right Point
		xIndex = index.x + stepSizeSculpt;
		yIndex = index.y - stepSizeSculpt;
		SculptPoint bottomRightPoint;
		if(sculptPoints.ContainsKey(new Vector2(xIndex, yIndex)))
		{
			bottomRightPoint = sculptPoints[new Vector2(xIndex, yIndex)];
			sumY += bottomRightPoint.position.y;	
			pointsAmount++;
		}


					
		float yValue = (sumY / pointsAmount);
		return yValue;
			
	}
	
	
	
	void SmoothBasePoints()
	{
		//Loop through all possible points in the field
		for(int x = 0; x < fieldIndexSize.x; x++)
		{
			for(int y = 0; y < fieldIndexSize.y; y++)
			{				
				float xIndex = x*stepSize;
				float yIndex = y*stepSize;

				BasePoint basePoint;
				float sumY = 0.0f;
				int pointsAmount = 1;

				
				
				if(basePointsNew.ContainsKey(new Vector2(xIndex, yIndex)))
				{					
					basePoint = basePointsNew[new Vector2(xIndex, yIndex)];
					sumY += basePoint.position.y;

					//Right Point
					xIndex = (x+1)*stepSize;
					yIndex = y*stepSize;
					BasePoint rightPoint;
					if(basePointsNew.ContainsKey(new Vector2(xIndex, yIndex)))
					{
						rightPoint = basePointsNew[new Vector2(xIndex, yIndex)];
						sumY += rightPoint.position.y;	
						pointsAmount++;
					}

					//TopRight Point
					xIndex = (x+1)*stepSize;
					yIndex = (y+1)*stepSize;
					BasePoint topRightPoint;
					if(basePointsNew.ContainsKey(new Vector2(xIndex, yIndex)))
					{
						topRightPoint = basePointsNew[new Vector2(xIndex, yIndex)];
						sumY += topRightPoint.position.y;	
						pointsAmount++;
					}


					//Top Point
					xIndex = (x)*stepSize;
					yIndex = (y+1)*stepSize;
					BasePoint topPoint;
					if(basePointsNew.ContainsKey(new Vector2(xIndex, yIndex)))
					{
						topPoint = basePointsNew[new Vector2(xIndex, yIndex)];
						sumY += topPoint.position.y;	
						pointsAmount++;
					}

					//TopLeft Point
					xIndex = (x-1)*stepSize;
					yIndex = (y+1)*stepSize;
					BasePoint topLeftPoint;
					if(basePointsNew.ContainsKey(new Vector2(xIndex, yIndex)))
					{
						topLeftPoint = basePointsNew[new Vector2(xIndex, yIndex)];
						sumY += topLeftPoint.position.y;	
						pointsAmount++;
					}

					//Left Point
					xIndex = (x-1)*stepSize;
					yIndex = y*stepSize;
					BasePoint leftPoint;
					if(basePointsNew.ContainsKey(new Vector2(xIndex, yIndex)))
					{
						leftPoint = basePointsNew[new Vector2(xIndex, yIndex)];
						sumY += leftPoint.position.y;	
						pointsAmount++;
					}

					//Bottom Left Point
					xIndex = (x-1)*stepSize;
					yIndex = (y-1)*stepSize;
					BasePoint bottomLeftPoint;
					if(basePointsNew.ContainsKey(new Vector2(xIndex, yIndex)))
					{
						bottomLeftPoint = basePointsNew[new Vector2(xIndex, yIndex)];
						sumY += bottomLeftPoint.position.y;	
						pointsAmount++;
					}

					//Bottom Point
					xIndex = (x)*stepSize;
					yIndex = (y-1)*stepSize;
					BasePoint bottomPoint;
					if(basePointsNew.ContainsKey(new Vector2(xIndex, yIndex)))
					{
						bottomPoint = basePointsNew[new Vector2(xIndex, yIndex)];
						sumY += bottomPoint.position.y;	
						pointsAmount++;
					}

					//Bottom Right Point
					xIndex = (x+1)*stepSize;
					yIndex = (y-1)*stepSize;
					BasePoint bottomRightPoint;
					if(basePointsNew.ContainsKey(new Vector2(xIndex, yIndex)))
					{
						bottomRightPoint = basePointsNew[new Vector2(xIndex, yIndex)];
						sumY += bottomRightPoint.position.y;	
						pointsAmount++;
					}


					xIndex = x*stepSize;
					yIndex = y*stepSize;					

					Vector3 temp = basePointsNew[new Vector2(xIndex, yIndex)].position;
					//Apply intensity
					float newValue = (sumY / pointsAmount);
					float original = temp.y;
					float dif = (original - newValue) * 1.0f;
					float final = original - dif;

					temp = new Vector3(temp.x, final, temp.z);

					basePointsNew[new Vector2(xIndex, yIndex)].position = temp;//(sumVector / pointsAmount);
				}									
			}			
		}	
	}
	
	
	#endregion
		
	#region Scene GUI

	void OnSceneGUI(SceneView sceneView)
	{	
		if(disabled || terrainsParent == null)
		{ 	
			return;
		}

		//Mouse Editing
		if(terrainSubMode == TerrainEditorSubMode.Edit &&  mode == UiMainMode.TerrainEditor)
		{
			sceneEvent = Event.current;			
			
			//Mouse button lifted, Reset Colliders. Add deltas for Undo
			if(sceneEvent.type == EventType.MouseUp && sceneEvent.button == 0 && !sceneEvent.alt)
			{				
				if(autoUpdateMeshCollider)
				{
					ResetCollidersEditing();					
				}
						
				//if user moved back with undo, but then painted details, remove all redos, since they are rewritten
				if(undoIndex > 0)
				{
					for(int i = 0; i < undoIndex; i++)
					{
						sculptPointsDeltas.RemoveAt(undoIndex-i);	
					}
				}
				
				sculptPointsDeltas.Insert(0, new Dictionary<Vector2, float>());
				undoIndex = 0;				
				
				SceneView.RepaintAll();			
			};					
			
						
			Vector3 hitPos;
			
			mousePos = sceneEvent.mousePosition;
			
			Ray myRay = HandleUtility.GUIPointToWorldRay( mousePos ); 
			RaycastHit hit;	
			
			if(Physics.Raycast(myRay, out hit))
			{
				hitPos = hit.point;		

				if(terrains.Count > 0)
				{
					float stepSizeSculpt = reduceDenisty * stepSize;		

					//clean up the position for the grid step sizes
					hitPos = new Vector3(hitPos.x - (hitPos.x % stepSizeSculpt), hitPos.y, hitPos.z - (hitPos.z % stepSizeSculpt));


					if(myProjector == null)
					{
						CreateProjector();					
					}
					else
					{
						myProjector.transform.position = new Vector3(hitPos.x, terrainPosY + 1500.0f, hitPos.z);
						projectorComponent.orthographicSize = brushSize + brushOutline;					
					}
					
					if(!brushDrag)
					{
						if(sceneEvent.type == EventType.MouseDown)
						{
							mouseDown = true;	
						}
					}
					
					if(sceneEvent.button == 0 && !sceneEvent.alt )
					{
						if(brushDrag)
						{
							mouseDown = true;
						}
						
						hitPoint = hitPos;
						
						if(sceneEvent.shift)
						{
							brushDirection = -1.0f;	
						}
						else
						{
							brushDirection = 1.0f;	
						}
					}				
					else
					{
						if(brushDrag)
						{
							mouseDown = false;	
							hitPoint = Vector3.zero;
						}
					}
				}
				
			}			
			
			
			if(brushDrag)
			{
				if(sceneEvent.button != 0 || sceneEvent.alt || sceneEvent.type != EventType.MouseDrag)
				{
					mouseDown = false;	
					hitPoint = Vector3.zero;
				}
			}
			else
			{
				if(sceneEvent.button != 0 || sceneEvent.alt || sceneEvent.type == EventType.MouseUp)
				{
					mouseDown = false;	
					hitPoint = Vector3.zero;
				}
			}				
			
			
			//Disables selecting objects and selection frame on mouse down
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
			
		}
		
		if(terrainSubMode != TerrainEditorSubMode.Edit || mode != UiMainMode.TerrainEditor)
		{
			if(!destroyedProjector)
			{				
				DestroyProjector();	
				destroyedProjector = true;
			}
		}

		if(terrainSubMode == TerrainEditorSubMode.Spline)
		{
			//spline serves as parent of the whole spline object/points. Its used as reference for everything

			//Access parents transform and rotation
			Transform splineTransform = spline.parent.transform;
			Quaternion handleRotation = (Tools.pivotRotation == PivotRotation.Local) ? splineTransform.rotation : Quaternion.identity;

			//Use Handles to move the parent around
			Vector3 newPosition = Handles.DoPositionHandle(splineTransform.position, handleRotation);

			//Asign the new position back to the
			Undo.RecordObject(spline.parent.transform , "Spline Point");
			spline.parent.transform.position = newPosition;

			Color tempColor = new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.4f);
			Handles.color = tempColor;
			Handles.SphereCap(0, newPosition, Quaternion.identity, 5.0f);


			for(int i = 0; i < spline.splinePoints.Count; i++)
			{
				EditorGUI.BeginChangeCheck();

				Vector3 point = splineTransform.TransformPoint(spline.splinePoints[i].transform.position);
				Vector3 handlePosition = Handles.DoPositionHandle(point, handleRotation);


				if(EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(spline.splinePoints[i].transform, "Spline Point"); 
					spline.splinePoints[i].transform.position = splineTransform.InverseTransformPoint( handlePosition );
				}
			}	

			Handles.DrawLine(spline.splinePoints[0].transform.position + spline.parent.transform.position, spline.splinePoints[1].transform.position + spline.parent.transform.position);

		}

		if(showField)
		{
			//Field Visuals			
			
			//Lock start and end so that start cant be flipped with end
			if(fieldStart.x > fieldEnd.x)
			{
				fieldEnd = new Vector4(fieldStart.x, fieldEnd.y, fieldEnd.z); 
			}
			
			if(fieldStart.z > fieldEnd.z)
			{
				fieldEnd = new Vector4(fieldEnd.x, fieldEnd.y, fieldStart.z); 
			}
			
			//Draw Main Rect
			float terrainParentPosY = terrainsParent.transform.position.y;
			Color cyan = new Color(Color.cyan.r + 0.1f, Color.cyan.g - 0.2f, Color.cyan.b + 0.1f, 0.2f);		
			Handles.color = cyan;				
			Vector3[] fieldRect = new Vector3[4]{fieldStart, new Vector3(fieldEnd.x, terrainParentPosY, fieldStart.z ), fieldEnd, new Vector3(fieldStart.x, terrainParentPosY, fieldEnd.z)};
			Handles.DrawAAConvexPolygon(fieldRect);	
			
			//Draw outer Edge
			Vector3[] linePoints = new Vector3[5]{fieldStart, new Vector3(fieldEnd.x, terrainParentPosY, fieldStart.z ), fieldEnd, new Vector3(fieldStart.x, terrainParentPosY, fieldEnd.z), fieldStart};
			Handles.color = Color.cyan;
			Handles.DrawAAPolyLine(3.0f, linePoints);			
			
			
			//Handles
			//Reset Handles Y postion
			fieldStart = new Vector3(fieldStart.x, terrainParentPosY, fieldStart.z);
			fieldEnd = new Vector3(fieldEnd.x, terrainParentPosY, fieldEnd.z);
			//Draw Handles and update position
			fieldStart = Handles.DoPositionHandle(fieldStart, Quaternion.identity);
			fieldEnd = Handles.DoPositionHandle(fieldEnd, Quaternion.identity);
			
			Handles.color = Color.green;
			Handles.SphereCap(0, fieldStart, Quaternion.identity, 10.0f);
			Handles.color = Color.red;
			Handles.SphereCap(0, fieldEnd, Quaternion.identity, 10.0f);
		}
		
		if(mode == UiMainMode.CreateTerrains && terrainSubModeCreator == TerrainEditorSubModeCreator.Settings)
		{
			Color cyan = new Color(Color.cyan.r + 0.1f, Color.cyan.g - 0.2f, Color.cyan.b + 0.1f, 0.2f);		
			Handles.color = cyan;	
			
			Vector3 pos = startPositionGrid;			
			
			Handles.DrawLine(pos, new Vector3(pos.x + (defaultTerrainSize * gridDimensions.x), pos.y, pos.z));
			Handles.DrawLine(pos, new Vector3(pos.x, pos.y, pos.z + (defaultTerrainSize * gridDimensions.y)));
			Handles.DrawLine(new Vector3(pos.x + (defaultTerrainSize * gridDimensions.x), pos.y, pos.z), new Vector3(pos.x + (defaultTerrainSize * gridDimensions.x), pos.y, pos.z + (defaultTerrainSize * gridDimensions.y)));
			Handles.DrawLine(new Vector3(pos.x, pos.y, pos.z + (defaultTerrainSize * gridDimensions.y)), new Vector3(pos.x + (defaultTerrainSize * gridDimensions.x), pos.y, pos.z + (defaultTerrainSize * gridDimensions.y)));
		}
		
	/*
		foreach(Vector3 tempPoint in tempPoints)
		{			
			Color tempColor = new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.4f);
			Handles.color = tempColor;

			Handles.SphereCap(0, tempPoint, Quaternion.identity, 5.0f);			
		}		
*/	

//		Vector3 size = new Vector3(2,2,2);

//		foreach(KeyValuePair<Vector2, SculptPoint> tempPoint in sculptPoints)
//		{			
//			Color tempColor = new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.4f);
//			Handles.color = tempColor;
//
//			//Handles.SphereHandleCap(0, tempPoint.Value.position, Quaternion.identity, 5.0f, EventType.repaint);
//			Handles.DrawWireCube(tempPoint.Value.position, size);
//		}

//		foreach(KeyValuePair<Vector2, SculptPoint> tempPoint in sculptPoints)
//		{			
//			Color tempColor = new Color(Color.red.r, Color.red.g, Color.red.b, 0.4f);
//			Handles.color = tempColor;
//
//			//Handles.SphereHandleCap(0, tempPoint.Value.position, Quaternion.identity, 5.0f, EventType.repaint);
//			Handles.DrawWireCube(tempPoint.Value.originalPosition, size);
//		}
//
//
//
//		foreach(KeyValuePair<Vector2, BasePoint> tempPoint in basePointsNew)
//		{			
//			Color tempColor = new Color(Color.green.r, Color.green.g, Color.green.b, 0.4f);
//			Handles.color = tempColor;
//
//			Handles.SphereHandleCap(0, tempPoint.Value.position, Quaternion.identity, 5.0f, EventType.repaint);	
//
//		}
//
//		foreach(List<Vector3> mesh in meshVertsLists)
//		{			
//			foreach(Vector3 temp in mesh)
//			{
//				Color tempColor = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.4f);
//				Handles.color = tempColor;
//
//				Handles.DrawWireCube(temp, size);
//			}
//		}	

	}	
	
	#endregion	
	
}







