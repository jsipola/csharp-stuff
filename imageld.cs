using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
 
public class imageld : MonoBehaviour {
     Thread receiveThread;
     Thread iThread; 
     Socket sock;
     //public string IP = "192.168.1.14";
     //public int port = 54345;
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
	StartCoroutine(load_image_plane());

     }

     IEnumerator load_image_plane(){

	 yield return 0;
	 string image_url = "http://192.168.1.14:8080/current.png";//http://192.168.1.14:8080/logo.png
	 //Texture2D  texture = Resources.Load(image_url) as Texture2D;
	 //print("Texture loaded"+texture);
	 WWW www = new WWW(image_url);
        yield return www;
        // assign texture
        //Renderer renderer = GetComponent<Renderer>();
        //renderer.material.mainTexture = www.texture;
	Texture2D  texture = www.texture;
	Material material = new Material(Shader.Find("Diffuse"));
	
	material.mainTexture = texture;
	
	GameObject plane2 = GameObject.CreatePrimitive(PrimitiveType.Plane);
	plane2.transform.position = new Vector3(-1.0f, 1F, 0);
	plane2.transform.eulerAngles = new Vector3(90, 0, 0);
	plane2.transform.localScale = new Vector3(0.1f, -0.1f, 0.1f);
	
        // Apply		    to Plane
     	mr = plane2.GetComponent<MeshRenderer> ();
     	mr.material = material;
    }

    IEnumerator reload_image_plane(){
	done = 0;
	yield return 0;
	 string image_url = "http://192.168.1.14:8080/current.png";//http://192.168.1.14:8080/logo.png
	 //Texture2D  texture = Resources.Load(image_url) as Texture2D;
	 //print("Texture loaded"+texture);
	 WWW www = new WWW(image_url);
         yield return www;
        // assign texture
	 done = 1;
	 mr.material.mainTexture = www.texture;
    }
	
	 
     void Update() {
       	 if(done==1) StartCoroutine(reload_image_plane());
     }

     private void image_thread(){
	 try{
	     while (Thread.CurrentThread.IsAlive){
		 while(done==0)
		     Thread.Sleep(10);		 
	     }
	 } catch(Exception e) {	     
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
     public void OnApplicationQuit()   {
      receiveThread.Abort();
      iThread.Abort();
      if (sock!=null) sock.Close(); 
      print("Stop"); 
     }    
}

