using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarRobo.Type
{
    /// <summary>
    /// StellarRobo上でメンバーや動作を直接変更できるオブジェクトを定義します。
    /// </summary>
    public sealed class StellarRoboDynamicObject : StellarRoboObject
    {
        private Dictionary<string, StellarRoboReference> funcs = new Dictionary<string, StellarRoboReference>();
        /// <summary>
        /// 現在.NETから定義されているメソッドを取得します。
        /// </summary>
        public IReadOnlyDictionary<string, StellarRoboReference> FunctionReferences => funcs;

        private Dictionary<string, StellarRoboReference> props = new Dictionary<string, StellarRoboReference>();
        /// <summary>
        /// 現在.NET/StellarRobo上から定義されているプロパティの参照を取得します。
        /// </summary>
        public IReadOnlyDictionary<string, StellarRoboReference> PropertyReferences => props;

        /// <summary>
        /// <see cref="Call(StellarRoboContext, StellarRoboObject[])"/>が呼ばれた場合に呼び出される
        /// デリゲートを取得・設定します。
        /// </summary>
        public Func<StellarRoboObject, StellarRoboObject[], StellarRoboObject> CallingFunction { get; set; }

        /// <summary>
        /// <see cref="GetIndexerReference(StellarRoboObject[])"/>が呼ばれた場合に呼び出される
        /// デリゲートを取得・設定します。
        /// </summary>
        public Func<StellarRoboObject, StellarRoboObject[], StellarRoboReference> IndexerFunction { get; set; }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        public StellarRoboDynamicObject()
        {
            Type = TypeCode.Object;
            ExtraType = "DynamicObject";
        }

        #region Registerers
        /// <summary>
        /// メソッドを登録します。
        /// </summary>
        /// <param name="name">メソッド名</param>
        /// <param name="func">登録するメソッドのデリゲート</param>
        public void AddFunction(string name, StellarRoboInteropDelegate func)
        {
            funcs[name] = StellarRoboReference.Right(this, func);
        }

        /// <summary>
        /// メソッドを登録します。
        /// </summary>
        /// <param name="name">メソッド名</param>
        /// <param name="func">
        /// 登録するメソッドのデリゲート。
        /// インスタンスのみが渡されます。
        /// </param>
        public void AddFunction(string name, Action<StellarRoboObject> func)
        {
            StellarRoboInteropDelegate wp =
                (c, s, a) =>
                {
                    func(s);
                    return StellarRoboNil.Instance.NoResume();
                };
            funcs[name] = StellarRoboReference.Right(this, wp);
        }

        /// <summary>
        /// メソッドを登録します。
        /// </summary>
        /// <param name="name">メソッド名</param>
        /// <param name="func">
        /// 登録するメソッドのデリゲート。
        /// インスタンスと引数配列が渡されます。
        /// </param>
        public void AddFunction(string name, Action<StellarRoboObject, StellarRoboObject[]> func)
        {
            StellarRoboInteropDelegate wp =
                (c, s, a) =>
                {
                    func(s, a);
                    return StellarRoboNil.Instance.NoResume();
                };
            funcs[name] = StellarRoboReference.Right(this, wp);
        }

        /// <summary>
        /// メソッドを登録します。
        /// </summary>
        /// <param name="name">メソッド名</param>
        /// <param name="func">
        /// 登録するメソッドのデリゲート。
        /// インスタンスのみが渡されます。
        /// </param>
        public void AddFunction(string name, Func<StellarRoboObject, StellarRoboObject> func)
        {
            StellarRoboInteropDelegate wp = (c, s, a) => func(s).NoResume();
            funcs[name] = StellarRoboReference.Right(this, wp);
        }

        /// <summary>
        /// メソッドを登録します。
        /// </summary>
        /// <param name="name">メソッド名</param>
        /// <param name="func">
        /// 登録するメソッドのデリゲート。
        /// インスタンスと引数が渡されます。
        /// </param>
        public void AddFunction(string name, Func<StellarRoboObject, StellarRoboObject[], StellarRoboObject> func)
        {
            StellarRoboInteropDelegate wp = (c, s, a) => func(s, a).NoResume();
            funcs[name] = StellarRoboReference.Right(this, wp);
        }

        /// <summary>
        /// メソッドを登録します。
        /// </summary>
        /// <param name="name">メソッド名</param>
        /// <param name="func">
        /// 登録するメソッドのデリゲート。
        /// インスタンスと引数が渡されます。
        /// </param>
        public void AddInt32Function(string name, Func<StellarRoboObject, StellarRoboObject[], int> func)
        {
            StellarRoboInteropDelegate wp = (c, s, a) => func(s, a).AsStellarRoboInteger().NoResume();
            funcs[name] = StellarRoboReference.Right(this, wp);
        }

        /// <summary>
        /// メソッドを登録します。
        /// </summary>
        /// <param name="name">メソッド名</param>
        /// <param name="func">
        /// 登録するメソッドのデリゲート。
        /// インスタンスと引数が渡されます。
        /// </param>
        public void AddInt64Function(string name, Func<StellarRoboObject, StellarRoboObject[], long> func)
        {
            StellarRoboInteropDelegate wp = (c, s, a) => func(s, a).AsStellarRoboInteger().NoResume();
            funcs[name] = StellarRoboReference.Right(this, wp);
        }

        /// <summary>
        /// メソッドを登録します。
        /// </summary>
        /// <param name="name">メソッド名</param>
        /// <param name="func">
        /// 登録するメソッドのデリゲート。
        /// インスタンスと引数が渡されます。
        /// </param>
        public void AddSingleFunction(string name, Func<StellarRoboObject, StellarRoboObject[], float> func)
        {
            StellarRoboInteropDelegate wp = (c, s, a) => ((double)func(s, a)).AsStellarRoboFloat().NoResume();
            funcs[name] = StellarRoboReference.Right(this, wp);
        }

        /// <summary>
        /// メソッドを登録します。
        /// </summary>
        /// <param name="name">メソッド名</param>
        /// <param name="func">
        /// 登録するメソッドのデリゲート。
        /// インスタンスと引数が渡されます。
        /// </param>
        public void AddDoubleFunction(string name, Func<StellarRoboObject, StellarRoboObject[], double> func)
        {
            StellarRoboInteropDelegate wp = (c, s, a) => func(s, a).AsStellarRoboFloat().NoResume();
            funcs[name] = StellarRoboReference.Right(this, wp);
        }

        /// <summary>
        /// メソッドを登録します。
        /// </summary>
        /// <param name="name">メソッド名</param>
        /// <param name="func">
        /// 登録するメソッドのデリゲート。
        /// インスタンスと引数が渡されます。
        /// </param>
        public void AddBooleanFunction(string name, Func<StellarRoboObject, StellarRoboObject[], bool> func)
        {
            StellarRoboInteropDelegate wp = (c, s, a) => func(s, a).AsStellarRoboBoolean().NoResume();
            funcs[name] = StellarRoboReference.Right(this, wp);
        }

        /// <summary>
        /// メソッドを登録します。
        /// </summary>
        /// <param name="name">メソッド名</param>
        /// <param name="func">
        /// 登録するメソッドのデリゲート。
        /// インスタンスと引数が渡されます。
        /// </param>
        public void AddStringFunction(string name, Func<StellarRoboObject, StellarRoboObject[], string> func)
        {
            StellarRoboInteropDelegate wp = (c, s, a) => func(s, a).AsStellarRoboString().NoResume();
            funcs[name] = StellarRoboReference.Right(this, wp);
        }
        #endregion

        #region Overrides
#pragma warning disable 1591
        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            StellarRoboReference result;
            if (PropertyReferences.TryGetValue(name, out result)) return result;
            if (FunctionReferences.TryGetValue(name, out result)) return result;
            result = new StellarRoboReference { IsLeftValue = true };
            props[name] = result;
            return result;
        }

        protected internal override StellarRoboFunctionResult Call(StellarRoboContext context, StellarRoboObject[] args)
        {
            if (CallingFunction == null) return StellarRoboNil.Instance.NoResume();
            return CallingFunction(this, args).NoResume();
        }

        protected internal override StellarRoboReference GetIndexerReference(StellarRoboObject[] indices)
        {
            if (IndexerFunction == null) return StellarRoboNil.Reference;
            return IndexerFunction(this, indices);
        }

        protected internal override StellarRoboObject ExpressionOperation(StellarRoboILCodeType op, StellarRoboObject target)
        {
            if (target.ExtraType == "DynamicObject")
            {
                var t = target as StellarRoboDynamicObject;
                switch (op)
                {
                    case StellarRoboILCodeType.Equal: return (t == target).AsStellarRoboBoolean();
                    case StellarRoboILCodeType.NotEqual: return (t != target).AsStellarRoboBoolean();
                    default: return base.ExpressionOperation(op, target);
                }
            }
            else
            {
                switch (op)
                {
                    case StellarRoboILCodeType.Equal: return StellarRoboBoolean.False;
                    case StellarRoboILCodeType.NotEqual: return StellarRoboBoolean.True;
                    default: return base.ExpressionOperation(op, target);
                }
            }
        }

        public override bool Equals(object obj)
        {
            var t = obj as StellarRoboDynamicObject;
            if (t == null) return false;
            return funcs.Equals(t.funcs) && props.Equals(t.props);
        }
        public override int GetHashCode() => unchecked(funcs.GetHashCode() * props.GetHashCode());

#pragma warning restore 1591
        #endregion
    }
}
