using System.Collections;
using System.Globalization;
using JetBrains.Annotations;

namespace VSharp.Core.Analysis.Text;

public sealed class SourceText : IComparable, ICloneable, IConvertible, IComparable<SourceText>, IEnumerable<char>, IEquatable<SourceText>
{
    // private readonly string m_text;
    private readonly string m_text;

    private SourceText(string text, string fileName = "null")
    {
        m_text = text;
        Length = text.Length;
        FileName = fileName;
        Lines = ParseLines(this, text);
    }

    public static readonly SourceText Empty = new(""); 
    
    public int Length { get; }
    public string FileName { get;  }

    public IReadOnlyList<TextLine> Lines { get; }
    
    public char this[int index]
    {
        get
        {
            if (index > Length)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            return m_text[index];
        }
    }
    
    private static IReadOnlyList<TextLine> ParseLines(in SourceText sourceText, string text)
    {
        var result = new List<TextLine>();

        var position = 0;
        var lineStart = 0;

        while (position < text.Length)
        {
            var lineBreakWidth = GetLineBreakWidth(text, position);
            if (lineBreakWidth == 0)
            {
                position++;
            }
            else
            {
                AddLine(result, sourceText, position, lineStart, lineBreakWidth);
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

    private static void AddLine(ICollection<TextLine> result, SourceText sourceText, int position, int lineStart, int lineBreakWidth)
    {
        var lineLength = position - lineStart;
        var lineLengthIncludingLineBreak = lineLength + lineBreakWidth;
        var line = new TextLine(sourceText, lineStart, lineLength, lineLengthIncludingLineBreak);
        result.Add(line);
    }
    
    private static int GetLineBreakWidth(in string text, int position)
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
    
    public int GetLineIndex(int position)
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
    
    public static SourceText From(string text, string fileName = "null")
    {
        return new SourceText(text, fileName);
    }
    
    public string Substring(int startIndex, int length)
    {
        return m_text.Substring(startIndex, length);
    }

    public string ToString(TextSpan span)
    {
        return Substring(span.Start, span.Length);
    }
    
    // Overrides
    public override string ToString()
    {
        return m_text;
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

        return string.Compare(m_text, obj as string, StringComparison.CurrentCulture);
    }

    public object Clone()
    {
        return new SourceText(new string(m_text), FileName);
    }

    public TypeCode GetTypeCode()
    {
        return TypeCode.String;
    }

    public bool ToBoolean(IFormatProvider? provider)
    {
        return Convert.ToBoolean(m_text, provider);
    }

    public byte ToByte(IFormatProvider? provider)
    {
        return Convert.ToByte(m_text, provider);
    }

    public char ToChar(IFormatProvider? provider)
    {
        return Convert.ToChar(m_text, provider);
    }

    public DateTime ToDateTime(IFormatProvider? provider)
    {
        return Convert.ToDateTime(m_text, provider);
    }

    public decimal ToDecimal(IFormatProvider? provider)
    {
        return Convert.ToDecimal(m_text, provider);
    }

    public double ToDouble(IFormatProvider? provider)
    {
        return Convert.ToDouble(m_text, provider);
    }

    public short ToInt16(IFormatProvider? provider)
    {
        return Convert.ToInt16(m_text, provider);
    }

    public int ToInt32(IFormatProvider? provider)
    {
        return Convert.ToInt32(m_text, provider);
    }

    public long ToInt64(IFormatProvider? provider)
    {
        return Convert.ToInt64(m_text, provider);
    }

    public sbyte ToSByte(IFormatProvider? provider)
    {
        return Convert.ToSByte(m_text, provider);
    }

    public float ToSingle(IFormatProvider? provider)
    {
        return Convert.ToSingle(m_text, provider);
    }

    public string ToString(IFormatProvider? provider)
    {
        return m_text;
    }

    public object ToType(Type conversionType, IFormatProvider? provider)
    {
        return Convert.ChangeType(m_text, conversionType, provider);
    }

    public ushort ToUInt16(IFormatProvider? provider)
    {
        return Convert.ToUInt16(m_text, provider);
    }

    public uint ToUInt32(IFormatProvider? provider)
    {
        return Convert.ToUInt32(m_text, provider);
    }

    public ulong ToUInt64(IFormatProvider? provider)
    {
        return Convert.ToUInt64(m_text, provider);
    }

    public IEnumerator<char> GetEnumerator()
    {
        return m_text.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int CompareTo(SourceText? other)
    {
        return string.CompareOrdinal(m_text, other?.m_text);
    }

    public bool Equals(SourceText? other)
    {
        return m_text == other?.m_text &&
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

        return (source.m_text, source.Length, source.FileName, source.Lines)
            .Equals((source.m_text, source.Length, source.FileName, source.Lines));
    }

    public override int GetHashCode()
    {
        return (m_text, Length, FileName, Lines).GetHashCode();
    }
}