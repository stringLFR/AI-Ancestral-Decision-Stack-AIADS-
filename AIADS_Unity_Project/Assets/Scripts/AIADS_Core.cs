using System.Collections.Generic;
using UnityEngine;

public interface IAIADS_Creator
{
    public AIADS_Decision CreateAIADSDecision();
    public AIADS_Blackboard CreateAIADSBlackboard();
}

public class AIADS_Core : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

public class AIADS_Stack
{
    public Dictionary<string, AIADS_Decision> Decisions;
    public Dictionary<string, AIADS_Blackboard> Blackboards;

    public AIADS_Decision currentDecision;
}

public abstract class AIADS_Decision
{
    string key, blackboardKey;

    string[] childDecisionsKeys;

    public AIADS_Decision(string k, string b, string[] c)
    {
        key = k;
        blackboardKey = b;
        childDecisionsKeys = c;
    }

    public string Key => key;

    public string BlackboardKey => blackboardKey;

    public string[] ChildDecisionsKeys => childDecisionsKeys;

    public AIADS_Decision Parent;

    public abstract float GetCondition(AIADS_Blackboard board, AIADS_Core core);

    public abstract float DoDecision(AIADS_Blackboard board, AIADS_Core core);
}

public abstract class AIADS_Blackboard
{
    string key;

    public AIADS_Blackboard(string k) => key = k;

    public string Key => key;
}
