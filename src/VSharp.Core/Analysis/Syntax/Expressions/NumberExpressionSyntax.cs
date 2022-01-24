namespace VSharp.Core.Analysis.Syntax.Expressions;

[PublicAPI]
public sealed class UInt8LiteralExpressionSyntax : ExpressionSyntax
{
	public UInt8LiteralExpressionSyntax(in SyntaxToken<uint8> numberToken)
	{
		NumberToken = numberToken;
	}

	public override SyntaxKind Kind => SyntaxKind.UInt8LiteralExpression;
	public SyntaxToken<uint8> NumberToken { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return NumberToken;
	}
}

[PublicAPI]
public sealed class Int8LiteralExpressionSyntax : ExpressionSyntax
{
	public Int8LiteralExpressionSyntax(in SyntaxToken<int8> numberToken)
	{
		NumberToken = numberToken;
	}

	public override SyntaxKind Kind => SyntaxKind.Int8LiteralExpression;
	public SyntaxToken<int8> NumberToken { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return NumberToken;
	}
}

[PublicAPI]
public sealed class UInt16LiteralExpressionSyntax : ExpressionSyntax
{
	public UInt16LiteralExpressionSyntax(in SyntaxToken<uint16> numberToken)
	{
		NumberToken = numberToken;
	}

	public override SyntaxKind Kind => SyntaxKind.UInt16LiteralExpression;
	public SyntaxToken<uint16> NumberToken { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return NumberToken;
	}
}

[PublicAPI]
public sealed class Int16LiteralExpressionSyntax : ExpressionSyntax
{
	public Int16LiteralExpressionSyntax(in SyntaxToken<int16> numberToken)
	{
		NumberToken = numberToken;
	}

	public override SyntaxKind Kind => SyntaxKind.Int16LiteralExpression;
	public SyntaxToken<int16> NumberToken { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return NumberToken;
	}
}

[PublicAPI]
public sealed class Int32LiteralExpressionSyntax : ExpressionSyntax
{
	public Int32LiteralExpressionSyntax(in SyntaxToken<int32> numberToken)
	{
		NumberToken = numberToken;
	}

	public override SyntaxKind Kind => SyntaxKind.Int32LiteralExpression;
	public SyntaxToken<int32> NumberToken { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return NumberToken;
	}
}

[PublicAPI]
public sealed class UInt32LiteralExpressionSyntax : ExpressionSyntax
{
	public UInt32LiteralExpressionSyntax(in SyntaxToken<uint32> numberToken)
	{
		NumberToken = numberToken;
	}

	public override SyntaxKind Kind => SyntaxKind.UInt32LiteralExpression;
	public SyntaxToken<uint32> NumberToken { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return NumberToken;
	}
}

[PublicAPI]
public sealed class Int64LiteralExpressionSyntax : ExpressionSyntax
{
	public Int64LiteralExpressionSyntax(in SyntaxToken<int64> numberToken)
	{
		NumberToken = numberToken;
	}

	public override SyntaxKind Kind => SyntaxKind.Int64LiteralExpression;
	public SyntaxToken<int64> NumberToken { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return NumberToken;
	}
}

[PublicAPI]
public sealed class UInt64LiteralExpressionSyntax : ExpressionSyntax
{
	public UInt64LiteralExpressionSyntax(in SyntaxToken<uint64> numberToken)
	{
		NumberToken = numberToken;
	}

	public override SyntaxKind Kind => SyntaxKind.Int64LiteralExpression;
	public SyntaxToken<uint64> NumberToken { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return NumberToken;
	}
}


[PublicAPI]
public sealed class Float32LiteralExpressionSyntax : ExpressionSyntax
{
	public Float32LiteralExpressionSyntax(in SyntaxToken<float32> numberToken)
	{
		NumberToken = numberToken;
	}

	public override SyntaxKind Kind => SyntaxKind.Float32LiteralExpression;
	public SyntaxToken<float32> NumberToken { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return NumberToken;
	}
}

[PublicAPI]
public sealed class Float64LiteralExpressionSyntax : ExpressionSyntax
{
	public Float64LiteralExpressionSyntax(in SyntaxToken<float64> numberToken)
	{
		NumberToken = numberToken;
	}

	public override SyntaxKind Kind => SyntaxKind.Float64LiteralExpression;
	public SyntaxToken<float64> NumberToken { get; }
	
	public override IEnumerable<SyntaxNode> GetChildren()
	{
		yield return NumberToken;
	}
}