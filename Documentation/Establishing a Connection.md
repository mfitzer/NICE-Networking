# Establishing a Connection
To establish a network connection, a client and server are needed. In the client project, create an empty GameObject and add a ClientBehaviour component. In the server project, do the same, but instead add a ServerBehaviour component.

The ClientBehaviour needs know the IP address of the server to which it is going to connect. This can be set through the inspector or it can be set at runtime using the NetworkSettings static class. An example of how to do this can be seen in the ClientConfigPanel prefab in `NICE-Networking/Prefabs`.

## Example
To see an example of this set up, navigate to `NICE-Networking/Samples/EstablishingConnection`. Here you can find client and server scenes showing how to set up the client and server respectively.

To test the sample, build and run the client scene and run the server scene in the editor. The client will connect to the server once the server IP address is properly configured.