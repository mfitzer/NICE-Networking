using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NICE_Networking
{
    public class ClientAvatar : MonoBehaviour
    {
        /// <summary>
        /// Is the client connected to the server?
        /// </summary>
        [HideInInspector]
        public bool connected = true;

        /// <summary>
        /// Client ID of associated with this avatar.
        /// </summary>
        [HideInInspector]
        public short clientID;
    }
}