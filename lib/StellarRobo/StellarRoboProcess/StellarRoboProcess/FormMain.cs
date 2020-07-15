using StellarRobo;
using StellarRobo.Analyze;
using StellarRobo.Type;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using StellarLink.Windows.GlobalHook;
using UserException;

namespace StellarRoboProcess
{
    public partial class FormMain : Form
    {
        #region API
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr FindResource(IntPtr hModule, string lpName, string lpType);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);
        [DllImport("kernel32.dll")]
        static extern IntPtr LockResource(IntPtr hResData);
        #endregion

        #region 変数
        private StellarRoboContext ctx;                 //Script実行環境 ※処理の関係上グローバル変数にせざるを得ない
        string mineTypeList = string.Empty;

        #endregion

        public FormMain()
        {
            InitializeComponent();
            //MineType一覧作成
            mineTypeList = StellarRobo.MineTypeData.GetMineType(StellarRoboProcess.MineType.MineTypeResource.ResourceManager, Application.StartupPath);

            //notifyIconのPopupを設定する
            notifyIcon.Text = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //アプリを終了させる
            Application.Exit();
        }

        #region Scriptソース関連
        private string GetSource(string ResourceName)
        {
            //変数宣言
            string SourceData = string.Empty;
            string FileName = Application.ExecutablePath;
            string AES_IV = AESCryption.AESCryption.AES_IV;
            string AES_KEY = AESCryption.AESCryption.AES_KEY;

            //モジュール読み込み
            IntPtr handle = LoadLibrary(FileName);
            if (handle == IntPtr.Zero)
            {
                //終了(異常)
                return string.Empty;
            }

            //リソースの検索
            IntPtr findRes = FindResource(handle, ResourceName, "RT_STRING");
            if (findRes == IntPtr.Zero)
            {
                //終了(異常)
                return string.Empty;
            }

            //リソース読み込み
            IntPtr loadRes = LoadResource(handle, findRes);
            if (loadRes == IntPtr.Zero)
            {
                //終了(異常)
                return string.Empty;
            }

            //リソースロック
            IntPtr lockRes = LockResource(loadRes);
            if (lockRes == IntPtr.Zero)
            {
                //終了(異常)
                return string.Empty;
            }

            //目的のリソースのサイズを取得
            uint sizeRes = SizeofResource(handle, findRes);

            //リソースを取得する
            byte[] Resource = new byte[sizeRes];
            Marshal.Copy(lockRes, Resource, 0, (int)sizeRes);

            //Bytes → String
            SourceData = Encoding.UTF8.GetString(Resource);

            //戻り値設定
            return SourceData;
        }
        #endregion

