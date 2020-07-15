using StellarRobo.Type;
using System;

namespace StellarRobo
{
    /// <summary>
    /// StellarRoboで定義されるメソッドを定義します。
    /// </summary>
    public sealed class StellarRoboScriptMethodInfo : StellarRoboMethodInfo
    {

        /// <summary>
        /// このメソッドの<see cref="StellarRoboIL"/>を取得します。
        /// </summary>
        public StellarRoboIL Codes { get; set; }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="name">メソッド名</param>
        /// <param name="length">引数の数</param>
        /// <param name="vargs">可変長引数の場合はtrue</param>
        public StellarRoboScriptMethodInfo(string name, int length, bool vargs)
        {
            Name = name;
            ArgumentLength = length;
            VariableArgument = vargs;
        }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="name">メソッド名</param>
        public StellarRoboScriptMethodInfo(string name)
        {
            Name = name;
            ArgumentLength = 0;
            VariableArgument = false;
        }
    }
}
