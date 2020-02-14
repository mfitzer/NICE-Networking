using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

namespace NICE_Networking
{
    internal static partial class MessageParser
    {
        /// <summary>
        /// Parses data from streamReader into a byte array for processing.
        /// </summary>
        public static void parse(DataStreamReader streamReader)
        {
            //Tracks where in the data stream you are and how much you've read
            var readerContext = default(DataStreamReader.Context);

            //Attempt to read Message byte array from streamReader
            byte[] msgBytes = streamReader.ReadBytesAsArray(ref readerContext, streamReader.Length);

            processMessage(msgBytes); //Process the message data
        }

        /// <summary>
        /// Processes the message data according to the message header included in the data.
        /// </summary>
        /// <param name="msg">Message to process.</param>
        private static void processMessage(byte[] msg)
        {
            byte msgHeader = ObjectSerializer.deserializeByte(ref msg);

            switch (msgHeader)
            {
                case 0:
                    processTransformMessage(msg, false);
                    break;
                case 1:
                    processTransformMessage(msg, true);
                    break;
                case 2:
                    processRigidbodyMessage(msg, false);
                    break;
                case 3:
                    processRigidbodyMessage(msg, true);
                    break;
                case 4:
                    processSetActiveMessage(msg, false);
                    break;
                case 5:
                    processSetActiveMessage(msg, true);
                    break;
                case 6:
                    processNetworkAuthorityMessage(msg, false);
                    break;
                case 7:
                    processNetworkAuthorityMessage(msg, true);
                    break;
                case 8:
                    processAddJointMessage(msg, false);
                    break;
                case 9:
                    processAddJointMessage(msg, true);
                    break;
                case 10:
                    processRemoveJointMessage(msg, false);
                    break;
                case 11:
                    processRemoveJointMessage(msg, true);
                    break;
                case 12:
                    processClientConnectionEventMessage(msg);
                    break;
                case 13:
                    processClientDisconnectionEventMessage(msg);
                    break;
                case 14:
                    processClientReconnectionEventMessage(msg);
                    break;
                case 15:
                    processClientIDMessage(msg);
                    break;
                case 16:
                    processClientReidentifiedMessage(msg);
                    break;
                case 17:
                    processBaseSyncVarMessage(msg, false);
                    break;
                case 18:
                    processBaseSyncVarMessage(msg, true);
                    break;
                case 19:
                    processColorSyncVarMessage(msg, false);
                    break;
                case 20:
                    processColorSyncVarMessage(msg, true);
                    break;
                case 21:
                    processGameObjectSyncVarMessage(msg, false);
                    break;
                case 22:
                    processGameObjectSyncVarMessage(msg, true);
                    break;
                case 23:
                    processQuaternionSyncVarMessage(msg, false);
                    break;
                case 24:
                    processQuaternionSyncVarMessage(msg, true);
                    break;
                case 25:
                    processVector2SyncVarMessage(msg, false);
                    break;
                case 26:
                    processVector2SyncVarMessage(msg, true);
                    break;
                case 27:
                    processVector3SyncVarMessage(msg, false);
                    break;
                case 28:
                    processVector3SyncVarMessage(msg, true);
                    break;
                case 29:
                    processNetworkIDRequestMessage(msg);
                    break;
                case 30:
                    processNetworkIDMessage(msg);
                    break;
                case 31:
                    processAvatarRequestMessage(msg, false);
                    break;
                case 32:
                    processAvatarRequestMessage(msg, true);
                    break;
                case 33:
                    processNetworkEventMessage(msg, true);
                    break;
                case 34:
                    processNetworkEventMessage(msg, false);
                    break;
                default:
                    processCustomMessage(msgHeader, msg);
                    break;
            }
        }

        #region Misc

