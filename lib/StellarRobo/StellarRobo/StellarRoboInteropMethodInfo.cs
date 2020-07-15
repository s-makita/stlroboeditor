using StellarRobo.Type;
using System;

namespace StellarRobo
{
    /// <summary>
    /// .NETで定義したメソッドの情報を提供します。
    /// </summary>
    public sealed class StellarRoboInteropMethodInfo : StellarRoboMethodInfo
    {

        /// <summary>
        /// 実行される<see cref="StellarRoboInteropDelegate"/>を取得します。
        /// </summary>
        public StellarRoboInteropDelegate Body { get; }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="name">メソッド名</param>
        /// <param name="length">引数の数</param>
        /// <param name="bd">メソッドのデリゲート</param>
        public StellarRoboInteropMethodInfo(string name, int length, StellarRoboInteropDelegate bd)
        {
            Name = name;
            ArgumentLength = length;
            Body = bd;
        }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="name">メソッド名</param>
        /// <param name="bd">メソッドのデリゲート</param>
        public StellarRoboInteropMethodInfo(string name, StellarRoboInteropDelegate bd) : this(name, 0, bd)
        {

        }
    }
}
