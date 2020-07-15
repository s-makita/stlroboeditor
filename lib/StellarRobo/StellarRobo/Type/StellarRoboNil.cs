using System;

namespace StellarRobo.Type
{
    /// <summary>
    /// StellarRoboのnil(null)を定義します。
    /// </summary>
    public sealed class StellarRoboNil : StellarRoboObject
    {

        private static StellarRoboNil instance = new StellarRoboNil();
        /// <summary>
        /// 唯一のインスタンスを取得します。
        /// </summary>
        public static StellarRoboNil Instance { get; } = instance;
        /// <summary>
        /// <see cref="Instance"/>への参照を取得します。
        /// </summary>
        public static StellarRoboReference Reference { get; } = StellarRoboReference.Right(Instance);

        private StellarRoboNil()
        {
            Type = TypeCode.Empty;
            ExtraType = "Nil";
        }

        /// <summary>
        /// このオブジェクトに対して二項式としての演算をしようとしても大体nilです。
        /// </summary>
        /// <param name="op">演算子</param>
        /// <param name="target">2項目の<see cref="StellarRoboObject"/></param>
        /// <returns></returns>
        protected internal override StellarRoboObject ExpressionOperation(StellarRoboILCodeType op, StellarRoboObject target)
        {
            switch (op)
            {
                case StellarRoboILCodeType.Equal:
                    return (ExtraType == target.ExtraType).AsStellarRoboBoolean();
                case StellarRoboILCodeType.NotEqual:
                    return (ExtraType != target.ExtraType).AsStellarRoboBoolean();
                default:
                    return Instance;
            }
        }

        /// <summary>
        /// 現在の以下略。
        /// </summary>
        /// <returns>知るか</returns>
        public override string ToString() => "nil";

#pragma warning disable 1591
        public override object Clone() => Instance;
        public override bool Equals(object obj) => obj is StellarRoboNil;
        public override int GetHashCode() => "nil".GetHashCode();
#pragma warning restore 1591
    }
}
