using System;
using System.IO;
using System.Text;
using System.Linq;

namespace MiniJava
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			//Console.WriteLine ("Hello World!");

			var main = @"public int[] foo(int b) {
							int [] c;
							c = new int[2 * b];
							assert( 2 * b == b + b );
							{
								return x || y && z;
								return a + b.foo(x, z, z);
							}
							
							return a[b].foo().length;
						}";

			//var cs = TestHelper.GetLexemeCategories (main);
			//cs.ForEach (c => Console.WriteLine(c));
			var lex1 = new Lexer(new StringReader (main));
			lex1.ToList().ForEach (
				c => Console.WriteLine(c.Category + " " + c.Body)
			);

			var lexer = new Lexer (new StringReader (main));
			var parser = new Parser (lexer);
			parser.getNextLexeme ();

			var declaration = parser.parseMethodDeclaration ();

			//Console.WriteLine ("Alive");
			var pretty = new StringBuilder();
			declaration.prettyPrint (pretty);
			Console.WriteLine (pretty);


		}
	}
}
