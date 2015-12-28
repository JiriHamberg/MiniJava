using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniJava
{
	public class Parser
	{
		IEnumerator<Lexeme> lexemes;
		Lexeme current;
		Lexeme accepted;


		public Parser (IEnumerable<Lexeme> lexemes)
		{
			this.lexemes = lexemes.GetEnumerator ();
		}

		public void getNextLexeme()
		{
			lexemes.MoveNext ();
			try {
				current = lexemes.Current;
			} catch(System.InvalidOperationException) {
				//current is already EOF lexeme 				
			}
		}



		bool accept(IEnumerable<LexemeCategory> categories)
		{
			foreach (var category in categories) {
				if(category == current.Category) {
					accepted = current;
					getNextLexeme();
					return true;	
				}
			}
			//accepted = LexemeCategory.NONE;
			return false;
		}

		bool accept(params LexemeCategory[] categories)
		{
			return accept (categories.AsEnumerable());
		}


		bool expect(IEnumerable<LexemeCategory> categories)
		{
			if (accept (categories)) {
				return true;
			}
			addError (current, categories);
			return false;
		}
		bool expect(params LexemeCategory[] categories)
		{
			return expect (categories.AsEnumerable());
		}


		void addError(Lexeme found, IEnumerable<LexemeCategory> expected) 
		{
			string msg = "Expected " + string.Join(", ", expected.Select(c => c.ToString())) + " but " + found.Category + "(" + found.Body + ")"  + " was found.";
			/*foreach (var category in expected) {
				msg += category
			}*/
			throw new Exception (msg);
		}
			
		Identifier parseIdentifier()
		{
			expect (LexemeCategory.Identifier);
			return new Identifier (accepted);
		}

		ReturnType parseReturnType()
		{
			expect (LexemeCategory.TypeInt, LexemeCategory.TypeBoolean, LexemeCategory.Void, LexemeCategory.Identifier);
			var typeName = accepted;
			var isArray = false;
			if(accept(LexemeCategory.LSquareBracket)) {
				expect(LexemeCategory.RSquareBracket);
				isArray = true;
				Console.WriteLine ("IS AN ARRAY");
			}
			Console.WriteLine (isArray);
			return new ReturnType (typeName, isArray);
		}

		/*VariableDeclaration parseVariableDeclaration()
		{
			var type = parseReturnType ();
			var identifier = parseIdentifier ();
			expect (LexemeCategory.Semicolon);
			return new VariableDeclaration (type, identifier);
		}*/

		/* Consumes comma separated list of variables (possibly empty).
		 * The brackets are not consumed.
		 */
		IList<Variable> parseVariableList()
		{
			var vars = new List<Variable> ();
			while (Variable.FirstSet.Contains (current.Category)) {
				var type = parseReturnType ();
				var identifier = parseIdentifier ();
				vars.Add (new Variable (type, identifier));
				if(!(accept(LexemeCategory.Comma))) {
					break;
				}
			}
			return vars;

			/*while (accept (Variable.FirstSet)) {
				var type = new ReturnType (accepted);
				//var type = parseReturnType ();
				var identifier = parseIdentifier ();
				vars.Add(new Variable(type, identifier));
				if(!accept(LexemeCategory.Comma)) {
					break;						
				}
			}
			return vars;*/
		}

		public Statement parseStatement()
		{
			if (current.Category == LexemeCategory.LCurlyBracket) {
				return parseBlock ();
			}
			switch (current.Category) {
			case LexemeCategory.LCurlyBracket:
				return parseBlock ();
			case LexemeCategory.Return:
				return parseReturn ();
			case LexemeCategory.Assert:
				return parseAssert ();
			case LexemeCategory.TypeBoolean:
			case LexemeCategory.TypeInt:
				return parseVariableDeclaration ();
			//case LexemeCategory.Identifier:
				/*var name = current;
				getNextLexeme ();
				if (accept (LexemeCategory.Identifier)) {
					var variableName = accepted;
					expect (LexemeCategory.Semicolon);
					return new VariableDeclaration (name, variableName);
				}*/

				// this is a bit dirty, parse expression and see if next token is an identifier - then destruct the expression accordingly
				//var e = parseExpression ();


			default:
				throw new Exception ("Invalid lexeme: " + current.Category.ToString());
			}
		}

		Statement parseReturn()
		{
			expect(LexemeCategory.Return);
			var expr = parseExpression ();
			expect (LexemeCategory.Semicolon);
			return new Return (expr);
		}

		Statement parseBlock()
		{
			List<Statement> statements = new List<Statement> ();
			expect (LexemeCategory.LCurlyBracket);
			while (!accept (LexemeCategory.RCurlyBracket)) {
				statements.Add (parseStatement());
			}
			return new Block(statements);
		}

		Statement parseAssert()
		{
			expect (LexemeCategory.Assert);
			expect (LexemeCategory.LBracket);
			var assertion = parseExpression ();
			expect (LexemeCategory.RBracket);
			expect (LexemeCategory.Semicolon);
			return new AssertStatement (assertion);
		}

		Statement parseVariableDeclaration()
		{
			var type = parseReturnType ();
			var identifier = parseIdentifier ();
			expect (LexemeCategory.Semicolon);
			return new VariableDeclaration (type, identifier);
		}

		public MethodDeclaration parseMethodDeclaration()
		{
			var statements = new List<Statement> ();
			expect (LexemeCategory.Public);
			var type = parseReturnType ();
			var name = parseIdentifier ();
			expect (LexemeCategory.LBracket);
			var vars = parseVariableList ();
			expect (LexemeCategory.RBracket);
			expect (LexemeCategory.LCurlyBracket);

			while (!accept (LexemeCategory.RCurlyBracket)) {
				statements.Add (parseStatement ());
			}
			return new MethodDeclaration (type, name, vars, statements); 
		}


		/*** PRATT PARSING FOR EXPRESSIONS ***/

		IList<Expression> parseArguments()
		{
			var args = new List<Expression> ();
			expect (LexemeCategory.LBracket);
			if(accept(LexemeCategory.RBracket)) {
				return args;
			}
			do 
			{
				args.Add(parseExpression(0));
			} while (accept (LexemeCategory.Comma));
			expect(LexemeCategory.RBracket);
			return args;
		}

		/* Given an expression <head>, tries to extend that expression with a "tail"
		 * 		1. indexing:	<head> [<expr>]
		 *		2. method:		<head> "." <identifier> "(" [ <expr> ("," <expr>)* ] ")"
		 *		3. length:		<head> "." length
		 */
		Expression parseTail(Expression head)
		{
			if (accept (LexemeCategory.LSquareBracket)) {
				var index = parseExpression (0);
				expect (LexemeCategory.RSquareBracket);
				return parseTail(new ArrayAccess (head, index));
			} else if (accept (LexemeCategory.Dot)) {
				expect (LexemeCategory.Length, LexemeCategory.Identifier);
				switch (accepted.Category) {
					case LexemeCategory.Length:
						return parseTail(new ArrayLength (head));
					case LexemeCategory.Identifier:
						var methodName = new Identifier(accepted);
						var args = parseArguments ();
						return parseTail(new MethodInvocation (head, methodName, args));
					default:
						throw new Exception ("Invalid accepted token: " + accepted.Category.ToString());
				}
			} else {
				return head;
			}
		}

		Expression parsePrefix()
		{
			expect (Lexeme.ExpressionFirstSet);
			switch (accepted.Category) 
			{
			case LexemeCategory.NOT:
				var oper = accepted; 
				var operand = parsePrefix (); //parseExpression (0);
				return new UnaryOp (operand, oper); 
			case LexemeCategory.LBracket:
				var expr = parseExpression (0);
				expect (LexemeCategory.RBracket);
				return expr;
			case LexemeCategory.LiteralBoolean:
				return new LiteralBool (accepted);
			case LexemeCategory.LiteralInt:
				return new LiteralInt (accepted);
			case LexemeCategory.Identifier:
				return new LeafIdentifier (accepted);
			case LexemeCategory.New:
				if (accept (LexemeCategory.TypeBoolean, LexemeCategory.TypeInt)) {
					var typeName = new ReturnType(accepted, false);
					expect (LexemeCategory.LSquareBracket);
					var len = parseExpression (0);
					expect (LexemeCategory.RSquareBracket);
					return new NewArray (typeName, len);
				} else {
					expect (LexemeCategory.Identifier);
					var name = accepted;
					expect (LexemeCategory.LBracket, LexemeCategory.LSquareBracket);
					switch (accepted.Category) {
					case LexemeCategory.LBracket:
						expect (LexemeCategory.RBracket);
						return new NewObject (new Identifier(name));
					case LexemeCategory.LSquareBracket:
						var len = parseExpression (0);
						expect (LexemeCategory.RSquareBracket);
						return new NewArray (new ReturnType(name, false), len);
					default:
						throw new Exception ("Invalid lexeme was accepted: " + accepted.Category.ToString());
					}
				}
			default:
				throw new Exception ("Invalid lexeme was accepted: " + accepted.Category.ToString());
			}
		}
	

		public Expression parseExpression()
		{
			return parseExpression (0);
		}

		Expression parseExpression(int precedence)
		{
			Expression left = parseTail(parsePrefix ());
			//getNextLexeme ();
			//Console.WriteLine (precedence);

			while(Lexeme.BinaryOp.Contains(current.Category) && precedence < Precedence.GetPrecedence(current.Category))
			{
				var nextPrecedence = Precedence.GetPrecedence (current.Category);
				var oper = current;
				getNextLexeme ();
				//Console.WriteLine (accepted.Category);
				left = new BinaryOp (left, parseExpression (nextPrecedence), oper);
			}
			return left;
		}
			


	}
}

