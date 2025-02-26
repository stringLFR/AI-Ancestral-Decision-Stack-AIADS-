using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Inferfaces

//These interface can be used to create both decisions and blackborads!
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
    #region SerializeFields

    //AIADS monobehaviours for bot handling both doDecision and getCondition requests!
    [SerializeField] protected AIADS_Decision_Reciver[] recivers;
    [SerializeField] protected AIADS_Info_Gatherer[] gatherers;

    //These floats handle update loop ticks and current max decide delay!
    [SerializeField] protected float updateTickRateInSeconds = 1f;
    [SerializeField] protected float decisionTickRateInSeconds = 1f;
    [SerializeField] protected float maxDecideDelay = 1f;

    //These are the constructor settings for the root decision! Here you can set up initial children decisions in childDecisionsKeyValues!
    [Header("AIADS_Decision_Root_Stats")]
    [SerializeField] protected string keyValue, blackboardKeyValue;
    [SerializeField] protected string[] childDecisionsKeyValues;
    [SerializeField] protected float minimumScoreValue;
    [SerializeField] protected int decideDelayValue,reciverIndexValue,gathererIndexValue;

    #endregion

    //The stack like container for decisons. It also stores all blackboards!
    protected AIADS_Stack myStack = new AIADS_Stack();

    protected AIADS_Decision root;

    //The update loop and tick rate holders!
    protected Coroutine loop;
    protected WaitForSeconds updateTick;
    protected WaitForSeconds decisionTick;

    //Creates the root!
    protected virtual void Awake()
    {
        root = new AIADS_Root(keyValue, blackboardKeyValue, childDecisionsKeyValues, minimumScoreValue, decideDelayValue, reciverIndexValue, gathererIndexValue);
    }

    //Starts the update corutine!
    protected virtual void Start()
    {
        updateTick = new WaitForSeconds(updateTickRateInSeconds);
        decisionTick = new WaitForSeconds(decisionTickRateInSeconds);

        if (root == null || myStack.currentDecision == null) return;

        loop = StartCoroutine(UpdateLoop());
    }

    #region MainUpdateLoop_And_AIADS_Stack_Manipulators

    //Main update loop. It will return a waitforseconds based on updateTick value!
    protected virtual IEnumerator UpdateLoop()
    {
        GetDecisionScore();

        if (myStack.currentDecision != null)
        {
            float time = 0;
            
            while(time < maxDecideDelay)
            {
                //Check if a decision can be activated and set the waitingForDecisionCall to true!
                ActivateDecision(myStack.currentDecision, myStack.count, time);

                //This makes a small break, then + in the amount of time waited into time!
                yield return decisionTick;
                time += decisionTickRateInSeconds;
            }

            //Reset all decisions waitingForDecisionCall to false!
            ResetDecisions(myStack.currentDecision, myStack.count);
        }

        yield return updateTick;
    }

    //Switches target decision's parent to another one from the dictionary!
    public virtual void SwitchParentDecision(AIADS_Decision targetChildDecision, string newParentDecisionKey)
    {
        AIADS_Decision grandParent = targetChildDecision.Parent.Parent;

        targetChildDecision.Parent.Parent = null;

        targetChildDecision.Parent = myStack.Decisions[newParentDecisionKey];

        targetChildDecision.Parent.Parent = grandParent;
    }

    //Stops the updateLoop!
    public virtual void StopUpdateLoop() => StopCoroutine(loop);

    //Starts the UpdateLoop!
    public virtual void StartUpdateLoop() => loop = StartCoroutine(UpdateLoop());

    //These four methods both stops and restarts the updateLoop to insert/remove decisons and blackboards!
    #region Insert/Remove_Mehtods
    public virtual void AIADSStackInsert(AIADS_Decision decision)
    {
        StopCoroutine(loop);
        myStack.Decisions.Add(decision.Key, decision);
        loop = StartCoroutine(UpdateLoop());
    }

    public virtual void AIADSStackInsert(AIADS_Blackboard board)
    {
        StopCoroutine(loop);
        myStack.Blackboards.Add(board.Key, board);
        loop = StartCoroutine(UpdateLoop());
    }

    public virtual void AIADSStackRemove(AIADS_Decision decision)
    {
        StopCoroutine(loop);
        myStack.Decisions.Remove(decision.Key);
        loop = StartCoroutine(UpdateLoop());
    }

    public virtual void AIADSStackRemove(AIADS_Blackboard board)
    {
        StopCoroutine(loop);
        myStack.Blackboards.Remove(board.Key);
        loop = StartCoroutine(UpdateLoop());
    }
    #endregion
    
    protected virtual void ResetDecisions(AIADS_Decision decision, int currentCount)
    {
        for(int i = currentCount; i > 0; i--)
        {
            if (decision == root || decision == null) return;

            decision.waitingForDecisionCall = true;

            decision = decision.Parent;
        }
    }

    //Based on totalStepsBack, will go back through the parent chain in the stack strucutre and return a parent decision!
    public virtual AIADS_Decision GetAIADSStackMemeber(AIADS_Decision parent, int currentCount, int totalStepsBack)
    {
        for (int i = currentCount; i > 0; i--)
        {
            if (parent == root || parent == null) return null;

            if (totalStepsBack <= 0) return parent;

            parent = parent.Parent;
        }
        return null; //This should never be returned!
    }

    protected virtual void ActivateDecision(AIADS_Decision decision, int currentCount, float time)
    {
        for (int i = currentCount; i > 0; i--)
        {
            if (decision == root || decision == null) return;

            if (time >= decision.DecideDelay && decision.waitingForDecisionCall == true)
            {
                decision.waitingForDecisionCall = false;
                decision.DoDecision(myStack.Blackboards[decision.BlackboardKey], this, recivers[decision.ReciverIndex]);
            }

            decision = decision.Parent;
        }
    }

    //This methods checks which decisions can be added to the stack strucutre! Any child decision takes priority before the method checks currentDecision!
    protected virtual void GetDecisionScore()
    {
        float bestScore = 0f;
        string bestDecision = null;
        string[] decisionArray = myStack.currentDecision == null ? root.ChildDecisionsKeys : myStack.currentDecision.ChildDecisionsKeys;

        if (decisionArray != null)
        {
            foreach (string decision in decisionArray)
            {
                float score = myStack.Decisions[decision].GetCondition(myStack.Blackboards[myStack.Decisions[decision].BlackboardKey], this, gatherers[myStack.Decisions[decision].GathererIndex]);

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

                    //Makes a recursive call!
                    if (myStack.currentDecision.ChildDecisionsKeys != null) GetDecisionScore();

                    return;
                }
            }
        }

        if (myStack.currentDecision != null || myStack.currentDecision != root)
        {
            float currentScore = myStack.currentDecision.GetCondition(myStack.Blackboards[myStack.currentDecision.BlackboardKey], this, gatherers[myStack.currentDecision.GathererIndex]);

            if (myStack.currentDecision.MinumimScore > currentScore)
            {
                string formerCurrentKey = myStack.currentDecision.Key;
                myStack.currentDecision = myStack.currentDecision.Parent;
                myStack.count--;
                myStack.Decisions[formerCurrentKey].Parent = null;

                if (myStack.currentDecision != null || myStack.currentDecision != root) GetDecisionScore();
            }
        }
    }

    #endregion
}

