# Key Concepts
Below are some key concepts that describe the fundamentals of the NICE Networking system.

## Networking GameObjects
NICE Networking is designed to facilitate networking functionality across multiple Unity projects. This means that GameObjects must have unique, network identifiers that can be used for identifying the same GameObject in every project. To do this, GameObjects have a NetworkIdentity component attached. The NetworkIdentity registers the path of the GameObject in the scene hierarchy with a unique network ID on the server. This network ID is then used to identify that GameObject across the network.

Due to the path in the scene hierarchy being used to identify networked GameObjects, there are two things that should be noted. The first is that it is crucial that networked GameObjects have the exact same path in the scene hierarchy across projects or they will be identified as different objects. The second is that there cannot be two networked GameObjects with the same name at the same level in the hierarchy.

## Network Authority
NICE Networking uses an authoritative network design. This means that each networked GameObject is either under client or server authority. Network authority determines which network entity, the client or the server, has control over that object. This means that when the network entity with authority over an object changes some piece of networked data, such as its position, that change is propagated across the network to any other connected network entities.

Network authority is set by selecting either *Client* or *Server* authority on the NetworkIdentity component.

### Example
To see an example of the above concepts, navigate to `NICE-Networking/Samples/NetworkingGameObjects`.  Here you can find client and server scenes which show the right and wrong way to setup networked GameObjects. It also shows examples of both client and server controlled GameObjects.

To test the sample, build and run the client scene and run the server scene in the editor. Once the client is connected, disable all four cubes on the server. Then, if you go back to the client, you'll see only the cube that was properly placed in the scene hierarchy and under server authority was disabled.
