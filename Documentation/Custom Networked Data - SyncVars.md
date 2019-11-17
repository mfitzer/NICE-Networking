# Custom Networked Data - SyncVars
Networking custom data can be done using the SyncVar attribute. The SyncVar attribute marks a variable to be shared across the network, so every time its value is changed by the authoritative network entity, that change is sent to the other connected network entities.

## Usage
To use SyncVars, there are a couple requirements that must be met:
 - SyncVars will only work in scripts that inherit from NetworkBehaviour
 - In the Start method of the script, the NetworkBehaviour method, `initialize()` must be called, passing the keyword `this` as a parameter

### Supported SyncVar Data Types
The type of a SyncVar **must be a field, not a property,** and it must meet one of the following requirements for it to work:
 - Is serializable and does not implement `ICollection` interface
 - Is a `GameObject` (*The GameObject value must have a NetworkIdentity attached*)
 - Is one of the following types: `Vector3`, `Vector2`, `Quaternion`, or `Color`

## Example
To see an example of this set up, navigate to `NICE-Networking/Samples/SyncVars`. Here you can find client and server scenes showing how to set up the client and server respectively.

Looking at these scenes, you'll see that each scene has a `SyncVarTest` component which is set to *Server* authority.

To test the sample, build and run the client scene and run the server scene in the editor. Once the client is connected, start changing the values of the public fields on the `SyncVarTest` component. On the client, the current value of each of those fields is displayed.
