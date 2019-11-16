using System.Collections.Generic;
using System.Collections;
using Unity.Collections;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using Unity.Collections.LowLevel.Unsafe;

namespace NICE_Networking
{
    [RequireComponent(typeof(ClientManager))]
    public class ClientBehaviour : MonoBehaviour
    {
        private static ClientBehaviour instance;
        public static ClientBehaviour Instance
        {
            get
            {
                if (!instance)
                    instance = FindObjectOfType<ClientBehaviour>();
                return instance;
            }
        }

        #region Client Events

        public delegate void ClientEvent();
        public delegate void ClientEvent<T>(T param);

        /// <summary>
        /// Called when the client recieves the connection event from the server.
        /// </summary>
        public ClientEvent OnConnected;

        /// <summary>
        /// Called when the client recieves the disconnection event from the server.
        /// </summary>
        public ClientEvent OnDisconnected;

        /// <summary>
        /// Called when a connection attempt times out.
        /// </summary>
        public ClientEvent OnConnectionTimedOut;

        /// <summary>
        /// Called when the client ID is set.
        /// </summary>
        public ClientEvent<short> OnClientIDSet;

        #endregion Client Events

        private UdpNetworkDriver networkDriver;
        private NetworkConnection connectionToServer; //Connection to the network
        private NetworkEndPoint networkEndPoint; //Endpoint configured for connecting to the server
        private NetworkPipeline unreliablePipeline; //Pipeline used for transporting packets without a guaranteed order of delivery
        private NetworkPipeline reliablePipeline; //Pipeline used for transporting packets in a reliable order

        /// <summary>
        /// Maximum number of packets that can be inflight at one time in the reliable pipeline (32 is Unity's limit)
        /// See Documentation: https://github.com/Unity-Technologies/multiplayer/blob/master/com.unity.transport/Documentation/pipelines-usage.md
        /// </summary>
        private const int MaxInflightReliablePackets = 32;

        /// <summary>
        /// Indicates if the client is ready for a network connection.
        /// </summary>
        private bool readyForConnection = false;

        #region Client ID

        /// <summary>
        /// Value of the client ID if it has not yet been set.
        /// </summary>
        public const short ID_NOT_SET = -1;

        /// <summary>
        /// Uniquely identifies the client on the network.
        /// </summary>
        public short ClientID
        {
            get
            {
                return clientID;
            }
            set
            {
                clientID = value;
                OnClientIDSet?.Invoke(clientID); //Notify any observers
            }
        }

        private short clientID = ID_NOT_SET;

        #endregion Client ID

        [Tooltip("IP address of the server the client will try to connect to.")]
        [SerializeField]
        private string serverIPAddress = "";

        [Tooltip("Port on which the client will be connecting to the server."), SerializeField]
        private ushort serverPort = 9000;

        [Tooltip("How long the client will wait for a connection event with the server before it cancels the request."), SerializeField]
        private float connectionTimeout = 5f;

        [Tooltip("How many times the client will try to connect to the server.")]
        [SerializeField]
        private int maxConnectionAttempts = 100;
        private int connectionAttemptsMade = 0;

        /// <summary>
        /// Holds messages waiting to be sent to the server.
        /// </summary>
        private Queue<QueuedMessage> messageQueue;

        //Indicates if client is connected to the server
        public bool connectedToServer
        {
            get
            {
                if (connectionToServer.IsCreated)
                {
                    switch (connectionToServer.GetState(networkDriver))
                    {
                        case NetworkConnection.State.AwaitingResponse:
                            return false;
                        case NetworkConnection.State.Connecting:
                            return false;
                        case NetworkConnection.State.Connected:
                            return true;
                        case NetworkConnection.State.Disconnected:
                            if (readyForConnection) //Ready for a connection
                            {
                                Debug.Log("<color=lime><b>[Client]</b></color> Disconnected from server, attempting to reconnect.");
                                connectToServer();
                            }
                            return false;
                        default:
                            return false;
                    }
                }
                else
                {
                    configureNetworkEndpoint();
                }
                return false;
            }
        }

        private void Awake()
        {
            ClientBehaviour[] clients = FindObjectsOfType<ClientBehaviour>();
            if (clients.Length > 1) //Destroy object if a ClientBehaviour already exists
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject); //Stop client from being destroyed when a scene is loaded

            initializeMessageQueue();

            OnConnected += sendQueuedMessages; //Send any queued messages once connected to the server

            setServerIP(); //Sets the server IP address in NetworkSettings

            configure();
        }

        private void Start()
        {
            NetworkSettings.OnServerIPChanged += handleServerIPChanged;
        }

        #region General Network Operations

        #region Connection

        /// <summary>
        /// Sets the IP address of the server the client connects to.
        /// </summary>
        private void setServerIP()
        {
            if (serverIPAddress != "" && serverIPAddress != NetworkSettings.serverIP)
                NetworkSettings.serverIP = serverIPAddress;
        }