        private static void processTransformMessage(byte[] msg, bool clientMessage)
        {
            short netID = ObjectSerializer.deserializeShort(ref msg);

            if (NetworkEntityManager.Instance.findComponent(netID, out NetworkTransform networkTransform)) //NetworkTransform found
            {
                bool receivedPos = ObjectSerializer.deserializeByte(ref msg) == 1;
                if (receivedPos) //Position data was sent
                {
                    float posX = ObjectSerializer.deserializeFloat(ref msg);
                    float posY = ObjectSerializer.deserializeFloat(ref msg);
                    float posZ = ObjectSerializer.deserializeFloat(ref msg);
                    networkTransform.Position = new Vector3(posX, posY, posZ);
                }

                bool receivedRot = ObjectSerializer.deserializeByte(ref msg) == 1;
                if (receivedRot) //Rotation data was sent
                {
                    float rotX = ObjectSerializer.deserializeFloat(ref msg);
                    float rotY = ObjectSerializer.deserializeFloat(ref msg);
                    float rotZ = ObjectSerializer.deserializeFloat(ref msg);
                    networkTransform.Rotation = Quaternion.Euler(new Vector3(rotX, rotY, rotZ));
                }

                bool receivedScale = ObjectSerializer.deserializeByte(ref msg) == 1;
                if (receivedScale) //Scale data was sent
                {
                    float scaleX = ObjectSerializer.deserializeFloat(ref msg);
                    float scaleY = ObjectSerializer.deserializeFloat(ref msg);
                    float scaleZ = ObjectSerializer.deserializeFloat(ref msg);
                    networkTransform.Scale = new Vector3(scaleX, scaleY, scaleZ);
                }

                //Message was sent from a client, now being processed on the server
                if (clientMessage && ServerBehaviour.Instance)
                {
                    //What transform space is this data in? (World / Local)
                    bool worldSpace = ObjectSerializer.deserializeByte(ref msg) == 1;
                    short clientID = ObjectSerializer.deserializeShort(ref msg); //Client who sent the message
                    NetworkTransform.TransformSpace transformSpace = worldSpace ? NetworkTransform.TransformSpace.World : NetworkTransform.TransformSpace.Local;
                    byte[] transformMsg = MessageFactory.createTransformMessage(networkTransform.networkIdentity, transformSpace, receivedPos, receivedRot, receivedScale);
                    ServerBehaviour.Instance.sendMessage(transformMsg, clientID); //Forward to all clients but the sender
                }
            }
        }

        private static void processRigidbodyMessage(byte[] msg, bool clientMessage)
        {
            short netID = ObjectSerializer.deserializeShort(ref msg);

            if (NetworkEntityManager.Instance.findComponent(netID, out NetworkTransform networkTransform)) //NetworkTransform found
            {
                bool receivedVelocity = ObjectSerializer.deserializeByte(ref msg) == 1;
                if (receivedVelocity) //Velocity data was sent
                {
                    float velX = ObjectSerializer.deserializeFloat(ref msg);
                    float velY = ObjectSerializer.deserializeFloat(ref msg);
                    float velZ = ObjectSerializer.deserializeFloat(ref msg);
                    networkTransform.Velocity = new Vector3(velX, velY, velZ);
                }

                bool receivedAngularVelocity = ObjectSerializer.deserializeByte(ref msg) == 1;
                if (receivedAngularVelocity) //Angular velocity data was sent
                {
                    float aVelX = ObjectSerializer.deserializeFloat(ref msg);
                    float aVelY = ObjectSerializer.deserializeFloat(ref msg);
                    float aVelZ = ObjectSerializer.deserializeFloat(ref msg);
                    networkTransform.AngularVelocity = new Vector3(aVelX, aVelY, aVelZ);
                }

                //Message was sent from a client, now being processed on the server
                if (clientMessage && ServerBehaviour.Instance)
                {
                    short clientID = ObjectSerializer.deserializeShort(ref msg); //Client who sent the message
                    byte[] rigidbodyMsg = MessageFactory.createRigidbodyMessage(networkTransform.networkIdentity, networkTransform.rb, receivedVelocity, receivedAngularVelocity);
                    ServerBehaviour.Instance.sendMessage(rigidbodyMsg, clientID); //Forward to all clients but the sender
                }
            }
        }

        private static void processSetActiveMessage(byte[] msg, bool clientMessage)
        {
            short netID = ObjectSerializer.deserializeShort(ref msg);

            if (NetworkEntityManager.Instance.findComponent(netID, out NetworkIdentity netIdentity))
            {
                netIdentity.gameObject.SetActive(ObjectSerializer.deserializeByte(ref msg) == 1);

                //Message was sent from a client, now being processed on the server
                if (clientMessage && ServerBehaviour.Instance)
                {
                    short clientID = ObjectSerializer.deserializeShort(ref msg); //Client who sent the message
                    byte[] setActiveMsg = MessageFactory.createSetActiveMessage(netIdentity);
                    ServerBehaviour.Instance.sendMessage(setActiveMsg, clientID, true); //Forward to all clients but the sender
                }
            }
        }

