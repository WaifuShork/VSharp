using System.Globalization;
using JetBrains.Annotations;
using VSharp.Core.Analysis.Text;

namespace VSharp.Core.Extensions;

public static class SourceTextExtensions
{
	[ContractAnnotation("text:null => true")]
	public static bool IsNullOrWhiteSpace(this SourceText? text)
	{
		if (text is null)
		{
			return true;
		}

		return string.IsNullOrWhiteSpace(text.ToString(CultureInfo.CurrentCulture));
	}
}