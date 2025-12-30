namespace DialogMaker.Core.Executioning
{
    public enum DialogByteCode : byte
    {
        ShowReplica,
        ShowFullScreenReplica,
        ShowColorReplica,
        ShowChoice,

        Equals,
        NotEquals,
        Above,
        AboveOrEquals,
        Less,
        LessOrEquals,

        Jump,
        JumpIfTrue,
        End,

        Add,
        Subtract,
        Multiply,
        Divide,
        Replace,

        GetVariable,
        SetVariable,
        GetData
    }
}