        private static void processNetworkAuthorityMessage(byte[] msg, bool clientMessage)
        {
            short netID = ObjectSerializer.deserializeShort(ref msg);

            if (NetworkEntityManager.Instance.findComponent(netID, out NetworkIdentity netIdentity))
            {
                NetworkAuthority networkAuthority = ObjectSerializer.deserializeByte(ref msg) == 1 ? NetworkAuthority.CLIENT : NetworkAuthority.SERVER;
                netIdentity.networkAuthority = networkAuthority;

                //Message was sent from a client, now being processed on the server
                if (clientMessage && ServerBehaviour.Instance)
                {
                    short clientID = ObjectSerializer.deserializeShort(ref msg); //Client who sent the message
                    byte[] networkAuthorityMsg = MessageFactory.createNetworkAuthorityMessage(netIdentity, networkAuthority);
                    ServerBehaviour.Instance.sendMessage(networkAuthorityMsg, clientID, true); //Forward to all clients but the sender
                }
            }
        }

        #endregion Misc

        #region Joints

        private static void processAddJointMessage(byte[] msg, bool clientMessage)
        {
            short jointParentID = ObjectSerializer.deserializeShort(ref msg);

            if (NetworkEntityManager.Instance.findComponent(jointParentID, out NetworkIdentity jointParent))
            {
                short connectedBodyID = ObjectSerializer.deserializeShort(ref msg);

                if (connectedBodyID == -2) //No connected body
                {
                    byte[] addJointMsg = null; //Message to forward to other clients if this is a client message
                    bool forwardMessage = clientMessage && ServerBehaviour.Instance; //Should the message be forwarded?

                    byte jointType = ObjectSerializer.deserializeByte(ref msg);
                    switch (jointType)
                    {
                        case 0:
                            jointParent.gameObject.AddComponent<FixedJoint>();

                            if (forwardMessage)
                                addJointMsg = MessageFactory.createAddJointMessage<FixedJoint>(jointParent, null);
                            break;
                        case 1:
                            jointParent.gameObject.AddComponent<HingeJoint>();

                            if (forwardMessage)
                                addJointMsg = MessageFactory.createAddJointMessage<HingeJoint>(jointParent, null);
                            break;
                        case 2:
                            jointParent.gameObject.AddComponent<SpringJoint>();

                            if (forwardMessage)
                                addJointMsg = MessageFactory.createAddJointMessage<SpringJoint>(jointParent, null);
                            break;
                        case 3:
                            jointParent.gameObject.AddComponent<ConfigurableJoint>();

                            if (forwardMessage)
                                addJointMsg = MessageFactory.createAddJointMessage<ConfigurableJoint>(jointParent, null);
                            break;
                        case 4:
                            jointParent.gameObject.AddComponent<CharacterJoint>();

                            if (forwardMessage)
                                addJointMsg = MessageFactory.createAddJointMessage<CharacterJoint>(jointParent, null);
                            break;
                    }

                    //Message was sent from a client, now being processed on the server
                    if (forwardMessage)
                    {
                        short clientID = ObjectSerializer.deserializeShort(ref msg); //Client who sent the message
                        ServerBehaviour.Instance.sendMessage(addJointMsg, clientID, true); //Forward to all clients but the sender
                    }
                }
                else //Connected body
                {
                    if (NetworkEntityManager.Instance.findComponent(connectedBodyID, out NetworkIdentity connectedBody))
                    {
                        if (NetworkEntityManager.Instance.findComponent(connectedBodyID, out Rigidbody connectedBodyRb))
                        {
                            byte[] addJointMsg = null; //Message to forward to other clients if this is a client message
                            bool forwardMessage = clientMessage && ServerBehaviour.Instance; //Should the message be forwarded?

                            byte jointType = ObjectSerializer.deserializeByte(ref msg);
                            switch (jointType)
                            {
                                case 0:
                                    jointParent.gameObject.AddComponent<FixedJoint>().connectedBody = connectedBodyRb;

                                    if (forwardMessage)
                                        addJointMsg = MessageFactory.createAddJointMessage<FixedJoint>(jointParent, connectedBody);
                                    break;
                                case 1:
                                    jointParent.gameObject.AddComponent<HingeJoint>().connectedBody = connectedBodyRb;

                                    if (forwardMessage)
                                        addJointMsg = MessageFactory.createAddJointMessage<HingeJoint>(jointParent, connectedBody);
                                    break;
                                case 2:
                                    jointParent.gameObject.AddComponent<SpringJoint>().connectedBody = connectedBodyRb;

                                    if (forwardMessage)
                                        addJointMsg = MessageFactory.createAddJointMessage<SpringJoint>(jointParent, connectedBody);
                                    break;
                                case 3:
                                    jointParent.gameObject.AddComponent<ConfigurableJoint>().connectedBody = connectedBodyRb;

                                    if (forwardMessage)
                                        addJointMsg = MessageFactory.createAddJointMessage<ConfigurableJoint>(jointParent, connectedBody);
                                    break;
                                case 4:
                                    jointParent.gameObject.AddComponent<CharacterJoint>().connectedBody = connectedBodyRb;

                                    if (forwardMessage)
                                        addJointMsg = MessageFactory.createAddJointMessage<CharacterJoint>(jointParent, connectedBody);
                                    break;
                            }

                            //Message was sent from a client, now being processed on the server
                            if (forwardMessage)
                            {
                                short clientID = ObjectSerializer.deserializeShort(ref msg); //Client who sent the message
                                ServerBehaviour.Instance.sendMessage(addJointMsg, clientID, true); //Forward to all clients but the sender
                            }
                        }
                    }
                }
            }
        }

