This Project has been abandoned due to discovery of a much nicer project suited for my needs- [immich-go](https://github.com/simulot/immich-go). It should still work as is but is not going to be maintained anymore.

## Google Takeout Merge tool
This is a tool that allows you to merge your exif data (GPS/ datetime created and so on) fomr Takeout .json files into your photos. It converts .pngs into .tiffs without compression so if you have a lot of .pngs you might want to switch to a compression algorithm (in code, there are methods ready, just didn't bother putting in options)
JPG/JPEG and TIFF are being merged with their json files, and the rest of the usual takeout files, except for .json are being copied to the dest folder. It also flattens the structure to make it easier to upload.

## How does it work
Firstly, the tool will rename all the .json files and remove all the `supplemental-metadata.json` bits from the name, doing so by matching the name with a regular expression.
Then the tool will convert all .png files into uncompressed .tiff files, increasing their size, and apply the matched JSON metadata to those .tiff files, saving them in the output location.
After that, the tool will match all the Tag type photos (TIFF, JPG/JPEG) and apply all the matched json metadata to them.
It will save all the photos in the destination directory and leave the original folder untouched. 
Formats: .gif, .mp4, .mpeg, .dng and so on are not supported and are going to be copied 'as is'. It is not a problem normally because all the metadata should already be present on your files.

## Building/Running
To run this you need to:
- unzip Google Takeout into a folder
- run the tool using `.\TakeoutMerger "C:\Workspace\Takeout\Testing" "C:\Workspace\Takeout\TestingOutput"` as an example

### Known Bugs
- 

### To do
- unit tests for public methods
- options for different compressions/skipping png files (copying them as is with metadata loss) and/or adding and option to copy png files as is along their new tiff equivalents
- replacing the  System.Drawing dependency with something cross platform (like ImageSharp) due to the usage of Windows GDI+ which is not cross platform
- improve performance
- improve json matching accuracy
