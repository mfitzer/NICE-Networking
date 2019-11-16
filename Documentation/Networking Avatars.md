# Networking Avatars
Player avatars are not required with NICE Networking, however, they are supported by the AvatarManager component. To use it, the AvatarManager must be added to both the client and the server. 

## Client Setup
On the client, a player avatar prefab must be provided if that client is going to have a networked player avatar. A client avatar prefab must also be provided to represent other connected clients. Make sure the NetworkAuthority on the client avatar prefab is set to *Server* because that avatar is controlled by another client, through the server. The client avatar prefab should not have any player control scripts on it because it is controlled by the other clients.

## Server Setup
On the server, the a different client avatar prefab should be provided to the AvatarManager, with its NetworkAuthority set to *Client*.

## Mixed Avatar Setup
There are some use cases which require only some clients to have avatars, so a client will only have an avatar on the network if a player avatar prefab is provided to the AvatarManager. However, that client will still see client avatars for other connected clients who have avatars as long as it has the AvatarManager component.

## Example
To see an example of this set up, navigate to `NICE-Networking/Samples/Avatars`. Here you can find client and server scenes showing how to set up the client and server respectively. Notice there are two different client scenes, one with a player avatar and one without one.

Looking at these scenes, you'll see that each scene has an AvatarManager. Pay attention to how each one is configured.

To test the sample, build and run both client scenes and run the server scene in the editor. Once the clients are connected, start moving the player avatar in the client with a player avatar using the WASD or arrow keys on your keyboard. The player avatar will then move on both the server and client without an avatar.