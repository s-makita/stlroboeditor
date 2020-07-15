using System;

namespace StellarRobo.Type
{
    /// <summary>
    /// StellarRoboでの倍精度浮動小数点数を定義します。
    /// </summary>
    public sealed class StellarRoboFloat : StellarRoboObject
    {

        /// <summary>
        /// 実際の値を取得・設定します。
        /// </summary>
        public new double Value { get; set; }

        /// <summary>
        /// 最大値のインスタンスを取得します。
        /// </summary>
        public static StellarRoboReference MaxValue = StellarRoboReference.Right(double.MaxValue);

        /// <summary>
        /// 最小値のインスタンスを取得します。
        /// </summary>
        public static StellarRoboReference MinValue = StellarRoboReference.Right(double.MinValue);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected internal override StellarRoboReference GetMemberReference(string name)
        {
            switch (name)
            {
                case "MAX_VALUE":
                    return MaxValue;
                case "MIN_VALUE":
                    return MinValue;
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
            if (op == StellarRoboILCodeType.Negative) return (-Value).AsStellarRoboFloat();
            if (target.Type == TypeCode.Int64)
            {
                return ExpressionOperation(op, (StellarRoboInteger)target);
            }
            else if (target.Type == TypeCode.Double)
            {
                return ExpressionOperation(op, (StellarRoboFloat)target);
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

        private StellarRoboObject ExpressionOperation(StellarRoboILCodeType op, StellarRoboInteger target)
        {
            switch (op)
            {
                case StellarRoboILCodeType.Negative:
                    return (-Value).AsStellarRoboFloat();
                case StellarRoboILCodeType.Plus:
                    return (Value + target.Value).AsStellarRoboFloat();
                case StellarRoboILCodeType.Minus:
                    return (Value - target.Value).AsStellarRoboFloat();
                case StellarRoboILCodeType.Multiply:
                    return (Value * target.Value).AsStellarRoboFloat();
                case StellarRoboILCodeType.Divide:
                    return (Value / target.Value).AsStellarRoboFloat();
                case StellarRoboILCodeType.Modular:
                    return (Value % target.Value).AsStellarRoboFloat();
                case StellarRoboILCodeType.Equal:
                    return (Value == target.Value).AsStellarRoboBoolean();
                case StellarRoboILCodeType.NotEqual:
                    return (Value != target.Value).AsStellarRoboBoolean();
                case StellarRoboILCodeType.Greater:
                    return (Value > target.Value).AsStellarRoboBoolean();
                case StellarRoboILCodeType.Lesser:
                    return (Value < target.Value).AsStellarRoboBoolean();
                case StellarRoboILCodeType.GreaterEqual:
                    return (Value >= target.Value).AsStellarRoboBoolean();
                case StellarRoboILCodeType.LesserEqual:
                    return (Value <= target.Value).AsStellarRoboBoolean();
                default:
                    return StellarRoboNil.Instance;
            }
        }

        private StellarRoboObject ExpressionOperation(StellarRoboILCodeType op, StellarRoboFloat target)
        {
            switch (op)
            {
                case StellarRoboILCodeType.Negative:
                    return (-Value).AsStellarRoboFloat();
                case StellarRoboILCodeType.Plus:
                    return (Value + target.Value).AsStellarRoboFloat();
                case StellarRoboILCodeType.Minus:
                    return (Value - target.Value).AsStellarRoboFloat();
                case StellarRoboILCodeType.Multiply:
                    return (Value * target.Value).AsStellarRoboFloat();
                case StellarRoboILCodeType.Divide:
                    return (Value / target.Value).AsStellarRoboFloat();
                case StellarRoboILCodeType.Modular:
                    return (Value % target.Value).AsStellarRoboFloat();
                case StellarRoboILCodeType.Equal:
                    return (Value == target.Value).AsStellarRoboBoolean();
                case StellarRoboILCodeType.NotEqual:
                    return (Value != target.Value).AsStellarRoboBoolean();
                case StellarRoboILCodeType.Greater:
                    return (Value > target.Value).AsStellarRoboBoolean();
                case StellarRoboILCodeType.Lesser:
                    return (Value < target.Value).AsStellarRoboBoolean();
                case StellarRoboILCodeType.GreaterEqual:
                    return (Value >= target.Value).AsStellarRoboBoolean();
                case StellarRoboILCodeType.LesserEqual:
                    return (Value <= target.Value).AsStellarRoboBoolean();
                default:
                    return StellarRoboNil.Instance;
            }
        }

        /// <summary>
        /// 新しいインスタンスを生成します。
        /// </summary>
        public StellarRoboFloat()
        {
            Type = TypeCode.Double;
            ExtraType = "Float";
        }

#pragma warning disable 1591
        public override object Clone() => Value.AsStellarRoboFloat();
        public override StellarRoboObject AsByValValue() => Value.AsStellarRoboFloat();
        public override bool Equals(object obj)
        {
            var t = obj as StellarRoboFloat;
            return t != null && t.Value == Value;
        }
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
        public override long ToInt64() => (long)Value;
        public override double ToDouble() => Value;
#pragma warning restore 1591
    }
}
