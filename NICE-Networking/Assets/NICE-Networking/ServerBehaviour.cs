using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using UnityEngine.Assertions;
using Unity.Networking.Transport.Utilities;
using Unity.Collections.LowLevel.Unsafe;

namespace NICE_Networking
{
    [RequireComponent(typeof(ClientManager))]
    public class ServerBehaviour : MonoBehaviour
    {
        private static ServerBehaviour instance;
        public static ServerBehaviour Instance
        {
            get
            {
                if (!instance)
                    instance = FindObjectOfType<ServerBehaviour>();
                return instance;
            }
        }

        #region Server Events

        public delegate void ServerEvent();
        public delegate void ServerEvent<T>(T param);
        public struct ClientIdentification
        {
            /// <summary>
            /// Client ID previously assigned to the client. This ID is active.
            /// </summary>
            public short clientsideID;

            /// <summary>
            /// Client ID assigned to the client when it reconnected. This ID is not active.
            /// </summary>
            public short serversideID;

            public ClientIdentification(short clientsideID, short serversideID)
            {
                this.clientsideID = clientsideID;
                this.serversideID = serversideID;
            }
        }

        /// <summary>
        /// Called when a client connects to the server. Passes the client ID.
        /// </summary>
        public ServerEvent<short> OnClientConnected;

        /// <summary>
        /// Called when a client reconnects. 
        /// Passes the previously assigned ID and the newly assigned ID.
        /// </summary>
        public ServerEvent<ClientIdentification> OnClientReconnected;

        /// <summary>
        /// Called when a client disconnects from the server. Passes the client ID.
        /// </summary>
        public ServerEvent<short> OnClientDisconnected;

        #endregion Server Events

        private UdpNetworkDriver networkDriver;
        private NativeList<NetworkConnection> networkConnections; //Holds a list of active network connections
        private NetworkPipeline unreliablePipeline; //Pipeline used for transporting packets without a guaranteed order of delivery
        private NetworkPipeline reliablePipeline; //Pipeline used for transporting packets in a reliable order

        /// <summary>
        /// Maximum number of packets that can be inflight at one time in the reliable pipeline (32 is Unity's limit)
        /// See Documentation: https://github.com/Unity-Technologies/multiplayer/blob/master/com.unity.transport/Documentation/pipelines-usage.md
        /// </summary>
        private const int MaxInflightReliablePackets = 32;

        private Dictionary<short, NetworkConnection> connectedClients; //Associates NetworkConnections with assigned clientIDs
        private Dictionary<short, Queue<QueuedMessage>> clientMessageQueues; //Holds messages waiting to be sent to specific clients
        private Queue<QueuedMessage> messageQueue; //Holds messages waiting to be sent to the clients

        /// <summary>
        /// Total number of clients that have connected while the server is running.
        /// </summary>
        private short totalClientsConnected = 0;

        /// <summary>
        /// Indicates if the server has any clients connected to it.
        /// </summary>
        public bool hasConnections
        {
            get
            {
                if (networkConnections.IsCreated) //networkConnections list has been instantiated
                    return networkConnections.Length > 0;

                return false;
            }
        }

        [Tooltip("Port on which the clients will be connecting to the server."), SerializeField]
        private ushort port = 9000;

        /// <summary>
        /// Maximum number of clients that can be connected at one time.
        /// </summary>
        [Tooltip("Maximum number of clients that can be connected at one time."), SerializeField]
        private int maxClients = 16;

        private void Awake()
        {
            ServerBehaviour[] servers = FindObjectsOfType<ServerBehaviour>();
            if (servers.Length > 1) //Destroy object if a ServerBehaviour already exists
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject); //Stop server from being destroyed when a scene is loaded

            initializeMessageQueues();
        }

        private void Start()
        {
            setServerPort();

            configure();
        }

        #region General Network Operations

        /// <summary>
        /// Sets the port on which the client will try to connect to the server.
        /// </summary>
        private void setServerPort()
        {
            if (port != NetworkSettings.serverPort)
                NetworkSettings.serverPort = port;
        }

