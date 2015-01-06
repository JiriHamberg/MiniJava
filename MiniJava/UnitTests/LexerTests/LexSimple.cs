using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace MiniJava
{
	[TestFixture()]
	public class LexerTests
	{

		[Test()]
		public void LexHelloWorld ()
		{
			var main = @"public static void main()
 						{
							System.out.println(1);
						}";
			var correct = new List<LexemeCategory> () {
				LexemeCategory.Public,
				LexemeCategory.Static,
				LexemeCategory.Void,
				LexemeCategory.Identifier,
				LexemeCategory.LBracket,
				LexemeCategory.RBracket,
				LexemeCategory.LCurlyBracket,
				LexemeCategory.System,
				LexemeCategory.Dot,
				LexemeCategory.Out,
				LexemeCategory.Dot,
				LexemeCategory.Println,
				LexemeCategory.LBracket,
				LexemeCategory.LiteralInt,
				LexemeCategory.RBracket,
				LexemeCategory.Semicolon,
				LexemeCategory.RCurlyBracket,
				LexemeCategory.EOF
			};
			var lexemes = TestHelper.GetLexemeCategories(main);
			Assert.That (lexemes, Is.EquivalentTo (correct));
		}

		[Test()]
		public void LexClass() 
		{
			var main = @"class Foo extends Bar 
						{
							//do not lex me
							int i_am_cool_variable_99;
							boolean i_am_cool_variable_100 = false;
							
							/* I am
							 * not going
							 * to be lexed.
							 */
							public void do_fancy_stuff(boolean i_am_cool_param_100)
							{
								SomeType[] local_var_1 = new SomeType[1+3];								
							}
	
						}";
			var correct = new List<LexemeCategory> () {
				LexemeCategory.Class,
				LexemeCategory.Identifier,
				LexemeCategory.Extends,
				LexemeCategory.Identifier,
				LexemeCategory.LCurlyBracket,
					LexemeCategory.TypeInt,
					LexemeCategory.Identifier,
					LexemeCategory.Semicolon,
					
					LexemeCategory.TypeBoolean,	
					LexemeCategory.Identifier,
					LexemeCategory.Assignment,
					LexemeCategory.LiteralBoolean,
					LexemeCategory.Semicolon,

					LexemeCategory.Public,
					LexemeCategory.Void,
					LexemeCategory.Identifier,
					LexemeCategory.LBracket,
						LexemeCategory.TypeBoolean,
						LexemeCategory.Identifier,
					LexemeCategory.RBracket,
					LexemeCategory.LCurlyBracket,
						LexemeCategory.Identifier,
						LexemeCategory.LSquareBracket,
						LexemeCategory.RSquareBracket,
						LexemeCategory.Identifier,
						LexemeCategory.Assignment,
						LexemeCategory.New,
						LexemeCategory.Identifier,
						LexemeCategory.LSquareBracket,
							LexemeCategory.LiteralInt,
							LexemeCategory.ADD,
							LexemeCategory.LiteralInt,
						LexemeCategory.RSquareBracket,
						LexemeCategory.Semicolon,
					LexemeCategory.RCurlyBracket,
				LexemeCategory.RCurlyBracket,
				LexemeCategory.EOF
			};
			var lexemes = TestHelper.GetLexemeCategories(main);
			//Assert.That (lexemes, Is.EquivalentTo (correct));
			for (int i=0; i<lexemes.Count; ++i) {
				Assert.AreEqual(correct[i], lexemes[i], "At lexeme " + (i+1));
			}

		}

	}
}

