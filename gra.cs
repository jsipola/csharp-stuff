﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
 
public class gra : MonoBehaviour {
	GameObject cube;
    Thread receiveThread;
    Thread iThread; 
    Socket sock;
    public string IP = "192.168.1.14";
	public string DownloadLink = "";
    public int port = 54345;
	
	//public string IP_2 = "";
	//public int port_2 = 54345;
	
    System.Text.ASCIIEncoding encode = new System.Text.ASCIIEncoding();
    private IPEndPoint EP;
    string data = string.Empty;
    public string name = "";
    Vector3 pos;
    Vector3 rot;
    int done = 1;
    MeshRenderer mr;
	
	List<string> objs = new List<string>();
	List<Thread> threadList = new List<Thread>();
	List<Socket> socketList = new List<Socket>();
	
	Dictionary<string,Vector3> posit = new Dictionary<string,Vector3>();
	Dictionary<string,Vector3> rots = new Dictionary<string,Vector3>();
	
	
    public void Start(){
		done = 1;
		//init();
    }

	/*private*/
    public void init(){
		/*Initialises the object and makes the socket*/
		pos = new Vector3(0,0,0);
		rot = new Vector3(0,0,0);
		
		
		cube = downloadObj(); // download object during runtime
		objs.Add(name);
		posit.Add(name,pos);
		rots.Add(name,rot);
		
		EP = new IPEndPoint(IPAddress.Parse(IP),port);
		sock = new Socket(EP.AddressFamily, SocketType.Stream, ProtocolType.Tcp); // make tcp socket
		socketList.Add(sock);
		//sock = new Socket(EP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
		try {
			sock.Connect(EP); // TODO: if failed try later in socket_tick
			sock.Send(Encoding.UTF8.GetBytes("l pos\n")); // send msg to start sending
		} catch (Exception e) {
			print("problem with the socket connection");
		}
		print("CREATING Socket Thread");
		receiveThread = new Thread(new ThreadStart(socket_thread)); // makes thread for receiving data
		receiveThread.IsBackground = true;
		receiveThread.Start();
		threadList.Add(receiveThread);
		print("Started");
     }
     void Update() {
		//cube.transform.position = pos; // modify downloaded objects location
		//cube.transform.eulerAngles = rot;
		 
		//this.gameObject.transform.position = pos;
		//this.gameObject.transform.eulerAngles = rot;
		
		moveObject();
		
     }

	     
    private void socket_thread(){
		/*Receives the location data*/
        try {
			string[] buffer = "pos 0 0 0 0 0 0".Split();
			while (Thread.CurrentThread.IsAlive){
				byte[] rec = new byte[100];
				sock.Receive(rec);
				data = encode.GetString(rec);
				print("REC:"+data);
				buffer = data.Split();
				string[] model = buffer[0].Split(new Char[] {'.'});
				
				if (model[1] == "pos"){
					pos[0] = float.Parse(buffer[1]);
					pos[1] = float.Parse(buffer[3]); // Y up in unity
					pos[2] = float.Parse(buffer[2]);
					rot[0] = float.Parse(buffer[4]);
					rot[1] = -float.Parse(buffer[6]); // horizontal rotation
					rot[2] = float.Parse(buffer[5]);
					
					if (model[0] == "cube"){
						posit["cube"] = pos;
						rots["cube"] = rot;
					}
					if (model[0] == "copter"){
						posit["copter"] = pos;
						rots["copter"] = rot;
					}
				}
				Thread.Sleep(10);
			}  
        } catch(Exception e) {
			sock = null;
			print("EEEE"+e);
		}        
     }
	 
    // sendData
    private void sendString(string message)    {
        try {
	    sock.Send( Encoding.UTF8.GetBytes(message));
        }
        catch (Exception err)  {
            print(err.ToString());
        }
    }
	
	private GameObject downloadObj(){
		print("Downloading files");
		using (var client = new System.Net.WebClient())
		{
			try {
				client.DownloadFile("http://"+DownloadLink+":8000/random/arrow_color.obj", @"C:\Users\kone6\Documents\Office\Assets\arrow_color.obj");
				client.DownloadFile("http://192.168.1.84:8000/random/arrow_color.obj", @"C:\Users\kone6\Documents\Office\Assets\arrow_color.obj");
				client.DownloadFile("http://192.168.1.84:8000/random/arrow_color.mtl", @"C:\Users\kone6\Documents\Office\Assets\arrow_color.mtl");
			} catch (Exception e) {
				print("Something went wrong when downloading");
			}
		}
//		GameObject cuube = OBJLoader.LoadOBJFile(@"C:\Users\kone6\Documents\cube.obj");
		GameObject cuube = OBJLoader.LoadOBJFile(@"C:\Users\kone6\arrow_color.obj");
		cuube.name = name; // rename the object

		print("Object loaded");
		return cuube;
	}
	
	private void moveObject(){
		Vector3 pos_vec;
		Vector3 rot_vec;
		try {
			foreach(var entry in objs) { // rotates the objects of the scene
				pos_vec = posit[entry];
				rot_vec = rots[entry];
				GameObject obj = GameObject.Find(entry);
				obj.transform.position = pos_vec;
				obj.transform.eulerAngles = rot_vec;
			}
		} catch(Exception e) {
			
		}
		
	}
	
     public void OnApplicationQuit()   {
      //receiveThread.Abort();
		try {
			foreach(var thread in threadList) { // rotates the objects of the scene
				thread.Abort();
			}
		} catch(Exception e) {
			
		}
		try {
			foreach(var socket in socketList) { // rotates the objects of the scene
				if (socket!=null) socket.Close(); 
			}
		} catch(Exception e) {
			
		}
      //if (sock!=null) sock.Close(); 
      print("Stop"); 
     }    
}