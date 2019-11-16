using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

namespace NICE_Networking
{
    public class NetworkEntityManager
    {
        private static NetworkEntityManager instance;
        public static NetworkEntityManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new NetworkEntityManager();
                return instance;
            }
        }

        private NetworkEntityManager()
        {
            unidentifiedNetworkEntities = new Dictionary<string, NetworkIdentity>();
            networkEntityManifest = new Dictionary<short, NetworkEntity>();
        }

        /// <summary>
        /// NetworkIdentity objects without an assigned NetworkID.
        /// </summary>
        private Dictionary<string, NetworkIdentity> unidentifiedNetworkEntities;

        /// <summary>
        /// Network entities with unique network IDs.
        /// </summary>
        private Dictionary<short, NetworkEntity> networkEntityManifest;

        /// <summary>
        /// Adds the NetworkIdentity with the given path in the hierarchy to the manifest if it doesn't already exist.
        /// </summary>
        /// <param name="networkIdentity">NetworkIdentity being added.</param>
        internal bool add(NetworkIdentity networkIdentity)
        {
            //Need to wait for the network ID from the server
            if (networkIdentity.networkID == NetworkIdentity.ID_NOT_SET)
            {
                string transformPath = networkIdentity.transform.getFullTransformPath();

                if (!unidentifiedNetworkEntities.ContainsKey(transformPath)) //Not already added
                    unidentifiedNetworkEntities.Add(transformPath, networkIdentity);

                if (ServerBehaviour.Instance) //Running on server, don't need to request ID over the network
                    setNetworkID(transformPath, NetworkIdentityRegistry.Instance.getNetworkID(transformPath));
                else if (ClientBehaviour.Instance) //Running on the client, request ID from the server
                {
                    ClientMessageSender.Instance.sendMessage(MessageFactory.createNetworkIDRequestMessage(transformPath), true);
                }
                return true;
            }
            else if (!networkEntityManifest.ContainsKey(networkIdentity.networkID))
            {
                networkEntityManifest.Add(networkIdentity.networkID, new NetworkEntity(networkIdentity));
                Debug.Log("<b><color=purple>[NetworkEntityManager]</color></b> Adding: " + networkIdentity.transform.getFullTransformPath() + " with id: " + networkIdentity.networkID + " to manifest.");
                return true;
            }
            else
            {
                Debug.Log("<b><color=purple>[NetworkEntityManager]</color></b> " + networkIdentity.networkID + " could not be added to manifest, it already exists.");
            }
            return false;
        }       

        /// <summary>
        /// Resend network ID requests for unidentified network entities.
        /// </summary>
        private void resendNetworkIDRequests(object source, ElapsedEventArgs e)
        {
            if (unidentifiedNetworkEntities.Count > 0) //There are still some unidentified network entities
            {
                foreach (KeyValuePair<string, NetworkIdentity> waitingNetEntity in unidentifiedNetworkEntities)
                {
                    ClientMessageSender.Instance.sendMessage(MessageFactory.createNetworkIDRequestMessage(waitingNetEntity.Key), true);
                }
            }
        }

        /// <summary>
        /// Sets the network ID of an unidentified NetworkIdentity with the given transform path.
        /// </summary>
        /// <param name="transformPath"></param>
        /// <param name="networkID"></param>
        /// <returns>Bool indicating if the network ID was successfully set.</returns>
        internal bool setNetworkID(string transformPath, short networkID)
        {
            if (unidentifiedNetworkEntities.ContainsKey(transformPath))
            {
                NetworkIdentity networkIdentity = unidentifiedNetworkEntities[transformPath];
                unidentifiedNetworkEntities.Remove(transformPath);

                networkIdentity.networkID = networkID;
                add(networkIdentity); //Add to manifest
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the gameObject with the given network ID from the manifest if it exists.
        /// </summary>
        /// <param name="networkID">Network ID of the object being removed.</param>
        internal bool remove(short networkID)
        {
            return networkEntityManifest.Remove(networkID);
        }

        /// <summary>
        /// Removes the given NetworkIdentity from the manifest if present.
        /// </summary>
        /// <param name="networkID">Network ID of the object being removed.</param>
        internal bool remove(NetworkIdentity networkIdentity)
        {
            unidentifiedNetworkEntities.Remove(networkIdentity.transform.getFullTransformPath());
            return networkEntityManifest.Remove(networkIdentity.networkID);
        }

        /// <summary>
        /// Searches the manifest for a GameObject with the given path in the hierarchy.
        /// </summary>
        /// <param name="networkID">Network ID of the object being searched.</param>
        /// <param name="obj">GameObject at the given transform path.</param>
        public bool search(short networkID, out GameObject obj)
        {
            if (networkEntityManifest.ContainsKey(networkID))
            {
                obj = networkEntityManifest[networkID].gameObject;
                return true;
            }

            obj = null;
            return false;
        }

        /// <summary>
        /// Searches the manifest for a Component of type T attached to a GameObject with the given path in the hierarchy.
        /// </summary>
        /// <typeparam name="T">Type of the Component to be retrieved.</typeparam>
        /// <param name="networkID">Network ID of the object being searched.</param>
        /// <param name="component">First instance of a Component of type T attached to a GameObject with the given transformPath.</param>
        /// <param name="shouldRefresh">Determines if the list of Component types of type T will be refreshed.</param>
        /// <returns>Boolean indicating if a component was found.</returns>
        public bool findComponent<T>(short networkID, out T component, bool shouldRefresh = false) where T : Component
        {
            //An object with the given transform path has been discovered
            if (networkEntityManifest.ContainsKey(networkID))
            {
                //Found a component of type T
                if (networkEntityManifest[networkID].searchComponent(out T foundComponent, shouldRefresh))
                {
                    component = foundComponent;
                    return true;
                }
            }

            component = null;
            return false;
        }

        /// <summary>
        /// Searches the manifest for all Components of type T attached to a GameObject with the given path in the hierarchy.
        /// </summary>
        /// <typeparam name="T">Type of the Component to be retrieved.</typeparam>
        /// <param name="networkID">Network ID of the object being searched.</param>
        /// <param name="components">List of all Components of type T attached to a GameObject with the given transformPath.</param>
        /// <param name="shouldRefresh">Determines if the list of Component types of type T will be refreshed.</param>
        /// <returns>Boolean indicating if a component was found.</returns>
        public bool findComponents<T>(short networkID, out List<T> components, bool shouldRefresh = false) where T : Component
        {
            //An object with the given transform path has been discovered
            if (networkEntityManifest.ContainsKey(networkID))
            {
                //Found components of type T
                if (networkEntityManifest[networkID].searchComponents(out List<T> foundComponents, shouldRefresh))
                {
                    components = foundComponents;
                    return true;
                }
            }

            components = null;
            return false;
        }

        private class NetworkEntity
        {
            public GameObject gameObject;
            private IDictionary<System.Type, List<Component>> discoveredComponents;

            public NetworkEntity(NetworkIdentity netIdentity)
            {
                gameObject = netIdentity.gameObject;
                discoveredComponents = new Dictionary<System.Type, List<Component>>();

                discoveredComponents.Add(typeof(NetworkIdentity), new List<Component>() { netIdentity });
            }

            #region Search

            /// <summary>
            /// Finds the first instance of a Component of type T attached to gameObject.
            /// </summary>
            /// <typeparam name="T">Type of the Component to be retrieved.</typeparam>
            /// <param name="component">First instance of a Component of type T attached to gameObject.</param>
            /// <param name="shouldRefresh">Determines if the list of Component types of type T will be refreshed.</param>
            /// <returns>Boolean indicating if a Component of type T could be found.</returns>
            public bool searchComponent<T>(out T component, bool shouldRefresh = false) where T : Component
            {
                addComponentType<T>(shouldRefresh);

                component = null;

                //gameObject has Components of type T attached
                if (discoveredComponents.ContainsKey(typeof(T)))
                {
                    List<Component> components = discoveredComponents[typeof(T)];
                    if (components != null && components.Count > 0)
                    {
                        //Continuously stores the first element of components in component
                        //until the end of the list is reached or a nonnull component is found
                        while (components.Count > 0 && (component = (T)components[0]) == null)
                        {
                            components.Remove(components[0]); //Remove null component
                        }

                        if (component)
                            return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Finds the all instances of a Component of type T attached to gameObject.
            /// </summary>
            /// <typeparam name="T">Type of the Component to be retrieved.</typeparam>
            /// <param name="components">List of all instance of a Component of type T.</param>
            /// <param name="shouldRefresh">Determines if the list of Component types of type T will be refreshed.</param>
            /// <returns>Boolean indicating if any Components of type T could be found.</returns>
            public bool searchComponents<T>(out List<T> components, bool shouldRefresh = false) where T : Component
            {
                addComponentType<T>(shouldRefresh);

                //gameObject has Components of type T attached
                if (discoveredComponents.ContainsKey(typeof(T)))
                {
                    components = new List<T>();

                    //Build components List, casting each Component instance as type T
                    foreach (Component component in discoveredComponents[typeof(T)])
                    {
                        if (component) //Component is not null
                            components.Add((T)component);
                        else //Remove null component
                            discoveredComponents[typeof(T)].Remove(component);
                    }

                    //Remove component list for type T since there are none attached
                    if (components.Count == 0)
                    {
                        discoveredComponents.Remove(typeof(T));
                        return false;
                    }

                    return true;
                }

                components = null;
                return false;
            }

            #endregion Search

            /// <summary>
            /// Finds all Components of type T attached to gameObject.
            /// </summary>
            /// <typeparam name="T">Type of the Component to be found.</typeparam>
            /// <returns></returns>
            private bool addComponentType<T>(bool shouldRefresh = false) where T : Component
            {
                if (!discoveredComponents.ContainsKey(typeof(T))) //Doesn't contain a list for the type T
                {
                    T[] objInstances = gameObject.GetComponents<T>(); //Find all Components of type T on gameObject
                    if (objInstances != null && objInstances.Length > 0) //gameObject has Components of type T attached
                    {
                        //Add a new list of components of type T
                        discoveredComponents.Add(typeof(T), new List<Component>(objInstances));
                        return true;
                    }
                }
                else if (shouldRefresh)
                {
                    T[] objInstances = gameObject.GetComponents<T>(); //Find all Components of type T on gameObject
                    if (objInstances != null && objInstances.Length > 0) //gameObject has Components of type T attached
                    {
                        //Overwrite old list with udpated list of components of type T
                        discoveredComponents[typeof(T)] = new List<Component>(objInstances);
                        return true;
                    }
                }

                //Already contains a list for type T or no components of type T could be found
                return false;
            }
        }
    }
}
