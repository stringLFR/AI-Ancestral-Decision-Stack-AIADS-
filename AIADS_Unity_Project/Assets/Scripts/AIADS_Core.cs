using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAIADS_Decision_Creator
{
    public AIADS_Decision CreateAIADSDecision(); 
}

public interface IAIADS_Blackboard_Creator
{
    public AIADS_Blackboard CreateAIADSBlackboard();
}

public class AIADS_Core : MonoBehaviour
{
    [SerializeField] float updateTickRate = 1f;

    AIADS_Stack myStack = new AIADS_Stack();

    //Make sure to assign a root decision!
    //A root decision do not need to have anything special with it, just store childDecisionsKeys in it!
    AIADS_Decision root;

    Coroutine loop;

    WaitForSeconds updateTick;
    WaitForSeconds decideTick;

    protected virtual void Start()
    {
        updateTick = new WaitForSeconds(updateTickRate);
        UpdateCorutine();
    }

    void UpdateCorutine()
    {
        if (root == null || myStack.currentDecision == null) return;

        StopCoroutine(loop);
        loop = StartCoroutine(UpdateLoop());
    }

    IEnumerator UpdateLoop()
    {
        GetDecisionScore();

        if (myStack.currentDecision != null)
        {
            int loops = 0;

            while(loops < myStack.count)
            {
                decideTick = new WaitForSeconds(GetDecision(myStack.currentDecision, loops, 0));
                loops++;
            }
        }

        yield return updateTick;
    }

    float GetDecision(AIADS_Decision decision, int maxLoops, int currentLoop)
    {
        if (decision == root || decision == null) return 0f;

        int current = currentLoop;

        if (maxLoops > currentLoop) GetDecision(myStack.currentDecision, maxLoops, current++);
        else
        {
            decision.DoDecision(myStack.Blackboards[decision.BlackboardKey], this);
            return decision.DecideDelay;
        }

        return 0f;
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
                    myStack.count++;

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
                myStack.count--;

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

    public int count = 0;
}

public abstract class AIADS_Decision
{
    string key, blackboardKey;

    string[] childDecisionsKeys;

    float minimumScore = 0f;

    float decideDelay = 0f;

    public AIADS_Decision(string keyValue, string blackboardValue, string[] childDecisinValues, float minimumScoreValue, float decideDelayValue)
    {
        key = keyValue;
        blackboardKey = blackboardValue;
        childDecisionsKeys = childDecisinValues;
        minimumScore = minimumScoreValue;
        decideDelay = decideDelayValue;
    }

    public string Key => key;

    public string BlackboardKey => blackboardKey;

    public string[] ChildDecisionsKeys => childDecisionsKeys;

    public float MinumimScore => minimumScore;

    public float DecideDelay => decideDelay;

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
