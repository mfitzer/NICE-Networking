using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using UnityEngine;
using System;

public static class ExtensionMethods
{
    /// <summary>
    /// Adds the specified data to the end of the array.
    /// </summary>
    /// <param name="data">Original data.</param>
    /// <param name="newData">New data bytes to be appended.</param>
    /// <returns>New message containing appended newData.</returns>
    public static T[] append<T>(this T[] data, T[] newData)
    {
        List<T> completeData = new List<T>(data);
        completeData.AddRange(newData);
        return completeData.ToArray();
    }

    #region byte[]

    #region Compression & Decompression

    public static byte[] compress(this byte[] bytes)
    {
        // Initiate two streams and copy from one to other, compressing on the way
        using (var memStreamIn = new MemoryStream(bytes))
        using (var memStreamOut = new MemoryStream())
        {
            using (var gs = new GZipStream(memStreamOut, CompressionMode.Compress))
            {
                memStreamIn.copyStream(gs);
            }

            //Debug.Log("<color=red><b>[Compression]</b></color> Compressed byte array from: " + bytes.Length + " to: " + memStreamOut.ToArray().Length);

            return memStreamOut.ToArray();
        }
    }

    public static byte[] decompress(this byte[] bytes)
    {
        // Same mechanism as Compress, just different compression mode which decompresses
        using (var memStreamIn = new MemoryStream(bytes))
        using (var memStreamOut = new MemoryStream())
        {
            using (var gs = new GZipStream(memStreamIn, CompressionMode.Decompress))
            {
                gs.copyStream(memStreamOut);
            }

            //Debug.Log("<color=red><b>[Decompression]</b></color> Decompressed byte array from: " + bytes.Length + " to: " + memStreamOut.ToArray().Length);

            return memStreamOut.ToArray();
        }
    }

    public static void copyStream(this Stream input, Stream output)
    // Helper funtion to copy from one stream to another
    {
        // Magic number is 2^16
        byte[] buffer = new byte[32768];
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, read);
        }
    }

    #endregion Compression & Decompression

    #region Saving & Loading Data

    /// <summary>
    /// Saves byte array to a file with name, fileName.
    /// </summary>
    /// <param name="data">Data to be stored.</param>
    /// <param name="fileName">Name of the file the data will be stored in.</param>
    public static void saveData(this byte[] data, string fileName)
    {
        //Specify file path to save data at
        string path = Path.Combine(Application.persistentDataPath, fileName);

        FileStream stream;

        try
        {
            //Write the data to a file at path
            stream = new FileStream(path, FileMode.Create);
            stream.Write(data, 0, data.Length); //Overwrites any existing file at path
            stream.Close();

            Debug.Log("<color=blue><b>[DataStorage]</b></color> Data file saved at: " + path);
        }
        catch
        {
            Debug.LogError("<color=blue><b>[DataStorage]</b></color> Data file could not be created at: " + path + ", it may be open in another program.");
        }
        
    }

    /// <summary>
    /// Loads byte array from a file with name, fileName.
    /// </summary>
    /// <param name="fileName">Name of the file the data will be loaded from.</param>
    /// <returns></returns>
    public static byte[] loadData(string fileName)
    {
        //Specify file path to load data from
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(path)) //Data file exists
        {
            try
            {
                FileStream stream = new FileStream(path, FileMode.Open);
                byte[] data = new byte[stream.Length]; //Stores the data read from stream

                stream.Read(data, 0, (int)stream.Length); //Read data from file into data[]
                stream.Close();

                Debug.Log("<color=blue><b>[DataStorage]</b></color> Data file loaded from: " + path);
                return data;
            }
            catch
            {
                Debug.Log("<color=blue><b>[DataStorage]</b></color> Data file could not be loaded at: " + path + ", it may be open in another program");
                return null;
            }
            
        }
        else //File doesn't exist
        {
            Debug.Log("<color=blue><b>[DataStorage]</b></color> Data file could not be found at: " + path);
            return null;
        }
    }

    #endregion Saving & Loading Data

    #endregion byte[]

    #region Transform Path

    /// <summary>
    /// Get the full path to a transform in the scene hierarchy, including the transform name itself.
    /// </summary>
    public static string getFullTransformPath(this Transform transform)
    {
        if (transform.parent) //Transform has a parent
        {
            return getFullTransformPath(transform.parent) + "/" + transform.name;
        }
        else //Transform doesn't have a parent
        {
            return "/" + transform.name; //Top of the scene hierarchy
        }
    }

    /// <summary>
    /// Get the path to a transform in the scene hierarchy, excluding the name of the transform itself.
    /// </summary>
    public static string getPathToTransform(this Transform transform)
    {
        if (transform.parent) //Transform has a parent
        {
            return getFullTransformPath(transform.parent) + "/" + transform.name;
        }
        else //Transform doesn't have a parent
        {
            return ""; //Top of the scene hierarchy
        }
    }

    #endregion Transform Path
}
