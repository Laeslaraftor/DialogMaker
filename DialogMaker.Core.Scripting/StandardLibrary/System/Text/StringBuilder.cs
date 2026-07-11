namespace System.Text;

public class StringBuilder
{
    public StringBuilder() : this(string.Empty)
    {
    }
    public StringBuilder(string text)
    {
        _result = text;
    }

    private string _result;

    public void Append(string text) => _result += text;
    public void AppendLine() => _result += Environment.NewLine;
    public void AppendLine(string text)
    {
        _result += text;
        AppendLine();
    }
    public override string ToString() => _result;
}