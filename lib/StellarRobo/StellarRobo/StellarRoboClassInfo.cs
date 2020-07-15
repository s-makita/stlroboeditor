using System.Collections.Generic;

namespace StellarRobo
{
    /// <summary>
    /// Kecakanoahで利用されるクラスの情報を提供します。
    /// </summary>
    public abstract class StellarRoboClassInfo
    {
        /// <summary>
        /// クラス名を取得します。
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// インナークラスを取得します。
        /// </summary>
        public IReadOnlyList<StellarRoboClassInfo> InnerClasses { get; protected set; }

        /// <summary>
        /// インスタンスメソッドを取得します。
        /// </summary>
        public IReadOnlyList<StellarRoboMethodInfo> InstanceMethods { get; protected set; }

        /// <summary>
        /// クラスメソッドを取得します。
        /// </summary>
        public IReadOnlyList<StellarRoboMethodInfo> ClassMethods { get; protected set; }

        /// <summary>
        /// 予め定義されるフィールドを定義します。
        /// </summary>
        public IReadOnlyList<string> Locals { get; protected set; }

        /// <summary>
        /// 継承元クラスの名前を取得します。
        /// </summary>
        public string BaseClass { get; protected set; }
    }
}
