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
			
			var program = @"class Foo {
				public static void main() {
					System.out.println(3);
					Bar b;
					b = new Bar();
					b.init(5);
					b.set(1, 100);
					System.out.println(b.get(1));
				}
			}
			
			class Bar {
				int[] a;
				
				public void init(int size) {
					a = new int[size];
				}
 				
				public void set(int index, int value) {
					a[index] = value;
				}
				boolean foo;
				public int get(int index) {
					return a[index];
				}
			}";

			var lex1 = new Lexer(new StringReader (program));
			lex1.ToList().ForEach (
				c => Console.WriteLine(c.Category + " " + c.Body)
			);

			var lexer = new Lexer (new StringReader (program));
			var parser = new Parser (lexer);
			parser.getNextLexeme ();

			var ast = parser.parseProgram ();

			//Console.WriteLine ("Alive");
			var pretty = new StringBuilder();
			ast.prettyPrint (pretty);
			Console.WriteLine (pretty);

			/*
			var main = @"public int[] foo(int b) {
							int [] c;
							c = new int[2 * b];
							assert( 5 - ((2 + b) * 8) == b + b );
							{
								return x || y && z;
								return a + b.foo(x, z, z);
							}
							A.foo(b - 1);
							//return a[b].foo().length;
							return A.foo(b - 1);
						}";

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
			*/

		}
	}
}
