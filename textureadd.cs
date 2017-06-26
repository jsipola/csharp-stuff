using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class textureadd : MonoBehaviour {
	UdpClient listener;
	IPEndPoint EP;
	bool done = true;
	System.Text.ASCIIEncoding encode = new System.Text.ASCIIEncoding();
	Thread recThread, udpThread;
	volatile bool val = false;
	PointsPCD target;
	cubePCD target1;
	System.Diagnostics.Stopwatch sw;
	// Use this for initialization
	void Start () {

		target = GetComponent<PointsPCD>();
		target.dataPath = @"\test1_pcd";

		target1 = GameObject.Find("CubeCreator").GetComponent<cubePCD>();
		target1.dataPath = @"\test2_pcd";
		
		//MakeCube();
		
		recThread = new Thread(() => readBytes()); 
		recThread.IsBackground = true;
		recThread.Start();
		
		sw = new System.Diagnostics.Stopwatch();

		sw.Start();
		sw.Stop();
		print(sw.Elapsed);

		/*
		udpThread = new Thread(() => ReadUDP()); 
		udpThread.IsBackground = true;
		udpThread.Start();
		*/
	}
	
	
	void readBytes() {
		string data;
		string path = Directory.GetCurrentDirectory();
		Thread.Sleep(1000);
		while (done)
		{
			try {
				if (val == false){
					if (downloadPCD(path)) {
						val = true;
					}
				}
				Thread.Sleep(2000);
			} catch (Exception e) {
				print("MAIN::"+e.ToString());
			}
		}
	}

	bool downloadPCD(string path){
		using (var client = new System.Net.WebClient())
		{
			bool download = false;
			while(download != true) {
				try { // downloads pcd file from a HTTP server"
					Debug.Log("Downloading files");
					client.DownloadFile("http://192.168.10.200:8000/test_pcd.pcd", path+"/Assets/"+"test1_pcd.pcd");
					//client.DownloadFile("http://192.168.1.2:8000/test_pcd.pcd", path+"/Assets/"+"test1_pcd.pcd");
					download = true;
					return true;
				} catch (Exception e) {
					Debug.Log("Problem downloading pcd: "+ e.ToString());


					Thread.Sleep(2000);
					continue;
					return false;
				}
			}
			return false;
		}
	}
	
	void MakePCD(){
		
		try{
			Destroy(GameObject.Find("test1_pcd"));
		} catch (Exception e) {
			print("EEEEE: "+ e.ToString());
		}
		
		Debug.Log("LOADING PCD");
		if (target == null){
			target = GetComponent<PointsPCD>();
			target.dataPath = @"\test1_pcd";
		}
//		target.dataPath = @"\PointCloud\example";
//		target.enable = true;
		target.makeCloud();
		val = false;
	}
	
	void MakeCube(){
		
		Debug.Log("Making CUBES");
		if (target1 == null){
			target1 = GameObject.Find("CubeCreator").GetComponent<cubePCD>();
			target1.dataPath = @"\test1_pcd";
		}
//		target.dataPath = @"\PointCloud\example";
//		target.enable = true;
		target1.makeCloud();
		val = false;
	}	
	
	void ReadUDP()
	{
		IPEndPoint EP;
		listener = new UdpClient(8888);
		EP = new IPEndPoint(IPAddress.Any,8888);
		byte[] rec = new byte[100];
		string[] buffer;
		string data;
		
		Thread.Sleep(1000);
		while (done)
		{
			try {
				print("WAITING FOR INCOMING UDP PCD");
				rec = listener.Receive(ref EP); // blocking call !!!
				data = encode.GetString(rec);
				print("REC:"+data);
				buffer = data.Split();
				print("CER:");
				Thread.Sleep(10);
			} catch (Exception e) {
				print("MAIN::"+e.ToString());
			}
		}
	}
	
	
	
	void Update(){
		if (val == true) {
			MakePCD();
			//MakeCube();
		}
		//target1.MakePoints();
	}
	
	
	public void OnApplicationQuit(){
		///////////////////////////////
		//// Application end procedures
		///////////////////////////////
		try 
		{
			done = false;
			recThread.Abort();
			listener.Close();
			//udpThread.Abort();
		} catch(Exception e) 
		{
			print("Problem closing application: "+ e.ToString());
		}
		print("Stopped"); 
    }
}
