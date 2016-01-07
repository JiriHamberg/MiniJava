using System;
using System.Collections.Generic;
using System.Linq;

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
		Comma,
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

		public static ISet<LexemeCategory> LeftBrackets = new HashSet<LexemeCategory>() {
			LexemeCategory.LBracket, LexemeCategory.LSquareBracket, LexemeCategory.LCurlyBracket
		};
		public static ISet<LexemeCategory> RightBrackets = new HashSet<LexemeCategory>() {
			LexemeCategory.RBracket, LexemeCategory.RSquareBracket, LexemeCategory.RCurlyBracket
		};
		public static ISet<LexemeCategory> BinaryOp = new HashSet<LexemeCategory>() {
			LexemeCategory.ADD,
			LexemeCategory.SUB,
			LexemeCategory.MUL,
			LexemeCategory.DIV,
			LexemeCategory.MOD,
			LexemeCategory.AND,
			LexemeCategory.OR,
			LexemeCategory.EQ,
			LexemeCategory.LT,
			LexemeCategory.GT
		};

		public static ISet<LexemeCategory> UnaryOp = new HashSet<LexemeCategory>() {
			LexemeCategory.NOT
		};
		public static ISet<LexemeCategory> ExpressionFirstSet = UnaryOp.Union(new HashSet<LexemeCategory>() {
			LexemeCategory.LBracket, LexemeCategory.This, LexemeCategory.LiteralBoolean, 
			LexemeCategory.LiteralInt, LexemeCategory.Identifier, LexemeCategory.New 
		}).ToHashSet();

	}


	/*public class Identifier : Lexeme {

		public Identifier(string body, int line, int column) : base(LexemeCategory.Identifier, body, line, column) 
		{
		}

	}*/



}

