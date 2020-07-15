using System;

namespace StellarRobo.Type
{
    /// <summary>
    /// StellarRoboで利用される型のアクセスを提供します。
    /// </summary>
    public class StellarRoboObject : ICloneable
    {
        /// <summary>
        /// 実際の値を取得・設定します。
        /// </summary>
        public dynamic Value { get; set; }

        /// <summary>
        /// このオブジェクトの型を取得・設定します。
        /// </summary>
        public TypeCode Type { get; protected internal set; }

        /// <summary>
        /// 追加の型情報を取得します。
        /// </summary>
        public string ExtraType { get; protected internal set; } = "";

        /// <summary>
        /// 特定の名前を持つメンバーに対してアクセスを試み、参照を取得します。
        /// </summary>
        /// <param name="name">メンバー名</param>
        /// <returns>アクセスできる場合は対象のオブジェクト</returns>
        protected internal virtual StellarRoboReference GetMemberReference(string name)
        {
            switch (name)
            {
                case "to_str":
                    return InstanceToString(this);
                case "hash":
                    return InstanceHash(this);
                case "type":
                    return StellarRoboReference.Right(ExtraType);
                default:
                    return StellarRoboNil.Reference;
            }
        }

        /// <summary>
        /// <see cref="GetMemberReference(string)"/>の簡易版。
        /// <see cref="GetIndexerReference(StellarRoboObject[])"/>は参照しないので注意してください。
        /// </summary>
        /// <param name="name">メンバー名</param>
        /// <returns>アクセスできる場合は対象のオブジェクト</returns>
        public StellarRoboObject this[string name]
        {
            get { return GetMemberReference(name).RawObject; }
            set { GetMemberReference(name).RawObject = value; }
        }

        /// <summary>
        /// このオブジェクトに対してメソッドとしての呼び出しをします。
        /// </summary>
        /// <param name="context">実行される<see cref="StellarRoboContext"/></param>
        /// <param name="args">引数</param>
        /// <returns>返り値</returns>
        protected internal virtual StellarRoboFunctionResult Call(StellarRoboContext context, StellarRoboObject[] args)
        {
            throw new InvalidOperationException($"この{nameof(StellarRoboObject)}に対してメソッド呼び出しは出来ません。");
        }

        /// <summary>
        /// このオブジェクトに対してインデクサーアクセスを試みます。 
        /// </summary>
        /// <param name="indices">インデックス引数</param>
        /// <returns></returns>
        protected internal virtual StellarRoboReference GetIndexerReference(StellarRoboObject[] indices)
        {
            throw new InvalidOperationException($"この{nameof(StellarRoboObject)}に対してインデクサー呼び出しは出来ません。");
        }

        /// <summary>
        /// このオブジェクトに対して二項式としての演算をします。
        /// </summary>
        /// <param name="op">演算子</param>
        /// <param name="target">2項目の<see cref="StellarRoboObject"/></param>
        /// <returns></returns>
        protected internal virtual StellarRoboObject ExpressionOperation(StellarRoboILCodeType op, StellarRoboObject target)
        {
            throw new InvalidOperationException($"この{nameof(StellarRoboObject)}に対して式操作は出来ません。");
        }

        /// <summary>
        /// 新しいインスタンスを生成します。
        /// </summary>
        public StellarRoboObject()
        {
            Type = TypeCode.Object;
            ExtraType = "Array";
        }

        private static StellarRoboReference InstanceToString(StellarRoboObject self) => StellarRoboReference.Right(self, (ctx, s, args) => s.ToString().AsStellarRoboString().NoResume());

        private static StellarRoboReference InstanceHash(StellarRoboObject self) => StellarRoboReference.Right(self, (ctx, s, args) => s.GetHashCode().AsStellarRoboInteger().NoResume());

        /// <summary>
        /// 現在
        /// </summary>
        /// <returns></returns>
        public override string ToString() => "StellarRoboObject";

        /// <summary>
        /// 可能ならば<see cref="int"/>型に変換します。
        /// </summary>
        /// <returns></returns>
        public int ToInt32() => (int)ToInt64();

        /// <summary>
        /// 可能ならば<see cref="long"/>型に変換します。
        /// </summary>
        /// <returns></returns>
        public virtual long ToInt64()
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// 可能ならば<see cref="double"/>型に変換します。
        /// </summary>
        /// <returns></returns>
        public virtual double ToDouble()
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// 可能ならば<see cref="bool"/>型に変換します。
        /// </summary>
        /// <returns></returns>
        public virtual bool ToBoolean()
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// 値渡しの時に渡されるオブジェクトを生成します。
        /// 値型の場合はクローンが、参照型の場合には自分自身が帰ります。
        /// </summary>
        /// <returns></returns>
        public virtual StellarRoboObject AsByValValue() => this;

#pragma warning disable 1591
        public virtual object Clone() => this;

        public override bool Equals(object obj) => ReferenceEquals(this, obj);
        public override int GetHashCode() => base.GetHashCode();
#pragma warning restore 1591

    }

    /// <summary>
    /// StellarRoboのメソッドの実行結果を定義します。
    /// </summary>
    public sealed class StellarRoboFunctionResult
    {
        /// <summary>
        /// このメソッドを再開できるかどうかを取得します。
        /// </summary>
        public bool CanResume { get; }
        /// <summary>
        /// 今回のreturn/yieldに属する<see cref="StellarRoboObject"/>を取得します。
        /// </summary>
        public StellarRoboObject ReturningObject { get; }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="obj">返却する<see cref="StellarRoboObject"/></param>
        /// <param name="res">再開可能な場合はtrue</param>
        public StellarRoboFunctionResult(StellarRoboObject obj, bool res)
        {
            CanResume = res;
            ReturningObject = obj;
        }

    }
}
