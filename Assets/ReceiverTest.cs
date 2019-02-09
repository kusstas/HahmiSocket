using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiverTest : MonoBehaviour
{
    private HahmiSocket maneuverSocket = null;

    void Start()
    {
        maneuverSocket = HahmiSocket.GetSocketByName("HAHMI_Maneuvers");

       
        maneuverSocket.OnDataReceived += OnReceived;
    }

    public void OnReceived(string message)
    {
        Debug.Log("Test " + message);
    }
}
