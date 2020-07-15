using System;
using System.Linq;

namespace StellarRobo.Type
{
    /// <summary>
    /// StellarRoboでの文字列を定義します。
    /// </summary>
    public sealed class StellarRoboString : StellarRoboObject
    {
        private string raw = "";
        /// <summary>
        /// 実際の値を取得します。
        /// </summary>
        public new string Value
        {
            get { return raw; }
            set
            {
                raw = value;
                Length.RawObject = raw.Length.AsStellarRoboInteger();
            }
        }

        internal static StellarRoboInteropClassInfo Information { get; } = new StellarRoboInteropClassInfo("String");

        static StellarRoboString()
        {
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("join", ClassJoin));
        }

        /// <summary>
        /// 長さへの参照を取得します。
        /// </summary>
        public StellarRoboReference Length { get; } = new StellarRoboReference { IsLeftValue = true, RawObject = 0.AsStellarRoboInteger() };


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            switch (name)
            {
                case "length":
                    return Length;

                case nameof(replace): return replace;
                case nameof(substring): return substring;
                case nameof(split): return split;
                case nameof(to_upper): return to_upper;
                case nameof(to_lower): return to_lower;
                case nameof(starts): return starts;
                case nameof(ends): return ends;
                case nameof(pad_left): return pad_left;
                case nameof(pad_right): return pad_right;
            }
            return base.GetMemberReference(name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="op"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        protected internal override StellarRoboObject ExpressionOperation(StellarRoboILCodeType op, StellarRoboObject target)
        {
            if (target.Type == TypeCode.String)
            {
                var t = (StellarRoboString)target;
                switch (op)
                {
                    case StellarRoboILCodeType.Plus:
                        return (Value + t.Value).AsStellarRoboString();
                    case StellarRoboILCodeType.Equal:
                        return (Value == t.Value).AsStellarRoboBoolean();
                    case StellarRoboILCodeType.NotEqual:
                        return (Value != t.Value).AsStellarRoboBoolean();
                    case StellarRoboILCodeType.Greater:
                        return (Value.CompareTo(t.Value) > 0).AsStellarRoboBoolean();
                    case StellarRoboILCodeType.Lesser:
                        return (Value.CompareTo(t.Value) < 0).AsStellarRoboBoolean();
                    case StellarRoboILCodeType.GreaterEqual:
                        return (Value.CompareTo(t.Value) >= 0).AsStellarRoboBoolean();
                    case StellarRoboILCodeType.LesserEqual:
                        return (Value.CompareTo(t.Value) <= 0).AsStellarRoboBoolean();
                    default:
                        return StellarRoboNil.Instance;
                }
            }
            else
            {
                switch (op)
                {
                    case StellarRoboILCodeType.Equal:
                        return StellarRoboBoolean.False;
                    case StellarRoboILCodeType.NotEqual:
                        return StellarRoboBoolean.True;
                    default:
                        return StellarRoboNil.Instance;
                }
            }
        }

        /// <summary>
        /// 文字を参照します。
        /// </summary>
        /// <param name="indices">引数。Integer以外禁止。</param>
        /// <returns></returns>
        protected internal override StellarRoboReference GetIndexerReference(StellarRoboObject[] indices)
        {
            var i = indices[0].ToInt32();
            return StellarRoboReference.Right(raw[i].ToString().AsStellarRoboString());
        }

        /// <summary>
        /// 新しいインスタンスを生成します。
        /// </summary>
        public StellarRoboString()
        {
            Type = TypeCode.String;
            ExtraType = "String";

            split = StellarRoboReference.Right(this, InstanceSplit);
            replace = StellarRoboReference.Right(this, InstanceReplace);
            substring = StellarRoboReference.Right(this, InstanceSubstring);
            to_upper = StellarRoboReference.Right(this, InstanceToUpper);
            to_lower = StellarRoboReference.Right(this, InstanceToLower);
            starts = StellarRoboReference.Right(this, InstanceStartsWith);
            ends = StellarRoboReference.Right(this, InstanceEndsWith);
            pad_left = StellarRoboReference.Right(this, InstancePadLeft);
            pad_right = StellarRoboReference.Right(this, InstancePadRight);
        }

        private StellarRoboReference to_upper, to_lower, starts, ends, pad_left, pad_right, replace, substring, split;

        private StellarRoboFunctionResult InstanceSubstring(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            var t = self.ToString();
            switch (args.Length)
            {
                case 0:
                    return "".AsStellarRoboString().NoResume();
                case 1:
                    return t.Substring((int)args[0].ToInt64()).AsStellarRoboString().NoResume();
                default:
                    return t.Substring((int)args[0].ToInt64(), (int)args[1].ToInt64()).AsStellarRoboString().NoResume();
            }
        }

        private StellarRoboFunctionResult InstanceReplace(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args) => raw.Replace(args[0].ToString(), args[1].ToString()).AsStellarRoboString().NoResume();

        private StellarRoboFunctionResult InstanceSplit(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            var l = args[0];
            var op = StringSplitOptions.None;
            if (args.Length >= 2)
            {
                var flag = args[1].ToBoolean();
                if (flag) op = StringSplitOptions.RemoveEmptyEntries;
            }
            if (l.ExtraType == "Array")
            {
                var list = l.ToStringArray();
                var result = raw.Split(list.ToArray(), op);
                return new StellarRoboArray(result.Select(p => p.AsStellarRoboString())).NoResume();
            }
            else
            {
                var str = l.ToString();
                var result = raw.Split(new[] { str }, op);
                return new StellarRoboArray(result.Select(p => p.AsStellarRoboString())).NoResume();
            }
        }

        private StellarRoboFunctionResult InstanceStartsWith(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args) => raw.StartsWith(args[0].ToString()).AsStellarRoboBoolean().NoResume();

        private StellarRoboFunctionResult InstanceEndsWith(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args) => raw.EndsWith(args[0].ToString()).AsStellarRoboBoolean().NoResume();

        private StellarRoboFunctionResult InstancePadLeft(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args) => raw.PadLeft(args[0].ToInt32()).AsStellarRoboString().NoResume();

        private StellarRoboFunctionResult InstancePadRight(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args) => raw.PadRight(args[0].ToInt32()).AsStellarRoboString().NoResume();

        private StellarRoboFunctionResult InstanceToUpper(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args) => raw.ToUpper().AsStellarRoboString().NoResume();

        private StellarRoboFunctionResult InstanceToLower(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args) => raw.ToLower().AsStellarRoboString().NoResume();

        private static StellarRoboFunctionResult ClassJoin(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            var ls = args[1].ToStringArray();
            var s = args[0].ToString();
            var result = string.Join(s, ls);
            return result.AsStellarRoboString().NoResume();
        }

#pragma warning disable 1591
        public override object Clone() => Value.AsStellarRoboString();
        public override string ToString() => Value;
        public override StellarRoboObject AsByValValue() => Value.AsStellarRoboString();
        public override bool Equals(object obj)
        {
            var t = obj as StellarRoboString;
            return t != null && t.Value == Value;
        }
        public override int GetHashCode() => Value.GetHashCode();
#pragma warning restore 1591
    }
}
