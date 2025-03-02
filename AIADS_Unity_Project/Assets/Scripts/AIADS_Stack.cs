using System.Collections.Generic;

//The stack structure which holds blackboards, decisions, currentDecision and count!
public class AIADS_Stack
{
    readonly int maxCount = 0;
    public AIADS_Stack(int maxCountValue)
    {
        maxCount = maxCountValue;
    }

    public int MaxCount => maxCount;
    public Dictionary<string, AIADS_Decision> Decisions;
    public Dictionary<string, AIADS_Blackboard> Blackboards;
    public string currentDecisionKey;
    public int count = 0;
}
