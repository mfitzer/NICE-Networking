using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NICE_Networking
{
    public class NetworkedEvent : NetworkBehaviour
    {
        /// <summary>
        /// Called when the event is invoked.
        /// </summary>
        public UnityEvent OnEventInvoked;

        /// <summary>
        /// Invokes the event over the network.
        /// </summary>
        public void invoke()
        {            
            networkIdentity.sendMessage(MessageFactory.createNetworkEventMessage(networkIdentity, !NetworkIdentity.isServer));
            OnEventInvoked.Invoke();
        }
    }
}