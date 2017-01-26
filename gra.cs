using System.Collections;
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
    public int port = 54345;
    System.Text.ASCIIEncoding encode = new System.Text.ASCIIEncoding();
    private IPEndPoint EP;
    string data = string.Empty;
    public string name = "";
    Vector3 pos;
    Vector3 rot;
    int done = 1;
    MeshRenderer mr;

    public void Start(){
		done = 1;
		init();
    }

     private void init(){
		pos = new Vector3(0,0,0);
		rot = new Vector3(0,0,0);
		EP = new IPEndPoint(IPAddress.Parse(IP),port);
		
		downloadObj("url here"); // download object during runtime
		
		sock = new Socket(EP.AddressFamily, SocketType.Stream, ProtocolType.Tcp); // make tcp socket
		//sock = new Socket(EP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
		try {
			sock.Connect(EP); // TODO: if failed try later in socket_tick
			sock.Send(Encoding.UTF8.GetBytes("l pos\n")); // send msg to start sending
		} catch (Exception e) {
			print("problem with the socket connection");
		}
		print("CREATING Socket Thread");
		receiveThread = new Thread(new ThreadStart(socket_thread(sock))); // makes thread for receiving data
		receiveThread.IsBackground = true;
		receiveThread.Start();
		print("Started");
     }
     void Update() {
		cube.transform.position = pos; // modify downloaded objects location
		cube.transform.eulerAngles = rot;
		 
		//this.gameObject.transform.position = pos;
		//this.gameObject.transform.eulerAngles = rot;
     }

    private void make_socket_thread(string ip, int socket_port){
		IPEndpoint EP_new = new IPEndPoint(IPAddress.Parse(ip),socket_port);
		
		downloadObj("url here"); // download object during runtime
		
		Socket socket = new Socket(EP_new.AddressFamily, SocketType.Stream, ProtocolType.Tcp); // make tcp socket
		try {
			socket.Connect(EP_new); // TODO: if failed try later in socket_tick
			socket.Send(Encoding.UTF8.GetBytes("l pos\n")); // send msg to start sending
		} catch (Exception e) {
			print("problem with the socket connection");
		}
		print("CREATING Socket Thread");
		Thread socketThread = new Thread(new ThreadStart(socket_thread(socket))); // makes thread for receiving data
		socketThread.IsBackground = true;
		socketThread.Start();
	
    }

	     
    private void socket_thread(Socket socket){
        try {
			string[] buffer = "pos 0 0 0 0 0 0".Split();
			while (Thread.CurrentThread.IsAlive){
				byte[] rec = new byte[100];
				socket.Receive(rec);
				data = encode.GetString(rec);
				print("REC:"+data);
				buffer = data.Split();
				if (buffer[0] == "pos"){
					pos[0] = float.Parse(buffer[1]);
					pos[1] = float.Parse(buffer[3]); // Y up in unity
					pos[2] = float.Parse(buffer[2]);
					rot[0] = float.Parse(buffer[4]);
					rot[1] = -float.Parse(buffer[6]); // horizontal rotation
					rot[2] = float.Parse(buffer[5]);
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
	
	private void downloadObj(string urli){
		print("Downloading files");
		using (var client = new System.Net.WebClient())
		{
			try {
				client.DownloadFile("http://192.168.1.84:8000/random/arrow_color.obj", @"C:\Users\kone6\Documents\Office\Assets\arrow_color.obj");
				//client.DownloadFile(urli, @"C:\Users\kone6\Documents\Office\Assets\arrow_color.obj");
				client.DownloadFile("http://192.168.1.84:8000/random/arrow_color.mtl", @"C:\Users\kone6\Documents\Office\Assets\arrow_color.mtl");
			} catch (Exception e) {
				print("Something went wrong when downloading: "+e.ToString());
			}
		}
		cube = OBJLoader.LoadOBJFile(@"C:\Users\kone6\Documents\cube.obj");
		//cube.AddComponent<Liiku scripti>();
		print("Object loaded");
	}
/*	
	public Vector3 getPos(string str){
		if str.Equals("cube")
        {
            return pos;
        }
	}
	
	public Vector3 getRot(string str){
        if str.Equals("cube")
		    return rot;
	}
*/	
     public void OnApplicationQuit()   {
      receiveThread.Abort();
      if (sock!=null) sock.Close(); 
      print("Stop"); 
     }    
}
