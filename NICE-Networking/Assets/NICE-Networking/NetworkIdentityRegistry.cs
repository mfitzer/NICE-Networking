using System.Collections;
using System.Collections.Generic;

namespace NICE_Networking
{
    internal class NetworkIdentityRegistry
    {
        private static NetworkIdentityRegistry instance;
        public static NetworkIdentityRegistry Instance
        {
            get
            {
                if (instance == null)
                    instance = new NetworkIdentityRegistry();
                return instance;
            }
        }

        private NetworkIdentityRegistry()
        {
            networkIDRegistry = new Dictionary<string, short>();
        }

        /// <summary>
        /// Stores the network ID associated with a specific transformPath.
        /// </summary>
        private Dictionary<string, short> networkIDRegistry;

        /// <summary>
        /// Gets the network ID assigned to the given transformPath.
        /// </summary>
        /// <param name="transformPath">Path to the NetworkIdentity in the hierarchy.</param>
        /// <returns></returns>
        public short getNetworkID(string transformPath)
        {
            if (networkIDRegistry.ContainsKey(transformPath)) //ID already created for transformPath
                return networkIDRegistry[transformPath];
            else //Create new ID
            {
                short ID = IDCounter++;
                networkIDRegistry.Add(transformPath, ID);
                return ID;
            }
        }

        /// <summary>
        /// Used to create a unique ID.
        /// </summary>
        private short IDCounter = 0;
    }
}