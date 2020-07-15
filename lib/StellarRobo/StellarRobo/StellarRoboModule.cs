using StellarRobo.Analyze;
using StellarRobo.Type;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Text;

namespace StellarRobo
{
    /// <summary>
    /// StellarRoboのモジュール(名前空間)を定義します。
    /// </summary>
    public sealed class StellarRoboModule
    {
        /// <summary>
        /// このインスタンスが定義されている<see cref="StellarRoboEnvironment"/>を取得します。
        /// </summary>
        public StellarRoboEnvironment Environment { get; internal set; }

        /// <summary>
        /// このインスタンスの名前を取得します。
        /// </summary>
        public string Name { get; }

        internal Dictionary<string, StellarRoboReference> globalObjects = new Dictionary<string, StellarRoboReference>();
        /// <summary>
        /// このモジュール全体で定義されるオブジェクトを取得します。
        /// </summary>
        public IReadOnlyDictionary<string, StellarRoboReference> GlobalObjects => globalObjects;

        internal List<StellarRoboClassInfo> classes = new List<StellarRoboClassInfo>();
        internal List<StellarRoboReference> classReferences = new List<StellarRoboReference>();
        /// <summary>
        /// このモジュールで定義されているクラスを取得します。
        /// </summary>
        public IReadOnlyList<StellarRoboClassInfo> Classes => classes;

        internal List<StellarRoboMethodInfo> topMethods = new List<StellarRoboMethodInfo>();
        internal List<StellarRoboReference> methodReferences = new List<StellarRoboReference>();

        /// <summary>
        /// このモジュールで定義されているトップレベルメソッドを取得します。
        /// </summary>
        public IReadOnlyList<StellarRoboMethodInfo> TopLevelMethods => topMethods;

        /// <summary>
        /// 拡張ライブラリ検索の際のディレクトリを取得します。
        /// </summary>
        public string BaseDirectory { get; } =
            Path.Combine(Path.GetDirectoryName(typeof(StellarRoboModule).Assembly.Location), "kclib");

        /// <summary>
        /// 指定した名前を持つトップレベルの<see cref="StellarRoboObject"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public StellarRoboObject this[string name] => GetReference(name).RawObject;

        /// <summary>
        /// <see cref="StellarRoboModule"/>の新しいインスタンスを生成します。
        /// </summary>
        internal StellarRoboModule(string name)
        {
            Name = name;
            RegisterFunction(Eval, "eval");
        }

        /// <summary>
        /// 新しい<see cref="StellarRoboContext"/>を生成します。
        /// </summary>
        /// <returns>生成</returns>
        public StellarRoboContext CreateContext() => new StellarRoboContext(this);

        /// <summary>
        /// 定義されているオブジェクト・メソッド・クラスの中から検索し、参照を取得・設定します。
        /// </summary>
        /// <param name="name">キー</param>
        /// <returns>なければ<see cref="StellarRoboNil.Reference"/></returns>
        public StellarRoboReference GetReference(string name)
        {
            int idx = 0;
            if (GlobalObjects.ContainsKey(name)) return GlobalObjects[name];
            if ((idx = topMethods.FindLastIndex(p => p.Name == name)) >= 0) return methodReferences[idx];
            if ((idx = classes.FindLastIndex(p => p.Name == name)) >= 0) return classReferences[idx];
            return StellarRoboNil.Reference;
        }

        private StellarRoboFunctionResult Eval(StellarRoboContext ctx, StellarRoboObject self, StellarRoboObject[] args) => new StellarRoboContext(this).ExecuteExpressionIL(new StellarRoboPrecompiler().PrecompileExpression(new StellarRoboParser().ParseAsExpression(new StellarRoboLexer().AnalyzeFromSource(args[0].ToString())))).NoResume();

        #region Do****
        /// <summary>
        /// ファイルを読み込み、内容を登録します。
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        public StellarRoboObject DoFile(string fileName) => DoFile(fileName, Encoding.Default);

        /// <summary>
        /// 指定したエンコードでファイルを読み込み、内容を登録します。
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="enc">読み込む際に利用する<see cref="Encoding"/></param>
        public StellarRoboObject DoFile(string fileName, Encoding enc)
        {
            var fp = Path.GetFullPath(fileName);
            var le = Environment.Lexer.AnalyzeFromFile(fileName, enc);
            if (!le.Success) throw new StellarRoboParseException(le.Error);
            var ast = Environment.Parser.Parse(le);
            if (!ast.Success) throw new StellarRoboParseException(le.Error);
            var src = Environment.Precompiler.PrecompileAll(ast);
            RegisterSource(src);
            if (this["main"] != StellarRoboNil.Instance)
            {
                return new StellarRoboContext(this).CallInstant(this["main"]);
            }
            else
            {
                return StellarRoboNil.Instance;
            }
        }

