using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Inferfaces

public interface IAIADS_Decision_Creator
{
    public AIADS_Decision CreateAIADSDecision(); 
}

public interface IAIADS_Blackboard_Creator
{
    public AIADS_Blackboard CreateAIADSBlackboard();
}

#endregion

public class AIADS_Core : MonoBehaviour
{
    [SerializeField] protected float updateTickRateInSeconds = 1f;
    [SerializeField] protected float maxDecideDelay = 1f;

    [Header("AIADS_Decision_Root_Stats")]
    [SerializeField] protected string keyValue, blackboardKeyValue;
    [SerializeField] protected string[] childDecisionsKeyValues;
    [SerializeField] protected float minimumScoreValue;
    [SerializeField] protected int decideDelayInMillisecondsValue;

    protected AIADS_Stack myStack = new AIADS_Stack();
    protected AIADS_Decision root;
    protected Coroutine loop;
    protected WaitForSeconds updateTick;

    protected virtual void Awake()
    {
        root = new AIADS_Root(keyValue, blackboardKeyValue, childDecisionsKeyValues, minimumScoreValue, decideDelayInMillisecondsValue);
    }

    protected virtual void Start()
    {
        updateTick = new WaitForSeconds(updateTickRateInSeconds);

        if (root == null || myStack.currentDecision == null) return;

        loop = StartCoroutine(UpdateLoop());
    }

    #region MainUpdateLoop_And_AIADS_Stack_Manipulators

    protected virtual IEnumerator UpdateLoop()
    {
        GetDecisionScore();

        if (myStack.currentDecision != null)
        {
            float time = 0;
            
            while(time < maxDecideDelay)
            {
                GetDecision(myStack.currentDecision, myStack.count, time);
                time += Time.deltaTime;
                yield return null;
            }

            ResetDecisions(myStack.currentDecision, myStack.count);
        }

        yield return updateTick;
    }

    #endregion

    #region RecursiveMethods

    protected virtual void ResetDecisions(AIADS_Decision decision, int currentCount)
    {
        if (decision == root || decision == null || currentCount <= 0) return;

        decision.waitingForDecisionCall = true;

        ResetDecisions(decision.Parent, currentCount--);
    }

    protected virtual void GetDecision(AIADS_Decision decision, int currentCount, float time)
    {
        if (decision == root || decision == null || currentCount <= 0) return;

        if (time >= decision.DecideDelay && decision.waitingForDecisionCall == true)
        {
            decision.waitingForDecisionCall = false;
            decision.DoDecision(myStack.Blackboards[decision.BlackboardKey], this);
        }

        GetDecision(decision.Parent, currentCount--, time);
    }

    protected virtual void GetDecisionScore()
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

    #endregion
}

#region OtherClasses

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

    float decideDelay = 0;

    public AIADS_Decision(string keyValue, string blackboardValue, string[] childDecisionValues, float minimumScoreValue, int decideDelayValue)
    {
        key = keyValue;
        blackboardKey = blackboardValue;
        childDecisionsKeys = childDecisionValues;
        minimumScore = minimumScoreValue;
        decideDelay = decideDelayValue;
    }

    public string Key => key;

    public string BlackboardKey => blackboardKey;

    public string[] ChildDecisionsKeys => childDecisionsKeys;

    public float MinumimScore => minimumScore;

    public float DecideDelay => decideDelay;

    public AIADS_Decision Parent;

    public bool waitingForDecisionCall = true;

    public abstract float GetCondition(AIADS_Blackboard board, AIADS_Core core);

    public abstract void DoDecision(AIADS_Blackboard board, AIADS_Core core);
}

public class AIADS_Root : AIADS_Decision
{
    public AIADS_Root(string keyValue, string blackboardValue, string[] childDecisionValues, float minimumScoreValue, int decideDelayValue) : base(keyValue, blackboardValue, childDecisionValues, minimumScoreValue, decideDelayValue)
    {
    }

    public override void DoDecision(AIADS_Blackboard board, AIADS_Core core)
    {
        throw new System.NotImplementedException();
    }

    public override float GetCondition(AIADS_Blackboard board, AIADS_Core core)
    {
        throw new System.NotImplementedException();
    }
}

public abstract class AIADS_Blackboard
{
    string key;

    public AIADS_Blackboard(string k) => key = k;

    public string Key => key;
}

#endregion
