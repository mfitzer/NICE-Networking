using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NICE_Networking
{
    public class ClientConfig : MonoBehaviour
    {
        public InputField IPInput;
        public InputField portInput;

        private void Start()
        {
            IPInput.onEndEdit.AddListener(setServerIP);
            portInput.onEndEdit.AddListener(setServerPort);

            IPInput.text = NetworkSettings.serverIP;
            portInput.text = NetworkSettings.serverPort.ToString();
        }

        /// <summary>
        /// Sets the server IP address on which the client will be connecting to the server.
        /// </summary>
        public void setServerIP(string serverIP)
        {
            NetworkSettings.serverIP = serverIP;
        }

        /// <summary>
        /// Sets the port on which the client will be connecting to the server.
        /// </summary>
        public void setServerPort(string serverPort)
        {
            if (ushort.TryParse(serverPort, out ushort port))
            {
                NetworkSettings.serverPort = port;
            }
        }
    }
}