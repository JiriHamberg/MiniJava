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
		Lexeme pushedBack;

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

		/* Push back a the currently accepted lexeme,
		 * which is convinient for lookahead purposes.
		 * Calling this without consumin lexemes 
		 * will result in a runtime error
		 * (calling this twice in a row for example).
		 */
		void pushBack()
		{
			if (accepted == null) {
				throw new InvalidProgramException ("There is no accepted lexeme to push back!");
			}
			//lexemes = getPushBackEnumerator ();
			//current = accepted;
			//accepted = null;
			pushedBack = accepted;
		}

		LexemeCategory lookAhead()
		{
			if (pushedBack != null) {
				return pushedBack.Category;
			} else {
				return current.Category;
			}
		}


		bool accept(IEnumerable<LexemeCategory> categories)
		{
			foreach (var category in categories) {
				//first try to consume the pushed back lexeme
				if (pushedBack != null && category == pushedBack.Category) {
					accepted = pushedBack;
					pushedBack = null;
					return true;
				} else if(category == current.Category) {
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
			string msg = "Expected " + string.Join(", ", expected.Select(c => c.ToString())) + " but " + found.Category + "(" + found.Body + ")"  + " was found near line " + found.Line + " column " + found.Column;
			/*foreach (var category in expected) {
				msg += category
			}*/
			throw new Exception (msg);
		}
			
		public Program parseProgram()
		{
			var mainClass = parseMainClass ();
			var classes = new List<ClassDeclaration> ();
			while (lookAhead() == LexemeCategory.Class) {
				classes.Add (parseClass ());
			}
			expect (LexemeCategory.EOF);
			return new Program (mainClass, classes);
		}

		public MainClassDeclaration parseMainClass()
		{
			expect (LexemeCategory.Class);
			var className = parseIdentifier ();
			expect (LexemeCategory.LCurlyBracket);
			var mainMethod = parseMainMethod ();
			expect (LexemeCategory.RCurlyBracket);
			return new MainClassDeclaration (className, mainMethod);
		}

		public ClassDeclaration parseClass()
		{
			Identifier parentClassName = null;
			expect (LexemeCategory.Class);
			var className = parseIdentifier ();
			if (accept (LexemeCategory.Extends)) {
				parentClassName = parseIdentifier();
			}
			expect (LexemeCategory.LCurlyBracket);
			var variables = new List<VariableDeclaration> ();
			var methods = new List<MethodDeclaration> ();
			while (!accept (LexemeCategory.RCurlyBracket)) {
				if (lookAhead () == LexemeCategory.Public) {
					methods.Add (parseMethodDeclaration());
				} else {
					variables.Add (parseVariableDeclaration());
				}
			}
			return new ClassDeclaration (className, parentClassName, variables, methods);
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
				//Console.WriteLine ("IS AN ARRAY");
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
			while (Variable.FirstSet.Contains (lookAhead())) {
				var type = parseReturnType ();
				var identifier = parseIdentifier ();
				vars.Add (new Variable (type, identifier));
				if(!(accept(LexemeCategory.Comma))) {
					break;
				}
			}
			return vars;
		}
			

		public Statement parseStatement()
		{
			if (lookAhead() == LexemeCategory.LCurlyBracket) {
				return parseBlock ();
			}
			switch (lookAhead()) {
			case LexemeCategory.LCurlyBracket:
				return parseBlock ();
			case LexemeCategory.Return:
				return parseReturn ();
			case LexemeCategory.Assert:
				return parseAssert ();
			case LexemeCategory.System:
				return parsePrintStatement ();
			case LexemeCategory.TypeBoolean:
			case LexemeCategory.TypeInt:
				return parseVariableDeclaration ();
			//case LexemeCategory.This:
			//	return parseMethodInvocationOrAssignment (parseExpression ());
				/*var expression = parseExpression ();
				if (accept (LexemeCategory.Assignment)) {
					var rvalue = parseExpression ();
					expect (LexemeCategory.Semicolon);
					return new Assignment (expression, rvalue);
				} else {
					if (expression is MethodInvocation) {
						var invocation = new MethodInvocationStatement ((MethodInvocation)expression);
						expect (LexemeCategory.Semicolon);
						return invocation;
					} else {
						//TODO replace with adding error + recovery, this is just a syntax error
						Console.WriteLine("current: " + lookAhead());
						Console.WriteLine("accepted: " + accepted.Category);
						throw new Exception("Expression " + expression + " near line" + current.Line + ", column " + current.Column + " is not a valid statemnt");
					}
				}*/
			case LexemeCategory.Identifier:
				expect (LexemeCategory.Identifier);
				if (lookAhead() == LexemeCategory.Identifier) {
					pushBack ();
					return parseVariableDeclaration ();
				} else {
					//Console.WriteLine ("before pushback: " + current.Category);
					pushBack ();
					//Console.WriteLine ("after pushback: " + current.Category);
					/*var expression = parseExpression ();
					if (accept (LexemeCategory.Assignment)) {
						var rvalue = parseExpression ();
						expect (LexemeCategory.Semicolon);
						return new Assignment (expression, rvalue);
					} else {
						if (expression is MethodInvocation) {
							var invocation = new MethodInvocationStatement ((MethodInvocation)expression);
							expect (LexemeCategory.Semicolon);
							return invocation;
						} else {
							//TODO replace with adding error + recovery, this is just a syntax error
							Console.WriteLine("current: " + lookAhead());
							Console.WriteLine("accepted: " + accepted.Category);
							throw new Exception("Expression " + expression + " near line" + current.Line + ", column " + current.Column + " is not a valid statemnt");
						}*/
					return parseMethodInvocationOrAssignment (parseExpression ());
					}

			default:
				throw new Exception ("Invalid lexeme: " + lookAhead().ToString());
			}
		}

		Statement parseMethodInvocationOrAssignment(Expression left)
		{
			if (accept (LexemeCategory.Assignment)) {
				var rvalue = parseExpression ();
				expect (LexemeCategory.Semicolon);
				return new Assignment (left, rvalue);
			} else {
				if (left is MethodInvocation) {
					var invocation = new MethodInvocationStatement ((MethodInvocation)left);
					expect (LexemeCategory.Semicolon);
					return invocation;
				} else {
					//TODO replace with adding error + recovery, this is just a syntax error
					Console.WriteLine("current: " + lookAhead());
					Console.WriteLine("accepted: " + accepted.Category);
					throw new Exception("Expression " + left + " near line" + current.Line + ", column " + current.Column + " is not a valid statemnt");
				}
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

		Statement parsePrintStatement()
		{
			expect (LexemeCategory.System);
			expect (LexemeCategory.Dot);
			expect (LexemeCategory.Out);
			expect (LexemeCategory.Dot);
			expect (LexemeCategory.Println);
			expect (LexemeCategory.LBracket);
			var toPrint = parseExpression ();
			expect (LexemeCategory.RBracket);
			expect (LexemeCategory.Semicolon);
			return new PrintStatement (toPrint);
		}

		VariableDeclaration parseVariableDeclaration()
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

		public MethodDeclaration parseMainMethod()
		{
			var statements = new List<Statement> ();
			expect (LexemeCategory.Public);
			expect (LexemeCategory.Static);
			var type = parseReturnType ();
			var name = parseIdentifier ();
			expect (LexemeCategory.LBracket);
			expect (LexemeCategory.RBracket);
			expect (LexemeCategory.LCurlyBracket);

			while (!accept (LexemeCategory.RCurlyBracket)) {
				statements.Add (parseStatement ());
			}
			return new MethodDeclaration (type, name, new List<Variable>(), statements);
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
			case LexemeCategory.This:
				return new This ();
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
	
		/* Public hook for the expression parser.
		 * 
		 */
		public Expression parseExpression()
		{
			return parseExpression (0);
		}

		/* Start Pratt parsing from scratch
		 * 
		 */
		Expression parseExpression(int precedence)
		{
			Expression left = parseTail(parsePrefix ());
			return parseExpression (precedence, left);
		}

		/* Start Pratt parsing with an existing prefix expression
		 * expecting a binary operator. 
		 */
		Expression parseExpression(int precedence, Expression left)
		{
			while(Lexeme.BinaryOp.Contains(lookAhead()) && precedence < Precedence.GetPrecedence(lookAhead()))
			{
				var nextPrecedence = Precedence.GetPrecedence (lookAhead());
				var oper = current;
				getNextLexeme ();
				//Console.WriteLine (accepted.Category);
				left = new BinaryOp (left, parseExpression (nextPrecedence), oper);
			}
			return left;
		}
			


	}
}

