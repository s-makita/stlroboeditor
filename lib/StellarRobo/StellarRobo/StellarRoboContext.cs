using StellarRobo.Type;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
#if EDITOR
using ICSharpCode.AvalonEdit;
using StellarRoboEditor;
#endif
using System.IO;
using System.Text;

namespace StellarRobo
{
    /// <summary>
    /// StellarRoboを実行するためのスタックフレームなどのセットを提供します。
    /// </summary>
    public sealed class StellarRoboContext : IEnumerator<StellarRoboObject>
    {
        /// <summary>
        /// 属する<see cref="StellarRoboModule"/>を取得します。
        /// </summary>
        public StellarRoboModule Module { get; private set; }

        /// <summary>
        /// 現在呼び出し中のオブジェクトを取得します。
        /// </summary>
        public StellarRoboObject TargetObject { get; private set; } = StellarRoboNil.Instance;

        /// <summary>
        /// 可変長引数を含めた現在設定されている全ての引数を取得します。
        /// </summary>
        public IList<StellarRoboObject> Arguments { get; private set; } = new List<StellarRoboObject>();

        /// <summary>
        /// 現在の呼び出しが継続中であるかどうかを取得します。
        /// </summary>
        public bool IsResuming { get; private set; }

        /// <summary>
        /// デバックモードかを取得・設定します。
        /// </summary>
        public bool IsDebug { get; set; }

        /// <summary>
        /// ダンプモードかを取得・設定します。
        /// </summary>
        public bool IsDump { get; set; }

        /// <summary>
        /// ダンプ出力先
        /// </summary>
        public string DumpPath { get; set; }

        /// <summary>
        /// Trueに設定された場合には直ちに処理を停止し終了する
        /// </summary>
        public bool ForcedStop { get; set; } = false;

        /// <summary>
        /// 最後に返却されたオブジェクトを取得します。
        /// </summary>
        public StellarRoboObject Current { get; private set; }

        /// <summary>
        /// 実行中のILCodeを取得します
        /// </summary>
        public StellarRoboILCode NowILCode { get; set; }

        public CountdownEvent countdown { get; set; } = new CountdownEvent(1);

#if EDITOR
        public StellarRoboEditor.FormMain.DelegateSetDebugLine SetDebugLine { get; set; }
        public FormMain.DelegateSetMenuState SetMenuState { get; set; }

        public List<int> BreakPoint { get; set; } = new List<int>();
#endif
        /// <param name="module"></param>
        internal StellarRoboContext(StellarRoboModule module)
        {
            Module = module;
        }

        object IEnumerator.Current => Current;

        /// <summary>
        /// 現在の実行状態を破棄し、先頭からの実行に戻します。
        /// </summary>
        public void Reset()
        {
            IsResuming = false;
            IsDebug = false;
            ForcedStop = false;

            //Dumpファイル
            string now = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            DumpPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, string.Format("dump{0}.log", now));
        }

        /// <summary>
        /// メソッドの呼び出しを新規に設定します。
        /// </summary>
        /// <param name="obj">呼び出すメソッドに相当する<see cref="StellarRoboObject"/></param>
        /// <param name="args">引数</param>
        public void Initialize(StellarRoboObject obj, IList<StellarRoboObject> args)
        {
            Reset();
            TargetObject = obj;
            Arguments = args;
        }

        /// <summary>
        /// メソッドの呼び出しを新規に設定します。
        /// </summary>
        /// <param name="obj">呼び出すメソッドに相当する<see cref="StellarRoboObject"/></param>
        /// <param name="args">引数</param>
        public void Initialize(StellarRoboObject obj, params StellarRoboObject[] args)
        {
            Reset();
            TargetObject = obj;
            Arguments = args;
        }

        /// <summary>
        /// 指定された<see cref="StellarRoboObject"/>を継続なしで実行し、
        /// 最初に返却された<see cref="StellarRoboObject"/>を返します。
        /// </summary>
        /// <param name="obj">呼び出すメソッドに相当する<see cref="StellarRoboObject"/></param>
        /// <param name="args">引数</param>
        /// <returns>
        /// 最初に返却された<see cref="StellarRoboObject"/>。
        /// 返り値がない場合は<see cref="StellarRoboNil.Instance"/>。
        /// </returns>
        public StellarRoboObject CallInstant(StellarRoboObject obj, IList<StellarRoboObject> args) => obj.Call(this, args.ToArray()).ReturningObject;

