using System.Collections.Generic;
using UnityEngine;

public abstract class AIADS_Decision_Reciver : MonoBehaviour
{
    //What packets are alowed to be recived by this reciver! (Look at ReciverPacket.packetName for comparison whenever you get new packet!)
    [SerializeField] protected string[] alowedPackets; 

    protected Queue<Packet> recivedPackets;

    public string[] AlowedPackets => alowedPackets;

    protected virtual void Awake() => recivedPackets = new Queue<Packet>();

    public virtual void CreatePacket(string packetName,int[] ints = null, float[] floats = null, Vector3[] vectors = null, string[] strings = null, bool[] bools = null)
    {
        bool nameFound = false;

        foreach (string alowedPacket in alowedPackets) if (packetName == alowedPacket) nameFound = true;

        if (nameFound == false) return;

        Packet packet = new Packet(packetName, ints, floats, vectors, strings, bools);

        recivedPackets.Enqueue(packet);
    }

    public virtual void ClearPacketHistory() => recivedPackets.Clear();

}
