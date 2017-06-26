////////////////////
//// Based on AssetStore Asset: Point Cloud Free Viewer
//// url: https://www.assetstore.unity3d.com/en/#!/content/19811
////////////////////
using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Globalization;

public class cubePCD : MonoBehaviour {

    public Vector3 defaultScale = new Vector3(10,10,10);
    // File
    public string dataPath;
	private string filename;
	public Material matVertex;

	// GUI
	private float progress = 0;
	private string guiText;
	private bool loaded = false;

	// PointCloud
	private GameObject pointCloud;

	public float scale = 1;
	public bool invertYZ = false;
	public bool forceReload = false;
	//public bool enable = false;
	
	public int numPoints;
	public int numPointGroups;
	private int limitPoints = 65000;

	private Vector3[] points;
	private Color32[] colors;
	private Vector3 minValue;
	
	private Mesh newMesh = null;
	
	GameObject cube;
	int counter = 0;

	
	public void Start () {
		// Create Resources folder
		//createFolders ();

		// Get Filename
		//filename = Path.GetFileName(dataPath);

		//loadScene ();
	}

	/*
	void Update(){
		if (enable == true) { 
			makeCloud();
			enable = false;
		}
	}
	*/
	
	public void makeCloud(){
		// Create Resources folder
		createFolders ();

		// Get Filename
		filename = Path.GetFileName(dataPath);
		
		cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		
		loadScene ();
		
	}


	void loadScene(){
		// Check if the PointCloud was loaded previously
		if(!Directory.Exists (Application.dataPath + "/Resources/PointCloudMeshes/" + filename)){
			UnityEditor.AssetDatabase.CreateFolder ("Assets/Resources/PointCloudMeshes", filename);
			loadPointCloud ();
		} else if (forceReload){
			UnityEditor.FileUtil.DeleteFileOrDirectory(Application.dataPath + "/Resources/PointCloudMeshes/" + filename);
			UnityEditor.AssetDatabase.Refresh();
			UnityEditor.AssetDatabase.CreateFolder ("Assets/Resources/PointCloudMeshes", filename);
			loadPointCloud ();
		} else
			// Load stored PointCloud
			loadStoredMeshes();
	}
	
	
	void loadPointCloud(){
		// Check what file exists
		if (File.Exists (Application.dataPath + dataPath + ".pcd")) 
			// load off
			StartCoroutine ("loadOFF", dataPath + ".pcd");
		else 
			Debug.Log ("File '" + dataPath + "' could not be found"); 
		
	}
	
	// Load stored PointCloud
	void loadStoredMeshes(){

		Debug.Log ("Using previously loaded PointCloud: " + filename);

		GameObject pointGroup = Instantiate(Resources.Load ("PointCloudMeshes/" + filename)) as GameObject;

        pointGroup.transform.localScale = defaultScale;
        loaded = true;
	}
	
	// Start Coroutine of reading the points from the OFF file and creating the meshes
	IEnumerator loadOFF(string dPath){

		// Read file
		StreamReader sr = new StreamReader (Application.dataPath + dPath);
		//sr.ReadLine (); // OFF
		string[] buffer;
		
		int numPoints = File.ReadAllLines(Application.dataPath + dPath).Length - 11;
		points = new Vector3[numPoints];
		colors = new Color32[numPoints];
		
		for (int i = 0; i< 11; i++){
			buffer = sr.ReadLine ().Split ();
			if (buffer[0].Contains("POINTS")) {
				//numPoints = int.Parse (buffer[1]);
				//points = new Vector3[numPoints];
				//colors = new Color[numPoints];
				break; 
			}
		}
		
		buffer = sr.ReadLine ().Split(); // nPoints, nFaces
		//sr.ReadLine ();
		
		minValue = new Vector3();
		for (int i = 0; i< numPoints; i++){
			
			buffer = sr.ReadLine ().Split ();

			if (!invertYZ)
				points[i] = new Vector3 (float.Parse (buffer[0])*scale, float.Parse (buffer[1])*scale,float.Parse (buffer[2])*scale) ;
			else
				points[i] = new Vector3 (float.Parse (buffer[0])*scale, float.Parse (buffer[2])*scale,float.Parse (buffer[1])*scale) ;
			
			if (buffer.Length == 4) {
				colors[i] = new Color32((byte)((System.Convert.ToUInt32(double.Parse(buffer[3], CultureInfo.InvariantCulture)) >> 16) & 0xFF),
										(byte)((System.Convert.ToUInt32(double.Parse(buffer[3], CultureInfo.InvariantCulture)) >> 8) & 0xFF),
										(byte)((System.Convert.ToUInt32(double.Parse(buffer[3], CultureInfo.InvariantCulture)) >> 0) & 0xFF),
										(byte)((System.Convert.ToUInt32(double.Parse(buffer[3], CultureInfo.InvariantCulture)) >> 24) & 0xFF));
			} else
				colors[i] = Color.yellow;

			// Relocate Points near the origin
			//calculateMin(points[i]);

			// GUI
			progress = i *1.0f/(numPoints-1)*1.0f;
			if (i%Mathf.FloorToInt(numPoints/20) == 0){
				guiText=i.ToString() + " out of " + numPoints.ToString() + " loaded";
				yield return null;
			}
		}

		for (int i=0;i<numPoints;i++){
			CloneAndPlace(points[i], cube, colors[i]);
		}
		/*
		// Instantiate Point Groups
		numPointGroups = Mathf.CeilToInt (numPoints*1.0f / limitPoints*1.0f);

		pointCloud = new GameObject (filename);

		for (int i = 0; i < numPointGroups-1; i ++) {
			InstantiateMesh (i, limitPoints);
			if (i%10==0){
				guiText = i.ToString() + " out of " + numPointGroups.ToString() + " PointGroups loaded";
				yield return null;
			}
		}
		InstantiateMesh (numPointGroups-1, numPoints- (numPointGroups-1) * limitPoints);

		//Store PointCloud
		UnityEditor.PrefabUtility.CreatePrefab ("Assets/Resources/PointCloudMeshes/" + filename + ".prefab", pointCloud);
		*/
		sr.Close();
		loaded = true;
	}
	
