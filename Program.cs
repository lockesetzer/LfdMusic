using Idmr.Common;
using Idmr.LfdReader;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

// define generic error message
String errorMessage = @"

USAGE:
lfdmusic <command> <target>

Commands:
read - Opens LFD file and displays contents
write - Creates LFD file based on contents of directory
help - Displays information about this program";

// define disclaimer
String disclaimer = @"
THIS PROGRAM IS NOT MADE, DISTRIBUTED, OR SUPPORTED BY
LUCASARTS ENTERTAINMENT COMPANY. ELEMENTS TM & (c) LUCASARTS
ENTERTAINMENT COMPANY.

";

// define help message
String helpMessage = disclaimer + @"
LFDMusic is a small utility for comiling Creative Labs VOC files into an LFD 
for use in LucasArts's TIE Fighter game, which utilizes audio files compiled 
within a single LFD file.

For creating a new LFD collection of audio files, use the following command:

lfdmusic write <target>

where <target> is the file path to the directory where all desired audio files
are stored. The LFD file will be named after the directory.

For vieiwng a list of all audio files stored within an LFD file, use the
following command:

lfdmusic read <target>

where <target> is the file path to the LFD file you wish to review.

NOTE:
LFDMusic will not check to verify your files are in the appropriate file format,
that they have been recorded at the correct sample rate, or any other sort 
of check that would validate they will function in within your target platform.

LFDMusic DOES expect files will follow the prescribed naming convention:
- Radio messages end with 'r' followed by a number (1,2,3...) (ex: 1m1r1)
- Win messages end with 'w' followed by a number (1,2) (ex: 1m1w1)
- Lose message end with 'l1' (ex: 1m1l1)

Only files matching the expected naming convention will be loaded

LFDMusic also expects the following:
- All files in the specified folder are Createive Labs VOC files
- All files are named as they should be listed in the LFD file
- All files match the above prescribed naming convention
- All filenames have eight or less characters

Any violation of the above may result in errors (caught or incaught),
and potentially unexpected issues with attempted use of the generated LFD.
";

// if no arguments passed, show generic error
if (args.Length == 0)
{
    Console.WriteLine("ERROR - No arguments passed!" + errorMessage);
    return;
}

// if too many arguments passed, show generic error
else if (args.Length > 2)
{
    Console.WriteLine("ERROR - Too many arguments passed!" + errorMessage);
    return;
}

// if read argument passed, but target not specified, show error
else if ((args[0].ToLower() == "read" || args[0].ToLower() == "write") && args.Length == 1)
{
    Console.WriteLine("ERROR - Missing target argument for command!" + errorMessage);
    return;
}

// if help argument passed, display help message
else if (args[0].ToLower() == "help")
{
    Console.WriteLine(helpMessage);
    return;
}

// if valid read argument passed 
else if (args[0].ToLower() == "read")
{
    Console.WriteLine(disclaimer);

    int i = 0;

    // load target as LFD file
    LfdFile lfd = new LfdFile(args[1]);

    // identify number of objects within LFD
    Console.WriteLine(Path.GetFileName(args[1]) + " contains " + lfd.Resources.Count() + " objects.");

    // for each LFD audio file, print information about file
    foreach (Idmr.LfdReader.Blas r in lfd.Resources)
    {
        Console.WriteLine("===================");
        Console.WriteLine("Object #" + (i+1));
        Console.WriteLine("Name: " + r.Name);
        Console.WriteLine("Class Type: " + r.GetType());
        Console.WriteLine("Object Type: " + r.Type);
        Console.WriteLine("Offset: " + r.Offset);
        Console.WriteLine("Length: " + r.Length);
        Console.WriteLine("Duration: " + r.Duration);
        Console.WriteLine("Frequency: " + r.Frequency);
        i++;
    }
    Console.WriteLine("===================");
    Console.WriteLine("Done");
}

