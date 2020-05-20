# Pixi

Gamecraft mod for converting images into coloured blocks.
Think of it like automatic pixel art.

## Installation

To install the Pixi mod, copy `Pixi.dll` (from the latest release) into the `Plugins` folder in Gamecraft's main folder. 
You'll also need [GamecraftModdingAPI](https://git.exmods.org/modtainers/GamecraftModdingAPI) installed and Gamecraft patched with [GCIPA](https://git.exmods.org/modtainers/GCIPA/releases). 

## Usage

Pixi adds new commands to Gamecraft's command line to import images into a game. 
Since Pixi places vanilla Gamecraft blocks, imported images should be visible without Pixi installed. 

### Commands

`PixiText @"[image]"` converts an image to text and places a text block with that text beside you. 

`PixiConsole @"[image]" "[text block id]"` converts an image to text and places a console block beside you which changes the specified text block. 

`Pixi2D @"[image]"` converts an image to blocks and places it beside you (along the xy-plane). 

Anything between `[` and `]` characters is a command argument you must provide by replacing everything inside and including the square brackets. 
An argument like `[dog name]` is an argument named "dog name" and could be a value like `Clifford` or `doggo`, 
and `@"[dog name]"` could be a value like `@"Clifford"` or `@"doggo"`. 

For example, if you want to add an image called `pixel_art.png`, stored in Gamecraft's installation directory, 
execute the command `Pixi2D @"pixel_art.png"` to load the image as blocks. 
It's important to include the file extension, since Pixi isn't psychic (yet). 

**EXPERIMENTAL**

`PixiBot @"[bot]"` downloads a bot from Robocraft's community Factory and places it beside you. 

`PixiBotFile @"[bot]"` converts a `.bot` file from [rcbup](https://github.com/NGnius/rcbup) to blocks and places it beside you. 

**NOTE**

For the preceeding commands, do not forget the `@"` before and `"` after the command argument, otherwise the command won't work. 
If your image is not stored in the same folder as Gamecraft, you should specify the full filepath (eg `C:\path\to\image.png`) to the image. 
This works best with `.PNG` images, but `.JPG` also works -- you just won't be able to use transparency-based features. 
Optionally, if you know your command argument won't have a backslash `\` in it, you can omit the `@` symbol. 

`PixiThicc [depth]` sets the block thickness for `Pixi2D` image conversion. 
The depth should be a positive whole number, like 3 or 42, and not 3.14 or -42. 
The default thickness is 1. 

### Behaviour

PixiText and PixiConsole share the same image conversion system. 
The conversion system converts every pixel to a [<color> tag](http://digitalnativestudios.com/textmeshpro/docs/rich-text/#color) followed by a square text character. 
For PixiText, the resulting character string is set to the text field of the text block that the command places. 
For PixiConsole, the character string is automatically set to a console block in the form `ChangeTextBlockCommand [text block id] [character string]`. 

Pixi2D takes an image file and converts every pixel to a coloured block. 
Unfortunately, an image file supports over 6 million colours and Gamecraft only has 100 paint colours (and only 90 are used by Pixi2D). 
Pixi2D uses an algorithm to convert each pixel an image into the closest paint colour, but colour accuracy will never be as good as a regular image. 

Pixi2D's colour-conversion algorithm also uses pixel transparency so you can cut out shapes. 
A pixel which has opacity of less than 50% will be ignored. 
A pixel which has an opacity between 75% and 50% will be converted into a glass cube. 
A pixel which has an opacity greater than 75% will be converted into an aluminium cube. 
This only works with `.PNG` image files since the `.JPG` format doesn't store transparency. 

Pixi2D also groups blocks together, since images have a lot of pixels. 
After the colour-conversion algorithm, Pixi groups blocks in the same column with the same paint colour together. 
The grouping algorithm reduces the block count by over 75% in ideal cases, and it can reduce the block count by 50% in most cases. 
Imagine a standard 1080p screen (1920x1080 pixels), which has more than 2 million pixels. 
Pixi2D could import that image with less than 500K blocks, which will still hurt Gamecraft's performance even on good PCs but it won't make it completely unusable like 2M blocks will. 

PixiBot and PixiBotFile convert robot data to equivalent Gamecraft blocks. 
If the conversion algorithm encounters a block it cannot convert, it will place a text block, with additional information, instead. 
PixiBot uses the Factory to download robots, which involves a partial re-implementation of [rcbup](https://github.com/NGnius/rcbup). 
Robot parsing uses information from [RobocraftAssembler](https://github.com/dddontshoot/RoboCraftAssembler). 

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

# Disclaimer

Pixi, Exmods and NGnius are not endorsed or supported by Gamecraft or FreeJam. 
Modify Gamecraft at your own risk. 
Read the LICENSE file for licensing information. 
Please don't sue this project's contributors (that's what all disclaimers boil down to, right?). 

Pixi is not a psychic overlord which secretly rules the world. 
Well, not this world at least. 
