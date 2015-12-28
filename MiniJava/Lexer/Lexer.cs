using System;
using System.IO;
using System.Collections.Generic;

namespace MiniJava
{
	public class Lexer : IEnumerable<Lexeme>
	{
		const int EndOfStream = -1;

		static Dictionary<string, LexemeCategory> simpleLexemes = new Dictionary<string, LexemeCategory>()
		{
			{"(", LexemeCategory.LBracket},
			{")", LexemeCategory.RBracket},
			{"[", LexemeCategory.LSquareBracket},
			{"]", LexemeCategory.RSquareBracket},
			{"{", LexemeCategory.LCurlyBracket},
			{"}", LexemeCategory.RCurlyBracket},
			{".", LexemeCategory.Dot},
			{",", LexemeCategory.Comma},
			{";", LexemeCategory.Semicolon},
			{"+", LexemeCategory.ADD},
			{"-", LexemeCategory.SUB},
			{"*", LexemeCategory.MUL},
			{"%", LexemeCategory.MOD},
			{"<", LexemeCategory.LT},
			{">", LexemeCategory.GT},
			{"!", LexemeCategory.NOT}

			/*{Operators.LESS, Category.Binary_Operator},
			{"=", Category.Binary_Operator},
			{Operators.AND, Category.Binary_Operator},
			{Operators.ADDITION, Category.Binary_Operator},
			{Operators.SUBSTRACTION, Category.Binary_Operator},
			{Operators.MULTIPLICATION, Category.Binary_Operator},
			//{Operators.DIVISION, Category.Binary_Operator},
			{Operators.NOT, Category.Unary_Operator},
			{";", Category.Semicolon}*/
		};

		static Dictionary<string, LexemeCategory> reservedWords = new Dictionary<string, LexemeCategory>()
		{
			{"while", LexemeCategory.While},
			{"assert", LexemeCategory.Assert},
			{"if", LexemeCategory.If},
			{"else", LexemeCategory.Else},
			{"return", LexemeCategory.Return},
			{"class", LexemeCategory.Class},
			{"public", LexemeCategory.Public},
			{"static", LexemeCategory.Static},
			{"extends", LexemeCategory.Extends},
			{"this", LexemeCategory.This},
			{"new", LexemeCategory.New},
			{"length", LexemeCategory.Length},
			{"int", LexemeCategory.TypeInt},
			{"boolean", LexemeCategory.TypeBoolean},
			{"void", LexemeCategory.Void},
			{"true", LexemeCategory.LiteralBoolean},
			{"false", LexemeCategory.LiteralBoolean},
			{"System", LexemeCategory.System},
			{"out", LexemeCategory.Out},
			{"println", LexemeCategory.Println},
			{"||", LexemeCategory.OR},
			{"&&", LexemeCategory.AND}
		};

		//list of transitions in order of presedence
		List<KeyValuePair<Func<char, bool>, Action>> transitionTable;
		List<LexerError> errors = new List<LexerError>();

		Queue<Lexeme> tokenBuffer = new Queue<Lexeme>();
		TextReader charStream;

		char current;
		int line = 1;
		int column = 1;
		string lexemeBody;
		LexemeCategory category = LexemeCategory.NONE;
		int lexemeBeginLine = -1;
		int lexemeBeginColumn = -1;

		/*
		 * Public methods
		 */