        /// <summary>
        /// 指定したソースコードを直接解析し、実行します。
        /// </summary>
        /// <param name="source">ソースコード</param>
        public StellarRoboObject DoString(string source)
        {
            var le = Environment.Lexer.AnalyzeFromSource(source);
            if (!le.Success) throw new StellarRoboParseException(le.Error);
            var ast = Environment.Parser.Parse(le);
            if (!ast.Success) throw new StellarRoboParseException(le.Error);
            var src = Environment.Precompiler.PrecompileAll(ast);
            RegisterSource(src);
            if (this["main"] != StellarRoboNil.Instance)
            {
                return new StellarRoboContext(this).CallInstant(this["main"]);
            }
            else
            {
                return StellarRoboNil.Instance;
            }
        }

        /// <summary>
        /// 指定したソースコードを式として解析し、実行します。
        /// </summary>
        /// <param name="source">ソースコード</param>
        public StellarRoboObject DoExpressionString(string source)
        {
            var le = Environment.Lexer.AnalyzeFromSource(source);
            if (!le.Success) throw new StellarRoboParseException(le.Error);
            var ast = Environment.Parser.ParseAsExpression(le);
            if (!ast.Success) throw new StellarRoboParseException(le.Error);
            var src = Environment.Precompiler.PrecompileExpression(ast);
            return new StellarRoboContext(this).ExecuteExpressionIL(src);
        }
        #endregion

        #region Registerers
        /// <summary>
        /// プリコンパイルしたソースコードを登録します。
        /// </summary>
        /// <param name="src">登録する<see cref="StellarRoboSource"/></param>
        public void RegisterSource(StellarRoboSource src)
        {
            ProcessUseDirective(src);
            foreach (var c in src.Classes)
            {
                classes.Add(c);
                classReferences.Add(StellarRoboReference.Right(new StellarRoboScriptClassObject(c)));
            }
            foreach (var m in src.TopLevelMethods)
            {
                topMethods.Add(m);
                methodReferences.Add(StellarRoboReference.Right(new StellarRoboScriptFunction(StellarRoboNil.Instance, m)));
            }
        }

        /// <summary>
        /// .NET上のStellarRobo連携クラスを登録します。
        /// </summary>
        /// <param name="klass"></param>
        public void RegisterClass(StellarRoboInteropClassInfo klass)
        {
            classes.Add(klass);
            classReferences.Add(StellarRoboReference.Right(new StellarRoboInteropClassObject(klass)));
        }

        /// <summary>
        /// .NETメソッドをトップレベルに登録します。
        /// </summary>
        /// <param name="method">登録する<see cref="StellarRoboInteropMethodInfo"/>形式のメソッド</param>
        public void RegisterMethod(StellarRoboInteropMethodInfo method)
        {
            topMethods.Add(method);
            methodReferences.Add(StellarRoboReference.Right(StellarRoboNil.Instance, method.Body));
        }


        /// <summary>
        /// .NETメソッドをトップレベルに登録します。
        /// </summary>
        /// <param name="func">登録する<see cref="StellarRoboInteropDelegate"/>形式のメソッド</param>
        /// <param name="name">メソッド名</param>
        public void RegisterFunction(StellarRoboInteropDelegate func, string name)
        {
            var fo = new StellarRoboInteropMethodInfo(name, func);
            topMethods.Add(fo);
            methodReferences.Add(StellarRoboReference.Right(StellarRoboNil.Instance, func));
        }

        /// <summary>
        /// .NETメソッドをトップレベルに登録します。
        /// </summary>
        /// <param name="func">登録する<see cref="Func{T1, T2, TResult}"/>形式のメソッド</param>
        /// <param name="name">メソッド名</param>
        public void RegisterFunction(Func<StellarRoboObject, StellarRoboObject[], StellarRoboObject> func, string name)
        {
            StellarRoboInteropDelegate wp =
                (ctx, self, args) => func(self, args).NoResume();
            var fo = new StellarRoboInteropMethodInfo(name, wp);
            topMethods.Add(fo);
            methodReferences.Add(StellarRoboReference.Right(StellarRoboNil.Instance, wp));
        }

        /// <summary>
        /// .NETメソッドをトップレベルに登録します。
        /// </summary>
        /// <param name="func">登録する<see cref="Func{T1, T2, TResult}"/>形式のメソッド</param>
        /// <param name="name">メソッド名</param>
        public void RegisterInt32Function(Func<StellarRoboObject, StellarRoboObject[], int> func, string name)
        {
            StellarRoboInteropDelegate wp =
                (ctx, self, args) => func(self, args).AsStellarRoboInteger().NoResume();
            var fo = new StellarRoboInteropMethodInfo(name, wp);
            topMethods.Add(fo);
            methodReferences.Add(StellarRoboReference.Right(StellarRoboNil.Instance, wp));
        }

        /// <summary>
        /// .NETメソッドをトップレベルに登録します。
        /// </summary>
        /// <param name="func">登録する<see cref="Func{T1, T2, TResult}"/>形式のメソッド</param>
        /// <param name="name">メソッド名</param>
        public void RegisterInt64Function(Func<StellarRoboObject, StellarRoboObject[], long> func, string name)
        {
            StellarRoboInteropDelegate wp =
                (ctx, self, args) => func(self, args).AsStellarRoboInteger().NoResume();
            var fo = new StellarRoboInteropMethodInfo(name, wp);
            topMethods.Add(fo);
            methodReferences.Add(StellarRoboReference.Right(StellarRoboNil.Instance, wp));
        }