        /// <summary>
        /// Configures server to allow client connections.
        /// </summary>
        private void configure()
        {
            //Creates a network driver that can track up to 32 packets at a time (32 is the limit)
            //https://github.com/Unity-Technologies/multiplayer/blob/master/com.unity.transport/Documentation/pipelines-usage.md
            networkDriver = new UdpNetworkDriver(new ReliableUtility.Parameters { WindowSize = 32 });

            //This must use the same pipeline(s) as the client(s)
            unreliablePipeline = networkDriver.CreatePipeline(
                typeof(UnreliableSequencedPipelineStage)
            );

            //This must use the same pipeline(s) as the client(s)
            reliablePipeline = networkDriver.CreatePipeline(
                typeof(ReliableSequencedPipelineStage)
            );

            //Set up network endpoint to accept any Ipv4 connections on port networkSettings.port
            NetworkEndPoint networkEndpoint = NetworkEndPoint.AnyIpv4;
            networkEndpoint.Port = NetworkSettings.serverPort;

            //Binds the network driver to a specific network address and port
            if (networkDriver.Bind(networkEndpoint) != 0)
            {
                Debug.LogError("<color=magenta><b>[Server]</b></color> Failed to bind to port " + NetworkSettings.serverPort);
            }
            else //Successfully bound to port 9000
                networkDriver.Listen(); //Start listening for incoming connections

            //Create list that can hold up to the specified number of client connections
            networkConnections = new NativeList<NetworkConnection>(maxClients, Allocator.Persistent);

            //Creates a dictionary that tracks client connections with unique IDs
            connectedClients = new Dictionary<short, NetworkConnection>();
        }

        /// <summary>
        /// Updates network events for processing.
        /// </summary>
        private void updateNetworkEvents()
        {
            //Complete C# JobHandle to ensure network event updates can be processed
            networkDriver.ScheduleUpdate().Complete();
        }

        /// <summary>
        /// Clean up old, stale connections.
        /// </summary>
        private void cleanupConnections()
        {
            for (int i = 0; i < networkConnections.Length; i++)
            {
                if (!networkConnections[i].IsCreated) //Network connection is not created
                {
                    networkConnections.RemoveAtSwapBack(i);
                    --i;
                }
            }
        }

        /// <summary>
        /// Accepts new network connections.
        /// </summary>
        private void acceptNewConnections()
        {
            NetworkConnection newConnection;
            while ((newConnection = networkDriver.Accept()) != default)
            {
                networkConnections.Add(newConnection); //Add new connection to active connections

                Debug.Log("<color=magenta><b>[Server]</b></color> Accepted a connection.");

                assignClientID(newConnection);
            }
        }

        /// <summary>
        /// Assigns a new NetworkConnection a unique clientID.
        /// </summary>
        /// <param name="newConnection"></param>
        private short assignClientID(NetworkConnection newConnection)
        {
            short clientID = totalClientsConnected++;
            connectedClients.Add(clientID, newConnection);

            sendMessageToClient(MessageFactory.createClientIDMessage(clientID), clientID, true);

            OnClientConnected?.Invoke(clientID); //Notify any observers of the new connection

            return clientID;
        }

