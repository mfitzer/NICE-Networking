using System.Collections;
using System.Collections.Generic;

namespace NICE_Networking
{
    internal class ClientMessageSender
    {
        private static ClientMessageSender instance;
        public static ClientMessageSender Instance
        {
            get
            {
                if (instance == null)
                    instance = new ClientMessageSender();
                return instance;
            }
        }

        private Queue<QueuedMessage> clientMessageQueue;

        private ClientMessageSender()
        {
            clientMessageQueue = new Queue<QueuedMessage>();
            ClientBehaviour.Instance.OnClientIDSet += handleClientIDSet;
        }

        /// <summary>
        /// Sends a message to the server once the ClientBehavior has been assigned a client ID.
        /// </summary>
        /// <param name="message">Client message to be sent.</param>
        /// <param name="guaranteeDelivery">Determines if delivery and delivery order should be guaranteed.</param>
        public void sendMessage(byte[] message, bool guaranteeDelivery = false)
        {
            if (ClientBehaviour.Instance.ClientID == ClientBehaviour.ID_NOT_SET) //Client ID not set
            {
                if (guaranteeDelivery) //Queue message if delivery must be guaranteed
                    clientMessageQueue.Enqueue(new QueuedMessage(message, guaranteeDelivery));
            }
            else //Client ID is set, send message to server
                sendMessageToServer(message, guaranteeDelivery);
        }

        private void handleClientIDSet(short clientID)
        {
            //Send messages to server
            while (clientMessageQueue.Count > 0)
            {
                QueuedMessage queuedMessage = clientMessageQueue.Dequeue(); //Get next message
                sendMessageToServer(queuedMessage.msg, queuedMessage.guaranteeDelivery);
            }
        }

        private void sendMessageToServer(byte[] message, bool guaranteeDelivery)
        {
            message = message.append(ClientBehaviour.Instance.ClientID.serialize()); //Append client ID to the message
            ClientBehaviour.Instance.sendMessage(message, guaranteeDelivery);
        }
    }
}
