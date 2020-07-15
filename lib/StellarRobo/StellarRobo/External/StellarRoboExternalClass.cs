using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarRobo.External
{
    /// <summary>
    /// プラグインクラスを定義する際の情報を定義します。
    /// </summary>
    public sealed class StellarRoboExternalClassInfo
    {
        /// <summary>
        /// このクラスが静的クラスで、インスタンスメソッドを持たない場合はtrueを返します。
        /// </summary>
        public bool IsStaticClass { get; set; }

        /// <summary>
        /// このクラスの名前を取得します。
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// このクラスを定義する<see cref="StellarRoboInteropClassInfo"/>を取得します。
        /// </summary>
        public StellarRoboInteropClassInfo Information { get; set; }
    }

    /// <summary>
    /// <see cref="StellarRoboExternalClassInfo"/>を取得するためのデリゲートを定義します。
    /// </summary>
    /// <returns></returns>
    public delegate StellarRoboExternalClassInfo ExternalInfoFetcher();
}
