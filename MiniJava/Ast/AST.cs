using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniJava
{

	public abstract class AST
	{
		public abstract ISet<LexemeCategory> First { get; }
		public abstract ISet<LexemeCategory> Follow { get; }

		public abstract void prettyPrint (StringBuilder s);
	}

	public class ReturnType : AST 
	{
		public static readonly ISet<LexemeCategory> FirstSet = new HashSet<LexemeCategory>() {
			LexemeCategory.TypeBoolean, LexemeCategory.TypeInt, LexemeCategory.Identifier 
		};
		public static readonly ISet<LexemeCategory> FollowSet = new HashSet<LexemeCategory>() {
			LexemeCategory.Identifier 
		};

		public override ISet<LexemeCategory> First { get { return FirstSet; } }
		public override ISet<LexemeCategory> Follow { get { return FollowSet; } }

		public readonly Lexeme type;
		public readonly bool isArray;

		public ReturnType(Lexeme type, bool isArray)
		{
			this.type = type;
			this.isArray = isArray;
		}

		public override void prettyPrint(StringBuilder s)
		{
			s.AppendFormat("{0}", type.Body);
			if (isArray) {
				Console.WriteLine ("THIS IS ARRAY");
				s.Append ("[]");
			}
		}
	}

	public class Identifier : AST 
	{
		public static readonly ISet<LexemeCategory> FirstSet = new HashSet<LexemeCategory>() {
			LexemeCategory.Identifier 
		};
		public static readonly ISet<LexemeCategory> FollowSet = Lexeme.LeftBrackets.Union(Lexeme.BinaryOp).Union(
			new HashSet<LexemeCategory>() { LexemeCategory.Semicolon, LexemeCategory.Comma }
		).ToHashSet();

		public override ISet<LexemeCategory> First { get { return FirstSet; } }
		public override ISet<LexemeCategory> Follow { get { return FollowSet; } }

		public readonly Lexeme identifier;

		public Identifier(Lexeme identifier)
		{
			this.identifier = identifier;
		}

		public override void prettyPrint(StringBuilder s)
		{
			s.AppendFormat("{0}", identifier.Body);
		}
	}


	public class ClassDeclaration : AST
	{

		public static readonly ISet<LexemeCategory> FirstSet = new HashSet<LexemeCategory>() {
			LexemeCategory.Class 
		};
		public static readonly ISet<LexemeCategory> FollowSet = new HashSet<LexemeCategory>() {
			LexemeCategory.Class	
		};

		public override ISet<LexemeCategory> First { get { return FirstSet; } }
		public override ISet<LexemeCategory> Follow { get { return FollowSet; } }

		public readonly Identifier className;
		public readonly IList<Statement> statements;
		public readonly IList<VariableDeclaration> variables;
		public readonly IList<MethodDeclaration> methods;


		public ClassDeclaration(Identifier className, IList<Statement> statements, Identifier parentClass, IList<VariableDeclaration> variables, IList<MethodDeclaration> methods) {
			this.className = className;
			this.statements = statements;
			this.variables = variables;
			this.methods = methods;
		}

		public override void prettyPrint(StringBuilder s)
		{
			s.AppendFormat ("class {0}", className); 
			foreach (var v in variables) {
				//s.Append ("\t");
				v.prettyPrint (s);
				s.Append ("\n");
			}
			foreach (var m in methods) {
				m.prettyPrint (s);
				s.Append ("\n");
			}
			s.Append("\n");
		}

	}

	public abstract class Statement : AST
	{
	}

	public class Block : Statement
	{

		public static readonly ISet<LexemeCategory> FirstSet = new HashSet<LexemeCategory>() {LexemeCategory.LCurlyBracket};
		public static readonly ISet<LexemeCategory> FollowSet = new HashSet<LexemeCategory>() {
			//STATEMENT FOLLOW SET	
		};

		public override ISet<LexemeCategory> First { get { return FirstSet; } }
		public override ISet<LexemeCategory> Follow { get { return FollowSet; } }

		public readonly IList<Statement> statements;

		public Block(IList<Statement> statements)
		{
			this.statements = statements;
		}

		public override void prettyPrint(StringBuilder s)
		{
			s.Append ("{\n");
			foreach (var stm in statements) {
				s.Append ("\t");
				stm.prettyPrint (s);
				s.Append ("\n");
			}
			s.Append ("}");
		}

	}


	public class AssertStatement : Statement
	{
		public static readonly ISet<LexemeCategory> FirstSet = new HashSet<LexemeCategory>() {LexemeCategory.Assert};
		public static readonly ISet<LexemeCategory> FollowSet = new HashSet<LexemeCategory>() {
			//STATEMENT FOLLOW SET	
		};

		public override ISet<LexemeCategory> First { get { return FirstSet; } }
		public override ISet<LexemeCategory> Follow { get { return FollowSet; } }

		public readonly Expression assertion;

		public AssertStatement(Expression assertion)
		{
			this.assertion = assertion;
		}

		public override void prettyPrint(StringBuilder s)
		{
			s.Append ("assert ( ");
			assertion.prettyPrint (s);
			s.Append (" )");
		}
	}

	public class Return : Statement
	{
		public static readonly ISet<LexemeCategory> FirstSet = new HashSet<LexemeCategory>() {LexemeCategory.Return};
		public static readonly ISet<LexemeCategory> FollowSet = new HashSet<LexemeCategory>() {
			//STATEMENT FOLLOW SET	
		};

		public override ISet<LexemeCategory> First { get { return FirstSet; } }
		public override ISet<LexemeCategory> Follow { get { return FollowSet; } }

		public readonly Expression toReturn;

		public Return(Expression toReturn)
		{
			this.toReturn = toReturn;
		}

		public override void prettyPrint(StringBuilder s)
		{
			s.Append("return ");
			toReturn.prettyPrint (s);
		}
	}


	public class VariableDeclaration : Statement
	{

		public static readonly ISet<LexemeCategory> FirstSet = ReturnType.FirstSet;
		public static readonly ISet<LexemeCategory> FollowSet = new HashSet<LexemeCategory>() {
			//STATEMENT FOLLOW SET	
		};

		public override ISet<LexemeCategory> First { get { return FirstSet; } }
		public override ISet<LexemeCategory> Follow { get { return FollowSet; } }

		public readonly ReturnType type;
		public readonly Identifier identifier;


		public VariableDeclaration(ReturnType type, Identifier identifier)
		{
			this.type = type;
			this.identifier = identifier;
		}

		public override void prettyPrint(StringBuilder s)
		{
			type.prettyPrint (s);
			s.Append (" ");
			identifier.prettyPrint (s);
		}
	} 

	public class Variable : AST 
	{

		public static readonly ISet<LexemeCategory> FirstSet = ReturnType.FirstSet;
		public static readonly ISet<LexemeCategory> FollowSet = new HashSet<LexemeCategory>() {
			LexemeCategory.Comma, LexemeCategory.RBracket	
		};

		public override ISet<LexemeCategory> First { get { return FirstSet; } }
		public override ISet<LexemeCategory> Follow { get { return FollowSet; } }

		public readonly ReturnType type;
		public readonly Identifier identifier;


		public Variable(ReturnType type, Identifier identifier)
		{
			this.type = type;
			this.identifier = identifier;
		}

		public override void prettyPrint(StringBuilder s)
		{
			type.prettyPrint (s);
			s.Append (" ");
			identifier.prettyPrint (s);
		}
	}


	public class MethodDeclaration : AST
	{
		public static readonly ISet<LexemeCategory> FirstSet = new HashSet<LexemeCategory>() {
			LexemeCategory.Public 
		};
		public static readonly ISet<LexemeCategory> FollowSet = VariableDeclaration.FirstSet.Union(new HashSet<LexemeCategory>() {
			LexemeCategory.Public	
		}).ToHashSet();

		public override ISet<LexemeCategory> First { get { return FirstSet; } }
		public override ISet<LexemeCategory> Follow { get { return FollowSet; } }

		public readonly ReturnType returnType;
		public readonly Identifier identifier;
		public readonly IList<Variable> variables;
		public readonly IList<Statement> body;

		public MethodDeclaration(ReturnType returnType, Identifier identifier, IList<Variable> variables, IList<Statement> body)
		{
			this.returnType = returnType;
			this.identifier = identifier;
			this.variables = variables;
			this.body = body;
		}

		public override void prettyPrint(StringBuilder s)
		{
			identifier.prettyPrint (s);

			s.Append (" :: (");
			foreach (var v in variables) {
				v.prettyPrint (s);
			}
			s.Append (")");
			s.Append (" -> "); 
			returnType.prettyPrint (s);
			s.Append ("\n");
			foreach(var stm in body) {
				s.Append ("\t");
				stm.prettyPrint (s);
				s.Append ("\n");
			}
		}
	}


	// expressions


	public abstract class Expression : AST
	{
	}


	public class BinaryOp : Expression
	{
		public static readonly ISet<LexemeCategory> FirstSet = Lexeme.UnaryOp.Union(new HashSet<LexemeCategory>() {
			LexemeCategory.LBracket, LexemeCategory.This, LexemeCategory.LiteralBoolean, LexemeCategory.Identifier 
		}).ToHashSet();
		public static readonly ISet<LexemeCategory> FollowSet = Lexeme.BinaryOp.Union(new HashSet<LexemeCategory> () {
			LexemeCategory.LSquareBracket, LexemeCategory.RBracket, LexemeCategory.Assignment, 
			LexemeCategory.Semicolon, LexemeCategory.Comma
		}).ToHashSet();

		public override ISet<LexemeCategory> First { get { return FirstSet; } }
		public override ISet<LexemeCategory> Follow { get { return FollowSet; } }

		public readonly Expression leftOperand;
		public readonly Expression rightOperand;
		public readonly Lexeme oper;

		public BinaryOp(Expression leftOperand, Expression rightOperand, Lexeme oper)
		{
			this.leftOperand = leftOperand;
			this.rightOperand = rightOperand;
			this.oper = oper;
		}

		public override void prettyPrint(StringBuilder s)
		{
			s.Append ("(");
			leftOperand.prettyPrint (s);
			s.Append (oper.Body);
			rightOperand.prettyPrint (s);
			s.Append (")");
		}
	}

	public class UnaryOp : Expression
	{
		public static readonly ISet<LexemeCategory> FirstSet = BinaryOp.FirstSet;
		public static readonly ISet<LexemeCategory> FollowSet = BinaryOp.FollowSet;

		public override ISet<LexemeCategory> First { get { return FirstSet; } }
		public override ISet<LexemeCategory> Follow { get { return FollowSet; } }

		public readonly Expression operand;
		public readonly Lexeme oper;

		public UnaryOp(Expression operand, Lexeme oper)
		{
			this.operand = operand;
			this.oper = oper;
		}

		public override void prettyPrint(StringBuilder s)
		{
			s.Append ("(");
			s.Append (oper.Body);
			operand.prettyPrint (s);
			s.Append (")");
		}
	}

	public class ArrayAccess : Expression
	{
		public static readonly ISet<LexemeCategory> FirstSet = BinaryOp.FirstSet;
		public static readonly ISet<LexemeCategory> FollowSet = BinaryOp.FollowSet;

		public override ISet<LexemeCategory> First { get { return FirstSet; } }
		public override ISet<LexemeCategory> Follow { get { return FollowSet; } }

		public readonly Expression array;
		public readonly Expression index;

		public ArrayAccess(Expression array, Expression index)
		{
			this.array = array;
			this.index = index;
		}

		public override void prettyPrint(StringBuilder s)
		{
			array.prettyPrint (s);
			s.Append ("[");
			index.prettyPrint (s);
			s.Append ("]");
		}
	}

	public class ArrayLength : Expression
	{
		public static readonly ISet<LexemeCategory> FirstSet = BinaryOp.FirstSet;
		public static readonly ISet<LexemeCategory> FollowSet = BinaryOp.FollowSet;

		public override ISet<LexemeCategory> First { get { return FirstSet; } }
		public override ISet<LexemeCategory> Follow { get { return FollowSet; } }


		public readonly Expression array;

		public ArrayLength(Expression array)
		{
			this.array = array;
		}

		public override void prettyPrint(StringBuilder s)
		{
			array.prettyPrint (s);
			s.Append (".length");
		}
	}

	class NewObject : Expression
	{
		public static readonly ISet<LexemeCategory> FirstSet = BinaryOp.FirstSet;
		public static readonly ISet<LexemeCategory> FollowSet = BinaryOp.FollowSet;

		public override ISet<LexemeCategory> First { get { return FirstSet; } }
		public override ISet<LexemeCategory> Follow { get { return FollowSet; } }

		public readonly Identifier type;

		public NewObject(Identifier type)
		{
			this.type = type;
		}

		public override void prettyPrint(StringBuilder s)
		{
			s.AppendFormat("new ");
			type.prettyPrint (s);
			s.Append ("()");
		}
	}

	public class NewArray : Expression
	{
		public static readonly ISet<LexemeCategory> FirstSet = BinaryOp.FirstSet;
		public static readonly ISet<LexemeCategory> FollowSet = BinaryOp.FollowSet;

		public override ISet<LexemeCategory> First { get { return FirstSet; } }
		public override ISet<LexemeCategory> Follow { get { return FollowSet; } }

		public readonly ReturnType type;
		public readonly Expression length;

		public NewArray(ReturnType type, Expression length)
		{
			this.type = type;
			this.length = length;
		}

		public override void prettyPrint(StringBuilder s)
		{
			s.Append("new ");
			type.prettyPrint(s);
			s.Append("[");
			length.prettyPrint(s);
			s.Append("]");
		}
	}

	public class MethodInvocation : Expression
	{
		public static readonly ISet<LexemeCategory> FirstSet = BinaryOp.FirstSet;
		public static readonly ISet<LexemeCategory> FollowSet = BinaryOp.FollowSet;

		public override ISet<LexemeCategory> First { get { return FirstSet; } }
		public override ISet<LexemeCategory> Follow { get { return FollowSet; } }

		public readonly Expression className;
		public readonly Identifier methodName;
		public IList<Expression> arguments;

		public MethodInvocation(Expression className, Identifier methodName, IList<Expression> arguments)
		{
			this.className = className;
			this.methodName = methodName;
			this.arguments = arguments;
		}

		public override void prettyPrint(StringBuilder s)
		{
			className.prettyPrint (s);
			s.Append (".");
			methodName.prettyPrint (s);
			s.Append ("(");
			foreach (var e in arguments) {
				e.prettyPrint (s);
				s.Append (" ");
			}
			s.Append(")");
		}
	}

	public abstract class Leaf : Expression
	{
	}

	public class LiteralInt : Leaf
	{
		public static readonly ISet<LexemeCategory> FirstSet = BinaryOp.FirstSet;
		public static readonly ISet<LexemeCategory> FollowSet = BinaryOp.FollowSet;

		public override ISet<LexemeCategory> First { get { return FirstSet; } }
		public override ISet<LexemeCategory> Follow { get { return FollowSet; } }

		public readonly Lexeme literal;

		public LiteralInt(Lexeme literal)
		{
			this.literal = literal;
		}

		public override void prettyPrint(StringBuilder s)
		{
			s.AppendFormat("L({0})",literal.Body);
		}
	}

	public class LiteralBool : Leaf
	{
		public static readonly ISet<LexemeCategory> FirstSet = BinaryOp.FirstSet;
		public static readonly ISet<LexemeCategory> FollowSet = BinaryOp.FollowSet;

		public override ISet<LexemeCategory> First { get { return FirstSet; } }
		public override ISet<LexemeCategory> Follow { get { return FollowSet; } }

		public readonly Lexeme literal;

		public LiteralBool(Lexeme literal)
		{
			this.literal = literal;
		}

		public override void prettyPrint(StringBuilder s)
		{
			s.AppendFormat("L({0})",literal.Body);
		}
	}

	public class LeafIdentifier : Leaf
	{
		public static readonly ISet<LexemeCategory> FirstSet = BinaryOp.FirstSet;
		public static readonly ISet<LexemeCategory> FollowSet = BinaryOp.FollowSet;

		public override ISet<LexemeCategory> First { get { return FirstSet; } }
		public override ISet<LexemeCategory> Follow { get { return FollowSet; } }

		public readonly Lexeme literal;

		public LeafIdentifier(Lexeme literal)
		{
			this.literal = literal;
		}

		public override void prettyPrint(StringBuilder s)
		{
			s.AppendFormat("L({0})",literal.Body);
		}
	}



}

