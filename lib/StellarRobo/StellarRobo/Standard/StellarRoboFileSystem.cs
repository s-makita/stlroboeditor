using StellarRobo.Type;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarRobo.Standard
{
    /// <summary>
    /// StellarRoboでディレクトリ操作をします。
    /// </summary>
    public sealed class StellarRoboDirectory : StellarRoboObject
    {
        /// <summary>
        /// StellarRobo上でのクラス名を取得します。
        /// </summary>
        public static readonly string ClassName = "Directory";

        #region 改変不要
        /// <summary>
        /// このクラスのクラスメソッドが定義される<see cref="StellarRoboInteropClassInfo"/>を取得します。
        /// こいつを適当なタイミングで<see cref="StellarRoboModule.RegisterClass(StellarRoboInteropClassInfo)"/>に
        /// 渡してください。
        /// </summary>
        internal static StellarRoboInteropClassInfo Information { get; } = new StellarRoboInteropClassInfo(ClassName);
        #endregion

        /// <summary>
        /// 主にInformationを初期化します。
        /// コンストラクタを含む全てのクラスメソッドはここから追加してください。
        /// 逆に登録しなければコンストラクタを隠蔽できるということでもありますが。
        /// </summary>
        static StellarRoboDirectory()
        {
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("create", ClassCreateDirectory));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("delete", ClassDeleteDirectory));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("change", ClassChangeDirectory));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("exists", ClassExists));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("current", ClassCurrentDirectory));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("get_dirs", ClassGetDirectories));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("get_files", ClassGetFiles));
        }

        /// <summary>
        /// このクラスのインスタンスを初期化します。
        /// 要するにこのインスタンスがスクリプト中で参照されるので、
        /// インスタンスメソッドやプロパティの設定を忘れずにしてください。
        /// あと<see cref="StellarRoboObject.ExtraType"/>に型名をセットしておくと便利です。
        /// </summary>
        public StellarRoboDirectory()
        {
            ExtraType = ClassName;
        }

        #region クラスメソッド
        /* 
        当たり前ですがクラスメソッド呼び出しではselfはnullになります。
        selfに代入するのではなく生成したのをNoResumeで返却します。
        */

        private static StellarRoboFunctionResult ClassCreateDirectory(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var sarg = args.ExpectString(1, false);
            Directory.CreateDirectory(sarg[0]);
            return StellarRoboNil.Instance.NoResume();
        }

        private static StellarRoboFunctionResult ClassExists(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var sarg = args.ExpectString(1, false);
            return Directory.Exists(sarg[0]).AsStellarRoboBoolean().NoResume();
        }

        private static StellarRoboFunctionResult ClassChangeDirectory(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var sarg = args.ExpectString(1, false);
            Directory.SetCurrentDirectory(sarg[0]);
            return StellarRoboNil.Instance.NoResume();
        }

        private static StellarRoboFunctionResult ClassDeleteDirectory(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var sarg = args.ExpectString(1, false);
            Directory.Delete(sarg[0]);
            return StellarRoboNil.Instance.NoResume();
        }

        private static StellarRoboFunctionResult ClassCurrentDirectory(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args) => Directory.GetCurrentDirectory().AsStellarRoboString().NoResume();


        private static StellarRoboFunctionResult ClassGetFiles(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var sarg = args.ExpectString(1, true);
            if (sarg.Count >= 2)
            {
                var list = Directory.GetFiles(sarg[0], sarg[1]);
                return new StellarRoboArray(list.Select(p => p.AsStellarRoboString())).NoResume();
            }
            else
            {
                var list = Directory.GetFiles(sarg[0]);
                return new StellarRoboArray(list.Select(p => p.AsStellarRoboString())).NoResume();
            }
        }

        private static StellarRoboFunctionResult ClassGetDirectories(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var sarg = args.ExpectString(1, true);
            if (sarg.Count >= 2)
            {
                var list = Directory.GetDirectories(sarg[0], sarg[1]);
                return new StellarRoboArray(list.Select(p => p.AsStellarRoboString())).NoResume();
            }
            else
            {
                var list = Directory.GetFiles(sarg[0]);
                return new StellarRoboArray(list.Select(p => p.AsStellarRoboString())).NoResume();
            }
        }
        #endregion
    }

    /// <summary>
    /// StellarRoboでファイル操作をします。
    /// </summary>
    public sealed class StellarRoboFile : StellarRoboObject
    {
        /// <summary>
        /// StellarRobo上でのクラス名を取得します。
        /// </summary>
        public static readonly string ClassName = "File";

        #region 改変不要
        /// <summary>
        /// このクラスのクラスメソッドが定義される<see cref="StellarRoboInteropClassInfo"/>を取得します。
        /// こいつを適当なタイミングで<see cref="StellarRoboModule.RegisterClass(StellarRoboInteropClassInfo)"/>に
        /// 渡してください。
        /// </summary>
        internal static StellarRoboInteropClassInfo Information { get; } = new StellarRoboInteropClassInfo(ClassName);
        #endregion

        /// <summary>
        /// 主にInformationを初期化します。
        /// コンストラクタを含む全てのクラスメソッドはここから追加してください。
        /// 逆に登録しなければコンストラクタを隠蔽できるということでもありますが。
        /// </summary>
        static StellarRoboFile()
        {
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("delete", ClassDeleteFile));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("exists", ClassExists));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("write_text", ClassWriteText));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("read_text", ClassReadText));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("write_lines", ClassWriteLines));
            Information.AddClassMethod(new StellarRoboInteropMethodInfo("read_lines", ClassReadLines));
        }

        /// <summary>
        /// このクラスのインスタンスを初期化します。
        /// 要するにこのインスタンスがスクリプト中で参照されるので、
        /// インスタンスメソッドやプロパティの設定を忘れずにしてください。
        /// あと<see cref="StellarRoboObject.ExtraType"/>に型名をセットしておくと便利です。
        /// </summary>
        public StellarRoboFile()
        {
            ExtraType = ClassName;
        }

        #region クラスメソッド
        /* 
        当たり前ですがクラスメソッド呼び出しではselfはnullになります。
        selfに代入するのではなく生成したのをNoResumeで返却します。
        */

        private static StellarRoboFunctionResult ClassExists(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var sarg = args.ExpectString(1, false);
            return File.Exists(sarg[0]).AsStellarRoboBoolean().NoResume();
        }

        private static StellarRoboFunctionResult ClassDeleteFile(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var sarg = args.ExpectString(1, false);
            File.Delete(sarg[0]);
            return StellarRoboNil.Instance.NoResume();
        }

        private static StellarRoboFunctionResult ClassWriteText(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var sarg = args.ExpectString(2, true);
            if (sarg.Count >= 3)
            {
                File.WriteAllText(sarg[0], sarg[1], Encoding.GetEncoding(sarg[2]));
            }
            else
            {
                File.WriteAllText(sarg[0], sarg[1]);
            }
            return StellarRoboNil.Instance.NoResume();
        }

        private static StellarRoboFunctionResult ClassReadText(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var sarg = args.ExpectString(1, true);
            var result = "";
            if (sarg.Count >= 2)
            {
                result = File.ReadAllText(sarg[0], Encoding.GetEncoding(sarg[1]));
            }
            else
            {
                result = File.ReadAllText(sarg[0]);
            }
            return StellarRoboNil.Instance.NoResume();
        }

        private static StellarRoboFunctionResult ClassWriteLines(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var path = args[0].ToString();
            var list = args[1].ToStringArray();
            if (args.Length >= 3)
            {
                var enc = Encoding.GetEncoding(args[2].ToString());
                File.WriteAllLines(path, list, enc);
            }
            else
            {
                File.WriteAllLines(path, list);
            }
            return StellarRoboNil.Instance.NoResume();
        }

        private static StellarRoboFunctionResult ClassReadLines(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args)
        {
            var path = args[0].ToString();
            IList<string> list;
            if (args.Length >= 2)
            {
                var enc = Encoding.GetEncoding(args[1].ToString());
                list = File.ReadAllLines(path, enc);
            }
            else
            {
                list = File.ReadAllLines(path);
            }

            return new StellarRoboArray(list.Select(p => p.AsStellarRoboString())).NoResume();
        }
        #endregion
    }
}
