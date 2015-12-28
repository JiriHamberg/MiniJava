using System;

namespace MiniJava
{
	public abstract class Error
	{
		public readonly string Message;
		public readonly Lexeme Near;
		public readonly int? Line;
		public readonly int? Column;


		protected Error(string message, Lexeme near)
		{
			this.Message = message;
			this.Near = near;
			this.Line = null;
			this.Column = null;
		}

		protected Error(string message, int line, int column)
		{
			this.Message = message;
			this.Line = line;
			this.Column = column;
		}

	}

	public class LexerError : Error
	{
		public LexerError(string message, int Line, int Column) : base(message, Line, Column)
		{
		}
	}

	public class ParseError : Error
	{
		public ParseError(string message, int Line, int Column) : base(message, Line, Column)
		{
		}
	}

	//public class SemanticError : Error 
	//{
	//}

	//public class ErrorContainer : 

}

