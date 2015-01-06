using System;

namespace MiniJava
{

	public enum LexemeCategory
	{
		Identifier,

		//literals
		LiteralInt,
		LiteralBoolean,

		//both type (and a value? [in what context??]) 
		Void,

		TypeInt,
		TypeBoolean,

 		Class,
		Extends,
		Public,
		Static,
		This,
		New,
		Length, 

		//artificial sout construct
		System,
		Out,
		Println,

		//binary operators
		ADD,
		SUB,
		MUL,
		DIV,
		MOD,
		AND,
		OR,
		EQ,
		LT,
		GT,

		//unary operators
		NOT,

		Assignment,

		If,
		Else,
		While,
		Assert,
		Return,

		//misc
		LBracket,
		RBracket,
		LSquareBracket,
		RSquareBracket,
		LCurlyBracket,
		RCurlyBracket,
		Colon,
		Semicolon,
		Dot,

		//End of file
		EOF,

		//Error 
		NONE
	}

	public class Lexeme
	{
		public readonly LexemeCategory Category;
		public readonly string Body;
		public readonly int Line;
		public readonly int Column;

		public Lexeme (LexemeCategory category, string body, int line, int column)
		{
			this.Category = category;
			this.Body = body;
			this.Line = line;
			this.Column = column;
		}
	}
}

