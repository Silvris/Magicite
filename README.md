# Magicite
A BepInEx plugin for the FINAL FANTASY PIXEL REMASTERS that allows for the loading of new assets and replacement of existing assets

# Installation:
1. Install an IL2CPP Bleeding-Edge build of BepInEx 6, which can be found [here](https://builds.bepis.io/projects/bepinex_be) or used from a prior Memoria installation
2. Drop the BepInEx folder from the mod into the game's main directory and merge folders if prompted
3. Run the game to generate the configuration file, create the import directory if it does not already exist, and then place any further mods to designated path in the config file.

# Creating Mods for Magicite:
Each file within the Pixel Remasters is given a "group" entry and then a full path to the file. 
Magicite reads each folder within the mod directory as it's own group, and reads json files within the "keys" directory within each folder. 
These define the files that Magicite should manage and load. 
Each mod should define an upper-most folder that is descriptive to the contents of the mod, and contain the groups of the mod within.
To edit files that are present within the game by default, use the same group name as the original file (this can be found in the AssetsPath.json present in the asset bundles), as well as the same key. 
Using the same path is recommended, but not necessarily required.

Each json has two major values: "keys" and "values". 
Keys are the given name of each file, normally just the name of the file without path or extension. 
The value of a key is the full path to the file within the group folder.

Magicite supports the following file types:
* .txt - TextAsset\*
* .csv - TextAsset\*
* .json - TextAsset
* .bytes - TextAsset\*\*
* .png - Texture2D/Sprite\*\*\*
* .atlas - SpriteAtlas\*\*\*\*

\*.txt and .csv are loaded as [PartialAssets](#Partial-Assets), which will apply on top of the default game files stored in the asset bundles.

\*\*The .bytes extension signifies a binary file that is to be loaded as a TextAsset. This is mainly used for the Pixel Remaster's music/sfx.

\*\*\*In order to load a .png as a Sprite, an accompanying .spritedata must be present within the folder. [SpriteData information can be found here.](#SpriteData)
Without a .spritedata, the .png will be loaded as a Texture2D.

\*\*\*\*The .atlas is a text file that has the names of the Sprites it should pack within it as well as the relative path to the .spritedata from the mod folder root. These are separated by a semicolon (;), and each line defines a different sprite to pack within the atlas. These do not need to all use the same image, although that can be useful for simplifying a mod.


# SpriteData:
SpriteData attributes can be accessed by creating a .txt file, filling in the wanted parameters, and then renaming the file extension to .spriteData.

SpriteData exposes the following parameters:
* TextureOverride = a given path to a .png within the mod directory, to be used to generate the sprite. This is mainly used in conjunction with SpriteAtlases.
* Rect = the rectangle that "crops" the base Texture2D into a Sprite - format is [0,0,0,0]
* Pivot = the pivot point (in pixels) of the Sprite - format is [0,0]
* PixelsPerUnit = the number of pixels per unit to use for the sprite - note that the PRs will maintain a consistent pixel size, so PPU less than 1 will be scaled down (2 pixels to 1).
* Border = the border to be used in [9-slice scaling](https://docs.unity3d.com/Manual/9SliceSprites.html) - format is [0,0,0,0]
* WrapMode = Texture2D parameter that defines wrapping on the texture - options are Clamp, Repeat, Mirror, MirrorOnce

# Partial Assets

Certain types of assets are able to be loaded as partial assets, which will apply themselves on top of pre-existing files within the game's directory. This helps increase compatibility between mods that use certain common files (such as system_en.txt).

A partial asset can be created by reducing the file to only the field row (for .csvs, .txt does not have one) and any further edited lines.

Do note that for collisions within partial assets, the game will use whatever file is loaded last to decide each parameter.

# Examples
A set of [example mods](https://drive.google.com/drive/folders/10SN6KzV-_SJOqW6tCNYx6uenyI4K0saY?usp=sharing) for FINAL FANTASY III/V using Magicite. They can be used for understanding the structure needed for a mod.

NOTE: the FF3 DS sprites in the example were created by M3CH4 N1NJ4 on Discord. These will eventually be included in a proper set of FF3 DS sprites, when time allows for it.

# Export

Magicite can export the assets from a Pixel Remaster in the format it expects to receive assets within mods. To do this, run the game at least once with Magicite installed so the configuration file can be generated, and then open the configuration file (located at <game folder>/BepInEx/config/silvris.magicite.cfg). In the configuration file, set Export Enabled to true, and then run the game. After export is successfully completed, Magicite will automatically disable export.
  
Note: By default, Magicite does not export to the same folder as it imports, as the folder structure is incorrect for importing. It is not recommended to load the entire game's assets through Magicite, as it adds unnecessary processing load onto the game. Exporting is completely optional and only for individuals who want to create mods for Magicite or to extract the game files for other uses.
  
**WARNING: For Final Fantasy 1, 2, 3, and 4, loading a map directly from Magicite export will cause the game to hang. This is because these games include unnecessary sprite information for their maps that Magicite will try to load from. After exporting, it is recommended to remove the .spritedata from all map files. Final Fantasy 5 and 6 do not exhibit this issue, as they do not contain the unnecessary sprite information.**
  
# To-do List
* Implement GameObject serialization
