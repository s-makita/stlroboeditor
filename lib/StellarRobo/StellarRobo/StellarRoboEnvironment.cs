using Accessibility;
using HoldingSearch;
using Microsoft.VisualBasic;
using NPOI.SS.UserModel;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Support.UI;
using SnmpSharpNet;
using StellarLink.Windows.Simulation;
using StellarRobo.Analyze;
//using StellarRobo.Standard;
using StellarRobo.Type;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Xml.Linq;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace StellarRobo
{
    /// <summary>
    /// StellarRoboの実行環境を定義します。
    /// </summary>
    public class StellarRoboEnvironment
    {
        #region 列挙型
        enum ACCESSIBLE_TYPE : int
        {
            AccName,
            AccRole,
            AccState,
            AccValue,
        }

        #endregion

        #region 定数
        private const string FILE_NAME_CAN_NOT_USE = "(\\\\|/|:|\\*|\\?|\"|<|>|\\|)";                    //ファイルで使用出来ない文字
        private const string DEFAULT_ENCODE = "UTF-8";                                              //デフォルト文字コード
        private const string MACHE_ENCODE = "(UniCode|UTF-16|UTF-8|Shift_Jis|euc-jp|iso-2022-jp)";  //Matchさせたい文字コード
        private const int EXCEPTION_ERROR = 10000;
        private const int DOUBLE_CLICK_MARGIN = 10;                                                 //システムに登録されているDoubleClick間隔をそのまま使うと動かないので若干引く
        private const byte SPACE_KEY = 0x20;                                                        //Spaceキー押下エミュレート用
        private const int MAX_TIME = 1000 * 60 * 10;                                                //待ち時間最大値:10分(ミリ秒 × 秒 × 分)
        #endregion

        #region 変数
        static private int ErrorCode = 0;
        static private string ErrorMessage = string.Empty;
        private static Dictionary<string, string> GlobalVariables = new Dictionary<string, string>();
        private Dictionary<string, StellarRoboModule> modules = new Dictionary<string, StellarRoboModule>();
        private static string mine_type = string.Empty;                //Mine/Type一覧用
        #endregion

        #region Win32API

        #region GetCursoPos
        [DllImport("User32.dll")]
        static extern bool GetCursorPos(out POINT lppoint);
        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public int X { get; set; }
            public int Y { get; set; }
            public static implicit operator System.Drawing.Point(POINT point)
            {
                return new System.Drawing.Point(point.X, point.Y);
            }
        }
        #endregion

        #region FindWindow
        [System.Runtime.InteropServices.DllImport("user32.dll",
                CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        #endregion

        #region FindWindowEx
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        #endregion

        #region GetActiveWindow
        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();
        #endregion

        #region WindowFromPoint
        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(System.Drawing.Point p);
        #endregion

        #region GetFocus
        [DllImport("user32.dll")]
        static extern IntPtr GetFocus();
        #endregion

        #region GetClassName
        private const int GET_CLASSNAME_DEFAULT_MAX_COUNT = 256;
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        #endregion

        #region IsWindowVisible
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);
        #endregion

        #region IsWindowEnabled
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowEnabled(IntPtr hWnd);
        #endregion

        #region GetDesktopWindow
        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();
        #endregion

        #region GetWindow
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        private enum GetWindowType : uint
        {
            /// <summary>
            /// The retrieved handle identifies the window of the same type that is highest in the Z order.
            /// <para/>
            /// If the specified window is a topmost window, the handle identifies a topmost window.
            /// If the specified window is a top-level window, the handle identifies a top-level window.
            /// If the specified window is a child window, the handle identifies a sibling window.
            /// </summary>
            GW_HWNDFIRST = 0,
            /// <summary>
            /// The retrieved handle identifies the window of the same type that is lowest in the Z order.
            /// <para />
            /// If the specified window is a topmost window, the handle identifies a topmost window.
            /// If the specified window is a top-level window, the handle identifies a top-level window.
            /// If the specified window is a child window, the handle identifies a sibling window.
            /// </summary>
            GW_HWNDLAST = 1,
            /// <summary>
            /// The retrieved handle identifies the window below the specified window in the Z order.
            /// <para />
            /// If the specified window is a topmost window, the handle identifies a topmost window.
            /// If the specified window is a top-level window, the handle identifies a top-level window.
            /// If the specified window is a child window, the handle identifies a sibling window.
            /// </summary>
            GW_HWNDNEXT = 2,
            /// <summary>
            /// The retrieved handle identifies the window above the specified window in the Z order.
            /// <para />
            /// If the specified window is a topmost window, the handle identifies a topmost window.
            /// If the specified window is a top-level window, the handle identifies a top-level window.
            /// If the specified window is a child window, the handle identifies a sibling window.
            /// </summary>
            GW_HWNDPREV = 3,
            /// <summary>
            /// The retrieved handle identifies the specified window's owner window, if any.
            /// </summary>
            GW_OWNER = 4,
            /// <summary>
            /// The retrieved handle identifies the child window at the top of the Z order,
            /// if the specified window is a parent window; otherwise, the retrieved handle is NULL.
            /// The function examines only child windows of the specified window. It does not examine descendant windows.
            /// </summary>
            GW_CHILD = 5,
            /// <summary>
            /// The retrieved handle identifies the enabled popup window owned by the specified window (the
            /// search uses the first such window found using GW_HWNDNEXT); otherwise, if there are no enabled
            /// popup windows, the retrieved handle is that of the specified window.
            /// </summary>
            GW_ENABLEDPOPUP = 6
        }
        #endregion

        #region GetWindowText
        private const int GET_WINDOW_TEXT_DEFAULT_MAX_COUNT = 256;
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        #endregion

        #region GetWindowRect
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
#pragma warning disable 1591
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

            public int X
            {
                get { return Left; }
                set { Right -= (Left - value); Left = value; }
            }

            public int Y
            {
                get { return Top; }
                set { Bottom -= (Top - value); Top = value; }
            }

            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(Left, Top); }
                set { X = value.X; Y = value.Y; }
            }

            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
                set { Width = value.Width; Height = value.Height; }
            }

            public static implicit operator System.Drawing.Rectangle(RECT r)
            {
                return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator RECT(System.Drawing.Rectangle r)
            {
                return new RECT(r);
            }

            public static bool operator ==(RECT r1, RECT r2)
            {
                return r1.Equals(r2);
            }

            public static bool operator !=(RECT r1, RECT r2)
            {
                return !r1.Equals(r2);
            }

            public bool Equals(RECT r)
            {
                return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
            }

            public override bool Equals(object obj)
            {
                if (obj is RECT)
                    return Equals((RECT)obj);
                else if (obj is System.Drawing.Rectangle)
                    return Equals(new RECT((System.Drawing.Rectangle)obj));
                return false;
            }

            public override int GetHashCode()
            {
                return ((System.Drawing.Rectangle)this).GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
            }
        }
#pragma warning restore 1591
        #endregion

        #region SetWindowPos
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        [Flags()]
        private enum SetWindowPosFlags : uint
        {
            /// <summary>If the calling thread and the thread that owns the window are attached to different input queues, 
            /// the system posts the request to the thread that owns the window. This prevents the calling thread from 
            /// blocking its execution while other threads process the request.</summary>
            /// <remarks>SWP_ASYNCWINDOWPOS</remarks>
            AsynchronousWindowPosition = 0x4000,
            /// <summary>Prevents generation of the WM_SYNCPAINT message.</summary>
            /// <remarks>SWP_DEFERERASE</remarks>
            DeferErase = 0x2000,
            /// <summary>Draws a frame (defined in the window's class description) around the window.</summary>
            /// <remarks>SWP_DRAWFRAME</remarks>
            DrawFrame = 0x0020,
            /// <summary>Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to 
            /// the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE 
            /// is sent only when the window's size is being changed.</summary>
            /// <remarks>SWP_FRAMECHANGED</remarks>
            FrameChanged = 0x0020,
            /// <summary>Hides the window.</summary>
            /// <remarks>SWP_HIDEWINDOW</remarks>
            HideWindow = 0x0080,
            /// <summary>Does not activate the window. If this flag is not set, the window is activated and moved to the 
            /// top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter 
            /// parameter).</summary>
            /// <remarks>SWP_NOACTIVATE</remarks>
            DoNotActivate = 0x0010,
            /// <summary>Discards the entire contents of the client area. If this flag is not specified, the valid 
            /// contents of the client area are saved and copied back into the client area after the window is sized or 
            /// repositioned.</summary>
            /// <remarks>SWP_NOCOPYBITS</remarks>
            DoNotCopyBits = 0x0100,
            /// <summary>Retains the current position (ignores X and Y parameters).</summary>
            /// <remarks>SWP_NOMOVE</remarks>
            IgnoreMove = 0x0002,
            /// <summary>Does not change the owner window's position in the Z order.</summary>
            /// <remarks>SWP_NOOWNERZORDER</remarks>
            DoNotChangeOwnerZOrder = 0x0200,
            /// <summary>Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to 
            /// the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent 
            /// window uncovered as a result of the window being moved. When this flag is set, the application must 
            /// explicitly invalidate or redraw any parts of the window and parent window that need redrawing.</summary>
            /// <remarks>SWP_NOREDRAW</remarks>
            DoNotRedraw = 0x0008,
            /// <summary>Same as the SWP_NOOWNERZORDER flag.</summary>
            /// <remarks>SWP_NOREPOSITION</remarks>
            DoNotReposition = 0x0200,
            /// <summary>Prevents the window from receiving the WM_WINDOWPOSCHANGING message.</summary>
            /// <remarks>SWP_NOSENDCHANGING</remarks>
            DoNotSendChangingEvent = 0x0400,
            /// <summary>Retains the current size (ignores the cx and cy parameters).</summary>
            /// <remarks>SWP_NOSIZE</remarks>
            IgnoreResize = 0x0001,
            /// <summary>Retains the current Z order (ignores the hWndInsertAfter parameter).</summary>
            /// <remarks>SWP_NOZORDER</remarks>
            IgnoreZOrder = 0x0004,
            /// <summary>Displays the window.</summary>
            /// <remarks>SWP_SHOWWINDOW</remarks>
            ShowWindow = 0x0040,
        }
        #endregion

        #region GetWindowLong
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
#pragma warning disable 1591
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private enum GWL
        {
            GWL_WNDPROC = (-4),
            GWL_HINSTANCE = (-6),
            GWL_HWNDPARENT = (-8),
            GWL_STYLE = (-16),
            GWL_EXSTYLE = (-20),
            GWL_USERDATA = (-21),
            GWL_ID = (-12)
        }
#pragma warning restore 1591
        #endregion

        #region SetWindowLong
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        #endregion

        #region ShowWindow
        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
#pragma warning disable 1574
        enum ShowWindowCommands
        {
            /// <summary>
            /// Hides the window and activates another window.
            /// </summary>
            Hide = 0,
            /// <summary>
            /// Activates and displays a window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position.
            /// An application should specify this flag when displaying the window 
            /// for the first time.
            /// </summary>
            Normal = 1,
            /// <summary>
            /// Activates the window and displays it as a minimized window.
            /// </summary>
            ShowMinimized = 2,
            /// <summary>
            /// Maximizes the specified window.
            /// </summary>
            Maximize = 3, // is this the right value?
                          /// <summary>
                          /// Activates the window and displays it as a maximized window.
                          /// </summary>       
            ShowMaximized = 3,
            /// <summary>
            /// Displays a window in its most recent size and position. This value 
            /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except 
            /// the window is not activated.
            /// </summary>
            ShowNoActivate = 4,
            /// <summary>
            /// Activates the window and displays it in its current size and position. 
            /// </summary>
            Show = 5,
            /// <summary>
            /// Minimizes the specified window and activates the next top-level 
            /// window in the Z order.
            /// </summary>
            Minimize = 6,
            /// <summary>
            /// Displays the window as a minimized window. This value is similar to
            /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowMinNoActive = 7,
            /// <summary>
            /// Displays the window in its current size and position. This value is 
            /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowNA = 8,
            /// <summary>
            /// Activates and displays the window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position. 
            /// An application should specify this flag when restoring a minimized window.
            /// </summary>
            Restore = 9,
            /// <summary>
            /// Sets the show state based on the SW_* value specified in the 
            /// STARTUPINFO structure passed to the CreateProcess function by the 
            /// program that started the application.
            /// </summary>
            ShowDefault = 10,
            /// <summary>
            ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread 
            /// that owns the window is not responding. This flag should only be 
            /// used when minimizing windows from a different thread.
            /// </summary>
            ForceMinimize = 11
        }
#pragma warning restore 1574
        #endregion

        #region GetForegroundWindow
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        #endregion

        #region SetForegroundWindow
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        #endregion

        #region GetWindowThreadProcessId
        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        #endregion

        #region AttachThreadInput
        [DllImport("user32.dll")]
        static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
        #endregion

        #region SystemParametersInfo
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, IntPtr pvParam, SPIF fWinIni);

        // For setting a string parameter
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, String pvParam, SPIF fWinIni);

        // For reading a string parameter
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, StringBuilder pvParam, SPIF fWinIni);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, ref ANIMATIONINFO pvParam, SPIF fWinIni);
        private enum SPI
        {
            SPI_GETFOREGROUNDLOCKTIMEOUT = 0x2000,
            SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2001
        }
        [Flags]
        public enum SPIF
        {
            None = 0x00,
            /// <summary>Writes the new system-wide parameter setting to the user profile.</summary>
            SPIF_UPDATEINIFILE = 0x01,
            /// <summary>Broadcasts the WM_SETTINGCHANGE message after updating the user profile.</summary>
            SPIF_SENDCHANGE = 0x02,
            /// <summary>Same as SPIF_SENDCHANGE.</summary>
            SPIF_SENDWININICHANGE = 0x02
        }
        /// <summary>
        /// ANIMATIONINFO specifies animation effects associated with user actions. 
        /// Used with SystemParametersInfo when SPI_GETANIMATION or SPI_SETANIMATION action is specified.
        /// </summary>
        /// <remark>
        /// The uiParam value must be set to (System.UInt32)Marshal.SizeOf(typeof(ANIMATIONINFO)) when using this structure.
        /// </remark>
        [StructLayout(LayoutKind.Sequential)]
        public struct ANIMATIONINFO
        {
            /// <summary>
            /// Creates an AMINMATIONINFO structure.
            /// </summary>
            /// <param name="iMinAnimate">If non-zero and SPI_SETANIMATION is specified, enables minimize/restore animation.</param>
            public ANIMATIONINFO(System.Int32 iMinAnimate)
            {
                this.cbSize = (System.UInt32)Marshal.SizeOf(typeof(ANIMATIONINFO));
                this.iMinAnimate = iMinAnimate;
            }

            /// <summary>
            /// Always must be set to (System.UInt32)Marshal.SizeOf(typeof(ANIMATIONINFO)).
            /// </summary>
            public System.UInt32 cbSize;

            /// <summary>
            /// If non-zero, minimize/restore animation is enabled, otherwise disabled.
            /// </summary>
            public System.Int32 iMinAnimate;
        }
        #endregion

        #region SendMessage
#pragma warning disable 1591
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, StringBuilder lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, ref IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);
        private const int WM_CLOSE = 0x10;
        private const int BM_CLICK = 0x00F5;
        private const UInt32 TCM_FIRST = 0x1300;
        private const UInt32 TCM_SETCURSEL = (TCM_FIRST + 12);
#pragma warning restore 1591
        #endregion

        #region AccessibleObjectFromPoint
#pragma warning disable 1591
        [DllImport("oleacc.dll")]
        private static extern IntPtr AccessibleObjectFromPoint(int x, int y, [Out, MarshalAs(UnmanagedType.Interface)] out IAccessible accObj, [Out] out object ChildID);
        private const int S_OK = unchecked((int)0x00000000);
