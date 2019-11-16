using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace NICE_Networking
{
    public static class ObjectSerializer
    {
        #region Serialization

        /// <summary>
        /// Serializes the data to a byte array.
        /// </summary>
        public static byte[] serialize(this bool data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data);
                    bw.Flush();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Serializes the data to a byte array.
        /// </summary>
        public static byte[] serialize(this byte data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data);
                    bw.Flush();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Serializes the data as a byte array.
        /// </summary>
        public static byte[] serialize(this byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data.Length); //Length of the array
                    bw.Write(data);
                    bw.Flush();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Serializes the data to a byte array.
        /// </summary>
        public static byte[] serialize(this char data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data);
                    bw.Flush();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Serializes the data to a byte array.
        /// </summary>
        public static byte[] serialize(this char[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data.Length); //Length of the array
                    bw.Write(data);

                    bw.Flush();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Serializes the data to a byte array.
        /// </summary>
        public static byte[] serialize(this decimal data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data);
                    bw.Flush();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Serializes the data to a byte array.
        /// </summary>
        public static byte[] serialize(this double data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data);
                    bw.Flush();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Serializes the data to a byte array.
        /// </summary>
        public static byte[] serialize(this short data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data);
                    bw.Flush();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Serializes the data to a byte array.
        /// </summary>
        public static byte[] serialize(this int data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data);
                    bw.Flush();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Serializes the data to a byte array.
        /// </summary>
        public static byte[] serialize(this long data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data);
                    bw.Flush();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Serializes the data to a byte array.
        /// </summary>
        public static byte[] serialize(this sbyte data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data);
                    bw.Flush();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Serializes the data to a byte array.
        /// </summary>
        public static byte[] serialize(this float data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data);
                    bw.Flush();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Serializes the data to a byte array.
        /// </summary>
        public static byte[] serialize(this float[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data.Length); //Write size of array

                    foreach (float f in data) //Write each float in the array
                        bw.Write(f);

                    bw.Flush();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Serializes the data to a byte array.
        /// </summary>
        public static byte[] serialize(this string data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data);
                    bw.Flush();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Serializes the data to a byte array.
        /// </summary>
        public static byte[] serialize(this ushort data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data);
                    bw.Flush();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Serializes the data to a byte array.
        /// </summary>
        public static byte[] serialize(this uint data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data);
                    bw.Flush();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Serializes the data to a byte array.
        /// </summary>
        public static byte[] serialize(this ulong data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data);
                    bw.Flush();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Serializes the object to a byte array.
        /// </summary>
        public static byte[] serialize(this object obj)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (var memStream = new MemoryStream())
            {
                formatter.Serialize(memStream, obj);
                return memStream.ToArray();
            }
        }

        #endregion Serialization

        #region Deserialization

        /// <summary>
        /// Attempts to deserialize the data as a bool.
        /// </summary>
        public static bool deserializeBoolean(ref byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms))
                {
                    bool myBool = br.ReadBoolean();
                    data = removeBytes(data, Marshal.SizeOf<bool>()); //remove deserialized bytes from data array
                    return myBool;
                }
            }
        }

        /// <summary>
        /// Attempts to deserialize the data as a byte.
        /// </summary>
        public static byte deserializeByte(ref byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms))
                {
                    byte myByte = br.ReadByte();
                    data = removeBytes(data, Marshal.SizeOf<byte>()); //remove deserialized bytes from data array
                    return myByte;
                }
            }
        }

        /// <summary>
        /// Attempts to deserialize the data as a byte array.
        /// </summary>
        public static byte[] deserializeBytes(ref byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms))
                {
                    int size = br.ReadInt32();
                    byte[] myBytes = br.ReadBytes(size);
                    data = removeBytes(data, Marshal.SizeOf<int>() + size); //remove deserialized bytes from data array
                    return myBytes;
                }
            }
        }

        /// <summary>
        /// Attempts to deserialize the data as a char.
        /// </summary>
        public static char deserializeChar(ref byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms))
                {
                    char myChar = br.ReadChar();
                    data = removeBytes(data, Marshal.SizeOf<char>()); //remove deserialized bytes from data array
                    return myChar;
                }
            }
        }

        /// <summary>
        /// Attempts to deserialize the data as a char array.
        /// </summary>
        public static char[] deserializeChars(ref byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms))
                {
                    int size = br.ReadInt32();
                    char[] myChars = br.ReadChars(size);
                    data = removeBytes(data, Marshal.SizeOf<int>() + (size * Marshal.SizeOf<char>())); //remove deserialized bytes from data array
                    return myChars;
                }
            }
        }

        /// <summary>
        /// Attempts to deserialize the data as a decimal.
        /// </summary>
        public static decimal deserializeDecimal(ref byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms))
                {
                    decimal myDecimal = br.ReadDecimal();
                    data = removeBytes(data, Marshal.SizeOf<decimal>()); //remove deserialized bytes from data array
                    return myDecimal;
                }
            }
        }

        /// <summary>
        /// Attempts to deserialize the data as a double.
        /// </summary>
        public static double deserializeDouble(ref byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms))
                {
                    double myDouble = br.ReadDouble();
                    data = removeBytes(data, Marshal.SizeOf<double>()); //remove deserialized bytes from data array
                    return myDouble;
                }
            }
        }

        /// <summary>
        /// Attempts to deserialize the data as a short.
        /// </summary>
        public static short deserializeShort(ref byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms))
                {
                    short myShort = br.ReadInt16();
                    data = removeBytes(data, Marshal.SizeOf<short>()); //remove deserialized bytes from data array
                    return myShort;
                }
            }
        }

        /// <summary>
        /// Attempts to deserialize the data as a int.
        /// </summary>
        public static int deserializeInt(ref byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms))
                {
                    int myInt = br.ReadInt32();
                    data = removeBytes(data, Marshal.SizeOf<int>()); //remove deserialized bytes from data array
                    return myInt;
                }
            }
        }

        /// <summary>
        /// Attempts to deserialize the data as a long.
        /// </summary>
        public static long deserializeLong(ref byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms))
                {
                    long myLong = br.ReadInt64();
                    data = removeBytes(data, Marshal.SizeOf<long>()); //remove deserialized bytes from data array
                    return myLong;
                }
            }
        }

        /// <summary>
        /// Attempts to deserialize the data as a sbyte.
        /// </summary>
        public static sbyte deserializeSByte(ref byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms))
                {
                    sbyte mySByte = br.ReadSByte();
                    data = removeBytes(data, Marshal.SizeOf<sbyte>()); //remove deserialized bytes from data array
                    return mySByte;
                }
            }
        }

        /// <summary>
        /// Attempts to deserialize the data as a float.
        /// </summary>
        public static float deserializeFloat(ref byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms))
                {
                    float myFloat = br.ReadSingle();
                    data = removeBytes(data, Marshal.SizeOf<float>()); //remove deserialized bytes from data array
                    return myFloat;
                }
            }
        }

        /// <summary>
        /// Attempts to deserialize the data as a float array.
        /// </summary>
        public static float[] deserializeFloats(ref byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms))
                {
                    int size = br.ReadInt32();
                    float[] floats = new float[size];

                    for (int i = 0; i < size; i++)
                    {
                        floats[i] = br.ReadSingle();
                    }

                    data = removeBytes(data, Marshal.SizeOf<int>() + (size * Marshal.SizeOf<float>())); //remove deserialized bytes from data array
                    return floats;
                }
            }
        }

        /// <summary>
        /// Attempts to deserialize the data as a string.
        /// </summary>
        public static string deserializeString(ref byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms))
                {
                    string myString = br.ReadString();
                    int length = myString.serialize().Length; //Determine length of string in bytes
                    data = removeBytes(data, length); //remove deserialized bytes from data array
                    return myString;
                }
            }
        }

        /// <summary>
        /// Attempts to deserialize the data as a ushort.
        /// </summary>
        public static ushort deserializeUShort(ref byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms))
                {
                    ushort myUShort = br.ReadUInt16();
                    data = removeBytes(data, Marshal.SizeOf<ushort>()); //remove deserialized bytes from data array
                    return myUShort;
                }
            }
        }

        /// <summary>
        /// Attempts to deserialize the data as a uint.
        /// </summary>
        public static uint deserializeUInt(ref byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms))
                {
                    uint myUint = br.ReadUInt32();
                    data = removeBytes(data, Marshal.SizeOf<uint>()); //remove deserialized bytes from data array
                    return myUint;
                }
            }
        }

        /// <summary>
        /// Attempts to deserialize the data as a ulong.
        /// </summary>
        public static ulong deserializeULong(ref byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms))
                {
                    ulong myULong = br.ReadUInt64();
                    data = removeBytes(data, Marshal.SizeOf<ulong>()); //remove deserialized bytes from data array
                    return myULong;
                }
            }
        }

        /// <summary>
        /// Attempts to deserialize the data as an object.
        /// </summary>
        public static object deserializeObject(ref byte[] data)
        {
            //int objLength = deserializeInt(ref data); //Get object length

            using (var memStream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                //memStream.Write(data, 0, objLength); //Only read object length
                memStream.Write(data, 0, data.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = formatter.Deserialize(memStream);;
                //data = removeBytes(data, objLength); //remove deserialized bytes from data array
                data = removeBytes(data, obj.serialize().Length); //remove deserialized bytes from data array
                return obj;
            }
        }

        /// <summary>
        /// Attempts to deserialize the data as an object.
        /// </summary>
        public static T deserializeObject<T>(ref byte[] data)
        {
            using (var memStream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                memStream.Write(data, 0, data.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = (T)formatter.Deserialize(memStream); ;
                data = removeBytes(data, obj.serialize().Length); //Remove deserialized bytes from data array
                return obj;
            }
        }

        /// <summary>
        /// Removes bytes from the bytes array.
        /// </summary>
        /// <param name="count">Number of bytes to remove starting at index 0.</param>
        private static byte[] removeBytes(byte[] bytes, int count)
        {
            List<byte> byteList = new List<byte>(bytes);

            //Return byte array starting index count (removes all bytes before that index)
            return byteList.GetRange(count, byteList.Count - count).ToArray();
        }

        #endregion Deserialization
    }
}
