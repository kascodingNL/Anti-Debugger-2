using Assets.Scripts.kascodingNL;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using WebSocketSharp;

public class SocketClient : MonoBehaviour
{
    public string websocketIP = "ws://127.0.0.1:8180";
    public int websocketPort = 8180;

    public WebSocket ws;
    public ISender neededSender;

    //Start is called before the first frame update
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
        var externalip = new WebClient().DownloadString("http://icanhazip.com");
        
        var sending = "Sending handshake from " + externalip;

        ws.Send(ObjectToByteArray(sending));
    }

    #region Send methods
    public void SendData(String type, int sending)
    {
        ws.Send(ObjectToByteArray(type + " " + sending.ToString()));
    }

    public void SendData(String type, string sending)
    {
        ws.Send(ObjectToByteArray(type + " " + sending.ToString()));
    }

    public void SendData(String type, float sending)
    {
        ws.Send(ObjectToByteArray(type + " " + sending.ToString()));
    }

    public void SendData(String type, bool sending)
    {
        ws.Send(ObjectToByteArray(type + " " + sending.ToString()));
    }
    #endregion

    private byte[] ObjectToByteArray(object obj)
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

    public void OnApplicationQuit()
    {
        ws.Close();
    }

    public void Disconnect(ISender sender)
    {
        var caller = new StackFrame(1).GetMethod().Name;
        if(sender.senderHash == neededSender.senderHash)
        {
            ws.Close();
        }
    }

    public void Disconnect(string mCeAl)
    {
        var clAlre = new StackFrame(1).GetMethod().Name;
        if (clAlre == mCeAl)
        {
            ws.Close();
        }
    }
}
