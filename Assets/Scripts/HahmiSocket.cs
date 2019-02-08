using System;
using System.Collections;
using UnityEngine;

using WebSocketSharp;

public class HahmiSocket : MonoBehaviour
{
    [SerializeField]
    private string urlServer = "localhost";

    public bool IsConnected {
        get {
            return isConnected;
        }
        set {
            if (isConnected == value)
            {
                return;
            }
            isConnected = value;
        }
    }

    [SerializeField]
    private int portServer = 4444;

    [SerializeField]
    private float periodTryingConnect = 2.0f; 

    private const string wsFormat = "ws://{0}:{1}";

    private string wsUri = "";

    private WebSocket webSocket = null;

    private bool isConnected = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        wsUri = string.Format(wsFormat, urlServer, portServer);
        webSocket = new WebSocket(string.Format(wsFormat, urlServer, portServer));

        Debug.Log(string.Format("Hahmi socket awake: {0}:{1}", urlServer, portServer));

        StartCoroutine("TryConnect");
    }

    IEnumerator TryConnect()
    {
        while (!isConnected)
        {
            Debug.Log("Trying to connect HAHMI server...");
            try
            {
                webSocket.Connect();
                Debug.Log("Connect is successfully");
                isConnected = true;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }

            yield return new WaitForSecondsRealtime(periodTryingConnect);
        }

        yield return null;
    }
}
