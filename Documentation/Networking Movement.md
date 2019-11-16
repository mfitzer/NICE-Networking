
# Networking Movement
Networking movement in NICE Networking is done using the NetworkTransform component. The NetworkTransform updates the network anytime the position, rotation, or scale of the transform changes.

The NetworkTransform has a field called SyncMode, which can be set to either SyncTransform or SyncRigidbody. Both modes synchronize the transform across the network, however, if SyncRigidbody is used, it also synchronizes an attached Rigidbody. This allows for networked physics interactions.

## Example
To see an example of this set up, navigate to `NICE-Networking/Samples/Movement`. Here you can find client and server scenes showing how to set up the client and server respectively.

Looking at the two scenes, you'll see that there are two server controlled cubes, each with an attached NetworkTransform. However, they are using different SyncModes.

To test the sample, build and run the client scene and run the server scene in the editor. Once the client is connected, starting changing the transform data on the cubes in the server scene, note the difference in how the two cubes behave.