using System;

namespace StellarRobo.Type
{
    /// <summary>
    /// StellarRoboでの真偽値を定義します。
    /// </summary>
    public sealed class StellarRoboBoolean : StellarRoboObject
    {
        /// <summary>
        /// 実際の値を取得します。
        /// </summary>
        public new bool Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="op"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        protected internal override StellarRoboObject ExpressionOperation(StellarRoboILCodeType op, StellarRoboObject target)
        {
            if (op==StellarRoboILCodeType.Not) return (!Value).AsStellarRoboBoolean();
            if (target.Type == TypeCode.Boolean)
            {
                var t = (StellarRoboBoolean)target;
                switch (op)
                {
                    case StellarRoboILCodeType.AndAlso:
                        return (Value && t.Value).AsStellarRoboBoolean();
                    case StellarRoboILCodeType.OrElse:
                        return (Value || t.Value).AsStellarRoboBoolean();
                    case StellarRoboILCodeType.And:
                        return (Value & t.Value).AsStellarRoboBoolean();
                    case StellarRoboILCodeType.Or:
                        return (Value | t.Value).AsStellarRoboBoolean();
                    case StellarRoboILCodeType.Xor:
                        return (Value ^ t.Value).AsStellarRoboBoolean();
                    case StellarRoboILCodeType.Equal:
                        return (Value == t.Value).AsStellarRoboBoolean();
                    case StellarRoboILCodeType.NotEqual:
                        return (Value != t.Value).AsStellarRoboBoolean();
                    default:
                        return StellarRoboNil.Instance;
                }
            }
            else
            {
                switch (op)
                {
                    case StellarRoboILCodeType.Equal:
                        return False;
                    case StellarRoboILCodeType.NotEqual:
                        return True;
                    default:
                        return StellarRoboNil.Instance;
                }
            }

        }

        

        /// <summary>
        /// 新しいインスタンスを生成します。
        /// 
        /// </summary>
        public StellarRoboBoolean()
        {
            Type = TypeCode.Boolean;
            ExtraType = "Boolean";
        }

        /// <summary>
        /// true。
        /// </summary>
        public static StellarRoboBoolean True { get; } = true.AsStellarRoboBoolean();

        /// <summary>
        /// false。
        /// </summary>
        public static StellarRoboBoolean False { get; } = false.AsStellarRoboBoolean();

#pragma warning disable 1591
        public override object Clone() => Value.AsStellarRoboBoolean();
        public override StellarRoboObject AsByValValue() => Value.AsStellarRoboBoolean();
        public override bool Equals(object obj)
        {
            var t = obj as StellarRoboBoolean;
            return t != null && t.Value == Value;
        }
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
        public override bool ToBoolean() => Value;
#pragma warning restore 1591

    }
}
