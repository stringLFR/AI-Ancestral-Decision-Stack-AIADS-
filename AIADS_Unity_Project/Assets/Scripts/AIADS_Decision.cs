

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
    public string ParentKey;

    //Checks if a decision can be placed on the stack, based on info catched from target gatherer!
    public abstract float GetCondition(AIADS_Blackboard board, AIADS_Core core, AIADS_Info_Gatherer info);

    //The method which handles the main decision data and sends it out towards target reciver!
    public abstract void DoDecision(AIADS_Blackboard board, AIADS_Core core, AIADS_Decision_Reciver reciver);

    //This is called when this decision is pushed on the stack!
    public abstract void WhenPushedOnStack(AIADS_Blackboard board, AIADS_Core core);

    //This is called when this decision is popped from the stack!
    public abstract void WhenPoppedFromStack(AIADS_Blackboard board, AIADS_Core core);

    //This is called before "DoDecision" and it gets the current stack member history and their blackboards in an array line starting from this decision going backwards!
    //If you want to check the first member of the stack, you need to start iterating from the end of the array!
    public abstract void InheritFromParents(string[] parentKeyChain, string[] parentBoardChain, AIADS_Core core);
}