        /// <summary>
        /// Sets the port on which the client will try to connect to the server.
        /// </summary>
        private void setServerPort()
        {
            if (serverPort != NetworkSettings.serverPort)
                NetworkSettings.serverPort = serverPort;
        }

        /// <summary>
        /// Configures client to connect to a server.
        /// </summary>
        private void configure()
        {
            //Creates a network driver that can track up to the specified number of packets at a time
            networkDriver = new UdpNetworkDriver(new ReliableUtility.Parameters { WindowSize = MaxInflightReliablePackets });

            //This must use the same pipeline(s) as the server
            unreliablePipeline = networkDriver.CreatePipeline(
                typeof(UnreliableSequencedPipelineStage)
            );

            //This must use the same pipeline(s) as the server
            reliablePipeline = networkDriver.CreatePipeline(
                typeof(ReliableSequencedPipelineStage)
            );

            connectionToServer = default; //Setup up default network connection

            //Network endpoint is configured and ready to be connected to
            if (configureNetworkEndpoint())
            {
                connectToServer();
            }
        }

        /// <summary>
        /// Configure the network endpoint for the server using NetworkSettings.
        /// </summary>
        private bool configureNetworkEndpoint()
        {
            //Set up server address
            if (NetworkSettings.serverIP != NetworkSettings.INVALID_IP) //Server IP is set
            {
                NetworkEndPoint possibleNetEndPoint = NetworkEndPoint.Parse(NetworkSettings.serverIP, NetworkSettings.serverPort);

                networkEndPoint = possibleNetEndPoint;
                readyForConnection = true;
                return true;
            }
            else
            {
                Debug.LogError("<color=lime><b>[Client]</b></color> Server IP address invalid, cannot connect to server.");
            }

            readyForConnection = false;

            return false;
        }

        /// <summary>
        /// Connects the client to the server.
        /// </summary>
        private void connectToServer()
        {
            if (networkEndPoint != null || configureNetworkEndpoint())
            {
                Debug.Log("<color=lime><b>[Client]</b></color> Attempting to connect to server.");
                connectionToServer = networkDriver.Connect(networkEndPoint);

                StartCoroutine(checkForConnectionTimeout());
            }
        }

        /// <summary>
        /// Times out a connection attempt if it stays in the Connecting state past the specified connectionTimeout.
        /// </summary>
        private IEnumerator checkForConnectionTimeout()
        {
            bool timedOut = false;
            float connectingTime = 0; //Time spent trying to connect to the server

            //Client is trying to connect to the server and hasn't timed out yet
            while (connectionToServer.GetState(networkDriver) == NetworkConnection.State.Connecting && !timedOut)
            {
                yield return null;

                //Connecting for less time than the connectionTimeout
                if (connectingTime < connectionTimeout)
                {
                    connectingTime += Time.deltaTime;
                }
                else //Connection timed out
                {
                    disconnectFromServer();
                    OnConnectionTimedOut?.Invoke();

                    timedOut = true;
                    readyForConnection = false;

                    connectionAttemptsMade++;
                }
            }

            //Timed out and have not reached connection attempt limit, try to connect again
            if (timedOut && connectionAttemptsMade < maxConnectionAttempts)
            {
                configureNetworkEndpoint();
                connectToServer();
            }

            //Connected to the server successfully
            if (connectedToServer)
            {
                connectionAttemptsMade = 0; //Reset connection attempt counter
            }
        }

        #endregion Connection

        /// <summary>
        /// Disconnects the client from the server.
        /// </summary>
        public void disconnectFromServer()
        {
            if (connectionToServer.IsCreated)
            {
                networkDriver.Disconnect(connectionToServer);
                networkEndPoint = default;
            }
        }

        /// <summary>
        /// Updates network events.
        /// </summary>
        private void updateNetworkEvents()
        {
            if (networkEndPoint != null)
            {
                //Complete C# JobHandle to ensure network event updates can be processed
                networkDriver.ScheduleUpdate().Complete();
            }
        }

        /// <summary>
        /// Processes each new network event for the connectionToServer.
        /// </summary>
        private void processNetworkEvents()
        {
            bool disconnectedFromServer = false;
            DataStreamReader stream; //Used for reading data from data network events

            //Get network events for the connection
            NetworkEvent.Type networkEvent;
            while ((networkEvent = connectionToServer.PopEvent(networkDriver, out stream)) !=
                   NetworkEvent.Type.Empty)
            {
                if (networkEvent == NetworkEvent.Type.Connect) //Connected to server
                {
                    OnConnected?.Invoke();
                    Debug.Log("<color=lime><b>[Client]</b></color> Connected to the server.");
                }
                else if (networkEvent == NetworkEvent.Type.Data) //Server connection sent data
                {
                    MessageParser.parse(stream); //Parse data
                }
                else if (networkEvent == NetworkEvent.Type.Disconnect) //Disconnected from server
                {
                    Debug.Log("<color=lime><b>[Client]</b></color> Disconnected from the server.");
                    disconnectedFromServer = true;
                }
            }

            //Waiting until now to reset the connection ensures all network events are popped
            if (disconnectedFromServer)
            {
                OnDisconnected?.Invoke();

                //Reset connection to default to avoid stale reference
                connectionToServer = default;
            }
        }