        private static void processRemoveJointMessage(byte[] msg, bool clientMessage)
        {
            short jointParentID = ObjectSerializer.deserializeShort(ref msg);

            if (NetworkEntityManager.Instance.findComponent(jointParentID, out NetworkIdentity jointParent))
            {
                byte[] removeJointMsg = null;
                bool forwardMessage = clientMessage && ServerBehaviour.Instance; //Should the message be forwarded?

                byte jointType = ObjectSerializer.deserializeByte(ref msg);
                switch (jointType)
                {
                    case 0:
                        if (NetworkEntityManager.Instance.findComponent(jointParentID, out FixedJoint fixedJoint, true))
                            Object.Destroy(fixedJoint); //Destroy the joint

                        if (forwardMessage)
                            removeJointMsg = MessageFactory.createRemoveJointMessage<FixedJoint>(jointParent);
                        break;
                    case 1:
                        if (NetworkEntityManager.Instance.findComponent(jointParentID, out HingeJoint hingeJoint, true))
                            Object.Destroy(hingeJoint); //Destroy the joint

                        if (forwardMessage)
                            removeJointMsg = MessageFactory.createRemoveJointMessage<HingeJoint>(jointParent);
                        break;
                    case 2:
                        if (NetworkEntityManager.Instance.findComponent(jointParentID, out SpringJoint springJoint, true))
                            Object.Destroy(springJoint); //Destroy the joint

                        if (forwardMessage)
                            removeJointMsg = MessageFactory.createRemoveJointMessage<SpringJoint>(jointParent);
                        break;
                    case 3:
                        if (NetworkEntityManager.Instance.findComponent(jointParentID, out ConfigurableJoint configurableJoint, true))
                            Object.Destroy(configurableJoint); //Destroy the joint

                        if (forwardMessage)
                            removeJointMsg = MessageFactory.createRemoveJointMessage<ConfigurableJoint>(jointParent);
                        break;
                    case 4:
                        if (NetworkEntityManager.Instance.findComponent(jointParentID, out CharacterJoint characterJoint, true))
                            Object.Destroy(characterJoint); //Destroy the joint

                        if (forwardMessage)
                            removeJointMsg = MessageFactory.createRemoveJointMessage<CharacterJoint>(jointParent);
                        break;
                }

                //Message was sent from a client, now being processed on the server
                if (forwardMessage)
                {
                    short clientID = ObjectSerializer.deserializeShort(ref msg); //Client who sent the message
                    ServerBehaviour.Instance.sendMessage(removeJointMsg, clientID, true); //Forward to all clients but the sender
                }
            }
        }

        #endregion Joints

        #region Connection

        private static void processClientConnectionEventMessage(byte[] msg)
        {
            if (ClientManager.Instance)
            {
                short clientID = ObjectSerializer.deserializeShort(ref msg);
                ClientManager.Instance.handleClientConnected(clientID);
            }
        }

        private static void processClientDisconnectionEventMessage(byte[] msg)
        {
            if (ClientManager.Instance)
            {
                short clientID = ObjectSerializer.deserializeShort(ref msg);
                ClientManager.Instance.handleClientDisconnected(clientID);
            }
        }

