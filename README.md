
# Networking Events

The Unity Event system is amazingly flexible in its ability to quickly wire different components together through the editor. NICE Networking provides the NetworkedEvent component to utilize the that flexibility over the network.

The NetworkedEvent component has a UnityEvent, OnEventInvoked, which is invoked over the network by an invoke method. When invoke() is called on the NetworkedEvent that has NetworkAuthority, it sends a message over the network, which then invokes OnEventInvoked on the corresponding NetworkedEvent.

## Example

To see an example of this set up, navigate to `NICE-Networking/Samples/NetworkEvents`. Here you can find client and server scenes showing how to set up the client and server respectively.

To test the sample, build and run the client scene and run the server scene in the editor. Connect to the server and select the GameObject called Network Event Test. Then set the variable, InvokeEvent, on the NetworkEventTest component to true. This will fire the event once, incrementing a counter on the client.# NICE-Networking
This project is a networking library built on top of Unity's new real time multiplayer [Real-time Multiplayer](https://github.com/Unity-Technologies/multiplayer) solution. Specifically, this project is built on top of the [Workflow: Creating a minimal client and server](https://github.com/Unity-Technologies/multiplayer/blob/master/com.unity.transport/Documentation~/workflow-client-server.md) sample from Unity. It aims to make it easy to incorporate local multiplayer functionality into projects by replicating some features from Unity's old networking solution such as NetworkTransforms and SyncVars.

**Note:** The NICE Networking library is currently being developed in conjunction with the virtual reality project, [MN NICE](https://www.facebook.com/watch/?v=742787122902502).

## Getting Started
To get started with NICE-Networking, follow the installation instructions below and then view the documentation to see what NICE-Networking has to offer.

### Installation 
 - Clone this repository
 - Open the Unity project
 - Build your project on top of this project or select the `NICE-Networking/Assets/NICE-Networking` folder and export it as a package
 - Import the package into a new Unity project using Unity 2019.1 or newer
 - Copy the `NICE-Networking/Packages/com.unity.transport` folder into the Packages folder of your new project
 - Ensure the Scripting Runtime Version under `Edit > Project Settings > Player > Other Settings` is set to *Scripting Runtime Version 4.x Equivalent*
 - Check the box labeled *Allow 'unsafe' code* under `Edit > Project Settings > Player > Other Settings`. This allows the networking code to perform some manual memory management using the C# keyword, `unsafe`.

### Documentation
The below documentation goes over the basic features available with NICE-Networking and how to utilize them:

 - [Establishing a Connection](https://github.com/mfitzer/NICE-Networking/blob/master/Documentation/Establishing%20a%20Connection.md)
 - [Key Concepts](https://github.com/mfitzer/NICE-Networking/blob/master/Documentation/Key%20Concepts.md)
 - [Networking Movement](https://github.com/mfitzer/NICE-Networking/blob/master/Documentation/Networking%20Movement.md)
 - [Networking Avatars](https://github.com/mfitzer/NICE-Networking/blob/master/Documentation/Networking%20Avatars.md)
 - [Custom Networked Data - SyncVars](https://github.com/mfitzer/NICE-Networking/blob/master/Documentation/Custom%20Networked%20Data%20-%20SyncVars.md)
 - [Networking Events](https://github.com/mfitzer/NICE-Networking/blob/master/Documentation/Networking%20Events.md)

## License
This project is licensed under the MIT License - see the  [LICENSE.md](https://github.com/mfitzer/NICE-Networking/blob/master/LICENSE)  file for details