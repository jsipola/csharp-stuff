using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
	public string DownloadLink = "192.168.1.84";
    public int port = 54345;
	int version = 1;

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
    }

	/*private*/
    public void init(){
		////////////////////////////////////////////
		// Initialises the object and makes the socket
		// thread (activated when user presses "make bot" button)
		/////////////////////////////////////////////
		pos = new Vector3(0,0,0);
		rot = new Vector3(0,0,0);
		
		
		cube = downloadObj(); // download object during runtime
		if (cube == null){ // if no object is found on disk or server is inaccessible
			print("Failure loading object");
			EditorUtility.DisplayDialog("Error","No such object found on Disk or remote Server", "OK", "No");
			return;
		}

		objs.Add(name); // add object to the lists
		posit.Add(name,pos); // position dict
		rots.Add(name,rot);	// rotation dict
		
		//cube.name = name;
		
		EP = new IPEndPoint(IPAddress.Parse(IP),port);

		sock = new Socket(EP.AddressFamily, SocketType.Stream, ProtocolType.Tcp); // make tcp socket
		socketList.Add(sock);
		//sock = new Socket(EP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
		try {
			sock.ReceiveTimeout = 4000;
			sock.Connect(EP); // TODO: if failed try later in socket_tick
			sock.Send(Encoding.UTF8.GetBytes("l pos\n")); // send msg to tell server start sending coords 
		} catch (Exception e) {
			print("Problem with the socket: "+ e.ToString());
			print("problem with the socket connection");
		}
		print("CREATING Socket Thread");
		receiveThread = new Thread(() => socket_thread(sock)); // makes thread for receiving data
		receiveThread.IsBackground = true;
		receiveThread.Start();
		threadList.Add(receiveThread);
		print("Started");
		
     }
	 
     void Update() {
		moveObject();
     }

	     
    private void socket_thread(Socket soc){
		////////////////////////////
		//Receives the location data from the server and saves them to dictionaries
		////////////////////////////
        try {
			string[] buffer = "pos 0 0 0 0 0 0".Split();
			while (Thread.CurrentThread.IsAlive){
				byte[] rec = new byte[100];
				soc.Receive(rec);
				data = encode.GetString(rec);
				print("REC:"+data);
				buffer = data.Split();
				string[] model = buffer[0].Split(new Char[] {'.'}); // get the object which the server want to move
				if (model[1] == "pos"){
					pos[0] = float.Parse(buffer[1]);
					pos[1] = float.Parse(buffer[3]); // Y up in unity
					pos[2] = float.Parse(buffer[2]);
					rot[0] = float.Parse(buffer[4]);
					rot[1] = -float.Parse(buffer[6]); // horizontal rotation
					rot[2] = float.Parse(buffer[5]);
					if (objs.Contains(model[0])){
						posit[model[0]] = pos;
						rots[model[0]] = rot;
					}
				}
				Thread.Sleep(10);
			}  
        } catch(Exception e) {
			//sock = null;
			print("EEEE"+e);
		}
    }
	 
	private GameObject downloadObj(){
		//////////////////////////////
		//// Checks if the object is located on directory
		//// if not downloads from the server
		/////////////////////////////
		print("Downloading files");
		string path = System.IO.Path.GetDirectoryName(Application.dataPath);
		bool found = false;
		bool server = true;
		if (File.Exists(path+"/"+name+".obj")){
			found = true;
			print("OBJ file found");
		}
		if (!found){
			using (var client = new System.Net.WebClient())
			{
				try { // downloads object and mtl files from a HTTP server at address "DownloadLink"
					client.DownloadFile("http://"+DownloadLink+":8000/"+name+".obj", path+"/"+name+".obj");
					client.DownloadFile("http://"+DownloadLink+":8000/"+name+".mtl", path+"/"+name+".mtl");
				} catch (Exception e) {
					print("Problem downloading obj: "+ e.ToString());
					server = false;
				}
			}
		}
		path = System.IO.Path.Combine(@path,name+".obj");
		if ((found) || (server)) { // if the object is either downloaded or already on disk 
			GameObject cuube = OBJLoader.LoadOBJFile(path);	
			cuube.name = name; // rename the object
			print("Object loaded");
			return cuube;
		} else {
			print("File not found on disk and download failed");
			GameObject game = null;
			return game;
		}
	}

	private void moveObject(){
		///////////////////////////
		// Movo object in the scene 
		///////////////////////////
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
			//print("Problem moving object: "+ e.ToString());
		}
		
	}
	
     public void OnApplicationQuit(){
		/////////////////////////////
		//// Application end procedures
		/////////////////////////////
		try {
			foreach(var thread in threadList) { // rotates the objects of the scene
				thread.Abort();
			}
		} catch(Exception e) {
			print("Problem ending threads: "+ e.ToString());
		}
		try {
			foreach(var socket in socketList) { // rotates the objects of the scene
				if (socket!=null) socket.Close(); 
			}
		} catch(Exception e) {
			print("Problem ending sockets: "+ e.ToString());
		}
		print("Stop"); 
     }    
}