        private static void processClientReconnectionEventMessage(byte[] msg)
        {
            if (ClientManager.Instance)
            {
                short clientID = ObjectSerializer.deserializeShort(ref msg);
                ClientManager.Instance.handleClientReconnected(clientID);
            }
        }

        private static void processClientIDMessage(byte[] msg)
        {
            ClientBehaviour client = ClientBehaviour.Instance;
            if (client)
            {
                short clientID = ObjectSerializer.deserializeShort(ref msg);

                //Client ID has not already been set
                if (client.ClientID == ClientBehaviour.ID_NOT_SET)
                {
                    client.ClientID = clientID;
                }
                else //Client ID has already been set
                {
                    byte[] clientReidentifiedMessage = MessageFactory.createClientReidentifiedMessage(client.ClientID, clientID);
                    client.sendMessage(clientReidentifiedMessage, true);
                }
            }
        }

        private static void processClientReidentifiedMessage(byte[] msg)
        {
            if (ServerBehaviour.Instance)
            {
                short clientsideID = ObjectSerializer.deserializeShort(ref msg);
                short serversideID = ObjectSerializer.deserializeShort(ref msg);
                ServerBehaviour.Instance.handleClientReconnected(clientsideID, serversideID);
            }
        }

        #endregion Connection

        #region SyncVar

        private static void processBaseSyncVarMessage(byte[] msgBytes, bool clientMessage)
        {
            short networkID = ObjectSerializer.deserializeShort(ref msgBytes);

            //Decompress the bytes which were compressed
            byte[] decompressedBytes = ObjectSerializer.deserializeBytes(ref msgBytes).decompress();

            System.Type objType = ObjectSerializer.deserializeObject<System.Type>(ref decompressedBytes);
            System.Type fieldType = ObjectSerializer.deserializeObject<System.Type>(ref decompressedBytes);
            string fieldName = ObjectSerializer.deserializeString(ref decompressedBytes);

            if (NetworkEntityManager.Instance.findComponent(networkID, out NetworkIdentity netIdentity))
            {
                if (netIdentity.getNetworkBehaviour(objType, out NetworkBehaviour netBehaviour))
                {
                    object value = ObjectSerializer.deserializeObject(ref decompressedBytes);

                    netBehaviour.setSyncVar(objType, fieldName, value);

                    //Message was sent from a client, now being processed on the server
                    if (clientMessage && ServerBehaviour.Instance)
                    {
                        short clientID = ObjectSerializer.deserializeShort(ref msgBytes); //Client who sent the message
                        byte[] baseSyncVarMsg = MessageFactory.createBaseSyncVarMessage(netBehaviour, objType, fieldType, fieldName, value);
                        ServerBehaviour.Instance.sendMessage(baseSyncVarMsg, clientID); //Forward to all clients but the sender
                    }
                }
            }
        }

        private static void processColorSyncVarMessage(byte[] msgBytes, bool clientMessage)
        {
            short networkID = ObjectSerializer.deserializeShort(ref msgBytes);

            //Decompress the bytes which were compressed
            byte[] decompressedBytes = ObjectSerializer.deserializeBytes(ref msgBytes).decompress();

            System.Type objType = ObjectSerializer.deserializeObject<System.Type>(ref decompressedBytes);
            string fieldName = ObjectSerializer.deserializeString(ref decompressedBytes);

            if (NetworkEntityManager.Instance.findComponent(networkID, out NetworkIdentity netIdentity))
            {
                if (netIdentity.getNetworkBehaviour(objType, out NetworkBehaviour netBehaviour))
                {
                    Color value = new Color();
                    value.r = ObjectSerializer.deserializeFloat(ref msgBytes);
                    value.g = ObjectSerializer.deserializeFloat(ref msgBytes);
                    value.b = ObjectSerializer.deserializeFloat(ref msgBytes);
                    value.a = ObjectSerializer.deserializeFloat(ref msgBytes);

                    netBehaviour.setSyncVar(objType, fieldName, value);

                    //Message was sent from a client, now being processed on the server
                    if (clientMessage && ServerBehaviour.Instance)
                    {
                        short clientID = ObjectSerializer.deserializeShort(ref msgBytes); //Client who sent the message
                        byte[] colorSyncVarMsg = MessageFactory.createColorSyncVarMessage(netBehaviour, objType, fieldName, value);
                        ServerBehaviour.Instance.sendMessage(colorSyncVarMsg, clientID); //Forward to all clients but the sender
                    }
                }
            }
        }