#pragma warning restore 1591
        #endregion

        #region AccessibleObjectFromWindow
        [DllImport("oleacc.dll")]
        internal static extern int AccessibleObjectFromWindow(
         IntPtr hwnd,
         uint id,
         ref Guid iid,
         [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object ppvObject);
        internal enum OBJID : uint
        {
            WINDOW = 0x00000000,
            SYSMENU = 0xFFFFFFFF,
            TITLEBAR = 0xFFFFFFFE,
            MENU = 0xFFFFFFFD,
            CLIENT = 0xFFFFFFFC,
            VSCROLL = 0xFFFFFFFB,
            HSCROLL = 0xFFFFFFFA,
            SIZEGRIP = 0xFFFFFFF9,
            CARET = 0xFFFFFFF8,
            CURSOR = 0xFFFFFFF7,
            ALERT = 0xFFFFFFF6,
            SOUND = 0xFFFFFFF5,
        }
        internal static Guid IID_IAccessible = new Guid(0x618736e0, 0x3c3d, 0x11cf, 0x81, 0x0c, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
        #endregion

        #region AccessibleChildren
#pragma warning disable 1591
        [DllImport("oleacc.dll")]
        private static extern uint AccessibleChildren(IAccessible paccContainer, int iChildStart, int cChildren, [Out] object[] rgvarChildren, out int pcObtained);
#pragma warning restore 1591
        #endregion

        #region ImmGetContext
        [DllImport("Imm32.dll", SetLastError = true)]
        private static extern IntPtr ImmGetContext(IntPtr hWnd);
        #endregion

        #region ImmSetOpenStatus
        [DllImport("Imm32.dll")]
        private static extern bool ImmSetOpenStatus(IntPtr hIMC, bool flag);
        #endregion

        #region ImmGetOpenStatus
        [DllImport("imm32.dll")]
        private static extern int ImmGetOpenStatus(IntPtr hIMC);
        #endregion

        #region ImmReleaseContext
        [DllImport("Imm32.dll")]
        private static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);
        #endregion

        #region GetDoubleClickTime
        [DllImport("User32.dll")]
        private static extern uint GetDoubleClickTime();
        #endregion

        #region keybd_event
        [DllImport("user32.dll")]
        public static extern uint keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        #endregion

        #region SHGetKnownFolderPath
        [DllImport("shell32.dll")]
        static extern int SHGetKnownFolderPath(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
            uint dwFlags,
            IntPtr hToken,
            out IntPtr pszPath  // API uses CoTaskMemAlloc
        );
        public static class KnownFolder
        {
            // Synchronized with Windows SDK 10.0.16299.0 KnownFolders.h
            public static readonly Guid NetworkFolder = Guid.Parse("D20BEEC4-5CA8-4905-AE3B-BF251EA09B53");
            public static readonly Guid ComputerFolder = Guid.Parse("0AC0837C-BBF8-452A-850D-79D08E667CA7");
            public static readonly Guid InternetFolder = Guid.Parse("4D9F7874-4E0C-4904-967B-40B0D20C3E4B");
            public static readonly Guid ControlPanelFolder = Guid.Parse("82A74AEB-AEB4-465C-A014-D097EE346D63");
            public static readonly Guid PrintersFolder = Guid.Parse("76FC4E2D-D6AD-4519-A663-37BD56068185");
            public static readonly Guid SyncManagerFolder = Guid.Parse("43668BF8-C14E-49B2-97C9-747784D784B7");
            public static readonly Guid SyncSetupFolder = Guid.Parse("0F214138-B1D3-4A90-BBA9-27CBC0C5389A");
            public static readonly Guid ConflictFolder = Guid.Parse("4BFEFB45-347D-4006-A5BE-AC0CB0567192");
            public static readonly Guid SyncResultsFolder = Guid.Parse("289A9A43-BE44-4057-A41B-587A76D7E7F9");
            public static readonly Guid RecycleBinFolder = Guid.Parse("B7534046-3ECB-4C18-BE4E-64CD4CB7D6AC");
            public static readonly Guid ConnectionsFolder = Guid.Parse("6F0CD92B-2E97-45D1-88FF-B0D186B8DEDD");
            public static readonly Guid Fonts = Guid.Parse("FD228CB7-AE11-4AE3-864C-16F3910AB8FE");
            public static readonly Guid Desktop = Guid.Parse("B4BFCC3A-DB2C-424C-B029-7FE99A87C641");
            public static readonly Guid Startup = Guid.Parse("B97D20BB-F46A-4C97-BA10-5E3608430854");
            public static readonly Guid Programs = Guid.Parse("A77F5D77-2E2B-44C3-A6A2-ABA601054A51");
            public static readonly Guid StartMenu = Guid.Parse("625B53C3-AB48-4EC1-BA1F-A1EF4146FC19");
            public static readonly Guid Recent = Guid.Parse("AE50C081-EBD2-438A-8655-8A092E34987A");
            public static readonly Guid SendTo = Guid.Parse("8983036C-27C0-404B-8F08-102D10DCFD74");
            public static readonly Guid Documents = Guid.Parse("FDD39AD0-238F-46AF-ADB4-6C85480369C7");
            public static readonly Guid Favorites = Guid.Parse("1777F761-68AD-4D8A-87BD-30B759FA33DD");
            public static readonly Guid NetHood = Guid.Parse("C5ABBF53-E17F-4121-8900-86626FC2C973");
            public static readonly Guid PrintHood = Guid.Parse("9274BD8D-CFD1-41C3-B35E-B13F55A758F4");
            public static readonly Guid Templates = Guid.Parse("A63293E8-664E-48DB-A079-DF759E0509F7");
            public static readonly Guid CommonStartup = Guid.Parse("82A5EA35-D9CD-47C5-9629-E15D2F714E6E");
            public static readonly Guid CommonPrograms = Guid.Parse("0139D44E-6AFE-49F2-8690-3DAFCAE6FFB8");
            public static readonly Guid CommonStartMenu = Guid.Parse("A4115719-D62E-491D-AA7C-E74B8BE3B067");
            public static readonly Guid PublicDesktop = Guid.Parse("C4AA340D-F20F-4863-AFEF-F87EF2E6BA25");
            public static readonly Guid ProgramData = Guid.Parse("62AB5D82-FDC1-4DC3-A9DD-070D1D495D97");
            public static readonly Guid CommonTemplates = Guid.Parse("B94237E7-57AC-4347-9151-B08C6C32D1F7");
            public static readonly Guid PublicDocuments = Guid.Parse("ED4824AF-DCE4-45A8-81E2-FC7965083634");
            public static readonly Guid RoamingAppData = Guid.Parse("3EB685DB-65F9-4CF6-A03A-E3EF65729F3D");
            public static readonly Guid LocalAppData = Guid.Parse("F1B32785-6FBA-4FCF-9D55-7B8E7F157091");
            public static readonly Guid LocalAppDataLow = Guid.Parse("A520A1A4-1780-4FF6-BD18-167343C5AF16");
            public static readonly Guid InternetCache = Guid.Parse("352481E8-33BE-4251-BA85-6007CAEDCF9D");
            public static readonly Guid Cookies = Guid.Parse("2B0F765D-C0E9-4171-908E-08A611B84FF6");
            public static readonly Guid History = Guid.Parse("D9DC8A3B-B784-432E-A781-5A1130A75963");
            public static readonly Guid System = Guid.Parse("1AC14E77-02E7-4E5D-B744-2EB1AE5198B7");
            public static readonly Guid SystemX86 = Guid.Parse("D65231B0-B2F1-4857-A4CE-A8E7C6EA7D27");
            public static readonly Guid Windows = Guid.Parse("F38BF404-1D43-42F2-9305-67DE0B28FC23");
            public static readonly Guid Profile = Guid.Parse("5E6C858F-0E22-4760-9AFE-EA3317B67173");
            public static readonly Guid Pictures = Guid.Parse("33E28130-4E1E-4676-835A-98395C3BC3BB");
            public static readonly Guid ProgramFilesX86 = Guid.Parse("7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E");
            public static readonly Guid ProgramFilesCommonX86 = Guid.Parse("DE974D24-D9C6-4D3E-BF91-F4455120B917");
            public static readonly Guid ProgramFilesX64 = Guid.Parse("6D809377-6AF0-444B-8957-A3773F02200E");
            public static readonly Guid ProgramFilesCommonX64 = Guid.Parse("6365D5A7-0F0D-45E5-87F6-0DA56B6A4F7D");
            public static readonly Guid ProgramFiles = Guid.Parse("905E63B6-C1BF-494E-B29C-65B732D3D21A");
            public static readonly Guid ProgramFilesCommon = Guid.Parse("F7F1ED05-9F6D-47A2-AAAE-29D317C6F066");
            public static readonly Guid UserProgramFiles = Guid.Parse("5CD7AEE2-2219-4A67-B85D-6C9CE15660CB");
            public static readonly Guid UserProgramFilesCommon = Guid.Parse("BCBD3057-CA5C-4622-B42D-BC56DB0AE516");
            public static readonly Guid AdminTools = Guid.Parse("724EF170-A42D-4FEF-9F26-B60E846FBA4F");
            public static readonly Guid CommonAdminTools = Guid.Parse("D0384E7D-BAC3-4797-8F14-CBA229B392B5");
            public static readonly Guid Music = Guid.Parse("4BD8D571-6D19-48D3-BE97-422220080E43");
            public static readonly Guid Videos = Guid.Parse("18989B1D-99B5-455B-841C-AB7C74E4DDFC");
            public static readonly Guid Ringtones = Guid.Parse("C870044B-F49E-4126-A9C3-B52A1FF411E8");
            public static readonly Guid PublicPictures = Guid.Parse("B6EBFB86-6907-413C-9AF7-4FC2ABF07CC5");
            public static readonly Guid PublicMusic = Guid.Parse("3214FAB5-9757-4298-BB61-92A9DEAA44FF");
            public static readonly Guid PublicVideos = Guid.Parse("2400183A-6185-49FB-A2D8-4A392A602BA3");
            public static readonly Guid PublicRingtones = Guid.Parse("E555AB60-153B-4D17-9F04-A5FE99FC15EC");
            public static readonly Guid ResourceDir = Guid.Parse("8AD10C31-2ADB-4296-A8F7-E4701232C972");
            public static readonly Guid LocalizedResourcesDir = Guid.Parse("2A00375E-224C-49DE-B8D1-440DF7EF3DDC");
            public static readonly Guid CommonOEMLinks = Guid.Parse("C1BAE2D0-10DF-4334-BEDD-7AA20B227A9D");
            public static readonly Guid CDBurning = Guid.Parse("9E52AB10-F80D-49DF-ACB8-4330F5687855");
            public static readonly Guid UserProfiles = Guid.Parse("0762D272-C50A-4BB0-A382-697DCD729B80");
            public static readonly Guid Playlists = Guid.Parse("DE92C1C7-837F-4F69-A3BB-86E631204A23");
            public static readonly Guid SamplePlaylists = Guid.Parse("15CA69B3-30EE-49C1-ACE1-6B5EC372AFB5");
            public static readonly Guid SampleMusic = Guid.Parse("B250C668-F57D-4EE1-A63C-290EE7D1AA1F");
            public static readonly Guid SamplePictures = Guid.Parse("C4900540-2379-4C75-844B-64E6FAF8716B");
            public static readonly Guid SampleVideos = Guid.Parse("859EAD94-2E85-48AD-A71A-0969CB56A6CD");
            public static readonly Guid PhotoAlbums = Guid.Parse("69D2CF90-FC33-4FB7-9A0C-EBB0F0FCB43C");
            public static readonly Guid Public = Guid.Parse("DFDF76A2-C82A-4D63-906A-5644AC457385");
            public static readonly Guid ChangeRemovePrograms = Guid.Parse("DF7266AC-9274-4867-8D55-3BD661DE872D");
            public static readonly Guid AppUpdates = Guid.Parse("A305CE99-F527-492B-8B1A-7E76FA98D6E4");
            public static readonly Guid AddNewPrograms = Guid.Parse("DE61D971-5EBC-4F02-A3A9-6C82895E5C04");
            public static readonly Guid Downloads = Guid.Parse("374DE290-123F-4565-9164-39C4925E467B");
            public static readonly Guid PublicDownloads = Guid.Parse("3D644C9B-1FB8-4F30-9B45-F670235F79C0");
            public static readonly Guid SavedSearches = Guid.Parse("7D1D3A04-DEBB-4115-95CF-2F29DA2920DA");
            public static readonly Guid QuickLaunch = Guid.Parse("52A4F021-7B75-48A9-9F6B-4B87A210BC8F");
            public static readonly Guid Contacts = Guid.Parse("56784854-C6CB-462B-8169-88E350ACB882");
            public static readonly Guid SidebarParts = Guid.Parse("A75D362E-50FC-4FB7-AC2C-A8BEAA314493");
            public static readonly Guid SidebarDefaultParts = Guid.Parse("7B396E54-9EC5-4300-BE0A-2482EBAE1A26");
            public static readonly Guid PublicGameTasks = Guid.Parse("DEBF2536-E1A8-4C59-B6A2-414586476AEA");
            public static readonly Guid GameTasks = Guid.Parse("054FAE61-4DD8-4787-80B6-090220C4B700");
            public static readonly Guid SavedGames = Guid.Parse("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4");
            public static readonly Guid Games = Guid.Parse("CAC52C1A-B53D-4EDC-92D7-6B2E8AC19434");
            public static readonly Guid SEARCH_MAPI = Guid.Parse("98EC0E18-2098-4D44-8644-66979315A281");
            public static readonly Guid SEARCH_CSC = Guid.Parse("EE32E446-31CA-4ABA-814F-A5EBD2FD6D5E");
            public static readonly Guid Links = Guid.Parse("BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968");
            public static readonly Guid UsersFiles = Guid.Parse("F3CE0F7C-4901-4ACC-8648-D5D44B04EF8F");
            public static readonly Guid UsersLibraries = Guid.Parse("A302545D-DEFF-464B-ABE8-61C8648D939B");
            public static readonly Guid SearchHome = Guid.Parse("190337D1-B8CA-4121-A639-6D472D16972A");
            public static readonly Guid OriginalImages = Guid.Parse("2C36C0AA-5812-4B87-BFD0-4CD0DFB19B39");
            public static readonly Guid DocumentsLibrary = Guid.Parse("7B0DB17D-9CD2-4A93-9733-46CC89022E7C");
            public static readonly Guid MusicLibrary = Guid.Parse("2112AB0A-C86A-4FFE-A368-0DE96E47012E");
            public static readonly Guid PicturesLibrary = Guid.Parse("A990AE9F-A03B-4E80-94BC-9912D7504104");
            public static readonly Guid VideosLibrary = Guid.Parse("491E922F-5643-4AF4-A7EB-4E7A138D8174");
            public static readonly Guid RecordedTVLibrary = Guid.Parse("1A6FDBA2-F42D-4358-A798-B74D745926C5");
            public static readonly Guid HomeGroup = Guid.Parse("52528A6B-B9E3-4ADD-B60D-588C2DBA842D");
            public static readonly Guid HomeGroupCurrentUser = Guid.Parse("9B74B6A3-0DFD-4f11-9E78-5F7800F2E772");
            public static readonly Guid DeviceMetadataStore = Guid.Parse("5CE4A5E9-E4EB-479D-B89F-130C02886155");
            public static readonly Guid Libraries = Guid.Parse("1B3EA5DC-B587-4786-B4EF-BD1DC332AEAE");
            public static readonly Guid PublicLibraries = Guid.Parse("48DAF80B-E6CF-4F4E-B800-0E69D84EE384");
            public static readonly Guid UserPinned = Guid.Parse("9E3995AB-1F9C-4F13-B827-48B24B6C7174");
            public static readonly Guid ImplicitAppShortcuts = Guid.Parse("BCB5256F-79F6-4CEE-B725-DC34E402FD46");
            public static readonly Guid AccountPictures = Guid.Parse("008CA0B1-55B4-4C56-B8A8-4DE4B299D3BE");
            public static readonly Guid PublicUserTiles = Guid.Parse("0482AF6C-08F1-4C34-8C90-E17EC98B1E17");
            public static readonly Guid AppsFolder = Guid.Parse("1E87508D-89C2-42F0-8A7E-645A0F50CA58");
            public static readonly Guid StartMenuAllPrograms = Guid.Parse("F26305EF-6948-40B9-B255-81453D09C785");
            public static readonly Guid CommonStartMenuPlaces = Guid.Parse("A440879F-87A0-4F7D-B700-0207B966194A");
            public static readonly Guid ApplicationShortcuts = Guid.Parse("A3918781-E5F2-4890-B3D9-A7E54332328C");
            public static readonly Guid RoamingTiles = Guid.Parse("00BCFC5A-ED94-4e48-96A1-3F6217F21990");
            public static readonly Guid RoamedTileImages = Guid.Parse("AAA8D5A5-F1D6-4259-BAA8-78E7EF60835E");
            public static readonly Guid Screenshots = Guid.Parse("B7BEDE81-DF94-4682-A7D8-57A52620B86F");
            public static readonly Guid CameraRoll = Guid.Parse("AB5FB87B-7CE2-4F83-915D-550846C9537B");
            public static readonly Guid SkyDrive = Guid.Parse("A52BBA46-E9E1-435F-B3D9-28DAA648C0F6");
            public static readonly Guid OneDrive = Guid.Parse("A52BBA46-E9E1-435F-B3D9-28DAA648C0F6");
            public static readonly Guid SkyDriveDocuments = Guid.Parse("24D89E24-2F19-4534-9DDE-6A6671FBB8FE");
            public static readonly Guid SkyDrivePictures = Guid.Parse("339719B5-8C47-4894-94C2-D8F77ADD44A6");
            public static readonly Guid SkyDriveMusic = Guid.Parse("C3F2459E-80D6-45DC-BFEF-1F769F2BE730");
            public static readonly Guid SkyDriveCameraRoll = Guid.Parse("767E6811-49CB-4273-87C2-20F355E1085B");
            public static readonly Guid SearchHistory = Guid.Parse("0D4C3DB6-03A3-462F-A0E6-08924C41B5D4");
            public static readonly Guid SearchTemplates = Guid.Parse("7E636BFE-DFA9-4D5E-B456-D7B39851D8A9");
            public static readonly Guid CameraRollLibrary = Guid.Parse("2B20DF75-1EDA-4039-8097-38798227D5B7");
            public static readonly Guid SavedPictures = Guid.Parse("3B193882-D3AD-4EAB-965A-69829D1FB59F");
            public static readonly Guid SavedPicturesLibrary = Guid.Parse("E25B5812-BE88-4BD9-94B0-29233477B6C3");
            public static readonly Guid RetailDemo = Guid.Parse("12D4C69E-24AD-4923-BE19-31321C43A767");
            public static readonly Guid Device = Guid.Parse("1C2AC1DC-4358-4B6C-9733-AF21156576F0");
            public static readonly Guid DevelopmentFiles = Guid.Parse("DBE8E08E-3053-4BBC-B183-2A7B2B191E59");
            public static readonly Guid Objects3D = Guid.Parse("31C0DD25-9439-4F12-BF41-7FF4EDA38722");
            public static readonly Guid AppCaptures = Guid.Parse("EDC0FE71-98D8-4F4A-B920-C8DC133CB165");
            public static readonly Guid LocalDocuments = Guid.Parse("F42EE2D3-909F-4907-8871-4C22FC0BF756");
            public static readonly Guid LocalPictures = Guid.Parse("0DDD015D-B06C-45D5-8C4C-F59713854639");
            public static readonly Guid LocalVideos = Guid.Parse("35286A68-3C57-41A1-BBB1-0EAE73D76C95");
            public static readonly Guid LocalMusic = Guid.Parse("A0C69A99-21C8-4671-8703-7934162FCF1D");
            public static readonly Guid LocalDownloads = Guid.Parse("7D83EE9B-2244-4E70-B1F5-5393042AF1E4");
            public static readonly Guid RecordedCalls = Guid.Parse("2F8B40C2-83ED-48EE-B383-A1F157EC6F9A");
            public static readonly Guid AllAppMods = Guid.Parse("7AD67899-66AF-43BA-9156-6AAD42E6C596");
            public static readonly Guid CurrentAppMods = Guid.Parse("3DB40B20-2A30-4DBE-917E-771DD21DD099");
            public static readonly Guid AppDataDesktop = Guid.Parse("B2C5E279-7ADD-439F-B28C-C41FE1BBF672");
            public static readonly Guid AppDataDocuments = Guid.Parse("7BE16610-1F7F-44AC-BFF0-83E15F2FFCA1");
            public static readonly Guid AppDataFavorites = Guid.Parse("7CFBEFBC-DE1F-45AA-B843-A542AC536CC9");
            public static readonly Guid AppDataProgramData = Guid.Parse("559D40A3-A036-40FA-AF61-84CB430A4D34");
        }
        #endregion

        #region VerifyVersionInfo
        [DllImport("kernel32.dll")]
        static extern bool VerifyVersionInfo([In] ref OSVERSIONINFOEX lpVersionInfo, uint dwTypeMask, ulong dwlConditionMask);

        [StructLayout(LayoutKind.Sequential)]
        private struct OSVERSIONINFOEX
        {
            public uint dwOSVersionInfoSize;
            public uint dwMajorVersion;
            public uint dwMinorVersion;
            public uint dwBuildNumber;
            public uint dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string szCSDVersion;
            public UInt16 wServicePackMajor;
            public UInt16 wServicePackMinor;
            public UInt16 wSuiteMask;
            public byte wProductType;
            public byte wReserved;
        }
        #endregion

        #region VerSetConditionMask
        private const uint VER_MINORVERSION = 0x0000001; //dwMajorVersion
        private const uint VER_MAJORVERSION = 0x0000002; //dwMinorVersion
        private const uint VER_BUILDNUMBER = 0x0000004; //dwBuildNumber
        private const uint VER_PLATFORMID = 0x0000008; //dwPlatformId
        private const uint VER_SERVICEPACKMINOR = 0x0000010; //wServicePackMajor
        private const uint VER_SERVICEPACKMAJOR = 0x0000020; //wServicePackMinor
        private const uint VER_SUITENAME = 0x0000040; //wSuiteMask
        private const uint VER_PRODUCT_TYPE = 0x0000080; //wProductType

        //現在の値と指定された値が同じでなければならない
        private const byte VER_EQUAL = 1;
        //現在の値が指定された値より大きくなければならない
        private const byte VER_GREATER = 2;
        //現在の値が指定された値より大きいか同じでなければならない
        private const byte VER_GREATER_EQUAL = 3;
        //現在の値が指定された値より小さいくなければならない
        private const byte VER_LESS = 4;
        //現在の値が指定された値より小さいか同じでなければならない
        private const byte VER_LESS_EQUAL = 5;
        //指定されたwSuiteMaskがすべて含まれていななければならない
        private const byte VER_AND = 6;
        //指定されたwSuiteMaskの少なくとも1つが含まれていななければならない
        private const byte VER_OR = 7;

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern ulong VerSetConditionMask(ulong dwlConditionMask, uint dwTypeBitMask, byte dwConditionMask);
        #endregion

        #region GetProcessIdOfThread
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetProcessIdOfThread(IntPtr handle);
        #endregion

        #endregion

        #region エラー関連

        #region 列挙型
        private enum RESULT_CODE : int
        {
            Success = 0,                                            //正常終了
            TimeOut,                                                //タイムアウト
            ArgumentError,                                          //引数の数が正しくありません
            ArgumentIsNumberError,                                  //引数に数字以外が入力されました
            ArgumentIsBoolError,                                    //引数に論理型以外が入力されました
            ArgumentIsDateError,                                    //引数に日付型以外が入力されました
            NotGetHandle,                                           //ハンドルが取得出来ない
            NoSearchTarget,                                         //検索対象無し
            UnexpectedError = 99,                                   //予期しないエラー
            //組み込み関数関連エラー(100)
            BuiltInFunctionVariablInputNotEnteredError = 100,       //グルーバール変数名が入力されていません
            BuiltInFunctionVariableNotRegisteredError,              //グローバル変数未宣言
            BuiltInFunctionImeModeInputNotEnteredError,             //Imeモードが入力されていません
            BuiltInFunctionSearchTextInputNotEnteredError,          //検索対象テキストが入力されていません
            BuiltInFunctionSearchPatternInputNotEnteredError,       //検索パターンが入力されていません
            BuiltInFunctionIgnoreInputNotEnteredError,              //検索時大小文字無視フラグが入力されていません
            BuiltInFunctionImageTypeError,                          //キャプチャの保存形式が正しくありません
            BuiltInFunctionIntervalInputNotEnteredError,            //時間間隔が入力されていません
            BuiltInFunctionIntervalOutOfRangeError,                 //時間間隔は範囲外です
            BuiltInFunctionTimeIntervalInputNotEnteredError,        //加算時間間隔が入力されていません
            BuiltInFunctionTimeIntervalOutOfRangeError,             //加算時間間隔は範囲外です
            BuiltInFunctionReferenceDateInputNotEnteredError,       //基準日が入力されていません
            //ファイル関連エラー(200)
            FileFileInputNotEnteredError = 200,                     //ファイル名が入力されていない
            FileFileExistsError,                                    //入力されたファイルが存在しない
            FileFileNameCanNotUseError,                             //ファイル名に使用出来ない文字が使用されています
            FileEncodeInputNotEnteredError,                         //Encode方式が入力されていません
            FileEncodeCanNotUseError,                               //このEncode方式は使用できません
            FileReNamePathInputNotEnteredError,                     //対象ファイルがあるパスが入力されていません
            FileSourceFileInputNotEnteredError,                     //元ファイル名が入力されていません
            FileSourceFileNotExistsError,                           //元ファイルが存在していません
            FileDestinationFileInputNotEnteredError,                //先ファイル名が入力されていません
            FileDestinationFileExistsError,                         //先ファイルが存在しています
            FileDestinationFileNotExistsError,                      //先ファイルが存在しません
            FileDeleteFileInputNotEnteredError,                     //削除ファイル名が入力されていません
            FileDeleteFileExistsError,                              //削除ファイルが存在しません
            FileSearchPathInputNotEnteredError,                     //検索パスが入力されていません
            FileSearchFileInputNotEnteredError,                     //検索ファイル名が入力されていません
            FileCombinePathInputNotEnteredError,                    //結合パスが入力されていません
            FileCombineFileInputNotEnteredError,                    //結合ファイル名が入力されていません
            FileGetFileNameInputNotEnteredError,                    //ファイルパスが入力されていません
            FileCurrentPathInputNotEnteredError,                    //カレントパスが入力されていません
            //マウス関連エラー(300)
            MouseCoordinateNotEnteredError = 300,                   //マウス座標が入力されていません
            MouseSearchTypeOutOfRangeError,                         //入力されたSearchTypeは範囲外です
            MouseSearchWordInputNotEnteredError,                    //SearchWord未入力
            //入力関連エラー(400)
            InputWaitTimeNotEnteredError = 400,                     //入力間隔時間が入力されていません
            InputWaitTimeOutOutOfRangeError,                        //入力された入力間隔時間は範囲外です
            InputSleepTimeNotEnteredError,                          //待ち時間が入力されていません
            InputSleepTimeOutOutOfRangeError,                       //入力された待ち時間は範囲外です
            InputWindowHandleInputNotEnteredError,                  //Windowハンドルが入力されていません
            InputSearchTypeOutOfRangeError,                         //入力されたSearchTypeは範囲外です
            InputSearchWordInputNotEnteredError,                    //SearchWord未入力
            InputPromptInputNotEnteredError,                        //プロンプト未入力
            //アプリケーション関連エラー(500)
            ApplicationExecutionFileInputNotEnteredError = 500,     //起動アプリが入力されていません
            ApplicationExecutionFileExistsError,                    //起動アプリが存在していません
            ApplicationWindowStyleInputNotEnteredError,             //WindowStyleが入力されていません
            ApplicationExistsWaitInputNotEnteredError,              //アプリ終了待機フラグが入力されていません
            ApplicationExistsWaitTimeInputNotEnteredError,          //アプリ終了待機最大時間が入力されていません
            ApplicationExistsWaitTimeOutOfRangeError,               //入力されたアプリ終了待機最大時間は範囲外です
            ApplicationWindowNameInputNotEnteredError,              //WindowNameが入力されていません
            ApplicationWaitStateInputNotEnteredError,               //待ち状態が入力されていません
            ApplicationTimeOutInputNotEnteredError,                 //タイムアウト時間が入力されていません
            ApplicationTimeOutTimeOutOfRangeError,                  //入力されたタイムアウト時間は範囲外です
            ApplicationWindowHandleInputNotEnteredError,            //Windowハンドルが入力されていません
            ApplicationNotFoundError,                               //対象となるWindowが見つかりません
            ApplicationEnterEitherError,                            //ClassName、WindowNameいずれかが入力されていません
            ApplicationLeftPositionInputNotEnteredError,            //Left座標が入力されていません
            ApplicationTopPositionInputNotEnteredError,             //Top座標が入力されていません
            ApplicationRightPositionInputNotEnteredError,           //Right座標が入力されていません
            ApplicationBottomPositionInputNotEnteredError,          //Bottom座標が入力されていません
            ApplicationHandleInputNotEnteredError,                  //ハンドルが入力されていません
            ApplicationParentHandleInputNotEnteredError,            //親ハンドルが入力されていません
            ApplicationZOrderInputNotEnteredError,                  //Orderが入力されていません
            ApplicationPermeabilityNotEnteredError,                 //透過度が入力されていません
            ApplicationCoordinateNotEntererdError,                  //座標が入力されていません
            //OpenCV関連エラー(600)
            OpenCVComparisonFileInputNotEnteredError = 600,         //対象画像ファイル名が入力されていません
            OpenCVComparisonFileExistsError,                        //対象画像ファイルが存在していません
            OpenCVThresholdInputNotEnteredError,                    //閾値が入力されていません
            OpenCVTimeoutInputNotEnteredError,                      //TimeOut時間が入力されていません
            OpenCVRectangleInputNotEnteredError,                    //矩形指定座標が入力されていません
            OpenCVAreaSpecificationError,                           //矩形指定が正しくありません
            OpenCVGrayScaleInputNotEnteredError,                    //グレースケールフラグが入力されていません
            OpenCVMatchPositionModeInputNotEnteredError,            //結果座標返却方法フラグが入力されていません
            //Accessibleインターフェイス関連エラー(700)
            AccessibleCoordinateNotEntererdError = 700,             //座標が入力されていません
            AccessibleOutOfRangeError,                              //指定された座標は範囲外です
            AccessibleNoTargerError,                                //対象コントロールはCheckBox及び、OptionButtonではありません
            //Browser関連エラー(800)
            BrowserURLInputNotEnteredError = 800,                   //URLが入力されていません
            BrowserNotFoundError,                                   //指定されたBrowserが存在していません
            BrowserKeyInputNotEnteredError,                         //BrowserKeyが入力されていません
            BrowserKeyNotFoundError,                                //このKeyに紐付くBrowserが存在していません
            BrowserTypeOutOfRangeError,                             //入力されたBrowserTypeは範囲外です
            BrowserSearchTypeOutOfRangeError,                       //入力されたSearchTypeは範囲外です
            BrowserWaitTimeOut,                                     //TimeOut時間が入力されていません
            BrowserWaitTimeOutOutOfRangeError,                      //TimeOut時間は範囲外です
            BrowserElementInputNotEnteredError,                     //Browser Element未入力
            BrowserLinkNameInputNotEnteredError,                    //Browser Link名未入力
            BrowserMagnificationInputNotEnteredError,               //表示倍率が入力されていません
            BrowserOwnerHandleInputNotEnteredError,                 //OwnerHandleが入力されていません
            //Excel関連エラー(900)
            ExcelFileNameInputNotEnteredError = 900,                //Excelファイル名が入力されていません
            ExcelFileExistsError,                                   //Excelファイルが存在していません
            ExcelFileCreateError,                                   //Excelファイルが既に存在しています
            ExcelFileExtensionError,                                //Excelファイルに、この拡張子は使用できません
            ExcelWorkBookKeyNotFoundError,                          //このKeyに紐付くExcelが存在していません
            ExcelSheetKeyNotFoundError,                             //Sheet名が入力されていません
            ExcelSheetExistError,                                   //指定されたSheet名は既に存在しています
            ExcelWorkBookKeyInputNotEnteredError,                   //ExcelKeyが入力されていません
            ExcelSheetKeyInputNotEnteredError,                      //Excel SheetKey未入力
            ExcelIndexInputNotEnteredError,                         //Excel SearchResult Index未入力
            ExcelCoordinateLineNotEnteredError,                     //Lineが入力されていません
            ExcelCoordinateColumnNotEntererdError,                  //Columnが入力されていません
            ExcelSearchResultOutOfRangeError,                       //Excel SearchResult Index範囲外
            //Explorer関連エラー(1000)
            ExplorerFolderInputNotEnteredError = 1000,              //起動アプリが入力されていません
            ExplorerFolderExistsError,                              //起動アプリが存在していません
        }
        #endregion

        #region 変数
        private static Dictionary<int, string> ERROR_DATA = new Dictionary<int, string>
        {
            { (int)RESULT_CODE.Success,"正常終了" },
            {(int)RESULT_CODE.TimeOut,"タイムアウト" },
            {(int)RESULT_CODE.ArgumentError,"引数の数が正しくありません" },
            {(int)RESULT_CODE.ArgumentIsNumberError,"引数に数値型以外が入力されました" },
            {(int)RESULT_CODE.ArgumentIsBoolError,"引数に論理型以外が入力されました" },
            {(int)RESULT_CODE.ArgumentIsDateError,"引数に日付型以外が入力されました" },
            {(int)RESULT_CODE.NoSearchTarget,"検索対象無し" },
            {(int)RESULT_CODE.UnexpectedError,"予期しないエラーです。" },
            {(int)RESULT_CODE.BuiltInFunctionVariablInputNotEnteredError,"グルーバール変数名が入力されていません" },
            {(int)RESULT_CODE.BuiltInFunctionVariableNotRegisteredError,"グローバル変数未宣言" },
            {(int)RESULT_CODE.BuiltInFunctionImeModeInputNotEnteredError,"Imeモードが入力されていません" },
            {(int)RESULT_CODE.BuiltInFunctionSearchTextInputNotEnteredError,"検索対象テキストが入力されていません" },
            {(int)RESULT_CODE.BuiltInFunctionSearchPatternInputNotEnteredError,"検索パターンが入力されていません" },
            {(int)RESULT_CODE.BuiltInFunctionIgnoreInputNotEnteredError,"検索時大小文字無視フラグが入力されていません" },
            {(int)RESULT_CODE.BuiltInFunctionImageTypeError,"キャプチャの保存形式が正しくありません" },
            {(int)RESULT_CODE.BuiltInFunctionIntervalInputNotEnteredError,"時間間隔が入力されていません" },
            {(int)RESULT_CODE.BuiltInFunctionIntervalOutOfRangeError,"時間間隔は範囲外です" },
            {(int)RESULT_CODE.BuiltInFunctionTimeIntervalInputNotEnteredError,"加算時間間隔が入力されていません" },
            {(int)RESULT_CODE.BuiltInFunctionTimeIntervalOutOfRangeError,"加算時間間隔は範囲外です" },
            {(int)RESULT_CODE.BuiltInFunctionReferenceDateInputNotEnteredError,"基準日が入力されていません" },
            {(int)RESULT_CODE.FileFileInputNotEnteredError,"ファイル名が入力されていません。" },
            {(int)RESULT_CODE.FileFileExistsError,"入力されたファイルが存在しません。" },
            {(int)RESULT_CODE.FileFileNameCanNotUseError,"ファイル名に使用出来ない文字が使用されています" },
            {(int)RESULT_CODE.FileEncodeInputNotEnteredError,"Encode方式が入力されていません。" },
            {(int)RESULT_CODE.FileEncodeCanNotUseError,"このEncode方式は使用できません" },
            {(int)RESULT_CODE.FileReNamePathInputNotEnteredError,"対象ファイルがあるパスが入力されていません" },
            {(int)RESULT_CODE.FileSourceFileInputNotEnteredError,"元ファイル名が入力されていません" },
            {(int)RESULT_CODE.FileSourceFileNotExistsError,"元ファイルが存在していません" },
            {(int)RESULT_CODE.FileDestinationFileInputNotEnteredError,"先ファイル名が入力されていません" },
            {(int)RESULT_CODE.FileDestinationFileExistsError,"先ファイルが存在しています" },
            {(int)RESULT_CODE.FileDestinationFileNotExistsError,"先ファイルが存在しません"},
            {(int)RESULT_CODE.FileDeleteFileInputNotEnteredError,"削除ファイル名が入力されていません" },
            {(int)RESULT_CODE.FileDeleteFileExistsError,"削除ファイルが存在しません" },
            {(int)RESULT_CODE.FileSearchPathInputNotEnteredError,"検索パスが入力されていません" },
            {(int)RESULT_CODE.FileSearchFileInputNotEnteredError,"検索ファイル名が入力されていません" },
            {(int)RESULT_CODE.FileCombinePathInputNotEnteredError,"結合パスが入力されていません" },
            {(int)RESULT_CODE.FileCombineFileInputNotEnteredError,"結合ファイル名が入力されていません" },
            {(int)RESULT_CODE.FileGetFileNameInputNotEnteredError,"ファイルパスが入力されていません" },
            {(int)RESULT_CODE.FileCurrentPathInputNotEnteredError, "カレントパスが入力されていません" },
            {(int)RESULT_CODE.MouseCoordinateNotEnteredError,"マウス座標が入力されていません" },
            {(int)RESULT_CODE.MouseSearchTypeOutOfRangeError,"入力されたSearchTypeは範囲外です" },
            {(int)RESULT_CODE.MouseSearchWordInputNotEnteredError,"SearchWord未入力" },
            {(int)RESULT_CODE.InputWaitTimeNotEnteredError,"入力間隔時間が入力されていません" },
            {(int)RESULT_CODE.InputWaitTimeOutOutOfRangeError,"入力された入力間隔時間は範囲外です" },
            {(int)RESULT_CODE.InputSleepTimeNotEnteredError,"待ち時間が入力されていません" },
            {(int)RESULT_CODE.InputSleepTimeOutOutOfRangeError,"入力された待ち時間は範囲外です" },
            {(int)RESULT_CODE.InputWindowHandleInputNotEnteredError,"Windowハンドルが入力されていません" },
            {(int)RESULT_CODE.InputSearchTypeOutOfRangeError,"入力されたSearchTypeは範囲外です" },
            {(int)RESULT_CODE.InputSearchWordInputNotEnteredError,"SearchWord未入力" },
            {(int)RESULT_CODE.InputPromptInputNotEnteredError,"プロンプト未入力" },
            {(int)RESULT_CODE.ApplicationExecutionFileInputNotEnteredError,"起動アプリが入力されていません" },
            {(int)RESULT_CODE.ApplicationExecutionFileExistsError,"起動アプリが存在していません" },
            {(int)RESULT_CODE.ApplicationWindowStyleInputNotEnteredError,"WindowStyleが入力されていません" },
            {(int)RESULT_CODE.ApplicationExistsWaitInputNotEnteredError,"アプリ終了待機フラグが入力されていません" },
            {(int)RESULT_CODE.ApplicationExistsWaitTimeInputNotEnteredError,"アプリ終了待機最大時間が入力されていません" },
            {(int)RESULT_CODE.ApplicationExistsWaitTimeOutOfRangeError,"入力されたアプリ終了待機最大時間は範囲外です" },
            {(int)RESULT_CODE.ApplicationWindowNameInputNotEnteredError,"WindowNameが入力されていません" },
            {(int)RESULT_CODE.ApplicationWaitStateInputNotEnteredError,"待ち状態が入力されていません" },
            {(int)RESULT_CODE.ApplicationTimeOutInputNotEnteredError,"タイムアウト時間が入力されていません" },
            {(int)RESULT_CODE.ApplicationTimeOutTimeOutOfRangeError,"入力されたタイムアウト時間は範囲外です" },
            {(int)RESULT_CODE.ApplicationWindowHandleInputNotEnteredError,"Windowハンドルが入力されていません" },
            {(int)RESULT_CODE.ApplicationNotFoundError,"対象となるWindowが見つかりません" },
            {(int)RESULT_CODE.ApplicationEnterEitherError,"ClassName、WindowNameいずれかが入力されていません" },
            {(int)RESULT_CODE.ApplicationLeftPositionInputNotEnteredError,"Left座標が入力されていません" },
            {(int)RESULT_CODE.ApplicationTopPositionInputNotEnteredError,"Top座標が入力されていません" },
            {(int)RESULT_CODE.ApplicationRightPositionInputNotEnteredError,"Right座標が入力されていません" },
            {(int)RESULT_CODE.ApplicationBottomPositionInputNotEnteredError,"Bottom座標が入力されていません" },
            {(int)RESULT_CODE.ApplicationHandleInputNotEnteredError,"ハンドルが入力されていません" },
            {(int)RESULT_CODE.ApplicationParentHandleInputNotEnteredError,"親ハンドルが入力されていません" },
            {(int)RESULT_CODE.ApplicationZOrderInputNotEnteredError,"Z-Orderが入力されていません" },
            {(int)RESULT_CODE.ApplicationPermeabilityNotEnteredError,"透過度が入力されていません" },
            {(int)RESULT_CODE.ApplicationCoordinateNotEntererdError,"座標が入力されていません" },
            {(int)RESULT_CODE.OpenCVComparisonFileInputNotEnteredError,"対象画像ファイル名が入力されていません" },
            {(int)RESULT_CODE.OpenCVComparisonFileExistsError,"対象画像ファイルが存在していません" },
            {(int)RESULT_CODE.OpenCVThresholdInputNotEnteredError,"閾値が入力されていません" },
            {(int)RESULT_CODE.OpenCVTimeoutInputNotEnteredError,"TimeOut時間が入力されていません" },
            {(int)RESULT_CODE.OpenCVRectangleInputNotEnteredError,"矩形指定座標が入力されていません" },
            {(int)RESULT_CODE.OpenCVAreaSpecificationError,"矩形指定が正しくありません" },
            {(int)RESULT_CODE.OpenCVGrayScaleInputNotEnteredError,"グレースケールフラグが入力されていません" },
            {(int)RESULT_CODE.OpenCVMatchPositionModeInputNotEnteredError, "結果座標返却方法フラグが入力されていません" },
            {(int)RESULT_CODE.AccessibleCoordinateNotEntererdError,"座標が入力されていません" },
            {(int)RESULT_CODE.AccessibleOutOfRangeError,"指定された座標は範囲外です" },
            {(int)RESULT_CODE.AccessibleNoTargerError,"対象コントロールはCheckBox及び、OptionButtonではありません" },
            {(int)RESULT_CODE.BrowserURLInputNotEnteredError,"URLが入力されていません" },
            {(int)RESULT_CODE.BrowserNotFoundError,"指定されたBrowserが存在していません" },
            {(int)RESULT_CODE.BrowserKeyInputNotEnteredError,"BrowserKeyが入力されていません" },
            {(int)RESULT_CODE.BrowserKeyNotFoundError,"このKeyに紐付くBrowserが存在していません" },
            {(int)RESULT_CODE.BrowserTypeOutOfRangeError,"入力されたBrowserTypeは範囲外です" },
            {(int)RESULT_CODE.BrowserSearchTypeOutOfRangeError,"入力されたSearchTypeは範囲外です" },
            {(int)RESULT_CODE.BrowserWaitTimeOut,"TimeOut時間が入力されていません" },
            {(int)RESULT_CODE.BrowserWaitTimeOutOutOfRangeError,"TimeOut時間は範囲外です" },
            {(int)RESULT_CODE.BrowserElementInputNotEnteredError,"Elementが入力されていません" },
            {(int)RESULT_CODE.BrowserLinkNameInputNotEnteredError,"Link名が入力されていません" },
            {(int)RESULT_CODE.BrowserMagnificationInputNotEnteredError,"表示倍率が入力されていません" },
            {(int)RESULT_CODE.BrowserOwnerHandleInputNotEnteredError,"OwnerHandleが入力されていません" },
            {(int)RESULT_CODE.ExcelFileNameInputNotEnteredError,"Excelファイル名が入力されていません" },
            {(int)RESULT_CODE.ExcelFileExistsError,"Excelファイルが存在していません" },
            {(int)RESULT_CODE.ExcelFileCreateError,"Excelファイルが既に存在しています" },
            {(int)RESULT_CODE.ExcelFileExtensionError,"Excelファイルに、この拡張子は使用できません" },
            {(int)RESULT_CODE.ExcelWorkBookKeyInputNotEnteredError,"ExcelKeyが入力されていません" },
            {(int)RESULT_CODE.ExcelWorkBookKeyNotFoundError,"このKeyに紐付くExcelが存在していません" },
            {(int)RESULT_CODE.ExcelSheetKeyNotFoundError,"Sheet名が入力されていません" },
            {(int)RESULT_CODE.ExcelSheetExistError,"指定されたSheet名は既に存在しています" },
            {(int)RESULT_CODE.ExcelCoordinateLineNotEnteredError,"Lineが入力されていません" },
            {(int)RESULT_CODE.ExcelCoordinateColumnNotEntererdError,"Columnが入力されていません" },
            {(int)RESULT_CODE.ExplorerFolderInputNotEnteredError,"フォルダが入力されていません" },
            {(int)RESULT_CODE.ExplorerFolderExistsError,"フォルダが存在していません" }

        };
        #endregion

        #endregion

        #region Browser関連

        #region 列挙型
        private enum BrowserType
        {
            InternetExplorer,
            FireFox,
            Chrome,
            End,
        }
        private enum ElementSearchType
        {
            Id,
            Name,
            ClassName,
            LinkText,
            XPath,
            End,
        }
        #endregion

        #region 定数
        private const int DEFAULT_TIME_OUT = 10;
        #endregion

        #region クラス
        //Browser情報用
        private class BrowserInfo
        {
            //DriverServer用
            public DriverService driverService;

            //WebDriver用
            public IWebDriver webDriver;
        }
        #endregion

        #region 変数
        //Browser情報用
        private static Dictionary<string, BrowserInfo> Browser = new Dictionary<string, BrowserInfo>();
        #endregion

        #endregion

        #region UI Automation関連

        #region 列挙型
        private enum ControlSearchType
        {
            Name,
            End,
        }
        #endregion

        #endregion

        #region Excel関連

        #region クラス
        //Excel情報用
        private struct Excel_Data
        {
            public string FileName;                         //Excelファイル名
            public IWorkbook WorkBook;                      //WorkBookオブジェクト
            public Dictionary<string, ISheet> Sheet;        //Sheetオブジェクト
            public List<Tuple<string, int, int>> MatchData; //検索結果

            public bool IsExistsSheetName(string SheetName)
            {
                return this.Sheet.ContainsKey(SheetName);
            }
        };
        #endregion

        #region 変数
        //Excel情報用
        private static Dictionary<string, Excel_Data> Excel = new Dictionary<string, Excel_Data>();
        #endregion

        #endregion

        #region logger関連

        #region 列挙型
        private enum LoggerJudgmentsType
        {
            Days = 0,           //日単位(日)
            Capacity,           //容量単位(MB)
        }
        #endregion

        #endregion
        /// <summary>
        /// このインスタンスで定義されている<see cref="StellarRoboModule"/>を取得します。
        /// </summary>
        public StellarRoboModule this[string name] => modules[name];

        /// <summary>
        /// 利用される<see cref="StellarRoboLexer"/>を取得します。
        /// </summary>
        public StellarRoboLexer Lexer { get; } = new StellarRoboLexer();

        /// <summary>
        /// 利用される<see cref="StellarRoboLexer"/>を取得します。
        /// </summary>
        public StellarRoboParser Parser { get; } = new StellarRoboParser();

        /// <summary>
        /// 利用される<see cref="StellarRoboPrecompiler"/>を取得します。
        /// </summary>
        public StellarRoboPrecompiler Precompiler { get; } = new StellarRoboPrecompiler();

        /// <summary>
        /// 現在の<see cref="StellarRoboModule"/>を取得します。
        /// </summary>
        public StellarRoboModule CurrentModule { get; private set; }

        /// <summary>
        /// 実行された状態を設定する
        /// </summary>
        private static void SetErrorInfo(int argErrorCode)
        {
            //エラー情報を設定する
            ErrorCode = argErrorCode * -1;
            ErrorMessage = ERROR_DATA.ContainsKey(argErrorCode) ? ERROR_DATA[argErrorCode] : string.Empty;
        }

        /// <summary>
        /// モジュールを作成し、<see cref="CurrentModule"/>に設定します。
        /// </summary>
        /// <param name="name">名前</param>
        /// <returns>作成されたモジュール</returns>
        public StellarRoboModule CreateModule(string name)
        {
            //変数初期化
            Excel.Clear();
            Browser.Clear();

            var result = new StellarRoboModule(name);
            result.Environment = this;
            modules[name] = result;
            CurrentModule = result;

            result.RegisterFunction(CreateArray, "array");
            result.RegisterFunction(ReadLine, "readln");
            result.RegisterFunction(WriteLine, "println");
            result.RegisterFunction(Write, "print");
            result.RegisterFunction(Format, "format");
            result.RegisterFunction(Exit, "exit");
            result.RegisterFunction(Throw, "throw");

            //RPAに必要な関数
            #region 組み込み関数関連
            result.RegisterFunction(NowDate, "now");                                    //現在の日時を取得
            result.RegisterFunction(GlobalVariable, "global_variable");                 //グローバル変数
            result.RegisterFunction(ReadXml, "read_xml");                               //XMLデータ読み込み
            //result.RegisterFunction(Include, "include");                                //インクルード
            result.RegisterFunction(SetImeStatus, "set_ime_status");                    //IMEの状態を設定する
            result.RegisterFunction(GetImeStatus, "get_ime_status");                    //IMEの状態を取得する
            result.RegisterFunction(CheckData, "check_data");                           //指定文字列を正規表現で検索し、ヒットした文字列を取得する
            result.RegisterFunction(GetRegExCount, "get_reg_ex_count");                 //指定文字列を正規表現で検索し、ヒットした件数を取得する
            result.RegisterFunction(ToInt, "to_int");                                   //数字から数値に変換
            result.RegisterFunction(ToStr, "to_str");                                   //数値から数字に変換
            result.RegisterFunction(MsgBox, "message_box");                             //メッセージボックスを表示する
            result.RegisterFunction(GetErrorCode, "get_error_code");                    //直前の実行結果(コード)を取得する
            result.RegisterFunction(GetErrorMessage, "get_error_message");              //直前の実行結果(メッセージ)を取得する
            result.RegisterFunction(GetDownLoadPath, "get_download_path");              //DownLoadパスを取得する
            result.RegisterFunction(GetFileName, "get_file_name");                      //ファイルパスより、ファイル名を取得する
            result.RegisterFunction(CombinePath, "combine_path");                       //パスとファイル名を結合する
            result.RegisterFunction(CheckDate, "check_date");                           //日付のチェックを行う
            result.RegisterFunction(GetWindowsVersion, "get_windows_version");          //現在実行中のWindowsのバージョンを返す
            result.RegisterFunction(Logger, "logger");                                  //ログメッセージを取得する
            result.RegisterFunction(ImageCapture, "capture");                           //画面のキャプチャを取得する
            result.RegisterFunction(DateAdd, "date_add");                               //日付の時間間隔の加算を行う
            #endregion

            #region ファイル関連
            result.RegisterFunction(InitFile, "init_file");                             //ファイル初期化
            result.RegisterFunction(WriteFile, "write_file");                           //ファイル書き出し
            result.RegisterFunction(ReadFile, "read_file");                             //ファイル読み込み
            result.RegisterFunction(FileCopy, "file_copy");                             //ファイルコピー
            result.RegisterFunction(FileMove, "file_move");                             //ファイルムーブ
            result.RegisterFunction(FileReName, "file_rename");                         //ファイルリネーム
            result.RegisterFunction(FileDelete, "file_delete");                         //ファイルデリート
            result.RegisterFunction(FileExists, "file_exists");                         //ファイル存在確認
            result.RegisterFunction(DirectoryExists, "directory_exists");               //ディレクトリ存在確認
            result.RegisterFunction(SearchFile, "search_file");                         //ファイル検索
            result.RegisterFunction(GetCurrentPath, "get_current_path");                //カレントパス取得
            result.RegisterFunction(SetCurrentPath, "set_current_path");                //カレントパス設定
            #endregion

            #region マウス関連
            result.RegisterFunction(MouseMove, "move");                                 //マウスカーソル移動
            result.RegisterFunction(LeftDown, "left_down");                             //左ボタンDown
            result.RegisterFunction(LeftUp, "left_up");                                 //左ボタンUp
            result.RegisterFunction(LeftClick, "left_click");                           //左ボタンClick
            result.RegisterFunction(LeftDoubleClick, "left_double_click");              //左ボタンDoubleClick
            result.RegisterFunction(RightDown, "right_down");                           //右ボタンDown
            result.RegisterFunction(RightUp, "right_up");                               //右ボタンUp
            result.RegisterFunction(RightClick, "right_click");                         //右ボタンClick
            result.RegisterFunction(RightDoubleClick, "right_double_click");            //右ボタンDoubleClick
            result.RegisterFunction(MiddleDown, "middle_down");                         //中ボタンDown
            result.RegisterFunction(MiddleUp, "middle_up");                             //中ボタンUp
            result.RegisterFunction(MiddleClick, "middle_click");                       //中ボタンClick
            result.RegisterFunction(MiddleDoubleClick, "middle_double_click");          //中ボタンDoubleClick
            result.RegisterFunction(Click, "click");                                    //Click
            #endregion

            #region 入力関連
            result.RegisterFunction(InputKeys, "input_keys");                           //文字列入力
            result.RegisterFunction(InputTextBox, "input_text_box");                    //TextBox入力
            result.RegisterFunction(OutputTextBox, "output_text_box");                  //TextBox出力
            result.RegisterFunction(SendKeys, "send_keys");                             //キー送信
            result.RegisterFunction(Wait, "wait");                                      //待つ
            result.RegisterFunction(InputBox, "input_box");                             //文字列入力
            #endregion

            #region アプリケーション関連
            result.RegisterFunction(AppOpen, "app_open");                               //指定アプリケーション起動
            result.RegisterFunction(AppClose, "app_close");                             //指定アプリケーション終了
            result.RegisterFunction(AppActive, "app_active");                           //指定アプリケーションアクティブ化
            result.RegisterFunction(AppWait, "app_wait");                               //指定アプリケーション起動・未起動待ち
            //result.RegisterFunction(AppWindowEnable, "app_window_enable");              //指定ハンドルのウィンドが使用可能かを
            result.RegisterFunction(AppSetPos, "app_set_pos");                          //指定アプリケーション位置変更
            result.RegisterFunction(GetWindowHandle, "get_window_handle");              //指定ウィンドのハンドル取得
            result.RegisterFunction(SetWindowPosZ, "set_window_pos_z");                 //指定ウィンドのZ-Order設定
            result.RegisterFunction(GetWindowText, "get_window_text");                  //指定ウィンドのタイトル文字を取得
            result.RegisterFunction(GetWindowHandlePoint, "get_window_handle_point");   //現在のマウス座標のハンドルを取得する
            result.RegisterFunction(GetWindowHandleParent, "get_window_handle_parent"); //指定ウィンドのOwnerハンドルを取得
            result.RegisterFunction(GetParentTitle, "get_parent_title");                //指定座標より、ハンドルを取得しウィンドウタイトルを取得する
            #endregion

            #region 判定関連
            #endregion

            #region OpenCV関連
            result.RegisterFunction(ImageMatching, "image_match");                      //現在のトップ画面より指定の画像とマッチする場所の中心座標を取得する
            #endregion

            #region Accessibleインターフェイス関連
            result.RegisterFunction(GetAccName, "get_acc_name");                        //指定座標のActiveAccessibility情報(Name)を取得
            result.RegisterFunction(GetAccRole, "get_acc_role");                        //指定座標のActiveAccessibility情報(Role)を取得
            result.RegisterFunction(GetAccValue, "get_acc_value");                      //指定座標のActiveAccessibility情報(Value)を取得
            result.RegisterFunction(GetAccIsChecked, "get_acc_is_checked");             //指定座標のActiveAccessibility情報(チェックボックス/ラジオボタン)のチェック状態を判定
            #endregion

            #region Browser関連
            result.RegisterFunction(BrowserNavigate, "browser_navigate");                   //指定URLのページに移動する
            result.RegisterFunction(BrowserOpenUrl, "browser_open_url");                    //指定URLへ移動する
            result.RegisterFunction(BrowserWait, "browser_wait");                           //ブラウザの処理終了を待つ
            result.RegisterFunction(BrowserInput, "browser_input");                         //ID(NAME)付きフィールドへ入力を行う
            result.RegisterFunction(BrowserOutput, "browser_output");                       //ID(NAME)付きフィールドから出力を行う
            result.RegisterFunction(BrowserClose, "browser_close");                         //Browserのタブを閉じる
            result.RegisterFunction(BrowserQuit, "browser_quit");                           //Browserを閉じる
            result.RegisterFunction(BrowserClick, "browser_click");                         //ID(NAME)付きフィールドへのクリックを行う
            result.RegisterFunction(BrowserCheckID, "browser_check_id");                    //IDが存在するかチェックする
            result.RegisterFunction(BrowserSelectBox, "browser_select_box");                //BrowserのComboBoxを選択する
            result.RegisterFunction(BrowserList, "browser_list");                           //親Browserに紐付く子Browserを取得する
            result.RegisterFunction(BrowserChange, "browser_change");                       //Browserの制御を変更する
            result.RegisterFunction(BrowserOwner, "browser_owner");                         //親Browserを返す
            result.RegisterFunction(BrowserGetCoordinate, "browser_get_coordinate");        //指定ID(NAME)月フィールドの座標(x, y, width, height)を取得する
            #endregion

            #region Excel関連
            result.RegisterFunction(ExcelOpen, "excel_open");                           //Excelファイルを開く
            result.RegisterFunction(ExcelClose, "excel_close");                         //Excelファイルを閉じる
            result.RegisterFunction(ExcelGetCell, "excel_get_cell");                    //ExcelCell取得する
            result.RegisterFunction(ExcelSetCell, "excel_set_cell");                    //ExcelCell取得する
            result.RegisterFunction(ExcelSearch, "excel_search");                       //ExcelSheet検索
            result.RegisterFunction(ExcelGetSearchResult, "excel_get_search_result");   //ExcelSheet検索結果取得
            result.RegisterFunction(ExcelGetSearchCount, "excel_get_search_count");     //ExcelSheet検索結果件数
            result.RegisterFunction(ExcelAddSheet, "excel_add_sheet");                  //ExcelSheet追加
            result.RegisterFunction(ExcelRemoveSheet, "excel_remove_sheet");            //ExcelSheet削除
            #endregion

            #region Explorer関連
            result.RegisterFunction(FolderOpen, "folder_open");                         //Folder指定でExplorerを開く
            #endregion

            /*
             * 実装予定
             * SetWindowState()
             * SetWindowSize()
             * GetWindowHandle()
             * GetChildWindowHandle()
             * GetWindowHandleActive()
             * GetWindowHandlePoint()
             * GetWindowHandleParent()
             * GetWindowHandleTop()
             * GetWindowHandleFocus()
             * IsWindowVisible()
             * GetWindowClass()
             * GetWindowRect()
             * SetWindowTrans()
             * SetWindowTransColor()
             * GetCaretPos()
             * 
             * GetAccValue()
             * GetAccState()
             * GetAccFocus()
             * GetAccParentName()
             * GetAccParentRole()
             * GetAccChildCount()
             * GetAccDescription()
             * GetAccLocation()
             * GetAccDefaultAction()
             * DoAccDefaultAction()
             */
            return result;
        }

        #region 組み込み関数

        #region 元からあるの関数
        private static StellarRoboFunctionResult CreateArray(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            if (args.Length == 0) throw new ArgumentException("次元数が不正です");
            if (args.Length >= 5) throw new ArgumentException("次元数が多すぎます");
            var dq = args.Select(p => (int)p.ToInt64()).ToArray();
            var result = new StellarRoboArray(dq);
            return result.NoResume();
        }

        private static StellarRoboFunctionResult WriteLine(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine(args[0].ToString());
            }

            return StellarRoboNil.Instance.NoResume();
        }

        private static StellarRoboFunctionResult ReadLine(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args) => Console.ReadLine().AsStellarRoboString().NoResume();

        private static StellarRoboFunctionResult Format(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            var b = args[0].ToString();
            var ar = args.Skip(1).Select(p => p.ToString()).ToArray();
            return string.Format(b, ar).AsStellarRoboString().NoResume();
        }

        private static StellarRoboFunctionResult Write(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            Console.Write(args[0]);
            return StellarRoboNil.Instance.NoResume();
        }

        private static StellarRoboFunctionResult Exit(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            Environment.Exit(args.Length > 0 ? (int)args[0].ToInt64() : 0);
            return StellarRoboNil.Instance.NoResume();
        }

        private static StellarRoboFunctionResult Throw(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            var d = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            switch (args[0].ExtraType)
            {
                case "String":
                    Console.WriteLine($"StellarRoboで例外がスローされました : {args[0].ToString()}");
                    break;
                default:
                    var ex = args[0] as StellarRoboInstance;
                    if (ex == null) throw new ArgumentException("StellarRobo上で生成したインスタンス以外渡せません");
                    var mes = ex["message"].ToString();
                    Console.WriteLine($"{ex.Class.Name}: {mes}");
                    break;

            }
            Console.Write("Enterで終了します...");
            Console.ReadLine();
            Environment.Exit(-1);
            return StellarRoboNil.Instance.NoResume();
        }
        #endregion

        #region RPA関数

        #region 共通(内部で使用)
        private static string CreateObjectKey()
        {
            //変数宣言
            string KeyData = string.Empty;

            //現時点での日付を利用しDictionaryで使用するキーを作成する
            SHA1 sha1 = SHA1.Create();
            byte[] data = sha1.ComputeHash(Encoding.UTF8.GetBytes(DateTime.Now.ToString("yyyyMMddHHmmssfff")));
            for(int i = 0; i < data.Length; i++)
            {
                KeyData += data[i].ToString("x2").ToLower();
            }

            //戻り値設定
            return KeyData;
        }

        private static BrowserInfo CreateBrowser(BrowserType browserType)
        {
            //変数宣言
            DriverService driverService = null;
            IWebDriver webDriver = null;
            BrowserInfo browserInfo = new BrowserInfo();

            //Browserを選択
            switch (browserType)
            {
                //InternetExcplorer
                case BrowserType.InternetExplorer:
                    //InternetExplorerはインストールされているか？
                    if (existsInternetExplorer())
                    {
                        //BrowserUpDate
                        new DriverManager().SetUpDriver(new InternetExplorerConfig());

                        //DriverService生成
                        driverService = InternetExplorerDriverService.CreateDefaultService();

                        //コンソール画面を非表示にする
                        driverService.HideCommandPromptWindow = true;

                        //Browser生成
                        webDriver = new InternetExplorerDriver((InternetExplorerDriverService)driverService);
                    }
                    break;
                //FireFox
                case BrowserType.FireFox:
                    //FireFoxはインストールされているか？
                    if (existsFireFox())
                    {
                        //BrowserUpDate
                        new DriverManager().SetUpDriver(new FirefoxConfig());

                        //DriverService生成
                        driverService = FirefoxDriverService.CreateDefaultService();

                        //コンソール画面を非表示にする
                        driverService.HideCommandPromptWindow = true;
                        ((FirefoxDriverService)driverService).ConnectToRunningBrowser = false;

                        //ダウンロード時、ダイアログを非表示にする
                        FirefoxOptions firefoxOptions = new FirefoxOptions();
                        FirefoxProfile firefoxProfile = new FirefoxProfile();
                        firefoxProfile.SetPreference("browser.download.folderList", 1);
                        firefoxProfile.SetPreference("browser.download.useDownloadDir", true);
                        firefoxProfile.SetPreference("browser.helperApps.neverAsk.openFile", mine_type);
                        firefoxProfile.SetPreference("browser.helperApps.neverAsk.saveToDisk", mine_type);  //Mine/Typeを指定
                        firefoxOptions.Profile = firefoxProfile;

                        //Browser生成
                        webDriver = new FirefoxDriver((FirefoxDriverService)driverService, firefoxOptions);
                    }
                    break;
                //Chrome
                case BrowserType.Chrome:
                    //Chromeはインストールされているか？
                    if (existsChrome())
                    {
                        //BrowserUpDate
                        new DriverManager().SetUpDriver(new ChromeConfig());

                        //DriverService生成
                        driverService = ChromeDriverService.CreateDefaultService();

                        //コンソール画面を非表示にする
                        driverService.HideCommandPromptWindow = true;

                        //Browser生成
                        webDriver = new ChromeDriver((ChromeDriverService)driverService);
                    }
                    break;
            }

            //戻り値設定
            browserInfo.driverService = driverService;
            browserInfo.webDriver = webDriver;
            return browserInfo;
        }
        private static BrowserType GetDefaultBrowser()
        {
            //変数宣言
            BrowserType browserType = BrowserType.InternetExplorer;
            string subKey = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice";

            //Registryに登録されているDefaultBrowserを取得する
            using (Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(subKey, false))
            {
                //取得出来たか？ ※取得出来なかった場合にはデフォルトで入っているInternet Explorerを返す
                if(regKey != null)
                {
                    //DefaultBrowser取得
                    string defaultBrowser = (string)regKey.GetValue("ProgId");
                    switch (defaultBrowser)
                    {
                        //InterneExplorer
                        case string key when Regex.IsMatch(key, "^IE", RegexOptions.IgnoreCase):
                            browserType = BrowserType.InternetExplorer;
                            break;
                        //FireFox
                        case string key when Regex.IsMatch(key, "^Firefox", RegexOptions.IgnoreCase):
                            browserType = BrowserType.FireFox;
                            break;
                        //Chrome
                        case string key when Regex.IsMatch(key, "^Chrome", RegexOptions.IgnoreCase):
                            browserType = BrowserType.Chrome;
                            break;
                        default:
                            browserType = BrowserType.InternetExplorer;
                            break;
                    }
                }
            }

            //戻り値設定
            return browserType;
        }
        private static IReadOnlyCollection<IWebElement> SearchElement(IWebDriver webDriver, int SearchType, string SearchElement)
        {
            //変数宣言
            IReadOnlyCollection<IWebElement> webElements = null;
            List<IWebElement> frameInfo = new List<IWebElement>();
            By by = null;

            //Frame検索
            #region Frame検索
            List<IWebElement> SearchFrame(By byFrame)
            {
                //変数宣言
                IReadOnlyCollection<IWebElement> frames;
                List<IWebElement> frameName = new List<IWebElement>();

                //frameを検索
                frames = webDriver.FindElements(byFrame);

                //取得出来たか？
                if(frames.Count>0)
                {
                    //Nameの値を取得する
                    frameName.AddRange(frames.ToList<IWebElement>());
                }

                //戻り値設定
                return frameName;
            }
            #endregion

            //Frame情報を取得する
            frameInfo.AddRange(SearchFrame(By.TagName("frame")));
            frameInfo.AddRange(SearchFrame(By.TagName("iframe")));

            //検索タイプで検索方法を分岐
            #region 検索タイプで検索方法を分岐
            switch (SearchType)
            {
                //ID属性
                case (int)ElementSearchType.Id:
                    by = By.Id(SearchElement);
                    break;
                //Name属性
                case (int)ElementSearchType.Name:
                    by = By.Name(SearchElement);
                    break;
                //Class属性
                case (int)ElementSearchType.ClassName:
                    by = By.ClassName(SearchElement);
                    break;
                //LinkText
                case (int)ElementSearchType.LinkText:
                    by = By.LinkText(SearchElement);
                    break;
                //XPath
                case (int)ElementSearchType.XPath:
                    by = By.XPath(SearchElement);
                    break;
            }
            #endregion


            //Frameはあるか？
            if (frameInfo.Count > 0)
            {
                //Frame毎に処理を行う
                foreach(IWebElement frameName in frameInfo)
                {
                    //Frameに移動
                    webDriver.SwitchTo().Frame(frameName);

                    //検索
                    webElements = webDriver.FindElements(by);

                    //親に戻る
                    webDriver.SwitchTo().DefaultContent();

                    //取得出来たか？
                    if (webElements.Count > 0)
                    {
                        //処理を抜ける
                        break;
                    }
                }
            }

            //Frame内で既に見つかっていなければ、親部分で検索を行う
            if (webElements == null || webElements.Count == 0)
            {
                //検索
                webElements = webDriver.FindElements(by);
            }

            //戻り値設定
            return webElements;
        }
        private static AutomationElementCollection SearchControl(IntPtr Handle, int SearchType, string SearchControlName)
        {
            //変数宣言
            AutomationElementCollection elementCollection = null;
            AutomationElement ElementHandle = null;
            uint ProcessID = 0;

            //対象のAutomationElementを取得する
            GetWindowThreadProcessId(Handle, out ProcessID);

            //uint ProcessID = GetProcessIdOfThread(Handle);
            ElementHandle = AutomationElement.FromHandle(Handle);

            //検索タイプで検索方法を分岐
            #region 検索タイプで検索方法を分岐
            switch (SearchType)
            {
                //Name属性
                case (int)ControlSearchType.Name:
                    elementCollection = ElementHandle.FindAll(TreeScope.Subtree, new PropertyCondition(AutomationElement.AutomationIdProperty, SearchControlName));
                    break;
            }
            #endregion

            //戻り値設定
            return elementCollection;
        }
        #endregion

        #region 組み込み関数関連
        /// <summary>
        /// 外部からの値のやり取り用
        /// </summary>
        /// <param name="VariableName"></param>
        /// <param name="VariableValue"></param>
        public void GlobalVariable(string VariableName,string VariableValue)
        {
            //キーが空ならば、何もせずに抜ける
            if (VariableName.Trim() == string.Empty)
            {
                return;
            }

            //値を保存、既に登録されているのならば値は上書きされる
            GlobalVariables[VariableName] = VariableValue;
        }
        /// <summary>
        /// 外部からの値のやり取り用(Mine/Type)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="self"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public void mineType(string mineType)
        {
            mine_type = mineType;
        }

        private static StellarRoboFunctionResult GlobalVariable(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string VariableName = string.Empty;
            string VariableValue = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 1)
            {
                //変数名を取得
                VariableName = args[0].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            #region 入力チェック
            //グローバル変数名は入力されているか？
            if (string.IsNullOrEmpty(VariableName))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.BuiltInFunctionVariableNotRegisteredError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            #endregion

            try
            {
                //グローバル変数は登録されているか？
                if ((GlobalVariables.Count() != 0) && GlobalVariables.ContainsKey(VariableName))
                {
                    //登録されている値を取得
                    VariableValue = GlobalVariables[VariableName];
                }
                else
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.BuiltInFunctionVariableNotRegisteredError);
                    return string.Empty.AsStellarRoboString().NoResume();
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return VariableValue.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult Include(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //戻り値設定
            return StellarRoboNil.Instance.NoResume();
        }
        private static StellarRoboFunctionResult SetImeStatus(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            IntPtr GetFocusHandle = IntPtr.Zero;
            IntPtr ImeHandle = IntPtr.Zero;
            string ImeModeTmp = string.Empty;
            bool ImeMode = false;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if(args.Length==1)
            {
                //ImeModeを取得
                ImeModeTmp = args[0].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //ImeModeは入力されているか？
            if (string.IsNullOrEmpty(ImeModeTmp))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.BuiltInFunctionImeModeInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            int tmp = 0;
            if(!int.TryParse(ImeModeTmp,out tmp))
            {
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            ImeMode = (tmp == 0) ? false : true;
            #endregion

            try
            {
                //現在フォーカスが当たってるコントロールのハンドルを取得する
                if ((GetFocusHandle = GetFocus()) == IntPtr.Zero)
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.NotGetHandle);
                    //エラー値設定
                    return false.AsStellarRoboBoolean().NoResume();
                }

                //Imeのハンドルを取得する
                if ((ImeHandle = ImmGetContext(GetFocusHandle)) == IntPtr.Zero)
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.NotGetHandle);
                    //エラー値設定
                    return false.AsStellarRoboBoolean().NoResume();
                }

                //Imeモードを設定
                ImmSetOpenStatus(ImeHandle, ImeMode);

                //Ime開放
                ImmReleaseContext(GetFocusHandle, ImeHandle);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult GetImeStatus(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            IntPtr GetFocusHandle = IntPtr.Zero;
            IntPtr ImeHandle = IntPtr.Zero;
            int ImeMode = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length != 0)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return 0.AsStellarRoboInteger().NoResume();
            }

            try
            {
                //現在フォーカスが当たってるコントロールのハンドルを取得する
                if ((GetFocusHandle = GetFocus()) == IntPtr.Zero)
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.NotGetHandle);
                    //エラー値設定
                    return 0.AsStellarRoboInteger().NoResume();
                }

                //Imeのハンドルを取得する
                if ((ImeHandle = ImmGetContext(GetFocusHandle)) == IntPtr.Zero)
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.NotGetHandle);
                    //エラー値設定
                    return 0.AsStellarRoboInteger().NoResume();
                }

                //Imeモードを取得
                ImeMode = ImmGetOpenStatus(ImeHandle);

                //Ime開放
                ImmReleaseContext(GetFocusHandle, ImeHandle);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return 0.AsStellarRoboInteger().NoResume();
            }

            //戻り値設定
            return ImeMode.AsStellarRoboInteger().NoResume();
        }
        private static StellarRoboFunctionResult CheckData(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string source = string.Empty;
            string pattern = string.Empty;
            Match match;
            string result = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 2)
            {
                //対象テキスト取得
                source = args[0].ToString().Trim();

                //検索パターン取得
                pattern = args[1].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            #region 入力チェック
            //対象テキストは入力されているか？
            if (string.IsNullOrEmpty(source))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.BuiltInFunctionSearchTextInputNotEnteredError);
                return 0.AsStellarRoboInteger().NoResume();
            }
            //検索パターンは入力されているか？
            if (string.IsNullOrEmpty(pattern))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.BuiltInFunctionSearchPatternInputNotEnteredError);
                return 0.AsStellarRoboInteger().NoResume();
            }
            #endregion

            try
            {
                //正規表現で、文字列の検索を行う
                if ((match = Regex.Match(source, pattern)).Success)
                {
                    result = match.Value;
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return result.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult GetRegExCount(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string source = string.Empty;
            string pattern = string.Empty;
            string IgnoreTmp = string.Empty;
            bool Ignore = false;
            Regex regex;
            MatchCollection match;
            int matchCount = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if ((args.Length == 2) || (args.Length == 3))
            {
                //対象テキスト取得
                source = args[0].ToString().Trim();

                //検索パターン取得
                pattern = args[1].ToString().Trim();

                //大小文字無視か？
                if (args.Length == 3)
                {
                    //大小文字無視か？
                    IgnoreTmp = args[2].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return 0.AsStellarRoboInteger().NoResume();
            }

            #region 入力チェック
            //対象テキストは入力されているか？
            if (string.IsNullOrEmpty(source))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.BuiltInFunctionSearchTextInputNotEnteredError);
                return 0.AsStellarRoboInteger().NoResume();
            }
            //検索パターンは入力されているか？
            if (string.IsNullOrEmpty(pattern))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.BuiltInFunctionSearchPatternInputNotEnteredError);
                return 0.AsStellarRoboInteger().NoResume();
            }
            if (args.Length == 3)
            {
                //大小文字無視は入力されているか？
                if (string.IsNullOrEmpty(IgnoreTmp))
                {
                    SetErrorInfo((int)RESULT_CODE.BuiltInFunctionIgnoreInputNotEnteredError);
                    return 0.AsStellarRoboInteger().NoResume();
                }
                int tmp = 0;
                if (!int.TryParse(IgnoreTmp, out tmp))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return 0.AsStellarRoboInteger().NoResume();
                }
                Ignore = (tmp == 0) ? true : false;
            }
            #endregion

            try
            {
                //正規表現で、文字列の検索を行い
                //その件数を結果として返す
                regex = new Regex(pattern, Ignore ? RegexOptions.IgnoreCase : RegexOptions.None);
                matchCount = (match = regex.Matches(source)).Count;
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return 0.AsStellarRoboInteger().NoResume();
            }

            //戻り値設定
            return matchCount.AsStellarRoboInteger().NoResume();
        }
        private static StellarRoboFunctionResult ToInt(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string value = string.Empty;
            int result = 0;

            //引数チェック
            if (args.Length == 1)
            {
                //数字取得
                try
                {
                    value = args[0].ToString().Trim();
                }
                catch
                {
                    value = "0";
                }
            }
            else
            {
                //エラー値設定
                return ((int)0).AsStellarRoboInteger().NoResume();
            }

            //変換
            if(!int.TryParse(value,out result))
            {
                result = 0;
            }

            //戻り値設定
            return result.AsStellarRoboInteger().NoResume();
        }
        private static StellarRoboFunctionResult ToStr(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string value = string.Empty;

            //引数チェック
            if (args.Length == 1)
            {
                //数字取得
                value = args[0].ToString().Trim();
            }
            else
            {
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return value.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult MsgBox(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            int result = 0;
            MessageBoxButtons messageBoxButtons = MessageBoxButtons.OK;
            MessageBoxIcon messageBoxIcon = MessageBoxIcon.Information;
            string message = string.Empty;
            int value = 0;

            //引数チェック
            if(args.Length>=1 || args.Length <= 3)
            {
                //メッセージ取得
                message = args[0].ToString();

                //ボタンは設定するか？
                if (args.Length >= 2)
                {
                    //数値変換 ※変換出来なければデフォルトとしてOKボタンを使用する
                    if(int.TryParse(args[1].ToString(),out value))
                    {
                        //ボタンとして使用出来る値以外の場合にはOKボタンとする
                        if((value >= (int)MessageBoxButtons.OK) && (value <= (int)MessageBoxButtons.RetryCancel))
                        {
                            messageBoxButtons = (MessageBoxButtons)Enum.ToObject(typeof(MessageBoxButtons), value);
                        }
                    }
                }

                //アイコンは設定するか？
                if (args.Length == 3)
                {
                    //数値変換 ※変換出来なければデフォルトとして、Informationアイコンを使用する
                    if(int.TryParse(args[2].ToString(),out value))
                    {
                        switch (value)
                        {
                            case (int)MessageBoxIcon.Error:
                            case (int)MessageBoxIcon.Question:
                            case (int)MessageBoxIcon.Warning:
                            case (int)MessageBoxIcon.Information:
                                messageBoxIcon = (MessageBoxIcon)Enum.ToObject(typeof(MessageBoxIcon), value);
                                break;
                            default:
                                messageBoxIcon = MessageBoxIcon.Information;
                                break;
                        }
                    }
                }
            }
            else
            {
                //エラー値設定
                return ((int)RESULT_CODE.ArgumentError).AsStellarRoboInteger().NoResume();
            }

            //メッセージボックス表示
            using (Form frmDummy = new Form())
            {
                frmDummy.Opacity = 0;                                    //Form透明化
                frmDummy.StartPosition = FormStartPosition.CenterScreen; //表示位置を中央に設定
                frmDummy.Show();                                         //画面を表示
                frmDummy.TopMost = true;                                 //最前面

                //見えない最前面の画面で、メッセージボックス表示
                result = (int)MessageBox.Show(message, "StellarRobo", messageBoxButtons, messageBoxIcon);

                frmDummy.TopMost = false;                                //最前面解除
            }

            //戻り値設定
            return result.AsStellarRoboInteger().NoResume();
        }
        private static StellarRoboFunctionResult ReadXml(StellarRoboContext context,StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string fileName = string.Empty;
            string elementName = string.Empty;
            string XmlData = string.Empty;
            XDocument xml;
            string ResultValue = string.Empty;

            //引数チェック
            if (args.Length == 2)
            {
                //ファイル名取得
                fileName = args[0].ToString();

                //Element名取得
                elementName = args[1].ToString().Trim();
            }
            else
            {
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //ファイルは存在するか？
            if(!File.Exists(fileName))
            {
                //本来ならばExceptionを出力するのが通常だか空白を返すだけにする
                return string.Empty.AsStellarRoboString().NoResume();
            }

            try
            {
                //XMLの下準備
                xml = XDocument.Load(fileName);

                //Rootより下部Elementを読み込む
                foreach(var row in xml.Elements("config"))
                {
                    //指定Elementから値を取得する
                    ResultValue = row.Element(elementName).Value;
                }
            }
            catch(Exception)
            {
                //エラー値設定(念のために初期化)
                ResultValue = string.Empty;
            }

            //戻り値設定
            return ResultValue.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult NowDate(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string formatString = "yyyy/MM/dd HH:mm:ss";
            DateTime nowDate = DateTime.Now;

            //引数チェック
            if (args.Length == 0 || args.Length == 1)
            {
                //フォーマット指定はあるか？無ければデフォルトの'yyyy/MM/dd HH:mm:ss'を設定
                if (args.Length == 1) {
                    formatString = args[0].ToString();
                }
            }
            else
            {
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return nowDate.ToString(formatString).AsStellarRoboString().NoResume();
        }
        public StellarRoboFunctionResult GetErrorCode(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //戻り値設定
            return ErrorCode.AsStellarRoboInteger().NoResume();
        }
        public StellarRoboFunctionResult GetErrorMessage(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //戻り値設定
            return ErrorMessage.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult GetDownLoadPath(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string myDownLoad = string.Empty;
            IntPtr pPath;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length != 0)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            try
            {
                //OSのバージョンはVista以降か？
                OSVERSIONINFOEX osVerInfo = new OSVERSIONINFOEX();
                uint typeMask = 0;
                ulong conditionMask = 0;

                //MajorVersion指定
                osVerInfo.dwMajorVersion = 10;
                conditionMask = VerSetConditionMask(conditionMask, VER_MAJORVERSION, VER_GREATER_EQUAL);
                typeMask |= VER_MAJORVERSION;

                //MinorVersion指定
                osVerInfo.dwMinorVersion = 0;
                conditionMask = VerSetConditionMask(conditionMask, VER_MINORVERSION, VER_GREATER_EQUAL);
                typeMask |= VER_MINORVERSION;

                //ServicePackMajor指定
                osVerInfo.wServicePackMajor = 0;
                conditionMask = VerSetConditionMask(conditionMask, VER_SERVICEPACKMAJOR, VER_GREATER_EQUAL);
                typeMask |= VER_SERVICEPACKMAJOR;

                if (VerifyVersionInfo(ref osVerInfo, typeMask, conditionMask))
                {

                    //MyDownLoadパスを取得
                    if (SHGetKnownFolderPath(KnownFolder.LocalDownloads, 0, IntPtr.Zero, out pPath) == 0)
                    {
                        //取得
                        myDownLoad = Marshal.PtrToStringUni(pPath);

                        //開放
                        Marshal.FreeCoTaskMem(pPath);
                    }
                }
                else
                {
                    myDownLoad = System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
                }

                //扱いやすいように\を/に置換
                if (myDownLoad.Trim() != string.Empty)
                {
                    myDownLoad = myDownLoad.Replace('\\', '/');
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return myDownLoad.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult GetFileName(StellarRoboContext context,StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string filePathName = string.Empty;
            string fileName = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 1)
            {
                //ファイルパスを取得
                filePathName = args[0].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            #region 入力チェック
            //ファイルパスは入力されているか？
            if (string.IsNullOrEmpty(filePathName))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileGetFileNameInputNotEnteredError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            #endregion  

            try
            {
                //ファイルパスからファイル名を抽出する
                fileName = Path.GetFileName(filePathName);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();

            }

            //戻り値設定
            return fileName.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult CombinePath(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string filePath = string.Empty;
            string fileName = string.Empty;
            string filePathName = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if(args.Length == 2)
            {
                //ファイルパスを取得
                filePath = args[0].ToString().Trim();

                //ファイル名を取得
                fileName = args[1].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            #region 入力チェック
            //結合パスは入力されているか？
            if (string.IsNullOrEmpty(filePath))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileCombinePathInputNotEnteredError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            //結合ファイル名は入力されているか？
            if (string.IsNullOrEmpty(fileName))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileCombineFileInputNotEnteredError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            #endregion  

            try
            {
                //ファイルパスとファイル名を結合させる
                filePathName = Path.Combine(filePath, fileName).Replace('\\', '/');
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return filePathName.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult CheckDate(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            bool checkDate = false;
            string dateData = string.Empty;
            DateTime tmp;

            //引数チェック
            if (args.Length == 1)
            {
                //日付を取得
                dateData = args[0].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            try
            {
                //日付として使用できるか？
                checkDate = DateTime.TryParse(dateData, out tmp);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return checkDate.AsStellarRoboBoolean().NoResume();
        }
        public StellarRoboFunctionResult GetWindowsVersion(StellarRoboContext context, StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string windowsVersion = "Other OS";

            //引数チェック
            if (args.Length != 0)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            try
            {
                //実行しているWindowsのバージョンを取得する
                string tmp = string.Empty;
                ManagementClass mc = new ManagementClass("Win32_OperatingSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach(ManagementObject mo in moc)
                {
                    tmp = mo["version"].ToString();
                    if(tmp.Trim() != string.Empty)
                    {
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(tmp))
                {
                    string[] ver = tmp.Split('.');
                    switch (ver[0])
                    {
                        //Windows NT 3.1、Windows NT 3.5、Windows NT 3.51
                        case "3":
                            switch (ver[1])
                            {
                                case "1":
                                    windowsVersion = "WindowsNT3.1";
                                    break;
                                case "5":
                                    windowsVersion = "WindowsNT3.5";
                                    break;
                                case "51":
                                    windowsVersion = "WindowsNT3.51";
                                    break;
                            }
                            break;
                        //Windows 95、Windows 98、Windows ME
                        case "4":
                            switch (ver[1])
                            {
                                case "0":
                                    windowsVersion = "Windows95";
                                    break;
                                case "1":
                                    windowsVersion = "Windows98";
                                    break;
                                case "9":
                                    windowsVersion = "WindowsMe";
                                    break;
                            }
                            break;
                        //Windows XP
                        case "5":
                            switch (ver[1])
                            {
                                case "1":
                                    windowsVersion = "WindowsXp";
                                    break;
                            }
                            break;
                        //Window Vista、Windows 7、Windows 8、Windows 8.1
                        case "6":
                            switch (ver[1])
                            {
                                case "0":
                                    windowsVersion = "WindowsVista";
                                    break;
                                case "1":
                                    windowsVersion = "Windows7";
                                    break;
                                case "2":
                                    windowsVersion = "Windows8";
                                    break;
                                case "3":
                                    windowsVersion = "Windows8.1";
                                    break;
                                default:
                                    break;
                            }
                            break;
                        //Windows 10
                        case "10":
                            windowsVersion = "Windows10";
                            break;
                    }
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return windowsVersion.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult Logger(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            int outputType = 0;
            string message = string.Empty;
            List<string> attachedFile = new List<string>();
            VbCollection col = new VbCollection();

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 2 || args.Length == 3)
            {
                //出力先の指定を取得
                outputType = args[0].ToInt32();

                //出力するメッセージを取得
                message = args[1].ToString().Trim();

                //添付ファイルは指定されているか？
                if(args.Length > 2)
                {
                    string[] files = args[2].ToString().Trim().Split('|');
                    foreach(string file in files)
                    {
                        //空文字は追加しない
                        if (!string.IsNullOrEmpty(file.Trim()))
                        {
                            //添付ファイル保存
                            attachedFile.Add(file.Trim());
                        }
                    }
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return ErrorCode.AsStellarRoboInteger().NoResume();
            }

            try
            {
                //設定ファイルのロード
                StellarRoboSetting setting = null;
                string setting_file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StellarRobo.xml");

                if (File.Exists(setting_file))
                {
                    setting = StellarRoboSetting.LoadData(setting_file);
                }
                else
                {
                    throw new Exception("設定ファイルが存在しない為、動作を終了します。");
                }

                switch (outputType)
                {
                    case 0:
                        //出力しない
                        break;
                    case 1:
                        //ログを出力
                        Logger_Log(setting, message);
                        break;
                    case 2:
                        //メールを送信
                        if(setting.Mailer_Host == string.Empty || setting.Mailer_Port == string.Empty || setting.Mailer_User == string.Empty || setting.Mailer_Password == string.Empty
                             || setting.MailAddressFrom == string.Empty || setting.MailAddressTo == string.Empty || setting.MailDisplayNameFrom == string.Empty
                             || setting.MailDisplayNameTo == string.Empty || setting.MailSubject == string.Empty)
                        {
                            Logger_Log(setting, "メールの送信に必要な設定値がありません。設定内容をご確認ください。");
                        }
                        else
                        {
                            Logger_Mail(setting, message, attachedFile);
                        }
                        break;
                    case 3:
                        //ログ、メールを出力
                        Logger_Log(setting, message);
                        if (setting.Mailer_Host == string.Empty || setting.Mailer_Port == string.Empty || setting.Mailer_User == string.Empty || setting.Mailer_Password == string.Empty
                             || setting.MailAddressFrom == string.Empty || setting.MailAddressTo == string.Empty || setting.MailDisplayNameFrom == string.Empty
                             || setting.MailDisplayNameTo == string.Empty || setting.MailSubject == string.Empty)
                        {
                            Logger_Log(setting, "メールの送信に必要な設定値がありません。設定内容をご確認ください。");
                        }
                        else
                        {
                            Logger_Mail(setting, message, attachedFile);
                        }
                        break;
                    case 4:
                        //SNMPを出力
                        if (setting.Snmp_Host == string.Empty || setting.Snmp_Port == string.Empty || setting.EngineID == string.Empty)
                        {
                            Logger_Log(setting, "SNMPの出力に必要な設定値がありません。設定内容をご確認ください。");
                        }
                        else
                        {
                            Logger_SNMP(setting, message);
                        }
                        break;
                    case 5:
                        //ログ、SNMPを出力
                        Logger_Log(setting, message);
                        if (setting.Snmp_Host == string.Empty || setting.Snmp_Port == string.Empty || setting.EngineID == string.Empty)
                        {
                            Logger_Log(setting, "SNMPの出力に必要な設定値がありません。設定内容をご確認ください。");
                        }
                        else
                        {
                            Logger_SNMP(setting, message);
                        }
                        break;
                    case 6:
                        //メール、SNMPを出力
                        if (setting.Mailer_Host == string.Empty || setting.Mailer_Port == string.Empty || setting.Mailer_User == string.Empty || setting.Mailer_Password == string.Empty
                             || setting.MailAddressFrom == string.Empty || setting.MailAddressTo == string.Empty || setting.MailDisplayNameFrom == string.Empty
                             || setting.MailDisplayNameTo == string.Empty || setting.MailSubject == string.Empty)
                        {
                            Logger_Log(setting, "メールの送信に必要な設定値がありません。設定内容をご確認ください。");
                        }
                        else
                        {
                            Logger_Mail(setting, message, attachedFile);
                        }
                        if (setting.Snmp_Host == string.Empty || setting.Snmp_Port == string.Empty || setting.EngineID == string.Empty)
                        {
                            Logger_Log(setting, "SNMPの出力に必要な設定値がありません。設定内容をご確認ください。");
                        }
                        else
                        {
                            Logger_SNMP(setting, message);
                        }
                        break;
                    case 7:
                        //ログ、メール、SNMPを出力
                        Logger_Log(setting, message);
                        if (setting.Mailer_Host == string.Empty || setting.Mailer_Port == string.Empty || setting.Mailer_User == string.Empty || setting.Mailer_Password == string.Empty
                             || setting.MailAddressFrom == string.Empty || setting.MailAddressTo == string.Empty || setting.MailDisplayNameFrom == string.Empty
                             || setting.MailDisplayNameTo == string.Empty || setting.MailSubject == string.Empty)
                        {
                            Logger_Log(setting, "メールの送信に必要な設定値がありません。設定内容をご確認ください。");
                        }
                        else
                        {
                            Logger_Mail(setting, message, attachedFile);
                        }
                        if (setting.Snmp_Host == string.Empty || setting.Snmp_Port == string.Empty || setting.EngineID == string.Empty)
                        {
                            Logger_Log(setting, "SNMPの出力に必要な設定値がありません。設定内容をご確認ください。");
                        }
                        else
                        {
                            Logger_SNMP(setting, message);
                        }
                        break;
                    default:
                        //ログを出力
                        Logger_Log(setting, message);
                        break;
                }
            }
            catch (Exception e)
            {
                string me = e.Message;
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return ErrorCode.AsStellarRoboInteger().NoResume();
            }
            //戻り値設定
            return ErrorCode.AsStellarRoboInteger().NoResume();
        }
        #region Logger関数処理
        #region Logger関数_ログファイル出力
        private static void Logger_Log(StellarRoboSetting setting, string message)
        {
            //変数宣言
            int rotateState = -1;
            int rotateNumber = -1;
            int deleteHistory = -1;

            long getCapacity(long byteCapacity)
            {
                //ファイルサイズはByteで取得されるのでMByteに変換する
                return long.Parse((byteCapacity / (1024 * 1024)).ToString());
            }

            List<string> getHistoryFile(string logFile)
            {
                List<string> files = Directory.EnumerateFiles(Path.GetDirectoryName(logFile), Path.GetFileName(logFile) + ".*", SearchOption.TopDirectoryOnly).ToList<string>();
                files.Reverse();

                //戻り値設定
                return files;
            }
            int checkExtension(string file)
            {
                //変数宣言
                int history = -1;

                //拡張子は数字か？
                string extension = Path.GetExtension(file).Replace(".", "");
                if(!Regex.IsMatch(extension, "[^0-9]+"))
                {
                    history = int.Parse(extension);
                }

                //戻り値設定
                return history;
            }

            //ログを出力
            //設定値が見つからなかった場合は、デフォルトログファイルStellarRobo.logを出力する。
            string log_file;
            if (setting.Logfile == string.Empty)
            {
                log_file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StellarRobo.log");
            }
            else
            {
                log_file = setting.Logfile;
            }

            //ファイルの存在確認を行う
            if (!File.Exists(log_file))
            {
                //ファイルが無ければ新規作成する
                using (FileStream fileStream = File.Open(log_file, FileMode.Create))
                {
                    //初期化なので何もせず
                }
                File.SetCreationTime(log_file, DateTime.Now);
            }
            else
            {
                //設定ファイルよりRotateの設定を取得する
                rotateState = int.TryParse(setting.RotateType, out rotateState) ? rotateState : -1;
                rotateNumber = int.TryParse(setting.RotateCount, out rotateNumber) ? rotateNumber : -1;
                deleteHistory = int.TryParse(setting.HistoryCount, out deleteHistory) ? deleteHistory : -1;

                //各Rotate設定で1つでも未設定があれば、Rotate処理は行わない
                if ((rotateState >= 0) && (rotateNumber >= 0) && (deleteHistory >= 0))
                {
                    //Rotateを行うか？
                    if (rotateState >= 0)
                    {
                        #region Rotate判定
                        double interval = -1;
                        bool rotateFile = false;
                        FileInfo fileInfo = new FileInfo(log_file);

                        switch (rotateState)
                        {
                            //日
                            case (int)LoggerJudgmentsType.Days:
                                //経過日数を取得する
                                interval = Math.Truncate((DateTime.Now - fileInfo.CreationTime).TotalDays);
                                if (interval >= (double)rotateNumber)
                                {
                                    //ファイルのRotateを行う
                                    rotateFile = true;
                                }
                                break;
                            //容量
                            case (int)LoggerJudgmentsType.Capacity:
                                //ファイルサイズを取得する
                                if (getCapacity(fileInfo.Length) >= rotateNumber)
                                {
                                    //ファイルのRotateを行う
                                    rotateFile = true;
                                }
                                break;
                            //以外
                            default:
                                //経過日数を取得する
                                interval = Math.Truncate((DateTime.Now - fileInfo.CreationTime).TotalDays);
                                if (interval >= (double)rotateNumber)
                                {
                                    //ファイルのRotateを行う
                                    rotateFile = true;
                                }
                                break;
                        }
                        #endregion

                        #region Rotate処理
                        //Rotateするか？
                        if (rotateFile)
                        {
                            foreach (string file in getHistoryFile(log_file))
                            {
                                //拡張子は数字か？
                                int historyExtension = checkExtension(file);

                                //変更後のLogファイルは存在するか？
                                string newLogFileName = (historyExtension > 0) ? Path.ChangeExtension(file, (historyExtension + 1).ToString()) : file + ".1";
                                if (!File.Exists(newLogFileName))
                                {
                                    //存在しなければ、ファイルをリネーム
                                    File.Move(file, newLogFileName);
                                }
                            }

                            //新規でファイルを開く
                            //ファイルが無ければ新規作成する
                            using (FileStream fileStream = File.Open(log_file, FileMode.Create))
                            {
                                //初期化なので何もせず
                            }
                            File.SetCreationTime(log_file, DateTime.Now);
                        }
                        #endregion
                    }

                    //世代処理は行うか？
                    if (deleteHistory > 0)
                    {
                        #region 世代処理
                        //Rotateファイルを削除するか？
                        if (deleteHistory > 0)
                        {
                            //指定数以降のRotateファイルの削除を行う
                            foreach (string file in getHistoryFile(log_file))
                            {
                                //拡張子は数字か？
                                int historyExtension = checkExtension(file);
                                if (historyExtension > 0)
                                {
                                    //世帯を確認
                                    if (historyExtension > deleteHistory)
                                    {
                                        //指定世代以降なのでファイルを削除する
                                        File.Delete(file);
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
            }

            //ファイルを開く
            using (FileStream fileStream = File.Open(log_file, FileMode.Append))
            using (StreamWriter streamWriter = new StreamWriter(fileStream, System.Text.Encoding.GetEncoding("UTF-8")))
            {
                string formatString = "yyyy/MM/dd HH:mm:ss";
                DateTime nowDate = DateTime.Now;
                //書き込み
                streamWriter.WriteLine(nowDate.ToString(formatString) + " " + message);
            }
        }
        #endregion
        #region Logger関数_メール送信
        private static void Logger_Mail(StellarRoboSetting setting, string message, List<string> attachedFiles)
        {
            //メールを送信
            try
            {
                System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
                msg.From = new System.Net.Mail.MailAddress(setting.MailAddressFrom, setting.MailDisplayNameFrom);
                msg.To.Add(new System.Net.Mail.MailAddress(setting.MailAddressTo, setting.MailDisplayNameTo));
                msg.Subject = setting.MailSubject;
                msg.Body = message;

                //添付ファイルは指定されているか？
                if (attachedFiles.Count > 0)
                {
                    foreach (string attachedFile in attachedFiles)
                    {
                        //メールに添付ファイルを設定する
                        System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(attachedFile);
                        attachment.Name = Path.GetFileName(attachedFile);
                        msg.Attachments.Add(attachment);
                    }
                }

                System.Net.Mail.SmtpClient sc = new System.Net.Mail.SmtpClient();

                sc.Host = setting.Mailer_Host;
                sc.Port = Convert.ToInt32(setting.Mailer_Port);
                sc.Credentials = new System.Net.NetworkCredential(setting.Mailer_User, setting.Mailer_Password);
                if (setting.EnableSsl == string.Empty)
                {
                    //デフォルトはfalse
                    sc.EnableSsl = false;
                }
                else
                {
                    sc.EnableSsl = Convert.ToBoolean(setting.EnableSsl);
                }

                sc.Send(msg);
                msg.Dispose();
            }
            catch(Exception)
            {
                Logger_Log(setting, "メールの送信に失敗しました。設定内容をご確認ください。");
            }
        }
        #endregion
        #region Logger関数_SNMP出力
        private static void Logger_SNMP(StellarRoboSetting setting, string message)
        {
            //SNMPを出力
            try
            {
                //エンジンIDの取得
                //偶数桁であること
                if (setting.EngineID.Length % 2 == 1)
                {
                    throw new Exception();
                }
                List<byte> bs = new List<byte>();
                byte[] engineId;
                foreach (string s in Regex.Split(setting.EngineID.Substring(2), @"(?<=\G.{2})(?!$)"))
                {
                    bs.Add(Convert.ToByte(s, 16));
                }
                engineId = bs.ToArray();

                TrapAgent agent = new TrapAgent();

                // TRAPに含める情報を保持するVbCollectionを作成
                VbCollection col = new VbCollection();
                col.Add(new SnmpSharpNet.Oid("1.3.6.1.2.1.1.1.0"), new OctetString("StellarRobo"));             //sysDescr 機器・ホスト名
                //col.Add(new SnmpSharpNet.Oid("1.3.6.1.2.1.1.2.0"), new SnmpSharpNet.Oid("1.3.6.1.9.1.1.0"));    //sysObjectID 企業番号・製品番号
                //col.Add(new SnmpSharpNet.Oid("1.3.6.1.2.1.1.3.0"), new TimeTicks(2324));                        //sysUpTime SNMPエージェント起動後の経過時間 SendV3Trapに設定済
                //col.Add(new SnmpSharpNet.Oid("1.3.6.1.2.1.1.4.0"), new OctetString("Milan"));                   //sysContact 機器の管理者のメールアドレス
                col.Add(new SnmpSharpNet.Oid("1.3.6.1.2.1.1.5.0"), new OctetString(message));                   //sysName SNMPエージェントの名称 ここにmessageを出力

                //SNMP バージョン3(認証、暗号化無し) TRAP送信
                //引数1:関連ノードのIDアドレス
                //引数2:TRAPのポート番号
                //引数3:エンジンID
                //引数4:Boots値
                //引数5:EngineTime値
                //引数6:ユーザー名
                //引数7:sysUpTime値
                //引数8:OID値 TRAPの種類を指定
                //引数9:TRAPに含める情報を保持するVbCollection
                agent.SendV3Trap(new IpAddress(setting.Snmp_Host), Convert.ToInt32(setting.Snmp_Port),
                    engineId,
                    1, 500, "mysecurityname", 13434, new SnmpSharpNet.Oid(".1.3.6.1.6.3.1.1.5"), col);
            }
            catch(Exception)
            {
                Logger_Log(setting, "SNMPの出力に失敗しました。設定内容をご確認ください。");
            }
        }
        #endregion
        #endregion

        private StellarRoboFunctionResult ImageCapture(StellarRoboContext context, StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string fileName = string.Empty;
            string imageType = string.Empty;
            System.Drawing.Imaging.ImageFormat imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 0 || args.Length == 1 || args.Length == 2)
            {
                if (args.Length > 0)
                {
                    //ファイル名を取得
                    fileName = args[0].ToString().Trim();

                    if (args.Length == 2)
                    {
                        //保存形式取得
                        imageType = args[1].ToString().Trim();
                    }
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //保存形式は入力されているか？
            if (string.IsNullOrEmpty(imageType))
            {
                //未入力の場合にはjpegで保存する
                imageType = "jpeg";
                imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
            }
            else
            {
                if (!Regex.IsMatch(imageType, "(jpeg|jpg|bmp|png)", RegexOptions.IgnoreCase))
                {
                    SetErrorInfo((int)RESULT_CODE.BuiltInFunctionImageTypeError);
                    return false.AsStellarRoboBoolean().NoResume();
                }

                //保存形式取得
                switch (imageType.ToUpper())
                {
                    case "JPEG":
                    case "JPG":
                        imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                        break;
                    case "BMP":
                        imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                        break;
                    case "PNG":
                        imageFormat = System.Drawing.Imaging.ImageFormat.Png;
                        break;
                }
            }
            //ファイル名は入力されているか？
            if (string.IsNullOrEmpty(fileName))
            {
                //未入力の場合にはyyyyMMddHHmmss.imageTypeでCaptureする
                fileName = "Capture_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + imageType;
            }
            #endregion  

            try
            {
                //画面サイズを取得する
                var screen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                int width = screen.Width;
                int height = screen.Height;

                //スクリーンショットを作成
                Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                }

                //拡張子は付けられているか？
                if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                {
                    fileName += "." + imageType;
                }

                //ファイルは存在しているか？
                if (File.Exists(fileName))
                {
                    //上書きする為に、削除する
                    File.Delete(fileName);
                }

                //スクリーンショットを保存する
                bmp.Save(fileName, imageFormat);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();

        }
        private StellarRoboFunctionResult DateAdd(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string nowDateTmp = string.Empty;
            DateTime nowDate = DateTime.Now;
            DateTime calcDate;
            string IntervalTmp = string.Empty;
            int Interval = 0;
            DateInterval dateInterval = DateInterval.Year;
            string TimeIntervalTmp = string.Empty;
            int TimeInterval = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 3)
            {
                //時間間隔を取得
                IntervalTmp = args[0].ToString().Trim();

                //加算時間間隔を取得
                TimeIntervalTmp = args[1].ToString().Trim();

                //基準日付を取得
                nowDateTmp = args[2].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();

            }

            #region 入力チェック
            //時間間隔は入力されているか？
            if (string.IsNullOrEmpty(IntervalTmp))
            {
                SetErrorInfo((int)RESULT_CODE.BuiltInFunctionIntervalInputNotEnteredError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            else
            {
                //数値か？
                if(!int.TryParse(IntervalTmp, out Interval))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return string.Empty.AsStellarRoboString().NoResume();
                }
                //範囲内か？
                if(Interval < 0 || Interval > 2)
                {
                    SetErrorInfo((int)RESULT_CODE.BuiltInFunctionIntervalOutOfRangeError);
                    return string.Empty.AsStellarRoboString().NoResume();
                }
                switch (Interval)
                {
                    //年
                    case 0:
                        dateInterval = DateInterval.Year;
                        break;
                    //月
                    case 1:
                        dateInterval = DateInterval.Month;
                        break;
                    //日
                    case 2:
                        dateInterval = DateInterval.Day;
                        break;
                    //その他
                    default:
                        dateInterval = DateInterval.Year;
                        break;
                }
            }
            //加算時間間隔は入力されているか？
            if (string.IsNullOrEmpty(TimeIntervalTmp))
            {
                SetErrorInfo((int)RESULT_CODE.BuiltInFunctionTimeIntervalInputNotEnteredError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            else
            {
                //数値か？
                if(!int.TryParse(TimeIntervalTmp, out TimeInterval))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return string.Empty.AsStellarRoboString().NoResume();
                }
                //範囲内か？
                if(TimeInterval> 999|| TimeInterval < -999)
                {
                    SetErrorInfo((int)RESULT_CODE.BuiltInFunctionTimeIntervalOutOfRangeError);
                    return string.Empty.AsStellarRoboString().NoResume();
                }
            }
            //基準日付は入力されているか？
            if (string.IsNullOrEmpty(nowDateTmp))
            {
                SetErrorInfo((int)RESULT_CODE.BuiltInFunctionReferenceDateInputNotEnteredError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            else
            {
                //日付として正しか？
                if(!DateTime.TryParse(nowDateTmp, out nowDate))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsDateError);
                    return string.Empty.AsStellarRoboString().NoResume();
                }
            }
            #endregion  

            try
            {
                //日付間隔の計算を行う
                calcDate = DateAndTime.DateAdd(dateInterval, TimeInterval, nowDate);
            }
            catch(Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();

            }

            //戻り値設定
            return calcDate.ToString("yyyy/MM/dd").AsStellarRoboString().NoResume();
        }
        #endregion

        #region ファイル関連
        private static StellarRoboFunctionResult InitFile(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string fileName = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if(args.Length == 1)
            {
                //ファイル・パス取得
                fileName = args[0].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //ファイル名は入力されているか？
            if (string.IsNullOrEmpty(fileName))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileFileInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            #endregion  

            try
            {
                //ファイルをFileMode.Createで開く
                using (FileStream fileStream = File.Open(fileName, FileMode.Create))
                {
                    //初期化なので何もせず
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult WriteFile(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string fileName = string.Empty;
            string text = string.Empty;
            string encoding = DEFAULT_ENCODE;                  //文字コードはデフォルト[UTF-8]

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 2 || args.Length == 3)
            {
                //ファイル・パス取得
                fileName = args[0].ToString().Trim();

                //テキスト・データ取得
                text = args[1].ToString();

                //文字コードは指定されているか？
                if(args.Length==3)
                {
                    //文字コード取得
                    encoding = args[2].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //ファイル名は入力されているか？
            if (string.IsNullOrEmpty(fileName))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileFileInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //ファイルは存在するか？
            if (!File.Exists(fileName))
            {
                //存在しないのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileFileExistsError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            if (args.Length == 3)
            {
                //Encode方式は入力されているか？
                if (string.IsNullOrEmpty(encoding))
                {
                    //未入力なのでエラーとする
                    SetErrorInfo((int)RESULT_CODE.FileEncodeInputNotEnteredError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                //取得した文字コードは使用出来るか？ ※大文字、小文字は区別しない
                if (!Regex.IsMatch(encoding, MACHE_ENCODE, RegexOptions.IgnoreCase))
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.FileEncodeCanNotUseError);
                    //エラー値設定
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            #endregion

            try
            {
                //ファイルを開く
                using (FileStream fileStream = File.Open(fileName, FileMode.Append))
                using (StreamWriter streamWriter = new StreamWriter(fileStream, System.Text.Encoding.GetEncoding(encoding)))
                {
                    try
                    {
                        //書き込み
                        streamWriter.WriteLine(text);
                    }
                    catch (Exception e)
                    {
                        //エラー値設定
                        SetErrorInfo(e.HResult + EXCEPTION_ERROR);
                        return false.AsStellarRoboBoolean().NoResume();
                    }
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult ReadFile(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string fileName = string.Empty;
            string text = string.Empty;
            string encoding = DEFAULT_ENCODE;                  //文字コードはデフォルト[UTF-8]

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 1 || args.Length == 2)
            {
                //ファイル・パス取得
                fileName = args[0].ToString().Trim();

                //文字コードは指定されているか？
                if(args.Length==2)
                {
                    //文字コードを取得する
                    encoding = args[1].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            #region 入力チェック
            //ファイル名は入力されているか？
            if (string.IsNullOrEmpty(fileName))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileFileInputNotEnteredError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            //ファイルは存在するか？
            if (!File.Exists(fileName))
            {
                //存在しないのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileFileExistsError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            if (args.Length == 2)
            {
                //Encode方式は入力されているか？
                if (string.IsNullOrEmpty(encoding))
                {
                    //未入力なのでエラーとする
                    SetErrorInfo((int)RESULT_CODE.FileEncodeInputNotEnteredError);
                    return string.Empty.AsStellarRoboString().NoResume();
                }
                //取得した文字コードは使用出来るか？
                if (!Regex.IsMatch(encoding, MACHE_ENCODE, RegexOptions.IgnoreCase))
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.FileEncodeCanNotUseError);
                    //エラー値設定
                    return string.Empty.AsStellarRoboString().NoResume();
                }
            }
            #endregion

            try
            {
                //ファイルを開く
                using (FileStream fileStream = File.Open(fileName, FileMode.Open))
                using (StreamReader streamReader = new StreamReader(fileStream, System.Text.Encoding.GetEncoding(encoding)))
                {
                    try
                    {
                        //読み込み
                        text = streamReader.ReadToEnd();
                    }
                    catch (Exception e)
                    {
                        //エラー値設定
                        SetErrorInfo(e.HResult + EXCEPTION_ERROR);
                        return string.Empty.AsStellarRoboString().NoResume();
                    }
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return text.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult FileCopy(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string sourceFileName = string.Empty;
            string destFileName = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if(args.Length==2)
            {
                //コピー情報取得
                sourceFileName = args[0].ToString().Trim();
                destFileName = args[1].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //コピー元は入力されているか？
            if (string.IsNullOrEmpty(sourceFileName))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileSourceFileInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //コピー先は入力されているか？
            if (string.IsNullOrEmpty(destFileName))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileDestinationFileInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //コピー元ファイルは存在するか？
            if (!File.Exists(sourceFileName))
            {
                //存在しなのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileSourceFileNotExistsError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            #endregion

            try
            {
                //ファイルコピーを行う
                File.Copy(sourceFileName, destFileName, true);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult FileMove(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string sourceFileName = string.Empty;
            string destFileName = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 2)
            {
                //コピー情報取得
                sourceFileName = args[0].ToString().Trim();
                destFileName = args[1].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //移動元は入力されているか？
            if (string.IsNullOrEmpty(sourceFileName))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileSourceFileInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //移動先は入力されているか？
            if (string.IsNullOrEmpty(destFileName))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileDestinationFileInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //移動元ファイルは存在するか？
            if (!File.Exists(sourceFileName))
            {
                //存在しなのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileSourceFileNotExistsError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //移動先ファイルは存在するか？
            if (File.Exists(destFileName))
            {
                //存在するのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileDeleteFileExistsError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            #endregion

            try
            {
                //ファイル移動を行う
                File.Move(sourceFileName, destFileName);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult FileReName(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string filePath = string.Empty;
            string sourceFileName = string.Empty;
            string destFileName = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 3)
            {
                //リネーム情報取得
                filePath = args[0].ToString().Trim();
                sourceFileName = @args[1].ToString().Trim();
                destFileName = @args[2].ToString().Trim();
            }else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //パスは入力されているか？
            if (string.IsNullOrEmpty(filePath))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileReNamePathInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //コピー元は入力されているか？
            if (string.IsNullOrEmpty(sourceFileName))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileSourceFileInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //コピー先は入力されているか？
            if (string.IsNullOrEmpty(destFileName))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileDestinationFileInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //コピー元ファイル名に使用不可の文字列は使われているか？
            if (Regex.IsMatch(sourceFileName, FILE_NAME_CAN_NOT_USE))
            {
                //ファイルに使用出来ない文字を使用していたのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileFileNameCanNotUseError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //コピー先ファイル名に使用不可の文字列は使われているか？
            if (Regex.IsMatch(destFileName, FILE_NAME_CAN_NOT_USE))
            {
                //ファイルに使用出来ない文字を使用していたのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileFileNameCanNotUseError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //コピー元ファイルは存在するか？
            try
            {
                if (!File.Exists(Path.Combine(filePath, sourceFileName)))
                {
                    //存在しなのでエラーとする
                    SetErrorInfo((int)RESULT_CODE.FileSourceFileNotExistsError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }
            #endregion

            try
            {
                //ファイルリネームを行う
                File.Move(Path.Combine(filePath, sourceFileName), Path.Combine(filePath, destFileName));
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult FileDelete(StellarRoboContext context , StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string deleteFileName = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if(args.Length==1)
            {
                //デリート情報取得
                deleteFileName = args[0].ToString().Trim();
            } else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //削除ファイルは入力されているか？
            if (string.IsNullOrEmpty(deleteFileName))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileDeleteFileInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //削除ファイルは存在するか？
            if (!File.Exists(deleteFileName))
            {
                //存在しなのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileDeleteFileExistsError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            #endregion

            try
            {
                //ファイルデリートを行う
                File.Delete(deleteFileName);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult FileExists(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            bool fileExists = false;
            string filePath = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if(args.Length==1)
            {
                //確認情報取得
                filePath = args[0].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            try
            {
                //ファイルの存在確認を行う
                fileExists = File.Exists(filePath);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return fileExists.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult DirectoryExists(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            bool directoryExists = false;
            string directoryPath = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if(args.Length == 1)
            {
                //確認情報取得
                directoryPath = args[0].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            try
            {
                //ディレクトリの存在確認を行う
                directoryExists = Directory.Exists(directoryPath);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return directoryExists.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult SearchFile(StellarRoboContext context,StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string[] searchFileName = new string[] { };
            string filePath = string.Empty;
            string fileName = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if(args.Length == 2)
            {
                //検索ファイルパス取得
                filePath = args[0].ToString().Trim();

                //検索ファイル名取得
                fileName = args[1].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return StellarRoboNil.Instance.NoResume();
            }

            #region 入力チェック
            //検索ファイルパスは入力されているか？
            if (string.IsNullOrEmpty(filePath))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileDeleteFileInputNotEnteredError);
                return StellarRoboNil.Instance.NoResume();
            }
            //検索ファイルは入力されているか？
            if (string.IsNullOrEmpty(fileName))
            {
                //未入力なのでエラー
                SetErrorInfo((int)RESULT_CODE.FileSearchFileInputNotEnteredError);
                return StellarRoboNil.Instance.NoResume();
            }
            #endregion

            try
            {
                //ファイルを検索する
                searchFileName = Directory.GetFiles(filePath, fileName);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return StellarRoboNil.Instance.NoResume();
            }

            //戻り値設定
            return searchFileName.ToStellarRoboArray().NoResume();
        }
        private static StellarRoboFunctionResult GetCurrentPath(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string currentPath = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length != 0)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            try
            {
                //カレントパスを取得する
                currentPath = Environment.CurrentDirectory;
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return currentPath.AsStellarRoboString().NoResume();

        }
        private static StellarRoboFunctionResult SetCurrentPath(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string CurrentPath = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 1)
            {
                //カレント・パス取得
                CurrentPath = args[0].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //カレントパスは入力されているか？
            if (string.IsNullOrEmpty(CurrentPath))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.FileCurrentPathInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            #endregion  

            try
            {
                //カレントパスを設定する
                Environment.CurrentDirectory = CurrentPath;
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();

        }
        #endregion

        #region マウス関連
        private static int GetDoubleClickInterval()
        {
            //DoubleClickの間隔を取得する
            int doubleClickTime = ((int)GetDoubleClickTime()) - DOUBLE_CLICK_MARGIN;
            return doubleClickTime < 0 ? 0 : doubleClickTime;
        }
        private static int MouseProcessing(StellarRoboObject[] args,List<InputSimulator.MouseStroke> mouseStrokes)
        {
            //変数宣言
            List<InputSimulator.Input> inputs = new List<InputSimulator.Input>();
            List<InputSimulator.MouseStroke> flags = new List<InputSimulator.MouseStroke>();
            int x = 0;
            int y = 0;

            if (args.Length == 0)
            {
                //現在のマウスカーソル座標を取得する
                if (GetCursorPos(out POINT pt))
                {
                    //座標取得
                    x = pt.X;
                    y = pt.Y;
                }
                else
                {
                    x = 1;
                    y = 1;
                }
            }
            else
            {
                //座標設定
                //マウス座標は入力されているか
                if (string.IsNullOrEmpty(args[0].ToString().Trim()))
                {
                    //入力されていないのでエラーとする
                    return ((int)RESULT_CODE.MouseCoordinateNotEnteredError);
                }
                if (string.IsNullOrEmpty(args[1].ToString().Trim()))
                {
                    //入力されていないのでエラーとする
                    return ((int)RESULT_CODE.MouseCoordinateNotEnteredError);
                }
                //数字以外が入力されているか？
                if(!int.TryParse(args[0].ToString().Trim(),out x))
                {
                    return ((int)RESULT_CODE.ArgumentIsNumberError);
                }
                if (!int.TryParse(args[1].ToString().Trim(), out y))
                {
                    return ((int)RESULT_CODE.ArgumentIsNumberError);
                }
            }

            //マウスイベント登録
            foreach (InputSimulator.MouseStroke mouseStroke in mouseStrokes)
            {
                flags.Add(mouseStroke);
            }
            InputSimulator.AddMouseInput(ref inputs, flags, 0, true, x, y);

            //マウス動作をエミュレートする
            InputSimulator.SendInput(inputs);

            //終了
            return (int)RESULT_CODE.Success;
        }
        private static StellarRoboFunctionResult MouseMove(StellarRoboContext context, StellarRoboObject self,StellarRoboObject[] args)
        {
            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length != 2)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            try
            {
                //マウスイベント登録
                int result = MouseProcessing(args, new List<InputSimulator.MouseStroke>() { InputSimulator.MouseStroke.MOVE });
                if (result != (int)RESULT_CODE.Success)
                {
                    //ステータス設定
                    SetErrorInfo(result);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult LeftDown(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (!(args.Length == 0 || args.Length == 2))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            try
            {
                //マウスイベント登録
                int result = MouseProcessing(args, new List<InputSimulator.MouseStroke>() { InputSimulator.MouseStroke.MOVE, InputSimulator.MouseStroke.LEFT_DOWN });
                if (result != (int)RESULT_CODE.Success)
                {
                    //ステータス設定
                    SetErrorInfo(result);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult LeftUp(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (!(args.Length == 0 || args.Length == 2))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            try
            {
                //マウスイベント登録
                int result = MouseProcessing(args, new List<InputSimulator.MouseStroke>() { InputSimulator.MouseStroke.MOVE, InputSimulator.MouseStroke.LEFT_UP });
                if (result != (int)RESULT_CODE.Success)
                {
                    //ステータス設定
                    SetErrorInfo(result);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult LeftClick(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (!(args.Length == 0 || args.Length == 2))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            try
            {
                //マウスイベント登録
                int result = MouseProcessing(args, new List<InputSimulator.MouseStroke>() { InputSimulator.MouseStroke.MOVE, InputSimulator.MouseStroke.LEFT_DOWN, InputSimulator.MouseStroke.LEFT_UP });
                if (result != (int)RESULT_CODE.Success)
                {
                    //ステータス設定
                    SetErrorInfo(result);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult LeftDoubleClick(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            int doubleClickTime = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (!(args.Length == 0 || args.Length == 2))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            try
            {
                //DoubleClickの間隔を取得
                doubleClickTime = GetDoubleClickInterval();

                //マウスイベント登録
                int result = MouseProcessing(args, new List<InputSimulator.MouseStroke>() { InputSimulator.MouseStroke.MOVE, InputSimulator.MouseStroke.LEFT_DOWN, InputSimulator.MouseStroke.LEFT_UP });
                if(result != (int)RESULT_CODE.Success)
                {
                    //ステータス設定
                    SetErrorInfo(result);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                Thread.Sleep(doubleClickTime);
                result = MouseProcessing(args, new List<InputSimulator.MouseStroke>() { InputSimulator.MouseStroke.MOVE, InputSimulator.MouseStroke.LEFT_DOWN, InputSimulator.MouseStroke.LEFT_UP });
                if (result != (int)RESULT_CODE.Success)
                {
                    //ステータス設定
                    SetErrorInfo(result);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult RightDown(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (!(args.Length == 0 || args.Length == 2))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            try
            {
                //マウスイベント登録
                int result = MouseProcessing(args, new List<InputSimulator.MouseStroke>() { InputSimulator.MouseStroke.MOVE, InputSimulator.MouseStroke.RIGHT_DOWN });
                if(result != (int)RESULT_CODE.Success)
                { 
                    //ステータス設定
                    SetErrorInfo(result);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult RightUp(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (!(args.Length == 0 || args.Length == 2))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            try
            {
                //マウスイベント登録
                int result = MouseProcessing(args, new List<InputSimulator.MouseStroke>() { InputSimulator.MouseStroke.MOVE, InputSimulator.MouseStroke.RIGHT_UP });
                if(result != (int)RESULT_CODE.Success)
                {
                    //ステータス設定
                    SetErrorInfo(result);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult RightClick(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (!(args.Length == 0 || args.Length == 2))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            try
            {
                //マウスイベント登録
                int result = MouseProcessing(args, new List<InputSimulator.MouseStroke>() { InputSimulator.MouseStroke.MOVE, InputSimulator.MouseStroke.RIGHT_DOWN, InputSimulator.MouseStroke.RIGHT_UP });
                if(result != (int)RESULT_CODE.Success)
                {
                    //ステータス設定
                    SetErrorInfo(result);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult RightDoubleClick(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            int doubleClickTime = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (!(args.Length == 0 || args.Length == 2))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            try
            {
                //DoubleClickの間隔を取得
                doubleClickTime = GetDoubleClickInterval();

                //マウスイベント登録
                int result = MouseProcessing(args, new List<InputSimulator.MouseStroke>() { InputSimulator.MouseStroke.MOVE, InputSimulator.MouseStroke.RIGHT_DOWN, InputSimulator.MouseStroke.RIGHT_UP });
                if(result != (int)RESULT_CODE.Success)
                {
                    //ステータス設定
                    SetErrorInfo(result);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                Thread.Sleep(doubleClickTime);
                result = MouseProcessing(args, new List<InputSimulator.MouseStroke>() { InputSimulator.MouseStroke.MOVE, InputSimulator.MouseStroke.RIGHT_DOWN, InputSimulator.MouseStroke.RIGHT_UP });
                if(result != (int)RESULT_CODE.Success)
                {
                    //ステータス設定
                    SetErrorInfo(result);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult MiddleDown(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (!(args.Length == 0 || args.Length == 2))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            try
            {
                //マウスイベント登録
                int result = MouseProcessing(args, new List<InputSimulator.MouseStroke>() { InputSimulator.MouseStroke.MOVE, InputSimulator.MouseStroke.MIDDLE_DOWN });
                if(result != (int)RESULT_CODE.Success)
                {
                    //ステータス設定
                    SetErrorInfo(result);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult MiddleUp(StellarRoboContext context, StellarRoboObject self,StellarRoboObject[] args)
        {
            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (!(args.Length == 0 || args.Length == 2))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            try
            {
                //マウスイベント登録
                int result = MouseProcessing(args, new List<InputSimulator.MouseStroke>() { InputSimulator.MouseStroke.MOVE, InputSimulator.MouseStroke.MIDDLE_UP });
                if(result != (int)RESULT_CODE.Success)
                {
                    //ステータス設定
                    SetErrorInfo(result);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult MiddleClick(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (!(args.Length == 0 || args.Length == 2))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            try
            {
                //マウスイベント登録
                int result = MouseProcessing(args, new List<InputSimulator.MouseStroke>() { InputSimulator.MouseStroke.MOVE, InputSimulator.MouseStroke.MIDDLE_DOWN, InputSimulator.MouseStroke.MIDDLE_UP });
                if(result != (int)RESULT_CODE.Success)
                {
                    //ステータス設定
                    SetErrorInfo(result);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult MiddleDoubleClick(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            int doubleClickTime = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (!(args.Length == 0 || args.Length == 2))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            try
            {
                //DoubleClickの間隔を取得
                doubleClickTime = GetDoubleClickInterval();

                //マウスイベント登録
                int result = MouseProcessing(args, new List<InputSimulator.MouseStroke>() { InputSimulator.MouseStroke.MOVE, InputSimulator.MouseStroke.MIDDLE_DOWN, InputSimulator.MouseStroke.MIDDLE_UP });
                if(result != (int)RESULT_CODE.Success)
                {
                    //ステータス設定
                    SetErrorInfo(result);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                Thread.Sleep(doubleClickTime);
                result = MouseProcessing(args, new List<InputSimulator.MouseStroke>() { InputSimulator.MouseStroke.MOVE, InputSimulator.MouseStroke.MIDDLE_DOWN, InputSimulator.MouseStroke.MIDDLE_UP });
                if(result != (int)RESULT_CODE.Success)
                {
                    //ステータス設定
                    SetErrorInfo(result);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult Click(StellarRoboContext context, StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string HandleTmp = string.Empty;
            IntPtr Handle = IntPtr.Zero;
            string SearchWord = string.Empty;
            string SearchTypeTmp = string.Empty;
            int SearchType = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 2 || args.Length == 3)
            {
                //Handle取得
                HandleTmp = args[0].ToString();

                //SearchWord取得
                SearchWord = args[1].ToString().Trim();

                //検索タイプ取得
                if (args.Length > 2)
                {
                    SearchTypeTmp = args[2].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //Windowハンドルは入力されているか？
            if (string.IsNullOrEmpty(HandleTmp))
            {
                //Windowハンドルが入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationWindowHandleInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //変換出来ない場合には強制的に0に変換
            Int64 tmp = 0;
            if (!Int64.TryParse(HandleTmp, out tmp)) { tmp = 0; }
            Handle = (IntPtr)tmp;

            //SearchWordは入力されているか？
            if (string.IsNullOrEmpty(SearchWord))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.MouseSearchWordInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //検索タイプは入力されているか？
            if (string.IsNullOrEmpty(SearchTypeTmp))
            {
                //省略されているならば、IDで検索を行う
                SearchType = 0;
            }
            else
            {
                //数値入力か？
                if (!int.TryParse(SearchTypeTmp, out SearchType))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                //範囲内か？
                if (!(SearchType >= 0 && SearchType < (int)ControlSearchType.End))
                {
                    SetErrorInfo((int)RESULT_CODE.MouseSearchTypeOutOfRangeError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            #endregion

            try
            {
                //検索条件を指定しControlを取得する
                AutomationElementCollection elementCollection = SearchControl(Handle, SearchType, SearchWord);

                //取得出来たか？
                if(elementCollection.Count> 0)
                {
                    AutomationElement element = elementCollection[0];

                    switch (element.Current.ControlType)
                    {
                        //Button
                        case ControlType controlType when controlType == ControlType.Button:
                            //取得した先頭のButtonControlに対しClick処理を行う
                            var control = (InvokePattern)element.GetCurrentPattern(InvokePattern.Pattern);
                            control.Invoke();
                            break;
                        //CheckBox
                        case ControlType controlType when controlType == ControlType.CheckBox:
                            object togglePattern;
                            if(element.TryGetCurrentPattern(TogglePattern.Pattern, out togglePattern))
                            {
                                ((TogglePattern)togglePattern).Toggle();
                            }
                            break;
                        //RadioButton
                        case ControlType controlType when controlType == ControlType.RadioButton:
                            object selectionItemPattern;
                            if(element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out selectionItemPattern))
                            {
                                ((SelectionItemPattern)selectionItemPattern).Select();
                            }
                            break;
                    }
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        #endregion

        #region 入力関連
        private static StellarRoboFunctionResult InputBox(StellarRoboContext context,StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string input_message = string.Empty;
            string prompt = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 1)
            {
                //出力文字列取得
                prompt = args[0].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return "".AsStellarRoboString().NoResume();
            }

            #region 入力チェック
            //Promptは入力されているか？
            if (string.IsNullOrEmpty(prompt))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.InputPromptInputNotEnteredError);
                return "".AsStellarRoboString().NoResume();
            }
            #endregion  

            try
            {
                //文字列入力
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                input_message = Interaction.InputBox(prompt, String.Format("StellarRobo Ver.{0}", asm.GetName().Version));
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return "".AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return input_message.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult InputKeys(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            List<InputSimulator.Input> inputs = new List<InputSimulator.Input>();
            string text = string.Empty;
            string waitTimeTmp = string.Empty;
            int waitTime = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if(args.Length == 1||args.Length==2) { 
                //出力文字列取得
                text = args[0].ToString().Trim();

                //Wait時間は設定されているか？
                if(args.Length==2)
                {
                    waitTimeTmp = args[1].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            if (args.Length == 2)
            {
                //Wait時間は入力されているか？
                if (string.IsNullOrEmpty(waitTimeTmp))
                {
                    //未入力なのでエラーとする
                    SetErrorInfo((int)RESULT_CODE.InputWaitTimeNotEnteredError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                //変換出来ない場合にはエラーとする
                if(!int.TryParse(waitTimeTmp,out waitTime))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                if ((waitTime < 0) || (waitTime > MAX_TIME))
                {
                    SetErrorInfo((int)RESULT_CODE.InputWaitTimeOutOutOfRangeError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                
            }
            #endregion  

            try
            {
                for (int i = 0; i < text.Length; i++)
                {
                    //1文字取得
                    string char_data = text.Substring(i, 1);
                    inputs.Clear();

                    //キーボードデータ作成
                    InputSimulator.AddKeyboardInput(ref inputs, char_data);

                    //キーボード動作をエミュレートする
                    InputSimulator.SendInput(inputs);

                    //待つ
                    Thread.Sleep(waitTime);
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult InputTextBox(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string HandleTmp = string.Empty;
            IntPtr Handle = IntPtr.Zero;
            string SearchWord = string.Empty;
            string SearchTypeTmp = string.Empty;
            string Message = string.Empty;
            int SearchType = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 3 || args.Length == 4)
            {
                //Handle取得
                HandleTmp = args[0].ToString();

                //SearchWord取得
                SearchWord = args[1].ToString().Trim();

                //入力メッセージ取得
                Message = args[2].ToString();

                //検索タイプ取得
                if (args.Length > 3)
                {
                    SearchTypeTmp = args[3].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //Windowハンドルは入力されているか？
            if (string.IsNullOrEmpty(HandleTmp))
            {
                //Windowハンドルが入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.InputWindowHandleInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //変換出来ない場合には強制的に0に変換
            Int64 tmp = 0;
            if (!Int64.TryParse(HandleTmp, out tmp)) { tmp = 0; }
            Handle = (IntPtr)tmp;

            //SearchWordは入力されているか？
            if (string.IsNullOrEmpty(SearchWord))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.InputSearchWordInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //検索タイプは入力されているか？
            if (string.IsNullOrEmpty(SearchTypeTmp))
            {
                //省略されているならば、IDで検索を行う
                SearchType = 0;
            }
            else
            {
                //数値入力か？
                if (!int.TryParse(SearchTypeTmp, out SearchType))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                //範囲内か？
                if (!(SearchType >= 0 && SearchType < (int)ControlSearchType.End))
                {
                    SetErrorInfo((int)RESULT_CODE.InputSearchTypeOutOfRangeError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            #endregion

            try
            {
                //検索条件を指定しControlを取得する
                AutomationElementCollection elementCollection = SearchControl(Handle, SearchType, SearchWord);

                //取得出来たか？
                if (elementCollection.Count > 0)
                {
                    AutomationElement element = elementCollection[0];

                    switch(element.Current.ControlType)
                    {
                        //TextBox
                        case ControlType controlType when controlType == ControlType.Edit:
                            object textBoxPattern;
                            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out textBoxPattern))
                            {
                                ((ValuePattern)textBoxPattern).SetValue(Message);
                            }
                            break;
                    }
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult OutputTextBox(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string HandleTmp = string.Empty;
            IntPtr Handle = IntPtr.Zero;
            string SearchWord = string.Empty;
            string SearchTypeTmp = string.Empty;
            string Message = string.Empty;
            int SearchType = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 3 || args.Length == 4)
            {
                //Handle取得
                HandleTmp = args[0].ToString();

                //SearchWord取得
                SearchWord = args[1].ToString().Trim();

                //検索タイプ取得
                if (args.Length > 2)
                {
                    SearchTypeTmp = args[2].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            #region 入力チェック
            //Windowハンドルは入力されているか？
            if (string.IsNullOrEmpty(HandleTmp))
            {
                //Windowハンドルが入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.InputWindowHandleInputNotEnteredError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            //変換出来ない場合には強制的に0に変換
            Int64 tmp = 0;
            if (!Int64.TryParse(HandleTmp, out tmp)) { tmp = 0; }
            Handle = (IntPtr)tmp;

            //SearchWordは入力されているか？
            if (string.IsNullOrEmpty(SearchWord))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.InputSearchWordInputNotEnteredError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            //検索タイプは入力されているか？
            if (string.IsNullOrEmpty(SearchTypeTmp))
            {
                //省略されているならば、IDで検索を行う
                SearchType = 0;
            }
            else
            {
                //数値入力か？
                if (!int.TryParse(SearchTypeTmp, out SearchType))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return string.Empty.AsStellarRoboString().NoResume();
                }
                //範囲内か？
                if (!(SearchType >= 0 && SearchType < (int)ControlSearchType.End))
                {
                    SetErrorInfo((int)RESULT_CODE.InputSearchTypeOutOfRangeError);
                    return string.Empty.AsStellarRoboString().NoResume();
                }
            }
            #endregion

            try
            {
                //検索条件を指定しControlを取得する
                AutomationElementCollection elementCollection = SearchControl(Handle, SearchType, SearchWord);

                //取得出来たか？
                if (elementCollection.Count > 0)
                {
                    AutomationElement element = elementCollection[0];

                    switch (element.Current.ControlType)
                    {
                        //TextBox
                        case ControlType controlType when controlType == ControlType.Edit:
                            object textBoxPattern;
                            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out textBoxPattern))
                            {
                                Message = ((ValuePattern)textBoxPattern).Current.Value;
                            }
                            break;
                    }
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return Message.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult SendKeys(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string send_data = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 1)
            {
                //送信文字列取得
                send_data = args[0].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            //送信文字列が空ならば、何も送信せずに終了する
            if(send_data.Trim() == string.Empty)
            {
                return true.AsStellarRoboBoolean().NoResume();
            }

            try
            {
                //キー情報送信
                //Spaceキー
                if(Regex.IsMatch(send_data, @"\{space\}|\{space \d*\}", RegexOptions.IgnoreCase))
                {
                    //繰り返し数を取得する
                    int RepetitionCount = 0;
                    if (!int.TryParse(Regex.Replace(send_data, @"[^0-9]", ""), out RepetitionCount)) { RepetitionCount = 1; }

                    //{Space}キーを送る
                    for (int i = 0; i < RepetitionCount; i++)
                    {
                        keybd_event(SPACE_KEY, 0, 0, (UIntPtr)0);
                        keybd_event(SPACE_KEY, 0, 2, (UIntPtr)0);
                    }
                }
                //以外
                else
                {
                    //通常のSend_keys
                    System.Windows.Forms.SendKeys.SendWait(send_data);
                }
            }
            catch(Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult Wait(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string sleepTimeTmp = string.Empty;
            int sleepTime = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if(args.Length==1)
            {
                //Sleep時間を取得
                sleepTimeTmp = args[0].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //SleepTime時間は入力されているか？
            if (string.IsNullOrEmpty(sleepTimeTmp))
            {
                //未入力なのでエラーとする
                SetErrorInfo((int)RESULT_CODE.InputSleepTimeNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //変換出来ない場合にはエラーとする
            if (!int.TryParse(sleepTimeTmp, out sleepTime))
            {
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            if ((sleepTime < 0) || (sleepTime > MAX_TIME))
            {
                SetErrorInfo((int)RESULT_CODE.InputSleepTimeOutOutOfRangeError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            #endregion  

            try
            {
                //Sleep
                Thread.Sleep(sleepTime);
            }
            catch(Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        #endregion

        #region アプリケーション関連
        private static StellarRoboFunctionResult AppOpen(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string[] filePath;
            ProcessStartInfo psi = new ProcessStartInfo();
            string windowStyleTmp = string.Empty;
            int windowStyle = 0;
            string exitsWaitTmp = string.Empty;
            int exitsWaitState = 0;
            bool exitsWait = false;
            string exitsWaitTimeTmp = string.Empty;
            int exitsWaitTime = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length >= 1 && args.Length <= 4)
            {
                //起動情報取得
                filePath = args[0].ToString().Trim().Split('|');

                //ウィインドサイズは指定されているか？
                if (args.Length >= 2)
                {
                    windowStyleTmp = args[1].ToString().Trim();
                }

                //起動されたアプリが終了時まで待つか？
                if (args.Length >= 3)
                {
                    exitsWaitTmp = args[2].ToString().Trim();
                }

                //最大待機時間は設定されているか？
                if (args.Length == 4)
                {
                    exitsWaitTimeTmp = args[3].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return (-1).AsStellarRoboInteger().NoResume();
            }

            #region 入力チェック
            //ファイルは入力されているか？
            if (string.IsNullOrEmpty(filePath[0]))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ApplicationExecutionFileInputNotEnteredError);
                return (-1).AsStellarRoboInteger().NoResume();
            }
            //ファイルは存在しているか？
            if (!File.Exists(filePath[0]))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ApplicationExecutionFileExistsError);
                return (-1).AsStellarRoboInteger().NoResume();
            }

            if (args.Length >= 2)
            {
                //WindowSizeは入力されているか？
                if (string.IsNullOrEmpty(windowStyleTmp))
                {
                    //WindowSizeが入力されていないのでエラー
                    SetErrorInfo((int)RESULT_CODE.ApplicationWindowStyleInputNotEnteredError);
                    return (-1).AsStellarRoboInteger().NoResume();
                }
                //変換出来ない場合にはエラーとする
                if (!int.TryParse(windowStyleTmp, out windowStyle))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return (-1).AsStellarRoboInteger().NoResume();
                }

                switch (windowStyle)
                {
                    //通常起動
                    case 0:
                        psi.WindowStyle = ProcessWindowStyle.Normal;
                        break;
                    //最小化起動
                    case 1:
                        psi.WindowStyle = ProcessWindowStyle.Minimized;
                        break;
                    //最大化起動
                    case 2:
                        psi.WindowStyle = ProcessWindowStyle.Maximized;
                        break;
                    //上記以外の指定は全て通常起動とする
                    default:
                        psi.WindowStyle = ProcessWindowStyle.Normal;
                        break;
                }

            }
            if (args.Length >= 3)
            {
                //起動されたアプリが終了時間まで待つかが入力されているか？
                if (string.IsNullOrEmpty(exitsWaitTmp))
                {
                    //未入力なのでエラーとする
                    SetErrorInfo((int)RESULT_CODE.ApplicationExistsWaitInputNotEnteredError);
                    return (-1).AsStellarRoboInteger().NoResume();
                }
                //変換出来ない場合にエラーとする
                if(!int.TryParse(exitsWaitTmp,out exitsWaitState))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return (-1).AsStellarRoboInteger().NoResume();
                }
                exitsWait = (exitsWaitState == 0) ? false : true;
            }
            if (args.Length == 4)
            {
                //最大待機時間は入力されているか？
                if (string.IsNullOrEmpty(exitsWaitTimeTmp))
                {
                    //最大待機時間が未入力なのでエラーとする
                    SetErrorInfo((int)RESULT_CODE.ApplicationExistsWaitTimeInputNotEnteredError);
                    return (-1).AsStellarRoboInteger().NoResume();
                }
                //変換出来ない場合にはエラーとする
                if(!int.TryParse(exitsWaitTimeTmp,out exitsWaitTime))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return (-1).AsStellarRoboInteger().NoResume();
                }
                if((exitsWaitTime < 0) || (exitsWaitTime > MAX_TIME))
                {
                    SetErrorInfo((int)RESULT_CODE.ApplicationExistsWaitTimeOutOfRangeError);
                    return (-1).AsStellarRoboInteger().NoResume();
                }
            }
            #endregion  

            try
            {
                //指定アプリを起動する
                psi.FileName = filePath[0];

                //引数は設定されているか？
                if(filePath.Length > 1)
                {
                    string arg = string.Empty;
                    for(int i = 1;i<filePath.Length; i++)
                    {
                        arg += filePath[i] + " ";
                    }
                    psi.Arguments = arg;
                }
                Process process = Process.Start(psi);

                //終了待機は行う？
                if (exitsWait)
                {
                    //最大待機時間は指定されているか？
                    if (exitsWaitTime == 0)
                    {
                        //指定されていないので無制限に待ち続ける
                        process.WaitForExit();

                        //終了コードを指定して終了
                        int exitCode = process.ExitCode;
                        return exitCode.AsStellarRoboInteger().NoResume();
                    }
                    else
                    {
                        //指定時間を過ぎれば、終了如何を問わず処理を終了する
                        process.WaitForExit(exitsWaitTime);

                        //タイムアウトで終了したか？
                        if (!process.HasExited)
                        {
                            //タイムアウトで終了した事を返す
                            SetErrorInfo((int)RESULT_CODE.TimeOut);
                            return 0.AsStellarRoboInteger().NoResume();
                        }
                    }
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return(-1).AsStellarRoboInteger().NoResume();
            }

            //戻り値設定
            return 0.AsStellarRoboInteger().NoResume();
        }
        private static StellarRoboFunctionResult AppWait(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string WindowName = string.Empty;
            string waitStateTmp = string.Empty;
            int waitStateValue = 0;
            bool waitState = true;
            string timeOutTmp = string.Empty;
            int timeOut = 0;
            IntPtr Handle;
            bool timeOutFlg = false;
            System.Threading.Timer timer;

            //経過確認用
            void checkTime(object arg)
            {
                timeOutFlg = true;
            }

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 2 || args.Length == 3)
            {
                //WindowName取得
                WindowName = args[0].ToString().Trim();

                //待ち状態取得
                waitStateTmp = args[1].ToString().Trim();

                //TimeOut時間は設定されているか？
                if (args.Length == 3)
                {
                    timeOutTmp = args[2].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //WindowNameは入力されているか？
            if (string.IsNullOrEmpty(WindowName))
            {
                //WindowNameが入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationWindowNameInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //待ち状態は入力されているか？
            if (string.IsNullOrEmpty(waitStateTmp))
            {
                //待ち状態が入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationWaitStateInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //変換出来ない場合にはエラーとする
            if (!int.TryParse(waitStateTmp,out waitStateValue))
            {
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            waitState = (waitStateValue == 0) ? true : false;
            if (args.Length == 3)
            {
                //TimeOutが入力されているか？
                if (string.IsNullOrEmpty(timeOutTmp))
                {
                    //未入力なのでエラーとする
                    SetErrorInfo((int)RESULT_CODE.ApplicationTimeOutInputNotEnteredError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                //変換出来ない場合にはエラーとする
                if (!int.TryParse(timeOutTmp, out timeOut))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                if ((timeOut < 0) || (timeOut > MAX_TIME))
                {
                    SetErrorInfo((int)RESULT_CODE.ApplicationTimeOutTimeOutOfRangeError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            #endregion

            //タイマ処理生成
            timer = new System.Threading.Timer(new TimerCallback(checkTime));

            //タイムアウト時間が設定されていたら、タイマ処理を行う
            if (timeOut != 0)
            {
                timer.Change(timeOut, timeOut);
            }

            try
            {
                while (true)
                {
                    //タイムアウト事案は設定されているか？
                    if(timeOut != 0)
                    {
                        //フラグが立っていたら抜ける
                        if (timeOutFlg) { break; }
                    }

                    //対象を取得する
                    Handle = FindWindow(null, WindowName);

                    //状態は？
                    if (waitState)
                    {
                        //起動したら抜ける
                        if (((int)Handle) != 0) break;
                    }
                    else
                    {
                        //閉じたら抜ける
                        if (((int)Handle) == 0) break;
                    }

                    //応答無い対処
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(1);
                }

            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }
            finally
            {
                //タイムアウト時間が設定されていたらタイマ処理を破棄する
                if (timeOut != 0)
                {
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                }
                if (timeOutFlg)
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.TimeOut);
                }
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult AppWindowEnable(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            bool state = false;
            string timeOutTmp = string.Empty;
            int timeOut = 0;
            bool timeOutFlg = false;
            string HandleTmp = string.Empty;
            IntPtr Handle;
            System.Threading.Timer timer;

            //経過確認用
            void checkTime(object arg)
            {
                timeOutFlg = true;
            }

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 1 || args.Length == 2)
            {
                //Windowハンドル取得
                HandleTmp = args[0].ToString().Trim();

                //TimeOut時間は設定されているか？
                if(args.Length==2)
                {
                    timeOutTmp = args[1].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //Windowハンドルは入力されているか？
            if (string.IsNullOrEmpty(HandleTmp))
            {
                //Windowハンドルが入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationWindowHandleInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //変換出来ない場合には強制的に0に変換
            Int64 tmp = 0;
            if(!Int64.TryParse(HandleTmp, out tmp)) { tmp = 0; }
            Handle = (IntPtr)tmp;
            //TimeOutは入力されているか？
            if (string.IsNullOrEmpty(timeOutTmp))
            {
                //待ち状態が入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationTimeOutInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //変換出来ない場合には強制的に0に変換
            if (!int.TryParse(timeOutTmp, out timeOut)) { timeOut = 0; }
            #endregion  

            try
            {
                //タイマ処理生成
                timer = new System.Threading.Timer(new TimerCallback(checkTime));

                //タイムアウト時間が設定されていたら、タイマ処理を行う
                if (timeOut != 0)
                {
                    timer.Change(timeOut, timeOut);
                }

                while (true)
                {
                    //タイムアウト時間は設定されているか？
                    if (timeOut != 0)
                    {
                        //フラグが立っていたら抜ける
                        if (timeOutFlg) { break; }
                    }

                    //対象の状態を取得する
                    state = IsWindowEnabled(Handle);

                    //状態は？
                    if (state)
                    {
                        //使用出来る状態になったので、抜ける
                        break;
                    }

                    //応答無い対応
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(1);
                }

                //タイムアウト時間が設定されていたらタイマ処理を破棄する
                if (timeOut != 0)
                {
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                }
                if (timeOutFlg)
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.TimeOut);
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult AppActive(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string ClassName = string.Empty;
            string WindowName = string.Empty;
            IntPtr Handle;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 2)
            {
                //ClassName取得
                ClassName = args[0].ToString().Trim();
                if (ClassName.Trim() == string.Empty)
                {
                    ClassName = null;
                }

                //WindowName取得
                WindowName = args[1].ToString().Trim();
                if (WindowName.Trim() == string.Empty)
                {
                    WindowName = null;
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //ClassName若しくは、WindowNameのどちらかが入力されているか？
            if (string.IsNullOrEmpty(ClassName) && string.IsNullOrEmpty(WindowName))
            {
                //ClassName及び、WindowNameが未入力なのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationEnterEitherError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            #endregion  

            try
            {
                //対象となるWindowを探す
                Handle = FindWindow(ClassName, WindowName);
                if ((int)Handle == 0)
                {
                    //対象が見つからないのなら終了する
                    SetErrorInfo((int)RESULT_CODE.ApplicationNotFoundError);
                    return false.AsStellarRoboBoolean().NoResume();
                }

                //対象をアクティブにする
                //SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, SetWindowPosFlags.IgnoreMove | SetWindowPosFlags.IgnoreResize | SetWindowPosFlags.ShowWindow);
                //SetWindowPos(Handle, HWND_NOTOPMOST, 0, 0, 0, 0, SetWindowPosFlags.IgnoreMove | SetWindowPosFlags.IgnoreResize);
                SetForegroundWindow(Handle);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult AppSetPos(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string ClassName = string.Empty;
            string WindowName = string.Empty;
            string LeftTmp = string.Empty;
            string TopTmp = string.Empty;
            string RightTmp = string.Empty;
            string BottomTmp = string.Empty;
            int Left = 0;
            int Top = 0;
            int Right = 0;
            int Bottom = 0;
            IntPtr Handle;
            uint flags;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if(args.Length==4 || args.Length==6)
            {
                //ClassName取得
                ClassName = args[0].ToString().Trim();
                if(ClassName.Trim() == string.Empty)
                {
                    ClassName = null;
                }

                //WindowName取得
                WindowName = args[1].ToString().Trim();
                if (WindowName.Trim() == string.Empty)
                {
                    WindowName = null;
                }

                //Left位置取得
                LeftTmp = args[2].ToString().Trim();

                //Top位置取得
                TopTmp = args[3].ToString().Trim();

                //引数にRight及びBottomは指定されているか？
                if(args.Length==6)
                {
                    //Right位置取得
                    RightTmp = args[4].ToString().Trim();

                    //Bottom位置取得
                    BottomTmp = args[5].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //ClassName若しくは、WindowNameのどちらかが入力されているか？
            if(string.IsNullOrEmpty(ClassName) && string.IsNullOrEmpty(WindowName))
            {
                //ClassName及び、WindowNameが未入力なのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationEnterEitherError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //Left位置は入力されているか？
            if (string.IsNullOrEmpty(LeftTmp))
            {
                //Left位置が入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationLeftPositionInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //変換出来ない場合にはエラーとする
            if (!int.TryParse(LeftTmp, out Left))
            {
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //Top位置は入力されているか？
            if (string.IsNullOrEmpty(TopTmp))
            {
                //Left位置が入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationTopPositionInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //変換出来ない場合にはエラーとする
            if (!int.TryParse(TopTmp, out Top))
            {
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return false.AsStellarRoboBoolean().NoResume();
            }

            if (args.Length == 6)
            {
                //Right位置は入力されているか？
                if (string.IsNullOrEmpty(RightTmp))
                {
                    //Right位置が入力されていないのでエラー
                    SetErrorInfo((int)RESULT_CODE.ApplicationRightPositionInputNotEnteredError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                //変換出来ない場合にはエラーとする
                if (!int.TryParse(RightTmp, out Right))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                //Bottom位置は入力されているか？
                if (string.IsNullOrEmpty(BottomTmp))
                {
                    //Bottom位置が入力されていないのでエラー
                    SetErrorInfo((int)RESULT_CODE.ApplicationBottomPositionInputNotEnteredError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                //変換出来ない場合にはエラーとする
                if (!int.TryParse(BottomTmp, out Bottom))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            //RightもしくはBottomが0ならば、Windowサイズは変更させない
            if (Right == 0 || Bottom == 0)
            {
                flags = (uint)(SetWindowPosFlags.IgnoreZOrder | SetWindowPosFlags.IgnoreResize);
            }
            else
            {
                flags = (uint)SetWindowPosFlags.IgnoreZOrder;
            }
            #endregion  

            try
            {
                //対象となるWindowを検索する
                Handle = FindWindow(ClassName, WindowName);
                if ((int)Handle == 0)
                {
                    //対象が見つからないのなら終了する
                    SetErrorInfo((int)RESULT_CODE.ApplicationNotFoundError);
                    return false.AsStellarRoboBoolean().NoResume();
                }

                //対象Windowを移動させる
                SetWindowPos(Handle, HWND_TOP, Left, Top, Right, Bottom, (SetWindowPosFlags)flags);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult AppClose(StellarRoboContext context, StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string WindowName = string.Empty;
            IntPtr Handle;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if(args.Length==1)
            {
                //WindowName取得
                WindowName = args[0].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //WindowNameは入力されているか？
            if (string.IsNullOrEmpty(WindowName))
            {
                //WindowNameが入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationWindowNameInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            #endregion  

            try
            {
                //対象となるWindowを検索する
                Handle = FindWindow(null, WindowName);
                if ((int)Handle != 0)
                {
                    //終了コマンドを送信
                    SendMessage(Handle, WM_CLOSE, (int)0, IntPtr.Zero);
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult GetDeskTopPath(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string deskTopPath = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length != 0)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            try
            {
                //デスクトップのパスを取得
                deskTopPath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return deskTopPath.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult GetMyDocumentPath(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string myDocument = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length != 0)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            try
            {
                //MyDocumnetのパスを取得
                myDocument = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return myDocument.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult GetMyPicturePath(StellarRoboContext context, StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string myPicture = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length != 0)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            try
            {
                //MyPictureのパスを取得
                myPicture = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return myPicture.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult SetWindowState(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //戻り値設定
            return StellarRoboNil.Instance.NoResume();
        }
        private static StellarRoboFunctionResult SetWindowSize(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //戻り値設定
            return StellarRoboNil.Instance.NoResume();
        }
        private static StellarRoboFunctionResult GetWindowHandle(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string ClassName = string.Empty;
            string WindowName = string.Empty;
            IntPtr Handle;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 1 || args.Length == 2)
            {
                //クラス名取得
                ClassName = args[0].ToString().Trim();
                //クラス名が空ならばNullを設定
                if (string.IsNullOrEmpty(ClassName)) { ClassName = null; }
                if(args.Length==2)
                {
                    //ウィンド名取得
                    WindowName = args[1].ToString().Trim();
                    //ウィンド名がからならばNullを設定
                    if (string.IsNullOrEmpty(WindowName)) { WindowName = null; }
                }
                else
                {
                    WindowName = null;
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return 0.AsStellarRoboInteger().NoResume();
            }

            #region 入力チェック
            //ClassName若しくは、WindowNameのどちらかが入力されているか？
            if (string.IsNullOrEmpty(ClassName) && string.IsNullOrEmpty(WindowName))
            {
                //ClassName及び、WindowNameが未入力なのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationEnterEitherError);
                return 0.AsStellarRoboInteger().NoResume();
            }
            #endregion  

            try
            {
                //Window検索
                Handle = FindWindow(ClassName, WindowName);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return 0.AsStellarRoboInteger().NoResume();
            }

            //戻り値設定
            return (Handle.ToInt64()).AsStellarRoboInteger().NoResume();
        }
        private static StellarRoboFunctionResult GetChildWindowHandle(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            Int64 parentHandle;
            string className = string.Empty;
            string windowName = string.Empty;
            string HandleTmp = string.Empty;
            IntPtr Handle;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if(args.Length==3)
            {
                //親Handle取得
                HandleTmp = args[0].ToString().Trim();

                //クラス名取得
                className = args[1].ToString().Trim();
                if (string.IsNullOrEmpty(className)) { className = null; }

                //ウィンド名取得
                windowName = args[2].ToString().Trim();
                if (string.IsNullOrEmpty(windowName)) { windowName = null; }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return 0.AsStellarRoboInteger().NoResume();
            }

            #region 入力チェック
            //親Handleは入力されているか？
            if (string.IsNullOrEmpty(HandleTmp))
            {
                //親Handleが入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationParentHandleInputNotEnteredError);
                return 0.AsStellarRoboInteger().NoResume();
            }
            //変換出来ない場合には強制的に0に変換
            if(!Int64.TryParse(HandleTmp,out parentHandle)) { parentHandle = 0; }
            #endregion

            try
            {
                //子Window検索
                Handle = FindWindowEx((IntPtr)parentHandle, (IntPtr)0, className, windowName);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return 0.AsStellarRoboInteger().NoResume();
            }

            //戻り値設定
            return (Handle.ToInt64()).AsStellarRoboInteger().NoResume();
        }
        private static StellarRoboFunctionResult GetWindowHandleActive(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            IntPtr Handle;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length != 0)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return 0.AsStellarRoboInteger().NoResume();
            }

            try
            {
                //アクティブなウィンド・ハンドルを取得する
                Handle = GetActiveWindow();
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return 0.AsStellarRoboInteger().NoResume();
            }

            //戻り値設定
            return (Handle.ToInt64()).AsStellarRoboInteger().NoResume();
        }
        private static StellarRoboFunctionResult GetWindowHandlePoint(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            POINT cpt;                  //マウスカーソル座標用
            System.Drawing.Point pt = new System.Drawing.Point(0, 0); //座標用
            IntPtr Handle;              //ハンドル用

            //引数チェック
            if (args.Length != 0)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return 0.AsStellarRoboInteger().NoResume();
            }

            try
            {
                //現在のマウスカーソル座標を取得する
                if (GetCursorPos(out cpt))
                {
                    //座標設定
                    pt.X = cpt.X;
                    pt.Y = cpt.Y;
                }
                else
                {
                    //取得に失敗したら左上の座標を設定
                    pt.X = 1;
                    pt.Y = 1;
                }
                //座標を指定してウィンドウ・ハンドルを取得
                Handle = WindowFromPoint(pt);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return 0.AsStellarRoboInteger().NoResume();
            }

            //戻り値設定
            return (Handle.ToInt64()).AsStellarRoboInteger().NoResume();
        }
        private static StellarRoboFunctionResult GetWindowHandleParent(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string HandleTmp = string.Empty;
            IntPtr Handle;
            IntPtr resultHandle = IntPtr.Zero;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if(args.Length==1)
            {
                //ハンドルを取得する
                HandleTmp = args[0].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return 0.AsStellarRoboInteger().NoResume();
            }

            #region 入力チェック
            //Handleは入力されているか？
            if (string.IsNullOrEmpty(HandleTmp))
            {
                //Handleが入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationHandleInputNotEnteredError);
                return 0.AsStellarRoboInteger().NoResume();
            }
            //変換出来ない場合には強制的に0に変換
            Int64 tmp = 0;
            if (!Int64.TryParse(HandleTmp, out tmp)) { tmp = 0; }
            Handle = (IntPtr)tmp;
            #endregion

            try
            {
                //引数のハンドルを元にし、Ownerのハンドルを取得する
                resultHandle = GetWindowLong(Handle, (int)GWL.GWL_HWNDPARENT);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return 0.AsStellarRoboInteger().NoResume();
            }

            //戻り値設定
            return (resultHandle == IntPtr.Zero) ? 0.AsStellarRoboInteger().NoResume() : resultHandle.ToInt64().AsStellarRoboInteger().NoResume();
        }
        private static StellarRoboFunctionResult GetWindowHandleTop(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //戻り値設定
            return StellarRoboNil.Instance.NoResume();
        }
        private static StellarRoboFunctionResult GetWindowHandleFocus(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            IntPtr Handle;              //ウィンド・ハンドル用

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length != 0)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return 0.AsStellarRoboInteger().NoResume();
            }

            try
            {
                //フォーカスを持つウィンド・ハンドルを取得
                Handle = GetFocus();
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return 0.AsStellarRoboInteger().NoResume();
            }

            //戻り値設定
            return (Handle.ToInt64()).AsStellarRoboInteger().NoResume();
        }
        private static StellarRoboFunctionResult IsWindowVisible(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string HandleTmp = string.Empty;
            IntPtr Handle;              //ウィンド・ハンドル用
            bool visibleState;          //ウィンドVisible判定用

            //引数チェック
            if(args.Length==1)
            {
                //ウィンド・ハンドルを取得
                HandleTmp = args[0].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //Handleは入力されているか？
            if (string.IsNullOrEmpty(HandleTmp))
            {
                //Handleが入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationHandleInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //変換出来ない場合には強制的に0に変換
            Int64 tmp = 0;
            if (!Int64.TryParse(HandleTmp, out tmp)) { tmp = 0; }
            Handle = (IntPtr)tmp;
            #endregion

            try
            {
                //ウィンドVisibleの状態を取得する
                visibleState = IsWindowVisible(Handle);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return visibleState.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult GetWindowClass(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            StringBuilder classNameBuffer = new StringBuilder("", GET_CLASSNAME_DEFAULT_MAX_COUNT);
            string className = string.Empty;
            string HandleTmp = string.Empty;
            IntPtr Handle;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //変数チェック
            if(args.Length==1)
            {
                //ハンドル取得
                HandleTmp = args[0].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            #region 入力チェック
            //Handleは入力されているか？
            if (string.IsNullOrEmpty(HandleTmp))
            {
                //Handleが入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationHandleInputNotEnteredError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            //変換出来ない場合には強制的に0に変換
            Int64 tmp = 0;
            if (!Int64.TryParse(HandleTmp, out tmp)) { tmp = 0; }
            Handle = (IntPtr)tmp;
            #endregion

            try
            {
                //クラス名取得
                if (GetClassName(Handle, classNameBuffer, GET_CLASSNAME_DEFAULT_MAX_COUNT) != 0)
                {
                    className = classNameBuffer.ToString();
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();

            }

            //戻り値設定
            return className.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult GetWindowText(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string HandleTmp = string.Empty;
            IntPtr Handle;                      //ウィンド・ハンドル用
            StringBuilder windowTextBuffer = new StringBuilder("", GET_WINDOW_TEXT_DEFAULT_MAX_COUNT);
            string windowText = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if(args.Length==1)
            {
                //ウィンド・ハンドルを取得
                HandleTmp = args[0].ToString().Trim();
            }else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            #region 入力チェック
            //Handleは入力されているか？
            if (string.IsNullOrEmpty(HandleTmp))
            {
                //Handleが入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationHandleInputNotEnteredError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            //変換出来ない場合には強制的に0に変換
            Int64 tmp = 0;
            if (!Int64.TryParse(HandleTmp, out tmp)) { tmp = 0; }
            Handle = (IntPtr)tmp;
            #endregion

            try
            {
                //ウィンドのTextを取得する
                if (GetWindowText(Handle, windowTextBuffer, GET_WINDOW_TEXT_DEFAULT_MAX_COUNT) != 0)
                {
                    windowText = windowTextBuffer.ToString();
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return windowText.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult GetWindowRect(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string HandleTmp = string.Empty;
            IntPtr Handle = IntPtr.Zero;
            RECT windowRectBuffer;
            StellarRoboArray windowRect = new StellarRoboArray(new int[] { 0 });

            //引数チェック
            if (args.Length == 1)
            {
                HandleTmp = args[0].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return StellarRoboNil.Instance.NoResume();
            }

            #region 入力チェック
            //Handleは入力されているか？
            if (string.IsNullOrEmpty(HandleTmp))
            {
                //Handleが入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationHandleInputNotEnteredError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            //変換出来ない場合には強制的に0に変換
            Int64 tmp = 0;
            if (!Int64.TryParse(HandleTmp, out tmp)) { tmp = 0; }
            Handle = (IntPtr)tmp;
            #endregion

            try
            {
                //ウィンドの位置情報取得
                if (GetWindowRect(Handle, out windowRectBuffer))
                {
                    windowRect = new StellarRoboArray(new int[] { windowRectBuffer.Left, windowRectBuffer.Top, windowRectBuffer.Right, windowRectBuffer.Bottom });
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return StellarRoboNil.Instance.NoResume();
            }

            //戻り値設定
            return windowRect.NoResume();
        }
        private static StellarRoboFunctionResult SetWindowPosZ(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string HandleTmp = string.Empty;
            IntPtr Handle;
            string InsertAfterTmp = string.Empty;
            IntPtr InsertAfter;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if(args.Length==2)
            {
                //ウィンド・ハンドル取得
                HandleTmp = args[0].ToString().Trim();

                //Z-Order取得
                InsertAfterTmp = args[1].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //Handleは入力されているか？
            if (string.IsNullOrEmpty(HandleTmp))
            {
                //Handleが入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationHandleInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //変換出来ない場合にはエラーとする
            Int64 tmp = 0;
            if (!Int64.TryParse(HandleTmp, out tmp))
            {
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            Handle = (IntPtr)tmp;
            //Z-Orderは入力されているか？
            if (string.IsNullOrEmpty(InsertAfterTmp))
            {
                //Z-Orderが入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationZOrderInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //変換出来ない場合にはエラーとする
            int tmp2 = 0;
            if(!int.TryParse(InsertAfterTmp,out tmp2))
            {
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            switch (tmp2)
            {
                //Top
                case 1:
                    InsertAfter = HWND_TOPMOST;
                    break;
                //Bottom
                case 2:
                    InsertAfter = HWND_BOTTOM;
                    break;
                //それ以外はHWND_TOPにする
                default:
                    InsertAfter = HWND_TOPMOST;
                    break;
            }
            #endregion

            try
            {
                //HWND_BOTTOMが選択されていたならば、HWND_NOTOPMOSTを発行してからHWND_BOTTOMを発行する
                if (InsertAfter == HWND_BOTTOM)
                {
                    SetWindowPos(Handle, HWND_NOTOPMOST, 0, 0, 0, 0, SetWindowPosFlags.IgnoreMove | SetWindowPosFlags.IgnoreResize);
                }

                //ウィンドのZ-Orderを設定する
                SetWindowPos(Handle, InsertAfter, 0, 0, 0, 0, SetWindowPosFlags.IgnoreMove | SetWindowPosFlags.IgnoreResize);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
         private static StellarRoboFunctionResult SetWindowTrans(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string HandleTmp = string.Empty;
            IntPtr Handle;
            string transValueTmp = string.Empty;
            IntPtr transValue;
            IntPtr windowState;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 2)
            {
                //ウィンド・ハンドル取得
                HandleTmp = args[0].ToString().Trim();

                //透過度取得
                transValueTmp = args[1].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //Handleは入力されているか？
            if (string.IsNullOrEmpty(HandleTmp))
            {
                //Handleが入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationHandleInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //変換出来ない場合には強制的に0に変換
            Int64 tmp = 0;
            if (!Int64.TryParse(HandleTmp, out tmp)) { tmp = 0; }
            Handle = (IntPtr)tmp;
            //透過度は入力されているか？
            if (string.IsNullOrEmpty(transValueTmp))
            {
                //透過度が入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationPermeabilityNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //変換出来ない場合には強制的に100に変換
            int tmp2 = 0;
            if (!int.TryParse(transValueTmp, out tmp2)) { tmp2 = 100; }
            //マイナスの値や、100を超える場合には0若しくは100に置き換える
            if (tmp2 < 0) { tmp2 = 0; }
            if (tmp2 > 100) { tmp2 = 100; }
            transValue = (IntPtr)tmp2;
            #endregion

            try
            {
                //ウィンド情報取得
                windowState = GetWindowLong(Handle, ((int)GWL.GWL_EXSTYLE | WS_EX_TRANSPARENT));

                if ((int)windowState != 0)
                {
                    //ウィンド情報設定
                    SetWindowLong(Handle, ((int)GWL.GWL_EXSTYLE | WS_EX_TRANSPARENT), transValue);
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult SetWindowTransColor()
        {
            //戻り値設定
            return StellarRoboNil.Instance.NoResume();
        }
        private static StellarRoboFunctionResult GetParentTitle(StellarRoboContext context, StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string xTmp = string.Empty;
            string yTmp = string.Empty;
            int x = 0;
            int y = 0;
            string title = string.Empty;
            StringBuilder windowTextBuffer = new StringBuilder("", GET_WINDOW_TEXT_DEFAULT_MAX_COUNT);
            Stack<IntPtr> handle = new Stack<IntPtr>();
            IntPtr nowHandle;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 2)
            {
                //座標を取得
                xTmp = args[0].ToString().Trim();
                yTmp = args[1].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            #region 入力チェック
            //X座標は入力されているか？
            if (string.IsNullOrEmpty(xTmp))
            {
                //X座標が入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationCoordinateNotEntererdError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            //変換出来ない場合にはエラー
            if (!int.TryParse(xTmp, out x))
            {
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            //Y座標は入力されているか？
            if (string.IsNullOrEmpty(yTmp))
            {
                //X座標が入力されていないのでエラー
                SetErrorInfo((int)RESULT_CODE.ApplicationCoordinateNotEntererdError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            //変換出来ない場合にはエラー
            if (!int.TryParse(yTmp, out y))
            {
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            #endregion

            try
            {
                //座標よりハンドルを取得する
                nowHandle = WindowFromPoint(new System.Drawing.Point(x, y));

                //ハンドルは取得出来たか？
                if (nowHandle.ToInt32() > 0)
                {
                    //ハンドルを保存する
                    handle.Push(nowHandle);

                    do
                    {
                        //親ハンドル取得
                        nowHandle = GetWindowLong(nowHandle, (int)GWL.GWL_HWNDPARENT);
                        //親は存在するか？
                        if (nowHandle.ToInt32() > 0)
                        {
                            //親ハンドルを保持する
                            handle.Push(nowHandle);
                        }
                        else
                        {
                            //ループを抜ける
                            break;
                        }
                    } while (nowHandle.ToInt32() > 0);

                    //直近のハンドルを取得
                    nowHandle = handle.Pop();

                    //タイトルを取得する
                    if (GetWindowText(nowHandle, windowTextBuffer, GET_WINDOW_TEXT_DEFAULT_MAX_COUNT) != 0)
                    {
                        title = windowTextBuffer.ToString();
                    }
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return title.AsStellarRoboString().NoResume();
        }
        #endregion

        #region OpenCV関連
        private static StellarRoboFunctionResult ImageMatching(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string filePath = string.Empty;
            string thresholdTmp = string.Empty;
            double threshold = 0.8;                         //閾値の初期値は0.8
            string x1Tmp = string.Empty;
            string y1Tmp = string.Empty;
            string x2Tmp = string.Empty;
            string y2Tmp = string.Empty;
            int x1 = 0;                                     //検索範囲矩形座標
            int y1 = 0;                                     //検索範囲矩形座標
            int x2 = Screen.PrimaryScreen.Bounds.Width;     //検索範囲矩形座標
            int y2 = Screen.PrimaryScreen.Bounds.Height;    //検索範囲矩形座標
            string grayScaleTmp = string.Empty;
            bool grayScale = false;                         //グレースケールで確認
            string matchPositionModeTmp = string.Empty;
            bool matchPositionMode = false;                 //検索結果を中心座標で返すか
            string timeOutTmp = string.Empty;
            int timeOut = 10000;                //10秒待つ
            bool timeOutFlg = false;
            System.Threading.Timer timer;
            int[] matchingPoint = new int[2] { 0, 0 };
            Mat template = new Mat();

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //経過確認用
            void checkTime(object arg)
            {
                timeOutFlg = true;
            }

            //引数チェック
            if (
                args.Length == 1 ||
                args.Length == 2 ||
                args.Length == 3 ||
                args.Length == 7 ||
                args.Length == 8 ||
                args.Length == 9)
            {
                //対象画像ファイルの情報を取得する
                filePath = args[0].ToString().Trim().Trim();

                //閾値は設定されているか？
                if (args.Length >= 2)
                {
                    //閾値を取得する
                    thresholdTmp = args[1].ToString().Trim();
                }

                //タイムアウト時間は指定されているか？
                if (args.Length >= 3)
                {
                    timeOutTmp = args[2].ToString().Trim();
                }

                //検索範囲が設定されているか？
                if (args.Length >= 7)
                {
                    x1Tmp = args[3].ToString().Trim();
                    y1Tmp = args[4].ToString().Trim();
                    x2Tmp = args[5].ToString().Trim();
                    y2Tmp = args[6].ToString().Trim();
                }

                //グレースケールで確認するか？
                if(args.Length >= 8)
                {
                    grayScaleTmp = args[7].ToString().Trim();
                }

                //結果は左上で返すか？
                if(args.Length == 9)
                {
                    matchPositionModeTmp = args[8].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return StellarRoboNil.Instance.NoResume();
            }

            #region 入力チェック
            //対象画像ファイルは入力されているか？
            if (string.IsNullOrEmpty(filePath))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.OpenCVComparisonFileInputNotEnteredError);
                return StellarRoboNil.Instance.NoResume();
            }
            //対象画像ファイルは存在するか？
            if (!File.Exists(filePath))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.OpenCVComparisonFileExistsError);
                return StellarRoboNil.Instance.NoResume();
            }
            if (args.Length >= 2)
            {
                //閾値は入力されてるか？
                if (string.IsNullOrEmpty(thresholdTmp))
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.OpenCVThresholdInputNotEnteredError);
                    return StellarRoboNil.Instance.NoResume();
                }
                //変換出来ない場合にはエラーとする
                if(!double.TryParse(thresholdTmp,out threshold))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return StellarRoboNil.Instance.NoResume();
                }
            }
            if (args.Length >= 3)
            {
                //タイムアウト時間は入力されているか？
                if (string.IsNullOrEmpty(timeOutTmp))
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.OpenCVTimeoutInputNotEnteredError);
                    return StellarRoboNil.Instance.NoResume();
                }
                //変換出来ない場合にはエラーとする
                if(!int.TryParse(timeOutTmp, out timeOut))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return StellarRoboNil.Instance.NoResume();
                }
            }
            if (args.Length >= 7)
            {
                //矩形指定は入力されているか？
                if (string.IsNullOrEmpty(x1Tmp))
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.OpenCVRectangleInputNotEnteredError);
                    return StellarRoboNil.Instance.NoResume();
                }
                if (string.IsNullOrEmpty(y1Tmp))
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.OpenCVRectangleInputNotEnteredError);
                    return StellarRoboNil.Instance.NoResume();
                }
                if (string.IsNullOrEmpty(x2Tmp))
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.OpenCVRectangleInputNotEnteredError);
                    return StellarRoboNil.Instance.NoResume();
                }
                if (string.IsNullOrEmpty(y2Tmp))
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.OpenCVRectangleInputNotEnteredError);
                    return StellarRoboNil.Instance.NoResume();
                }
                //変換出来ない場合にはエラーとする
                if(!int.TryParse(x1Tmp, out x1))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return StellarRoboNil.Instance.NoResume();
                }
                if(!int.TryParse(y1Tmp,out y1))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return StellarRoboNil.Instance.NoResume();
                }
                if (!int.TryParse(x2Tmp, out x2))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return StellarRoboNil.Instance.NoResume();
                }
                if(!int.TryParse(y2Tmp,out y2))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return StellarRoboNil.Instance.NoResume();
                }
                var screen = Screen.PrimaryScreen.Bounds;
                if (x2 == 0)
                {
                    x2 = screen.Width;
                }
                if (y2 == 0)
                {
                    y2 = screen.Height;
                }
            
                //領域指定は正しいか？
                if ((x2 - x1) <= 0)
                {
                    SetErrorInfo((int)RESULT_CODE.OpenCVAreaSpecificationError);
                    return StellarRoboNil.Instance.NoResume();
                }
                if ((y2 - y1) <= 0)
                {
                    SetErrorInfo((int)RESULT_CODE.OpenCVAreaSpecificationError);
                    return StellarRoboNil.Instance.NoResume();
                }
            }
            if (args.Length >= 8)
            {
                //グレースケールフラグは入力されているか？
                if (string.IsNullOrEmpty(grayScaleTmp))
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.OpenCVGrayScaleInputNotEnteredError);
                    return StellarRoboNil.Instance.NoResume();
                }
                //変換出来ない場合にはエラーとする
                if(!bool.TryParse(grayScaleTmp,out grayScale))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsBoolError);
                    return StellarRoboNil.Instance.NoResume();
                }
            }
            if(args.Length==9)
            {
                //座標返却方法フラグは入力されているか？
                if (string.IsNullOrEmpty(matchPositionModeTmp))
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.OpenCVMatchPositionModeInputNotEnteredError);
                    return StellarRoboNil.Instance.NoResume();
                }
                //変換出来ない場合にはエラーとする
                if(!bool.TryParse(matchPositionModeTmp, out matchPositionMode))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsBoolError);
                    return StellarRoboNil.Instance.NoResume();
                }
            }
            #endregion

            try
            {
                //タイマ処理生成
                timer = new System.Threading.Timer(new TimerCallback(checkTime));

                //タイムアウト時間が設定されていたらタイマ処理を行う
                if (timeOut != 0)
                {
                    timer.Change(timeOut, timeOut);
                }

                using (Bitmap templateBmp = new Bitmap(filePath))
                {
                    //グレースケールで確認するか？
                    if (grayScale)
                    {
                        //Mat型に変換
                        Mat bitmapScale = BitmapConverter.ToMat(templateBmp);

                        //グレースケールに変換
                        Cv2.CvtColor(bitmapScale, template, ColorConversionCodes.BGR2GRAY);
                    }
                    else
                    {
                        //Mat型に変換
                        template = BitmapConverter.ToMat(templateBmp);
                    }
                    while (true)
                    {
                        //現在のトップ画面のスクリーンショットを取得する
                        Bitmap bmp = new Bitmap(x2 - x1, y2 - y1, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.CopyFromScreen(x1, y1, 0, 0, bmp.Size);
                        }

                        //OpenCVで画像マッチングを行う下準備
                        Mat screen = BitmapConverter.ToMat(bmp);
                        if (grayScale)
                        {
                            //スクリーンショットをグレースケールに変換
                            Cv2.CvtColor(screen, screen, ColorConversionCodes.BGR2GRAY);
                        }

                        //タイムアウト時間は設定されているか？
                        if (timeOut != 0)
                        {
                            //フラグが立っていたら抜ける
                            if (timeOutFlg) { break; }
                        }

                        //画像マッチング開始
                        Mat result = new Mat();
                        Cv2.MatchTemplate(screen, template, result, TemplateMatchModes.CCoeffNormed);
                        Cv2.Threshold(result, result, threshold, 1.0, ThresholdTypes.Binary);
                        Cv2.MinMaxLoc(result, out OpenCvSharp.Point minPoint, out OpenCvSharp.Point maxPoint);

                        //マッチング結果の座標が両方0ならばマッチングしなかったので、そのまま返す
                        if ((maxPoint.X != 0) || (maxPoint.Y != 0))
                        {
                            //マッチングした座標(左上)を返す
                            matchingPoint[0] = x1 + maxPoint.X;
                            matchingPoint[1] = y1 + maxPoint.Y;

                            //マッチングした座標は左上を起点として返すか？
                            if (matchPositionMode)
                            {
                                //マッチングした画像の中心座標を含めて返す
                                matchingPoint[0] += ((int)templateBmp.Width / 2);
                                matchingPoint[1] += ((int)templateBmp.Height / 2);
                            }

                            //メモリを明示的に開放
                            result.Dispose();

                            //処理を抜ける
                            break;
                        }

                        //メモリを明示的に開放
                        result.Dispose();

                        Thread.Sleep(1);
                    }
                }

                //タイムアウト時間が設定されていあら、タイマ処理を破棄する
                if (timeOut != 0)
                {
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                }

                //戻り値設定
                if (timeOutFlg)
                {
                    matchingPoint[0] = 0;
                    matchingPoint[1] = 0;
                    SetErrorInfo((int)RESULT_CODE.TimeOut);
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return StellarRoboNil.Instance.NoResume();
            }

            //戻り値設定
            return matchingPoint.ToStellarRoboArray().NoResume();
        }
        private static StellarRoboFunctionResult GetCaretPos(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //戻り値設定
            return StellarRoboNil.Instance.NoResume();
        }
        #endregion

        #region Accessibleインターフェイス関連
        private static string GetAccData(int x,int y, ACCESSIBLE_TYPE acc_type)
        {
            //変数宣言
            IAccessible accessible;
            object child;
            string data = string.Empty;

            //指定座標よりIAccessibleを取得する
            IntPtr result = AccessibleObjectFromPoint(x, y, out accessible, out child);

            //取得出来たか？
            if((int)result == S_OK)
            {
                try
                {
                    switch (acc_type)
                    {
                        //Name
                        case ACCESSIBLE_TYPE.AccName:
                            data = accessible.accName[child];
                            break;
                        //Role
                        case ACCESSIBLE_TYPE.AccRole:
                            data = accessible.accRole[child].ToString();
                            break;
                        //State
                        case ACCESSIBLE_TYPE.AccState:
                            data = accessible.accState[child].ToString();
                            break;
                        //Value
                        case ACCESSIBLE_TYPE.AccValue:
                            data = accessible.accValue[child].ToString();
                            break;
                        default:
                            data = string.Empty;
                            break;
                    }
                }
                catch
                {
                    data = string.Empty;
                }
            }

            //戻り値設定
            return data ?? string.Empty;
        }
        private static StellarRoboFunctionResult GetAccName(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string xTmp = string.Empty;
            string yTmp = string.Empty;
            int x = 0;
            int y = 0;
            string accName = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length==2)
            {
                //座標取得
                xTmp = args[0].ToString().Trim();
                yTmp = args[1].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            #region 入力チェック
            //座標は入力されているか？
            if (string.IsNullOrEmpty(xTmp))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.AccessibleCoordinateNotEntererdError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            if (string.IsNullOrEmpty(yTmp))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.AccessibleCoordinateNotEntererdError);
                return string.Empty.AsStellarRoboString().NoResume();

            }
            //変換出来ない場合にはエラーとする
            if(!int.TryParse(xTmp,out x))
            {
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            if(!int.TryParse(yTmp,out y))
            {
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //画面サイズを取得する
            int ScreenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            int ScreenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;

            if((x < 0) || (x > ScreenWidth))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.AccessibleOutOfRangeError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            if ((y < 0) || y > ScreenHeight)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.AccessibleOutOfRangeError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            #endregion

            try
            {
                //指定座標よりコントロール名を取得する
                accName = GetAccData(x, y, ACCESSIBLE_TYPE.AccName);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return accName.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult GetAccRole(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string xTmp = string.Empty;
            string yTmp = string.Empty;
            int x = 0;
            int y = 0;
            string accRole = string.Empty;
            int Role = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if(args.Length==2)
            {
                //座標取得
                xTmp = args[0].ToString().Trim();
                yTmp = args[1].ToString().Trim();
            } 
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return 0.AsStellarRoboInteger().NoResume();
            }

            #region 入力チェック
            //座標は入力されているか？
            if (string.IsNullOrEmpty(xTmp))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.AccessibleCoordinateNotEntererdError);
                return 0.AsStellarRoboInteger().NoResume();
            }
            if (string.IsNullOrEmpty(yTmp))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.AccessibleCoordinateNotEntererdError);
                return 0.AsStellarRoboInteger().NoResume();

            }
            //変換出来ない場合にはエラーとする
            if (!int.TryParse(xTmp, out x))
            {
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return 0.AsStellarRoboInteger().NoResume();
            }
            if (!int.TryParse(yTmp, out y))
            {
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return 0.AsStellarRoboInteger().NoResume();
            }

            //画面サイズを取得する
            int ScreenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            int ScreenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;

            if ((x < 0) || (x > ScreenWidth))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.AccessibleOutOfRangeError);
                return 0.AsStellarRoboInteger().NoResume();
            }
            if ((y < 0) || y > ScreenHeight)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.AccessibleOutOfRangeError);
                return 0.AsStellarRoboInteger().NoResume();
            }
            #endregion

            try
            {
                //指定座標よりロール名を取得する
                accRole = GetAccData(x, y, ACCESSIBLE_TYPE.AccRole);
                Role = (accRole.Trim() == string.Empty) ? 0 : int.Parse(accRole);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return 0.AsStellarRoboInteger().NoResume();
            }

            //戻り値設定
            return Role.AsStellarRoboInteger().NoResume();
        }
        private static StellarRoboFunctionResult GetAccValue(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string xTmp = string.Empty;
            string yTmp = string.Empty;
            int x = 0;
            int y = 0;
            string accValue = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 2)
            {
                //座標取得
                xTmp = args[0].ToString().Trim();
                yTmp = args[1].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            #region 入力チェック
            //座標は入力されているか？
            if (string.IsNullOrEmpty(xTmp))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.AccessibleCoordinateNotEntererdError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            if (string.IsNullOrEmpty(yTmp))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.AccessibleCoordinateNotEntererdError);
                return string.Empty.AsStellarRoboString().NoResume();

            }
            //変換出来ない場合にはエラーとする
            if (!int.TryParse(xTmp, out x))
            {
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            if (!int.TryParse(yTmp, out y))
            {
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //画面サイズを取得する
            int ScreenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            int ScreenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;

            if ((x < 0) || (x > ScreenWidth))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.AccessibleOutOfRangeError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            if ((y < 0) || y > ScreenHeight)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.AccessibleOutOfRangeError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            #endregion

            try
            {
                //指定座標より値を取得する
                accValue = GetAccData(x, y, ACCESSIBLE_TYPE.AccValue);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return accValue.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult GetAccIsChecked(StellarRoboContext context, StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string xTmp = string.Empty;
            string yTmp = string.Empty;
            int x = 0;
            int y = 0;
            int state = 0;
            int accState = 0;
            int accRole = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if(args.Length==2)
            {
                //座標を取得する
                xTmp = args[0].ToString();
                yTmp = args[1].ToString();
            } 
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return 0.AsStellarRoboInteger().NoResume();
            }

            #region 入力チェック
            //座標は入力されているか？
            if (string.IsNullOrEmpty(xTmp))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.AccessibleCoordinateNotEntererdError);
                return 0.AsStellarRoboInteger().NoResume();
            }
            if (string.IsNullOrEmpty(yTmp))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.AccessibleCoordinateNotEntererdError);
                return 0.AsStellarRoboInteger().NoResume();

            }
            //変換出来ない場合にはエラーとする
            if (!int.TryParse(xTmp, out x))
            {
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return 0.AsStellarRoboInteger().NoResume();
            }
            if (!int.TryParse(yTmp, out y))
            {
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return 0.AsStellarRoboInteger().NoResume();
            }

            //画面サイズを取得する
            int ScreenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            int ScreenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;

            if ((x < 0) || (x > ScreenWidth))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.AccessibleOutOfRangeError);
                return 0.AsStellarRoboInteger().NoResume();
            }
            if ((y < 0) || y > ScreenHeight)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.AccessibleOutOfRangeError);
                return 0.AsStellarRoboInteger().NoResume();
            }
            #endregion

            try
            {
                //指定座標よりコントロールのロールを取得し対象となっているかを確認
                //対象とするのはオプションボタン及び、チェックボックス
                //それ以外の場合にはNoTargetを返す
                accRole = GetAccRole(context, self, args).ReturningObject.ToInt32();
                if (!((accRole == 44) || (accRole == 45)))
                {
                    SetErrorInfo((int)RESULT_CODE.AccessibleNoTargerError);
                    return 0.AsStellarRoboInteger().NoResume();
                }

                //指定座標よりコントロールの状態を取得する
                accState = int.Parse(GetAccData(x, y, ACCESSIBLE_TYPE.AccState));
                state = (accState & 0x10) != 0 ? 1 : 0;
            }
            catch(Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return 0.AsStellarRoboInteger().NoResume();
            }

            //戻り値設定
            return state.AsStellarRoboInteger().NoResume();
        }
        private static void ForegroundWindow(IntPtr foregroundWindowHandle)
        {
            //変数宣言
            IntPtr NowForegroundWindowHandle = IntPtr.Zero;
            uint ThreadID = 0;
            uint ForegroundThreadID = 0;
            ANIMATIONINFO buff = new ANIMATIONINFO();

            //現在最前列にいるWindowのハンドルを取得する
            NowForegroundWindowHandle = GetForegroundWindow();

            //取得出来なければ何もしない
            if(NowForegroundWindowHandle == IntPtr.Zero) { return; }

            //紐付いているスレッドIDを取得
            ThreadID = GetWindowThreadProcessId(NowForegroundWindowHandle, IntPtr.Zero);

            //取得出来なければ何もしない
            if (ThreadID == 0) { return; }

            //最前列に表示したWindowのスレッドIDを取得
            ForegroundThreadID = GetWindowThreadProcessId(foregroundWindowHandle, IntPtr.Zero);

            //取得出来なければ何もしない
            if (ForegroundThreadID == 0) { return; }

            //最前列にいるWindowにAttachする
            AttachThreadInput(ForegroundThreadID, ThreadID, true);

            //現在のWindowに設定
            SystemParametersInfo(SPI.SPI_GETFOREGROUNDLOCKTIMEOUT, 0, ref buff, SPIF.None);
            SystemParametersInfo(SPI.SPI_SETFOREGROUNDLOCKTIMEOUT, 0, IntPtr.Zero, SPIF.None);

            //最前面に移動
            SetForegroundWindow(foregroundWindowHandle);

            //現在のWindowの設定を戻す
            SystemParametersInfo(SPI.SPI_SETFOREGROUNDLOCKTIMEOUT, 0, ref buff, SPIF.None);

            //Detaachする
            AttachThreadInput(ForegroundThreadID, ThreadID, false);
        }
        #endregion

        #region Browser関連
        private static bool existsChrome()
        {
            //変数宣言
            bool result = false;

            //レジストリよりChromeがインストールされているパスを取得する
            using (Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe", false))
            {
                //レジストリ・キーが取得されているか？
                if (regKey != null)
                {
                    //レジストリよりChromeがインストールされているパスを取得する
                    string chromePath = regKey.GetValue("Path").ToString();

                    //chrome.exeが存在するならインストールされているとみなす
                    result = File.Exists(Path.Combine(chromePath, "chrome.exe"));
                }
            }

            //戻り値設定
            return result;
        }
        private static bool existsFireFox()
        {
            //変数宣言
            bool result = false;

            //レジストリよりFireFoxがインストールされているパスを取得する
            using (Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Mozilla\Mozilla Firefox", false))
            {
                //レジストリ・キーが取得されているか？
                if (regKey != null)
                {
                    //レジストリよりFireFoxがインストールされているパスを取得する
                    string fireFoxVersionName = regKey.GetValue("").ToString();
                    using (Microsoft.Win32.RegistryKey regKey2 = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(String.Format(@"Software\Mozilla\Mozilla Firefox {0}\bin", fireFoxVersionName), false))
                    {
                        //レジストリよりFireFoxがインストールされているパスを取得する
                        string fireFoxPath = regKey2.GetValue("PathToExe").ToString();

                        //FireFox.exeが存在するならインストールされているとみなす
                        result = File.Exists(fireFoxPath);
                    }
                }
            }
            //戻り値設定
            return result;
        }
        private static bool existsInternetExplorer()
        {
            //変数宣言
            bool result = false;

            //レジストリよりInternetExplorerがインストールされているパスを取得する
            using (Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\IEXPLORE.EXE", false))
            {
                //レジストリ・キーは取得されているか？
                if (regKey != null)
                {
                    //レジストリよりInternetExplorerがインストールされているパスを取得する
                    string internetExplorerPath = regKey.GetValue("Path").ToString();

                    //iexplore.exeが存在するならインストールされているとみなす
                    result = File.Exists(Path.Combine(internetExplorerPath.Substring(0, internetExplorerPath.Length - 1), "iexplore.exe"));
                }
            }

            //戻り値設定
            return result;
        }
        private static StellarRoboFunctionResult BrowserNavigate(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string BrowserKey = string.Empty;
            string browserTypeTmp = string.Empty;
            BrowserType browserType = BrowserType.FireFox;
            string URL = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 1 || args.Length == 2)
            {
                //URL取得
                URL = args[0].ToString().Trim();

                //Browser選択
                if (args.Length > 1)
                {
                    browserTypeTmp = args[1].ToString();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            #region 入力チェック
            //URLは入力されているか？
            if (string.IsNullOrEmpty(URL))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserURLInputNotEnteredError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            //BrowserType入力チェック
            if(string.IsNullOrEmpty(browserTypeTmp))
            {
                //未入力ならば、デフォルトで指定されているBrowserを使用する
                browserType = GetDefaultBrowser();
            }
            else
            {
                //数字か？
                int browserTypeNum = 0;
                if(!int.TryParse(browserTypeTmp,out browserTypeNum))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return string.Empty.AsStellarRoboString().NoResume();
                }

                //範囲内か？
                if(!(browserTypeNum >= 0 && (int)BrowserType.End > browserTypeNum))
                {
                    SetErrorInfo((int)RESULT_CODE.BrowserTypeOutOfRangeError);
                    return string.Empty.AsStellarRoboString().NoResume();
                }

                //BrowserType選択
                switch(browserTypeNum)
                {
                    //InternetExplorer
                    case (int)BrowserType.InternetExplorer:
                        browserType = BrowserType.InternetExplorer;
                        break;
                    //FireFox
                    case (int)BrowserType.FireFox:
                        browserType = BrowserType.FireFox;
                        break;
                    //Chrome
                    case (int)BrowserType.Chrome:
                        browserType = BrowserType.Chrome;
                        break;
                    //その他
                    default:
                        browserType = GetDefaultBrowser();
                        break;
                }
            }
            #endregion

            try
            {
                //現時点での日付を利用しDictionaryのキーを作成する
                BrowserKey = CreateObjectKey();

                //Browserを開く
                BrowserInfo browserInfo = CreateBrowser(browserType);

                //Browserは取得出来たか？
                if (browserInfo.webDriver != null)
                {
                    //URL指定
                    browserInfo.webDriver.Url = URL;

                    //Browserオブジェクトを保存する
                    Browser.Add(BrowserKey, browserInfo);

                    //ScrollBarを消す
                    ((IJavaScriptExecutor)Browser[BrowserKey].webDriver).ExecuteScript("document.body.style='overflow: hidden;'");
                }
                else
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.BrowserNotFoundError);
                    //起動するべきBrowserが存在しないのでエラーとする
                    return string.Empty.AsStellarRoboString().NoResume();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return BrowserKey.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult BrowserOpenUrl(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {

            //変数宣言
            string BrowserKey = string.Empty;
            string URL = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 2)
            {
                //BrowserKey取得
                BrowserKey = args[0].ToString();

                //URL取得
                URL = args[1].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //BrowserKeyは入力されているか？
            if (string.IsNullOrEmpty(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return false.AsStellarRoboBoolean().NoResume();
            }

            //指定されたBrowserKeyは存在しているか？
            if (!Browser.ContainsKey(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyNotFoundError);
                //存在していいないのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //URLは入力されているか？
            if (string.IsNullOrEmpty(URL))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserURLInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();

            }
            #endregion

            try
            {
                //ブラウザを開く
                Browser[BrowserKey].webDriver.Navigate().GoToUrl(URL);

                //ScrollBarを消す
                ((IJavaScriptExecutor)Browser[BrowserKey].webDriver).ExecuteScript("document.body.style='overflow: hidden;'");
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult BrowserWait(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string BrowserKey = string.Empty;
            string timeOutTmp = string.Empty;
            Stopwatch stopWatch = new Stopwatch();
            int timeOut = 0;
            string matchWord = string.Empty;
            int pid = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            #region 内部関数
            int GetParentProcess(int ID)
            {
                int parentPid = 0;
                using (ManagementObject mo = new ManagementObject($"win32_process.handle='{ID}'"))
                {
                    mo.Get();
                    parentPid = Convert.ToInt32(mo["ParentProcessId"]);
                }
                return parentPid;
            }
            int GetBrowserProcessID(string browserName,int processID)
            {
                IEnumerable<Process> processes;

                if (browserName != "firefox")
                {
                    processes = Process.GetProcessesByName(browserName).Where(_ => !_.MainWindowHandle.Equals(IntPtr.Zero));
                }
                else
                {
                    processes = Process.GetProcessesByName(browserName);
                }

                foreach (var process in processes)
                {
                    var parentID = GetParentProcess(process.Id);
                    if (parentID == processID)
                    {
                        return process.Id;
                    }
                }
                return 0;
            }

            int GetDownLoadFileCount(int downLoadPid, string downLoadMatchWord)
            {
                //ファイルはダウンロードされているか？
                List<string> DownLoadFile = new List<string>();
                foreach (HoldingFileInfo holdingFileInfo in HoldingFileSearch.HoldingFileName(downLoadPid))
                {
                    if (Regex.IsMatch(holdingFileInfo.Name, downLoadMatchWord))
                    {
                        //ダウンロードファイル保存
                        DownLoadFile.Add(holdingFileInfo.Name);
                    }
                }

                //戻り値設定
                return DownLoadFile.Count();
            }
            #endregion

            //引数チェック
            if (args.Length == 1 || args.Length == 2)
            {
                //BrowserKey取得
                BrowserKey = args[0].ToString();

                //タイムアウト時間は指定されているか？
                if (args.Length == 2)
                {
                    //タイムアウト時間取得
                    timeOutTmp = args[1].ToString();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //BrowserKeyは入力されているか？
            if (string.IsNullOrEmpty(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return false.AsStellarRoboBoolean().NoResume();
            }

            //指定されたBrowserKeyは存在しているか？
            if (!Browser.ContainsKey(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyNotFoundError);
                //存在していいないのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //TimeOut時間は設定されているか？
            if (!string.IsNullOrEmpty(timeOutTmp))
            {
                //変換出来ない場合には強制的に0にする
                if (!int.TryParse(timeOutTmp, out timeOut)) { timeOut = 0; }
            }
            if ((timeOut < 0) || (timeOut > Int32.MaxValue))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserWaitTimeOutOutOfRangeError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            #endregion

            try
            {
                //抽出文字作成
                switch (Browser[BrowserKey].webDriver)
                {
                    //IE
                    case IWebDriver webDriver when webDriver is InternetExplorerDriver:
                        matchWord = "\\.partial$";
                        pid = GetBrowserProcessID("iexplore", Browser[BrowserKey].driverService.ProcessId);
                        pid = GetBrowserProcessID("iexplore", pid);
                        break;
                    //FireFox
                    case IWebDriver webDriver when webDriver is FirefoxDriver:
                        matchWord = "\\.part$";
                        pid = GetBrowserProcessID("firefox", Browser[BrowserKey].driverService.ProcessId);
                        pid = GetBrowserProcessID("firefox", pid);
                        break;
                    //Chrome
                    case IWebDriver webDriver when webDriver is ChromeDriver:
                        matchWord = "\\.crdownload$";
                        pid = GetBrowserProcessID("chrome", Browser[BrowserKey].driverService.ProcessId);
                        break;
                }

                //あまり早すぎると、ダウンロードファイルを取得出来ないのでWaitを入れる
                Thread.Sleep(1000);

                //計測開始
                stopWatch.Reset();
                stopWatch.Start();

                //Browserが完全に表示されるまで待つ
                WebDriverWait wait = new WebDriverWait(Browser[BrowserKey].webDriver, TimeSpan.FromSeconds(timeOut));
#pragma warning disable CS0618
                wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("/html/body")));
#pragma warning restore CS0618

                //その後、ダウンロードされているのであれば終了するまで待つ
                if(GetDownLoadFileCount(pid, matchWord) > 0)
                {
                    do
                    {
                        //Wait
                        Thread.Sleep(1000);

                        //経過時間を取得
                        stopWatch.Stop();
                        long elapsedTime = stopWatch.ElapsedMilliseconds;
                        stopWatch.Start();

                        //経過時間は指定時間を超えているか？
                        if ((timeOut > 0) && (elapsedTime > timeOut))
                        {
                            //タイムアウトを発行する
                            throw new TimeoutException();
                        }
                    } while (GetDownLoadFileCount(pid, matchWord) > 0);
                }
            }
            catch(TimeoutException)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.TimeOut);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();

            }
            finally
            {
                //計測終了
                stopWatch.Stop();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult BrowserGetCoordinate(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string BrowserKey = string.Empty;
            string TargetElement = string.Empty;
            string SearchTypeTmp = string.Empty;
            int SearchType = 0;
            int[] coordinate = { 0, 0, 0, 0 };

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 2 || args.Length == 3)
            {
                //BrowserKey取得
                BrowserKey = args[0].ToString();

                //エレメント取得
                TargetElement = args[1].ToString().Trim();

                //検索タイプ取得
                if (args.Length > 2)
                {
                    SearchTypeTmp = args[2].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return (new int[4] { 0, 0, 0, 0 }).ToStellarRoboArray().NoResume();
            }

            #region 入力チェック
            //BrowserKeyは入力されているか？
            if (string.IsNullOrEmpty(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return (new int[4] { 0, 0, 0, 0 }).ToStellarRoboArray().NoResume();
            }

            //指定されたBrowserKeyは存在しているか？
            if (!Browser.ContainsKey(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyNotFoundError);
                //存在していいないのでエラーとする
                return (new int[4] { 0, 0, 0, 0 }).ToStellarRoboArray().NoResume();
            }

            //Elementは入力されているか？
            if (string.IsNullOrEmpty(TargetElement))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserElementInputNotEnteredError);
                return (new int[4] { 0, 0, 0, 0 }).ToStellarRoboArray().NoResume();
            }
            //検索タイプは入力されているか？
            if (string.IsNullOrEmpty(SearchTypeTmp))
            {
                //省略されているならば、IDで検索を行う
                SearchType = 0;
            }
            else
            {
                //数値入力か？
                if (!int.TryParse(SearchTypeTmp, out SearchType))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return (new int[4] { 0, 0, 0, 0 }).ToStellarRoboArray().NoResume();
                }
                //範囲内か？
                if (!(SearchType >= 0 && SearchType < (int)ElementSearchType.End))
                {
                    SetErrorInfo((int)RESULT_CODE.BrowserSearchTypeOutOfRangeError);
                    return (new int[4] { 0, 0, 0, 0 }).ToStellarRoboArray().NoResume();
                }
            }
            #endregion

            try
            {
                IReadOnlyCollection<IWebElement> webElements;

                //検索
                webElements = SearchElement(Browser[BrowserKey].webDriver, SearchType, TargetElement);

                //取得出来たか？
                if (webElements.Count > 0)
                {
                    //先頭でマッチしたオブジェクトを対象とする
                    IWebElement webElement = webElements.ElementAt(0);

                    //ブラウザのサイズを取得する
                    int x = 0;
                    int y = 0;
                    int availHeight = 0;
                    int availWidth = 0;
                    int windowHeight = 0;
                    int windowWidth = 0;
                    if(!int.TryParse(((IJavaScriptExecutor)Browser[BrowserKey].webDriver).ExecuteScript("return document.body.clientHeight;").ToString(), out availHeight)) { availHeight = 0; }
                    if(!int.TryParse(((IJavaScriptExecutor)Browser[BrowserKey].webDriver).ExecuteScript("return document.body.clientWidth;").ToString(), out availWidth)) { availWidth = 0; }
                    windowHeight = Browser[BrowserKey].webDriver.Manage().Window.Size.Height;
                    windowWidth = Browser[BrowserKey].webDriver.Manage().Window.Size.Width;
                    x = Browser[BrowserKey].webDriver.Manage().Window.Position.X;
                    y = Browser[BrowserKey].webDriver.Manage().Window.Position.Y;

                    //座標設定
                    coordinate[0] = webElement.Location.X + x + ((windowWidth - availWidth) / 2);
                    coordinate[1] = webElement.Location.Y + y + (windowHeight - availHeight);
                    coordinate[2] = webElement.Size.Width;
                    coordinate[3] = webElement.Size.Height;
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return (new int[4] { 0, 0, 0, 0 }).ToStellarRoboArray().NoResume();
            }

            //戻り値設定
            return coordinate.ToStellarRoboArray().NoResume();

        }
        private static StellarRoboFunctionResult BrowserInput(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string BrowserKey = string.Empty;
            string InputData = string.Empty;
            string TargetElement = string.Empty;
            string SearchTypeTmp = string.Empty;
            int SearchType = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 3 || args.Length == 4)
            {
                //BrowserKey取得
                BrowserKey = args[0].ToString();

                //入力値取得
                InputData = args[1].ToString();

                //エレメント取得
                TargetElement = args[2].ToString().Trim();

                //検索タイプ取得
                if (args.Length > 3)
                {
                    SearchTypeTmp = args[3].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //BrowserKeyは入力されているか？
            if (string.IsNullOrEmpty(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return false.AsStellarRoboBoolean().NoResume();
            }

            //指定されたBrowserKeyは存在しているか？
            if (!Browser.ContainsKey(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyNotFoundError);
                //存在していいないのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //Elementは入力されているか？
            if (string.IsNullOrEmpty(TargetElement))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserElementInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //検索タイプは入力されているか？
            if(string.IsNullOrEmpty(SearchTypeTmp))
            {
                //省略されているならば、IDで検索を行う
                SearchType = 0;
            }
            else
            {
                //数値入力か？
                if(!int.TryParse(SearchTypeTmp,out SearchType))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                //範囲内か？
                if(!(SearchType >= 0 && SearchType < (int)ElementSearchType.End))
                {
                    SetErrorInfo((int)RESULT_CODE.BrowserSearchTypeOutOfRangeError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            #endregion

            try
            {
                IReadOnlyCollection<IWebElement> webElements;

                //検索
                webElements = SearchElement(Browser[BrowserKey].webDriver, SearchType, TargetElement);

                //取得出来たか？
                if (webElements.Count > 0)
                {
                    //先頭でマッチしたオブジェクトに送る
                    webElements.ElementAt(0).SendKeys(InputData);
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult BrowserOutput(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string BrowserKey = string.Empty;
            string OutputData = string.Empty;
            string TargetElement = string.Empty;
            string SearchTypeTmp = string.Empty;
            String AttributeName = string.Empty;
            int SearchType = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length >= 2 || args.Length <= 4)
            {
                //BrowserKey取得
                BrowserKey = args[0].ToString();

                //エレメント取得
                TargetElement = args[1].ToString().Trim();

                //検索タイプ取得
                if (args.Length > 2)
                {
                    SearchTypeTmp = args[2].ToString().Trim();
                }

                //属性名
                if(args.Length > 3)
                {
                    AttributeName = args[3].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            #region 入力チェック
            //BrowserKeyは入力されているか？
            if (string.IsNullOrEmpty(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //指定されたBrowserKeyは存在しているか？
            if (!Browser.ContainsKey(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyNotFoundError);
                //存在していいないのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();
            }
            //Elementは入力されているか？
            if (string.IsNullOrEmpty(TargetElement))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserElementInputNotEnteredError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            //検索タイプは入力されているか？
            if (string.IsNullOrEmpty(SearchTypeTmp))
            {
                //省略されているならば、IDで検索を行う
                SearchType = 0;
            }
            else
            {
                //数値入力か？
                if (!int.TryParse(SearchTypeTmp, out SearchType))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return string.Empty.AsStellarRoboString().NoResume();
                }
                //範囲内か？
                if (!(SearchType >= 0 && SearchType < (int)ElementSearchType.End))
                {
                    SetErrorInfo((int)RESULT_CODE.BrowserSearchTypeOutOfRangeError);
                    return string.Empty.AsStellarRoboString().NoResume();
                }
            }
            #endregion

            try
            {
                IReadOnlyCollection<IWebElement> webElements;

                //検索
                webElements = SearchElement(Browser[BrowserKey].webDriver, SearchType, TargetElement);

                //取得出来たか？
                if (webElements.Count > 0)
                {
                    //属性名は指定されているか？
                    if (string.IsNullOrEmpty(AttributeName))
                    {
                        //先頭でマッチしたオブジェクトから情報を取得する
                        OutputData = webElements.ElementAt(0).Text;
                    }
                    else
                    {
                        //属性名を指定して取得
                        OutputData = string.IsNullOrEmpty(webElements.ElementAt(0).GetAttribute(AttributeName)) ? string.Empty : webElements.ElementAt(0).GetAttribute(AttributeName).ToString().Trim();
                    }
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return OutputData.AsStellarRoboString().NoResume();
        }
        private static StellarRoboFunctionResult BrowserCheckID(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string BrowserKey = string.Empty;
            string TargetElement = string.Empty;
            string SearchTypeTmp = string.Empty;
            int SearchType = 0;
            bool checkID = false;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 2 || args.Length == 3)
            {
                //BrowserKey取得
                BrowserKey = args[0].ToString();

                //エレメント取得
                TargetElement = args[1].ToString().Trim();

                //検索タイプ取得
                if (args.Length > 2)
                {
                    SearchTypeTmp = args[2].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //BrowserKeyは入力されているか？
            if (string.IsNullOrEmpty(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return false.AsStellarRoboBoolean().NoResume();
            }

            //指定されたBrowserKeyは存在しているか？
            if (!Browser.ContainsKey(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyNotFoundError);
                //存在していいないのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //Elementは入力されているか？
            if (string.IsNullOrEmpty(TargetElement))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserElementInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //検索タイプは入力されているか？
            if (string.IsNullOrEmpty(SearchTypeTmp))
            {
                //省略されているならば、IDで検索を行う
                SearchType = 0;
            }
            else
            {
                //数値入力か？
                if (!int.TryParse(SearchTypeTmp, out SearchType))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                //範囲内か？
                if (!(SearchType >= 0 && SearchType < (int)ElementSearchType.End))
                {
                    SetErrorInfo((int)RESULT_CODE.BrowserSearchTypeOutOfRangeError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            #endregion

            try
            {
                IReadOnlyCollection<IWebElement> webElements;

                //検索
                webElements = SearchElement(Browser[BrowserKey].webDriver, SearchType, TargetElement);

                //取得出来たか？
                if (webElements.Count > 0)
                {
                    //ﾌﾗｸﾞ設定
                    checkID = true;
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return checkID.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult BrowserClose(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string BrowserKey = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 1)
            {
                //MapName取得
                BrowserKey = args[0].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //BrowserKeyは入力されているか？
            if (string.IsNullOrEmpty(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return false.AsStellarRoboBoolean().NoResume();
            }

            //指定されたBrowserKeyは存在しているか？
            if (!Browser.ContainsKey(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyNotFoundError);
                //存在していいないのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }
            #endregion

            try
            {
                //Browserを閉じる
                Browser[BrowserKey].webDriver.Close();
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult BrowserQuit(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string BrowserKey = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 1)
            {
                //MapName取得
                BrowserKey = args[0].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //BrowserKeyは入力されているか？
            if (string.IsNullOrEmpty(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return false.AsStellarRoboBoolean().NoResume();
            }

            //指定されたBrowserKeyは存在しているか？
            if (!Browser.ContainsKey(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyNotFoundError);
                //存在していいないのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }
            #endregion

            try
            {
                //Browserを閉じる
                Browser[BrowserKey].webDriver.Quit();

                //Browser情報を破棄する
                Browser[BrowserKey].webDriver.Dispose();
                Browser[BrowserKey].driverService.Dispose();

                //保存していたBrowser情報を破棄する
                Browser.Remove(BrowserKey);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult BrowserClick(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string BrowserKey = string.Empty;
            string TargetElement = string.Empty;
            string SearchTypeTmp = string.Empty;
            int SearchType = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 2 || args.Length == 3)
            {
                //BrowserKey取得
                BrowserKey = args[0].ToString();

                //エレメント取得
                TargetElement = args[1].ToString().Trim();

                //検索タイプ取得
                if (args.Length > 2)
                {
                    SearchTypeTmp = args[2].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //BrowserKeyは入力されているか？
            if (string.IsNullOrEmpty(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return false.AsStellarRoboBoolean().NoResume();
            }

            //指定されたBrowserKeyは存在しているか？
            if (!Browser.ContainsKey(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyNotFoundError);
                //存在していいないのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //Elementは入力されているか？
            if (string.IsNullOrEmpty(TargetElement))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserElementInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //検索タイプは入力されているか？
            if (string.IsNullOrEmpty(SearchTypeTmp))
            {
                //省略されているならば、IDで検索を行う
                SearchType = 0;
            }
            else
            {
                //数値入力か？
                if (!int.TryParse(SearchTypeTmp, out SearchType))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                //範囲内か？
                if (!(SearchType >= 0 && SearchType < (int)ElementSearchType.End))
                {
                    SetErrorInfo((int)RESULT_CODE.BrowserSearchTypeOutOfRangeError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            #endregion

            try
            {
                IReadOnlyCollection<IWebElement> webElements;

                //検索
                webElements = SearchElement(Browser[BrowserKey].webDriver, SearchType, TargetElement);

                //取得出来たか？
                if (webElements.Count > 0)
                {
                    //先頭でマッチしたオブジェクトを対象とする
                    IWebElement webElement = webElements.ElementAt(0);

                    //Submitか？
                    if(webElement.GetAttribute("type").ToLower() == "submit")
                    {
                        webElement.Submit();
                    }
                    else
                    {
                        //Submit以外はClickを発行する
                        webElement.Click();
                    }
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private StellarRoboFunctionResult BrowserSelectBox(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string BrowserKey = string.Empty;
            string TargetElement = string.Empty;
            string Message = string.Empty;
            string SearchTypeTmp = string.Empty;
            int SearchType = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 3 || args.Length == 4)
            {
                //BrowserKey取得
                BrowserKey = args[0].ToString();

                //エレメント取得
                TargetElement = args[1].ToString().Trim();

                //対象となる表示メッセージを取得
                Message = args[2].ToString();

                //検索タイプ取得
                if (args.Length > 3)
                {
                    SearchTypeTmp = args[3].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //BrowserKeyは入力されているか？
            if (string.IsNullOrEmpty(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return false.AsStellarRoboBoolean().NoResume();
            }

            //指定されたBrowserKeyは存在しているか？
            if (!Browser.ContainsKey(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyNotFoundError);
                //存在していいないのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //Elementは入力されているか？
            if (string.IsNullOrEmpty(TargetElement))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserElementInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //検索タイプは入力されているか？
            if (string.IsNullOrEmpty(SearchTypeTmp))
            {
                //省略されているならば、IDで検索を行う
                SearchType = 0;
            }
            else
            {
                //数値入力か？
                if (!int.TryParse(SearchTypeTmp, out SearchType))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                //範囲内か？
                if (!(SearchType >= 0 && SearchType < (int)ElementSearchType.End))
                {
                    SetErrorInfo((int)RESULT_CODE.BrowserSearchTypeOutOfRangeError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            #endregion

            try
            {
                IReadOnlyCollection<IWebElement> webElements;

                //検索
                webElements = SearchElement(Browser[BrowserKey].webDriver, SearchType, TargetElement);

                //取得出来たか？
                if (webElements.Count > 0)
                {
                    //先頭でマッチしたオブジェクトから選択する
                    SelectElement dropdown = new SelectElement(webElements.ElementAt(0));
                    dropdown.SelectByText(Message);
                }
            }
            catch (NoSuchElementException)
            {
                //検索対象無し
                SetErrorInfo((int)RESULT_CODE.NoSearchTarget);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        private static StellarRoboFunctionResult BrowserList(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string BrowserKey = string.Empty;
            List<string> BrowserList = new List<string>();


            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 1)
            {
                //BrowserKey取得
                BrowserKey = args[0].ToString();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //BrowserKeyは入力されているか？
            if (string.IsNullOrEmpty(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return false.AsStellarRoboBoolean().NoResume();
            }

            //指定されたBrowserKeyは存在しているか？
            if (!Browser.ContainsKey(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyNotFoundError);
                //存在していいないのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }
            #endregion

            try
            {
                //一覧取得
                BrowserList = Browser[BrowserKey].webDriver.WindowHandles.ToList();

                //一覧より親を除く
                BrowserList.Remove(Browser[BrowserKey].webDriver.CurrentWindowHandle);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return new List<string>().ToStellarRoboArray().NoResume();
            }

            //戻り値設定
            return BrowserList.ToStellarRoboArray().NoResume();

        }
        private static StellarRoboFunctionResult BrowserChange(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string BrowserKey = string.Empty;
            string ChangeOwnerHandle = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 2)
            {
                //BrowserKey取得
                BrowserKey = args[0].ToString().Trim();

                //ChangeOwnerHandle取得
                ChangeOwnerHandle = args[1].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //BrowserKeyは入力されているか？
            if (string.IsNullOrEmpty(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return false.AsStellarRoboBoolean().NoResume();
            }

            //指定されたBrowserKeyは存在しているか？
            if (!Browser.ContainsKey(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyNotFoundError);
                //存在していいないのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }
            //ChangeOwnerHandleは入力されているか？
            if (string.IsNullOrEmpty(ChangeOwnerHandle))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserOwnerHandleInputNotEnteredError);
                //ChangeOwnerHandleが入力されていないのでエラー
                return false.AsStellarRoboBoolean().NoResume();
            }
            #endregion

            try
            {
                //Ownerを変更する
                Browser[BrowserKey].webDriver.SwitchTo().Window(ChangeOwnerHandle);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();

        }
        private static StellarRoboFunctionResult BrowserOwner(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string BrowserKey = string.Empty;
            string OwnerHandle = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 1)
            {
                //BrowserKey取得
                BrowserKey = args[0].ToString();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //BrowserKeyは入力されているか？
            if (string.IsNullOrEmpty(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return false.AsStellarRoboBoolean().NoResume();
            }

            //指定されたBrowserKeyは存在しているか？
            if (!Browser.ContainsKey(BrowserKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.BrowserKeyNotFoundError);
                //存在していいないのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }
            #endregion

            try
            {
                //OwnerHandleを取得する
                OwnerHandle = Browser[BrowserKey].webDriver.CurrentWindowHandle;
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return OwnerHandle.AsStellarRoboString().NoResume();
        }
        #endregion

        #region Excel関連
        public StellarRoboFunctionResult ExcelOpen(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string ExcelKey = string.Empty;
            string fileName = string.Empty;
            bool createNew = false;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 1 || args.Length == 2)
            {
                //Excelファイル名取得
                fileName = args[0].ToString().Trim();

                //新規作成フラグ
                if (args.Length > 1)
                {
                    //変換出来ない場合には強制的にfalse扱いとする
                    if(!bool.TryParse(args[1].ToString(),out createNew)) { createNew = false; }
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            #region 入力チェック
            //Excelファイルは入力されているか？
            if (string.IsNullOrEmpty(fileName))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelFileNameInputNotEnteredError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            bool fileExists = File.Exists(fileName);
            if (createNew)
            {
                //新規作成
                if (fileExists)
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.ExcelFileCreateError);
                    return string.Empty.AsStellarRoboString().NoResume();
                }
            }
            else
            {
                //既存読込
                if (!fileExists)
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.ExcelFileExistsError);
                    return string.Empty.AsStellarRoboString().NoResume();
                }
            }
            #endregion

            try
            {
                //現時点での日時を利用しDictionaryのキーを作成する
                ExcelKey = CreateObjectKey();

                //新規作成フラグは指定されているか？
                IWorkbook workbook;
                if (createNew)
                {
                    //Excelファイル新規作成
                    string Extension = Path.GetExtension(fileName);
                    if(Extension == ".xls")
                    {
                        workbook = new NPOI.HSSF.UserModel.HSSFWorkbook();
                    }
                    else if (Extension == ".xlsx")
                    {
                        workbook = new NPOI.XSSF.UserModel.XSSFWorkbook();
                    }
                    else
                    {
                        //ステータス設定
                        SetErrorInfo((int)RESULT_CODE.ExcelFileExtensionError);
                        return string.Empty.AsStellarRoboString().NoResume();
                    }
                }
                else
                {
                    //Excelファイル・オープン
                    using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
                    {
                        workbook = WorkbookFactory.Create(fileStream);
                    }
                }

                //Excel操作オブジェクトを保存する
                Excel_Data excel = new Excel_Data
                {
                    FileName = fileName,
                    WorkBook = workbook
                };

                //Sheetも同時に保存する
                for (int i = 0; i < workbook.NumberOfSheets; i++)
                {
                    ISheet sheet = workbook.GetSheetAt(i);
                    if (excel.Sheet == null) { excel.Sheet = new Dictionary<string, ISheet>(); }
                    excel.Sheet.Add(sheet.SheetName, sheet);
                }

                //保存
                Excel.Add(ExcelKey, excel);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return ExcelKey.AsStellarRoboString().NoResume();
        }
        public StellarRoboFunctionResult ExcelClose(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string ExcelKey = string.Empty;
            bool SaveFlg = true;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 1 || args.Length == 2)
            {
                //MapName取得
                ExcelKey = args[0].ToString().Trim();

                //保存フラグ取得
                if (args.Length == 2)
                {
                    //変換出来ない場合には強制的にtrue扱いとする
                    if(!bool.TryParse(args[1].ToString(),out SaveFlg)) { SaveFlg = true; }
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //ExcelKeyは入力されているか？
            if (string.IsNullOrEmpty(ExcelKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelWorkBookKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return false.AsStellarRoboBoolean().NoResume();
            }

            //指定されたExcelKeyは存在しているか？
            if (!Excel.ContainsKey(ExcelKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelWorkBookKeyNotFoundError);
                //存在していいないのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }
            #endregion

            try
            {
                //保存するのか？
                if (SaveFlg)
                {
                    //Excel保存
                    using (FileStream fileStream = new FileStream(Excel[ExcelKey].FileName, FileMode.Create))
                    {
                        Excel[ExcelKey].WorkBook.Write(fileStream);
                    }
                }

                //WorkBookを閉じる
                Excel[ExcelKey].WorkBook.Close();

                //保存していたWorkBookを削除する
                Excel.Remove(ExcelKey);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        public StellarRoboFunctionResult ExcelGetCell(StellarRoboContext context, StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string ExcelKey = string.Empty;
            string SheetName = string.Empty;
            string CoordinateLineTmp = string.Empty;
            string CoordinateColumnTmp = string.Empty;
            int CoordinateLine = 0;                 //行
            int CoordinateColumn = 0;               //列
            string resultValue = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 4)
            {
                //ExcelKeyName取得
                ExcelKey = args[0].ToString().Trim();

                //SheetName取得
                SheetName = args[1].ToString().Trim();

                //行を取得
                CoordinateLineTmp = args[2].ToString().Trim();

                //列を取得
                CoordinateColumnTmp = args[3].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return string.Empty.AsStellarRoboString().NoResume();
            }

            #region 入力チェック
            //ExcelKeyは入力されているか？
            if (string.IsNullOrEmpty(ExcelKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelWorkBookKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return string.Empty.AsStellarRoboString().NoResume();
            }
            //SheetNameは入力されているか？
            if (string.IsNullOrEmpty(SheetName))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelSheetKeyInputNotEnteredError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            //指定されたExcelKeyは存在しているか？
            if (!Excel.ContainsKey(ExcelKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelWorkBookKeyNotFoundError);
                //存在していいないのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();
            }
            //指定されたSheetNameは存在しているか？
            if (!Excel[ExcelKey].IsExistsSheetName(SheetName))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelSheetKeyNotFoundError);
                //存在していないのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();
            }
            //CoordinateLineは入力されているか？
            if (string.IsNullOrEmpty(CoordinateLineTmp))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelCoordinateLineNotEnteredError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            if (!int.TryParse(CoordinateLineTmp, out CoordinateLine))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            //CoordinateColumnTmpは入力されているか？
            if (string.IsNullOrEmpty(CoordinateColumnTmp))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelCoordinateColumnNotEntererdError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            if (!int.TryParse(CoordinateColumnTmp, out CoordinateColumn))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return string.Empty.AsStellarRoboString().NoResume();
            }
            #endregion

            try
            {
                //Excel情報を取得
                ISheet sheet = Excel[ExcelKey].Sheet[SheetName];

                //行と列を指定し、値を取得する
                IRow row = sheet.GetRow(CoordinateLine) ?? sheet.CreateRow(CoordinateLine);
                ICell cell = row.GetCell(CoordinateColumn) ?? row.CreateCell(CoordinateColumn);
                switch (cell.CellType)
                {
                    //数値
                    case CellType.Numeric:
                        if (DateUtil.IsCellDateFormatted(cell))
                        {
                            //日付
                            resultValue = cell.DateCellValue.ToShortDateString();
                        }
                        else
                        {
                            //数値
                            resultValue = cell.NumericCellValue.ToString();
                        }
                        break;
                    //文字列
                    case CellType.String:
                        resultValue = cell.StringCellValue;
                        break;
                    //Formula
                    case CellType.Formula:
                        resultValue = string.Empty;
                        break;
                    //Blank
                    case CellType.Blank:
                        resultValue = string.Empty;
                        break;
                    //Boolean
                    case CellType.Boolean:
                        resultValue = cell.BooleanCellValue.ToString();
                        break;
                    //Error
                    case CellType.Error:
                        resultValue = string.Empty;
                        break;
                    //Unknown
                    case CellType.Unknown:
                        resultValue = string.Empty;
                        break;
                    default:
                        resultValue = string.Empty;
                        break;
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return string.Empty.AsStellarRoboString().NoResume();
            }

            //戻り値設定
            return resultValue.AsStellarRoboString().NoResume();
        }
        public StellarRoboFunctionResult ExcelSetCell(StellarRoboContext context , StellarRoboObject self, StellarRoboObject[] args)
        {
            //変数宣言
            string ExcelKey = string.Empty;
            string SheetName = string.Empty;
            string CoordinateLineTmp = string.Empty;
            string CoordinateColumnTmp = string.Empty;
            int CoordinateLine = 0;                 //行
            int CoordinateColumn = 0;               //列
            string setValue = string.Empty;         //設定値

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 5)
            {
                //ExcelKeyName取得
                ExcelKey = args[0].ToString().Trim();

                //SheetName取得
                SheetName = args[1].ToString().Trim();

                //行を取得
                CoordinateLineTmp = args[2].ToString().Trim();

                //列を取得
                CoordinateColumnTmp = args[3].ToString().Trim();

                //設定値取得
                setValue = args[4].ToString();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //ExcelKeyは入力されているか？
            if (string.IsNullOrEmpty(ExcelKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelWorkBookKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return 0.AsStellarRoboInteger().NoResume();
            }
            //SheetNameは入力されているか？
            if (string.IsNullOrEmpty(SheetName))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelSheetKeyInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //指定されたExcelKeyは存在しているか？
            if(!Excel.ContainsKey(ExcelKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelWorkBookKeyNotFoundError);
                //存在していいないのでエラーとする
                return 0.AsStellarRoboInteger().NoResume();
            }
            //指定されたSheetNameは存在しているか？
            if (!Excel[ExcelKey].IsExistsSheetName(SheetName))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelSheetKeyNotFoundError);
                //存在していないのでエラーとする
                return 0.AsStellarRoboInteger().NoResume();
            }
            //CoordinateLineは入力されているか？
            if (string.IsNullOrEmpty(CoordinateLineTmp))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelCoordinateLineNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            if(!int.TryParse(CoordinateLineTmp, out CoordinateLine))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //CoordinateColumnTmpは入力されているか？
            if (string.IsNullOrEmpty(CoordinateColumnTmp))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelCoordinateColumnNotEntererdError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            if(!int.TryParse(CoordinateColumnTmp, out CoordinateColumn))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            #endregion

            try
            {
                //Excel情報を取得
                ISheet sheet = Excel[ExcelKey].Sheet[SheetName];

                //行と列を指定し、値を設定する
                IRow row = sheet.GetRow(CoordinateLine) ?? sheet.CreateRow(CoordinateLine);
                ICell cell = row.GetCell(CoordinateColumn) ?? row.CreateCell(CoordinateColumn);
                cell.SetCellValue(setValue);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();

        }
        public StellarRoboFunctionResult ExcelSearch(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            int SearchMatchCount = 0;
            string SearchWord = string.Empty;
            string ExcelKey = string.Empty;
            string SheetName = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 2 || args.Length == 3)
            {
                //ExcelKeyName取得
                ExcelKey = args[0].ToString().Trim();

                //検索値取得
                SearchWord = args[1].ToString().Trim();

                if (args.Length == 3)
                {
                    //SheetName取得
                    SheetName = args[2].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return 0.AsStellarRoboInteger().NoResume();
            }

            #region 入力チェック
            //ExcelKeyは入力されているか？
            if (string.IsNullOrEmpty(ExcelKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelWorkBookKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return 0.AsStellarRoboInteger().NoResume();
            }

            //指定されたExcelKeyは存在しているか？
            if (Excel.Where(x => x.Key == ExcelKey).Count() == 0)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelWorkBookKeyNotFoundError);
                //存在していいないのでエラーとする
                return 0.AsStellarRoboInteger().NoResume();
            }
            //シートは指定されているか？
            if (!string.IsNullOrEmpty(SheetName))
            {
                //指定されたSheetNameは存在しているか？
                if(Excel[ExcelKey].Sheet.Where(x => x.Key== SheetName).Count() == 0)
                {
                    //ステータス設定
                    SetErrorInfo((int)RESULT_CODE.ExcelSheetKeyNotFoundError);
                    //存在していないのでエラーとする
                    return 0.AsStellarRoboInteger().NoResume();
                }
            }
            #endregion

            //Excel取得
            Excel_Data excel = Excel[ExcelKey];

            //初期化
            if(excel.MatchData== null)
            {
                excel.MatchData = new List<Tuple<string, int, int>>();
            }
            else
            {
                excel.MatchData.Clear();
            }

            try
            {
                //Sheetを対象として、検索を行う
                foreach (KeyValuePair<string, ISheet> sheet in excel.Sheet)
                {
                    //Sheet指定はされているか？
                    if (!string.IsNullOrEmpty(SheetName))
                    {
                        //指定Sheet以外は読み飛ばす
                        if (sheet.Key != SheetName)
                        {
                            continue;
                        }
                    }

                    foreach (IRow row in (ISheet)sheet.Value)
                    {
                        foreach (ICell cell in row.Cells)
                        {
                            string CellValue = string.Empty;
                            //セルタイプは色々存在するが、値としてはstring型で保持する
                            #region CellType判別
                            switch (cell.CellType)
                            {
                                case CellType.String:
                                    CellValue = cell.StringCellValue;
                                    break;
                                case CellType.Numeric:
                                    //日付形式化？
                                    if (DateUtil.IsCellDateFormatted(cell))
                                    {
                                        //日付として扱う
                                        CellValue = cell.DateCellValue.ToString("yyyy/MM/dd HH:mm:ss");
                                    }
                                    else
                                    {
                                        //数値として扱う
                                        CellValue = cell.NumericCellValue.ToString();
                                    }
                                    break;
                                case CellType.Boolean:
                                    CellValue = cell.BooleanCellValue.ToString();
                                    break;
                                case CellType.Blank:
                                    CellValue = string.Empty;
                                    break;
                                case CellType.Formula:  //数式は検索対象から外す
                                    CellValue = string.Empty;
                                    break;
                                default:
                                    CellValue = string.Empty;
                                    break;
                            }
                            #endregion

                            //比較する
                            if (CellValue == SearchWord)
                            {
                                //Matchした座標を保存しておく
                                excel.MatchData.Add(new Tuple<string, int, int>(sheet.Key, cell.RowIndex, cell.ColumnIndex));

                                //Match数を加算
                                SearchMatchCount++;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return 0.AsStellarRoboInteger().NoResume();
            }

            //検索出来たか？
            if (SearchMatchCount > 0)
            {
                //データを保存する
                Excel[ExcelKey] = excel;
            }

            //戻り値設定
            return SearchMatchCount.AsStellarRoboInteger().NoResume();
        }
        public StellarRoboFunctionResult ExcelGetSearchResult(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string ExcelKey = string.Empty;
            string ArgsTmp = string.Empty;
            int Index = 0;
            string[] result = { "", "", "" };

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 2)
            {
                //ExcelName取得
                ExcelKey = args[0].ToString().Trim();

                //Index取得
                ArgsTmp = args[1].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return StellarRoboNil.Instance.NoResume();
            }

            #region 入力チェック
            //ExcelKeyは入力されているか？
            if (string.IsNullOrEmpty(ExcelKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelWorkBookKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return StellarRoboNil.Instance.NoResume();
            }
            //Indexは入力されているか？
            if (string.IsNullOrEmpty(ArgsTmp))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelIndexInputNotEnteredError);
                return StellarRoboNil.Instance.NoResume();
            }
            if(!int.TryParse(ArgsTmp,out Index))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                return StellarRoboNil.Instance.NoResume();
            }

            //指定されたExcelKeyは存在しているか？
            if(!Excel.ContainsKey(ExcelKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelWorkBookKeyNotFoundError);
                //存在していいないのでエラーとする
                return StellarRoboNil.Instance.NoResume();
            }
            //Indexの値は範囲内か？
            if ((Excel[ExcelKey].MatchData == null) || (Index < 0 || Index > Excel[ExcelKey].MatchData.Count()))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelSearchResultOutOfRangeError);
                //範囲外なのでエラーとする
                return StellarRoboNil.Instance.NoResume();
            }
            #endregion

            //検索結果を取得する
            try
            {
                Tuple<string, int, int> SearchResult = Excel[ExcelKey].MatchData[Index];
                result[0] = SearchResult.Item1;
                result[1] = SearchResult.Item2.ToString();
                result[2] = SearchResult.Item3.ToString();
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return StellarRoboNil.Instance.NoResume();
            }

            //戻り値設定
            return result.ToStellarRoboArray().NoResume();
        }
        public StellarRoboFunctionResult ExcelGetSearchCount(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string ExcelKey = string.Empty;
            int SearchCount = 0;                    //検索結果件数

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 1)
            {
                //ExcelKeyName取得
                ExcelKey = args[0].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return 0.AsStellarRoboInteger().NoResume();
            }

            #region 入力チェック
            //ExcelKeyは入力されているか？
            if (string.IsNullOrEmpty(ExcelKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelWorkBookKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return 0.AsStellarRoboInteger().NoResume();
            }

            //指定されたExcelKeyは存在しているか？
            if(!Excel.ContainsKey(ExcelKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelWorkBookKeyNotFoundError);
                //存在していいないのでエラーとする
                return 0.AsStellarRoboInteger().NoResume();
            }
            #endregion

            try
            {
                //検索結果件数取得
                SearchCount = (Excel[ExcelKey].MatchData == null) ? 0 : Excel[ExcelKey].MatchData.Count();
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return 0.AsStellarRoboInteger().NoResume();
            }

            //戻り値設定
            return SearchCount.AsStellarRoboInteger().NoResume();
        }
        public StellarRoboFunctionResult ExcelAddSheet(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string ExcelKey = string.Empty;
            string SheetName = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if(args.Length==2)
            {
                //ExcelKeyName取得
                ExcelKey = args[0].ToString().Trim();

                //SheetName取得
                SheetName = args[1].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //ExcelKeyは入力されているか？
            if (string.IsNullOrEmpty(ExcelKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelWorkBookKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return false.AsStellarRoboBoolean().NoResume();
            }

            //SheetKeyは入力されているか？
            if (string.IsNullOrEmpty(SheetName))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelSheetKeyInputNotEnteredError);
                //SheetKeyが入力されていないのでエラー
                return false.AsStellarRoboBoolean().NoResume();
            }

            //指定されたExcelKeyは存在しているか？
            if(!Excel.ContainsKey(ExcelKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelWorkBookKeyNotFoundError);
                //存在していいないのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }
            //シート名は使用されているか？
            if(!Excel[ExcelKey].IsExistsSheetName(SheetName))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelSheetExistError);
                //使用されているのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }
            #endregion

            //Excel取得
            Excel_Data excel = Excel[ExcelKey];

            //領域が作成されていなければ、作成する
            if (excel.Sheet == null) { excel.Sheet = new Dictionary<string, ISheet>(); }

            //新たに作成して追加
            try
            {
                ISheet sheet = excel.WorkBook.CreateSheet(SheetName);
                excel.Sheet.Add(SheetName, sheet);
            }
            catch(Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //データを保存する
            Excel[ExcelKey] = excel;

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        public StellarRoboFunctionResult ExcelRemoveSheet(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string ExcelKey = string.Empty;
            string SheetName = string.Empty;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length == 2)
            {
                //ExcelKey
                ExcelKey = args[0].ToString().Trim();

                //SheetName取得
                SheetName = args[1].ToString().Trim();
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);

                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //ExcelKeyは入力されているか？
            if (string.IsNullOrEmpty(ExcelKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelWorkBookKeyInputNotEnteredError);
                //ExcelKeyが入力されていないのでエラー
                return false.AsStellarRoboBoolean().NoResume();
            }

            //SheetKeyは入力されているか？
            if (string.IsNullOrEmpty(SheetName))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelSheetKeyInputNotEnteredError);
                //SheetKeyが入力されていないのでエラー
                return false.AsStellarRoboBoolean().NoResume();
            }

            //指定されたExcelKeyは存在しているか？
            if(!Excel.ContainsKey(ExcelKey))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelWorkBookKeyNotFoundError);
                //存在していいないのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }
            //SheetNameは存在しているか？
            if(!Excel[ExcelKey].IsExistsSheetName(SheetName))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExcelSheetKeyNotFoundError);
                //存在していないのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }
            #endregion

            try
            {
                //削除する
                Excel[ExcelKey].WorkBook.RemoveName(SheetName);
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();
        }
        #endregion

        #region Explorer関連
        public StellarRoboFunctionResult FolderOpen(StellarRoboContext context,StellarRoboObject self,StellarRoboObject[] args)
        {
            //変数宣言
            string folderPath = string.Empty;
            ProcessStartInfo psi = new ProcessStartInfo();
            string windowStyleTmp = string.Empty;
            int windowStyle = 0;
            string exitsWaitTmp = string.Empty;
            int exitsWaitState = 0;
            bool exitsWait = false;
            string exitsWaitTimeTmp = string.Empty;
            int exitsWaitTime = 0;

            //ステータス設定
            SetErrorInfo((int)RESULT_CODE.Success);

            //引数チェック
            if (args.Length >= 1 && args.Length <= 4)
            {
                //Folder情報取得
                folderPath = args[0].ToString().Trim();

                //ウィインドサイズは指定されているか？
                if (args.Length >= 2)
                {
                    windowStyleTmp = args[1].ToString().Trim();
                }

                //起動されたアプリが終了時まで待つか？
                if (args.Length >= 3)
                {
                    exitsWaitTmp = args[2].ToString().Trim();
                }

                //最大待機時間は設定されているか？
                if (args.Length == 4)
                {
                    exitsWaitTimeTmp = args[3].ToString().Trim();
                }
            }
            else
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ArgumentError);
                //エラー値設定
                return false.AsStellarRoboBoolean().NoResume();
            }

            #region 入力チェック
            //フォルダは入力されているか？
            if (string.IsNullOrEmpty(folderPath))
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.ExplorerFolderInputNotEnteredError);
                return false.AsStellarRoboBoolean().NoResume();
            }
            //フォルダは存在しているか？
            if (!Directory.Exists(folderPath))
            {
                SetErrorInfo((int)RESULT_CODE.ExplorerFolderExistsError);
                return false.AsStellarRoboBoolean().NoResume();
            }

            if (args.Length >= 2)
            {
                //WindowSizeは入力されているか？
                if (string.IsNullOrEmpty(windowStyleTmp))
                {
                    //WindowSizeが入力されていないのでエラー
                    SetErrorInfo((int)RESULT_CODE.ApplicationWindowStyleInputNotEnteredError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                //変換出来ない場合にはエラーとする
                if (!int.TryParse(windowStyleTmp, out windowStyle))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return false.AsStellarRoboBoolean().NoResume();
                }

                switch (windowStyle)
                {
                    //通常起動
                    case 0:
                        psi.WindowStyle = ProcessWindowStyle.Normal;
                        break;
                    //最小化起動
                    case 1:
                        psi.WindowStyle = ProcessWindowStyle.Minimized;
                        break;
                    //最大化起動
                    case 2:
                        psi.WindowStyle = ProcessWindowStyle.Maximized;
                        break;
                    //上記以外の指定は全て通常起動とする
                    default:
                        psi.WindowStyle = ProcessWindowStyle.Normal;
                        break;
                }

            }
            if (args.Length >= 3)
            {
                //起動されたアプリが終了時間まで待つかが入力されているか？
                if (string.IsNullOrEmpty(exitsWaitTmp))
                {
                    //未入力なのでエラーとする
                    SetErrorInfo((int)RESULT_CODE.ApplicationExistsWaitInputNotEnteredError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                //変換出来ない場合にエラーとする
                if (!int.TryParse(exitsWaitTmp, out exitsWaitState))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                exitsWait = (exitsWaitState == 0) ? false : true;
            }
            if (args.Length == 4)
            {
                //最大待機時間は入力されているか？
                if (string.IsNullOrEmpty(exitsWaitTimeTmp))
                {
                    //最大待機時間が未入力なのでエラーとする
                    SetErrorInfo((int)RESULT_CODE.ApplicationExistsWaitTimeInputNotEnteredError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                //変換出来ない場合にはエラーとする
                if (!int.TryParse(exitsWaitTimeTmp, out exitsWaitTime))
                {
                    SetErrorInfo((int)RESULT_CODE.ArgumentIsNumberError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
                if ((exitsWaitTime < 0) || (exitsWaitTime > MAX_TIME))
                {
                    SetErrorInfo((int)RESULT_CODE.ApplicationExistsWaitTimeOutOfRangeError);
                    return false.AsStellarRoboBoolean().NoResume();
                }
            }
            #endregion  

            try
            {
                //指定アプリを起動する
                psi.FileName = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), "explorer.exe");
                psi.Arguments = folderPath.Replace('/','\\');
                Process process = Process.Start(psi);

                //終了待機は行う？
                if (exitsWait)
                {
                    //最大待機時間は指定されているか？
                    if (exitsWaitTime == 0)
                    {
                        //指定されていないので無制限に待ち続ける
                        process.WaitForExit();
                    }
                    else
                    {
                        //指定時間を過ぎれば、終了如何を問わず処理を終了する
                        process.WaitForExit(exitsWaitTime);

                        //タイムアウトで終了したか？
                        if (!process.HasExited)
                        {
                            //タイムアウトで終了した事を返す
                            SetErrorInfo((int)RESULT_CODE.TimeOut);
                            return true.AsStellarRoboBoolean().NoResume();
                        }
                    }
                }
            }
            catch (Exception)
            {
                //ステータス設定
                SetErrorInfo((int)RESULT_CODE.UnexpectedError);
                //予期せぬエラーが発生したのでエラーとする
                return false.AsStellarRoboBoolean().NoResume();
            }

            //戻り値設定
            return true.AsStellarRoboBoolean().NoResume();

        }
        #endregion

        #endregion

        #endregion
    }

    /// <summary>
    /// StellarRoboの実行中の例外を定義します。
    /// </summary>
    public class StellarRoboException : Exception
    {
        /// <summary>
        /// 付与されたオブジェクトを取得します。
        /// </summary>
        public StellarRoboObject Object { get; internal set; }
    }
}
