using Flint.Common;
using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Castclass : Ast
	{
		public readonly TypeReference Type;
		public readonly Ast Value;
		public Castclass(CilPoint pt, TypeReference type, Ast value) : base(pt)
		{
			Type = type;
			Value = value;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Castclass), Hash.Code(Type), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Castclass cast)
			{
				return Are.Equal(Type, cast.Type)
					&& Are.Equal(Value, cast.Value);
			}
			return false;
		}

		protected override (Ast, MergeResult) Merge(Ast other)
		{
			if (other is Castclass cast)
			{
				if (Are.Equal(Type, cast.Type) == false)
					return NotMerged();

				var (value, valueMr) = Merge(Value, cast.Value);
				if (valueMr == MergeResult.NotMerged)
					return NotMerged();

				return OkMerged(new Castclass(CilPoint, Type, value));
			}
			return NotMerged();
		}
	}
}
