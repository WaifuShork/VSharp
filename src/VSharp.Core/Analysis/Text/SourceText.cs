namespace VSharp.Core.Analysis.Text;

using Spectre.Console;
using System.Collections;

[PublicAPI]
public sealed class SourceText : IComparable, ICloneable, IConvertible, IComparable<SourceText>, IEnumerable<char>, IEquatable<SourceText>
{
    private SourceText(in string text, in string fileName = "null")
    {
        Text = text;
        Length = text.Length;
        Position = 0;
        FileName = fileName;
        Lines = ParseLines(this, text);
    }

    public static readonly SourceText Empty = new(""); 
    
    public int Length { get; }
    public string FileName { get; }
    public string Text { get; }

    public IReadOnlyList<TextLine> Lines { get; }
    
    public int Position { get; private set; }
    
    public char this[int index]
    {
        get
        {
            if (!index.IsWithinBounds(this))
            {
                Console.WriteLine($"Index: {index}, Source: {Text}");
                throw new IndexOutOfRangeException(nameof(index));
            }
            
            return At(index);
        }
    }

    public SourceText this[Range range]
    {
        get
        {
            if (!range.IsWithinBounds(this))
            {
                throw new IndexOutOfRangeException(nameof(range));
            }

            return From(From(in range));
        }
    }

    public char At(in int index)
    {
        return Text.At(in index);
    }

    public string From(in Range range)
    {
        return Text.From(in range);
    }

    public void Increment(in int amount = 1)
    {
        var index = Position + amount;
        if (index.IsWithinBounds(Text))
        {
            Position = index;
        }
    }

    public static bool operator ==(in SourceText left, in SourceText right)
    {
        return left.Equals(right);
    }
    
    public static bool operator !=(in SourceText left, in SourceText right)
    {
        return !(left == right);
    }
    
    public static SourceText operator ++(in SourceText text)
    {
        text.Increment();
        return text;
    }

    public static SourceText operator --(in SourceText text)
    {
        text.Decrement();
        return text;
    }
    
    public void Decrement(in int amount = 1)
    {
        var index = Position - amount;
        if (index.IsWithinBounds(Text))
        {
            Position = amount;
        }
    }
    
    public SourceText[] Chunk()
    {
        var inserter = Lines.Count / 4;
        var lines = Lines.ToList();
        return new[]
        {
            From(lines.Slice(..inserter).ToString(SeparatorKind.NewLine)),
            From(lines.Slice((inserter + 1)..(inserter * 2)).ToString(SeparatorKind.NewLine)),
            From(lines.Slice((inserter * 2 + 1)..(inserter * 3)).ToString(SeparatorKind.NewLine)),
            From(lines.Slice((inserter * 3 + 1)..(inserter * 4)).ToString(SeparatorKind.NewLine)),
        };
    }
    
    private static IReadOnlyList<TextLine> ParseLines(in SourceText sourceText, in string text)
    {
        var result = new List<TextLine>();

        var position = 0;
        var lineStart = 0;

        while (position < text.Length)
        {
            var lineBreakWidth = GetLineBreakWidth(in text, in position);
            if (lineBreakWidth == 0)
            {
                position++;
            }
            else
            {
                AddLine(result, in sourceText, in position, in lineStart, in lineBreakWidth);
                position += lineBreakWidth;
                lineStart = position;
            }
        }

        if (position >= lineStart)
        {
            AddLine(result, sourceText, position, lineStart, 0);
        }

        return result;
    }

    private static void AddLine(ICollection<TextLine> result, in SourceText sourceText, in int position, in int lineStart, in int lineBreakWidth)
    {
        var lineLength = position - lineStart;
        var lineLengthIncludingLineBreak = lineLength + lineBreakWidth;
        var line = new TextLine(sourceText, lineStart, lineLength, lineLengthIncludingLineBreak);
        result.Add(line);
    }
    
    private static int GetLineBreakWidth(in string text, in int position)
    {
        var c = text[position];
        var l = position + 1 >= text.Length ? '\0' : text[position + 1];

        if (c == '\r' && l == '\n')
        {
            return 2;
        }

        if (c == '\r' || c == '\n')
        {
            return 1;
        }

        return 0;
    }
    
    public int GetLineIndex(in int position)
    {
        var lower = 0;
        var upper = Lines.Count - 1;

        while (lower <= upper)
        {
            var index = lower + ((upper - lower) / 2);
            var start = Lines[index].Start;

            if (position == start)
            {
                return index;
            }

            if (start > position)
            {
                upper = index - 1;
            }
            else
            {
                lower = index + 1;
            }
        }

        return lower - 1;
    }
    
    public static SourceText From(in string text, in string fileName = "src/script.vs")
    {
        return new SourceText(in text, in fileName);
    }

    public string Substring(TextSpan span)
    {
        if (span.Length <= 0)
        {
            return "";
        }

        return Substring(span.Start, span.Length);
    }
    
    public string Substring(in int startIndex, in int length)
    {
        return Text.Substring(startIndex, length);
    }

