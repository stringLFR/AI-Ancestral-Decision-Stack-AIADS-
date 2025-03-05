using System.Collections.Generic;
using UnityEngine;


public struct ReciverPacket
{
    readonly public string packetName;
    readonly public int[] intValues;
    readonly public float[] floatValues;
    readonly public Vector3[] vectorValues;
    readonly public string[] stringValues;

    public ReciverPacket(string name,int[] ints, float[] floats, Vector3[] vectors, string[] strings)
    {
        packetName = name;
        intValues = ints;
        floatValues = floats;
        vectorValues = vectors;
        stringValues = strings;
    }
}


public abstract class AIADS_Decision_Reciver : MonoBehaviour
{
    //What packets are alowed to be recived by this reciver! (Look at ReciverPacket.packetName for comparison whenever you get new packet!)
    [SerializeField] readonly protected string[] alowedPackets; 

    protected Queue<ReciverPacket> recivedPackets;

    public string[] AlowedPackets => alowedPackets;

    protected virtual void Start() => recivedPackets = new Queue<ReciverPacket>();

    public void RecivePacket(ReciverPacket packet) => recivedPackets.Enqueue(packet);

    public void ClearPacketHistory() => recivedPackets.Clear();

}
