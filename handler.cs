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

public class handler : MonoBehaviour {

	UdpClient listener;
	string server_r =  "192.168.1.84";
	string server_c;
	string server_o;
	string IP;
	//string name;
	Socket soc_1 = null;
	int port_r = 54545;
	int port;
	bool done = true;
	bool makeobject = false;
	gra mygra = null;
	Thread recThread;
	System.Text.ASCIIEncoding encode = new System.Text.ASCIIEncoding();


	// Use this for initialization
	void Start() {
		// start  thread for receiving data
		recThread = new Thread(() => commslisten()); 
		recThread.IsBackground = true;
		recThread.Start();
	}

	// Update is called once per frame
	void Update () {
		if (mygra != null) {
			if (makeobject) {
				mygra.init();
				makeobject = false;
			} else {
				mygra.moveObject();
			}
		}
	}
	
	void commslisten()
	{
		///////////////////////////////////////////////////////
		////// Main function which waits for a msg from an user
		///////////////////////////////////////////////////////
		IPEndPoint EP;
		listener = new UdpClient(8000);
		EP = new IPEndPoint(IPAddress.Any,8000);
		byte[] rec = new byte[100];
		string[] buffer;
		string data;
		
		Thread.Sleep(1000);
		while (done)
		{
			try {
				print("WAITING FOR INCOMING MESSAGES");
				rec = listener.Receive(ref EP); // blocking call !!!
				data = encode.GetString(rec);
				print("REC:"+data);
				buffer = data.Split();
				if (buffer[0] == "make")
				{
					init(buffer[1]);
				}
				print("CER:");
				Thread.Sleep(10);
			} catch (Exception e) {
				print("MAIN::"+e.ToString());
			}
		}
	}

	void init(string name)
	{
		///////////////////////////////////////////////////////
		////// Initialize the gra object and give the arguments
		///////////////////////////////////////////////////////
		if (soc_1 == null) {
			soc_1 = makeSocket(server_r, port_r);
		}
		// object and coordinate servers
		// in format ip:port
		server_o = getUrl(name+".object",soc_1);
		if (server_o == null) {
			return;
		}
		server_c = getUrl(name+".pos",soc_1);
		if (server_c == null) {
			return;
		}
		// split as "ip" and "port"
		string[] server_oo = server_o.Split(new Char[] {':'});
		print(server_oo[0]+ " " + server_oo[1]);
		string[] server_cc = server_c.Split(new Char[] {':'});
		print(server_cc[0]+ " " + server_cc[1]);
		
		if (mygra == null){
			mygra = new gra();
		}
		// ip of the object server
		mygra.DownloadLink = server_oo[0];
		//DownloadLink = server_oo[0];
		
		//mygra.port = Int32.Parse(server_oo[1]);
		//port = Int32.Parse(server_oo[1]);
		
		// ip of the coordinate server
		mygra.IP = server_cc[0];
		//IP = server_cc[0];
		// port of the coords server
		//mygra.port = server_cc[1];

		mygra.name = name;
		makeobject = true;
	}

	Socket makeSocket(string serv, int port){
		////////////////////////////////////////////////////////
		////// Makes a socket for the specified address and port
		////////////////////////////////////////////////////////
		try {
			IPEndPoint EP;
			EP = new IPEndPoint(IPAddress.Parse(serv),port);
			Socket sok = new Socket(EP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			sok.Connect(EP);
			return sok;
		} catch (Exception e) {
			print("SOCKET::"+ e.ToString());
			return null;
		}
	}

	string getUrl(string msg,Socket soc){
		////////////////////////////////////////////////////////
		/////// Sends a msg to the socket and reads the response
		////////////////////////////////////////////////////////
		try {
			soc.Send(Encoding.UTF8.GetBytes(msg+"\n"));
			byte[] rec = new byte[100];
			soc.Receive(rec);
			string data = encode.GetString(rec);
			print("DATA: "+ data);
			return data;
		} catch (Exception e) {
			print("URL::"+ e.ToString());
			return null;
		}
	}

	public void OnApplicationQuit(){
		///////////////////////////////
		//// Application end procedures
		///////////////////////////////
		try 
		{
			done = false;
			listener.Close();
			soc_1.Close();
			recThread.Abort();
			mygra.endProcesses();
		} catch(Exception e) 
		{
			print("Problem closing application: "+ e.ToString());
		}
		print("Stopped"); 
    }
}
