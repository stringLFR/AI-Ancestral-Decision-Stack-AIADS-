using System.Collections.Generic;

//The stack structure which holds blackboards, decisions, currentDecision and count!
public class AIADS_Stack
{
    public Dictionary<string, AIADS_Decision> Decisions;
    public Dictionary<string, AIADS_Blackboard> Blackboards;
    public string currentDecisionKey;
    public int count = 0;
}
