# Magicite
A BepInEx plugin for the FINAL FANTASY PIXEL REMASTERS that allows for the loading of new assets and replacement of existing assets

# To-do List
* Implement some system of multiple keys.json as to not have to deal with manually merging between similar mods
* Implement GameObject serialization

# Installation:
1. Install a Bleeding-Edge build of BepInEx 6, which can be found [here](https://builds.bepis.io/projects/bepinex_be) or used from a prior Memoria installation
2. Drop the BepInEx folder from the mod into the game's main directory and merge folders if prompted
3. Run the game to generate the configuration file, and then place any added mods to designated path in the config file.

# Creating Mods for Magicite:
Each file within the Pixel Remasters is given a "group" entry and then a full path to the file. Magicite reads each folder in the chosen directory as it's own group, and reads
keys.json file within each folder. These define the files that Magicite should manage and load. To edit files that are present within the game by default, use the same group name as 
the original file (this can be found in the AssetsPath.json present in the asset bundles), as well as the same key. Using the same path is recommended, but not necessarily required.

Each keys.json has two major values: "keys" and "values". Keys are the given name of each file, normally just the name of the file without path or extension. The value of a key

Magicite supports the following file types:
* .txt - TextAsset
* .csv - TextAsset
* .json - TextAsset
* .bytes - TextAsset*
* .png - Texture2D/Sprite**
* .atlas - SpriteAtlas***

*The .bytes extension signifies a binary file that is to be loaded as a TextAsset. This is mainly used for the Pixel Remaster's music.

**In order to load a .png as a Sprite, an accompanying .spritedata must be present within the folder. [SpriteData information can be found here.](#SpriteData).
Without a .spritedata, the .png will be loaded as a Texture2D.

***The .atlas is a text file that has the names of the Sprites it should pack within it (each separated by Enter/Return). Each Sprite *must* have a .spritedata accompanying.

# SpriteData:
SpriteData attributes can be accessed by creating a .txt file, filling in the wanted parameters, and then renaming the file extension to .spriteData.

SpriteData exposes the following parameters:
* Rect = the rectangle that "crops" the base Texture2D into a Sprite - format is [0,0,0,0]
* Pivot = the pivot point (in pixels) of the Sprite - format is [0,0]
* PixelsPerUnit = the number of pixels per unit to use for the sprite - note that the PRs will maintain a consistent pixel size, so PPU less than 1 will be scaled down (2 pixels to 1).
* Border = the border to be used in [9-slice scaling](https://docs.unity3d.com/Manual/9SliceSprites.html) - format is [0,0,0,0]
* WrapMode = Texture2D parameter that defines wrapping on the texture - options are Clamp, Repeat, Mirror, MirrorOnce

# Examples
A set of [example mods](https://drive.google.com/file/d/1m9FVivUR1uHkpvxd7lV8dK3MJcR89dpi/view?usp=sharing) for FINAL FANTASY III using Magicite. They can be used for understanding 
the structure needed for a mod.

NOTE: the FF3 DS sprites in the example were created by M3CH4 N1NJ4 on Discord. These will eventually be included in a proper set of FF3 DS sprites, when time allows for it.