        #region Script関連
        public void ScriptRun()
        {
            //変数宣言
            string Encrypt_Data = string.Empty;     //ソースファイル取得用(暗号化)
            string Decrypt_Data = string.Empty;     //ソースファイル取得用(復号化)
            string Plain_Text = string.Empty;       //ソースファイル
            string[] args;                          //引数用
            string loggerFile = string.Empty;       //ログファイル用
            bool InterruptFlg = false;              //割り込み用
            int dumpMode = 0;

            //設定ファイルパス取得
            string setting_file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StellarRobo.xml");

            //ファイル存在確認
            if (File.Exists(setting_file))
            {
                //設定ファイル読み込み
                StellarRoboSetting stellarRoboSetting = StellarRoboSetting.LoadData(setting_file);

                //Dumpモード設定
                if(!int.TryParse(stellarRoboSetting.Dump.ToString(),out dumpMode))
                {
                    //変換出来ない場合には、Dumpモードは無効に設定する
                    dumpMode = 0;
                }
            }
            else
            {
                //空の設定ファイルを作成する
                StellarRoboSetting stellarRoboSetting = new StellarRoboSetting();
                StellarRoboSetting.SaveData(setting_file, stellarRoboSetting);
            }

            //コマンドライン引数を取得
            args = Environment.GetCommandLineArgs();

            //リソースより、割り込みフラグを取得する
            InterruptFlg = Convert.ToBoolean(GetSource("Interrupt"));

            //リソースよりソースデータを取得する
            Encrypt_Data = GetSource("FileData");

            //ソースデータが取得出来ない場合には以下の処理は行わない
            if (Encrypt_Data.Trim() == string.Empty)
            {
                //終了(異常)
                MessageBox.Show("ソースデータが取得できませんでした。");
                Environment.Exit(-1);
            }

            //暗号化されているので復号化する
            Encrypt_Data = Regex.Replace(Encrypt_Data, "\0", "");
            Decrypt_Data = AESCryption.AESCryption.Decrypt(Encrypt_Data, AESCryption.AESCryption.AES_IV, AESCryption.AESCryption.AES_KEY);
            Plain_Text = Encoding.UTF8.GetString(Convert.FromBase64String(Decrypt_Data));

            //スクリプト実行
            StellarRoboLexer lexer = new StellarRoboLexer();
            StellarRoboParser parser = new StellarRoboParser();
            StellarRoboLexResult lr = lexer.AnalyzeFromSource(Plain_Text);
            if (!lr.Success)
            {
                //字句解析に失敗
                MessageBox.Show(string.Format("字句解析に失敗しました。Err({0})", lr.Error.Message));
                Environment.Exit(-1);
            }
            StellarRoboAst ast = parser.Parse(lr);
            if (!ast.Success)
            {
                //構文解析に失敗
                MessageBox.Show(string.Format("構文解析に失敗しました。Err({0})", ast.Error.Message));
                Environment.Exit(-1);
            }
            StellarRoboPrecompiler prc = new StellarRoboPrecompiler();
            StellarRoboSource src = prc.PrecompileAll(ast);
            StellarRoboEnvironment environment = new StellarRoboEnvironment();

            //Mine/Type設定
            environment.mineType(mineTypeList);

            //ここで、enviromentに対してGlobalVariableを使用し引数を与える
            //引数は指定されているか？
            if (args.Length > 1)
            {
                //引数をRPAに引き渡す
                for (int i = 1; i < args.Length; i++)
                {
                    //引数のKey部はArgumentの連番、Value部はそのまま渡す
                    environment.GlobalVariable(string.Format("Argument{0}", (i)), args[i]);

                    //エラー出力の為にログファイルパスは保存しておく
                    if (i == 3)
                    {
                        loggerFile = args[i];
                    }
                }
            }

            StellarRoboModule module = environment.CreateModule("Main");
            module.RegisterSource(src);
            ctx = module.CreateContext();
            ctx.IsDump = dumpMode == 0 ? false : true;
            var il = module["main"];
            var kargs = new List<StellarRoboObject>();
            kargs.Add(new StellarRoboArray(new List<StellarRoboObject>()));
            ctx.Initialize(il, kargs);
            try
            {
                //Hook開始
                if (InterruptFlg) { StartHook(); }

                //Script実行
                while (ctx.MoveNext()) ;
            }
            catch(ManualStopException)
            {
                //手動停止なので何もしない
            }
            catch (Exception ex)
            {
                //異常終了の原因をログに出力
                using (StreamWriter streamWrite = new StreamWriter(loggerFile, true, Encoding.UTF8))
                {
                    streamWrite.WriteLine(ex.Message);
                }

                //終了(異常)
                Environment.Exit(-1);
            }
            finally
            {
                //Hook終了
                if (InterruptFlg) { StopHook(); }
            }

            //終了
            Environment.Exit(Environment.ExitCode);
        }
        #endregion

        #region Hook関連
        private void StartHook()
        {
            //キーボードはフックされているか？
            if (!StellarLink.Windows.GlobalHook.KeyboardHook.IsHooking)
            {
                //キーコードはフックされていないので、フックを開始する
                StellarLink.Windows.GlobalHook.KeyboardHook.AddEvent(InterruptHookKeyBoard);
                StellarLink.Windows.GlobalHook.KeyboardHook.Start();
            }

            //マウスはフックされているか？
            if (!MouseHook.IsHooking)
            {
                //マウスはフックされていないので、フックを開始する
                MouseHook.AddEvent(InterruptHookMouse);
                MouseHook.Start();
            }
        }
        private void StopHook()
        {
            //マウスはフックされているか？
            if (MouseHook.IsHooking)
            {
                //マウスはフックされているので、フックを終了する
                MouseHook.Stop();
            }

            //キーボードはフックされているか？
            if (KeyboardHook.IsHooking)
            {
                //キーボードはフックされているので、フックを終了する
                KeyboardHook.Stop();
            }
        }

        #region 割込処理用Hook
        void InterruptHookKeyBoard(ref KeyboardHook.StateKeyboard s)
        {
            //ハードウェアの動きのみ対象とする
            if ((int.Parse(s.Flags.ToString()) & 16) == 0)
            {
                //中止フラグを設定
                ctx.ForcedStop = true;
            }
        }
        void InterruptHookMouse(ref MouseHook.StateMouse s)
        {
            //ハードウェアの動きのみ対象とする
            if (int.Parse(s.Flags.ToString()) != 1)
            {
                //中止フラグを設定
                ctx.ForcedStop = true;
            }
        }
        #endregion

#endregion

        private void FormMain_Load(object sender, EventArgs e)
        {
            //Script実行
            ScriptRun();
        }
    }
}
