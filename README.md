This project is a networking library built on top of Unity's new [Real-time Multiplayer](https://github.com/Unity-Technologies/multiplayer) solution. Specifically, this project is built on top of the [Workflow: Creating a minimal client and server](https://github.com/Unity-Technologies/multiplayer/blob/master/com.unity.transport/Documentation~/workflow-client-server.md) sample from Unity. It aims to make it easy to incorporate local multiplayer functionality into projects by replicating some features from Unity's old networking solution such as NetworkTransforms and SyncVars.

**Note:** The development of the NICE Networking library is currently halted due to the virtual reality project, [MN NICE](https://www.facebook.com/watch/?v=742787122902502), being halted as well. There will be no more development on this at this time.

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
