namespace DialogMaker.Core.Executioning
{
    public abstract class OpCode(DialogByteCode code, string name)
    {
        public DialogByteCode Code { get; } = code;
        public string Name { get; } = name;
    }
}
