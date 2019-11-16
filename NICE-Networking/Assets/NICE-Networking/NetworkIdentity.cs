using System;
using System.Collections.Generic;
using UnityEngine;

namespace NICE_Networking
{
    public enum NetworkAuthority { CLIENT, SERVER }

    public class NetworkIdentity : MonoBehaviour
    {
        public static bool isClient
        {
            get
            {
                //Check for an instance of ClientBehaviour
                return ClientBehaviour.Instance;
            }
        }

        public static bool isServer
        {
            get
            {
                //Check for an instance of ServerBehaviour
                return ServerBehaviour.Instance;
            }
        }

        #region Message Sending

        /// <summary>
        /// Identity of this object on the network.
        /// </summary>
        public short networkID
        {
            get
            {
                return ID;
            }
            set
            {
                ID = value;
                sendQueuedMessages();
            }
        }
        private short ID = ID_NOT_SET;
        public const short ID_NOT_SET = -1;

        /// <summary>
        /// Messages queued to send once the messages can be sent.
        /// </summary>
        private Queue<QueuedMessage> queuedMessages;

        /// <summary>
        /// Tracks all attached NetworkBehaviours which have initialized SyncVars.
        /// </summary>
        private Dictionary<Type, List<NetworkBehaviour>> syncVarNetworkBehaviours;

        #endregion Message Sending

        [SerializeField, Tooltip("Which network entity has authority over the GameObject.")]
        private NetworkAuthority NetworkAuthority = NetworkAuthority.SERVER;

        /// <summary>
        /// Which network entity has authority over the GameObject.
        /// </summary>
        public NetworkAuthority networkAuthority
        {
            get
            {
                return NetworkAuthority;
            }
            set
            {
                NetworkAuthority = value;
                OnNetworkAuthorityChanged?.Invoke(NetworkAuthority); //Notify observers
            }
        }

        public delegate void NetworkIdentityEvent<T>(T param);

        /// <summary>
        /// Called when the network authority is changed.
        /// </summary>
        public NetworkIdentityEvent<NetworkAuthority> OnNetworkAuthorityChanged;

        private void Awake()
        {
            queuedMessages = new Queue<QueuedMessage>();
            syncVarNetworkBehaviours = new Dictionary<Type, List<NetworkBehaviour>>();
        }

        private void Start()
        {
            NetworkEntityManager.Instance.add(this);
        }

        //Update the network with the gameObject state when enabled
        private void OnEnable()
        {
            if (gameObject.activeSelf)
                sendGameObjectState();
        }

        //Update the network with the gameObject state when disabled
        private void OnDisable()
        {
            if (!gameObject.activeSelf)
                sendGameObjectState();
        }

        /// <summary>
        /// Sends the gameobject state over the network.
        /// </summary>
        private void sendGameObjectState()
        {
            if (networkAuthority == NetworkAuthority.CLIENT && isClient)
                sendMessage(MessageFactory.createSetActiveMessage(this, true), true);
            else if (networkAuthority == NetworkAuthority.SERVER && isServer)
                sendMessage(MessageFactory.createSetActiveMessage(this, false), true);
        }

        #region MessageSending

        /// <summary>
        /// Sends the message over the network.
        /// </summary>
        /// <param name="message">Message being sent.</param>
        /// <param name="guaranteeDelivery">Determines if delivery and delivery order should be guaranteed.</param>
        public void sendMessage(byte[] message, bool guaranteeDelivery = false)
        {
            if (message != null && message.Length > 0)
            {
                if (networkID != ID_NOT_SET) //Network ID is set, send message
                    sendMessageOverNetwork(message, guaranteeDelivery);
                else if (guaranteeDelivery) //Queue message until it can be sent
                    queuedMessages.Enqueue(new QueuedMessage(message, guaranteeDelivery));
            }
        }

        /// <summary>
        /// Sends any messages that were queued before the NetworkID was set.
        /// </summary>
        private void sendQueuedMessages()
        {
            foreach (QueuedMessage queuedMsg in queuedMessages)
            {
                sendMessageOverNetwork(updateNetworkID(queuedMsg.msg), queuedMsg.guaranteeDelivery);
            }
        }

        /// <summary>
        /// Sends the message over the network depending on if this is a client or server.
        /// </summary>
        /// <param name="message">Message being sent.</param>
        /// <param name="guaranteeDelivery">Determines if delivery and delivery order should be guaranteed.</param>
        private void sendMessageOverNetwork(byte[] message, bool guaranteeDelivery)
        {
            if (isServer)
                ServerBehaviour.Instance.sendMessage(message, guaranteeDelivery);
            else if (isClient)
                ClientMessageSender.Instance.sendMessage(message, guaranteeDelivery);
        }

        /// <summary>
        /// Updates the network ID in the message.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns>New message bytes.</returns>
        private byte[] updateNetworkID(byte[] msg)
        {
            byte msgHeader = ObjectSerializer.deserializeByte(ref msg);
            short oldNetID = ObjectSerializer.deserializeShort(ref msg);

            List<byte> updatedMsg = new List<byte>();
            updatedMsg.Add(msgHeader); //Reserialize the msg header
            updatedMsg.AddRange(networkID.serialize()); //Serialize new network ID
            updatedMsg.AddRange(msg); //Serialize the rest of the message
            return updatedMsg.ToArray();
        }

        #endregion MessageSending

        #region SyncVar Network Behaviours

        /// <summary>
        /// Ensures that the network behaviour will be able to track SyncVars.
        /// </summary>
        /// <param name="networkBehaviour">NetworkBehaviour whose SyncVars will be tracked.</param>
        internal void trackSyncVars<T>(T networkBehaviour) where T : NetworkBehaviour
        {
            Type objType = typeof(T); //Type of the object the SyncVar is on

            if (!syncVarNetworkBehaviours.ContainsKey(objType)) //Add NetworkBehaviour list for objType
                syncVarNetworkBehaviours.Add(objType, new List<NetworkBehaviour>());

            syncVarNetworkBehaviours[objType].Add(networkBehaviour);
        }

        /// <summary>
        /// Gets a NetworkBehaviour of the Type, objType, attached to this NetworkIdentity GameObject.
        /// </summary>
        /// <param name="objType">Type of the NetworkBehaviour being looked for.</param>
        /// <param name="networkBehaviour">Retrieved NetworkBehaviour.</param>
        /// <returns>Bool indicating if the NetworkBehaviour was found.</returns>
        public bool getNetworkBehaviour(Type objType, out NetworkBehaviour networkBehaviour)
        {
            if (syncVarNetworkBehaviours.ContainsKey(objType))
            {
                List<NetworkBehaviour> netBehaviours = syncVarNetworkBehaviours[objType];

                if (netBehaviours.Count > 0)
                {
                    networkBehaviour = netBehaviours[0]; //Return first NetworkBehaviour of type, object type
                    return true;
                }
            }

            networkBehaviour = null;
            return false;
        }

        #endregion SyncVar Network Behaviours

        private void OnDestroy()
        {
            NetworkEntityManager.Instance.remove(this);
        }
    }
}
