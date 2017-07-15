using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

namespace DataMesh.AR.Utility
{

    public static class BytesUtility
    {

        public static void WriteByteToTBytes(byte[] data, byte b, ref int index)
        {
            data[index] = b;
            index++;
        }

        public static void WriteFloatToBytes(byte[] data, float f, ref int index)
        {
            byte[] floatBytes;

            floatBytes = BitConverter.GetBytes(f);
            Array.Copy(floatBytes, 0, data, index, floatBytes.Length);
            index += floatBytes.Length;
        }

        public static void WriteIntToBytes(byte[] data, int n, ref int index)
        {
            byte[] intBytes;

            intBytes = BitConverter.GetBytes(n);
            Array.Copy(intBytes, 0, data, index, intBytes.Length);
            index += intBytes.Length;
        }

        public static void WriteVectorToBytes(byte[] data, Vector3 vector, ref int index)
        {
            WriteFloatToBytes(data, vector.x, ref index);
            WriteFloatToBytes(data, vector.y, ref index);
            WriteFloatToBytes(data, vector.z, ref index);
        }

        public static void WriteVectorToBytes(byte[] data, Quaternion vector, ref int index)
        {
            WriteFloatToBytes(data, vector.x, ref index);
            WriteFloatToBytes(data, vector.y, ref index);
            WriteFloatToBytes(data, vector.z, ref index);
            WriteFloatToBytes(data, vector.w, ref index);
        }

        public static void WriteLongToBytes(byte[] data, long n, ref int index)
        {
            byte[] intBytes;

            intBytes = BitConverter.GetBytes(n);
            Array.Copy(intBytes, 0, data, index, intBytes.Length);
            index += intBytes.Length;
        }

        public static void GetByteFromBytes(byte[] data, out byte b, ref int index)
        {
            b = data[index];
            index++;
        }

        public static void GetFloatFromBytes(byte[] data, out float f, ref int index)
        {
            f = BitConverter.ToSingle(data, index);
            index += sizeof(float);
        }

        public static void GetIntFromBytes(byte[] data, out int n, ref int index)
        {
            n = BitConverter.ToInt32(data, index);
            index += sizeof(int);
        }

        public static void GetLongFromBytes(byte[] data, out long n, ref int index)
        {
            n = BitConverter.ToInt64(data, index);
            index += sizeof(long);
        }

        public static void GetVectorFromBytes(byte[] data, out Vector3 vector, ref int index)
        {
            vector = new Vector3();

            GetFloatFromBytes(data, out vector.x, ref index);
            GetFloatFromBytes(data, out vector.y, ref index);
            GetFloatFromBytes(data, out vector.z, ref index);
        }

        public static void GetVectorFromBytes(byte[] data, out Quaternion quaternion, ref int index)
        {
            quaternion = new Quaternion();

            GetFloatFromBytes(data, out quaternion.x, ref index);
            GetFloatFromBytes(data, out quaternion.y, ref index);
            GetFloatFromBytes(data, out quaternion.z, ref index);
            GetFloatFromBytes(data, out quaternion.w, ref index);
        }
    }

}