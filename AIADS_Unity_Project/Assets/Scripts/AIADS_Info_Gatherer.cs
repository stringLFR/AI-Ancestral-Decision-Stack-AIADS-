using UnityEngine;

public abstract class AIADS_Info_Gatherer : MonoBehaviour
{
    public abstract Packet GetTargetPacketFromGatherer(string targetPacket);

}
