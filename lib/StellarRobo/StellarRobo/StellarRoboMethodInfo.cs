using StellarRobo.Type;
using System;

namespace StellarRobo
{
    /// <summary>
    /// StellarRoboで利用されるメソッドの規定を定義します。
    /// </summary>
    public class StellarRoboMethodInfo
    {
        /// <summary>
        /// このメソッドの名前を取得します。
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// 引数の数を取得します。
        /// </summary>
        public int ArgumentLength { get; protected set; }

        /// <summary>
        /// 可変長引数メソッドかどうかを取得します。
        /// </summary>
        public bool VariableArgument { get; protected set; }
    }
}
