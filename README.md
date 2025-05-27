## Google Takeout Merge tool
This is a tool that allows you to merge your exif data (GPS/ datetime created and so on) fomr Takeout .json files into your photos. It converts .pngs into .tiffs without compressiong so if you have a lot of pngs you might want to switch to a compression algorithm (in code, there are methods ready just didnt bother putting in options)
JPG/JPEG and TIFF are being merged with their json files and the rest of the usual takeout files except for .json are being copied to the dest folder. It also flattens the structure to make it easier to upload.

### Disclaimer
This is not by any means an efficient implementation, nor is it clean. I might work on it when I have time but the 10h I spent researching differences between file types, writing to file metadata and so on has got me good, so I am taking a break. Happy to answer any questions on this.
