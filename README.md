# Pixi

A mod for importing images and more into Gamecraft. 

Developed by NGnius.

## Installation

Before installing Pixi, please patch Gamecraft with [GCIPA](https://git.exmods.org/modtainers/GCIPA/releases) and install the latest version of [GamecraftModdingAPI](https://git.exmods.org/modtainers/GamecraftModdingAPI/releases). 

Once that's done, install Pixi by copying `Pixi.dll` (from the latest release) into the `Plugins` folder in Gamecraft's main folder. 
Alternately, follow the install guide: https://www.exmods.org/guides/install.html (ignore the part about a zip file -- move Pixi.dll into the Plugins folder instead). 

## Usage

Pixi adds new commands to Gamecraft's command line to import images, and other stuff, into a game. 
Since Pixi places vanilla Gamecraft blocks, imported files should be visible without Pixi installed. 

For the following section, anything between `[` and `]` characters is a command argument you must provide by replacing everything inside and including the square brackets. 
An argument like `[dog name]` is an argument named "dog name" and could be a value like `Clifford` or `doggo`, 
and `@"[dog name]"` could be a value like `@"Clifford"` or `@"doggo"`. 

### Commands

`Pixi @"[thing]"` to import `thing` into your Gamecraft game.

For example, if you want to add an image called `pixel_art.png`, stored in Gamecraft's installation directory, 
execute the command `Pixi @"pixel_art.png"` to load the image as blocks. 
It's important to include the file extension, since that's what makes Pixi's magic work. 

If you know the name of the Pixi spell you want, you can also use

`Pixi2 "[spell]" @"[thing]"` to use `spell` to import `thing` into your Gamecraft game.

Some commands also have hidden features, like image rotation and bot scaling. 
Talk to NGnius on the Exmods Discord server or read Pixi's source code to figure that out.

### Behaviour

ImageText and ImageConsole share the same image conversion system. 
The conversion system converts every pixel to a [color tag](http://digitalnativestudios.com/textmeshpro/docs/rich-text/#color) followed by a square text character. 
For ImageText, the resulting character string is set to the text field of the text block that the command places. 
For ImageConsole, the character string is automatically set to a console block in the form `ChangeTextBlockCommand [text block id] [character string]`. 
Due to limitations in Gamecraft, larger images will crash your game!!! 

ImageCanvas takes an image file and converts every pixel to a coloured block. 
ImageCanvas uses an algorithm to convert each pixel in an image into the closest paint colour, but colour accuracy will never be as good as a regular image. 

ImageCanvas' colour-conversion algorithm also uses pixel transparency so you can cut out shapes. 
A pixel which has opacity of less than 50% will be ignored. 
A pixel which has an opacity between 75% and 50% will be converted into a glass cube. 
A pixel which has an opacity greater than 75% will be converted into the block you're holding (or aluminium if you've got your hand selected). 
This only works with `.PNG` image files since the `.JPG` format doesn't support image transparency. 

ImageCanvas also optimises block placement, since images have a lot of pixels. 
The blocks grouping ratio is displayed in the command line output once image importing is completed. 

RobocraftRobot converts a robot to equivalent Gamecraft blocks. 
If the conversion algorithm encounters a block it cannot convert, it will place a text block, with the block name, instead. 

## Development

Show your love by offering your help!

### Ways To Contribute

- Build a Robocraft block that's not currently supported by Pixi (send it to NGnius on Discord). 
- Report any bugs that you encounter while using Pixi.
- Report an idea for an improvement to Pixi or for a new file format. 

For questions, concerns, or any other inquiry, please contact NGnius in the [Exmods Discord server](https://discord.exmods.org).

### Setup

Pixi's development environment is similar to most Gamecraft mods, since it's based on HelloModdingWorld's configuration. 

This project requires most of Gamecraft's `.dll` files to function correctly. 
Most, but not all, of these files are stored in Gamecraft's `Gamecraft_Data\Managed` folder. 
The project is pre-configured to look in a folder called ref in the solution's main directory or one level up from that. 

You can make sure Pixi can find all `.dll` files it needs by copying your Gamecraft folder here and renaming it to `ref`, but you'll have to re-copy it after every Gamecraft update. 
To avoid that, create a symbolic link (look it up) to your Gamecraft install folder named `ref` in this folder instead. 

Like most mods, you will have to patch your game with [GCIPA](https://git.exmods.org/modtainers/GCIPA). 
Pixi also requires the [GamecraftModdingAPI](https://git.exmods.org/modtainers/GamecraftModdingAPI) library to be installed (in `ref/Plugins/GamecraftModdingAPI.dll`, the usual place). 

### Building

After you've completed the setup, open the solution file `Pixi.sln` in your prefered C# .NET/Mono development environment. 
I'd recommend Visual Studio Community Edition for Windows or JetBrains Rider for Linux. 

If you've successfully completed setup, you should be able to build the Pixi project without errors. 
If it doesn't work and you can't figure out why, ask for help on the [Exmods Discord server](https://discord.exmods.org). 

# Acknowledgements

RobocraftRobot uses the Factory to download robots, which involves a partial re-implementation of [rcbup](https://github.com/NGnius/rcbup). 
Robot parsing uses information from [RobocraftAssembler](https://github.com/dddontshoot/RoboCraftAssembler). 

Gamecraft interactions use the [GamecraftModdingAPI](https://git.exmods.org/modtainers/GamecraftModdingAPI). 

MIDI file processing uses an integrated copy of melanchall's [DryWetMidi](https://github.com/melanchall/drywetmidi) library, licensed under the [MIT License](https://github.com/melanchall/drywetmidi/blob/develop/LICENSE).

Thanks to **TheGreenGoblin** and their Python app for converting images to coloured square characters, which inspired the PixiConsole and PixiText commands.

Thanks to **Mr. Rotor** for all of the Robocraft blocks used in the PixiBot and PixiBotFile commands.

# Disclaimer

Pixi source code and releases are available free of charge as open-source software for the purpose of modding Gamecraft. 
Modify Gamecraft at your own risk. 
Read the LICENSE file for official licensing information. 
Pixi, Exmods and NGnius are not endorsed or supported by Gamecraft or FreeJam. 
Please don't sue this project or its contributors (that's what all disclaimers boil down to, right?). 

Pixi is actually just sufficiently advanced technology that's indistinguishable from magic. 
