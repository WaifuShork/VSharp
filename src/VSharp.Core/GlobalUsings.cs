global using System;
global using System.Collections;
global using JetBrains.Annotations;
global using VSharp.Core.Utilities;
global using VSharp.Core.Extensions;
global using VSharp.Core.Utilities.IO;
global using VSharp.Core.Configuration;
global using VSharp.Core.Analysis.Text;
global using System.Collections.Generic;
global using VSharp.Core.Analysis.Syntax;
global using VSharp.Core.Analysis.Lexing;
global using VSharp.Core.Analysis.Parsing;
global using VSharp.Core.Utilities.Caching;
global using VSharp.Core.Analysis.Diagnostics;
global using VSharp.Core.Analysis.Syntax.Statements;
global using VSharp.Core.Analysis.Syntax.Expressions;
global using VSharp.Core.Analysis.Syntax.Constraints;
global using VSharp.Core.Analysis.Syntax.Errors;
global using VSharp.Core.Analysis.Syntax.Declarations;

global using Operator = System.String; 
global using Error = System.String;
global using Any = System.String;
global using Keyword = System.String;
global using Delimiter = System.String;
global using Identifier = System.String;
global using Modifier = System.String;
global using UType = System.String;
global using Bracket = System.String;
global using Argument = System.String;
global using Parameter = System.String;

global using int8 = System.SByte;
global using uint8 = System.Byte;
global using int16 = System.Int16;
global using uint16 = System.UInt16;
global using int32 = System.Int32;
global using uint32 = System.UInt32;
global using int64 = System.Int64;
global using uint64 = System.UInt64;
global using infint = System.Numerics.BigInteger;

global using float32 = System.Single;
global using float64 = System.Double;
global using float128 = System.Decimal;

global using rune = System.Text.Rune;