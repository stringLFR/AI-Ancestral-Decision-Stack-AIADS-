using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public interface IAIADS_Creator
{
    public AIADS_Decision CreateAIADSDecision();
    public AIADS_Blackboard CreateAIADSBlackboard();
}

public class AIADS_Core : MonoBehaviour
{
    AIADS_Stack myStack = new AIADS_Stack();

    //Make sure to assign a root decision!
    //A root decision do not need to have anything special with it, just store childDecisionsKeys in it!
    AIADS_Decision root; 

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        
    }

    void GetDecisionScore()
    {
        float bestScore = 0f;
        string bestDecision = null;
        string[] decisionArray = myStack.currentDecision == null ? root.ChildDecisionsKeys : myStack.currentDecision.ChildDecisionsKeys;

        if (decisionArray != null)
        {
            foreach (string decision in decisionArray)
            {
                float score = myStack.Decisions[decision].GetCondition(myStack.Blackboards[myStack.Decisions[decision].BlackboardKey], this);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestDecision = myStack.Decisions[decision].Key;
                }
            }

            if (bestDecision != null)
            {
                if (myStack.Decisions[bestDecision].MinumimScore < bestScore)
                {
                    myStack.Decisions[bestDecision].Parent = myStack.currentDecision == null ? root : myStack.currentDecision;
                    myStack.currentDecision = myStack.Decisions[bestDecision];

                    if (myStack.currentDecision.ChildDecisionsKeys != null) GetDecisionScore();

                    return;
                }
            }
        }

        if (myStack.currentDecision != null || myStack.currentDecision != root)
        {
            float currentScore = myStack.currentDecision.GetCondition(myStack.Blackboards[myStack.currentDecision.BlackboardKey], this);

            if (myStack.currentDecision.MinumimScore > currentScore)
            {
                myStack.currentDecision = myStack.currentDecision.Parent;

                if (myStack.currentDecision != null || myStack.currentDecision != root) GetDecisionScore();
            }
        }
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

    float minimumScore = 0f;

    public AIADS_Decision(string k, string b, string[] c, float s)
    {
        key = k;
        blackboardKey = b;
        childDecisionsKeys = c;
        minimumScore = s;
    }

    public string Key => key;

    public string BlackboardKey => blackboardKey;

    public string[] ChildDecisionsKeys => childDecisionsKeys;

    public float MinumimScore => minimumScore;

    public AIADS_Decision Parent;

    public abstract float GetCondition(AIADS_Blackboard board, AIADS_Core core);

    public abstract void DoDecision(AIADS_Blackboard board, AIADS_Core core);
}

public abstract class AIADS_Blackboard
{
    string key;

    public AIADS_Blackboard(string k) => key = k;

    public string Key => key;
}