        /// <summary>
        /// 指定された<see cref="StellarRoboObject"/>を継続なしで実行し、
        /// 最初に返却された<see cref="StellarRoboObject"/>を返します。
        /// </summary>
        /// <param name="obj">呼び出すメソッドに相当する<see cref="StellarRoboObject"/></param>
        /// <param name="args">引数</param>
        /// <returns>
        /// 最初に返却された<see cref="StellarRoboObject"/>。
        /// 返り値がない場合は<see cref="StellarRoboNil.Instance"/>。
        /// </returns>
        public StellarRoboObject CallInstant(StellarRoboObject obj, params StellarRoboObject[] args) => obj.Call(this, args).ReturningObject;

        /// <summary>
        /// 指定されたILを式として実行し、結果を返します。
        /// </summary>
        /// <param name="il">実行する<see cref="StellarRoboIL"/></param>
        /// <returns>結果</returns>
        public StellarRoboObject ExecuteExpressionIL(StellarRoboIL il)
        {
            var s = new StellarRoboStackFrame(this, il);
            s.Execute();
            return s.ReturningObject;
        }

        /// <summary>
        /// 指定されている<see cref="StellarRoboObject"/>を呼び出し、次の値を取得します。
        /// </summary>
        /// <returns>継続可能な場合はtrue、それ以外の場合はfalse。</returns>
        public bool MoveNext()
        {
            //Dumpは出力するか？
            if (IsDump)
            {
                //必要な情報を取得
                var screen = System.Windows.Forms.Screen.PrimaryScreen;
                int width = screen.Bounds.Width;
                int height = screen.Bounds.Height;

                #region PC情報取得
                string os = string.Empty;
                string ver = string.Empty;
                string build = string.Empty;
                string totalMemory = string.Empty;
                string freePhysicalMemory = string.Empty;
                string totalVirtualMemorySize = string.Empty;
                string freeVirtualMemory = string.Empty;
                int nVal;

                System.Management.ManagementClass mc = new System.Management.ManagementClass("Win32_OperatingSystem");
                System.Management.ManagementObjectCollection moc = mc.GetInstances();

                foreach (System.Management.ManagementObject mo in moc)
                {
                    // OSの情報
                    // エディション
                    os = mo["Caption"].ToString();

                    // バージョン
                    ver = mo["Version"].ToString();

                    // ビルド番号
                    build = mo["BuildNumber"].ToString();

                    // メモリー情報
                    // 合計物理メモリー
                    nVal = System.Convert.ToInt32(mo["TotalVisibleMemorySize"]) / 1024;    // 単位 KB -> MB
                    totalMemory = nVal.ToString();

                    // 利用可能な物理メモリー
                    nVal = System.Convert.ToInt32(mo["FreePhysicalMemory"]) / 1024;    // 単位 KB -> MB
                    freePhysicalMemory = nVal.ToString();

                    // 合計仮想メモリー
                    nVal = System.Convert.ToInt32(mo["TotalVirtualMemorySize"]) / 1024;    // 単位 KB -> MB
                    totalVirtualMemorySize = nVal.ToString();

                    // 利用可能な仮想メモリー
                    nVal = System.Convert.ToInt32(mo["FreeVirtualMemory"]) / 1024;    // 単位 KB -> MB
                    freeVirtualMemory = nVal.ToString();
                }
                #endregion

                //ダンプ内容作成
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(string.Format("OS: {0}(version:{1} BuildNumber:{2})\n", os, ver, build));
                stringBuilder.Append(string.Format( "ScreenSize:{0}x{1}\n",width.ToString(),height.ToString()));
                stringBuilder.Append(string.Format("TotalVisibleMemorySize:{0}\n", totalMemory));
                stringBuilder.Append(string.Format("FreePhysicalMemory:{0}\n", freePhysicalMemory));
                stringBuilder.Append(string.Format("TotalVirtualMemorySize:{0}\n", totalVirtualMemorySize));
                stringBuilder.Append(string.Format("FreeVirtualMemory:{0}\n", freeVirtualMemory));

                //ダンプ出力
                writeDump(stringBuilder.ToString());
            }
            var r = TargetObject.Call(this, IsResuming ? null : Arguments.ToArray());
            Current = r.ReturningObject;
            return IsResuming = r.CanResume;
        }

        /// <summary>
        /// このコンテキストを破棄し、参照を開放します。
        /// </summary>
        public void Dispose()
        {
            Arguments.Clear();
            TargetObject = null;
            Module = null;
        }

        public void writeDump(string dumpData)
        {
            //Dumpファイルを開く
            using (FileStream fileStream = new FileStream(DumpPath, FileMode.OpenOrCreate | FileMode.Append))
            using (StreamWriter streamWriter = new StreamWriter(fileStream))
            {
                //Dump内容書き込み
                streamWriter.Write(dumpData);
            }
        }
    }
}
