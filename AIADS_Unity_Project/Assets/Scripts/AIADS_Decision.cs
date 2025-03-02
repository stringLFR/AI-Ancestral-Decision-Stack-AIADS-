

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

    public abstract float GetCondition(AIADS_Blackboard board, AIADS_Core core, AIADS_Info_Gatherer info);
    public abstract void DoDecision(AIADS_Blackboard board, AIADS_Core core, AIADS_Decision_Reciver reciver);

    public abstract void InheritFromParents(string[] parentKeyChain, string[] parentBoardChain, AIADS_Core core);
}
