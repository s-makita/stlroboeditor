using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Snippets;
using StellarLink.Windows.GlobalHook;
using StellarRobo;
using StellarRobo.Analyze;
using StellarRobo.Type;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Xml;
using System.Diagnostics;
using UserException;
using System.Threading;

namespace StellarRoboEditor
{

    public partial class FormMain : Form
    {
        #region API

        #region UpdateResource
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool UpdateResource(IntPtr hUpdate, string lpType, string lpName, ushort wLanguage, IntPtr lpData, uint cbData);
        #endregion

        #region BeginUpdateResource
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr BeginUpdateResource(string pFileName,
        [MarshalAs(UnmanagedType.Bool)]bool bDeleteExistingResources);
        #endregion

        #region EndUpdateResource
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);
        #endregion

        #region WindowFromPoint
        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(System.Drawing.Point p);
        #endregion

        #region GetKeyState
        [DllImport("USER32.dll")]
        static extern short GetKeyState(VirtualKeyStates nVirtKey);
        enum VirtualKeyStates : int
        {
            VK_LBUTTON = 0x01,
            VK_RBUTTON = 0x02,
            VK_CANCEL = 0x03,
            VK_MBUTTON = 0x04,
            //
            VK_XBUTTON1 = 0x05,
            VK_XBUTTON2 = 0x06,
            //
            VK_BACK = 0x08,
            VK_TAB = 0x09,
            //
            VK_CLEAR = 0x0C,
            VK_RETURN = 0x0D,
            //
            VK_SHIFT = 0x10,
            VK_CONTROL = 0x11,
            VK_MENU = 0x12,
            VK_PAUSE = 0x13,
            VK_CAPITAL = 0x14,
            //
            VK_KANA = 0x15,
            VK_HANGEUL = 0x15,  /* old name - should be here for compatibility */
            VK_HANGUL = 0x15,
            VK_JUNJA = 0x17,
            VK_FINAL = 0x18,
            VK_HANJA = 0x19,
            VK_KANJI = 0x19,
            //
            VK_ESCAPE = 0x1B,
            //
            VK_CONVERT = 0x1C,
            VK_NONCONVERT = 0x1D,
            VK_ACCEPT = 0x1E,
            VK_MODECHANGE = 0x1F,
            //
            VK_SPACE = 0x20,
            VK_PRIOR = 0x21,
            VK_NEXT = 0x22,
            VK_END = 0x23,
            VK_HOME = 0x24,
            VK_LEFT = 0x25,
            VK_UP = 0x26,
            VK_RIGHT = 0x27,
            VK_DOWN = 0x28,
            VK_SELECT = 0x29,
            VK_PRINT = 0x2A,
            VK_EXECUTE = 0x2B,
            VK_SNAPSHOT = 0x2C,
            VK_INSERT = 0x2D,
            VK_DELETE = 0x2E,
            VK_HELP = 0x2F,
            //
            VK_LWIN = 0x5B,
            VK_RWIN = 0x5C,
            VK_APPS = 0x5D,
            //
            VK_SLEEP = 0x5F,
            //
            VK_NUMPAD0 = 0x60,
            VK_NUMPAD1 = 0x61,
            VK_NUMPAD2 = 0x62,
            VK_NUMPAD3 = 0x63,
            VK_NUMPAD4 = 0x64,
            VK_NUMPAD5 = 0x65,
            VK_NUMPAD6 = 0x66,
            VK_NUMPAD7 = 0x67,
            VK_NUMPAD8 = 0x68,
            VK_NUMPAD9 = 0x69,
            VK_MULTIPLY = 0x6A,
            VK_ADD = 0x6B,
            VK_SEPARATOR = 0x6C,
            VK_SUBTRACT = 0x6D,
            VK_DECIMAL = 0x6E,
            VK_DIVIDE = 0x6F,
            VK_F1 = 0x70,
            VK_F2 = 0x71,
            VK_F3 = 0x72,
            VK_F4 = 0x73,
            VK_F5 = 0x74,
            VK_F6 = 0x75,
            VK_F7 = 0x76,
            VK_F8 = 0x77,
            VK_F9 = 0x78,
            VK_F10 = 0x79,
            VK_F11 = 0x7A,
            VK_F12 = 0x7B,
            VK_F13 = 0x7C,
            VK_F14 = 0x7D,
            VK_F15 = 0x7E,
            VK_F16 = 0x7F,
            VK_F17 = 0x80,
            VK_F18 = 0x81,
            VK_F19 = 0x82,
            VK_F20 = 0x83,
            VK_F21 = 0x84,
            VK_F22 = 0x85,
            VK_F23 = 0x86,
            VK_F24 = 0x87,
            //
            VK_NUMLOCK = 0x90,
            VK_SCROLL = 0x91,
            //
            VK_OEM_NEC_EQUAL = 0x92,   // '=' key on numpad
                                       //
            VK_OEM_FJ_JISHO = 0x92,   // 'Dictionary' key
            VK_OEM_FJ_MASSHOU = 0x93,   // 'Unregister word' key
            VK_OEM_FJ_TOUROKU = 0x94,   // 'Register word' key
            VK_OEM_FJ_LOYA = 0x95,   // 'Left OYAYUBI' key
            VK_OEM_FJ_ROYA = 0x96,   // 'Right OYAYUBI' key
                                     //
            VK_LSHIFT = 0xA0,
            VK_RSHIFT = 0xA1,
            VK_LCONTROL = 0xA2,
            VK_RCONTROL = 0xA3,
            VK_LMENU = 0xA4,
            VK_RMENU = 0xA5,
            //
            VK_BROWSER_BACK = 0xA6,
            VK_BROWSER_FORWARD = 0xA7,
            VK_BROWSER_REFRESH = 0xA8,
            VK_BROWSER_STOP = 0xA9,
            VK_BROWSER_SEARCH = 0xAA,
            VK_BROWSER_FAVORITES = 0xAB,
            VK_BROWSER_HOME = 0xAC,
            //
            VK_VOLUME_MUTE = 0xAD,
            VK_VOLUME_DOWN = 0xAE,
            VK_VOLUME_UP = 0xAF,
            VK_MEDIA_NEXT_TRACK = 0xB0,
            VK_MEDIA_PREV_TRACK = 0xB1,
            VK_MEDIA_STOP = 0xB2,
            VK_MEDIA_PLAY_PAUSE = 0xB3,
            VK_LAUNCH_MAIL = 0xB4,
            VK_LAUNCH_MEDIA_SELECT = 0xB5,
            VK_LAUNCH_APP1 = 0xB6,
            VK_LAUNCH_APP2 = 0xB7,
            //
            VK_OEM_1 = 0xBA,   // ';:' for US
            VK_OEM_PLUS = 0xBB,   // '+' any country
            VK_OEM_COMMA = 0xBC,   // ',' any country
            VK_OEM_MINUS = 0xBD,   // '-' any country
            VK_OEM_PERIOD = 0xBE,   // '.' any country
            VK_OEM_2 = 0xBF,   // '/?' for US
            VK_OEM_3 = 0xC0,   // '`~' for US
                               //
            VK_OEM_4 = 0xDB,  //  '[{' for US
            VK_OEM_5 = 0xDC,  //  '\|' for US
            VK_OEM_6 = 0xDD,  //  ']}' for US
            VK_OEM_7 = 0xDE,  //  ''"' for US
            VK_OEM_8 = 0xDF,
            //
            VK_OEM_AX = 0xE1,  //  'AX' key on Japanese AX kbd
            VK_OEM_102 = 0xE2,  //  "<>" or "\|" on RT 102-key kbd.
            VK_ICO_HELP = 0xE3,  //  Help key on ICO
            VK_ICO_00 = 0xE4,  //  00 key on ICO
                               //
            VK_PROCESSKEY = 0xE5,
            //
            VK_ICO_CLEAR = 0xE6,
            //
            VK_PACKET = 0xE7,
            //
            VK_OEM_RESET = 0xE9,
            VK_OEM_JUMP = 0xEA,
            VK_OEM_PA1 = 0xEB,
            VK_OEM_PA2 = 0xEC,
            VK_OEM_PA3 = 0xED,
            VK_OEM_WSCTRL = 0xEE,
            VK_OEM_CUSEL = 0xEF,
            VK_OEM_ATTN = 0xF0,
            VK_OEM_FINISH = 0xF1,
            VK_OEM_COPY = 0xF2,
            VK_OEM_AUTO = 0xF3,
            VK_OEM_ENLW = 0xF4,
            VK_OEM_BACKTAB = 0xF5,
            //
            VK_ATTN = 0xF6,
            VK_CRSEL = 0xF7,
            VK_EXSEL = 0xF8,
            VK_EREOF = 0xF9,
            VK_PLAY = 0xFA,
            VK_ZOOM = 0xFB,
            VK_NONAME = 0xFC,
            VK_PA1 = 0xFD,
            VK_OEM_CLEAR = 0xFE
        }
        #endregion

        #endregion

        #region 定数宣言
        private double FONT_SIZE = 12;              //フォントサイズ
        private string FONT_FAMILY = "ms gothic";   //フォント名
        private string NEW_LINE = "\r\n";           //改行コード
        private string SIMPLE_NEW_LINE = "\n";      //改行コード
        private string NEW_LINE_SYMBOL = "\u23CE";  //改行記号
        #endregion

        #region 変数宣言
        //変数宣言
        string fileName = string.Empty;
        string filePath = string.Empty;
        string headerText = string.Empty;
        bool interruptFlg = false;
        private struct SimulateData
        {
            public int type;
            public MouseHook.Stroke MouseStroke;
            public int X;
            public int Y;
            public uint Data;
            public uint Flags;
            public uint Time;
            public System.IntPtr ExtraInfo;
            public KeyboardHook.Stroke KeyStroke;
            public System.Windows.Forms.Keys Key;
            public uint ScanCode;
            public bool ShiftKeyStatus;
            public bool CtrlKeyStatus;
            public bool AltKeyStatus;
            public bool CapsLockKeyStatus;
        }
        Queue<SimulateData> Simulate = new Queue<SimulateData>();
        private TextEditor _editor;                                     //ソース表示用
        private CompletionWindow completionWindow;                      //補完データ用
        private IList<ICompletionData> AllCompletionsData;              //補完データ用
        private StellarRoboContext ctx;                                 //Script実行環境 ※処理の関係上グローバル変数にせざるを得ない
        private ToolStripControlHost toolStripCheckBoxInterrupt;        //割り込みCheckBox用
        private ToolStripControlHost toolStripCheckBoxDump;             //ダンプ出力CheckBox用

        public class CompletionData : ICompletionData
        {
            public CompletionData(string text, string description)
            {
                this.Text = text;
                this.description = description;
            }
            public object Content { get { return this.Text; } }
            public object Description { get { return this.description; } }
            public ImageSource Image { get { return null; } }
            public double Priority { get; set; }
            public string Text { get; private set; }
            public string description { get; private set; }
            public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
            {
                //変数宣言
                int pos = -1;
                string separateChar = string.Empty;

                //置換位置取得の為にテキストデータを取得
                string textData = textArea.Document.Text;

                //オフセット位置より、ブランク若しくは改行までの位置を取得
                for (int i = completionSegment.EndOffset - 1; i >= 0; i--)
                {
                    //一文字取得
                    char checkChar = textData[i];

                    //文字確認
                    if (Regex.IsMatch(checkChar.ToString(), "( |　|\n|\r\n|\t)"))
                    {
                        //今の位置及び区切り文字を保存しループを抜ける
                        pos = i;
                        separateChar = checkChar.ToString();
                        break;
                    }
                }

                //開始位置はあったか？
                if (pos == -1)
                {
                    //開始位置を一番先頭に
                    pos = 0;
                }

                //置換を行う
                textArea.Document.Replace(pos, completionSegment.EndOffset - pos, separateChar + Text);
            }
        }
        Thread scriptProcess;

        public delegate void DelegateSetDebugLine(int line);
        public enum EditorState
        {
            None,                   //何も行われていない
            Running,                //実行中
            DuringDebugging,        //デバッグ中
            DuringExport,           //書き出し中
            DuringRecording,        //録画中
        }
        public delegate void DelegateSetMenuState(EditorState editorState);

