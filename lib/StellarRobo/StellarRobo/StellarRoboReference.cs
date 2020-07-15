using StellarRobo.Type;
using System;

namespace StellarRobo
{
    /// <summary>
    /// StellarRoboでの値に関する全ての参照を定義します。
    /// </summary>
    public class StellarRoboReference
    {
        private StellarRoboObject obj = StellarRoboNil.Instance;
        /// <summary>
        /// 内部的に保持する<see cref="StellarRoboObject"/>を取得します。
        /// </summary>
        public virtual StellarRoboObject RawObject
        {
            get { return obj; }
            set
            {
                if (!IsLeftValue) throw new InvalidOperationException("右辺値です");
                obj = value;
            }
        }

        /// <summary>
        /// この参照が左辺値であるかどうかを取得します。
        /// </summary>
        public bool IsLeftValue { get; protected internal set; }

        /// <summary>
        /// 指定した名前の参照を取得します。
        /// </summary>
        /// <param name="name">名前</param>
        /// <returns>参照</returns>
        public StellarRoboReference GetMemberReference(string name) => RawObject.GetMemberReference(name);

        /// <summary>
        /// 右辺値を生成します。
        /// </summary>
        /// <param name="sobj">対象</param>
        /// <returns>右辺値参照</returns>
        public static StellarRoboReference Right(StellarRoboObject sobj) => new StellarRoboReference
        {
            IsLeftValue = false,
            obj = sobj
        };

        /// <summary>
        /// 右辺値を生成します。
        /// </summary>
        /// <param name="sobj">対象</param>
        /// <returns>右辺値参照</returns>
        public static StellarRoboReference Right(long sobj) => new StellarRoboReference
        {
            IsLeftValue = false,
            obj = sobj.AsStellarRoboInteger()
        };

        /// <summary>
        /// 右辺値を生成します。
        /// </summary>
        /// <param name="sobj">対象</param>
        /// <returns>右辺値参照</returns>
        public static StellarRoboReference Right(double sobj) => new StellarRoboReference
        {
            IsLeftValue = false,
            obj = sobj.AsStellarRoboFloat()
        };

        /// <summary>
        /// 右辺値を生成します。
        /// </summary>
        /// <param name="sobj">対象</param>
        /// <returns>右辺値参照</returns>
        public static StellarRoboReference Right(bool sobj) => new StellarRoboReference
        {
            IsLeftValue = false,
            obj = sobj.AsStellarRoboBoolean()
        };

        /// <summary>
        /// 右辺値を生成します。
        /// </summary>
        /// <param name="sobj">対象</param>
        /// <returns>右辺値参照</returns>
        public static StellarRoboReference Right(string sobj) => new StellarRoboReference
        {
            IsLeftValue = false,
            obj = sobj.AsStellarRoboString()
        };

        /// <summary>
        /// 右辺値を生成します。
        /// </summary>
        /// <param name="self">属するインスタンス</param>
        /// <param name="sobj">対象</param>
        /// <returns>右辺値参照</returns>
        public static StellarRoboReference Right(StellarRoboObject self, StellarRoboInteropDelegate sobj) => new StellarRoboReference
        {
            IsLeftValue = false,
            obj = new StellarRoboInteropFunction(self, sobj)
        };

        /// <summary>
        /// 左辺値を生成します。
        /// </summary>
        /// <param name="sobj">対象</param>
        /// <returns>左辺値参照</returns>
        public static StellarRoboReference Left(StellarRoboObject sobj) => new StellarRoboReference
        {
            IsLeftValue = true,
            obj = sobj
        };

        /// <summary>
        /// 左辺値を生成します。
        /// </summary>
        /// <param name="sobj">対象</param>
        /// <returns>左辺値参照</returns>
        public static StellarRoboReference Left(long sobj) => new StellarRoboReference
        {
            IsLeftValue = true,
            obj = sobj.AsStellarRoboInteger()
        };

        /// <summary>
        /// 左辺値を生成します。
        /// </summary>
        /// <param name="sobj">対象</param>
        /// <returns>左辺値参照</returns>
        public static StellarRoboReference Left(double sobj) => new StellarRoboReference
        {
            IsLeftValue = true,
            obj = sobj.AsStellarRoboFloat()
        };

        /// <summary>
        /// 左辺値を生成します。
        /// </summary>
        /// <param name="sobj">対象</param>
        /// <returns>左辺値参照</returns>
        public static StellarRoboReference Left(bool sobj) => new StellarRoboReference
        {
            IsLeftValue = true,
            obj = sobj.AsStellarRoboBoolean()
        };

        /// <summary>
        /// 左辺値を生成します。
        /// </summary>
        /// <param name="sobj">対象</param>
        /// <returns>左辺値参照</returns>
        public static StellarRoboReference Left(string sobj) => new StellarRoboReference
        {
            IsLeftValue = true,
            obj = sobj.AsStellarRoboString()
        };

        /// <summary>
        /// 左辺値を生成します。
        /// </summary>
        /// <param name="self">属するインスタンス</param>
        /// <param name="sobj">対象</param>
        /// <returns>左辺値参照</returns>
        public static StellarRoboReference Left(StellarRoboObject self, StellarRoboInteropDelegate sobj) => new StellarRoboReference
        {
            IsLeftValue = true,
            obj = new StellarRoboInteropFunction(self, sobj)
        };
    }
}
