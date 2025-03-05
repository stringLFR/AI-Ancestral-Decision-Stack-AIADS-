
//AIADS_Root Does not need to have any functionality other than storing the child decision keys! But you can always modify :D
public class AIADS_Root : AIADS_Decision
{
    public AIADS_Root(string keyValue, string blackboardValue, string[] childDecisionValues, float minimumScoreValue, int decideDelayValue, int reciverIndexValue, int gathererIndexValue) : base(keyValue, blackboardValue, childDecisionValues, minimumScoreValue, decideDelayValue, reciverIndexValue, gathererIndexValue)
    {
    }

    public override void DoDecision(AIADS_Blackboard board, AIADS_Core core, AIADS_Decision_Reciver reciver) => throw new System.NotImplementedException();
    public override float GetCondition(AIADS_Blackboard board, AIADS_Core core, AIADS_Info_Gatherer info) => throw new System.NotImplementedException();
    public override void InheritFromParents(string[] parentKeyChain, string[] parentBoardChain, AIADS_Core core) => throw new System.NotImplementedException();
    public override void WhenPoppedFromStack(AIADS_Blackboard board, AIADS_Core core) => throw new System.NotImplementedException();
    public override void WhenPushedOnStack(AIADS_Blackboard board, AIADS_Core core) => throw new System.NotImplementedException();
}
