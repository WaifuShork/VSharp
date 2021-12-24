using VSharp.Core.Analysis.Text;

namespace VSharp.Core.Analysis.Syntax;

public readonly record struct SyntaxTrivia(SyntaxKind Kind, int Position, string Text)
{
	public TextSpan Span => new(Position, Text?.Length ?? 0);
}