using System;
using System.Collections.Generic;

namespace MiniJava
{
	public static class Precedence
	{
		
		static Dictionary<LexemeCategory, int> operPrecedence = new Dictionary<LexemeCategory, int>() 
		{
			{LexemeCategory.ADD, 5},
			{LexemeCategory.MUL, 10},
			{LexemeCategory.SUB, 5},
			{LexemeCategory.MOD, 10},
			{LexemeCategory.AND, 2},
			{LexemeCategory.OR, 2},
			{LexemeCategory.DIV, 9},
			{LexemeCategory.EQ, 1},
			{LexemeCategory.GT, 1},
			{LexemeCategory.LT, 1}
		};

		public static int GetPrecedence(LexemeCategory oper)
		{
			int precedence;
			bool found = operPrecedence.TryGetValue (oper, out precedence);
			if (found) {
				return precedence;
			} else {
				return 0;
			}
		}
	}
}

