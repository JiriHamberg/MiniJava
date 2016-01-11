using System;
using System.Collections.Generic;

namespace MiniJava
{
	public static class Precedence
	{
		
		static Dictionary<LexemeCategory, int> operPrecedence = new Dictionary<LexemeCategory, int>() 
		{
			{LexemeCategory.MUL, 12},
			{LexemeCategory.DIV, 12},
			{LexemeCategory.MOD, 12},

			{LexemeCategory.ADD, 11},
			{LexemeCategory.SUB, 11},

			{LexemeCategory.GT, 9},
			{LexemeCategory.LT, 9},

			{LexemeCategory.EQ, 8},

			{LexemeCategory.AND, 4},
			{LexemeCategory.OR, 3}
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

