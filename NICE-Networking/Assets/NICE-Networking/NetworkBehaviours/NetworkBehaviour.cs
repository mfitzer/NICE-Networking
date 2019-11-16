using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NICE_Networking
{
    [RequireComponent(typeof(NetworkIdentity))]
    public abstract class NetworkBehaviour : MonoBehaviour
    {
        /// <summary>
        /// NetworkIdentity component attached to this GameObject
        /// </summary>
        public NetworkIdentity networkIdentity { get; private set; }

        /// <summary>
        /// Indicates if the network behaviour has network authority over the object.
        /// </summary>
        protected bool hasNetworkAuthority
        {
            get
            {
                //Only have network authority if the network identity matches the NetworkAuthority
                return (networkIdentity.networkAuthority == NetworkAuthority.CLIENT && NetworkIdentity.isClient) ||
                       (networkIdentity.networkAuthority == NetworkAuthority.SERVER && NetworkIdentity.isServer);
            }
        }

        #region SyncVars

        private class SyncVarData
        {
            /// <summary>
            /// Network object with the SyncVar.
            /// </summary>
            public NetworkBehaviour networkObj { get; private set; }

            /// <summary>
            /// SyncVar field.
            /// </summary>
            public FieldInfo field { get; private set; }

            /// <summary>
            /// Value of the SyncVar.
            /// </summary>
            public object value;

            public SyncVarData(NetworkBehaviour networkObj, FieldInfo field, object value)
            {
                this.networkObj = networkObj;
                this.field = field;
                this.value = value;
            }
        }

        /// <summary>
        /// Holds references to all SyncVars on this NetworkBehaviour.
        /// </summary>
        private Dictionary<Type, Dictionary<string, SyncVarData>> syncVars;

        #endregion SyncVars

        private void Awake()
        {
            networkIdentity = GetComponent<NetworkIdentity>();
            syncVars = new Dictionary<Type, Dictionary<string, SyncVarData>>();
        }

        private void Start()
        {
            networkIdentity.OnNetworkAuthorityChanged += OnNetworkAuthorityChanged;
        }

        /// <summary>
        /// Called when the network authority on networkIdentity is changed.
        /// </summary>
        /// <param name="networkAuthority">New network authority</param>
        protected virtual void OnNetworkAuthorityChanged(NetworkAuthority networkAuthority) { }

        /// <summary>
        /// Performs required initializations for the NetworkBehaviour.
        /// </summary>
        /// <param name="networkObj">Object being initialized.</param>
        protected void initialize<T>(T networkObj) where T : NetworkBehaviour
        {
            networkIdentity.trackSyncVars(networkObj);
            loadSyncVars(networkObj);
        }

        #region SyncVars

        /// <summary>
        /// Loads all SyncVars in networkObj.
        /// </summary>
        /// <param name="networkObj">The object that the SyncVars are loaded from.</param>
        private void loadSyncVars<T>(T networkObj) where T : NetworkBehaviour
        {
            Type objType = typeof(T);
            FieldInfo[] objectFields = objType.GetFields();
            foreach (FieldInfo field in objectFields)
            {
                SyncVar syncVar = (SyncVar)Attribute.GetCustomAttribute(field, typeof(SyncVar));

                //Found a SyncVar
                if (syncVar != null)
                {
                    //Add a new Dictionary for object type if it doesn't exist
                    if (!syncVars.ContainsKey(objType))
                        syncVars.Add(objType, new Dictionary<string, SyncVarData>());

                    object value = field.GetValue(networkObj);

                    //Can only use SyncVars supported by the SyncVarMessageFactory
                    if (SyncVarMessageFactory.isTypeSupported(field.GetType()))
                    {
                        syncVars[objType].Add(field.Name, new SyncVarData(networkObj, field, value));
                        //Debug.Log("[SyncVar Loaded] ObjectType: " + objType + " | Field: " + field.Name + " | Value: " + value);
                    }
                    else
                        Debug.LogWarning("The type, " + field.GetType() + ", is not a supported SyncVar type.");
                }
            }
        }

        /// <summary>
        /// Sets the value of a SyncVar with the name, fieldName on a NetworkBehaviour object of Type, objType.
        /// </summary>
        /// <typeparam name="T">Type of the SyncVar field.</typeparam>
        /// <param name="objType">Type of the object with the SyncVar.</param>
        /// <param name="fieldName">Name of the SyncVar field.</param>
        /// <param name="value">Value of the SyncVar.</param>
        internal bool setSyncVar<T>(Type objType, string fieldName, T value)
        {
            //There is at least one SyncVar on an object of Type, objType
            if (syncVars.ContainsKey(objType))
            {
                //Found the SyncVar
                if (syncVars[objType].ContainsKey(fieldName))
                {
                    SyncVarData syncVarData = syncVars[objType][fieldName];
                    syncVarData.field.SetValue(syncVarData.networkObj, value);
                    return true;
                }
                else
                    Debug.Log("Could not find a SyncVar with field name: " + fieldName);
            }
            else
                Debug.Log("Could not find a SyncVar on an object of Type: " + objType);

            return false;
        }

        /// <summary>
        /// Monitors SyncVars for change.
        /// </summary>
        private void monitorSyncVars()
        {
            //Find SyncVars that changed
            foreach (KeyValuePair<Type, Dictionary<string, SyncVarData>> typeSyncVars in syncVars)
            {
                foreach (KeyValuePair<string, SyncVarData> fieldSyncVarData in typeSyncVars.Value)
                {
                    SyncVarData syncVarData = fieldSyncVarData.Value;
                    object value = syncVarData.field.GetValue(syncVarData.networkObj);

                    //Value of SyncVar changed
                    if (!value.Equals(syncVarData.value))
                    {
                        syncVarData.value = value;

                        if (networkIdentity.networkAuthority == NetworkAuthority.CLIENT)
                            networkIdentity.sendMessage(SyncVarMessageFactory.createSyncVarMessage(this, typeSyncVars.Key, value.GetType(), syncVarData.field.Name, value, true), true);
                        else if (networkIdentity.networkAuthority == NetworkAuthority.SERVER)
                            networkIdentity.sendMessage(SyncVarMessageFactory.createSyncVarMessage(this, typeSyncVars.Key, value.GetType(), syncVarData.field.Name, value, false), true);
                    }
                }
            }
        }

        private void LateUpdate()
        {
            //Only monitor value of SyncVars when this has NetworkAuthority over them
            if (hasNetworkAuthority)
            {
                monitorSyncVars();
            }
        }

        #endregion SyncVars
    }
}