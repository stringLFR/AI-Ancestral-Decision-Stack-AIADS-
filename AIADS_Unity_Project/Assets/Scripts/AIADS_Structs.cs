using UnityEngine;

readonly public struct Packet
{
    readonly public string packetName;
    readonly public int[] intValues;
    readonly public float[] floatValues;
    readonly public Vector3[] vectorValues;
    readonly public string[] stringValues;
    readonly public bool[] boolValues;

    public Packet(string name, int[] ints = null, float[] floats = null, Vector3[] vectors = null, string[] strings = null, bool[] bools = null)
    {
        packetName = name;
        intValues = ints;
        floatValues = floats;
        vectorValues = vectors;
        stringValues = strings;
        boolValues = bools;
    }
}