using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace MiniJava
{
	[TestFixture()]
	public class ParserTests
	{

		[Test()]
		public void ParseEmptyClass ()
		{
			/*var main = @"class A extends B {				
							public void foo(int b) {
								return b * b;
							}
						}";*/

			var main = @"public int[] foo(int b) {
							return b * b;
						}";

			var lexer = new Lexer (new StringReader (main));
			var parser = new Parser (lexer);
			var declaration = parser.parseMethodDeclaration ();

			Console.WriteLine (declaration);

			Assert.That (true);
		}


	}
}

