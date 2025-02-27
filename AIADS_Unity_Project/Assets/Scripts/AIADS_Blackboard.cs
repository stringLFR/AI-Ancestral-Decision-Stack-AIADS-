public abstract class AIADS_Blackboard
{
    //This can only be set by the constructor!!!
    readonly string key;
    public AIADS_Blackboard(string k) => key = k;
    public string Key => key;
}