        /// <summary>
        /// Process any new network events.
        /// </summary>
        private void processNetworkEvents()
        {
            DataStreamReader stream; //Used for reading data from data network events

            if (hasConnections) //There are clients connected
            {
                List<short> clientIDs = new List<short>(connectedClients.Keys);
                for (int i = 0; i < clientIDs.Count; i++)
                {
                    short clientID = clientIDs[i];
                    NetworkConnection connection = connectedClients[clientID];

                    if (!connection.IsCreated)
                        Assert.IsTrue(true);

                    //Get network events for the connection
                    NetworkEvent.Type networkEvent;
                    while ((networkEvent = networkDriver.PopEventForConnection(connection, out stream)) != NetworkEvent.Type.Empty)
                    {
                        if (networkEvent == NetworkEvent.Type.Data) //Connection sent data
                        {
                            MessageParser.parse(stream); //Parse data
                        }
                        else if (networkEvent == NetworkEvent.Type.Disconnect) //Connection disconnected
                        {
                            Debug.Log("<color=magenta><b>[Server]</b></color> Client " + clientID + " disconnected from server.");

                            OnClientDisconnected?.Invoke(clientID); //Notify any observers of the disconnection event

                            clientMessageQueues.Remove(clientID); //Remove any queued messages for the connection

                            connection = default; //This ensures the connections will be cleaned up in cleanupConnections()
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Disconnects all connected clients.
        /// </summary>
        private void disconnectClients()
        {
            for (int i = 0; i < connectedClients.Count; i++)
            {
                networkDriver.Disconnect(networkConnections[i]);
            }
        }

        #region Event Handling

        /// <summary>
        /// Handles the event of the client reconnecting to the server.
        /// This is used for maintaining associations between client IDs and NetworkConnections even if the client disconnects and reconnects.
        /// </summary>
        /// <param name="clientsideID">ID of the client on the client (previously assigned client ID)</param>
        /// <param name="serversideID">ID of the client on the server (newly assigned client ID)</param>
        internal void handleClientReconnected(short clientsideID, short serversideID)
        {
            if (connectedClients.ContainsKey(clientsideID)) //Client ID is already associated with a connection
            {
                if (connectedClients.ContainsKey(serversideID)) //Serverside ID is associated with the active client connection
                {
                    //Associate active connection with the original clientID
                    connectedClients[clientsideID] = connectedClients[serversideID];

                    //Transfer any queued messages for new client to old client
                    if (clientMessageQueues.ContainsKey(serversideID))
                    {
                        initializeClientMessageQueue(clientsideID);

                        //Get messages queued to new client ID
                        Queue<QueuedMessage> queuedMsgs = clientMessageQueues[serversideID];

                        //Transfer queued messages to the original client ID's message queue
                        while (queuedMsgs.Count > 0)
                            clientMessageQueues[clientsideID].Enqueue(queuedMsgs.Dequeue());
                    }

                    //Remove message queue for the serverside ID
                    clientMessageQueues.Remove(serversideID);

                    //Remove the serverside client ID connection
                    connectedClients.Remove(serversideID);

                    Debug.Log("<color=magenta><b>[Server]</b></color> Client " + clientsideID + " reconnected and identified itself as the new client, " + serversideID);

                    OnClientReconnected?.Invoke(new ClientIdentification(clientsideID, serversideID));
                }
            }
        }

        #endregion Event Handling

        #region Memory Management

        private void OnDestroy()
        {
            disconnectClients();
            disposeUnmanagedMemory();
        }

        private void disposeUnmanagedMemory()
        {
            if (networkDriver.IsCreated)
                networkDriver.Dispose();

            if (networkConnections.IsCreated)
                networkConnections.Dispose();
        }

        #endregion Memory Management

        #endregion General Network Operations

        #region Message Sending

        /// <summary>
        /// Initializes the message queues if not already initialized.
        /// </summary>
        private void initializeMessageQueues()
        {
            if (clientMessageQueues == null)
                clientMessageQueues = new Dictionary<short, Queue<QueuedMessage>>();

            if (messageQueue == null)
                messageQueue = new Queue<QueuedMessage>();
        }

        /// <summary>
        /// Initializes a message queue for a client if not already created.
        /// </summary>
        private void initializeClientMessageQueue(short clientID)
        {
            //Create message queue for client if not already created
            if (!clientMessageQueues.ContainsKey(clientID))
                clientMessageQueues.Add(clientID, new Queue<QueuedMessage>());
        }

        /// <summary>
        /// Sends the message to any connected clients.
        /// </summary>
        /// <param name="message">The message being queued.</param>
        /// <param name="guaranteeDelivery">Determines if delivery and delivery order should be guaranteed.</param>
        public void sendMessage(byte[] message, bool guaranteeDelivery = false)
        {
            if (hasConnections) //Send message immediately if clients are connected
            {
                sendMessageToClients(connectedClients, message, guaranteeDelivery);
            }
            else if (guaranteeDelivery) //Only queue the message if delivery must be guaranteed
            {
                initializeMessageQueues();
                messageQueue.Enqueue(new QueuedMessage(message, guaranteeDelivery));
            }
        }

        /// <summary>
        /// Sends the message to all clients except for the clients with IDs listed in excludedClients.
        /// </summary>
        /// <param name="message">The message being queued.</param>
        /// <param name="guaranteeDelivery">Determines if delivery and delivery order should be guaranteed.</param>
        /// <param name="excludedClients">The IDs of the clients who will be excluded from the message.</param>
        /// <returns>Boolean indicating if the message was queued, it won't be queued if a connection could not be found matching clientID.</returns>
        public void sendMessage(byte[] message, List<short> excludedClients, bool guaranteeDelivery = false)
        {
            if (excludedClients.Count > 0)
            {
                foreach (KeyValuePair<short, NetworkConnection> client in connectedClients)
                {
                    short clientID = client.Key;
                    if (!excludedClients.Contains(clientID)) //Client is not one of the clients being excluded
                    {
                        sendMessageToClient(message, clientID, guaranteeDelivery);
                    }
                }
            }
            else //No clients being excluded, send to all clients
            {
                sendMessage(message, guaranteeDelivery);
            }
        }

        /// <summary>
        /// Sends the message to all clients except for the client with ID, excludedClient.
        /// </summary>
        /// <param name="message">The message being queued.</param>
        /// <param name="guaranteeDelivery">Determines if delivery and delivery order should be guaranteed.</param>
        /// <param name="excludedClient">The ID of the client who will be excluded from the message.</param>
        /// <returns>Boolean indicating if the message was queued, it won't be queued if a connection could not be found matching clientID.</returns>
        public void sendMessage(byte[] message, short excludedClient, bool guaranteeDelivery = false)
        {
            foreach (KeyValuePair<short, NetworkConnection> client in connectedClients)
            {
                short clientID = client.Key;
                if (clientID != excludedClient) //Client is not the client being excluded
                {
                    sendMessageToClient(message, clientID, guaranteeDelivery);
                }
            }
        }

        /// <summary>
        /// Sends the message to a client with ID, clientID.
        /// </summary>
        /// <param name="message">The message being queued.</param>
        /// <param name="clientID">The ID of the client the message will be queued for.</param>
        /// <param name="guaranteeDelivery">Determines if delivery and delivery order should be guaranteed.</param>
        /// <returns>Boolean indicating if the message was queued, it won't be queued if a connection could not be found matching clientID.</returns>
        internal bool sendMessageToClient(byte[] message, short clientID, bool guaranteeDelivery = false)
        {
            if (hasConnections) //Send message immediately if clients are connected
            {
                if (connectedClients.ContainsKey(clientID))
                {
                    sendMessageToClient(connectedClients[clientID], clientID, message, guaranteeDelivery);
                    return true;
                }
            }
            else if (guaranteeDelivery) //Only queue the message if delivery must be guaranteed
            {
                initializeClientMessageQueue(clientID); //Message queue has not been created for connection
                initializeMessageQueues();
                clientMessageQueues[clientID].Enqueue(new QueuedMessage(message, guaranteeDelivery));
            }

            return false;
        }

        /// <summary>
        /// Sends queued messages to the desired client(s).
        /// </summary>
        private void sendQueuedMessages()
        {
            #region Messages to All Connected Clients

            int queuedMessages = messageQueue.Count; //Number of messages queued when this method was called

            /* Using queuedMessages instead of messageQueue.Count to avoid an infinite loop caused by messages
            being requeued once the reliable message limit is reached */
            for (int i = 0; i < queuedMessages; i++)
            {
                QueuedMessage queuedMsg = messageQueue.Dequeue();
                sendMessageToClients(connectedClients, queuedMsg.msg, queuedMsg.guaranteeDelivery);
            }

            #endregion Messages to All Connected Clients

            #region Messages to Individual Clients

            //Send any client specific queued messages (messages only going to one client)
            foreach (KeyValuePair<short, Queue<QueuedMessage>> client in clientMessageQueues)
            {
                short clientID = client.Key;

                //Connection found and active
                if (connectedClients.ContainsKey(clientID) && connectedClients[clientID].IsCreated)
                {
                    if (clientMessageQueues.ContainsKey(clientID)) //Message queue has been created for connection
                    {
                        int queuedClientMessages = clientMessageQueues[clientID].Count; //Number of messages queued when this method was called

                        /* Using queuedMessages instead of messageQueue.Count to avoid an infinite loop caused by messages
                        being requeued once the reliable message limit is reached */
                        for (int i = 0; i < queuedMessages; i++)
                        {
                            QueuedMessage queuedMsg = clientMessageQueues[clientID].Dequeue();
                            sendMessageToClient(connectedClients[clientID], clientID, queuedMsg.msg, queuedMsg.guaranteeDelivery);
                        }
                    }
                }
            }

            #endregion Messages to Individual Clients
        }

        /// <summary>
        /// Sends message bytes to the NetworkConnection, connnection.
        /// </summary>
        private void sendMessageToClient(NetworkConnection connection, short clientID, byte[] msg, bool guaranteeDelivery)
        {
            if (!guaranteeDelivery) //Send message with unreliable pipeline
            {
                //DataStreamWriter is needed to send data
                //using statement makes sure DataStreamWriter memory is disposed
                using (var writer = new DataStreamWriter(msg.Length, Allocator.Temp))
                {
                    writer.Write(msg); //Write msg byte data

                    connection.Send(networkDriver, unreliablePipeline, writer); //Send msg data to server
                }
            }
            else //Need to send message with reliable pipeline
                sendReliableMessage(connection, clientID, msg);
        }

        /// <summary>
        /// Sends message bytes to each NetworkConnection in clientConnections.
        /// </summary>
        private void sendMessageToClients(Dictionary<short, NetworkConnection> clientConnections, byte[] msg, bool guaranteeDelivery)
        {
            if (!guaranteeDelivery) //Send message with unreliable pipeline
            {
                //DataStreamWriter is needed to send data
                //using statement makes sure DataStreamWriter memory is disposed
                using (var writer = new DataStreamWriter(msg.Length, Allocator.Temp))
                {
                    writer.Write(msg); //Write msg byte data

                    foreach (KeyValuePair<short, NetworkConnection> client in clientConnections)
                    {
                        client.Value.Send(networkDriver, unreliablePipeline, writer); //Send msg data to client
                    }
                }
            }
            else //Need to send message with reliable pipeline
            {
                foreach (KeyValuePair<short, NetworkConnection> client in clientConnections)
                {
                    sendReliableMessage(client.Value, client.Key, msg); //Send msg data to client
                }
            }
        }

        /// <summary>
        /// Sends a message using the reliable pipeline, queues message if inflight packet limit is reached.
        /// </summary>
        private unsafe void sendReliableMessage(NetworkConnection connection, short clientID, byte[] msg)
        {
            //DataStreamWriter is needed to send data
            //using statement makes sure DataStreamWriter memory is disposed
            using (var writer = new DataStreamWriter(msg.Length, Allocator.Temp))
            {
                writer.Write(msg); //Write msg byte data

                //Below code copied from documentation here: https://github.com/Unity-Technologies/multiplayer/blob/master/com.unity.transport/Documentation~/pipelines-usage.md

                // Get a reference to the internal state or shared context of the reliability
                NativeSlice<byte> tmpReceiveBuffer = default;
                NativeSlice<byte> tmpSendBuffer = default;
                NativeSlice<byte> serverReliableBuffer = default;
                networkDriver.GetPipelineBuffers(typeof(ReliableSequencedPipelineStage), connection, ref tmpReceiveBuffer, ref tmpSendBuffer, ref serverReliableBuffer);
                var serverReliableCtx = (ReliableUtility.SharedContext*)serverReliableBuffer.GetUnsafePtr();

                connection.Send(networkDriver, reliablePipeline, writer); //Send msg data to server

                // Failed to send with reliability, error code will be ReliableUtility.ErrorCodes.OutgoingQueueIsFull if no buffer space is left to store the packet   
                if (serverReliableCtx->errorCode != 0)
                {
                    initializeClientMessageQueue(clientID); //Make sure the client has a message queue
                    clientMessageQueues[clientID].Enqueue(new QueuedMessage(msg, true)); //Requeue message
                }
            }
        }

        #endregion Message Sending

        private void LateUpdate()
        {
            updateNetworkEvents();

            cleanupConnections();

            acceptNewConnections();

            processNetworkEvents();

            sendQueuedMessages();
        }
    }
}