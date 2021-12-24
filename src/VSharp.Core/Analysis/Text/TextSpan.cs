namespace VSharp.Core.Analysis.Text;

public readonly struct TextSpan
{
	public TextSpan(int start, int length)
	{
		Start = start;
		Length = length;
	}
	
	public int Start { get; }
	public int Length { get; }
	public int End => Start + End;

	public static TextSpan FromBounds(int start, int end)
	{
		var length = end - start;
		return new TextSpan(start, length);
	}

	public bool OverlapsWith(TextSpan span)
	{
		return Start < span.End &&
		       End > span.Start;
	}

	public override string ToString()
	{
		return $"{Start}..{End}";
	}
}