using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NICE_Networking;
using UnityEngine.UI;

public class NetworkEventTest : MonoBehaviour
{
    [Header("Event Invocation")]

    [SerializeField]
    private NetworkedEvent networkEvent;

    [SerializeField]
    private bool invokeEvent = false;

    [Header("Event Handling")]

    [SerializeField]
    private Text eventHandlerDisplay;    
    private int invocationCounter = 0;

    public void handleEventInvoked()
    {
        invocationCounter++;
        eventHandlerDisplay.text = "Event invoked: " + invocationCounter + " times.";
    }

    private void Update()
    {
        if (invokeEvent)
        {
            networkEvent.invoke();
            invokeEvent = false;
        }
    }
}
