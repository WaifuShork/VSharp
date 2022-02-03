using System.Collections;

namespace VSharp.Core.Analysis.Syntax;

[PublicAPI]
public sealed class ParameterListSyntax : SyntaxNode, IEnumerable<ParameterSyntax>
{
	public ParameterListSyntax(IReadOnlyList<ParameterSyntax> parameters)
	{
		Parameters = parameters;
	}
	
	public override SyntaxKind Kind => SyntaxKind.ParameterList;
	public IReadOnlyList<ParameterSyntax> Parameters { get; }

	public override IEnumerable<SyntaxNode> GetChildren()
	{
		return Parameters;
	}

	public IEnumerator<ParameterSyntax> GetEnumerator()
	{
		return Parameters.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}