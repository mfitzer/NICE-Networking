using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NICE_Networking
{
    public static partial class MessageFactory
    {
        #region Misc

        /// <summary>
        /// Creates a transform message which includes the specified data.
        /// </summary>
        /// <param name="clientMessage">Indicates if this is a client message.</param>
        public static byte[] createTransformMessage(NetworkIdentity netIdentity, NetworkTransform.TransformSpace transformSpace,
            bool sendPosition = true, bool sendRotation = true, bool sendScale = true, bool clientMessage = false)
        {
            List<byte> msgBytes = new List<byte>();

            byte msgHeader = (byte)(clientMessage ? 1 : 0); //Change message header to indicate if it's a client message or not
            msgBytes.Add(msgHeader); //Message Header

            msgBytes.AddRange(netIdentity.networkID.serialize()); //Identity of the transform on the network

            bool worldSpace = transformSpace == NetworkTransform.TransformSpace.World;

            //Position
            if (sendPosition)
            {
                msgBytes.AddRange(((byte)1).serialize()); //Signal position is being sent

                Vector3 pos = worldSpace ? netIdentity.transform.position : netIdentity.transform.localPosition;
                msgBytes.AddRange(pos.x.serialize());
                msgBytes.AddRange(pos.y.serialize());
                msgBytes.AddRange(pos.z.serialize());
            }
            else
                msgBytes.AddRange(((byte)0).serialize()); //Signal no position is being sent

            //Rotation
            if (sendRotation)
            {
                msgBytes.AddRange(((byte)1).serialize()); //Signal rotation is being sent

                Vector3 rot = worldSpace ? netIdentity.transform.eulerAngles : netIdentity.transform.localEulerAngles;
                msgBytes.AddRange(rot.x.serialize());
                msgBytes.AddRange(rot.y.serialize());
                msgBytes.AddRange(rot.z.serialize());
            }
            else
                msgBytes.AddRange(((byte)0).serialize()); //Signal no rotation is being sent

            //Scale
            if (sendScale)
            {
                msgBytes.AddRange(((byte)1).serialize()); //Signal scale is being sent
                msgBytes.AddRange(netIdentity.transform.localScale.x.serialize());
                msgBytes.AddRange(netIdentity.transform.localScale.y.serialize());
                msgBytes.AddRange(netIdentity.transform.localScale.z.serialize());
            }
            else
                msgBytes.AddRange(((byte)0).serialize()); //Signal no scale is being sent

            if (clientMessage)
                msgBytes.Add((byte)(worldSpace ? 1 : 0)); //Send transform space flag

            return msgBytes.ToArray();
        }

        /// <summary>
        /// Creates a rigidbody message which includes the specified data.
        /// </summary>
        /// <param name="clientMessage">Indicates if this is a client message.</param>
        public static byte[] createRigidbodyMessage(NetworkIdentity netIdentity, Rigidbody rigidbody, bool sendVelocity = true,
            bool sendAngularVelocity = true, bool clientMessage = false)
        {
            if (rigidbody)
            {
                List<byte> msgBytes = new List<byte>();

                byte msgHeader = (byte)(clientMessage ? 3 : 2); //Change message header to indicate if it's a client message or not
                msgBytes.Add(msgHeader); //Message Header

                msgBytes.AddRange(netIdentity.networkID.serialize()); //Identity of the rigidbody on the network

                //Velocity
                if (sendVelocity)
                {
                    msgBytes.AddRange(((byte)1).serialize()); //Signal velocity is being sent

                    msgBytes.AddRange(rigidbody.velocity.x.serialize());
                    msgBytes.AddRange(rigidbody.velocity.y.serialize());
                    msgBytes.AddRange(rigidbody.velocity.z.serialize());
                }
                else
                    msgBytes.AddRange(((byte)0).serialize()); //Signal no velocity is being sent

                //Angular Velocity
                if (sendAngularVelocity)
                {
                    msgBytes.AddRange(((byte)1).serialize()); //Signal angular velocity is being sent

                    msgBytes.AddRange(rigidbody.angularVelocity.x.serialize());
                    msgBytes.AddRange(rigidbody.angularVelocity.y.serialize());
                    msgBytes.AddRange(rigidbody.angularVelocity.z.serialize());
                }
                else
                    msgBytes.AddRange(((byte)0).serialize()); //Signal no angular velocity is being sent

                return msgBytes.ToArray();
            }

            return null;
        }

        /// <summary>
        /// Creates a set active message to share the GameObject's active state.
        /// </summary>
        /// <param name="clientMessage">Indicates if this is a client message.</param>
        public static byte[] createSetActiveMessage(NetworkIdentity netIdentity, bool clientMessage = false)
        {
            List<byte> msgBytes = new List<byte>();

            byte msgHeader = (byte)(clientMessage ? 5 : 4); //Change message header to indicate if it's a client message or not
            msgBytes.Add(msgHeader); //Message Header

            msgBytes.AddRange(netIdentity.networkID.serialize()); //Identity of the GameObject on the network

            msgBytes.Add((byte)(netIdentity.gameObject.activeSelf ? 1 : 0)); //Active state of the GameObject

            return msgBytes.ToArray();
        }

        /// <summary>
        /// Creates a network authority message to set the network authority on a network identity.
        /// </summary>
        /// <param name="clientMessage">Indicates if this is a client message.</param>
        public static byte[] createNetworkAuthorityMessage(NetworkIdentity netIdentity, NetworkAuthority netAuthority, bool clientMessage = false)
        {
            List<byte> msgBytes = new List<byte>();

            byte msgHeader = (byte)(clientMessage ? 7 : 6); //Change message header to indicate if it's a client message or not
            msgBytes.Add(msgHeader); //Message Header

            msgBytes.AddRange(netIdentity.networkID.serialize()); //Identity of the GameObject on the network

            msgBytes.Add((byte)(netAuthority == NetworkAuthority.CLIENT ? 1 : 0)); //Network authority of object

            return msgBytes.ToArray();
        }

        #endregion Misc

        #region Joints

        /// <summary>
        /// Creates an add joint message to add a joint to the joint parent with the specified connected body.
        /// </summary>
        /// <param name="clientMessage">Indicates if this is a client message.</param>
        public static byte[] createAddJointMessage<T>(NetworkIdentity jointParent, NetworkIdentity connectedBody, bool clientMessage = false)
        {
            List<byte> msgBytes = new List<byte>();

            byte msgHeader = (byte)(clientMessage ? 9 : 8); //Change message header to indicate if it's a client message or not
            msgBytes.Add(msgHeader); //Message Header

            msgBytes.AddRange(jointParent.networkID.serialize()); //Identity of the joint parent on the network

            if (connectedBody)
                msgBytes.AddRange(connectedBody.networkID.serialize()); //Identity of the connected body on the network
            else
                msgBytes.AddRange(((short)-2).serialize()); //No connected body

            byte jointType = 0;
            if (typeof(T) == typeof(FixedJoint))
                jointType = 0;
            else if (typeof(T) == typeof(HingeJoint))
                jointType = 1;
            else if (typeof(T) == typeof(SpringJoint))
                jointType = 2;
            else if (typeof(T) == typeof(ConfigurableJoint))
                jointType = 3;

            msgBytes.Add(jointType); //Specify what type of joint to add

            return msgBytes.ToArray();
        }

        /// <summary>
        /// Creates a remove joint message to remove a joint from the joint parent.
        /// </summary>
        /// <param name="clientMessage">Indicates if this is a client message.</param>
        public static byte[] createRemoveJointMessage<T>(NetworkIdentity jointParent, bool clientMessage = false)
        {
            List<byte> msgBytes = new List<byte>();

            byte msgHeader = (byte)(clientMessage ? 11 : 10); //Change message header to indicate if it's a client message or not
            msgBytes.Add(msgHeader); //Message Header

            msgBytes.AddRange(jointParent.networkID.serialize()); //Identity of the joint parent on the network

            byte jointType = 0;
            if (typeof(T) == typeof(FixedJoint))
                jointType = 0;
            else if (typeof(T) == typeof(HingeJoint))
                jointType = 1;
            else if (typeof(T) == typeof(SpringJoint))
                jointType = 2;
            else if (typeof(T) == typeof(ConfigurableJoint))
                jointType = 3;
            else if (typeof(T) == typeof(CharacterJoint))
                jointType = 4;

            msgBytes.Add(jointType); //Specify what type of joint to remove

            return msgBytes.ToArray();
        }

        #endregion Joints

        #region Connection

        /// <summary>
        /// Creates a client connection event message for notifying existing clients of a new client.
        /// </summary>
        /// <param name="clientID">ID of the client.</param>
        public static byte[] createClientConnectionEventMessage(short clientID)
        {
            List<byte> msgBytes = new List<byte>();

            msgBytes.Add(12); //Message Header

            msgBytes.AddRange(clientID.serialize()); //ID of connected client

            return msgBytes.ToArray();
        }

        /// <summary>
        /// Creates a client disconnection event message for notifying existing clients that a client disconnected.
        /// </summary>
        /// <param name="clientID">ID of the client.</param>
        public static byte[] createClientDisconnectionEventMessage(short clientID)
        {
            List<byte> msgBytes = new List<byte>();

            msgBytes.Add(13); //Message Header

            msgBytes.AddRange(clientID.serialize()); //ID of connected client

            return msgBytes.ToArray();
        }

        /// <summary>
        /// Creates a client reconnection event message for notifying existing clients that an old client reconnected.
        /// </summary>
        /// <param name="clientID">ID of the client.</param>
        public static byte[] createClientReconnectionEventMessage(short clientID)
        {
            List<byte> msgBytes = new List<byte>();

            msgBytes.Add(14); //Message Header

            msgBytes.AddRange(clientID.serialize()); //ID of connected client

            return msgBytes.ToArray();
        }

        /// <summary>
        /// Creates a client ID message for notifying a client of its ID.
        /// </summary>
        /// <param name="clientID">ID of the client.</param>
        public static byte[] createClientIDMessage(short clientID)
        {
            List<byte> msgBytes = new List<byte>();

            msgBytes.Add(15); //Message Header

            msgBytes.AddRange(clientID.serialize()); //Client ID

            return msgBytes.ToArray();
        }

        /// <summary>
        /// Creates a client reidentified message for notifying the server that of a client's previously assigned ID.
        /// </summary>
        /// <param name="clientsideID">Previously assigned client ID.</param>
        /// /// <param name="serversideID">Newly assigned client ID.</param>
        public static byte[] createClientReidentifiedMessage(short clientsideID, short serversideID)
        {
            List<byte> msgBytes = new List<byte>();

            msgBytes.Add(16); //Message Header

            msgBytes.AddRange(clientsideID.serialize()); //Clientside client ID
            msgBytes.AddRange(serversideID.serialize()); //Serverside client ID

            return msgBytes.ToArray();
        }

        #endregion Connection

        #region SyncVar

        /// <summary>
        /// Creates a SyncVar message for a serializable object.
        /// </summary>
        /// <param name="netBehaviour">NetworkBehaviour the SyncVar is on.</param>
        /// <param name="objType">Type of the object containing the SyncVar.</param>
        /// <param name="fieldType">Type of the SyncVar.</param>
        /// <param name="fieldName">Name of the SyncVar.</param>
        /// <param name="value">Value of the SyncVar.</param>
        /// <param name="clientMessage">Indicates if this is a client message.</param>
        public static byte[] createBaseSyncVarMessage(NetworkBehaviour netBehaviour, Type objType, Type fieldType, string fieldName, object value, bool clientMessage = false)
        {
            List<byte> msgBytes = new List<byte>();

            byte msgHeader = (byte)(clientMessage ? 18 : 17); //Change message header to indicate if it's a client message or not
            msgBytes.Add(msgHeader); //Message Header
            
            msgBytes.AddRange(netBehaviour.networkIdentity.networkID.serialize()); //Identity of the network behaviour object on the network

            List<byte> bytesToCompress = new List<byte>();

            //SyncVar data
            bytesToCompress.AddRange(objType.serialize());
            bytesToCompress.AddRange(fieldType.serialize());
            bytesToCompress.AddRange(fieldName.serialize());

            //SyncVar value
            bytesToCompress.AddRange(value.serialize());

            //Serializes compressed bytes as array for deserialization
            msgBytes.AddRange(bytesToCompress.ToArray().compress().serialize());

            return msgBytes.ToArray();
        }

        /// <summary>
        /// Creates a Color SyncVar message.
        /// </summary>
        /// <param name="netBehaviour">NetworkBehaviour the SyncVar is on.</param>
        /// <param name="objType">Type of the object containing the SyncVar.</param>
        /// <param name="fieldName">Name of the SyncVar.</param>
        /// <param name="value">Value of the SyncVar.</param>
        /// <param name="clientMessage">Indicates if this is a client message.</param>
        public static byte[] createColorSyncVarMessage(NetworkBehaviour netBehaviour, Type objType, string fieldName, Color value, bool clientMessage = false)
        {
            List<byte> msgBytes = new List<byte>();

            byte msgHeader = (byte)(clientMessage ? 20 : 19); //Change message header to indicate if it's a client message or not
            msgBytes.Add(msgHeader); //Message Header

            //Identity of the network behaviour object on the network
            msgBytes.AddRange(netBehaviour.networkIdentity.networkID.serialize());

            List<byte> bytesToCompress = new List<byte>();

            //SyncVar data
            bytesToCompress.AddRange(objType.serialize());
            bytesToCompress.AddRange(fieldName.serialize());

            //Serializes compressed bytes as array for deserialization
            msgBytes.AddRange(bytesToCompress.ToArray().compress().serialize());

            //SyncVar Color value
            msgBytes.AddRange(value.r.serialize());
            msgBytes.AddRange(value.g.serialize());
            msgBytes.AddRange(value.b.serialize());
            msgBytes.AddRange(value.a.serialize());

            return msgBytes.ToArray();
        }

        /// <summary>
        /// Creates a GameObject SyncVar message.
        /// </summary>
        /// <param name="netBehaviour">NetworkBehaviour the SyncVar is on.</param>
        /// <param name="objType">Type of the object containing the SyncVar.</param>
        /// <param name="fieldName">Name of the SyncVar.</param>
        /// <param name="value">Value of the SyncVar.</param>
        /// <param name="clientMessage">Indicates if this is a client message.</param>
        public static byte[] createGameObjectSyncVarMessage(NetworkBehaviour netBehaviour, Type objType, string fieldName, GameObject value, bool clientMessage = false)
        {
            List<byte> msgBytes = new List<byte>();

            NetworkIdentity valueNetIdentity = value?.GetComponent<NetworkIdentity>();

            if (valueNetIdentity)
            {
                byte msgHeader = (byte)(clientMessage ? 22 : 21); //Change message header to indicate if it's a client message or not
                msgBytes.Add(msgHeader); //Message Header

                //Identity of the network behaviour object on the network
                msgBytes.AddRange(netBehaviour.networkIdentity.networkID.serialize());

                List<byte> bytesToCompress = new List<byte>();

                //SyncVar data
                bytesToCompress.AddRange(objType.serialize());
                bytesToCompress.AddRange(fieldName.serialize());

                //Serializes compressed bytes as array for deserialization
                msgBytes.AddRange(bytesToCompress.ToArray().compress().serialize());

                //SyncVar GameObject network ID
                msgBytes.AddRange(valueNetIdentity.networkID.serialize());

                return msgBytes.ToArray();
            }
            else
                Debug.LogError(value.name + " does not have a NetworkIdentity on it, cannot send the GameObject over the network.");

            return null;
        }

        /// <summary>
        /// Creates a Quaternion SyncVar message.
        /// </summary>
        /// <param name="netBehaviour">NetworkBehaviour the SyncVar is on.</param>
        /// <param name="objType">Type of the object containing the SyncVar.</param>
        /// <param name="fieldName">Name of the SyncVar.</param>
        /// <param name="value">Value of the SyncVar.</param>
        /// <param name="clientMessage">Indicates if this is a client message.</param>
        public static byte[] createQuaternionSyncVarMessage(NetworkBehaviour netBehaviour, Type objType, string fieldName, Quaternion value, bool clientMessage = false)
        {
            List<byte> msgBytes = new List<byte>();

            byte msgHeader = (byte)(clientMessage ? 24 : 23); //Change message header to indicate if it's a client message or not
            msgBytes.Add(msgHeader); //Message Header

            //Identity of the network behaviour object on the network
            msgBytes.AddRange(netBehaviour.networkIdentity.networkID.serialize());

            List<byte> bytesToCompress = new List<byte>();

            //SyncVar data
            bytesToCompress.AddRange(objType.serialize());
            bytesToCompress.AddRange(fieldName.serialize());

            //Serializes compressed bytes as array for deserialization
            msgBytes.AddRange(bytesToCompress.ToArray().compress().serialize());

            //SyncVar Quaternion value
            msgBytes.AddRange(value.x.serialize());
            msgBytes.AddRange(value.y.serialize());
            msgBytes.AddRange(value.z.serialize());
            msgBytes.AddRange(value.w.serialize());

            return msgBytes.ToArray();
        }

        /// <summary>
        /// Creates a Vector2 SyncVar message.
        /// </summary>
        /// <param name="netBehaviour">NetworkBehaviour the SyncVar is on.</param>
        /// <param name="objType">Type of the object containing the SyncVar.</param>
        /// <param name="fieldName">Name of the SyncVar.</param>
        /// <param name="value">Value of the SyncVar.</param>
        /// <param name="clientMessage">Indicates if this is a client message.</param>
        public static byte[] createVector2SyncVarMessage(NetworkBehaviour netBehaviour, Type objType, string fieldName, Vector2 value, bool clientMessage = false)
        {
            List<byte> msgBytes = new List<byte>();

            byte msgHeader = (byte)(clientMessage ? 26 : 25); //Change message header to indicate if it's a client message or not
            msgBytes.Add(msgHeader); //Message Header

            //Identity of the network behaviour object on the network
            msgBytes.AddRange(netBehaviour.networkIdentity.networkID.serialize());

            List<byte> bytesToCompress = new List<byte>();

            //SyncVar data
            bytesToCompress.AddRange(objType.serialize());
            bytesToCompress.AddRange(fieldName.serialize());

            //Serializes compressed bytes as array for deserialization
            msgBytes.AddRange(bytesToCompress.ToArray().compress().serialize());

            //SyncVar Vector2 value
            msgBytes.AddRange(value.x.serialize());
            msgBytes.AddRange(value.y.serialize());

            return msgBytes.ToArray();
        }

        /// <summary>
        /// Creates a Vector3 SyncVar message.
        /// </summary>
        /// <param name="netBehaviour">NetworkBehaviour the SyncVar is on.</param>
        /// <param name="objType">Type of the object containing the SyncVar.</param>
        /// <param name="fieldName">Name of the SyncVar.</param>
        /// <param name="value">Value of the SyncVar.</param>
        /// <param name="clientMessage">Indicates if this is a client message.</param>
        public static byte[] createVector3SyncVarMessage(NetworkBehaviour netBehaviour, Type objType, string fieldName, Vector3 value, bool clientMessage = false)
        {
            List<byte> msgBytes = new List<byte>();

            byte msgHeader = (byte)(clientMessage ? 28 : 27); //Change message header to indicate if it's a client message or not
            msgBytes.Add(msgHeader); //Message Header

            //Identity of the network behaviour object on the network
            msgBytes.AddRange(netBehaviour.networkIdentity.networkID.serialize());

            List<byte> bytesToCompress = new List<byte>();

            //SyncVar data
            bytesToCompress.AddRange(objType.serialize());
            bytesToCompress.AddRange(fieldName.serialize());

            //Serializes compressed bytes as array for deserialization
            msgBytes.AddRange(bytesToCompress.ToArray().compress().serialize());

            //SyncVar Vector3 value
            msgBytes.AddRange(value.x.serialize());
            msgBytes.AddRange(value.y.serialize());
            msgBytes.AddRange(value.z.serialize());

            return msgBytes.ToArray();
        }

        #endregion SyncVar

        #region Network Identity

        /// <summary>
        /// Creates a network ID request message for requesting a network ID for the given transform path.
        /// </summary>
        /// <param name="transformPath">Transform path in the hierarchy of the NetworkIdentity.</param>
        public static byte[] createNetworkIDRequestMessage(string transformPath)
        {
            List<byte> msgBytes = new List<byte>();
            
            msgBytes.Add(29); //Message Header

            msgBytes.AddRange(transformPath.serialize()); //Transform path of the NetworkIdentity

            return msgBytes.ToArray();
        }

        /// <summary>
        /// Creates a network ID message for assigning a network ID to a given transform path.
        /// </summary>
        /// <param name="transformPath">Transform path in the hierarchy of the NetworkIdentity.</param>
        /// <param name="networkID">Network ID assigned to transformPath.</param>
        public static byte[] createNetworkIDMessage(string transformPath, short networkID)
        {
            List<byte> msgBytes = new List<byte>();

            msgBytes.Add(30); //Message Header

            msgBytes.AddRange(transformPath.serialize());
            msgBytes.AddRange(networkID.serialize());

            return msgBytes.ToArray();
        }

        #endregion Network Identity

        /// <summary>
        /// Creates an avatar request message for creating a network avatar for the given client.
        /// </summary>
        /// <param name="clientMessage">Indicates if this is a client message.</param>
        /// <param name="clientID">ID of the client requesting an avatar.</param>
        public static byte[] createAvatarRequestMessage(bool clientMessage = true, short clientID = -1)
        {
            List<byte> msgBytes = new List<byte>();

            byte msgHeader = (byte)(clientMessage ? 32 : 31); //Change message header to indicate if it's a client message or not
            msgBytes.Add(msgHeader); //Message Header

            //Client ID is added by ClientMessageSender automatically if it is sent by the client
            if (!clientMessage)
                msgBytes.AddRange(clientID.serialize());

            return msgBytes.ToArray();
        }

        public static byte[] createNetworkEventMessage(NetworkIdentity networkIdentity, bool clientMessage = false)
        {
            List<byte> msgBytes = new List<byte>();

            byte msgHeader = (byte)(clientMessage ? 33 : 34); //Change message header to indicate if it's a client message or not
            msgBytes.Add(msgHeader); //Message Header

            msgBytes.AddRange(networkIdentity.networkID.serialize()); //Identity of the network behaviour object on the network

            return msgBytes.ToArray();
        }
    }
}