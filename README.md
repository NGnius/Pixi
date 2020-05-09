# Pixi

Gamecraft mod for converting images into coloured blocks.
Think of it like automatic pixel art.

## Installation

To install the Pixi mod, copy the build's `Pixi.dll` into the `Plugins` folder in Gamecraft's main folder.

## Usage

Pixi adds new commands to Gamecraft's command line to import images into a game. 

`PixiScale [width] [height]` sets the block canvas size (usually you'll want this to be the same size as your image). 
When conversion using `Pixi2D` is done, if the canvas is larger than your image the image will be repeated. 
If the canvas is smaller than your image, the image will be cropped based to the lower left corner.

`Pixi2D "[image]"` converts an image to blocks and places it as blocks beside where you're standing (along the xy-plane). 
If your image is not stored in the same folder as Gamecraft, you should specify the full filepath (eg `C:\path\to\image.png`) to the image.

For example, if you want to add an image called `pixel_art.png`, 
with a resolution of 1920x1080, stored in Gamecraft's installation directory, 
execute the command `PixiScale 1920 1080` to set the size and then `Pixi2D "pixel_art.png` to load the image.

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
