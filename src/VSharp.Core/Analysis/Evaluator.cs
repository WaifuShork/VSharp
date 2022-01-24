namespace VSharp.Core.Analysis;

using static WaifuShork.Common.Math.Object.ObjectAdder;
using static WaifuShork.Common.Math.Object.ObjectMultiplier;
using static WaifuShork.Common.Math.Object.ObjectDivider;
using static WaifuShork.Common.Math.Object.ObjectSubtractor;
using static WaifuShork.Common.Math.Object.ObjectModder;

public sealed class Evaluator
{
	private readonly ExpressionSyntax m_root;
	
	public Evaluator(ExpressionSyntax root)
	{
		m_root = root;
	}

	public object Evaluate()
	{
		return EvaluateExpression(m_root);
	}

	private object EvaluateExpression(ExpressionSyntax node)
	{
		while (true)
		{
			switch (node)
			{
				case Int8LiteralExpressionSyntax i8:
					return i8.NumberToken.Value;
				case UInt8LiteralExpressionSyntax ui8:
					return ui8.NumberToken.Value;
				case Int16LiteralExpressionSyntax i16:
					return i16.NumberToken.Value;
				case UInt16LiteralExpressionSyntax ui16:
					return ui16.NumberToken.Value;
				case Int32LiteralExpressionSyntax i32:
					return i32.NumberToken.Value;
				case UInt32LiteralExpressionSyntax ui32:
					return ui32.NumberToken.Value;
				case Int64LiteralExpressionSyntax i64:
					return i64.NumberToken.Value;
				case UInt64LiteralExpressionSyntax ui64:
					return ui64.NumberToken.Value;
				case Float32LiteralExpressionSyntax f32:
					return f32.NumberToken.Value;
				case Float64LiteralExpressionSyntax f64:
					return f64.NumberToken.Value;
				case BooleanLiteralExpressionSyntax boolean:
					return boolean.BoolToken.Value;
				case BinaryExpressionSyntax b:
					var left = EvaluateExpression(b.Left);
					var right = EvaluateExpression(b.Right);

					switch (b.Operator.Kind)
					{
						case var _ when b.Operator.Kind == SyntaxKind.PlusToken:
							return Add(left, right)!;
						case var _ when b.Operator.Kind == SyntaxKind.MinusToken:
							return Subtract(left, right)!;
						case var _ when b.Operator.Kind == SyntaxKind.AsteriskToken:
							return Multiply(left, right)!;
						case var _ when b.Operator.Kind == SyntaxKind.FSlashToken:
							return Divide(left, right)!;
						case var _ when b.Operator.Kind == SyntaxKind.PercentToken:
							return Modulo(left, right)!;
						case var _ when b.Operator.Kind == SyntaxKind.LessToken:
							return (double)left < (double)right;
						case var _ when b.Operator.Kind == SyntaxKind.LessEqualsToken:
							return (double)left <= (double)right;
						case var _ when b.Operator.Kind == SyntaxKind.GreaterToken:
							return (double)left > (double)right;
						case var _ when b.Operator.Kind == SyntaxKind.GreaterEqualsToken:
							return (double)left >= (double)right;
						case var _ when b.Operator.Kind == SyntaxKind.PipeToken:
							return (bool)left | (bool)right;
						case var _ when b.Operator.Kind == SyntaxKind.PipePipeToken:
							return (bool)left || (bool)right;
						case var _ when b.Operator.Kind == SyntaxKind.AmpersandToken:
							return (bool)left & (bool)right;
						case var _ when b.Operator.Kind == SyntaxKind.AmpersandAmpersandToken:
							return (bool)left && (bool)right;
						case var _ when b.Operator.Kind == SyntaxKind.EqualsEqualsToken:
							return Equals(left, right);
						default:
							throw new Exception($"unexpected binary operator {b.Operator.Kind}");
					}
				case ParenthesizedExpressionSyntax p:
					node = p.Expression;
					continue;
				default:
					throw new Exception($"unexpected node {node.GetType()}");
			} 
		}
	}
}