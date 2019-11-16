using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NICE_Networking
{
    public class ClientManager : MonoBehaviour
    {
        private static ClientManager instance;
        public static ClientManager Instance
        {
            get
            {
                if (!instance)
                    instance = FindObjectOfType<ClientManager>();
                return instance;
            }
        }

        /// <summary>
        /// List of client IDs for all currently connected clients.
        /// </summary>
        private List<short> connectedClients;

        #region Client Events

        //Declare as a class to serialize paramterized UnityEvent in inspector. (Unity has trouble serializing generics)
        [System.Serializable]
        public class ClientConnectionEvent : UnityEvent<short> { }

        /// <summary>
        /// Called when a new client connects.
        /// </summary>
        public ClientConnectionEvent OnClientConnected;

        /// <summary>
        /// Called when a previously connected client reconnects.
        /// </summary>
        public ClientConnectionEvent OnClientReconnected;

        /// <summary>
        /// Called when a client disconnects.
        /// </summary>
        public ClientConnectionEvent OnClientDisconnected;

        #endregion Client Events

        private void Start()
        {
            connectedClients = new List<short>();

            if (NetworkIdentity.isServer)
            {
                ServerBehaviour.Instance.OnClientConnected += handleClientConnected;
                ServerBehaviour.Instance.OnClientReconnected += handleClientReconnected;
                ServerBehaviour.Instance.OnClientDisconnected += handleClientDisconnected;
            }
        }

        /// <summary>
        /// Handles the event of a new client connecting.
        /// </summary>
        public void handleClientConnected(short clientID)
        {
            if (NetworkIdentity.isServer)
            {
                //Notify other clients of new client
                ServerBehaviour.Instance.sendMessage(MessageFactory.createClientConnectionEventMessage(clientID), clientID, true);

                //Notify new client of existing clients
                foreach (short client in connectedClients)
                {
                    ServerBehaviour.Instance.sendMessage(MessageFactory.createClientConnectionEventMessage(client), true);
                }
            }

            connectedClients.Add(clientID);

            OnClientConnected.Invoke(clientID); //Notify oberservers
        }

        /// <summary>
        /// Handles the event of a client reconnecting.
        /// </summary>
        private void handleClientReconnected(ServerBehaviour.ClientIdentification clientIdentification)
        {
            //Remove any association with the newly created serverside client ID
            handleClientDisconnected(clientIdentification.serversideID);

            //Handle reconnection event for client
            handleClientReconnected(clientIdentification.clientsideID);
        }

        /// <summary>
        /// Handles the event of a client reconnecting.
        /// </summary>
        public void handleClientReconnected(short clientID)
        {
            if (NetworkIdentity.isServer)
                ServerBehaviour.Instance.sendMessage(MessageFactory.createClientReconnectionEventMessage(clientID), clientID, true);

            OnClientReconnected.Invoke(clientID); //Notify observers
        }

        /// <summary>
        /// Handles the event of a client disconnecting.
        /// </summary>
        public void handleClientDisconnected(short clientID)
        {
            if (connectedClients.Remove(clientID)) //Client was connected
            {
                if (NetworkIdentity.isServer)
                    ServerBehaviour.Instance.sendMessage(MessageFactory.createClientDisconnectionEventMessage(clientID), clientID, true);

                OnClientDisconnected.Invoke(clientID); //Notify observers
            }
        }
    }
}
