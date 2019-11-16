using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

namespace NICE_Networking
{
    internal partial class MessageParser
    {
        /// <summary>
        /// Processes the custom message data according to the message header included in the data.
        /// </summary>
        /// <param name="msg">Message to process.</param>
        private static void processCustomMessage(byte msgHeader, byte[] msg)
        {
            switch (msgHeader)
            {
                default:
                    Debug.LogError("Message header invalid: " + msgHeader);
                    break;
            }
        }

        ///Message processing methods go here
    }
}
