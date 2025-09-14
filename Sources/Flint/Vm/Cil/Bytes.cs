﻿using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Bytes : Ast
	{
		public readonly Ast Count;
		public Bytes(SequencePoint sp, Ast count) : base(sp)
		{
			Count = count;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Count;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Bytes), Count);
		}

		public override bool Equals(Ast other)
		{
			if (other is Bytes bytes)
			{
				return Count.Equals(bytes.Count);
			}
			return false;
		}
	}
}
