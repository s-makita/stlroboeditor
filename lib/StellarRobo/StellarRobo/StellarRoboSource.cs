using StellarRobo.Type;
using System.Collections.Generic;

namespace StellarRobo
{
    /// <summary>
    /// 1つのソースコードを元にしたクラスとメソッドの集合体を定義します。
    /// </summary>
    public sealed class StellarRoboSource
    {
        internal List<string> uses = new List<string>();
        /// <summary>
        /// useによるインポート対象を取得します。
        /// </summary>
        public IReadOnlyList<string> Uses => uses;

        internal List<StellarRoboScriptClassInfo> classes = new List<StellarRoboScriptClassInfo>();
        /// <summary>
        /// クラスを取得します。
        /// </summary>
        public IReadOnlyList<StellarRoboScriptClassInfo> Classes => classes;

        internal List<StellarRoboScriptMethodInfo> methods = new List<StellarRoboScriptMethodInfo>();
        /// <summary>
        /// トップレベルに定義されたメソッドを取得します。
        /// </summary>
        public IReadOnlyList<StellarRoboScriptMethodInfo> TopLevelMethods => methods;

    }
}