		public IEnumerator<Lexeme> GetEnumerator ()
		{
			while (true) {
				var next = Next ();
				yield return next;
				if (next.Category == LexemeCategory.EOF) break;
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public Lexer (TextReader charStream)
		{
			this.transitionTable = new List<KeyValuePair<Func<char, bool>, Action>> ()
			{
				//simple one char lexemes
				Transition (
					c => simpleLexemes.ContainsKey(c.ToString()),
					() => {category = simpleLexemes[current.ToString ()]; }), //simpleLexemes.TryGetValue(current.ToString(), out category) ),

				//identifier or reserved keyword
				Transition (
					Char.IsLetter,
					() => ReadWhile ( x => Char.IsLetterOrDigit(x) || x =='_')),    //x => !simpleLexemes.ContainsKey(x.ToString()) && x != '.' && !Char.IsWhiteSpace(x)) ),

				//integer literal
				Transition (
					Char.IsNumber,
					() => 
						{
							category = LexemeCategory.LiteralInt;
							ReadWhile (Char.IsDigit);
						}),

				//inline comment, multiline comment or division operator
				Transition (
					c => c == '/',
					() =>
						{
							if(PeekChar() == '/') {
								SkipWhile(x => x != '\n' );
								SkipBlank();
								ScanNextToken();
							} else if(PeekChar () == '*') {
								while(true) {
									SkipWhile(x => x != '*');
									if(charStream.Peek() == EndOfStream) {
										errors.Add(new LexerError("Unclosed multiline comment", lexemeBeginLine, lexemeBeginColumn));
										category = LexemeCategory.EOF;
										lexemeBody = "";
										return;
									}
									NextChar();
									if(PeekChar () == '/') {
										NextChar ();
										SkipBlank();
										break;
									}
								}
								ScanNextToken();
							} else {
								//lexeme += NextChar();
								category = LexemeCategory.DIV;
							}							
						}
					),

				Transition(
					c => c == '=',
					() => 
						{
							if (PeekChar () == '=') {
								lexemeBody += NextChar();
								category = LexemeCategory.EQ;
							} else {
								category = LexemeCategory.Assignment;
							}
						}	
				),
	
				Transition(
					c => c == '|',
					() => {
						//NextChar();
						if(PeekChar() == '|') {
							lexemeBody += NextChar();
							category = LexemeCategory.OR;
						}
					}
				),
				Transition(
					c => c == '&',
					() => {
						//NextChar();
						if(PeekChar() == '&') {
							lexemeBody += NextChar();
							category = LexemeCategory.AND;
						}
					}
				),
				

			};

			//this.errors = errContainer;
			this.charStream = charStream;
			SkipBlank ();
			while (charStream.Peek() != EndOfStream) {
				tokenBuffer.Enqueue(ScanNextToken());
			}
			tokenBuffer.Enqueue(new Lexeme(LexemeCategory.EOF, "", line, column));
		}

		public Lexeme Next ()
		{
			if (!HasNext ()) {
				throw new EndOfStreamException("Scanner has no more tokens");
			}
			return tokenBuffer.Dequeue();
		}

		public Lexeme Peek ()
		{
			if (!HasNext ()) {
				throw new EndOfStreamException("Scanner has no more tokens");
			}
			return tokenBuffer.Peek ();
		}

		public Boolean HasNext ()
		{
			return tokenBuffer.Count > 0;
		}

		/*
		 * Private methods
		 */ 

		Lexeme ScanNextToken ()
		{
			lexemeBody = "";
			category = LexemeCategory.NONE;
			lexemeBeginLine = -1;
			lexemeBeginColumn = -1;

			if (charStream.Peek () == EndOfStream) {
				throw new EndOfStreamException();
			}

			ReadLexeme ();
			SkipBlank();
			DecideCategory(); 

			/*switch (category) {
			case LexemeCategory.Identifier:
				return new Identifier (lexemeBody, lexemeBeginLine, lexemeBeginColumn);
			default:
				return new Lexeme (category, lexemeBody, lexemeBeginLine, lexemeBeginColumn);
			}*/
			return new Lexeme (category, lexemeBody, lexemeBeginLine, lexemeBeginColumn);
		}

		void ReadLexeme ()
		{
			if (charStream.Peek () == EndOfStream) {
				throw new EndOfStreamException ("Character stream ended unexpectedly");
			}

			current = NextChar ();
			lexemeBody += current;
			lexemeBeginColumn = column - 1;
			lexemeBeginLine = line;
			//find the action corresponding to the current character and invoke it
			var match = transitionTable.Find (kvp => kvp.Key (current));
			try {
				match.Value ();
			} catch (NullReferenceException ex) {
				errors.Add(new LexerError("Invalid input character", lexemeBeginLine, lexemeBeginColumn));
			}
		}


		void DecideCategory () 
		{
			if (category != LexemeCategory.NONE) { //already decided during readLexeme
				return;
			} else if (reservedWords.ContainsKey (lexemeBody)) {
				//reservedWords.TryGetValue (lexeme, out category);
				category = reservedWords [lexemeBody];
			} else if (IsValidIdentifier (lexemeBody)){ 
				category = LexemeCategory.Identifier;
			} else {
				//best guess is that this is a malformed identifier
				category = LexemeCategory.Identifier;
				errors.Add(new LexerError("Invalid identifier: " + lexemeBody, line, column));
			}
		}

		void SkipBlank ()
		{
			while (charStream.Peek() != EndOfStream) {
				if (IsBlank (PeekChar ())){
					NextChar ();
					continue;
				} else {
					break;
				}
			}
		}

		int PeekChar ()
		{
			if (charStream.Peek() == EndOfStream) {
				return EndOfStream;
			}
			return charStream.Peek();
		}

		char NextChar ()
		{
			char c = (char)charStream.Read();
			UpdateCursor (c);
			return c;
		}

		static bool IsBlank (int c)
		{
			return c == ' ' || c == '\t' || c == '\n';
		}

		static bool IsValidIdentifier (string s)
		{
			char[] chars = s.ToCharArray ();
			if (chars.Length < 1 || !Char.IsLetter (chars [0])) {
				return false;
			}
			for (int i = 1; i < chars.Length; i++) {
				char c = chars [i];
				if (!(Char.IsLetterOrDigit (c) || c == '_')) {
					return false;
				}
			}
			return true;
		}

		void ReadWhile (Func<char, bool> condition)
		{
			while(PeekChar() != EndOfStream && condition((char)PeekChar ())) {
				lexemeBody += NextChar ();
			}
		}

		void SkipWhile (Func<char, bool> condition)
		{
			while (PeekChar () != EndOfStream && condition((char)PeekChar ())) {
				NextChar ();
			}
		}

		void UpdateCursor (char c)
		{
			if (c == '\n') {
				line++;
				column = 1;
			} else {
				column++;
			}
		}

		//helper to hide nasty type-typing
		static KeyValuePair<Func<char, bool>, Action> Transition (Func<char, bool> condition, Action effect)
		{
			return new KeyValuePair<Func<char, bool>, Action> (condition, effect);
		}

	}
}

