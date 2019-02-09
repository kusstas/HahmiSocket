using System;
using UnityEngine;
using UnityEngine.Events;

using WebSocketSharp;

public class HahmiSocket : MonoBehaviour
{
    public string LastReceivedData { get; private set; }

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
    private string urlServer = "localhost";

    [SerializeField]
    private int portServer = 4444;

    [SerializeField]
    public DataReceivedEvent OnDataReceived = new DataReceivedEvent();

    private const string wsFormat = "ws://{0}:{1}";

    private string wsUri = "";

    private WebSocket webSocket = null;

    private volatile bool needTryingConnect = true;
    private volatile bool isConnected = false;

    public bool Send(string sendData)
    {
        if (webSocket != null)
        {
            webSocket.Send(sendData);
            return true;
        }

        return false;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        wsUri = string.Format(wsFormat, urlServer, portServer);

        CreateSocket();
        TryConnect();
    }

    private void OnDestroy()
    {
        if (webSocket != null)
        {
            webSocket.Close();
        }
    }

    private void Update()
    {
        if (needTryingConnect && !IsConnected && webSocket != null)
        {
            Debug.Log("Trying connect to HAHMI server...");
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

    private void CreateSocket()
    {
        Debug.Log("Create socket...");
        webSocket = new WebSocket(wsUri);
        webSocket.OnOpen += (sender, e) => {
            IsConnected = true;
        };
        webSocket.OnMessage += (sender, e) => {
            Debug.Log("Received data: " + e.Data);
            LastReceivedData = e.Data;
            if (OnDataReceived != null)
            {
                OnDataReceived.Invoke(e.Data);
            }
        };
        webSocket.OnFailed += (sender, e) => {
            Debug.Log("Failed: " + e);
            TryConnect();
        };
        webSocket.OnError += (sender, e) => {
            Debug.LogError("Error: " + e.Message);
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

    [Serializable]
    public class DataReceivedEvent : UnityEvent<string> { }
}
