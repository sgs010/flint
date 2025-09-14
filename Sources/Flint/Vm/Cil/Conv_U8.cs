﻿using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Conv_U8 : Ast
	{
		public readonly Ast Value;
		public Conv_U8(SequencePoint sp, Ast value) : base(sp)
		{
			Value = value;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Conv_U8), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Conv_U8 conv)
			{
				return Value.Equals(conv.Value);
			}
			return false;
		}
	}
}