        #region リソース名 DLL名対比
        private Dictionary<string, string> exportDll = new Dictionary<string, string>()
        {
            {"dll\\AngleSharp","dll\\AngleSharp.dll" },
            {"dll\\StellarRobo","StellarRobo.xml" },
            {"dll\\ICSharpCode_SharpZipLib","dll\\ICSharpCode.SharpZipLib.dll" },
            {"dll\\Microsoft_Win32_Primitives","dll\\Microsoft.Win32.Primitives.dll" },
            {"dll\\netstandard","dll\\netstandard.dll" },
            {"dll\\NPOI","dll\\NPOI.dll" },
            {"dll\\NPOI_OOXML","dll\\NPOI.OOXML.dll" },
            {"dll\\NPOI_OpenXml4Net","dll\\NPOI.OpenXml4Net.dll" },
            {"dll\\NPOI_OpenXmlFormats","dll\\NPOI.OpenXmlFormats.dll" },
            {"dll\\OpenCvSharp_Blob","dll\\OpenCvSharp.dll" },
            {"dll\\OpenCvSharp","dll\\OpenCvSharp.Blob.dll" },
            {"dll\\OpenCvSharp_Extensions","dll\\OpenCvSharp.Extensions.dll" },
            {"dll\\OpenCvSharp_UserInterface","dll\\OpenCvSharp.UserInterface.dll" },
            {"dll\\SnmpSharpNet","dll\\SnmpSharpNet.dll" },
            {"dll\\System_AppContext","dll\\System.AppContext.dll" },
            {"dll\\System_Collections_Concurrent","dll\\System.Collections.Concurrent.dll" },
            {"dll\\System_Collections","dll\\System.Collections.dll" },
            {"dll\\System_Collections_NonGeneric","dll\\System.Collections.NonGeneric.dll" },
            {"dll\\System_Collections_Specialized","dll\\System.Collections.Specialized.dll" },
            {"dll\\System_ComponentModel","dll\\System.ComponentModel.dll" },
            {"dll\\System_ComponentModel_EventBasedAsync","dll\\System.ComponentModel.EventBasedAsync.dll" },
            {"dll\\System_ComponentModel_Primitives","dll\\System.ComponentModel.Primitives.dll" },
            {"dll\\System_ComponentModel_TypeConverter","dll\\System.ComponentModel.TypeConverter.dll" },
            {"dll\\System_Console","dll\\System.Console.dll" },
            {"dll\\System_Data_Common","dll\\System.Data.Common.dll" },
            {"dll\\System_Diagnostics_Contracts","dll\\System.Diagnostics.Contracts.dll"},
            {"dll\\System_Diagnostics_Debug","dll\\System.Diagnostics.Debug.dll"},
            {"dll\\System_Diagnostics_FileVersionInfo","dll\\System.Diagnostics.FileVersionInfo.dll"},
            {"dll\\System_Diagnostics_Process","dll\\System.Diagnostics.Process.dll"},
            {"dll\\System_Diagnostics_StackTrace","dll\\System.Diagnostics.StackTrace.dll"},
            {"dll\\System_Diagnostics_TextWriterTraceListener","dll\\System.Diagnostics.TextWriterTraceListener.dll"},
            {"dll\\System_Diagnostics_Tools","dll\\System.Diagnostics.Tools.dll"},
            {"dll\\System_Diagnostics_TraceSource","dll\\System.Diagnostics.TraceSource.dll"},
            {"dll\\System_Diagnostics_Tracing","dll\\System.Diagnostics.Tracing.dll"},
            {"dll\\System_Drawing_Primitives","dll\\System.Drawing.Primitives.dll"},
            {"dll\\System_Dynamic_Runtime","dll\\System.Dynamic.Runtime.dll"},
            {"dll\\System_Globalization_Calendars","dll\\System.Globalization.Calendars.dll"},
            {"dll\\System_Globalization","dll\\System.Globalization.dll"},
            {"dll\\System_Globalization_Extensions","dll\\System.Globalization.Extensions.dll"},
            {"dll\\System_IO_Compression","dll\\System.IO.Compression.dll"},
            {"dll\\System_IO_Compression_ZipFile","dll\\System.IO.Compression.ZipFile.dll"},
            {"dll\\System_IO","dll\\System.IO.dll"},
            {"dll\\System_IO_FileSystem","dll\\System.IO.FileSystem.dll"},
            {"dll\\System_IO_FileSystem_DriveInfo","dll\\System.IO.FileSystem.DriveInfo.dll"},
            {"dll\\System_IO_FileSystem_Primitives","dll\\System.IO.FileSystem.Primitives.dll"},
            {"dll\\System_IO_FileSystem_Watcher","dll\\System.IO.FileSystem.Watcher.dll"},
            {"dll\\System_IO_IsolatedStorage","dll\\System.IO.IsolatedStorage.dll"},
            {"dll\\System_IO_MemoryMappedFiles","dll\\System.IO.MemoryMappedFiles.dll"},
            {"dll\\System_IO_Pipes","dll\\System.IO.Pipes.dll"},
            {"dll\\System_IO_UnmanagedMemoryStream","dll\\System.IO.UnmanagedMemoryStream.dll"},
            {"dll\\System_Linq","dll\\System.Linq.dll"},
            {"dll\\System_Linq_Expressions","dll\\System.Linq.Expressions.dll"},
            {"dll\\System_Linq_Parallel","dll\\System.Linq.Parallel.dll"},
            {"dll\\System_Linq_Queryable","dll\\System.Linq.Queryable.dll"},
            {"dll\\System_Net_Http","dll\\System.Net.Http.dll"},
            {"dll\\System_Net_NameResolution","dll\\System.Net.NameResolution.dll"},
            {"dll\\System_Net_NetworkInformation","dll\\System.Net.NetworkInformation.dll"},
            {"dll\\System_Net_Ping","dll\\System.Net.Ping.dll"},
            {"dll\\System_Net_Primitives","dll\\System.Net.Primitives.dll"},
            {"dll\\System_Net_Requests","dll\\System.Net.Requests.dll"},
            {"dll\\System_Net_Security","dll\\System.Net.Security.dll"},
            {"dll\\System_Net_Sockets","dll\\System.Net.Sockets.dll"},
            {"dll\\System_Net_WebHeaderCollection","dll\\System.Net.WebHeaderCollection.dll"},
            {"dll\\System_Net_WebSockets_Client","dll\\System.Net.WebSockets.Client.dll"},
            {"dll\\System_Net_WebSockets","dll\\System.Net.WebSockets.dll"},
            {"dll\\System_ObjectModel","dll\\System.ObjectModel.dll"},
            {"dll\\System_Reflection","dll\\System.Reflection.dll"},
            {"dll\\System_Reflection_Extensions","dll\\System.Reflection.Extensions.dll"},
            {"dll\\System_Reflection_Primitives","dll\\System.Reflection.Primitives.dll"},
            {"dll\\System_Resources_Reader","dll\\System.Resources.Reader.dll"},
            {"dll\\System_Resources_ResourceManager","dll\\System.Resources.ResourceManager.dll"},
            {"dll\\System_Resources_Writer","dll\\System.Resources.Writer.dll"},
            {"dll\\System_Runtime_CompilerServices_Unsafe","dll\\System.Runtime.CompilerServices.Unsafe.dll"},
            {"dll\\System_Runtime_CompilerServices_VisualC","dll\\System.Runtime.CompilerServices.VisualC.dll"},
            {"dll\\System_Runtime","dll\\System.Runtime.dll"},
            {"dll\\System_Runtime_Extensions","dll\\System.Runtime.Extensions.dll"},
            {"dll\\System_Runtime_Handles","dll\\System.Runtime.Handles.dll"},
            {"dll\\System_Runtime_InteropServices","dll\\System.Runtime.InteropServices.dll"},
            {"dll\\System_Runtime_InteropServices_RuntimeInformation","dll\\System.Runtime.InteropServices.RuntimeInformation.dll"},
            {"dll\\System_Runtime_Numerics","dll\\System.Runtime.Numerics.dll"},
            {"dll\\System_Runtime_Serialization_Formatters","dll\\System.Runtime.Serialization.Formatters.dll"},
            {"dll\\System_Runtime_Serialization_Json","dll\\System.Runtime.Serialization.Json.dll"},
            {"dll\\System_Runtime_Serialization_Primitives","dll\\System.Runtime.Serialization.Primitives.dll"},
            {"dll\\System_Runtime_Serialization_Xml","dll\\System.Runtime.Serialization.Xml.dll"},
            {"dll\\System_Security_Claims","dll\\System.Security.Claims.dll"},
            {"dll\\System_Security_Cryptography_Algorithms","dll\\System.Security.Cryptography.Algorithms.dll"},
            {"dll\\System_Security_Cryptography_Csp","dll\\System.Security.Cryptography.Csp.dll"},
            {"dll\\System_Security_Cryptography_Encoding","dll\\System.Security.Cryptography.Encoding.dll"},
            {"dll\\System_Security_Cryptography_Primitives","dll\\System.Security.Cryptography.Primitives.dll"},
            {"dll\\System_Security_Cryptography_X509Certificates","dll\\System.Security.Cryptography.X509Certificates.dll"},
            {"dll\\System_Security_Principal","dll\\System.Security.Principal.dll"},
            {"dll\\System_Security_SecureString","dll\\System.Security.SecureString.dll"},
            {"dll\\System_Text_Encoding_CodePages","dll\\System.Text.Encoding.CodePages.dll"},
            {"dll\\System_Text_Encoding","dll\\System.Text.Encoding.dll"},
            {"dll\\System_Text_Encoding_Extensions","dll\\System.Text.Encoding.Extensions.dll"},
            {"dll\\System_Text_RegularExpressions","dll\\System.Text.RegularExpressions.dll"},
            {"dll\\System_Threading","dll\\System.Threading.dll"},
            {"dll\\System_Threading_Overlapped","dll\\System.Threading.Overlapped.dll"},
            {"dll\\System_Threading_Tasks","dll\\System.Threading.Tasks.dll"},
            {"dll\\System_Threading_Tasks_Parallel","dll\\System.Threading.Tasks.Parallel.dll"},
            {"dll\\System_Threading_Thread","dll\\System.Threading.Thread.dll"},
            {"dll\\System_Threading_ThreadPool","dll\\System.Threading.ThreadPool.dll"},
            {"dll\\System_Threading_Timer","dll\\System.Threading.Timer.dll"},
            {"dll\\System_ValueTuple","dll\\System.ValueTuple.dll"},
            {"dll\\System_Xml_ReaderWriter","dll\\System.Xml.ReaderWriter.dll"},
            {"dll\\System_Xml_XDocument","dll\\System.Xml.XDocument.dll"},
            {"dll\\System_Xml_XmlDocument","dll\\System.Xml.XmlDocument.dll"},
            {"dll\\System_Xml_XmlSerializer","dll\\System.Xml.XmlSerializer.dll"},
            {"dll\\System_Xml_XPath","dll\\System.Xml.XPath.dll"},
            {"dll\\System_Xml_XPath_XDocument","dll\\System.Xml.XPath.XDocument.dll"},
            {"dll\\WebDriver","dll\\WebDriver.dll" },
            {"dll\\WebDriver_Support","dll\\WebDriver.Support.dll" },
            {"dll\\WebDriverManager","dll\\WebDriverManager.dll" },
            {"dll\\opencv_ffmpeg400_64","dll\\x64\\opencv_ffmpeg400_64.dll" },
            {"dll\\OpenCvSharpExtern64","dll\\x64\\OpenCvSharpExtern.dll" },
            {"dll\\opencv_ffmpeg400","dll\\x86\\opencv_ffmpeg400.dll" },
            {"dll\\OpenCvSharpExtern86","dll\\x86\\OpenCvSharpExtern.dll" },
        };
        #endregion

        #endregion

