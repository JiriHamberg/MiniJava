using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MiniJava
{
	public static class TestHelper
	{
		public static List<LexemeCategory> GetLexemeCategories (string program)
		{
			var lexer = new Lexer (new StringReader (program));
			var categories = lexer.Select (lex => lex.Category);
			return new List<LexemeCategory> (categories);
		}
	}
}

