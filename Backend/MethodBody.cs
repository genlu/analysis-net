﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;
using Backend.ThreeAddressCode;

namespace Backend
{
	public class MethodBody
	{
		public IMethodDefinition MethodDefinition { get; private set; }
		public IList<TryExceptionHandler> ExceptionHandlers { get; private set; }
		public IList<Instruction> Instructions { get; private set; }
		public IList<Variable> Parameters { get; private set; }
		public ISet<Variable> Variables { get; private set; }

		public MethodBody(IMethodDefinition methodDefinition)
		{
			this.MethodDefinition = methodDefinition;
			this.ExceptionHandlers = new List<TryExceptionHandler>();
			this.Instructions = new List<Instruction>();
			this.Parameters = new List<Variable>();
			this.Variables = new HashSet<Variable>();
		}

		public override string ToString()
		{
			var result = new StringBuilder();
			var header = MemberHelper.GetMethodSignature(this.MethodDefinition, NameFormattingOptions.Signature | NameFormattingOptions.ParameterName);

			result.AppendLine(header);

			foreach (var instruction in this.Instructions)
			{
				result.Append("  ");
				result.Append(instruction);
				result.AppendLine();
			}

			foreach (var handler in this.ExceptionHandlers)
			{
				result.AppendLine();
				result.Append(handler);
			}

			return result.ToString();
		}
	}
}
