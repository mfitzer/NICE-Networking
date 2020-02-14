# Networking Events

The Unity Event system is amazingly flexible in its ability to quickly wire different components together through the editor. NICE Networking provides the NetworkedEvent component to utilize the that flexibility over the network.

The NetworkedEvent component has a UnityEvent, OnEventInvoked, which is invoked over the network by an invoke method. When invoke() is called on the NetworkedEvent that has NetworkAuthority, it sends a message over the network, which then invokes OnEventInvoked on the corresponding NetworkedEvent.

## Example

To see an example of this set up, navigate to `NICE-Networking/Samples/NetworkEvents`. Here you can find client and server scenes showing how to set up the client and server respectively.

To test the sample, build and run the client scene and run the server scene in the editor. Connect to the server and select the GameObject called Network Event Test. Then set the variable, InvokeEvent, on the NetworkEventTest component to true. This will fire the event once, incrementing a counter on the client.