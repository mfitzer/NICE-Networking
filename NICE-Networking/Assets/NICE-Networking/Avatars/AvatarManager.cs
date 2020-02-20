using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NICE_Networking
{
    public class AvatarManager : MonoBehaviour
    {
        private static AvatarManager instance;
        public static AvatarManager Instance
        {
            get
            {
                if (!instance)
                    return FindObjectOfType<AvatarManager>();
                return instance;
            }
        }

        /// <summary>
        /// Avatar that will be instantiated for the local player.
        /// </summary>
        [Tooltip("Avatar that will be instantiated for the local player.")]
        public GameObject playerAvatarPrefab;

        /// <summary>
        /// Avatar that will be instantiated to represent other connected clients.
        /// </summary>
        [Tooltip("Avatar that will be instantiated to represent other connected clients.")]
        public GameObject clientAvatarPrefab;

        /// <summary>
        /// How long to wait in seconds before destroying a client avatar that is disconnected from the network.
        /// </summary>
        [Tooltip("How long to wait before destroying a client avatar that is disconnected from the network.")]
        public float disconnectedDestructionDelay = 5f;

        /// <summary>
        /// Tracks all clients who have requested avatars.
        /// </summary>
        private List<short> clientAvatarRequests;

        /// <summary>
        /// Tracks client avatars by client ID.
        /// </summary>
        private Dictionary<short, ClientAvatar> clientAvatarMappings;

        #region Avatar Events

        [System.Serializable]
        public class ClientAvatarEvent : UnityEvent<ClientAvatar> { }

        /// <summary>
        /// Called when a client avatar is first created.
        /// </summary>
        public ClientAvatarEvent OnClientAvatarCreated;

        /// <summary>
        /// Called when a client avatar is destroyed.
        /// </summary>
        public ClientAvatarEvent OnClientAvatarDestroyed;

        #endregion Avatar Events

        private void Start()
        {
            clientAvatarRequests = new List<short>();
            clientAvatarMappings = new Dictionary<short, ClientAvatar>();

            ClientManager.Instance?.OnClientConnected.AddListener(handleClientConnected);
            ClientManager.Instance?.OnClientReconnected.AddListener(handleClientReconnected);
            ClientManager.Instance?.OnClientDisconnected.AddListener(handleClientDisconnected);

            //Initialize the local player if this is a client
            if (NetworkIdentity.isClient)
                ClientBehaviour.Instance.OnClientIDSet += initializePlayer;
        }

        /// <summary>
        /// Performs necessary initializations for player avatar if one is provided.
        /// </summary>
        private void initializePlayer(short clientID)
        {
            //Player needs an avatar
            if (playerAvatarPrefab)
            {
                Instantiate(playerAvatarPrefab).name = getAvatarName(clientID);

                //Notify network that this client has an avatar
                ClientMessageSender.Instance.sendMessage(MessageFactory.createAvatarRequestMessage(true), true);
            }
        }

        #region Event Handling

        /// <summary>
        /// Handles a client request for an avatar.
        /// </summary>
        /// <param name="clientID">ID of the client who requested the avatar.</param>
        internal void handleAvatarRequest(short clientID)
        {
            //Avatar request is for this client, ignore it
            if (NetworkIdentity.isClient && clientID == ClientBehaviour.Instance.ClientID)
                return;

            if (!clientAvatarMappings.ContainsKey(clientID)) //Make sure the client doesn't already have an avatar
            {
                clientAvatarRequests.Add(clientID); //Record client's request for an avatar

                ClientAvatar newAvatar = Instantiate(clientAvatarPrefab).AddComponent<ClientAvatar>();
                newAvatar.clientID = clientID;
                newAvatar.name = getAvatarName(clientID);
                clientAvatarMappings.Add(clientID, newAvatar);

                Debug.Log("<b><color=teal>[AvatarManager]</color></b> Created an avatar for client: " + clientID);

                OnClientAvatarCreated.Invoke(newAvatar); //Notify observers
            }
        }

        /// <summary>
        /// Handles the event of a new client connecting.
        /// </summary>
        private void handleClientConnected(short clientID)
        {
            if (NetworkIdentity.isServer)
            {
                //Forward previous avatar requests to new client
                foreach (short client in clientAvatarRequests)
                {
                    ServerBehaviour.Instance.sendMessageToClient(MessageFactory.createAvatarRequestMessage(false, client), clientID, true);
                }
            }
        }

        /// <summary>
        /// Handles the event of a client reconnecting.
        /// </summary>
        private void handleClientReconnected(short clientID)
        {
            if (clientAvatarRequests.Contains(clientID)) //Client requested an avatar
            {
                Debug.Log("<b><color=teal>[AvatarManager]</color></b> Client " + clientID + " reconnected, reconnecting avatar.");

                if (clientAvatarMappings.ContainsKey(clientID)) //Client avatar still exists
                {
                    ClientAvatar avatar = clientAvatarMappings[clientID];
                    avatar.connected = true;
                    avatar.gameObject.SetActive(true);
                }
                else //Client avatar was already destroyed, create new one
                    handleAvatarRequest(clientID);
            }
        }

        /// <summary>
        /// Handles the event of a client disconnecting.
        /// </summary>
        private void handleClientDisconnected(short clientID)
        {
            if (clientAvatarMappings.ContainsKey(clientID)) //Client requested an avatar
            {
                ClientAvatar avatar = clientAvatarMappings[clientID];
                avatar.connected = false;
                avatar.gameObject.SetActive(false);
                StartCoroutine(destroyAvatar(clientID, disconnectedDestructionDelay)); //Destroy avatar after delay

                Debug.Log("<b><color=teal>[AvatarManager]</color></b> Client disconnected, deactivated avatar for client: " + clientID + ".");
            }
        }

        #endregion Event Handling

        #region Avatar Destruction

        /// <summary>
        /// Destroys a client avatar with the ID, clientID after the given delay if it doesn't reconnect.
        /// </summary>
        private IEnumerator destroyAvatar(short clientID, float delay)
        {
            if (clientAvatarMappings.ContainsKey(clientID))
            {
                float startTime = Time.time;
                ClientAvatar clientAvatar = clientAvatarMappings[clientID];

                while (Time.time - startTime < delay && !clientAvatar.connected)
                {
                    yield return null;
                }

                //Only destroy avatar if it is still disconnected
                if (!clientAvatar.connected)
                    destroyAvatar(clientID);
            }
        }

        /// <summary>
        /// Destroys a client avatar with the ID, clientID.
        /// </summary>
        private void destroyAvatar(short clientID)
        {
            if (clientAvatarMappings.ContainsKey(clientID))
            {
                ClientAvatar clientAvatar = clientAvatarMappings[clientID];
                clientAvatarMappings.Remove(clientID); //Remove references to client avatar
                clientAvatarRequests.Remove(clientID); //Remove record of avatar request
                OnClientAvatarDestroyed.Invoke(clientAvatar); //Notify observers
                Destroy(clientAvatar.gameObject); //Destroy avatar

                Debug.Log("<b><color=teal>[AvatarManager]</color></b> Destroyed avatar for client: " + clientID + ".");
            }
        }

        #endregion Avatar Destruction

        /// <summary>
        /// Returns the name of a client avatar for the given client ID.
        /// </summary>
        public static string getAvatarName(short clientID)
        {
            return "Avatar " + clientID;
        }
    }
}