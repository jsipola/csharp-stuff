using System;
using System.Threading;
using System.Net.Sockets;

public class fire {
    GameObject obj;
    Socket socket;
    gra GRA;
    string ip;
    int port;
    Vector3 pos;
    Vector3 rot;


    public void Start(){
        obj = GameObject.Find("Plane");
        GRA = obj.GetComponent<gra>();
    }


    public void Update(){
        pos = GRA.pos;
        rot = GRA.rot;
        this.gameObject.transform.position = pos;
        this.gameObject.transform.eulerAngles = rot;
    }

}
