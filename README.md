# LfdMusic

Version 1.0
2024-07-16

## About

LfdMusic is a lightweight command line utility for compiling audio files into a single LFD resource used by LucasArts games such as TIE Fighter. Specifically, this utility is presently designed for compiling Creative Lab VOC files into a single LFD in the format expected by TIE Fighter to load as in-flight audio messages during missions.

Future releases may include support for other LucasArts games that store audio in a similar manner.

## Dependencies

This utility is reliant on Michael Gaisser's Idmr.LfdReader.dll library, located at: https://github.com/MikeG621/LfdReader

## Use

LfdMusic will take a folder of audio files and compile them into a LFD file. Files are expected to be in the correct format (Creative Labs VOC, 11111 Hz sample rate) and follow the expected naming convention (see below)

Command Line Arguments:
lfdmusic \<command\> \<target\>

Commands:
- read - Opens LFD file and displays contents
- write - Creates LFD file based on contents of directory
- help - Displays information about this program;

### Write
For creating a new LFD collection of audio files, use the following command:

lfdmusic write \<target\>

where \<target\> is the file path to the directory where all desired audio files are stored. The LFD file will be named after the directory.

NOTE:
LFDMusic will not check to verify your files are in the appropriate file format, that they have been recorded at the correct sample rate, or any other sort of check that would validate they will function in within your target platform.

LFDMusic DOES expect files will follow the prescribed naming convention:
- Radio messages end with 'r' followed by a number (1,2,3...) (ex: 1m1r1)
- Win messages end with 'w' followed by a number (1,2) (ex: 1m1w1)
- Lose message end with 'l1' (ex: 1m1l1)

Only files matching the expected naming convention will be loaded. This is done to ensure that the files are loaded in the correct order, as TIE Fighter relies on the order of audio within the LFD file, rather than loading explictly named items within the LFD.

LFDMusic also expects the following:
- All files in the specified folder are Createive Labs VOC files
- All files are named as they should be listed in the LFD file
- All files match the above prescribed naming convention
- All filenames have eight or less characters

Any violation of the above may result in errors (caught or incaught), and potentially unexpected issues with attempted use of the generated LFD.

### Read
For vieiwng a list of all audio files stored within an existing LFD file, use the following command:

lfdmusic read \<target\>

where \<target\> is the file path to the LFD file you wish to review.

## References

For more information about custom audio in TIE Fighter, please view the following page:
https://wiki.emperorshammer.org/TIE_Fighter_Custom_Audio

## Version History

v1.0, July 16, 2024
- Initial release. Support for compiling audio into an LFD file for use in TIE Fighter missions.