	public static void CloneAndPlace(Vector3 newPosition, GameObject originalGameobject, Color32 color)
	{// http://in2gpu.com/2014/08/25/build-minecraft-unity-part4-worldgen/
		// Clone
		GameObject clone = (GameObject)Instantiate(originalGameobject, newPosition, Quaternion.identity);
		
		
		// Place
		clone.transform.position = newPosition;
		//MeshRenderer render = clone.GetComponent<MeshRenderer>();
		//Material mat = new Material(Shader.Find("grey_light"));
		
		//mat.color = color;
		//render.material = mat;
		
		// Rename
		clone.name = "Cube@" + clone.transform.position;
	}
	
	void InstantiateMesh(int meshInd, int nPoints){
		// Create Mesh
		GameObject pointGroup = new GameObject (filename + meshInd);
		pointGroup.AddComponent<MeshFilter> ();
		pointGroup.AddComponent<MeshRenderer> ();
		pointGroup.GetComponent<Renderer>().material = matVertex;

		pointGroup.GetComponent<MeshFilter> ().mesh = CreateMesh (meshInd, nPoints, limitPoints);
		pointGroup.transform.parent = pointCloud.transform;

		// Store Mesh
		UnityEditor.AssetDatabase.CreateAsset(pointGroup.GetComponent<MeshFilter> ().mesh, "Assets/Resources/PointCloudMeshes/" + filename + @"/" + filename + meshInd + ".asset");
		UnityEditor.AssetDatabase.SaveAssets ();
		UnityEditor.AssetDatabase.Refresh();
	}

	Mesh CreateMesh(int id, int nPoints, int limitPoints){
		
		Mesh mesh = new Mesh ();
		
		Vector3[] myPoints = new Vector3[nPoints]; 
		int[] indecies = new int[nPoints];
		Color[] myColors = new Color[nPoints];

		for(int i=0;i<nPoints;++i){
			myPoints[i] = points[id*limitPoints + i] - minValue;
			indecies[i] = i;
			myColors[i] = colors[id*limitPoints + i];
		}

		mesh.vertices = myPoints;
		mesh.colors = myColors;
		mesh.SetIndices(indecies, MeshTopology.Points,0);
		mesh.uv = new Vector2[nPoints];
		mesh.normals = new Vector3[nPoints];
		
		return mesh;
	}
	
	void MakePoints(string[] buffer) {
		Vector3 spot = new Vector3 (float.Parse (buffer[0])*scale, float.Parse (buffer[1])*scale,float.Parse (buffer[2])*scale);
		UpdateMesh(spot);
	}
	
	
	void UpdateMesh(Vector3 newPoints){
		if (newMesh == null){
			pointCloud = new GameObject("PCD-UP");
			newMesh = new Mesh();
			Vector3[] points = new Vector3[65000];
			newMesh.uv = new Vector2[65000];
			newMesh.normals = new Vector3[65000];
			newMesh.vertices = points;
		}
		newMesh = pointCloud.GetComponent<MeshFilter>().mesh;
		Vector3[] verts = newMesh.vertices;
		
		//for (int i=0;i<newPoints.Length;i++){
			
		if ((Array.IndexOf(verts, newPoints)) > -1 ){ 
			
		} else {
			verts[counter] = newPoints;
			counter++;
		}
		if (counter > 65000) { counter = 0;}
		//}
		newMesh.vertices = verts;
		
	}

	void calculateMin(Vector3 point){
		if (minValue.magnitude == 0)
			minValue = point;
		if (point.x < minValue.x)
			minValue.x = point.x;
		if (point.y < minValue.y)
			minValue.y = point.y;
		if (point.z < minValue.z)
			minValue.z = point.z;
	}

	void createFolders(){
		if(!Directory.Exists (Application.dataPath + "/Resources/"))
			UnityEditor.AssetDatabase.CreateFolder ("Assets", "Resources");

		if (!Directory.Exists (Application.dataPath + "/Resources/PointCloudMeshes/"))
			UnityEditor.AssetDatabase.CreateFolder ("Assets/Resources", "PointCloudMeshes");
	}


	void OnGUI(){


		if (!loaded){
			GUI.BeginGroup (new Rect(Screen.width/2-100, Screen.height/2, 400.0f, 20));
			GUI.Box (new Rect (0, 0, 200.0f, 20.0f), guiText);
			GUI.Box (new Rect (0, 0, progress*200.0f, 20), "");
			GUI.EndGroup ();
		}
	}

}