        private static void processGameObjectSyncVarMessage(byte[] msgBytes, bool clientMessage)
        {
            if (msgBytes != null && msgBytes.Length > 0)
            {
                short networkID = ObjectSerializer.deserializeShort(ref msgBytes);

                //Decompress the bytes which were compressed
                byte[] decompressedBytes = ObjectSerializer.deserializeBytes(ref msgBytes).decompress();

                System.Type objType = ObjectSerializer.deserializeObject<System.Type>(ref decompressedBytes);
                string fieldName = ObjectSerializer.deserializeString(ref decompressedBytes);

                if (NetworkEntityManager.Instance.findComponent(networkID, out NetworkIdentity netIdentity))
                {
                    if (netIdentity.getNetworkBehaviour(objType, out NetworkBehaviour netBehaviour))
                    {
                        if (NetworkEntityManager.Instance.search(ObjectSerializer.deserializeShort(ref msgBytes), out GameObject value))
                        {
                            netBehaviour.setSyncVar(objType, fieldName, value);

                            //Message was sent from a client, now being processed on the server
                            if (clientMessage && ServerBehaviour.Instance)
                            {
                                short clientID = ObjectSerializer.deserializeShort(ref msgBytes); //Client who sent the message
                                byte[] gameObjectSyncVarMsg = MessageFactory.createGameObjectSyncVarMessage(netBehaviour, objType, fieldName, value);
                                ServerBehaviour.Instance.sendMessage(gameObjectSyncVarMsg, clientID); //Forward to all clients but the sender
                            }
                        }
                    }
                }
            }
        }

        private static void processQuaternionSyncVarMessage(byte[] msgBytes, bool clientMessage)
        {
            short networkID = ObjectSerializer.deserializeShort(ref msgBytes);

            //Decompress the bytes which were compressed
            byte[] decompressedBytes = ObjectSerializer.deserializeBytes(ref msgBytes).decompress();

            System.Type objType = ObjectSerializer.deserializeObject<System.Type>(ref decompressedBytes);
            string fieldName = ObjectSerializer.deserializeString(ref decompressedBytes);

            if (NetworkEntityManager.Instance.findComponent(networkID, out NetworkIdentity netIdentity))
            {
                if (netIdentity.getNetworkBehaviour(objType, out NetworkBehaviour netBehaviour))
                {
                    Quaternion value = new Quaternion();
                    value.x = ObjectSerializer.deserializeFloat(ref msgBytes);
                    value.y = ObjectSerializer.deserializeFloat(ref msgBytes);
                    value.z = ObjectSerializer.deserializeFloat(ref msgBytes);
                    value.w = ObjectSerializer.deserializeFloat(ref msgBytes);

                    netBehaviour.setSyncVar(objType, fieldName, value);

                    //Message was sent from a client, now being processed on the server
                    if (clientMessage && ServerBehaviour.Instance)
                    {
                        short clientID = ObjectSerializer.deserializeShort(ref msgBytes); //Client who sent the message
                        byte[] quatSyncVarMsg = MessageFactory.createQuaternionSyncVarMessage(netBehaviour, objType, fieldName, value);
                        ServerBehaviour.Instance.sendMessage(quatSyncVarMsg, clientID); //Forward to all clients but the sender
                    }
                }
            }
        }

        private static void processVector2SyncVarMessage(byte[] msgBytes, bool clientMessage)
        {
            short networkID = ObjectSerializer.deserializeShort(ref msgBytes);

            //Decompress the bytes which were compressed
            byte[] decompressedBytes = ObjectSerializer.deserializeBytes(ref msgBytes).decompress();

            System.Type objType = ObjectSerializer.deserializeObject<System.Type>(ref decompressedBytes);
            string fieldName = ObjectSerializer.deserializeString(ref decompressedBytes);

            if (NetworkEntityManager.Instance.findComponent(networkID, out NetworkIdentity netIdentity))
            {
                if (netIdentity.getNetworkBehaviour(objType, out NetworkBehaviour netBehaviour))
                {
                    Vector2 value = new Vector2();
                    value.x = ObjectSerializer.deserializeFloat(ref msgBytes);
                    value.y = ObjectSerializer.deserializeFloat(ref msgBytes);

                    netBehaviour.setSyncVar(objType, fieldName, value);

                    //Message was sent from a client, now being processed on the server
                    if (clientMessage && ServerBehaviour.Instance)
                    {
                        short clientID = ObjectSerializer.deserializeShort(ref msgBytes); //Client who sent the message
                        byte[] v2SyncVarMsg = MessageFactory.createVector2SyncVarMessage(netBehaviour, objType, fieldName, value);
                        ServerBehaviour.Instance.sendMessage(v2SyncVarMsg, clientID); //Forward to all clients but the sender
                    }
                }
            }
        }

