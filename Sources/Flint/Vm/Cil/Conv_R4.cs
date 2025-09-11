﻿using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Conv_R4 : Ast
	{
		public readonly Ast Value;
		public Conv_R4(SequencePoint debug, Ast value) : base(debug)
		{
			Value = value;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Conv_R4), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Conv_R4 conv)
			{
				return Value.Equals(conv.Value);
			}
			return false;
		}
	}
}
