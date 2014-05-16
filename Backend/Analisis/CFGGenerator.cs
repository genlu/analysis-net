﻿using Backend.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backend.Analisis
{
	public enum CFGNodeKind
	{
		Enter,
		Exit,
		BasicBlock
	}

	public class CFGNode
	{
		public int Id { get; private set; }
		public CFGNodeKind Kind { get; private set; }
		public ISet<CFGNode> Predecessors { get; private set; }
		public ISet<CFGNode> Successors { get; private set; }
		public IList<Instruction> Instructions { get; private set; }

		public CFGNode(int id, CFGNodeKind kind)
		{
			this.Id = id;
			this.Kind = kind;
			this.Predecessors = new HashSet<CFGNode>();
			this.Successors = new HashSet<CFGNode>();
			this.Instructions = new List<Instruction>();
		}

		public string SerializeToDot()
		{
			string result;

			switch (this.Kind)
			{
				case CFGNodeKind.Enter: result = "enter"; break;
				case CFGNodeKind.Exit: result = "exit"; break;
				case CFGNodeKind.BasicBlock: result = string.Join("\\l", this.Instructions) + "\\l"; break;
				default: throw new Exception("Unknown Control Flow Graph node kind: " + this.Kind);
			}

			return result;
		}

		public override string ToString()
		{
			string result;

			switch (this.Kind)
			{
				case CFGNodeKind.Enter: result = "enter"; break;
				case CFGNodeKind.Exit: result = "exit"; break;
				case CFGNodeKind.BasicBlock: result = string.Join("\n", this.Instructions); break;
				default: throw new Exception("Unknown Control Flow Graph node kind: " + this.Kind);
			}

			return result;
		}
	}

	public class ControlFlowGraph
	{
		public CFGNode Enter { get; private set; }
		public CFGNode Exit { get; private set; }
		public ISet<CFGNode> Nodes { get; private set; }

		public ControlFlowGraph()
		{
			this.Enter = new CFGNode(0, CFGNodeKind.Enter);
			this.Exit = new CFGNode(1, CFGNodeKind.Exit);
			this.Nodes = new HashSet<CFGNode>() { this.Enter, this.Exit };
		}

		public ControlFlowGraph(MethodBody method)
			: this()
		{
			var targets = new Dictionary<string, CFGNode>();
			var nodeId = 2;

			foreach (var instruction in method.Instructions)
			{
				if (instruction is IBranchInstruction &&
					!targets.ContainsKey(instruction.Label))
				{
					var branch = instruction as IBranchInstruction;
					var node = new CFGNode(nodeId++, CFGNodeKind.BasicBlock);

					targets.Add(branch.Target, node);
				}
			}

			var createNewNode = true;
			var connectWithPreviousNode = true;
			var current = this.Enter;
			CFGNode previous;

			foreach (var instruction in method.Instructions)
			{
				if (targets.ContainsKey(instruction.Label))
				{
					previous = current;
					current = targets[instruction.Label];

					if (connectWithPreviousNode)
					{
						this.ConnectNodes(previous, current);
					}
				}
				else if (createNewNode)
				{
					previous = current;
					current = new CFGNode(nodeId++, CFGNodeKind.BasicBlock);

					if (connectWithPreviousNode)
					{
						this.ConnectNodes(previous, current);
					}
				}

				createNewNode = false;
				connectWithPreviousNode = true;
				current.Instructions.Add(instruction);

				if (instruction is IBranchInstruction)
				{
					var branch = instruction as IBranchInstruction;
					var target = targets[branch.Target];

					this.ConnectNodes(current, target);
					createNewNode = true;
					connectWithPreviousNode = instruction is ConditionalBranchInstruction;
				}
				else if (instruction is ReturnInstruction)
				{
					this.ConnectNodes(current, this.Exit);
					createNewNode = true;
					connectWithPreviousNode = false;
				}
			}

			this.ConnectNodes(current, this.Exit);
		}

		public void ConnectNodes(CFGNode predecessor, CFGNode successor)
		{
			successor.Predecessors.Add(predecessor);
			predecessor.Successors.Add(successor);
			this.Nodes.Add(predecessor);
			this.Nodes.Add(successor);
		}

		public string SerializeToDot()
		{
			var sb = new StringBuilder();
			sb.AppendLine("digraph ControlFlow\n{");
			sb.AppendLine("\tnode[shape=\"rect\"];");

			foreach (var node in this.Nodes)
			{
				var label = node.SerializeToDot();
				sb.AppendFormat("\t{0}[label=\"{1}\"];\n", node.Id, label);

				foreach (var successor in node.Successors)
				{
					sb.AppendFormat("\t{0} -> {1};\n", node.Id, successor.Id);
				}
			}

			sb.AppendLine("}");
			return sb.ToString();
		}
	}
}