        /// <summary>
        /// .NETメソッドをトップレベルに登録します。
        /// </summary>
        /// <param name="func">登録する<see cref="Func{T1, T2, TResult}"/>形式のメソッド</param>
        /// <param name="name">メソッド名</param>
        public void RegisterSingleFunction(Func<StellarRoboObject, StellarRoboObject[], float> func, string name)
        {
            StellarRoboInteropDelegate wp =
                (ctx, self, args) => ((double)func(self, args)).AsStellarRoboFloat().NoResume();
            var fo = new StellarRoboInteropMethodInfo(name, wp);
            topMethods.Add(fo);
            methodReferences.Add(StellarRoboReference.Right(StellarRoboNil.Instance, wp));
        }

        /// <summary>
        /// .NETメソッドをトップレベルに登録します。
        /// </summary>
        /// <param name="func">登録する<see cref="Func{T1, T2, TResult}"/>形式のメソッド</param>
        /// <param name="name">メソッド名</param>
        public void RegisterDoubleFunction(Func<StellarRoboObject, StellarRoboObject[], double> func, string name)
        {
            StellarRoboInteropDelegate wp =
                (ctx, self, args) => func(self, args).AsStellarRoboFloat().NoResume();
            var fo = new StellarRoboInteropMethodInfo(name, wp);
            topMethods.Add(fo);
            methodReferences.Add(StellarRoboReference.Right(StellarRoboNil.Instance, wp));
        }

        /// <summary>
        /// .NETメソッドをトップレベルに登録します。
        /// </summary>
        /// <param name="func">登録する<see cref="Func{T1, T2, TResult}"/>形式のメソッド</param>
        /// <param name="name">メソッド名</param>
        public void RegisterBooleanFunction(Func<StellarRoboObject, StellarRoboObject[], bool> func, string name)
        {
            StellarRoboInteropDelegate wp =
                (ctx, self, args) => func(self, args).AsStellarRoboBoolean().NoResume();
            var fo = new StellarRoboInteropMethodInfo(name, wp);
            topMethods.Add(fo);
            methodReferences.Add(StellarRoboReference.Right(StellarRoboNil.Instance, wp));
        }

        /// <summary>
        /// .NETメソッドをトップレベルに登録します。
        /// </summary>
        /// <param name="func">登録する<see cref="Func{T1, T2, TResult}"/>形式のメソッド</param>
        /// <param name="name">メソッド名</param>
        public void RegisterStringFunction(Func<StellarRoboObject, StellarRoboObject[], string> func, string name)
        {
            StellarRoboInteropDelegate wp =
                (ctx, self, args) => func(self, args).AsStellarRoboString().NoResume();
            var fo = new StellarRoboInteropMethodInfo(name, wp);
            topMethods.Add(fo);
            methodReferences.Add(StellarRoboReference.Right(StellarRoboNil.Instance, wp));
        }

        private void ProcessUseDirective(StellarRoboSource src)
        {
            var cur = Directory.GetCurrentDirectory();
            var asm = Path.GetDirectoryName(typeof(StellarRoboModule).Assembly.Location);
            var lex = new StellarRoboLexer();
            var par = new StellarRoboParser();
            var prc = new StellarRoboPrecompiler();
            foreach (var text in src.Uses)
            {
                var arg = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                switch (arg[0])
                {
                    case "import":
                        var it = Path.Combine(cur, arg[1]);
                        Directory.SetCurrentDirectory(Path.GetDirectoryName(it));
                        var s = prc.PrecompileAll(par.Parse(lex.AnalyzeFromFile(it)));
                        RegisterSource(s);
                        Directory.SetCurrentDirectory(cur);
                        break;
                    case "stdlib":
                        var lt = Path.Combine(asm, "lib");
                        Directory.SetCurrentDirectory(Path.GetDirectoryName(lt));
                        var s2 = prc.PrecompileAll(par.Parse(lex.AnalyzeFromFile(lt)));
                        RegisterSource(s2);
                        Directory.SetCurrentDirectory(cur);
                        break;
                }
            }
        }
        #endregion

        #region Stdlib Register
        /// <summary>
        /// 標準ライブラリを登録します。
        /// </summary>
        public void RegisterStandardLibraries()
        {
            RegisterClass(StellarRoboString.Information);
            /*
            RegisterClass(StellarRoboConvert.Information);
            RegisterClass(StellarRoboList.Information);
            RegisterClass(StellarRoboDictionary.Information);
            RegisterClass(StellarRoboDirectory.Information);
            RegisterClass(StellarRoboFile.Information);
            RegisterClass(StellarRoboMath.Information);
            RegisterClass(StellarRoboDynamicLibrary.Information);
            RegisterClass(StellarRoboHash.Information);
            RegisterClass(StellarRoboRegex.Information);
            RegisterClass(StellarRoboExtensionLibrary.Information);
            RegisterClass(StellarRoboDateTime.Information);
            RegisterClass(StellarRoboTimeSpan.Information);
            */
        }
        #endregion
    }
}
