using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NICE_Networking
{
    public static class SyncVarMessageFactory
    {
        /// <summary>
        /// Indicates if the given Type, syncVarType, is a supported SyncVarType.
        /// </summary>
        /// <param name="syncVarType"></param>
        /// <returns></returns>
        public static bool isTypeSupported(Type syncVarType)
        {
            List<Type> interfaces = new List<Type>(syncVarType.GetInterfaces());

            return !interfaces.Contains(typeof(ICollection)) && ( //Not a collection
                      syncVarType.IsSerializable ||
                      syncVarType == typeof(GameObject) ||
                      syncVarType == typeof(Vector3) ||
                      syncVarType == typeof(Vector2) ||
                      syncVarType == typeof(Quaternion) ||
                      syncVarType == typeof(Color)
                   );
        }

        /// <summary>
        /// Creates and returns a SyncVarMessage for the given fieldType if it's valid.
        /// </summary>
        /// <param name="networkBehaviour">NetworkBehaviour managing the SyncVar.</param>
        /// <param name="objType">Type of the SyncVar field.</param>
        /// <param name="fieldType">Type of the SyncVar field.</param>
        /// <param name="fieldName">Name of the SyncVar field.</param>
        /// <param name="value">Value of the SyncVar.</param>
        /// <param name="clientMessage">Determines if this is a client message.</param>
        /// <returns>SyncVar message bytes if the fieldType is valid, null otherwise.</returns>
        public static byte[] createSyncVarMessage(NetworkBehaviour networkBehaviour, Type objType, Type fieldType, string fieldName, object value, bool clientMessage)
        {
            if (fieldType.IsSerializable)
            {
                return MessageFactory.createBaseSyncVarMessage(networkBehaviour, objType, fieldType, fieldName, value);
            }
            else if (fieldType == typeof(GameObject))
            {
                return MessageFactory.createGameObjectSyncVarMessage(networkBehaviour, objType, fieldName, (GameObject)value);
            }
            else if (fieldType == typeof(Vector3))
            {
                return MessageFactory.createVector3SyncVarMessage(networkBehaviour, objType, fieldName, (Vector3)value);
            }
            else if (fieldType == typeof(Vector2))
            {
                return MessageFactory.createVector2SyncVarMessage(networkBehaviour, objType, fieldName, (Vector2)value);
            }
            else if (fieldType == typeof(Quaternion))
            {
                return MessageFactory.createQuaternionSyncVarMessage(networkBehaviour, objType, fieldName, (Quaternion)value);
            }
            else if (fieldType == typeof(Color))
            {
                return MessageFactory.createColorSyncVarMessage(networkBehaviour, objType, fieldName, (Color)value);
            }

            return null;
        }
    }
}