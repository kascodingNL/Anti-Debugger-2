using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using WebSocketSharp;

public class SocketClient : MonoBehaviour
{
    public string websocketIP = "ws://127.0.0.1";
    public int websocketPort = 8180;

    public WebSocket ws;

    // Start is called before the first frame update
    void Start()
    {
        ws = new WebSocket("ws://127.0.0.1:8180");

        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("Opened connection!");
        };

        ws.OnMessage += (sender, e) =>
        {
            Debug.Log(e.Data);
        };

        ws.Connect();

        var sending = 102.1481412f;

        ws.Send(ObjectToByteArray(sending.ToString()));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    byte[] ObjectToByteArray(object obj)
    {
        if (obj == null)
            return null;
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream())
        {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }
}