    public string ToString(in TextSpan span)
    {
        return Substring(span.Start, span.Length);
    }
    
    public override string ToString()
    {
        return Text;
    }
    
    public int CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1;
        }

        if (obj is not SourceText)
        {
            throw new ArgumentException("invalid type");
        }

        return string.Compare(Text, obj as string, StringComparison.CurrentCulture);
    }

    public object Clone()
    {
        return new SourceText(new string(Text), FileName);
    }

    public TypeCode GetTypeCode()
    {
        return TypeCode.String;
    }

    public bool ToBoolean(IFormatProvider? provider)
    {
        try
        {
            return Convert.ToBoolean(Text, provider);
        }
        catch (Exception exception)
        {
            AnsiConsole.WriteException(exception);
            return false;
        }    }

    public byte ToByte(IFormatProvider? provider)
    {
        try
        {
            return Convert.ToByte(Text, provider);
        }
        catch (Exception exception)
        {
            AnsiConsole.WriteException(exception);
            return 0;
        }    }

    public char ToChar(IFormatProvider? provider)
    {
        try
        {
            return Convert.ToChar(Text, provider);
        }
        catch (Exception exception)
        {
            AnsiConsole.WriteException(exception);
            return CharacterInfo.InvalidCharacter;
        }    
    }

    public DateTime ToDateTime(IFormatProvider? provider)
    {
        try
        {
            return Convert.ToDateTime(Text, provider);
        }
        catch (Exception exception)
        {
            AnsiConsole.WriteException(exception);
            return DateTime.Now;
        }    }

    public decimal ToDecimal(IFormatProvider? provider)
    {
        try
        {
            return Convert.ToDecimal(Text, provider);
        }
        catch (Exception exception)
        {
            AnsiConsole.WriteException(exception);
            return 0;
        }    }

    public double ToDouble(IFormatProvider? provider)
    {
        try
        {
            return Convert.ToDouble(Text, provider);
        }
        catch (Exception exception)
        {
            AnsiConsole.WriteException(exception);
            return 0;
        }    }

    public short ToInt16(IFormatProvider? provider)
    {
        try
        {
            return Convert.ToInt16(Text, provider);
        }
        catch (Exception exception)
        {
            AnsiConsole.WriteException(exception);
            return 0;
        }    }

    public int ToInt32(IFormatProvider? provider)
    {
        try
        {
            return Convert.ToInt32(Text, provider);
        }
        catch (Exception exception)
        {
            AnsiConsole.WriteException(exception);
            return 0;
        }    }

    public long ToInt64(IFormatProvider? provider)
    {
        try
        {
            return Convert.ToInt64(Text, provider);
        }
        catch (Exception exception)
        {
            AnsiConsole.WriteException(exception);
            return 0;
        }    }

    public sbyte ToSByte(IFormatProvider? provider)
    {
        try
        {
            return Convert.ToSByte(Text, provider);
        }
        catch (Exception exception)
        {
            AnsiConsole.WriteException(exception);
            return 0;
        }
    }

    public float ToSingle(IFormatProvider? provider)
    {
        try
        {
            return Convert.ToSingle(Text, provider);
        }
        catch (Exception exception)
        {
            AnsiConsole.WriteException(exception);
            return 0;
        }
    }

    public string ToString(IFormatProvider? provider)
    {
        return Text;
    }

    public object ToType(Type conversionType, IFormatProvider? provider)
    {
        try
        {
            return Convert.ChangeType(Text, conversionType, provider);
        }
        catch (Exception exception)
        {
            AnsiConsole.WriteException(exception);
            return Text;
        }
    }

    public ushort ToUInt16(IFormatProvider? provider)
    {
        try
        {
            return Convert.ToUInt16(Text, provider);
        }
        catch (Exception exception)
        {
            AnsiConsole.WriteException(exception);
            return 0;
        }
    }

    public uint ToUInt32(IFormatProvider? provider)
    {
        try
        {
            return Convert.ToUInt32(Text, provider);
        }
        catch (Exception exception)
        {
            AnsiConsole.WriteException(exception);
            return 0;
        }
    }

    public ulong ToUInt64(IFormatProvider? provider)
    {
        try
        {
            return Convert.ToUInt64(Text, provider);
        }
        catch (Exception exception)
        {
            AnsiConsole.WriteException(exception);
            return 0;
        }
    }

    public IEnumerator<char> GetEnumerator()
    {
        return Text.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int CompareTo(SourceText? other)
    {
        return string.CompareOrdinal(Text, other?.Text);
    }

    public bool Equals(SourceText? other)
    {
        return Text == other?.Text &&
               Length == other.Length &&
               FileName == other.FileName &&
               Lines.Equals(other.Lines);
    }

    public override bool Equals(object? other)
    {
        if (other is not SourceText source)
        {
            return false;
        }

        return (source.Text, source.Length, source.FileName, source.Lines)
            .Equals((source.Text, source.Length, source.FileName, source.Lines));
    }

    public override int GetHashCode()
    {
        return (Text, Length, FileName, Lines).GetHashCode();
    }
}