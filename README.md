# HelloModdingWorld

Shell project for Gamecraft mods.
Use this as a quick-start project structure for your own mods, or to learn how modding works.

## Setup

This project requires most of Gamecraft's `.dll` files to function correctly. 
Most, but not all, of these files are stored in Gamecraft's `Gamecraft_Data\Managed` folder. 
The project is pre-configured to look in a folder called ref in the solution's main directory or one level up from that. 

You can make sure HelloModdingWorld can find all of `.dll` files it needs by copying your Gamecraft folder here and renaming it to `ref`, but you'll have to re-copy it after every Gamecraft update. 
You can also create a symbolic link (look it up) to your Gamecraft install folder named `ref` in this folder to avoid having to re-copy files. 

For any mod to work, you will have to patch your game with [GCIPA](https://git.exmods.org/modtainers/GCIPA). 
[Direct link to install guide](https://git.exmods.org/modtainers/GCIPA/src/branch/master/README.md#how-to-install). 
!!Download a release from git.exmods.org not github.com!! 

This project also requires the [GamecraftModdingAPI](https://git.exmods.org/modtainers/GamecraftModdingAPI) library to be installed (in `ref/Plugins/GamecraftModdingAPI.dll`). 
[Direct link to install guide](https://www.exmods.org/guides/install.html).

## Building

After you've completed the setup, open the solution file `HelloModdingWorld.sln` in your prefered C# .NET/Mono development environment. 
I'd recommend Visual Studio Community Edition or JetBrains Rider for Windows and Monodevelop for Linux. 

If you've successfully completed setup, you should be able to build the HelloModdingWorld project without errors. 
If it doesn't work and you can't figure out why, ask for help on [our Discord server](https://discord.gg/xjnFxQV). 

## Installation

To install the HelloModdingWorld mod, copy the build's `HelloModdingWorld.dll` into the `Plugins` folder in Gamecraft's main folder.
