# Pixi

Gamecraft mod for converting images into coloured blocks.
Think of it like automatic pixel art.

## Installation

To install the Pixi mod, copy the build's `Pixi.dll` into the `Plugins` folder in Gamecraft's main folder.

## Development

Show your love by offering your time.

### Setup

Pixi's development environment is similar to most Gamecraft mods, since it's based on HelloModdingWorld's configuration.

This project requires most of Gamecraft's `.dll` files to function correctly. 
Most, but not all, of these files are stored in Gamecraft's `Gamecraft_Data\Managed` folder. 
The project is pre-configured to look in a folder called ref in the solution's main directory or one level up from that. 

You can make sure Pixi can find all of `.dll` files it needs by copying your Gamecraft folder here and renaming it to `ref`, but you'll have to re-copy it after every Gamecraft update. 
You can also create a symbolic link (look it up) to your Gamecraft install folder named `ref` in this folder to avoid having to re-copy files. 

Like most mods, you will have to patch your game with [GCIPA](https://git.exmods.org/modtainers/GCIPA). 
Pixi also requires the [GamecraftModdingAPI](https://git.exmods.org/modtainers/GamecraftModdingAPI) library to be installed (in `ref/Plugins/GamecraftModdingAPI.dll`). 

## Building

After you've completed the setup, open the solution file `Pixi.sln` in your prefered C# .NET/Mono development environment. 
I'd recommend Visual Studio Community Edition or JetBrains Rider for Windows and Monodevelop for Linux. 

If you've successfully completed setup, you should be able to build the Pixi project without errors. 
If it doesn't work and you can't figure out why, ask for help on [our Discord server](https://discord.gg/xjnFxQV). 
