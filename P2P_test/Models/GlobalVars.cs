namespace P2P_test.Models;

public static class GlobalVars
{
    private static uint _newMessageID;

    public static uint GetNewMessageID()
    {
        _newMessageID++;
        return _newMessageID;
    }
}