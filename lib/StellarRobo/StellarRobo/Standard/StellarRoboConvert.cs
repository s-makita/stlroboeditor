using StellarRobo.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarRobo.Standard
{
    /// <summary>
    /// 値変換を担います。
    /// </summary>
    public sealed class StellarRoboConvert : StellarRoboObject
    {
        /// <summary>
        /// StellarRobo上でのクラス名を取得します。
        /// </summary>
        public static readonly string ClassName = "Convert";

        #region 改変不要
        /// <summary>
        /// このクラスのクラスメソッドが定義される<see cref="StellarRoboInteropClassInfo"/>を取得します。
        /// こいつを適当なタイミングで<see cref="StellarRoboModule.RegisterClass(StellarRoboInteropClassInfo)"/>に
        /// 渡してください。
        /// </summary>
        internal static StellarRoboInteropClassInfo Information { get; } = new StellarRoboInteropClassInfo(ClassName);
        #endregion

        #region overrideメンバー
        /// <summary>
        /// 主にInformationを初期化します。
        /// コンストラクタを含む全てのクラスメソッドはここから追加してください。
        /// 逆に登録しなければコンストラクタを隠蔽できるということでもありますが。
        /// </summary>
        static StellarRoboConvert()
        {
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("to_int", ToInteger));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("to_float", ToFloat));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("to_bool", ToBoolean));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("to_str", ToString));
        }

        /// <summary>
        /// このクラスのインスタンスを初期化します。
        /// 要するにこのインスタンスがスクリプト中で参照されるので、
        /// インスタンスメソッドやプロパティの設定を忘れずにしてください。
        /// あと<see cref="StellarRoboObject.ExtraType"/>に型名をセットしておくと便利です。
        /// </summary>
        public StellarRoboConvert()
        {
            ExtraType = ClassName;
        }
        #endregion

        #region クラスメソッド
        /* 
        当たり前ですがクラスメソッド呼び出しではselfはnullになります。
        selfに代入するのではなく生成したのをNoResumeで返却します。
        */

        private static StellarRoboFunctionResult ToBoolean(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            switch (args[0].Type)
            {
                case TypeCode.Boolean:
                    return args[0].NoResume();
                case TypeCode.Int64:
                    return Convert.ToBoolean(args[0].ToInt64()).AsStellarRoboBoolean().NoResume();
                case TypeCode.Double:
                    return Convert.ToBoolean(args[0].ToDouble()).AsStellarRoboBoolean().NoResume();
                case TypeCode.String:
                    return Convert.ToBoolean(args[0].ToString()).AsStellarRoboBoolean().NoResume();
                default:
                    return false.AsStellarRoboBoolean().NoResume();
            }
        }

        private static StellarRoboFunctionResult ToInteger(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            switch (args[0].Type)
            {
                case TypeCode.Boolean:
                    return Convert.ToInt64(args[0].ToBoolean()).AsStellarRoboInteger().NoResume();
                case TypeCode.Int64:
                    return args[0].NoResume();
                case TypeCode.Double:
                    return Convert.ToInt64(args[0].ToDouble()).AsStellarRoboInteger().NoResume();
                case TypeCode.String:
                    return Convert.ToInt64(args[0].ToString()).AsStellarRoboInteger().NoResume();
                default:
                    return 0.AsStellarRoboInteger().NoResume();
            }
        }

        private static StellarRoboFunctionResult ToFloat(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            switch (args[0].Type)
            {
                case TypeCode.Boolean:
                    return Convert.ToDouble(args[0].ToBoolean()).AsStellarRoboFloat().NoResume();
                case TypeCode.Int64:
                    return Convert.ToDouble(args[0].ToInt64()).AsStellarRoboFloat().NoResume();
                case TypeCode.Double:
                    return args[0].NoResume();
                case TypeCode.String:
                    return Convert.ToDouble(args[0].ToString()).AsStellarRoboFloat().NoResume();
                default:
                    return 0.0.AsStellarRoboFloat().NoResume();
            }
        }

        private static StellarRoboFunctionResult ToString(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args) => args[0].ToString().AsStellarRoboString().NoResume();
        #endregion
    }
}