        public FormMain()
        {
            InitializeComponent();

            //Version設定
            FileVersionInfo ver = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            this.Text = this.Text + string.Format(" {0}.{1}", ver.FileMajorPart.ToString(), ver.FileMinorPart.ToString());
            headerText = this.Text;

            //初期ディレクトリはDeskTopにする
            filePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            #region ToolStripにコントロールを表示
            //ToolStripにコントロールを表示

            #region Interrupt
            ToolStripLabel labelInterrupt = new ToolStripLabel();
            labelInterrupt.Text = "割り込みを有効にする";
            toolStrip.Items.Add(labelInterrupt);

            CheckBox ctrlInterrupt = new CheckBox();
            ctrlInterrupt.Name = "checkboxInterrupt";
            toolStripCheckBoxInterrupt = new ToolStripControlHost(ctrlInterrupt);
            toolStrip.Items.Add(toolStripCheckBoxInterrupt);
            ((CheckBox)toolStripCheckBoxInterrupt.Control).Checked = true;
            #endregion

            #region Dump
            ToolStripLabel labelDump = new ToolStripLabel();
            labelDump.Text = "Dumpモード";
            toolStrip.Items.Add(labelDump);

            CheckBox ctrlDump = new CheckBox();
            ctrlDump.Name = "checkboxDump";
            toolStripCheckBoxDump = new ToolStripControlHost(ctrlDump);
            toolStrip.Items.Add(toolStripCheckBoxDump);
            ((CheckBox)toolStripCheckBoxDump.Control).Checked = false;
            #endregion

            #endregion

            //ソース表示コントロールを作成
            ElementHost host = new ElementHost();
            host.Size = new Size(this.Size.Width, (this.Size.Height - toolStrip.Size.Height));
            host.Location = new Point(0, toolStrip.Size.Height);
            host.Dock = DockStyle.Fill;

            _editor = new TextEditor();
            _editor.ShowLineNumbers = true;                     //行数を表示する
            _editor.FontSize = FONT_SIZE;                       //フォントサイズ設定
            _editor.FontFamily = new System.Windows.Media.FontFamily(FONT_FAMILY);   //フォント名
            _editor.AllowDrop = true;

            #region 詳細オプション設定
            //詳細オプションの設定を行う
            TextEditorOptions textEditorOptions = _editor.Options;
            textEditorOptions.HighlightCurrentLine = true;
            textEditorOptions.ShowSpaces = true;
            textEditorOptions.ShowEndOfLine = true;
            textEditorOptions.ShowTabs = true;
            textEditorOptions.NewLine = NEW_LINE_SYMBOL;
            _editor.Options = textEditorOptions;
            #endregion

            #region SyntaxHightlightColorの設定
            //SyntaxHightlightColorの設定を行う
            string SyntaxHighlightData = System.Text.Encoding.ASCII.GetString(StellarRoboEditor.Resource.SyntaxHighlightingFile);
            using (TextReader textReader = new StringReader(SyntaxHighlightData))
            using (XmlReader xmlReader = XmlReader.Create(textReader))
            {
                _editor.SyntaxHighlighting = HighlightingLoader.Load(xmlReader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
            }
            #endregion

            //イベント追加
            _editor.Document.UndoStack.PropertyChanged += UndoStack_PropertyChanged;
            _editor.TextArea.TextEntering += TextArea_TextEntering;
            _editor.TextArea.TextEntered += TextArea_TextEntered;
            _editor.TextArea.PreviewKeyDown += PreviewKeyDown;
            _editor.Drop += new System.Windows.DragEventHandler(Editor_DragDrop);
            _editor.DragEnter += new System.Windows.DragEventHandler(Editor_DragEnter);

            host.Child = _editor;
            panelEditor.Controls.Add(host);

            //その他設定
            Point pos = Cursor.Position;
            toolStripStatusCoordinate.Text = pos.X.ToString().PadLeft(6) + ":" + pos.Y.ToString().PadLeft(6); //ツールバー上にチェックボックスを表示
            scriptProcess = new Thread(() => { });                                                              //Scriptを別Threadで実行する用

            //補完用データ作成
            SetCompletionsData();

            //HooK開始
            StartHook();

            //Hookイベント追加
            MouseHook.AddEvent(CoordinateHookMouse);
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //テキストが変更されたか
            if(_editor.Document.UndoStack.IsUpdate)
            {
                //メッセージ表示
                DialogResult dialogResult = MessageBox.Show(String.Format("「{0}」は更新されています。\n保存しますか？", fileName.Trim() == string.Empty ? "無題" : fileName), "確認", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3);
                switch (dialogResult)
                {
                    //はい・いいえ
                    case DialogResult.Yes:
                    case DialogResult.No:
                        if(dialogResult == DialogResult.Yes)
                        {
                            //保存する
                            SavetoolStripButton_Click(sender, e);
                        }

                        if (!e.Cancel)
                        {
                            //Hook終了
                            StopHook();
                        }
                        break;
                    //キャンセル
                    case DialogResult.Cancel:
                        //処理を抜ける
                        e.Cancel = true;
                        break;
                }
            }
            else
            {
                //Hook終了
                StopHook();
            }
        }

        #region スクリプト関連

        #region 字句解析
        private StellarRoboLexResult Analayze(string Source)
        {
            //変数宣言
            StellarRoboLexer lexer = new StellarRoboLexer();
            StellarRoboLexResult result;

            //スクリプトの字句解析を行う
            string Script = Regex.Replace(Source, NEW_LINE, SIMPLE_NEW_LINE);
            Script = Regex.Replace(Script, SIMPLE_NEW_LINE, NEW_LINE);
            result = lexer.AnalyzeFromSource(Source);

            //字句解析の結果は？
            if (!result.Success)
            {
                //エラー
                _editor.TextArea.TextView.ErrorHighlightedLine = (result.Error.Line + 1);
                MessageBox.Show(string.Format("{0}行目にエラーがあります。ErrMsg({1}) File:{2}(字句解析)", (result.Error.Line + 1).ToString(), result.Error.Message, result.SourceName));
            }

            //戻り値設定
            return result;
        }
        #endregion  

        #region 構文解析
        private StellarRoboAst Parse(StellarRoboLexResult Lexer)
        {
            //変数宣言
            StellarRoboParser parser = new StellarRoboParser();
            StellarRoboAst result;

            //構文解析を行う
            result = parser.Parse(Lexer);

            //構文解析の結果は？
            if (!result.Success)
            {
                //エラー
                _editor.TextArea.TextView.ErrorHighlightedLine = (result.Error.Line + 1);
                MessageBox.Show(string.Format("{0}行目にエラーがあります。ErrMsg({1}) File:{2}(構文解析)", (result.Error.Line + 1).ToString(), result.Error.Message, result.SourceName));
            }

            //戻り値設定
            return result;
        }
        #endregion

        #region Script実行
        private void ExecutetoolStripButton_Click(object sender, EventArgs e)
        {
            //Scrptは実行中か？
            if (scriptProcess.IsAlive == true)
            {
                //Script実行(デバッグ)
                DebugExecute(false);
            }
            else
            {
                //Script実行
                ExecuteScript();
            }
        }
        private void DebugExecutetoolStripButton_Click(object sender, EventArgs e)
        {
            //メニュー有効/無効設定
            setMenuState(EditorState.DuringDebugging);
            //Script実行(デバッグ)
            DebugExecute(true);
        }
        private void DebugExecute(bool IsDebug)
        {
            //ソースは入力されているか？
            if (string.IsNullOrEmpty(_editor.Text.Trim())) { return; }

            //メニュー有効/無効設定
            setMenuState(EditorState.DuringDebugging);

            //Scrptは実行中か？
            if (scriptProcess.IsAlive == true)
            {
                if (!IsDebug)
                {
                    //Debugの為に背景色を変更していたので初期化する
                    _editor.TextArea.TextView.DebugHighlightedLine = 0;
                }

                //Debugフラグ設定
                ctx.IsDebug = IsDebug;

                //Dumpフラグ設定
                ctx.IsDump = ((CheckBox)toolStripCheckBoxDump.Control).Checked;

                //処理を再開する
                ctx.countdown.Signal();
                System.Threading.Thread.Sleep(1);
                ctx.countdown.Reset();
            }
            else
            {
                //Script実行
                ExecuteScript(IsDebug);
            }
        }

        public void EvalScript(string[] args)
        {
            string source = args[0];
            Array.Clear(args, 0, 1);
            source = Regex.Unescape(source);

            //ソースは入力されているか？
            if (string.IsNullOrEmpty(source.Trim())) { return; }

            //フラグ設定
            interruptFlg = false;

            //字句解析
            StellarRoboLexResult lexer = Analayze(source);
            if (!lexer.Success)
            {
                //字句解析に失敗
                return;
            }

            //構文解析
            StellarRoboAst ast = Parse(lexer);
            if (!ast.Success)
            {
                //構文解析に失敗
                return;
            }

            StellarRoboPrecompiler prc = new StellarRoboPrecompiler();
            StellarRoboSource src = prc.PrecompileAll(ast);
            StellarRoboEnvironment environment = new StellarRoboEnvironment();

            //Mine/Type設定
            string mineTypeList = StellarRobo.MineTypeData.GetMineType(StellarRoboEditor.MineType.MineTypeResource.ResourceManager, Application.StartupPath);
            environment.mineType(mineTypeList);

            //ここで、enviromentに対してGlobalVariableを使用し引数を与える
            //引数は指定されているか？
            if (args.Length > 1)
            {
                //引数をRPAに引き渡す
                for (int i = 0; i < args.Length; i++)
                {
                    //引数はKey:Valueの形で渡される事を想定しているので、":"で分割する
                    string[] Variable = args[i].ToString().Split(':');
                    //分割された値を設定する ※Key部分に:は含まれないと思うが、Value部には:が含まれる可能性があるのでBase64で変換しておく
                    environment.GlobalVariable(Variable[0], Encoding.UTF8.GetString(System.Convert.FromBase64String(Variable[1])));
                }
            }

            StellarRoboModule module = environment.CreateModule("Main");
            module.RegisterSource(src);
            ctx = module.CreateContext();

            var kargs = new List<StellarRoboObject>();
            kargs.Add(new StellarRoboArray(new List<StellarRoboObject>()));
            ctx.Initialize(module["main"], kargs);

            //タスクは実行中ではないか？
            if (scriptProcess.IsAlive != true)
            {
                //スレッド作成
                scriptProcess = new Thread(() =>
                {
                    try
                    {
                        //Hook開始
                        MouseHook.AddEvent(InterruptHookMouse);

                        //Scriptを実行
                        ctx.MoveNext();
                    }
                    finally
                    {
                        //Hook終了
                        MouseHook.RemoveEvent(InterruptHookMouse);
                    }
                });
                scriptProcess.SetApartmentState(ApartmentState.STA);
                scriptProcess.Start();
            }
        }

        private void ExecuteScript(bool DeBugMode = false)
        {
            string source = _editor.Text;
            string[] args = Environment.GetCommandLineArgs();

            //ソースは入力されているか？
            if (string.IsNullOrEmpty(_editor.Text.Trim())) { return; }

            //メニュー有効/無効設定
            setMenuState(EditorState.Running);

            //フラグ設定
            interruptFlg = false;

            if (source.Trim() != string.Empty)
            {
                //字句解析
                StellarRoboLexResult lexer = Analayze(source);
                if (!lexer.Success)
                {
                    //メニュー有効/無効設定
                    setMenuState(EditorState.None);

                    //字句解析に失敗
                    return;
                }

                //構文解析
                StellarRoboAst ast = Parse(lexer);
                if (!ast.Success)
                {
                    //メニュー有効/無効設定
                    setMenuState(EditorState.None);

                    //構文解析に失敗
                    return;
                }

                StellarRoboPrecompiler prc = new StellarRoboPrecompiler();
                StellarRoboSource src = prc.PrecompileAll(ast);
                StellarRoboEnvironment environment = new StellarRoboEnvironment();

                //Mine/Type設定
                string mineTypeList = StellarRobo.MineTypeData.GetMineType(StellarRoboEditor.MineType.MineTypeResource.ResourceManager, Application.StartupPath);
                environment.mineType(mineTypeList);

                //ここで、enviromentに対してGlobalVariableを使用し引数を与える
                //引数は指定されているか？
                if (args.Length > 1)
                {
                    //引数をRPAに引き渡す
                    for (int i = 0; i < args.Length; i++)
                    {
                        //引数はKey:Valueの形で渡される事を想定しているので、":"で分割する
                        string[] Variable = args[i].ToString().Split(':');

                        //分割された値を設定する ※Key部分に:は含まれないと思うが、Value部には:が含まれる可能性があるのでBase64で変換しておく
                        environment.GlobalVariable(Variable[0], Encoding.UTF8.GetString(System.Convert.FromBase64String(Variable[1])));
                    }
                }

                //エラー行表示になっていたら、消す
                if(_editor.TextArea.TextView.ErrorHighlightedLine >= 0)
                {
                    _editor.TextArea.TextView.ErrorHighlightedLine = -1;
                }
                StellarRoboModule module = environment.CreateModule("Main");
                module.RegisterSource(src);
                ctx = module.CreateContext();

                //BreakPoint設定
                ctx.BreakPoint = _editor.TextArea.TextView.BreakHighlightedLine;

                var il = module["main"];
                var kargs = new List<StellarRoboObject>();
                kargs.Add(new StellarRoboArray(new List<StellarRoboObject>()));
                ctx.Initialize(il, kargs);

                //タスクは実行中ではないか？
                if (scriptProcess.IsAlive != true)
                {
                    //Delegate設定
                    ctx.SetDebugLine = new DelegateSetDebugLine(SetDebugLine);
                    ctx.SetMenuState = new DelegateSetMenuState(setMenuState);

                    //スレッド作成
                    scriptProcess = new Thread(() =>
                    {
                        try
                        {
                            //割り込みを有効にするか？
                            if (((CheckBox)toolStripCheckBoxInterrupt.Control).Checked)
                            {
                                //Hook開始
                                MouseHook.AddEvent(InterruptHookMouse);
                            }

                            //実行モードを設定する
                            ctx.IsDebug = DeBugMode;
                            ctx.IsDump = ((CheckBox)toolStripCheckBoxDump.Control).Checked;
                            ctx.SetMenuState(DeBugMode ? EditorState.DuringDebugging : EditorState.Running);

                            //Scriptを実行
                            ctx.MoveNext();
                        }
                        catch (ManualStopException)
                        {
                            //手動停止したので何もしない
                        }
                        catch (Exception e)
                        {
                            //エラーメッセージ表示
                            MessageBox.Show(e.Message);
                        }
                        finally
                        {
                            //Hook終了
                            MouseHook.RemoveEvent(InterruptHookMouse);

                            //デバッグの背景初期化
                            SetDebugLine(0);
                        }

                        //メニュー有効/無効設定
                        ctx.SetMenuState(EditorState.None);

                        //終了
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show(this, "終了しました");
                        }));
                    });

                    scriptProcess.SetApartmentState(ApartmentState.STA);
                    scriptProcess.Start();
                }
            }
            else
            {
                //メニュー有効/無効設定
                setMenuState(EditorState.DuringDebugging);
            }
        }
        private void ExecuteStoptoolStripButton_Click(object sender, EventArgs e)
        {
            if (scriptProcess.IsAlive)
            {
                stopProcess("停止ボタンが押下されました。\n実行を中止しますか？");
            }
        }
        private bool stopProcess(string Message)
        {
            //実行中ならば、終了確認を行う
            if (ctx.IsDebug == false)
            {
                //メッセージ表示
                if(MessageBox.Show(Message,"",MessageBoxButtons.YesNo,MessageBoxIcon.Information,MessageBoxDefaultButton.Button2)!= DialogResult.Yes) { return false; }
            }

            //中止フラグを設定
            ctx.ForcedStop = true;

            //Debugモードか？
            if (ctx.IsDebug)
            {
                //中止フラグを設定した事を認識させる為に、処理を実行状態にさせる
                DebugExecute(false);
            }

            //戻り値設定
            return true;
        }
        #endregion

        #endregion

        #region ファイル関連

        #region 更新確認処理
        private bool checkUpdate(object sender, EventArgs e)
        {
            //変数宣言
            bool result = true;

            //テキストに変更が有れば確認する
            if (_editor.Document.UndoStack.IsUpdate)
            {
                DialogResult dr = MessageBox.Show("変更内容を保存しますか？", "確認", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (dr == DialogResult.Yes)
                {
                    //変更内容を保存
                    SavetoolStripButton_Click(sender, e);
                }
                else if (dr == DialogResult.Cancel)
                {
                    //戻り値設定
                    result = false;
                }
            }

            //戻り値設定
            return result;
        }
        #endregion

        #region Exeファイル書出処理
        private static IntPtr ToPtr(object data)
        {
            GCHandle h = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr ptr;
            try
            {
                ptr = h.AddrOfPinnedObject();
            }
            finally
            {
                h.Free();
            }
            return ptr;
        }
        private void writeResourceFile(string dllFileName, string resourceName)
        {
            using (FileStream fileStream = new FileStream(dllFileName, FileMode.Create))
            {
                //変数宣言
                byte[] byteData = new byte[] { };

                //ファイル内のリソースよりファイルデータを取得
                var fileData = StellarRoboEditor.Resource.ResourceManager.GetObject(resourceName);

                //String型か？
                if (fileData is String)
                {
                    //文字列をByte配列に
                    byteData = Encoding.UTF8.GetBytes(fileData.ToString());
                }
                else
                {
                    //Object型をByte配列に
                    byteData = (byte[])fileData;
                }

                //取得したデータを書き込む
                fileStream.Write(byteData, 0, byteData.Length);
            }

        }
        private void WriteFiletoolStripButton_Click(object sender, EventArgs e)
        {
            //変数宣言
            string Source = string.Empty;
            string filePath = string.Empty;
            StellarRoboLexResult lexer;
            StellarRoboAst ast;

            //保存前にソースの字句及び構文解析を行う
            Source = _editor.Text.Trim();

            //ソースが空なら処理を行わない
            if (Source == string.Empty)
            {
                //処理せず抜ける
                return;
            }
            lexer = Analayze(Source);
            if (!lexer.Success)
            {
                //エラー
                return;
            }
            ast = Parse(lexer);
            if (!ast.Success)
            {
                //エラー
                return;
            }
            //ファイルは保存されているか？
            //されていないのならば、一旦保存してもらう
            SavetoolStripButton_Click(sender, e);

            //ファイル名は指定されているか？
            if (fileName.Trim() != string.Empty)
            {
                //保存先を指定
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    #region Scriptソース暗号化
                    //暗号化情報取得
                    string AES_IV = AESCryption.AESCryption.AES_IV;
                    string AES_KEY = AESCryption.AESCryption.AES_KEY;

                    //プレーンテキスト取得
                    string PlainText = _editor.Text;

                    //Include命令が存在するか？
                    Queue<StellarRoboToken> tokens = new Queue<StellarRoboToken>(lexer.Tokens);
                    while (tokens.Count != 0)
                    {
                        //Tokenを取得する
                        StellarRoboToken token = tokens.Dequeue();

                        //Include命令か？
                        if (token.Type == StellarRoboTokenType.IncludeKeyWord)
                        {
                            //Tokenは残っているか？
                            if (tokens.Count != 0)
                            {
                                //Includeされているファイルを取得
                                StellarRoboToken fileToken = tokens.Dequeue();

                                //ファイルパス取得
                                filePath = fileToken.TokenString;

                                //ファイルを読み込む
                                string includeScript = File.ReadAllText(filePath);

                                //読み込んだScriptでIncludeを置換する
                                PlainText = Regex.Replace(PlainText, string.Format(" *include +\"{0}\"", fileToken.TokenString), includeScript);
                            }
                        }
                    }

                    //Base64に変換
                    byte[] PlainTextData = new byte[PlainText.Length];
                    PlainTextData = Encoding.UTF8.GetBytes(PlainText);
                    string PlainTextBase64 = System.Convert.ToBase64String(PlainTextData);

                    //暗号化し、Base64で変換
                    string EncryptData = AESCryption.AESCryption.Encrypt(PlainTextBase64, AES_IV, AES_KEY);
                    byte[] data = new byte[EncryptData.Length];
                    data = Encoding.UTF8.GetBytes(EncryptData);
                    #endregion

                    #region パラメタ作成
                    //ToolStrip上の設定を取得する
                    bool InterruptValue = ((CheckBox)toolStripCheckBoxInterrupt.Control).Checked;
                    byte[] InterruptFlg = Encoding.UTF8.GetBytes(InterruptValue.ToString());
                    #endregion

                    #region Exeファイル処理
                    //書き出すファイルの情報を取得
                    string writeExeFile = Path.Combine(folderBrowserDialog.SelectedPath, Path.GetFileNameWithoutExtension(fileName) + ".exe");
                    string dllPath = folderBrowserDialog.SelectedPath;

                    //ファイルは既に存在するか？
                    if (File.Exists(writeExeFile))
                    {
                        //上書き確認
                        if (MessageBox.Show("既に存在します。上書きしますか？", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        {
                            //処理を抜ける
                            return;
                        }
                    }

                    try
                    {
                        //マウスカーソル変更(砂時計)
                        this.Cursor = Cursors.WaitCursor;

                        //ファイル作成
                        writeResourceFile(writeExeFile, "StellarRoboProcess");

                        //ファイル作成(構築ファイル)
                        writeResourceFile(writeExeFile + ".config", "StellarRoboProcess_exe");

                        //DLL作成
                        foreach (KeyValuePair<string, string> item in exportDll)
                        {
                            //コピー元ファイル名作成
                            string sourceFileName = Path.Combine(Application.StartupPath, item.Value.ToString());
                            //コピー先ファイル名作成
                            string destinationFileName = Path.Combine(dllPath, item.Value.ToString());

                            //フォルダは存在するか？
                            if (!Directory.Exists(Path.GetDirectoryName(destinationFileName)))
                            {
                                //フォルダ作成
                                Directory.CreateDirectory(Path.GetDirectoryName(destinationFileName));
                            }

                            //コピー元のファイルは存在するか？
                            if(!File.Exists(sourceFileName))
                            {
                                //マウスカーソル変更(デフォルト)
                                this.Cursor = Cursors.WaitCursor;

                                //エラーメッセージ表示
                                MessageBox.Show("動作に必要なファイルがたりません。", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            //DLLをコピーする
                            File.Copy(sourceFileName, destinationFileName);
                        }
                    }
                    catch (IOException ioe)
                    {
                        //マウスカーソル変更(デフォルト)
                        this.Cursor = Cursors.WaitCursor;

                        //エラーメッセージ表示
                        MessageBox.Show(ioe.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    //リソースのハンドル取得
                    IntPtr handle = BeginUpdateResource(writeExeFile, false);
                    if (handle == IntPtr.Zero)
                    {
                        //マウスカーソル変更(デフォルト)
                        this.Cursor = Cursors.WaitCursor;

                        //エラーメッセージ表示
                        MessageBox.Show("Exeファイル作成に失敗", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    //書き出したExeファイルにリソースを埋め込む
                    #region Scriptソース
                    IntPtr ScriptHandle = ToPtr(data);
                    if (!UpdateResource(handle, "RT_STRING", "FileData", 1040, ScriptHandle, Convert.ToUInt32(data.Length)))
                    {
                        //マウスカーソル変更(デフォルト)
                        this.Cursor = Cursors.WaitCursor;

                        //エラーメッセージ表示
                        MessageBox.Show("Exeファイル作成に失敗", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    #endregion

                    #region 設定パラメタ
                    IntPtr InterruptHandle = ToPtr(InterruptFlg);
                    if (!UpdateResource(handle, "RT_STRING", "Interrupt", 1040, InterruptHandle, Convert.ToUInt32(InterruptFlg.Length)))
                    {
                        //マウスカーソル変更(デフォルト)
                        this.Cursor = Cursors.WaitCursor;

                        //エラーメッセージ表示
                        MessageBox.Show("Exeファイル作成に失敗", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    #endregion

                    //書き込んだリソースを確定する
                    if (!EndUpdateResource(handle, false))
                    {
                        //マウスカーソル変更(デフォルト)
                        this.Cursor = Cursors.WaitCursor;

                        //エラーメッセージ表示
                        MessageBox.Show("Exeファイル作成に失敗", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    //マウスカーソル変更(デフォルト)
                    this.Cursor = Cursors.WaitCursor;

                    //終了
                    MessageBox.Show("Exeファイル作成完了", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    #endregion
                }
            }
        }
        #endregion

        #region ファイル読み込み
        private void LoadtoolStripButton_Click(object sender, EventArgs e)
        {
            //読み込み前のテキストに変更が有れば確認する
            if(!checkUpdate(sender,e))
            {
                //処理を抜ける
                return;
            }

            //ファイル情報設定
            openFileDialog.Filter = "全てのﾌｧｲﾙ(*.*)|*.*|StellarRoboﾌｧｲﾙ(*.srd)|*.srd";
            openFileDialog.FilterIndex = 2;
            openFileDialog.DefaultExt = "srd";
            openFileDialog.InitialDirectory = filePath;

            //ファイルを選択
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //ファイル読み込み
                fileRead(openFileDialog.FileName);
            }
        }
        private void fileRead(string readFileName)
        {
            //Save時に使用するので、ファイル名を保存しておく
            fileName = readFileName;

            //選択されたファイルを読み込む
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader streamReader = new StreamReader(fileStream))
            {
                //全てを読み込む
                string sourceData = streamReader.ReadToEnd();

                //改行を置換する
                sourceData = Regex.Replace(sourceData, NEW_LINE, SIMPLE_NEW_LINE);
                sourceData = Regex.Replace(sourceData, SIMPLE_NEW_LINE, NEW_LINE);

                //Scriptを表示
                _editor.Text = sourceData;

                //Editor初期値の更新
                _editor.Document.UndoStack.MarkAsOriginalFile();
            }

            //ディレクトリを取得
            filePath = Path.GetDirectoryName(fileName);

            //ファイル名を表示
            this.Text = string.Format("{0} - {1}", Path.GetFileName(fileName), headerText);

        }
        #endregion

        #region ファイル保存
        private void SavetoolStripButton_Click(object sender, EventArgs e)
        {
            //ファイル情報設定
            saveFileDialog.Filter = "StellarRoboﾌｧｲﾙ(*.srd)|*.srd";
            saveFileDialog.DefaultExt = "srd";
            saveFileDialog.InitialDirectory = filePath;
            saveFileDialog.FileName = fileName;

            //新規作成されたものか？
            if (fileName.Trim() == string.Empty)
            {
                //保存するファイル名を取得する
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Saveファイル名を保存する
                    fileName = saveFileDialog.FileName;

                    //ファイル名を表示
                    this.Text = string.Format("{0} - {1}", Path.GetFileName(fileName), headerText);
                }
                else
                {
                    if (e is FormClosingEventArgs)
                    {
                        ((FormClosingEventArgs)e).Cancel = true;
                    }
                    return;
                }
            }

            //ファイル名は指定されているか？
            if (fileName.Trim() != string.Empty)
            {
                //データを保存する
                _editor.Save(fileName);

                //Editor初期値の更新
                _editor.Document.UndoStack.MarkAsOriginalFile();
            }
        }
        #endregion

        #region 新規
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewtoolStripButton_Click(object sender, EventArgs e)
        {
            //新規処理
            //新規前のテキストに変更が有れば確認する
            if(!checkUpdate(sender,e))
            {
                //処理を抜ける
                return;
            }

            _editor.Text = string.Empty;
            fileName = string.Empty;
            filePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            this.Text = headerText;

            //Editor初期値の更新
            _editor.Document.UndoStack.MarkAsOriginalFile();
        }
        #endregion

        #endregion

        #region Hook関連
        private void RecordingtoolStripButton_Click(object sender, EventArgs e)
        {
            //確認
            if (MessageBox.Show("マウスの操作を録画します、録画を実行すると作業中の内容は破棄されます。\nよろしいですか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            //メニュー有効/無効設定
            setMenuState(EditorState.DuringRecording);

            //変数初期化
            Simulate.Clear();
            _editor.Text = string.Empty;

            //Editor初期値の更新
            _editor.Document.UndoStack.MarkAsOriginalFile();
            _editor.Document.UndoStack.ClearAll();

            //Hook開始
            KeyboardHook.AddEvent(RecordingHookKeyBoard);
            MouseHook.AddEvent(RecordingHookMouse);
        }

        private void StoptoolStripButton_Click(object sender, EventArgs e)
        {
            //変数宣言
            string item = string.Empty;
            string specialKey = string.Empty;
            int beforeTime = 0;
            int elapsedTime = 0;

            //Hookを終了する
            MouseHook.RemoveEvent(RecordingHookMouse);
            KeyboardHook.RemoveEvent(RecordingHookKeyBoard);

            #region 操作取込
            //操作されているか？
            if (Simulate.Count > 0)
            {
                //ヘッダ挿入
                _editor.Text = "func main" + NEW_LINE;

                //操作を挿入
                while (Simulate.Count != 0)
                {
                    //一件取得
                    SimulateData sim = Simulate.Dequeue();

                    //マウスイベント
                    if (sim.type == 0)
                    {
                        switch (sim.MouseStroke)
                        {
                            case MouseHook.Stroke.LEFT_DOWN:
                                item = "\tleft_down(" + sim.X + ", " + sim.Y + ")" + NEW_LINE;
                                break;
                            case MouseHook.Stroke.LEFT_UP:
                                item = "\tleft_up(" + sim.X + ", " + sim.Y + ")" + NEW_LINE;
                                break;
                            case MouseHook.Stroke.RIGHT_DOWN:
                                item = "\tright_down(" + sim.X + ", " + sim.Y + ")" + NEW_LINE;
                                break;
                            case MouseHook.Stroke.RIGHT_UP:
                                item = "\tright_up(" + sim.X + ", " + sim.Y + ")" + NEW_LINE;
                                break;
                            case MouseHook.Stroke.MIDDLE_DOWN:
                                item = "\tmiddle_down(" + sim.X + ", " + sim.Y + ")" + NEW_LINE;
                                break;
                            case MouseHook.Stroke.MIDDLE_UP:
                                item = "\tmiddle_up(" + sim.X + ", " + sim.Y + ")" + NEW_LINE;
                                break;
                            default:
                                break;
                        }
                    }
                    //キーボードイベント
                    else
                    {
                        //変数初期化
                        specialKey = string.Empty;
                        item = string.Empty;

                        switch (sim.Key.ToString())
                        {
                            //そのまま返せるキー
                            case var PushKey when Regex.IsMatch(PushKey.ToString(), "(Tab|End|NumLock|Left|Up|Right|Down|Insert|Home|Delete)"):
                                item = "{" + sim.Key.ToString().ToLower() + "}";
                                break;
                            //変換が必要なキー
                            case "Escape":
                                item = "{esc}";
                                break;
                            case "PageUp":
                                item = "{pgup}";
                                break;
                            case "Next":
                                item = "{pgdn}";
                                break;
                            case "Divide":
                                item = "/";
                                break;
                            case "Multiply":
                                item = "*";
                                break;
                            case "Subtract":
                                item = "-";
                                break;
                            case "Add":
                                item = "+";
                                break;
                            case "OemMinus":
                                item = (sim.ShiftKeyStatus) ? "=" : "-";
                                break;
                            case "Oemtilde":
                                item = (sim.ShiftKeyStatus) ? "`" : "@";
                                break;
                            case "OemOpenBrackets":
                                item = (sim.ShiftKeyStatus) ? "{" : "[";
                                break;
                            case "Oemplus":
                                item = (sim.ShiftKeyStatus) ? "PLUS" : ";";
                                break;
                            case "Oemcomma":
                                item = (sim.ShiftKeyStatus) ? "<" : ",";
                                break;
                            case "OemPeriod":
                                item = (sim.ShiftKeyStatus) ? ">" : ".";
                                break;
                            case "OemBackslash":
                                item = (sim.ShiftKeyStatus) ? "_" : "\\";
                                break;
                            case "Oem1":
                                item = (sim.ShiftKeyStatus) ? "*" : ":";
                                break;
                            case "Oem2":
                            case "OemQuestion":
                                item = (sim.ShiftKeyStatus) ? "?" : "/";
                                break;
                            case "Oem5":
                                item = (sim.ShiftKeyStatus) ? "|" : "\\";
                                break;
                            case "Oem6":
                                item = (sim.ShiftKeyStatus) ? "}" : "]";
                                break;
                            case "Oem7":
                                item = (sim.ShiftKeyStatus) ? "~" : "^";
                                break;
                            case var PushKey when Regex.IsMatch(PushKey.ToString(), "F[0-9].*"):
                                item = "{" + sim.Key.ToString().ToLower() + "}";
                                break;
                            case var PushKey when Regex.IsMatch(PushKey.ToString(), "^D[0-9]{1}$"):
                                item = sim.Key.ToString().Replace("D", "");
                                //シフトキーは押下されていたか？
                                if (sim.ShiftKeyStatus)
                                {
                                    item = "+(" + item + ")";
                                }
                                break;
                            case var PushKey when Regex.IsMatch(PushKey.ToString(), "NumPad[0-9]"):
                                item = sim.Key.ToString().Replace("NumPad", "");
                                break;
                            case "Capital":
                                item = "{capslock}";
                                break;
                            case "Space":
                                item = " ";
                                break;
                            case "Return":
                                item = "{enter}";
                                break;
                            case "Back":
                                item = "{backspace}";
                                break;
                            case "Pause":
                                item = "{break}";
                                break;
                            case "Scroll":
                                item = "{scrolllock}";
                                break;
                            case "PrintScreen":
                                item = "{prtsc}";
                                break;
                            case var PushKey when Regex.IsMatch(PushKey, "(L|R)ShiftKey"):
                                item = "+";
                                break;
                            case var PushKey when Regex.IsMatch(PushKey, "(L|R)ControlKey"):
                                item = "^";
                                break;
                            case var PushKey when Regex.IsMatch(PushKey, "(L|R)Menu"):
                                item = "%";
                                break;
                            default:
                                //上記に当てはまらなのならば捨てる
                                if (sim.Key.ToString().Length == 1)
                                {
                                    //大文字・小文字変換
                                    item = (((sim.CapsLockKeyStatus == true) && (sim.ShiftKeyStatus == true)) || ((sim.CapsLockKeyStatus == false) && (sim.ShiftKeyStatus == false))) ? sim.Key.ToString().ToLower() : sim.Key.ToString().ToUpper();

                                    //特殊キーは押下されているか？
                                    if (sim.CtrlKeyStatus)
                                    {
                                        specialKey = "^";
                                    }
                                    if (sim.AltKeyStatus)
                                    {
                                        specialKey = "%";
                                    }
                                    if (!string.IsNullOrEmpty(specialKey))
                                    {
                                        item = specialKey + "(" + item + ")";
                                    }
                                }
                                break;
                        }

                        //Itemが{}で囲われてるか、もしくは+、^、%の場合はsend_keysで送る
                        if (!string.IsNullOrEmpty(item))
                        {
                            if (Regex.IsMatch(item, "\\{.*\\}|\\+|\\^|%"))
                            {
                                item = "\tsend_keys(\"" + item + "\")" + NEW_LINE;
                            }
                            else
                            {
                                item = "\tinput_keys(\"" + (item == "PLUS" ? "+" : item) + "\")" + NEW_LINE;
                            }
                        }
                    }

                    //経過時間を求める
                    elapsedTime = getElapsedTime(ref beforeTime, (int)sim.Time);

                    //経過時間が0なら書き込まない
                    if (elapsedTime > 0)
                    {
                        _editor.Text += "\twait(" + elapsedTime.ToString() + ")" + NEW_LINE;
                    }

                    //書き込み
                    _editor.Text += item;
                }
                //フッタを挿入
                _editor.Text += "endfunc";
            }
            #endregion

            //メニュー有効/無効設定
            setMenuState(EditorState.None);
        }
        private void StartHook()
        {
            //キーボードはHookされているか？
            if (!KeyboardHook.IsHooking)
            {
                //キーボードはHookされていないので、Hookを開始する
                KeyboardHook.Start();
            }

            //マウスはHookされているか？
            if (!MouseHook.IsHooking)
            {
                //マウスはHookされていないので、Hookを開始する
                MouseHook.Start();
            }
        }
        private void StopHook()
        {
            //マウスはHookされているか？
            if (MouseHook.IsHooking)
            {
                //マウスはHookされているので、Hookを終了する
                MouseHook.Stop();
            }

            //キーボードはHookされているか？
            if (KeyboardHook.IsHooking)
            {
                //キーボードはHookされているので、Hookを終了する
                KeyboardHook.Stop();
            }
        }
        private int getElapsedTime(ref int beforeTime, int afterTime)
        {
            //変数宣言
            int elapsedTime = 0;

            //戻り値設定
            if (beforeTime == 0)
            {
                beforeTime = afterTime;
            }

            //経過時間取得
            elapsedTime = (afterTime - beforeTime);

            //次の為に保存
            beforeTime = afterTime;

            //戻り値設定
            return elapsedTime;
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
            if (!interruptFlg && !ctx.IsDebug)
            {
                //フラグ設定
                interruptFlg = true;

                //ハードウェアの動きのみ対象とする
                if (int.Parse(s.Flags.ToString()) != 1)
                {
                    //停止
                    interruptFlg = stopProcess("マウス操作を検知しました。\n実行を中止しますか？");
                }
            }
        }
        #endregion

        #region 録画用Hook
        void RecordingHookKeyBoard(ref KeyboardHook.StateKeyboard s)
        {
            if ((int.Parse(s.Flags.ToString()) & 16) == 0)
            {
                //キーアップだけを取得
                if (s.Stroke == KeyboardHook.Stroke.KEY_UP)
                {
                    //キー情報登録
                    SimulateData sim = new SimulateData();

                    sim.type = 1;
                    sim.KeyStroke = s.Stroke;
                    sim.Key = s.Key;
                    sim.ScanCode = s.ScanCode;
                    sim.Flags = s.Flags;
                    sim.Time = s.Time;
                    sim.ExtraInfo = s.ExtraInfo;

                    //特殊キーの状態を取得しておく
                    sim.ShiftKeyStatus = ((GetKeyState(VirtualKeyStates.VK_LSHIFT) < 0) || (GetKeyState(VirtualKeyStates.VK_RSHIFT) < 0));
                    sim.CtrlKeyStatus = (GetKeyState(VirtualKeyStates.VK_CONTROL) < 0);
                    sim.AltKeyStatus = ((GetKeyState(VirtualKeyStates.VK_LMENU) < 0) || (GetKeyState(VirtualKeyStates.VK_RMENU) < 0));
                    sim.CapsLockKeyStatus = (GetKeyState(VirtualKeyStates.VK_CAPITAL) < 0);

                    Simulate.Enqueue(sim);
                }
            }
        }
        void RecordingHookMouse(ref MouseHook.StateMouse s)
        {
            if (int.Parse(s.Flags.ToString()) != 1)
            {
                IntPtr handle = WindowFromPoint(new System.Drawing.Point(s.X, s.Y));
                //停止ボタンのイベントは登録しない
                if (StoptoolStripButton.GetCurrentParent().Handle == handle)
                {
                    return;
                }

                //イベントはマウス関連か？
                switch ((int)s.Stroke)
                {
                    case (int)MouseHook.Stroke.LEFT_DOWN:
                    case (int)MouseHook.Stroke.LEFT_UP:
                    case (int)MouseHook.Stroke.RIGHT_DOWN:
                    case (int)MouseHook.Stroke.RIGHT_UP:
                    case (int)MouseHook.Stroke.MIDDLE_DOWN:
                    case (int)MouseHook.Stroke.MIDDLE_UP:
                        //マウス関連のデータだけを取得する
                        SimulateData sim = new SimulateData();
                        sim.type = 0;
                        sim.MouseStroke = s.Stroke;
                        sim.X = s.X;
                        sim.Y = s.Y;
                        sim.Data = s.Data;
                        sim.Flags = s.Flags;
                        sim.Time = s.Time;
                        sim.ExtraInfo = s.ExtraInfo;

                        Simulate.Enqueue(sim);
                        break;
                }
            }
        }
        #endregion

        #region マウス座標用Hook
        void CoordinateHookMouse(ref MouseHook.StateMouse s)
        {
            if (int.Parse(s.Flags.ToString()) != 1)
            {
                //座標を取得する
                int x = s.X;
                int y = s.Y;

                //座標が-1ならば0にする
                if (x < 0) { x = 0; }
                if (y < 0) { y = 0; }

                //表示用座標作成
                string MouseCoordinate = x.ToString().PadLeft(6) + ":" + y.ToString().PadLeft(6);

                //座標表示
                toolStripStatusCoordinate.Text = MouseCoordinate;
            }
        }
        #endregion

        #endregion

        #region エディタ関連

        #region 表示関連
        private void SetDebugLine(int line)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new DelegateSetDebugLine(this.SetDebugLine), line);
                return;
            }

            //メニュー有効/無効設定
            setMenuState(EditorState.DuringDebugging);

            //現在の行を背景色変更
            _editor.TextArea.TextView.DebugHighlightedLine = line;
        }
        private void setMenuState(EditorState editorState)
        {
            //変数宣言
            bool NewState = false;
            bool LoadState = false;
            bool SaveState = false;
            bool ExecuteState = false;
            bool DebugExecuteState = false;
            bool ExecuteStopState = false;
            bool WriteFileState = false;
            bool RecordingState = false;
            bool StopState = false;
            bool InterruptState = false;
            bool DumpState = false;

            if (this.InvokeRequired)
            {
                this.Invoke(new DelegateSetMenuState(this.setMenuState), editorState);
                return;
            }

            switch (editorState)
            {
                //None
                //録画中
                case EditorState.None:
                case EditorState.DuringRecording:
                    NewState = true;
                    LoadState = true;
                    SaveState = true;
                    ExecuteState = true;
                    DebugExecuteState = true;
                    ExecuteStopState = false;
                    WriteFileState = true;
                    RecordingState = true;
                    StopState = true;
                    InterruptState = true;
                    DumpState = true;
                    break;
                //実行中
                case EditorState.Running:
                    ExecuteStopState = true;
                    break;
                //デバッグ中
                case EditorState.DuringDebugging:
                    ExecuteState = true;
                    DebugExecuteState = true;
                    ExecuteStopState = true;
                    break;
                //書き出し中
                case EditorState.DuringExport:
                    StopState = true;
                    break;
            }

            //新規
            NewtoolStripButton.Enabled = NewState;
            //読込
            LoadtoolStripButton.Enabled = LoadState;
            //保存
            SavetoolStripButton.Enabled = SaveState;
            //実行
            ExecutetoolStripButton.Enabled = ExecuteState;
            //デバッグ実行
            DebugExecutetoolStripButton.Enabled = DebugExecuteState;
            //実行停止
            ExecuteStoptoolStripButton.Enabled = ExecuteStopState;
            //書出
            WriteFiletoolStripButton.Enabled = WriteFileState;
            //録画
            RecordingtoolStripButton.Enabled = RecordingState;
            //停止
            StoptoolStripButton.Enabled = StopState;
            //割り込み
            Control[] ctrl = this.Controls.Find("checkboxInterrupt", true);
            if (ctrl.Length > 0)
            {
                ((CheckBox)ctrl[0]).Enabled = InterruptState;
            }
            Control[] ctrl2 = this.Controls.Find("checkboxDump", true);
            if (ctrl2.Length > 0)
            {
                ((CheckBox)ctrl2[0]).Enabled = DumpState;
            }
        }
        #endregion

        #region Drag&Drop
        private void Editor_DragDrop(object sender, System.Windows.DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (files.Length > 1)
            {
                MessageBox.Show("複数Dropは出来ません。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                //更新されているか？
                if (!checkUpdate(sender, e))
                {
                    //処理を抜ける
                    return;
                }

                //EditorにDropされたファイルを読み込む
                fileRead(files[0]);
            }
        }
        private void Editor_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = System.Windows.DragDropEffects.Move;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
        }
        #endregion

        #region 補完処理
        private string GetKeyWord()
        {
            //変数宣言
            string keyWord = string.Empty;

            //現在のキャレット位置を取得する
            TextLocation textLocation = _editor.Document.GetLocation(_editor.CaretOffset);

            //現在のキャレット位置に基づいて1行テキストを取得する
            DocumentLine line = _editor.Document.GetLineByNumber(textLocation.Line);

            //全角ブランクから、半角ブランクに置き返す
            keyWord = _editor.Document.GetText(line.Offset, line.Length).Replace('　', ' ');

            //ブランクはあるか？
            int pos = keyWord.LastIndexOf(' ');
            if (pos != -1)
            {
                keyWord = keyWord.Substring(pos);
            }

            #region 正規表現で使用するキャラクタを置換する
            //正規表現で使用するキャラクタを置換する
            keyWord = keyWord.Replace("*", "\\*");
            keyWord = keyWord.Replace(".", "\\.");
            keyWord = keyWord.Replace("+", "\\+");
            keyWord = keyWord.Replace("?", "\\?");
            keyWord = keyWord.Replace("(", "\\(");
            keyWord = keyWord.Replace(")", "\\)");
            keyWord = keyWord.Replace("{", "\\{");
            keyWord = keyWord.Replace("}", "\\}");
            keyWord = keyWord.Replace("[", "\\[");
            keyWord = keyWord.Replace("]", "\\]");
            keyWord = keyWord.Replace("^", "\\^");
            keyWord = keyWord.Replace("$", "\\$");
            keyWord = keyWord.Replace("-", "\\-");
            keyWord = keyWord.Replace("|", "\\|");
            #endregion

            //戻り値設定
            return keyWord.Trim();
        }
        private void TextArea_TextEntering(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }
        private void UndoStack_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //テキストが変更されたか
            if(_editor.Document.UndoStack.IsUpdate)
            {
                //ファイル名は指定されているか？
                if (fileName.Trim() != string.Empty)
                {
                    this.Text = string.Format("{0}(更新) - {1}", Path.GetFileName(fileName), headerText);
                }
                else
                {
                    this.Text = string.Format("(更新) - {0}", headerText);
                }
            }
            else
            {
                //ファイル名は指定されているか？
                if (fileName.Trim() != string.Empty)
                {
                    this.Text = string.Format("{0} - {1}", Path.GetFileName(fileName), headerText);
                }
                else
                {
                    this.Text = headerText;
                }
            }
        }
#pragma warning disable CS1998
        private async void TextArea_TextEntered(object sender, System.Windows.Input.TextCompositionEventArgs e)
#pragma warning restore CS1998
        {
            //変数宣言
            IList<ICompletionData> data;
            string keyWord = string.Empty;
            string EscapeKeyWord = string.Empty;

            //キー入力された場合、エラー表示行を初期化する
            _editor.TextArea.TextView.ErrorHighlightedLine = 0;

            completionWindow = new CompletionWindow(_editor.TextArea);

            //キーワードを取得する
            keyWord = GetKeyWord();

            //KeyWordは取得出来たか？
            if (!string.IsNullOrEmpty(keyWord))
            {
                //CompletionData取得
                data = completionWindow.CompletionList.CompletionData;

                //正規表現の為にエスケープする
                for (int i = 0; i < keyWord.Length; i++)
                {
                    if (Regex.IsMatch(keyWord[i].ToString(), "(\\\\|\\*|\\+|\\.|\\?|\\{|\\}|\\(|\\)|\\[|\\]|\\^|\\$|\\-|\\|)"))
                    {
                        EscapeKeyWord += @"\" + keyWord[i].ToString();
                    }
                    else
                    {
                        EscapeKeyWord += keyWord[i].ToString();
                    }
                }

                //関数一覧から先頭一致でヒットする関数名を取得する
                foreach (ICompletionData item in AllCompletionsData.Where(x => Regex.IsMatch(x.Text, "^" + EscapeKeyWord)).OrderBy(x => x.Text))
                {
                    data.Add(item);
                }

                //表示する補完データは存在するか？
                if (data.Count > 0)
                {
                    completionWindow.Show();
                    completionWindow.Closed += delegate
                    {
                        completionWindow = null;
                    };
                    completionWindow.PreviewKeyDown += PreviewKeyDown;
                }
            }
        }
        private void SetCompletionsData()
        {
            //初期化されているか？
            if (AllCompletionsData == null)
            {
                //初期化
                AllCompletionsData = new List<ICompletionData>();
            }

            //補完データ作成
            #region 関数
            AllCompletionsData.Add(new CompletionData("now", StellarRoboEditor.Suggestion.Suggestion.now));
            AllCompletionsData.Add(new CompletionData("global_variable", StellarRoboEditor.Suggestion.Suggestion.global_variable));
            AllCompletionsData.Add(new CompletionData("read_xml", StellarRoboEditor.Suggestion.Suggestion.read_xml));
            //AllCompletionsData.Add(new CompletionData("include", "インクルード"));
            AllCompletionsData.Add(new CompletionData("set_ime_status", StellarRoboEditor.Suggestion.Suggestion.set_ime_status));
            AllCompletionsData.Add(new CompletionData("get_ime_status", StellarRoboEditor.Suggestion.Suggestion.get_ime_status));
            AllCompletionsData.Add(new CompletionData("check_data", StellarRoboEditor.Suggestion.Suggestion.check_data));
            AllCompletionsData.Add(new CompletionData("check_date", StellarRoboEditor.Suggestion.Suggestion.check_date));
            AllCompletionsData.Add(new CompletionData("get_reg_ex_count", StellarRoboEditor.Suggestion.Suggestion.get_reg_ex_count));
            AllCompletionsData.Add(new CompletionData("to_int", StellarRoboEditor.Suggestion.Suggestion.to_int));
            //AllCompletionsData.Add(new CompletionData("to_str", "数値から数字に変換"));
            AllCompletionsData.Add(new CompletionData("message_box", StellarRoboEditor.Suggestion.Suggestion.message_box));
            AllCompletionsData.Add(new CompletionData("get_error_code", StellarRoboEditor.Suggestion.Suggestion.get_error_code));
            AllCompletionsData.Add(new CompletionData("get_error_message", StellarRoboEditor.Suggestion.Suggestion.get_error_message));
            AllCompletionsData.Add(new CompletionData("init_file", StellarRoboEditor.Suggestion.Suggestion.init_file));
            AllCompletionsData.Add(new CompletionData("write_file", StellarRoboEditor.Suggestion.Suggestion.write_file));
            AllCompletionsData.Add(new CompletionData("read_file", StellarRoboEditor.Suggestion.Suggestion.read_file));
            AllCompletionsData.Add(new CompletionData("file_copy", StellarRoboEditor.Suggestion.Suggestion.file_copy));
            AllCompletionsData.Add(new CompletionData("file_move", StellarRoboEditor.Suggestion.Suggestion.file_move));
            AllCompletionsData.Add(new CompletionData("file_rename", StellarRoboEditor.Suggestion.Suggestion.file_rename));
            AllCompletionsData.Add(new CompletionData("file_delete", StellarRoboEditor.Suggestion.Suggestion.file_delete));
            AllCompletionsData.Add(new CompletionData("file_exists", StellarRoboEditor.Suggestion.Suggestion.file_exists));
            AllCompletionsData.Add(new CompletionData("directory_exists", StellarRoboEditor.Suggestion.Suggestion.directory_exists));
            AllCompletionsData.Add(new CompletionData("search_file", StellarRoboEditor.Suggestion.Suggestion.search_file));
            AllCompletionsData.Add(new CompletionData("get_file_name", StellarRoboEditor.Suggestion.Suggestion.get_file_name));
            AllCompletionsData.Add(new CompletionData("combine_path", StellarRoboEditor.Suggestion.Suggestion.combine_path));
            AllCompletionsData.Add(new CompletionData("click", StellarRoboEditor.Suggestion.Suggestion.click));
            AllCompletionsData.Add(new CompletionData("move", StellarRoboEditor.Suggestion.Suggestion.move));
            AllCompletionsData.Add(new CompletionData("left_down", StellarRoboEditor.Suggestion.Suggestion.left_down));
            AllCompletionsData.Add(new CompletionData("left_up", StellarRoboEditor.Suggestion.Suggestion.left_up));
            AllCompletionsData.Add(new CompletionData("left_click", StellarRoboEditor.Suggestion.Suggestion.left_click));
            AllCompletionsData.Add(new CompletionData("left_double_click", StellarRoboEditor.Suggestion.Suggestion.left_double_click));
            AllCompletionsData.Add(new CompletionData("right_down", StellarRoboEditor.Suggestion.Suggestion.right_down));
            AllCompletionsData.Add(new CompletionData("right_up", StellarRoboEditor.Suggestion.Suggestion.right_up));
            AllCompletionsData.Add(new CompletionData("right_click", StellarRoboEditor.Suggestion.Suggestion.right_click));
            AllCompletionsData.Add(new CompletionData("right_double_click", StellarRoboEditor.Suggestion.Suggestion.right_double_click));
            AllCompletionsData.Add(new CompletionData("middle_down", StellarRoboEditor.Suggestion.Suggestion.middle_down));
            AllCompletionsData.Add(new CompletionData("middle_up", StellarRoboEditor.Suggestion.Suggestion.middle_up));
            AllCompletionsData.Add(new CompletionData("middle_click", StellarRoboEditor.Suggestion.Suggestion.middle_click));
            AllCompletionsData.Add(new CompletionData("middle_double_click", StellarRoboEditor.Suggestion.Suggestion.middle_double_click));
            AllCompletionsData.Add(new CompletionData("input_keys", StellarRoboEditor.Suggestion.Suggestion.input_keys));
            AllCompletionsData.Add(new CompletionData("send_keys", StellarRoboEditor.Suggestion.Suggestion.send_keys));
            AllCompletionsData.Add(new CompletionData("wait", StellarRoboEditor.Suggestion.Suggestion.wait));
            AllCompletionsData.Add(new CompletionData("app_open", StellarRoboEditor.Suggestion.Suggestion.app_open));
            AllCompletionsData.Add(new CompletionData("app_close", StellarRoboEditor.Suggestion.Suggestion.app_close));
            AllCompletionsData.Add(new CompletionData("app_active", StellarRoboEditor.Suggestion.Suggestion.app_active));
            AllCompletionsData.Add(new CompletionData("app_wait", StellarRoboEditor.Suggestion.Suggestion.app_wait));
            //AllCompletionsData.Add(new CompletionData("app_window_enable", "指定ハンドルのウィンドが使用可能かを"));
            AllCompletionsData.Add(new CompletionData("app_set_pos", StellarRoboEditor.Suggestion.Suggestion.app_set_pos));
            AllCompletionsData.Add(new CompletionData("get_window_handle", StellarRoboEditor.Suggestion.Suggestion.get_window_handle));
            AllCompletionsData.Add(new CompletionData("set_window_pos_z", StellarRoboEditor.Suggestion.Suggestion.set_window_pos_z));
            AllCompletionsData.Add(new CompletionData("get_window_text", StellarRoboEditor.Suggestion.Suggestion.get_window_text));
            AllCompletionsData.Add(new CompletionData("get_window_handle_point", StellarRoboEditor.Suggestion.Suggestion.get_window_handle_point));
            AllCompletionsData.Add(new CompletionData("get_window_handle_parent", StellarRoboEditor.Suggestion.Suggestion.get_window_handle_parent));
            AllCompletionsData.Add(new CompletionData("get_parent_title", StellarRoboEditor.Suggestion.Suggestion.get_parent_title));
            AllCompletionsData.Add(new CompletionData("image_match", StellarRoboEditor.Suggestion.Suggestion.image_match));
            AllCompletionsData.Add(new CompletionData("get_acc_name", StellarRoboEditor.Suggestion.Suggestion.get_acc_name));
            AllCompletionsData.Add(new CompletionData("get_acc_role", StellarRoboEditor.Suggestion.Suggestion.get_acc_role));
            AllCompletionsData.Add(new CompletionData("get_acc_value", StellarRoboEditor.Suggestion.Suggestion.get_acc_value));
            AllCompletionsData.Add(new CompletionData("get_acc_is_checked", StellarRoboEditor.Suggestion.Suggestion.get_acc_is_checked));
            AllCompletionsData.Add(new CompletionData("browser_navigate", StellarRoboEditor.Suggestion.Suggestion.browser_navigate));
            AllCompletionsData.Add(new CompletionData("browser_open_url", StellarRoboEditor.Suggestion.Suggestion.browser_open_url));
            AllCompletionsData.Add(new CompletionData("browser_wait", StellarRoboEditor.Suggestion.Suggestion.browser_wait));
            AllCompletionsData.Add(new CompletionData("browser_get_coordinate", StellarRoboEditor.Suggestion.Suggestion.browser_get_coordinate));
            AllCompletionsData.Add(new CompletionData("browser_input", StellarRoboEditor.Suggestion.Suggestion.browser_input));
            AllCompletionsData.Add(new CompletionData("browser_output", StellarRoboEditor.Suggestion.Suggestion.browser_output));
            AllCompletionsData.Add(new CompletionData("browser_close", StellarRoboEditor.Suggestion.Suggestion.browser_close));
            AllCompletionsData.Add(new CompletionData("browser_quit", StellarRoboEditor.Suggestion.Suggestion.browser_quit));
            AllCompletionsData.Add(new CompletionData("browser_click", StellarRoboEditor.Suggestion.Suggestion.browser_click));
            AllCompletionsData.Add(new CompletionData("browser_check_id", StellarRoboEditor.Suggestion.Suggestion.browser_check_id));
            AllCompletionsData.Add(new CompletionData("browser_select_box", StellarRoboEditor.Suggestion.Suggestion.browser_select_box));
            AllCompletionsData.Add(new CompletionData("browser_list", StellarRoboEditor.Suggestion.Suggestion.browser_list));
            AllCompletionsData.Add(new CompletionData("browser_change", StellarRoboEditor.Suggestion.Suggestion.browser_change));
            AllCompletionsData.Add(new CompletionData("browser_owner", StellarRoboEditor.Suggestion.Suggestion.browser_owner));
            AllCompletionsData.Add(new CompletionData("excel_open", StellarRoboEditor.Suggestion.Suggestion.excel_open));
            AllCompletionsData.Add(new CompletionData("excel_close", StellarRoboEditor.Suggestion.Suggestion.excel_close));
            AllCompletionsData.Add(new CompletionData("excel_get_cell", StellarRoboEditor.Suggestion.Suggestion.excel_get_cell));
            AllCompletionsData.Add(new CompletionData("excel_set_cell", StellarRoboEditor.Suggestion.Suggestion.excel_set_cell));
            AllCompletionsData.Add(new CompletionData("excel_search", StellarRoboEditor.Suggestion.Suggestion.excel_search));
            AllCompletionsData.Add(new CompletionData("excel_get_search_result", StellarRoboEditor.Suggestion.Suggestion.excel_get_search_result));
            AllCompletionsData.Add(new CompletionData("excel_get_search_count", StellarRoboEditor.Suggestion.Suggestion.excel_get_search_count));
            AllCompletionsData.Add(new CompletionData("excel_add_sheet", StellarRoboEditor.Suggestion.Suggestion.excel_add_sheet));
            AllCompletionsData.Add(new CompletionData("excel_remove_sheet", StellarRoboEditor.Suggestion.Suggestion.excel_remove_sheet));
            AllCompletionsData.Add(new CompletionData("get_download_path", StellarRoboEditor.Suggestion.Suggestion.get_download_path));
            AllCompletionsData.Add(new CompletionData("folder_open", StellarRoboEditor.Suggestion.Suggestion.folder_open));
            AllCompletionsData.Add(new CompletionData("get_windows_version", StellarRoboEditor.Suggestion.Suggestion.get_windows_version));
            AllCompletionsData.Add(new CompletionData("logger", StellarRoboEditor.Suggestion.Suggestion.logger));
            AllCompletionsData.Add(new CompletionData("capture", StellarRoboEditor.Suggestion.Suggestion.capture));
            AllCompletionsData.Add(new CompletionData("date_add", StellarRoboEditor.Suggestion.Suggestion.date_add));
            AllCompletionsData.Add(new CompletionData("input_box", StellarRoboEditor.Suggestion.Suggestion.input_box));
            AllCompletionsData.Add(new CompletionData("input_text_box", StellarRoboEditor.Suggestion.Suggestion.input_text_box));
            AllCompletionsData.Add(new CompletionData("output_text_box", StellarRoboEditor.Suggestion.Suggestion.output_text_box));
            AllCompletionsData.Add(new CompletionData("get_current_path", StellarRoboEditor.Suggestion.Suggestion.get_current_path));
            AllCompletionsData.Add(new CompletionData("set_current_path", StellarRoboEditor.Suggestion.Suggestion.set_current_path));
            #endregion
            #region 命令
            AllCompletionsData.Add(new CompletionData("func", "関数宣言"));
            AllCompletionsData.Add(new CompletionData("if", "IF文"));
            AllCompletionsData.Add(new CompletionData("case", "CASE文"));
            AllCompletionsData.Add(new CompletionData("for", "FOR文"));
            AllCompletionsData.Add(new CompletionData("while", "WHILE文"));
            AllCompletionsData.Add(new CompletionData("foreach", "FOREACH文"));
            #endregion

        }
        #endregion

        #region スニペット処理
#pragma warning disable CS0108
        private void PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
#pragma warning restore CS0108
        {
            //キー入力された場合、エラー表示行を初期化する
            _editor.TextArea.TextView.ErrorHighlightedLine = 0;

            switch (e.Key)
            {
                //Tab
                case System.Windows.Input.Key.Tab:
                    if (sender is CompletionWindow)
                    {
                        SendKeys.Send("{down}");
                    }
                    else
                    {
                        #region Snippet処理
                        try
                        {
                            //関数名を取得する
                            string functionName = GetKeyWord();

                            //クラスの型を取得する
                            Type type = Type.GetType("StellarRoboEditor.FormMain");

                            //クラスの型を頼りにインスタンスを生成する
                            object temporary = System.Activator.CreateInstance(type);

                            //メソッド属性を取得する
                            MethodInfo methodInfo = type.GetMethod(functionName + "_order");

                            //メソッド属性を頼りにメソッドを呼ぶ
                            object result = methodInfo.Invoke(temporary, null);

                            //現在のキャレット位置取得
                            int offset = _editor.CaretOffset;

                            //スニペットを呼び出した際、元々書かれていた関数名とスニペットの関数名が二重になるので元の関数名を消す
                            _editor.Document.Remove(offset - functionName.Length, functionName.Length);

                            //スニペットを呼び出す
                            ((Snippet)result).Insert(_editor.TextArea);

                            //入力を無効に
                            e.Handled = true;
                        }
                        catch (Exception)
                        {
                            //関数が存在しなかった
                        }
                        #endregion
                    }
                    break;
                //F5、F8
                case System.Windows.Input.Key.F5:
                case System.Windows.Input.Key.F8:
                    //Debug状態を取得
                    bool IsDebug = (e.Key == System.Windows.Input.Key.F8);

                    //Script実行
                    DebugExecute(IsDebug);

                    break;
                case System.Windows.Input.Key.F9:
                    //現在のBreakPoint情報を取得
                    List<int> breakPoint = _editor.TextArea.TextView.BreakHighlightedLine;

                    //Caret位置を取得
                    Caret nowCaret = _editor.TextArea.Caret;

                    //Caret位置を元に行の情報を取得する
                    DocumentLine line = _editor.Document.GetLineByNumber(nowCaret.Line);
                    string text = _editor.Document.GetText(line.Offset, line.Length);

                    #region BreakPoint処理
                    //文字が入力されていなければBreakPointを貼らない
                    if (!string.IsNullOrEmpty(text.Replace('\t', ' ').Trim()))
                    {
                        //既に存在しているか？
                        int idx = breakPoint.FindIndex(x => x == nowCaret.Line);
                        if (idx >= 0)
                        {
                            //存在しているので消す
                            breakPoint.RemoveAt(idx);
                        }
                        else
                        {
                            //存在していないので追加
                            breakPoint.Add(nowCaret.Line);
                        }
                    }
                    #endregion

                    //BreakPoint情報を設定
                    _editor.TextArea.TextView.BreakHighlightedLine = breakPoint;
                    break;

                //Ctrl+s
                case System.Windows.Input.Key.S:
                    if (System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.LeftCtrl) == System.Windows.Input.KeyStates.Down ||
                       System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.RightCtrl) == System.Windows.Input.KeyStates.Down)
                    {
                        SavetoolStripButton_Click(sender, e);
                    }
                    break;
            }
        }
        public Snippet now_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="now(\"" },
                    new SnippetReplaceableTextElement{Text="yyyy/MM/dd hh:mm:ss" },
                    new SnippetTextElement{Text="\")\n" },
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet global_variable_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="global_variable(\""},
                    new SnippetReplaceableTextElement{Text="VariableName"},
                    new SnippetTextElement{Text="\")\n" },
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet read_xml_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="read_xml(\""},
                    new SnippetReplaceableTextElement{Text="XmlName"},
                    new SnippetTextElement{Text="\", \"" },
                    new SnippetReplaceableTextElement{Text="ElementName"},
                    new SnippetTextElement{Text="\")\n" },
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        //        public Snippet ShowSnippetInclude() { }
        public Snippet set_ime_status_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="set_ime_status("},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet get_ime_status_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="get_ime_status()\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet check_data_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="check_data(\"" },
                    new SnippetReplaceableTextElement{Text="TextData"},
                    new SnippetTextElement{Text="\", \"" },
                    new SnippetReplaceableTextElement{Text="Pattern"},
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet check_date_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="check_date(\"" },
                    new SnippetReplaceableTextElement{Text="Date"},
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet get_reg_ex_count_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="get_reg_ex_count(\""},
                    new SnippetReplaceableTextElement{Text="TextData" },
                    new SnippetTextElement{Text="\", \"" },
                    new SnippetReplaceableTextElement{Text="Pattern" },
                    new SnippetTextElement{Text="\", " },
                    new SnippetReplaceableTextElement{Text="0" },
                    new SnippetTextElement{Text=" )\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet to_int_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="to_int(\""},
                    new SnippetReplaceableTextElement{Text="StringValue" },
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        //private void ShowSnippetTo_str() { }
        public Snippet message_box_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="message_box(\""},
                    new SnippetReplaceableTextElement{Text="Message" },
                    new SnippetTextElement{Text="\", " },
                    new SnippetReplaceableTextElement{Text="0" },
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="64"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet init_file_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="init_file(\""},
                    new SnippetReplaceableTextElement{Text="FilePath" },
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet write_file_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="write_file(\""},
                    new SnippetReplaceableTextElement{Text="FileName" },
                    new SnippetTextElement{Text ="\", \""},
                    new SnippetReplaceableTextElement{Text="TextData" },
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="UTF-8" },
                    new SnippetTextElement{Text="\")\n" },
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet read_file_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="read_file(\""},
                    new SnippetReplaceableTextElement{Text="FileName" },
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="UTF-8"},
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet file_copy_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="file_copy(\""},
                    new SnippetReplaceableTextElement{Text="sourceFileName"},
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="destFileName"},
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet file_move_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="file_move(\""},
                    new SnippetReplaceableTextElement{Text="sourceFileName"},
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="destFileName"},
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet file_rename_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="file_rename(\""},
                    new SnippetReplaceableTextElement{Text="filePath" },
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="sourceFileName"},
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="destFileName"},
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet file_delete_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="file_delete(\""},
                    new SnippetReplaceableTextElement{Text="fileName" },
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet file_exists_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="file_exists(\"" },
                    new SnippetReplaceableTextElement{Text="fileName"},
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet directory_exists_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="directory_exists(\"" },
                    new SnippetReplaceableTextElement{Text="directoryName"},
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet search_file_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="search_file(\"" },
                    new SnippetReplaceableTextElement{Text="filePath"},
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="fileName"},
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet get_file_name_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="get_file_name(\"" },
                    new SnippetReplaceableTextElement{Text="filePath"},
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet combine_path_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="combine_path(\"" },
                    new SnippetReplaceableTextElement{Text="filePath"},
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="fileName"},
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet click_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="click(\"" },
                    new SnippetReplaceableTextElement{Text="Handle"},
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="SearchWord"},
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet move_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="move( "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet left_down_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="left_down( "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet left_up_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="left_up( "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet left_click_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="left_click( "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet left_double_click_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="left_double_click( "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet right_down_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="right_down( "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet right_up_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="right_up( "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet right_click_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="right_click( "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet right_double_click_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="right_double_click( "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet middle_down_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="middle_down( "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet middle_up_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="middle_up( "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet middle_click_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="middle_click( "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet middle_double_click_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="middle_double_click( "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet input_keys_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="input_keys(\""},
                    new SnippetReplaceableTextElement{Text="TextData"},
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="1000" },
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet input_text_box_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="input_text_box(\""},
                    new SnippetReplaceableTextElement{Text="Handle"},
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="SearchWord"},
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="Message"},
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="0" },
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet output_text_box_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="output_text_box(\""},
                    new SnippetReplaceableTextElement{Text="Handle"},
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="SearchWord"},
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="0" },
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet send_keys_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="send_keys(\""},
                    new SnippetReplaceableTextElement{Text="SendData" },
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet wait_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="wait("},
                    new SnippetReplaceableTextElement{Text="1000"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet app_open_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="app_open(\""},
                    new SnippetReplaceableTextElement{Text="FileName"},
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", " },
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet app_close_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="app_close(\""},
                    new SnippetReplaceableTextElement{Text="WindowTitle"},
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet app_active_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="app_active(\""},
                    new SnippetReplaceableTextElement{Text="ClassName" },
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="WindowTitle"},
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet app_wait_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="app_wait(\""},
                    new SnippetReplaceableTextElement{Text="WindowTitle"},
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        //        public Snippet ShowSnippetApp_window_enable() { }
        public Snippet app_set_pos_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="app_set_pos(\""},
                    new SnippetReplaceableTextElement{Text="ClassName"},
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="WindowTitle"},
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet get_window_handle_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="get_window_handle(\""},
                    new SnippetReplaceableTextElement{Text="ClassName"},
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="WindowTitle"},
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet set_window_pos_z_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="set_window_pos_z("},
                    new SnippetReplaceableTextElement{Text="WindowHandle"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="1"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet get_window_text_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="get_window_text("},
                    new SnippetReplaceableTextElement{Text="WindowHandle" },
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet get_window_handle_point_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="get_window_handle_point()\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet get_window_handle_parent_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="get_window_handle_parent("},
                    new SnippetReplaceableTextElement{Text="WindowHandle"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet get_parent_title_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="get_parent_title("},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet image_match_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="image_match(\""},
                    new SnippetReplaceableTextElement{Text="TemplateFilePath" },
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="0.8" },
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="1000" },
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0" },
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0" },
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="1920" },
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="1080" },
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="false" },
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="false" },
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet get_acc_name_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="get_acc_name("},
                    new SnippetReplaceableTextElement{Text="0" },
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet get_acc_role_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="get_acc_role("},
                    new SnippetReplaceableTextElement{Text="0" },
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet get_acc_value_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="get_acc_value("},
                    new SnippetReplaceableTextElement{Text="0" },
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet get_acc_is_checked_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="get_acc_is_checked("},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet browser_navigate_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="browser_navigate(\""},
                    new SnippetReplaceableTextElement{Text="URL" },
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet browser_open_url_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="browser_open_url(\""},
                    new SnippetReplaceableTextElement{Text="BrowserKey" },
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="URL"},
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet browser_wait_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="browser_wait(\""},
                    new SnippetReplaceableTextElement{Text="BrowserKey" },
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="0" },
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet browser_check_id_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="browser_check_id(\""},
                    new SnippetReplaceableTextElement{Text="BrowserKey" },
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="TargetID" },
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="0" },
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet browser_get_coordinate_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{ Text="browser_get_coordinate(\""},
                    new SnippetReplaceableTextElement{Text="BrowserKey" },
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="Element" },
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="0" },
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet browser_input_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="browser_input(\"" },
                    new SnippetReplaceableTextElement{Text="BrowserKey" },
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="InputData" },
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="Element" },
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="0" },
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet browser_output_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="browser_output(\"" },
                    new SnippetReplaceableTextElement{Text="BrowserKey" },
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="Element" },
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="0" },
                    new SnippetTextElement{Text=", \"" },
                    new SnippetReplaceableTextElement{Text="AttributeName" },
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet browser_close_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="browser_close(\""},
                    new SnippetReplaceableTextElement{Text="BrowserKey" },
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet browser_quit_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="browser_quit(\""},
                    new SnippetReplaceableTextElement{Text="BrowserKey" },
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet browser_click_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="browser_click(\""},
                    new SnippetReplaceableTextElement{Text="BrowserKey" },
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="Element" },
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="0" },
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet browser_select_box_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="browser_select_box(\""},
                    new SnippetReplaceableTextElement{Text="BrowserKey" },
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="Element" },
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="Message" },
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="0" },
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet browser_list_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="browser_list(\""},
                    new SnippetReplaceableTextElement{Text="BrowserKey" },
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet browser_change_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="browser_change(\""},
                    new SnippetReplaceableTextElement{Text="BrowserKey" },
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="ChangeOwnerHandle" },
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet browser_owner_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="browser_owner(\""},
                    new SnippetReplaceableTextElement{Text="BrowserKey" },
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet func_order()
        {
            //変数宣言
            Snippet snippet;

            //現在入力中の中にfunc mainは存在しているか？
            if (_editor.Text.IndexOf("func main") == -1)
            {
                //スニペット本体作成(func main)
                snippet = new Snippet
                {
                    Elements =
                    {
                        new SnippetTextElement{Text="func main\n"},
                        new SnippetCaretElement(),
                        new SnippetTextElement{Text="\nendfunc\n"}
                    }
                };
            }
            else
            {
                //スニペット本体作成(func FunctionName)
                snippet = new Snippet
                {
                    Elements =
                    {
                        new SnippetTextElement{Text="func " },
                        new SnippetReplaceableTextElement{Text="FunctionName" },
                        new SnippetTextElement{Text="\n" },
                        new SnippetCaretElement(),
                        new SnippetTextElement{Text="\nendfunc\n"}
                    }
                };
            }

            //作成した内容を反映する
            return snippet;
        }
        public Snippet if_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="if("},
                    new SnippetReplaceableTextElement{Text="expression"},
                    new SnippetTextElement{Text=") then\n" },
                    new SnippetCaretElement(),
                    new SnippetTextElement{Text="\nendif\n" }
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet case_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="case("},
                    new SnippetReplaceableTextElement{Text="target"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetTextElement{Text="\twhen "},
                    new SnippetReplaceableTextElement{Text="val" },
                    new SnippetTextElement{Text=":\n"},
                    new SnippetTextElement{Text="\t\t//Processing\n"},
                    new SnippetCaretElement(),
                    new SnippetTextElement{Text="\n\tdefault:\n"},
                    new SnippetTextElement{Text="\t\t//DefaultProcessing\n"},
                    new SnippetTextElement{Text="\nendcase\n"}
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet for_order()
        {
            //スニペット可動部分宣言
            SnippetReplaceableTextElement loopCounter = new SnippetReplaceableTextElement { Text = "i" };

            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="for( " },
                    new SnippetBoundElement{ TargetElement=loopCounter},
                    new SnippetTextElement{Text=" = " },
                    new SnippetReplaceableTextElement{Text="0" },
                    new SnippetTextElement{Text="; "},
                    loopCounter,
                    new SnippetTextElement{Text = " < "},
                    new SnippetReplaceableTextElement{Text ="end" },
                    new SnippetTextElement{Text="; "},
                    new SnippetBoundElement{ TargetElement=loopCounter },
                    new SnippetTextElement{Text="++ )\n"},
                    new SnippetCaretElement(),
                    new SnippetTextElement{Text="\nnext\n"}
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet while_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="while("},
                    new SnippetReplaceableTextElement{Text="condition"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement(),
                    new SnippetTextElement{Text="\nnext\n"}
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet foreach_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="foreach("},
                    new SnippetReplaceableTextElement{Text="i"},
                    new SnippetTextElement{Text=" in "},
                    new SnippetReplaceableTextElement{Text="source"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement(),
                    new SnippetTextElement{Text="\nnext\n"}
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet excel_open_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="excel_open(\""},
                    new SnippetReplaceableTextElement{Text="ExcelFileName" },
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="false"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet excel_close_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="excel_close(\""},
                    new SnippetReplaceableTextElement{Text="ExcelKey"},
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="true"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet excel_get_cell_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="excel_get_cell(\""},
                    new SnippetReplaceableTextElement{Text="ExcelKey" },
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="SheetName"},
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="Line"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="Column"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet excel_set_cell_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="excel_set_cell(\""},
                    new SnippetReplaceableTextElement{Text="ExcelKey" },
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="SheetName"},
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="Line"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="Column"},
                    new SnippetTextElement{Text=", \""},
                    new SnippetReplaceableTextElement{Text="SetValue"},
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet excel_search_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="excel_search(\""},
                    new SnippetReplaceableTextElement{Text="ExcelKey" },
                    new SnippetTextElement{Text="\", \"" },
                    new SnippetReplaceableTextElement{Text="SearchWord" },
                    new SnippetTextElement{Text="\", \"" },
                    new SnippetReplaceableTextElement{Text="SheetName" },
                    new SnippetTextElement{Text="\")\n" },
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet excel_get_search_result_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="excel_get_search_result(\""},
                    new SnippetReplaceableTextElement{Text="ExcelKey" },
                    new SnippetTextElement{Text="\", " },
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet excel_get_search_count_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="excel_get_search_count(\""},
                    new SnippetReplaceableTextElement{Text="ExcelKey"},
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet excel_add_sheet_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="excel_add_sheet(\""},
                    new SnippetReplaceableTextElement{Text="ExcelKey"},
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="SheetName" },
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet excel_remove_sheet_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="excel_remove_sheet(\""},
                    new SnippetReplaceableTextElement{Text="ExcelKey"},
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="SheetName" },
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet get_download_path_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="get_download_path()\n" },
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet folder_open_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="folder_open(\""},
                    new SnippetReplaceableTextElement{Text="FolderName"},
                    new SnippetTextElement{Text="\", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", " },
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet get_windows_version_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="get_windows_version()\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet get_error_code_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="get_error_code()\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet get_error_message_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="get_error_message()\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet logger_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="logger("},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", \""},
                    new SnippetReplaceableTextElement{Text="Message" },
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="AttachedFile" },
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public Snippet capture_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="capture(\""},
                    new SnippetReplaceableTextElement{Text="FileName"},
                    new SnippetTextElement{Text="\", \""},
                    new SnippetReplaceableTextElement{Text="jpeg" },
                    new SnippetTextElement{Text="\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public SnippetElement date_add_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="date_add("},
                    new SnippetReplaceableTextElement{Text="0"},
                    new SnippetTextElement{Text=", "},
                    new SnippetReplaceableTextElement{Text="0" },
                    new SnippetTextElement{Text=", \""},
                    new SnippetTextElement{Text=DateTime.Now.ToString("yyyy/MM/dd") + "\")\n"},
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public SnippetElement input_box_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="input_box(" },
                    new SnippetReplaceableTextElement{Text="\"Prompt\"" },
                    new SnippetTextElement{Text=")" },
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public SnippetElement get_current_path_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="get_current_path()" },
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        public SnippetElement set_current_path_order()
        {
            //スニペット本体作成
            Snippet snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement{Text="set_current_path(" },
                    new SnippetReplaceableTextElement{Text="\"CurrentPath\"" },
                    new SnippetTextElement{Text=")" },
                    new SnippetCaretElement()
                }
            };

            //作成した内容を反映する
            return snippet;
        }
        #endregion

        #endregion
    }
}
