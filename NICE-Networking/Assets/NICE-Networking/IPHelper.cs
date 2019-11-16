using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace NICE_Networking
{
    public static class IPHelper
    {
        /// <summary>
        /// Gets the local IPv4 address of the device.
        /// </summary>
        /// <returns></returns>
        public static string getLocalIPv4Address()
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }

                Debug.Log("<color=cyan><b>[IPManager]</b></color> IP address could not be found.");
                throw new System.Exception("IP address could not be found.");
            }
            else
            {
                Debug.Log("<color=cyan><b>[IPManager]</b></color> A network connection could not be found.");
                throw new System.Exception("A network connection could not be found.");
            }
        }

        /// <summary>
        /// Determines if the provided string is a valid IPv4 address.
        /// </summary>
        /// <param name="ipAddress">0.0.0.0 style IP address</param>
        /// <returns>bool</returns>
        public static bool validateIPv4(string ipAddress)
        {
            // Split string by ".", check that array length is 3
            char chrFullStop = '.';
            string[] arrOctets = ipAddress.Split(chrFullStop);
            if (arrOctets.Length != 4)
            {
                return false;
            }
            // Check each substring checking that the int value is less than 255 
            // and that is char[] length is !>     2
            Int16 MAXVALUE = 255;
            Int32 temp; // Parse returns Int32
            foreach (string strOctet in arrOctets)
            {
                if (strOctet.Length > 3)
                {
                    return false;
                }

                try
                {
                    temp = int.Parse(strOctet);
                    if (temp > MAXVALUE)
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
    }
}