#region OtherClasses

//The stack structure which holds blackboards, decisions, currentDecision and count!
public class AIADS_Stack
{
    public Dictionary<string, AIADS_Decision> Decisions;
    public Dictionary<string, AIADS_Blackboard> Blackboards;
    public AIADS_Decision currentDecision;
    public int count = 0;
}

public abstract class AIADS_Decision
{
    //These can only be set by the constructor!!!
    readonly protected string key, blackboardKey;
    readonly protected string[] childDecisionsKeys;
    readonly protected float minimumScore = 0f;
    readonly protected float decideDelay = 0;
    readonly protected int reciverIndex = 0;
    readonly protected int gathererIndex = 0;

    public AIADS_Decision(string keyValue, string blackboardValue, string[] childDecisionValues, float minimumScoreValue, int decideDelayValue, int reciverIndexValue, int gathererIndexValue)
    {
        key = keyValue;
        blackboardKey = blackboardValue;
        childDecisionsKeys = childDecisionValues;
        minimumScore = minimumScoreValue;
        decideDelay = decideDelayValue;
        reciverIndex = reciverIndexValue;
        gathererIndex = gathererIndexValue;
    }

    public string Key => key;
    public string BlackboardKey => blackboardKey;
    public string[] ChildDecisionsKeys => childDecisionsKeys;
    public float MinumimScore => minimumScore;
    public float DecideDelay => decideDelay;
    public int ReciverIndex => reciverIndex;
    public int GathererIndex => gathererIndex;
    public AIADS_Decision Parent;
    public bool waitingForDecisionCall = true;

    public abstract float GetCondition(AIADS_Blackboard board, AIADS_Core core, AIADS_Info_Gatherer info);
    public abstract void DoDecision(AIADS_Blackboard board, AIADS_Core core, AIADS_Decision_Reciver reciver);
}

//AIADS_Root Does not need to have any functionality other than storing the child decision keys! But you can always modify :D
public class AIADS_Root : AIADS_Decision
{
    public AIADS_Root(string keyValue, string blackboardValue, string[] childDecisionValues, float minimumScoreValue, int decideDelayValue, int reciverIndexValue, int gathererIndexValue) : base(keyValue, blackboardValue, childDecisionValues, minimumScoreValue, decideDelayValue, reciverIndexValue, gathererIndexValue)
    {
    }

    public override void DoDecision(AIADS_Blackboard board, AIADS_Core core, AIADS_Decision_Reciver reciver) => throw new System.NotImplementedException();
    public override float GetCondition(AIADS_Blackboard board, AIADS_Core core, AIADS_Info_Gatherer info) => throw new System.NotImplementedException();
}

public abstract class AIADS_Blackboard
{
    //This can only be set by the constructor!!!
    readonly string key;
    public AIADS_Blackboard(string k) => key = k;
    public string Key => key;
}

#endregion
