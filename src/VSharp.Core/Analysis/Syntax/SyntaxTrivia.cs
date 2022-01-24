namespace VSharp.Core.Analysis.Syntax;

using WaifuShork.Common;
using System.Diagnostics.CodeAnalysis;

public readonly record struct SyntaxTrivia(in SyntaxKind Kind, in int Position, in string Text)
{
	public TextSpan Span => new(Position, Text?.Length ?? 0);

	[SuppressMessage("ReSharper", "UseDeconstructionOnParameter")]
	public bool Equals(SyntaxTrivia other)
	{
		return other.Kind == Kind 
		    && other.Position == Position 
		    && other.Text == Text;
	}
	
	public override int GetHashCode()
	{
		var hash = (Kind, Position, Text).GetHashCode();
		if (hash.IsInvalidHash())
		{
			return (int)$"({Kind:X}|{Position:X}|{Text})".GetFnvHashCode();
		}

		return hash;
	}
}