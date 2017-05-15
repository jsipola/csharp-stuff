using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class objThread {
	
	public void getObj(){
		GameObject arrow;
	// Use this for initialization
			//WWW www = new WWW(url);
			//yield return www;
			Debug.Log("Downloading files");
			try {
				using (var client = new System.Net.WebClient())
				{
				client.DownloadFile("http://192.168.1.84:8000/random/arrow_color.obj", @"C:\Users\kone6\Documents\Office\Assets\arrow_color.obj");
				client.DownloadFile("http://192.168.1.84:8000/random/arrow_color.mtl", @"C:\Users\kone6\Documents\Office\Assets\arrow_color.mtl");
				}
			} catch (Exception e) {
				Debug.Log(e);
			}
			arrow = OBJLoader.LoadOBJFile(@"C:\Users\kone6\Documents\Office\Assets\arrow_color.obj");
	}
};

public class objectFetch : MonoBehaviour {
	public string url = "http://192.168.1.84:8000/random/arrow_color.fbx";
	GameObject arrow;
	Thread oThread;
	// Use this for initialization
	void Start () {
//	IEnumerator Start () {
		/*
		WWW www = new WWW(url);
		yield return www;
		
		Debug.Log("Downloading files");
		using (var client = new System.Net.WebClient())
		{
		client.DownloadFile("http://192.168.1.84:8000/random/arrow_color.obj", @"C:\Users\kone6\Documents\Office\Assets\arrow_color.obj");
		client.DownloadFile("http://192.168.1.84:8000/random/arrow_color.mtl", @"C:\Users\kone6\Documents\Office\Assets\arrow_color.mtl");
		}
		arrow = OBJLoader.LoadOBJFile(@"C:\Users\kone6\Documents\Office\Assets\arrow_color.obj");
		*/
		objThread obj = new objThread();
		oThread = new Thread(new ThreadStart(obj.getObj));
		oThread.Start();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.UpArrow)){
			if (arrow != null)
				arrow.transform.Translate(Vector3.forward * 10f * Time.deltaTime);
		}
	}
//	void OnApplicationQuit()
		//oThread.Join();
}
