# Pixi

Gamecraft mod for converting images into coloured blocks.
Think of it like automatic pixel art.

## Installation

To install the Pixi mod, copy the build's `Pixi.dll` into the `Plugins` folder in Gamecraft's main folder.

## Usage

Pixi adds new commands to Gamecraft's command line to import images into a game. 
Since Pixi places vanilla Gamecraft blocks, imported images should be visible without Pixi installed.

### Commands

`PixiScale [width] [height]` sets the block canvas size (usually you'll want this to be the same size as your image). 
When conversion using `Pixi2D` is done, if the canvas is larger than your image the image will be repeated. 
If the canvas is smaller than your image, the image will be cropped. 

`Pixi2D "[image]"` converts an image to blocks and places it beside where you're standing (along the xy-plane). 
If your image is not stored in the same folder as Gamecraft, you should specify the full filepath (eg `C:\path\to\image.png`) to the image. 
This works best with `.PNG` images, but `.JPG` also works -- you just won't be able to use transparency-based features. 

For example, if you want to add an image called `pixel_art.png`, 
with a resolution of 1920x1080, stored in Gamecraft's installation directory, 
execute the command `PixiScale 1920 1080` to set the size and then `Pixi2D "pixel_art.png"` to load the image. 

### Behaviour

Pixi takes an image file and converts every pixel to a coloured block. 
Unfortunately, an image file supports over 6 million colours and Gamecraft only has 100 paint colours (and only 90 are used by Pixi). 
Pixi uses an algorithm to convert each pixel an image into the closest paint colour, but colour accuracy will never be as good as a regular image.

Pixi's colour-conversion algorithm also uses pixel transparency to you can cut out shapes. 
A pixel which has opacity of less than 75% will be not be converted into a solid block. 
A pixel which has an opacity between 75% and 50% will be converted into a glass cube. 
A pixel which has an opacity greater than 75% will be converted into an aluminium cube. 
This only works with `.PNG` image files since the `.JPG` format doesn't store transparency. 

Pixi also groups blocks together, since images have a lot of pixels. 
After the colour-conversion algorithm, Pixi groups blocks in the same column with the same paint colour together. 
The grouping algorithm reduces the block count by over 75% in ideal cases, and it can reduce the block count by 50% in most cases. 
Imagine a standard 1080p screen (1920x1080 pixels), which has more than 2 million pixels. 
Pixi could import that image with less than 500K blocks, which will still hurt Gamecraft's performance even on good PCs but it won't make it completely unusable like 2M blocks will. 

## Suggestions and Bugs

If you find a bug or have an idea for an improvement to Pixi, please create an [issue](https://git.exmods.org/NGnius/Pixi/issues) with an in-depth description. 
If you'd like to discuss your issue instead, talk to NGnius on the [Exmods Discord server](https://discord.gg/xjnFxQV).

## Development

Show your love by offering your help!

### Setup

Pixi's development environment is similar to most Gamecraft mods, since it's based on HelloModdingWorld's configuration.

This project requires most of Gamecraft's `.dll` files to function correctly. 
Most, but not all, of these files are stored in Gamecraft's `Gamecraft_Data\Managed` folder. 
The project is pre-configured to look in a folder called ref in the solution's main directory or one level up from that. 

You can make sure Pixi can find all `.dll` files it needs by copying your Gamecraft folder here and renaming it to `ref`, but you'll have to re-copy it after every Gamecraft update. 
To avoid that, create a symbolic link (look it up) to your Gamecraft install folder named `ref` in this folder instead. 

Like most mods, you will have to patch your game with [GCIPA](https://git.exmods.org/modtainers/GCIPA). 
Pixi also requires the [GamecraftModdingAPI](https://git.exmods.org/modtainers/GamecraftModdingAPI) library to be installed (in `ref/Plugins/GamecraftModdingAPI.dll`). 

### Building

After you've completed the setup, open the solution file `Pixi.sln` in your prefered C# .NET/Mono development environment. 
I'd recommend Visual Studio Community Edition or JetBrains Rider for Windows and Monodevelop for Linux. 

If you've successfully completed setup, you should be able to build the Pixi project without errors. 
If it doesn't work and you can't figure out why, ask for help on the [Exmods Discord server](https://discord.gg/xjnFxQV). 
