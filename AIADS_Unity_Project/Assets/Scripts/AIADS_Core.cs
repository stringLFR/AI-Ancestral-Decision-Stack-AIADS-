using System.Collections;
using UnityEngine;


public class AIADS_Core : MonoBehaviour
{
    #region SerializeFields

    //AIADS monobehaviours for bot handling both doDecision and getCondition requests!
    [SerializeField] protected AIADS_Decision_Reciver[] recivers;
    [SerializeField] protected AIADS_Info_Gatherer[] gatherers;

    //These floats handle update loop ticks and amount of decision triggers in one update loop!
    [SerializeField] protected float updateTickRateInSeconds = 1f;
    [SerializeField] protected float decisionTickRateInSeconds = 1f;
    [SerializeField] protected int maxDecideTriggers = 1;

    //These are the constructor settings for the root decision! Here you can set up initial children decisions in childDecisionsKeyValues!
    [Header("AIADS_Decision_Root_Stats")]
    [SerializeField] protected string keyValue, blackboardKeyValue;
    [SerializeField] protected string[] childDecisionsKeyValues;
    [SerializeField] protected float minimumScoreValue;
    [SerializeField] protected int decideDelayValue,reciverIndexValue,gathererIndexValue;

    [Header("Max alowed stack size!")]
    [SerializeField] int maxCount = 20;

    #endregion

    //The stack like container for decisons. It also stores all blackboards!
    protected AIADS_Stack myStack;

    protected AIADS_Decision root;

    //The update loop and tick rate holders!
    protected Coroutine loop;
    protected WaitForSeconds updateTick;
    protected WaitForSeconds decisionTick;

    public AIADS_Stack MyStack => myStack;
    public AIADS_Decision_Reciver[] Recivers => recivers;
    public AIADS_Info_Gatherer[] Gatherers => gatherers;

    //Creates the root!
    protected virtual void Awake()
    {
        root = new AIADS_Root(keyValue, blackboardKeyValue, childDecisionsKeyValues, minimumScoreValue, decideDelayValue, reciverIndexValue, gathererIndexValue);
        myStack = new AIADS_Stack(maxCount);
    }

    //Starts the update corutine!
    protected virtual void Start()
    {
        updateTick = new WaitForSeconds(updateTickRateInSeconds);
        decisionTick = new WaitForSeconds(decisionTickRateInSeconds);

        if (root == null || myStack.currentDecisionKey == null) return;

        loop = StartCoroutine(UpdateLoop());
    }

    #region MainUpdateLoop_And_AIADS_Stack_Manipulators

    //Main update loop. It will return a waitforseconds based on updateTick value!
    protected virtual IEnumerator UpdateLoop()
    {
        GetDecisionScore();

        if (myStack.currentDecisionKey != null)
        {
            int triggers = 0;
            
            while(triggers < maxDecideTriggers)
            {
                ActivateDecision(myStack.Decisions[myStack.currentDecisionKey]);

                yield return decisionTick;
                triggers++;
            }
        }

        yield return updateTick;
    }

    //Switches target decision's parent to another one from the dictionary!
    public virtual void SwitchParentDecision(AIADS_Decision targetChildDecision, string newParentDecisionKey)
    {
        string grandParent = myStack.Decisions[targetChildDecision.ParentKey].ParentKey;

        myStack.Decisions[targetChildDecision.ParentKey].ParentKey = null;

        myStack.Decisions[targetChildDecision.Key].ParentKey = myStack.Decisions[newParentDecisionKey].Key;

        myStack.Decisions[targetChildDecision.ParentKey].ParentKey = grandParent;
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

    //Based on totalStepsBack, will go back through the parent chain in the stack strucutre and return a parent decision!
    public virtual AIADS_Decision GetAIADSStackMemeber(AIADS_Decision parent, int currentCount, int totalStepsBack)
    {
        for (int i = currentCount; i > 0; i--)
        {
            if (parent == root || parent == null) return null;

            if (totalStepsBack <= 0) return parent;

            parent = myStack.Decisions[parent.ParentKey];
        }
        return null; //This should never be returned!
    }

    protected virtual void ActivateDecision(AIADS_Decision decision)
    {
        if (decision == root || decision == null) return;

        int parentAmount = myStack.count - 1;
        string[] parents = new string[parentAmount];
        string[] parentBoards = new string[parentAmount];

        string currentParentKey = decision.ParentKey;
        int index = 0;

        for (int i = parentAmount; i != 0; i--)
        {
            parents[index] = currentParentKey;
            parentBoards[index] = myStack.Decisions[currentParentKey].BlackboardKey;
            index++;
            currentParentKey = myStack.Decisions[currentParentKey].ParentKey;
        }

        decision.InheritFromParents(parents, parentBoards, this);
        decision.DoDecision(myStack.Blackboards[decision.BlackboardKey], this, recivers[decision.ReciverIndex]);
    }

    //This methods checks which decisions can be added to the stack strucutre! Any child decision takes priority before the method checks currentDecision!
    protected virtual void GetDecisionScore()
    {
        float bestScore = 0f;
        string bestDecision = null;
        string[] decisionArray = myStack.currentDecisionKey == null ? root.ChildDecisionsKeys : myStack.Decisions[myStack.currentDecisionKey].ChildDecisionsKeys;

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
                    myStack.Decisions[bestDecision].ParentKey = myStack.currentDecisionKey == null ? root.Key : myStack.currentDecisionKey;
                    myStack.currentDecisionKey = myStack.Decisions[bestDecision].Key;
                    myStack.count++;
                    myStack.Decisions[bestDecision].WhenPushedOnStack(myStack.Blackboards[myStack.Decisions[bestDecision].BlackboardKey], this);

                    //Makes a recursive call!
                    if (myStack.Decisions[myStack.currentDecisionKey].ChildDecisionsKeys != null || myStack.MaxCount <= myStack.count) GetDecisionScore();

                    return;
                }
            }
        }

        if (myStack.currentDecisionKey != null || myStack.currentDecisionKey != root.Key)
        {
            float currentScore = myStack.Decisions[myStack.currentDecisionKey].GetCondition(myStack.Blackboards[myStack.Decisions[myStack.currentDecisionKey].BlackboardKey], this, gatherers[myStack.Decisions[myStack.currentDecisionKey].GathererIndex]);

            if (myStack.Decisions[myStack.currentDecisionKey].MinumimScore > currentScore)
            {
                myStack.Decisions[myStack.currentDecisionKey].WhenPoppedFromStack(myStack.Blackboards[myStack.Decisions[myStack.currentDecisionKey].BlackboardKey], this);
                string formerCurrentKey = myStack.currentDecisionKey;
                myStack.currentDecisionKey = myStack.Decisions[myStack.currentDecisionKey].ParentKey;
                myStack.count--;
                myStack.Decisions[formerCurrentKey].ParentKey = null;

                if (myStack.currentDecisionKey != null || myStack.Decisions[myStack.currentDecisionKey] != root) GetDecisionScore();
            }
        }
    }

    #endregion
}