// if valid write argument passed 
else if (args[0].ToLower() == "write")
{

    Console.WriteLine(disclaimer);

    // get all in directory. assuming all files are VOCs
    String[] newVOCs = Directory.GetFiles(args[1]);

    // get name of directory. we'll use this for naming the LFD
    string dirName = new DirectoryInfo(args[1]).Name;

    // create a new normal LFD file
    LfdFile lfd = new LfdFile(LfdFile.LfdCategory.Normal);

    // generate LFD
    lfd.Write();

    // generate header for VOIC
    char[] tb = new char[4];

    tb[0] = 'V';
    tb[1] = 'O';
    tb[2] = 'I';
    tb[3] = 'C';

    // convert to byte array
    byte[] tbBytes = tb.Select(t => (byte)t).ToArray();

    // look for radio messages
    Console.WriteLine("Looking for radio messages...");
   
    for (int i = 1; i <= 16; i++)
    {
        // find next radio object
        var filename = newVOCs.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).EndsWith("r" + i));

        if (filename != null)
        {

            var name = Path.GetFileNameWithoutExtension(filename);

            // initiate character array
            char[] cc = new char[8];

            // if file name length is greater than eight characters, throw error
            if (name.Length > 8)
            {
                Console.WriteLine("Invalid filename: " + name + "; max size is 8 characters");
                return;
            }

            // otherwise
            else
            {
                // pull characters from file name into char array
                for (int n = 0; n < name.Length; n++)
                {
                    cc[n] = name[n];
                }

                // conver char array with file name to byte array
                byte[] ccBytes = cc.Select(c => (byte)c).ToArray();

                // load VOC file
                var bytes = File.ReadAllBytes(filename);

                // get length of file
                int length = bytes.Length;

                // generate byte array with value of file length
                byte[] intBytes = BitConverter.GetBytes(length);

                // create byte array containing header bytes and file bytes
                byte[] finalBytes = tbBytes.Concat(ccBytes).ToArray().Concat(intBytes).ToArray().Concat(bytes).ToArray();

                // write to temporary file
                File.WriteAllBytes("temp", finalBytes);

                // load temp file as VOC object
                Blas vocBlas = new Blas("temp", 0);

                // add to lfd
                lfd.Resources.Add(vocBlas);

                // update lfd
                lfd.Write();

                Console.WriteLine(name + " processed...");
            }
        }
        
        else
        {
            Console.WriteLine("No more radio messages found.");
            break;
        }

    }

    // look for win messages
    Console.WriteLine("Looking for win messages...");
    // look for radio messages
    for (int i = 1; i <= 2; i++)
    {
        // find next win object
        var filename = newVOCs.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).EndsWith("w" + i));

        if (filename != null)
        {

            var name = Path.GetFileNameWithoutExtension(filename);

            // initiate character array
            char[] cc = new char[8];

            // if file name length is greater than eight characters, throw error
            if (name.Length > 8)
            {
                Console.WriteLine("Invalid filename: " + name + "; max size is 8 characters");
                return;
            }

            // otherwise
            else
            {
                // pull characters from file name into char array
                for (int n = 0; n < name.Length; n++)
                {
                    cc[n] = name[n];
                }

                // conver char array with file name to byte array
                byte[] ccBytes = cc.Select(c => (byte)c).ToArray();

                // load VOC file
                var bytes = File.ReadAllBytes(filename);

                // get length of file
                int length = bytes.Length;

                // generate byte array with value of file length
                byte[] intBytes = BitConverter.GetBytes(length);

                // create byte array containing header bytes and file bytes
                byte[] finalBytes = tbBytes.Concat(ccBytes).ToArray().Concat(intBytes).ToArray().Concat(bytes).ToArray();

                // write to temporary file
                File.WriteAllBytes("temp", finalBytes);

                // load temp file as VOC object
                Blas vocBlas = new Blas("temp", 0);

                // add to lfd
                lfd.Resources.Add(vocBlas);

                // update lfd
                lfd.Write();

                Console.WriteLine(name + " processed...");
            }
        }

        else
        {
            Console.WriteLine("No more win messages found.");
            break;
        }

    }

    // look for lose messages
    Console.WriteLine("Looking for lose messages...");
    // look for radio messages
    for (int i = 1; i <= 1; i++)
    {
        // find next lose object
        var filename = newVOCs.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).EndsWith("l" + i));

        if (filename != null)
        {

            var name = Path.GetFileNameWithoutExtension(filename);

            // initiate character array
            char[] cc = new char[8];

            // if file name length is greater than eight characters, throw error
            if (name.Length > 8)
            {
                Console.WriteLine("Invalid filename: " + name + "; max size is 8 characters");
                return;
            }

            // otherwise
            else
            {
                // pull characters from file name into char array
                for (int n = 0; n < name.Length; n++)
                {
                    cc[n] = name[n];
                }

                // conver char array with file name to byte array
                byte[] ccBytes = cc.Select(c => (byte)c).ToArray();

                // load VOC file
                var bytes = File.ReadAllBytes(filename);

                // get length of file
                int length = bytes.Length;

                // generate byte array with value of file length
                byte[] intBytes = BitConverter.GetBytes(length);

                // create byte array containing header bytes and file bytes
                byte[] finalBytes = tbBytes.Concat(ccBytes).ToArray().Concat(intBytes).ToArray().Concat(bytes).ToArray();

                // write to temporary file
                File.WriteAllBytes("temp", finalBytes);

                // load temp file as VOC object
                Blas vocBlas = new Blas("temp", 0);

                // add to lfd
                lfd.Resources.Add(vocBlas);

                // update lfd
                lfd.Write();

                Console.WriteLine(name + " processed...");
            }
        }

        else
        {
            Console.WriteLine("No more lose messages found.");
            break;
        }

    }

    // lock LFD
    lfd.Resources.CanEditStructure = false;
    // update LFD
    lfd.Write();
    // update headers to ensure correct object count is stored
    lfd.CreateRmap();
    // update LFD
    lfd.Write();

    // Rename file to match directory name
    System.IO.File.Move("resource.lfd", dirName + ".lfd");

    // delete temp file
    File.Delete("temp");

    Console.WriteLine("Done");

    Console.WriteLine(lfd.Resources.Count() + " objects written to " + dirName + ".lfd");

}

else
{
    Console.WriteLine("ERROR - UNKNOWN" + errorMessage);
    return;

}

return;