        private static void processVector3SyncVarMessage(byte[] msgBytes, bool clientMessage)
        {
            short networkID = ObjectSerializer.deserializeShort(ref msgBytes);

            //Decompress the bytes which were compressed
            byte[] decompressedBytes = ObjectSerializer.deserializeBytes(ref msgBytes).decompress();

            System.Type objType = ObjectSerializer.deserializeObject<System.Type>(ref decompressedBytes);
            string fieldName = ObjectSerializer.deserializeString(ref decompressedBytes);

            if (NetworkEntityManager.Instance.findComponent(networkID, out NetworkIdentity netIdentity))
            {
                if (netIdentity.getNetworkBehaviour(objType, out NetworkBehaviour netBehaviour))
                {
                    Vector3 value = new Vector3();
                    value.x = ObjectSerializer.deserializeFloat(ref msgBytes);
                    value.y = ObjectSerializer.deserializeFloat(ref msgBytes);
                    value.z = ObjectSerializer.deserializeFloat(ref msgBytes);

                    netBehaviour.setSyncVar(objType, fieldName, value);

                    //Message was sent from a client, now being processed on the server
                    if (clientMessage && ServerBehaviour.Instance)
                    {
                        short clientID = ObjectSerializer.deserializeShort(ref msgBytes); //Client who sent the message
                        byte[] v3SyncVarMsg = MessageFactory.createVector3SyncVarMessage(netBehaviour, objType, fieldName, value);
                        ServerBehaviour.Instance.sendMessage(v3SyncVarMsg, clientID); //Forward to all clients but the sender
                    }
                }
            }
        }

        #endregion SyncVar

        #region NetworkIdentity

        private static void processNetworkIDRequestMessage(byte[] msg)
        {
            string transformPath = ObjectSerializer.deserializeString(ref msg);

            if (ServerBehaviour.Instance) //Message is being processed on the server
            {
                short networkID = NetworkIdentityRegistry.Instance.getNetworkID(transformPath); //Get network ID for the transform path
                short clientID = ObjectSerializer.deserializeShort(ref msg); //Client who sent the message
                byte[] networkIDMsg = MessageFactory.createNetworkIDMessage(transformPath, networkID);
                ServerBehaviour.Instance.sendMessageToClient(networkIDMsg, clientID, true);
            }
        }

        private static void processNetworkIDMessage(byte[] msg)
        {
            string transformPath = ObjectSerializer.deserializeString(ref msg);
            short networkID = ObjectSerializer.deserializeShort(ref msg);

            NetworkEntityManager.Instance.setNetworkID(transformPath, networkID);
        }

        #endregion NetworkIdentity

        private static void processAvatarRequestMessage(byte[] msg, bool clientMessage)
        {
            short clientID = ObjectSerializer.deserializeShort(ref msg);

            AvatarManager.Instance?.handleAvatarRequest(clientID);

            if (ServerBehaviour.Instance) //Message is being processed on the server
            {
                byte[] clientIDMsg = MessageFactory.createAvatarRequestMessage(false, clientID);
                ServerBehaviour.Instance.sendMessage(clientIDMsg, clientID, true); //Forward to all clients but the sender
            }
        }

        private static void processNetworkEventMessage(byte[] msg, bool clientMessage)
        {
            short netID = ObjectSerializer.deserializeShort(ref msg);

            if (NetworkEntityManager.Instance.findComponent(netID, out NetworkedEvent networkedEvent))
            {
                networkedEvent.OnEventInvoked.Invoke();

                //Message was sent from a client, now being processed on the server
                if (clientMessage && ServerBehaviour.Instance)
                {
                    short clientID = ObjectSerializer.deserializeShort(ref msg); //Client who sent the message
                    byte[] networkEventMsg = MessageFactory.createNetworkEventMessage(networkedEvent.networkIdentity);
                    ServerBehaviour.Instance.sendMessage(networkEventMsg, clientID, true); //Forward to all clients but the sender
                }
            }
        }
    }
}
