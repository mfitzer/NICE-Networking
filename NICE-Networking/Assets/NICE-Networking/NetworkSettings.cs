namespace NICE_Networking
{
    public static class NetworkSettings
    {
        public delegate void SettingChanged();

        #region Server IP

        public static SettingChanged OnServerIPChanged;
        public const string INVALID_IP = "";
        
        private static string serverIPAddress = "";

        /// <summary>
        /// IP address of the server to which client(s) connect.
        /// </summary>
        public static string serverIP
        {
            get
            {
                //serverIPAddress is set and valid
                if (IPHelper.validateIPv4(serverIPAddress))
                    return serverIPAddress;

                return INVALID_IP;
            }
            set
            {
                //IP address is valid
                if (IPHelper.validateIPv4(value))
                {
                    serverIPAddress = value;
                    OnServerIPChanged?.Invoke();
                }                
            }
        }

        #endregion Server IP

        #region Port

        public static SettingChanged OnServerPortChanged;
        private static ushort connectionPort = 9000; ///9000 is default

        /// <summary>
        /// Port on which the client(s) will be connecting to the server.
        /// </summary>
        public static ushort serverPort
        {
            get
            {
                return connectionPort;
            }
            set
            {
                connectionPort = value;
                OnServerPortChanged?.Invoke();
            }
        }

        #endregion
    }
}
