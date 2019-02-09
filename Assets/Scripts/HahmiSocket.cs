using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

using WebSocketSharp;

public class HahmiSocket : MonoBehaviour
{
    public static HahmiSocket GetSocketByName(string name)
    {
        return sockets[name];
    }

    private static Dictionary<string, HahmiSocket> sockets = new Dictionary<string, HahmiSocket>();

    public bool IsConnected
    {
        get
        {
            return isConnected;
        }
        private set
        {
            if (IsConnected == value)
            {
                return;
            }
            isConnected = value;

            if (IsConnected)
            {
                Debug.Log(string.Format("Connect to {0} is successfully", wsUri));
            }
            else
            {
                Debug.Log(string.Format("Disconnect from {0}", wsUri));
            }
        }
    }

    [SerializeField]
    private string domainName = "HAHMI";

    [SerializeField]
    private string urlServer = "localhost";

    [SerializeField]
    private int portServer = 4444;

    public event DataReceivedEvent OnDataReceived;

    private const string wsFormat = "ws://{0}:{1}";

    private string wsUri = "";

    private WebSocket webSocket = null;

    private volatile bool needTryingConnect = false;
    private volatile bool isConnected = false;

    private Mutex messageMutex = new Mutex();
    private Queue<string> messages = new Queue<string>();

    public void Connect()
    {
        TryConnect();
    }

    public void Close()
    {
        DisableTryingConnect();
        webSocket.Close();
    }

    public bool Send(string sendData)
    {
        if (webSocket != null && IsConnected)
        {
            webSocket.Send(sendData);
            return true;
        }

        return false;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (domainName.IsNullOrEmpty())
        {
            throw new Exception("The socket domain name must be filled!");
        }
        if (sockets.ContainsKey(domainName))
        {
            throw new Exception(string.Format("The socket with domain name {0} exists yet!", domainName));
        }
        sockets.Add(domainName, this);

        wsUri = string.Format(wsFormat, urlServer, portServer);

        CreateSocket();
    }

    private void OnDestroy()
    {
        FixedUpdate();
        Close();
        sockets.Remove(domainName);
    }

    private void Update()
    {
        if (needTryingConnect && !IsConnected && webSocket != null)
        {
            Debug.Log(string.Format("Trying connect to {0} ...", webSocket.Url));
            DisableTryingConnect();
            try
            {
                webSocket.ConnectAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                TryConnect();
            }
        }
    }

    private void FixedUpdate()
    {
        if (messages.Count > 0 && OnDataReceived != null)
        {
            messageMutex.WaitOne();
            while (messages.Count > 0)
            {
                string data = messages.Dequeue();
                OnDataReceived.Invoke(data);
            }
            messageMutex.ReleaseMutex();
        }     
    }

    private void CreateSocket()
    {
        Debug.Log(string.Format("Create a socket {0}...", domainName));
        webSocket = new WebSocket(wsUri);
        webSocket.OnOpen += (sender, e) => {
            IsConnected = true;
        };
        webSocket.OnMessage += (sender, e) => {
            Debug.Log(string.Format("Received data on {0}: {1}", domainName, e.Data));
            if (OnDataReceived != null)
            {
                messageMutex.WaitOne();
                messages.Enqueue(e.Data);
                messageMutex.ReleaseMutex();
            }        
        };
        webSocket.OnFailed += (sender, e) => {
            Debug.Log(string.Format("Failed on {0}: {1}", domainName, e));
            TryConnect();
        };
        webSocket.OnError += (sender, e) => {
            Debug.Log(string.Format("Error on {0}: {1}", domainName, e.Message));
            IsConnected = false;
            TryConnect();
        };
        webSocket.OnClose += (sender, e) => {
            IsConnected = false;
            TryConnect();
        };
    }

    private void TryConnect()
    {
        needTryingConnect = true;
    }

    private void DisableTryingConnect()
    {
        needTryingConnect = false;
    }

    public delegate void DataReceivedEvent(string message);
}
