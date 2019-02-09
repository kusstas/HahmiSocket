using UnityEngine;

public class ReceiverTest : MonoBehaviour
{
    private HahmiSocket maneuverSocket = null;

    // Please note that connection is possible in function "Start" and functions which call later
    void Start()
    {
        // access to HahmiSocket
        maneuverSocket = HahmiSocket.GetSocketByName("HAHMI_Maneuvers");

        // Attach my own handler to event "OnDataReceived"
        maneuverSocket.OnDataReceived += OnReceived;

        // Connect to server, this function is asynchronus
        maneuverSocket.Connect();
    }
    

    public void OnReceived(string message)
    {
        // Handle message
        Debug.Log("Message: " + message);
    }
}