        #region Event Handling

        /// <summary>
        /// Handles the event of the server IP address changing in NetworkSettings.
        /// </summary>
        private void handleServerIPChanged()
        {
            serverIPAddress = NetworkSettings.serverIP;
            disconnectFromServer();
            configureNetworkEndpoint();
            connectToServer();
        }

        private void OnDestroy()
        {
            disconnectFromServer();
            disposeUnmanagedMemory();
        }

        #endregion Event Handling

        private void disposeUnmanagedMemory()
        {
            if (networkDriver.IsCreated)
                networkDriver.Dispose();
        }

        #endregion General Network Operations

        #region Message Sending

        /// <summary>
        /// Initializes the message queues if not already initialized.
        /// </summary>
        private void initializeMessageQueue()
        {
            if (messageQueue == null)
                messageQueue = new Queue<QueuedMessage>();
        }

        /// <summary>
        /// Sends the message to the server.
        /// </summary>
        /// <param name="message">Message being sent.</param>
        /// <param name="guaranteeDelivery">Determines if delivery and delivery order should be guaranteed.</param>
        public void sendMessage(byte[] message, bool guaranteeDelivery = false)
        {
            if (connectedToServer)
            {
                sendMessageToServer(message, guaranteeDelivery);
            }
            else if (guaranteeDelivery) //Queue message if delivery must be guaranteed and not connected to the server
            {
                initializeMessageQueue();
                messageQueue.Enqueue(new QueuedMessage(message, guaranteeDelivery));
            }
        }

        /// <summary>
        /// Sends all messages in messageQueue to the server.
        /// </summary>
        private void sendQueuedMessages()
        {
            int queuedMessages = messageQueue.Count; //Number of messages queued when this method was called

            /* Using queuedMessages instead of messageQueue.Count to avoid an infinite loop caused by messages
            being requeued once the reliable message limit is reached */
            for (int i = 0; i < queuedMessages; i++)
            {
                QueuedMessage queuedMsg = messageQueue.Dequeue();
                sendMessageToServer(queuedMsg.msg, queuedMsg.guaranteeDelivery);
            }
        }

        /// <summary>
        /// Sends message bytes to the server.
        /// </summary>
        private void sendMessageToServer(byte[] msg, bool guaranteeDelivery)
        {
            if (!guaranteeDelivery) //Send message with unreliable pipeline
            {
                //DataStreamWriter is needed to send data
                //using statement makes sure DataStreamWriter memory is disposed
                using (var writer = new DataStreamWriter(msg.Length, Allocator.Temp))
                {
                    writer.Write(msg); //Write msg byte data

                    connectionToServer.Send(networkDriver, unreliablePipeline, writer); //Send msg data to server
                }
            }
            else //Need to send message with reliable pipeline
                sendReliableMessage(msg);
        }

        /// <summary>
        /// Sends a message using the reliable pipeline, queues message if inflight packet limit is reached.
        /// </summary>
        private unsafe void sendReliableMessage(byte[] msg)
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
                networkDriver.GetPipelineBuffers(typeof(ReliableSequencedPipelineStage), connectionToServer, ref tmpReceiveBuffer, ref tmpSendBuffer, ref serverReliableBuffer);
                var serverReliableCtx = (ReliableUtility.SharedContext*)serverReliableBuffer.GetUnsafePtr();

                connectionToServer.Send(networkDriver, reliablePipeline, writer); //Send msg data to server

                // Failed to send with reliability, error code will be ReliableUtility.ErrorCodes.OutgoingQueueIsFull if no buffer space is left to store the packet   
                if (serverReliableCtx->errorCode != 0)
                {                 
                    messageQueue.Enqueue(new QueuedMessage(msg, true)); //Requeue message
                }
            }
        }

        #endregion Message Sending

        private void LateUpdate()
        {
            if (readyForConnection) //The client is done configuring the connection
            {
                updateNetworkEvents();

                if (connectedToServer) //Connected to server
                {
                    processNetworkEvents(); //Process any new network events
                    sendQueuedMessages(); //Send any queued messages
                }
            }
        }
    }

    public struct QueuedMessage
    {
        public byte[] msg;

        /// <summary>
        /// Determines if delivery should be guaranteed. 
        /// </summary>
        public bool guaranteeDelivery;

        public QueuedMessage(byte[] msg, bool guaranteeDelivery = false)
        {
            this.msg = msg;
            this.guaranteeDelivery = guaranteeDelivery;
        }
    }
}