using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objectFetch : MonoBehaviour {
	public string url = "http://192.168.1.84:8000/random/arrow_color.fbx";
	GameObject arrow;
	// Use this for initialization
	IEnumerator Start () {
		WWW www = new WWW(url);
		yield return www;
		
		Debug.Log("Downloading files");
		using (var client = new System.Net.WebClient())
		{
		client.DownloadFile("http://192.168.1.84:8000/random/arrow_color.obj", @"C:\Users\kone6\Documents\Office\Assets\arrow_color.obj");
		client.DownloadFile("http://192.168.1.84:8000/random/arrow_color.mtl", @"C:\Users\kone6\Documents\Office\Assets\arrow_color.mtl");
		}
		arrow = OBJLoader.LoadOBJFile(@"C:\Users\kone6\Documents\Office\Assets\arrow_color.obj");
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.UpArrow)){
			if (arrow != null)
				arrow.transform.Translate(Vector3.forward * 10f * Time.deltaTime);
		}
	}
}
