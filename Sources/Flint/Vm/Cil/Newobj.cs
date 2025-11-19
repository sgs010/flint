using Flint.Common;
using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Newobj : Ast
	{
		public readonly TypeReference Type;
		public readonly MethodReference Ctor;
		public readonly Ast[] Args;
		public Newobj(CilPoint pt, TypeReference type, MethodReference ctor, Ast[] args) : base(pt)
		{
			Type = type;
			Ctor = ctor;
			Args = args;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			foreach (var arg in Args)
				yield return arg;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Newobj), Hash.Code(Type), Hash.Code(Ctor), Args);
		}

		public override bool Equals(Ast other)
		{
			if (other is Newobj newobj)
			{
				return Are.Equal(Type, newobj.Type)
					&& Are.Equal(Ctor, newobj.Ctor)
					&& Args.SequenceEqual(newobj.Args);
			}
			return false;
		}

		protected override (Ast, MergeResult) Merge(Ast other)
		{
			if (other is Newobj newobj)
			{
				if (Are.Equal(Type, newobj.Type) == false)
					return NotMerged();

				if (Are.Equal(Ctor, newobj.Ctor) == false)
					return NotMerged();

				var (args, argsMr) = Merge(Args, newobj.Args);
				if (argsMr == MergeResult.NotMerged)
					return NotMerged();

				return OkMerged(new Newobj(CilPoint, Type, Ctor, args));
			}
			return NotMerged();
		}
	}
}
