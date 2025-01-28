using System.IO;
namespace ETL_Lib.Handlers;
//using Alphaleonis.Win32.Filesystem;
using System.Security.Permissions;
using System.Text;

public static class FileHandler
{
    /// <summary>
    /// Reads from a given path and returns a byte array.
    /// </summary>
    /// <param name="path">String path where the file to be read is located.</param>
    /// <returns>Byte array with contents of file.</returns>
    public static byte[] ReadFrom(string path)
    {
        byte[] bytes = new byte[0];

        try
        {
            using (FileStream fileStream = new FileStream(path,
                        FileMode.Open, FileAccess.Read))
            {
                // Read the source file into a byte array.
                bytes = new byte[fileStream.Length];
                int numberOfBytesToRead = (int)fileStream.Length;

                // Return if File is empty
                if (numberOfBytesToRead == 0)
                {
                    Console.WriteLine("File at path: " + path + " is empty.");
                    return bytes;
                }

                int numberOfBytesRead = 0;
                while (numberOfBytesToRead > 0)
                {
                    // Read may return anything from 0 to numBytesToRead.
                    int n = fileStream.Read(bytes, numberOfBytesRead, numberOfBytesToRead);

                    // Break when the end of the file is reached.
                    if (n == 0)
                        break;

                    numberOfBytesRead += n;
                    numberOfBytesToRead -= n;
                }

            }
        }
        catch (FileNotFoundException ioEx)
        {
            Console.WriteLine("File was not found at path "
            + path + ".\nError: " + ioEx.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error occured when opening file from path "
            + path + ".\nError: " + e.Message);
        }

        return bytes;
    }

    /// <summary>
    /// Writes a file to the provided path location with the content provided.
    /// </summary>
    /// <param name="path">Path location where file to where is to be written, must include the file and extension.</param>
    /// <param name="content">Content to be written to file.</param>
    public static void WriteTo(string path, string content)
    {
        if (path != string.Empty)
        {
            File.WriteAllText(path, content);
        }
        else
        {
            Console.WriteLine("Path was empty string. Please pass properpath.");
        }
    }

    /// <summary>
    /// Reads a file with a given encoding and returns string.
    /// </summary>
    /// <param name="path">String path where the file to be read is located.</param>
    /// <param name="encoding">System.Text.Encoding Class appropriete for reading the file.</param>
    /// <returns>String with contents of file.</returns>
    public static string ToString(string path, Encoding encoding)
    {
        byte[] bytes = ReadFrom(path);

        return encoding.GetString(bytes);
    }

    /// <summary>
    /// Reads a file and converts it from a specified encoding to another.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="srcEncoding"></param>
    /// <param name="dstEncoding"></param>
    /// <returns></returns>
    public static string ConvertEncodingToString(string path, Encoding srcEncoding, Encoding dstEncoding)
    {
        byte[] bytes = ReadFrom(path);

        bytes = Encoding.Convert(srcEncoding, dstEncoding, bytes);

        return dstEncoding.GetString(bytes);
    }

    /// <summary>
    /// Delegate used to apply an action to each file encounterd by a recursive file search.
    /// </summary>
    /// <param name="fileInfo">FileInfo object with information about the file. Filled in by traversal method.</param>
    /// <returns>Boolean for whether or not file action was successful.</returns>
    public delegate bool FileAction(FileInfo fileInfo);

    /// <summary>
    /// Recursively Traverses Directory provided and applies FileAction to each file encounterd.
    /// </summary>
    /// <param name="directoryPath">Starting Directory for recursive search</param>
    /// <param name="action">File Action Delegate method to be executed for each file encountered.</param>
    public static void RecursiveFileSearch(string directoryPath, FileAction action, bool suppressErrors = false)
    {
        //Find directory

        //For each element in directory fire FileAction delegate or kickoff next recursion.
        DirectoryInfo root = new DirectoryInfo(directoryPath);
        FileInfo[] files;
        DirectoryInfo[] subDirs;

        //Find Files and Run FileAction for each
        try
        {
            files = root.GetFiles("*.*");
            if (files != null)
            {
                foreach (FileInfo fi in files)
                {
                    action(fi);
                }
            }

            //Now find all the subdirectories under this directory.
            subDirs = root.GetDirectories();

            foreach (DirectoryInfo dirInfo in subDirs)
            {
                try
                {
                    //Resursive call for each subdirectory.
                    RecursiveFileSearch(dirInfo.FullName, action);
                }
                catch (FileNotFoundException e)
                {
                    if (!suppressErrors)
                        Console.WriteLine(e.Message);
                }

            }
        }
        catch (UnauthorizedAccessException e)
        {
            if (!suppressErrors)
                Console.WriteLine(e.Message);
        }
        catch (DirectoryNotFoundException e)
        {
            if (!suppressErrors)
                Console.WriteLine(e.Message);
        }
        catch (Exception e)
        {
            if (!suppressErrors)
                Console.WriteLine(e.Message);
        }
    }

    ///<summary>Delegate used to apply an action to each folder encounterd by a recursive folder search.
    ///If result returns false the search ends for that branch.</summary>
    public delegate bool FolderAction(DirectoryInfo fileInfo);

    ///<summary>Recursively Traverses Directory provided and applies FileAction to each file encounterd.</summary>
    public static void RecursiveFolderSearch(string directoryPath, FolderAction action)
    {
        //Find directory

        //For each element in directory fire FileAction delegate or kickoff next recursion.
        DirectoryInfo root = new DirectoryInfo(directoryPath);
        DirectoryInfo[] subDirs;

        //Now find all the subdirectories under this directory.
        subDirs = root.GetDirectories();

        //Traverse and check result for each
        bool actionResult = false;
        foreach (DirectoryInfo dirInfo in subDirs)
        {
            //Apply Folder Action
            actionResult = action(dirInfo);

            //Resursive call for each subdirectory if action result is positive.
            if (actionResult)
                RecursiveFolderSearch(dirInfo.FullName, action);
        }
    }

    ///<summary>Recursively Traverses Directory provided and applies FileAction to each file encounterd.</summary>
    public static void FlatFolderSearch(string directoryPath, FolderAction action)
    {
        //Find directory

        //For each element in directory fire FileAction delegate or kickoff next recursion.
        DirectoryInfo root = new DirectoryInfo(directoryPath);
        DirectoryInfo[] subDirs;

        //Now find all the subdirectories under this directory.
        subDirs = root.GetDirectories();

        //Traverse and check result for each
        bool actionResult = false;
        foreach (DirectoryInfo dirInfo in subDirs)
        {
            //Apply Folder Action
            actionResult = action(dirInfo);

            //End search if negative.
            if (!actionResult)
                break;
        }
    }

}