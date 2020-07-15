using System;
using System.Collections.Generic;
using System.Linq;

namespace StellarRobo
{
    /// <summary>
    /// StellarRoboで定義されるクラスを定義します。
    /// </summary>
    public sealed class StellarRoboScriptClassInfo : StellarRoboClassInfo
    {
        internal List<StellarRoboScriptClassInfo> inners = new List<StellarRoboScriptClassInfo>();
        internal List<StellarRoboScriptMethodInfo> methods = new List<StellarRoboScriptMethodInfo>();
        internal List<StellarRoboScriptMethodInfo> classMethods = new List<StellarRoboScriptMethodInfo>();
        private List<string> localnames = new List<string>();
        internal IList<StellarRoboScriptLocalInfo> LocalInfos { get; } = new List<StellarRoboScriptLocalInfo>();
        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="name">クラス名</param>
        public StellarRoboScriptClassInfo(string name)
        {
            Name = name;
            Locals = localnames;
            InnerClasses = inners;
            InstanceMethods = methods;
            ClassMethods = classMethods;
        }

        /// <summary>
        /// インナークラスを追加します。
        /// </summary>
        /// <param name="klass">追加するクラス</param>
        internal void AddInnerClass(StellarRoboScriptClassInfo klass)
        {
            if (inners.Any(p => p.Name == klass.Name)) throw new ArgumentException("同じ名前のインナークラスがすでに存在します。");
            inners.Add(klass);
        }

        /// <summary>
        /// メソッドを追加します。
        /// </summary>
        /// <param name="method">追加するメソッド</param>
        internal void AddInstanceMethod(StellarRoboScriptMethodInfo method)
        {
            methods.Add(method);
        }

        /// <summary>
        /// メソッドを追加します。
        /// </summary>
        /// <param name="method">追加するメソッド</param>
        internal void AddClassMethod(StellarRoboScriptMethodInfo method)
        {
            classMethods.Add(method);
        }

        /// <summary>
        /// フィールドを追加します。
        /// </summary>
        /// <param name="local">追加するメソッド</param>
        /// <param name="exp">初期化式を定義する<see cref="StellarRoboIL"/></param>
        internal void AddLocal(string local, StellarRoboIL exp)
        {
            LocalInfos.Add(new StellarRoboScriptLocalInfo { Name = local, InitializeIL = exp });
            localnames.Add(local);
        }

        /// <summary>
        /// スクリプトのlocal宣言の情報を定義します。
        /// </summary>
        internal sealed class StellarRoboScriptLocalInfo
        {
            /// <summary>
            /// 名前を取得します。
            /// </summary>
            public string Name { get; internal set; }

            /// <summary>
            /// 初期化式の<see cref="StellarRoboIL"/>を取得します。
            /// </summary>
            public StellarRoboIL InitializeIL { get; internal set; }
        }
    }
}
