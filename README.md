## Google Takeout Merge tool
This is a tool that allows you to merge your EXIF data (GPS/ datetime created, and so on) from Takeout .json files into your photos.

How it works: 
- The tool will rename all the .json files and remove all the `supplemental-metadata.json` bits from the name, doing so by matching the name with a few different matching algorithms.
- It converts .pngs into .tiffs without compression, so if you have a lot of .pngs, you might want to switch to a compression algorithm (in code, there are methods ready, just didn't bother putting in options). The reason for it is so we can store EXIF on the PNG photos whilst preserving the alpha channel (transparency)
- Any EXIF supporting files are being merged with their matched JSON file metadata, and the rest of the usual takeout files, except for .json, are being copied to the dest folder.
- Flattens the structure to make it easier to upload back to Google Photos.

This tool is handy if you are trying to upload files from one Google Photos account to another. If you are trying to upload photos from Google Takeout to Immich, I would recommend [immich-go](https://github.com/simulot/immich-go).

## Building/Running
To run this, you need to:
- unzip Google Takeout into a folder
- run the tool using `.\TakeoutMerger "C:\Workspace\Takeout\UnzippedTakeoutData" "C:\Workspace\Takeout\TakeoutOutput"` as an example
