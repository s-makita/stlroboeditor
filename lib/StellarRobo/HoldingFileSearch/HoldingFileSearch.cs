using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace HoldingSearch
{

    #region クラス定義
    public class HoldingFileInfo
    {
        public IntPtr Handle { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public IntPtr Object { get; set; }
        public IntPtr UniqueProcessId { get; set; }
        public uint GrantedAccess { get; set; }
        public ushort CreatorBackTraceIndex { get; set; }
        public ushort ObjectTypeIndex { get; set; }
        public uint HandleAttributes { get; set; }
        public uint Reserved { get; set; }
    }
    #endregion

    public static class HoldingFileSearch
    {
        #region 構造体定義
        [StructLayout(LayoutKind.Sequential)]
        struct GENERIC_MAPPING
        {
            public int GenericRead;
            public int GenericWrite;
            public int GenericExecute;
            public int GenericAll;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct OBJECT_TYPE_INFORMATION
        {
            public UNICODE_STRING Name;
            public uint TotalNumberOfObjects;
            public uint TotalNumberOfHandles;
            public uint TotalPagedPoolUsage;
            public uint TotalNonPagedPoolUsage;
            public uint TotalNamePoolUsage;
            public uint TotalHandleTableUsage;
            public uint HighWaterNumberOfObjects;
            public uint HighWaterNumberOfHandles;
            public uint HighWaterPagedPoolUsage;
            public uint HighWaterNonPagedPoolUsage;
            public uint HighWaterNamePoolUsage;
            public uint HighWaterHandleTableUsage;
            public uint InvalidAttributes;
            public GENERIC_MAPPING GenericMapping;
            public uint ValidAccess;
            public byte SecurityRequired;
            public byte MaintainHandleCount;
            public ushort MaintainTypeList;
            public int PoolType;
            public int PagedPoolUsage;
            public int NonPagedPoolUsage;
        }
        struct _SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX
        {
#pragma warning disable CS0649
            public IntPtr Object;
            public IntPtr UniqueProcessId;
            public IntPtr HandleValue;
            public uint GrantedAccess;
            public ushort CreatorBackTraceIndex;
            public ushort ObjectTypeIndex;
            public uint HandleAttributes;
            public uint Reserved;
#pragma warning restore CS0649
        }
        [StructLayout(LayoutKind.Sequential)]
        struct OBJECT_NAME_INFORMATION
        {
            public UNICODE_STRING Name;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct UNICODE_STRING
        {
            private IntPtr reserved;
            public IntPtr Buffer;

            public ushort Length
            {
                get { return (ushort)(reserved.ToInt64() & 0xffff); }
            }
            public ushort MaximumLength
            {
                get { return (ushort)(reserved.ToInt64() >> 16); }
            }

            public override string ToString()
            {
                if (Buffer == IntPtr.Zero)
                    return "";
                return Marshal.PtrToStringUni(Buffer, Wcslen());
            }

            public int Wcslen()
            {
                unsafe
                {
                    ushort* p = (ushort*)Buffer.ToPointer();
                    for (ushort i = 0; i < Length; i++)
                    {
                        if (p[i] == 0)
                            return i;
                    }
                    return Length;
                }
            }

        }
        #endregion

        #region API定義

        #region NtQuerySystemInformation
        [DllImport("ntdll.dll")]
        static extern NT_STATUS NtQuerySystemInformation(int SystemInformationClass, IntPtr SystemInformation, int SystemInformationLength, out int ReturnLength);
        #endregion

        #region NtDuplicateObject
        [DllImport("ntdll.dll")]
        static extern NT_STATUS NtDuplicateObject(IntPtr SourceProcessHandle, IntPtr SourceHandle, IntPtr TargetProcessHandle, out IntPtr TargetHandle, uint DesiredAccess, uint Attributes, uint Options);
        #endregion

        #region NtQueryObject
        [DllImport("ntdll.dll")]
        static extern NT_STATUS NtQueryObject(IntPtr ObjectHandle, ObjectInformationClass ObjectInformationClass, IntPtr ObjectInformation, int ObjectInformationLength, out int returnLength);
        #endregion

        #region QueryDosDevice
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);
        #endregion

        #region CloseHandle
        [DllImport("kernel32.dll")]
        static extern int CloseHandle(IntPtr hObject);
        #endregion

        #region OpenProcess
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);
        #endregion

        #endregion

        #region 列挙型定義
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            PROCESS_VM_READ = 0x10,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }
        enum NT_STATUS : uint
        {
            SUCCESS = 0x00000000,
            BUFFER_OVERFLOW = 0x80000005,
            INFO_LENGTH_MISMATCH = 0xC0000004
        }
        enum ObjectInformationClass : int
        {
            ObjectBasicInformation = 0,
            ObjectNameInformation = 1,
            ObjectTypeInformation = 2,
            ObjectAllTypesInformation = 3,
            ObjectHandleInformation = 4
        }

        #endregion

        #region 定数定義
        const int MAX_PATH = 260;
        const int SystemExtendedHandleInformation = 64;
        #endregion

        public static IEnumerable<HoldingFileInfo> HoldingFileName(int pid)
        {
            //PIDよりプロセスを取得
            using (var proc = Process.GetProcessById(pid))
            {
                IntPtr hProcess = OpenProcess(ProcessAccessFlags.DupHandle, false, pid);

                foreach (var SystemHandleTableEntryInfo in EnumHandles(pid))
                {
                    IntPtr hObj = IntPtr.Zero;
                    string hType = null;
                    string hName = null;

                    try
                    {
                        if (SystemHandleTableEntryInfo.HandleValue.ToInt32() == 1104)
                        {
                            Debug.Print("Debug");
                        }
                        if (!NT_SUCCESS(NtDuplicateObject(hProcess, SystemHandleTableEntryInfo.HandleValue, Process.GetCurrentProcess().Handle, out hObj, 0, 0, 0)))
                        {
                            continue;
                        }
                        using (var nto1 = new NtObject(hObj, ObjectInformationClass.ObjectTypeInformation, typeof(OBJECT_TYPE_INFORMATION)))
                        {
                            try
                            {
                                //Bufferの値が0x00000000の場合処理を行わない
                                if ((int)nto1.Buffer == 0) { continue; }

                                var oti = ObjectTypeInformationFromBuffer(nto1.Buffer);
                                hType = oti.Name.ToString();
                            }
                            catch (Exception e)
                            {
                                Debug.Print(e.Message);
                                Debug.Print(e.Source);
                                Debug.Print(e.StackTrace);
                            }
                        }
                        if (hType.Equals("File"))
                        {
                            using (var nto2 = new NtObject(hObj, ObjectInformationClass.ObjectNameInformation, typeof(OBJECT_NAME_INFORMATION)))
                            {
                                try
                                {
                                    //Bufferの値が0x00000000の場合処理を行わない
                                    if ((int)nto2.Buffer == 0) { continue; }

                                    var oni = ObjectNameInformationFromBuffer(nto2.Buffer);
                                    if (hType.Equals("File"))
                                    {
                                        hName = GetRegularFileNameFromDevice(oni.Name.ToString());
                                    }
                                    else
                                    {
                                        hName = oni.Name.ToString();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Debug.Print(e.Message);
                                    Debug.Print(e.Source);
                                    Debug.Print(e.StackTrace);
                                }
                            }

                        }
                        else
                        {
                            using (var nto2 = new NtObject(hObj, ObjectInformationClass.ObjectNameInformation, typeof(OBJECT_NAME_INFORMATION)))
                            {
                                try
                                {
                                    //Bufferの値が0x00000000の場合処理を行わない
                                    if ((int)nto2.Buffer == 0) { continue; }

                                    var oni = ObjectNameInformationFromBuffer(nto2.Buffer);
                                    if (hType.Equals("File"))
                                    {
                                        hName = GetRegularFileNameFromDevice(oni.Name.ToString());
                                    }
                                    else
                                    {
                                        hName = oni.Name.ToString();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Debug.Print(e.Message);
                                    Debug.Print(e.Source);
                                    Debug.Print(e.StackTrace);
                                }
                            }
                        }

                        //空なら読み飛ばす
                        if(String.IsNullOrEmpty(hName)) { continue; }

                        //取得したファイル名を保存
                        HoldingFileInfo holdingFileInfo = new HoldingFileInfo();
                        holdingFileInfo.Handle = SystemHandleTableEntryInfo.HandleValue;
                        holdingFileInfo.Type = hType;
                        holdingFileInfo.Name = hName;
                        holdingFileInfo.CreatorBackTraceIndex = SystemHandleTableEntryInfo.CreatorBackTraceIndex;
                        holdingFileInfo.GrantedAccess = SystemHandleTableEntryInfo.GrantedAccess;
                        holdingFileInfo.HandleAttributes = SystemHandleTableEntryInfo.HandleAttributes;
                        holdingFileInfo.Object = SystemHandleTableEntryInfo.Object;
                        holdingFileInfo.ObjectTypeIndex = SystemHandleTableEntryInfo.ObjectTypeIndex;
                        holdingFileInfo.Reserved = SystemHandleTableEntryInfo.Reserved;
                        holdingFileInfo.UniqueProcessId = SystemHandleTableEntryInfo.UniqueProcessId;
                        yield return holdingFileInfo;
                    }
                    finally
                    {
                        CloseHandle(hObj);
                    }

                }
                CloseHandle(hProcess);
            }
        }

        static OBJECT_TYPE_INFORMATION ObjectTypeInformationFromBuffer(IntPtr buffer)
        {
            unsafe
            {
                return *(OBJECT_TYPE_INFORMATION*)buffer.ToPointer();
            }
        }

        static OBJECT_NAME_INFORMATION ObjectNameInformationFromBuffer(IntPtr buffer)
        {
            unsafe
            {
                return *(OBJECT_NAME_INFORMATION*)buffer.ToPointer();
            }
        }

        class NtObject : IDisposable
        {
            public NtObject(IntPtr hObj, ObjectInformationClass infoClass, Type type)
            {
                Init(hObj, infoClass, Marshal.SizeOf(type));
            }

            public NtObject(IntPtr hObj, ObjectInformationClass infoClass, int estimatedSize)
            {
                Init(hObj, infoClass, estimatedSize);
            }

            public void Init(IntPtr hObj, ObjectInformationClass infoClass, int estimatedSize)
            {
                Close();

                Buffer = Query(hObj, infoClass, estimatedSize);
            }

            public void Close()
            {
                if (Buffer != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(Buffer);
                    Buffer = IntPtr.Zero;
                }
            }

            public void Dispose()
            {
                Close();
            }

            public IntPtr Buffer { get; private set; }

            public static IntPtr Query(IntPtr hObj, ObjectInformationClass infoClass, int estimatedSize)
            {
                int size = estimatedSize;
                IntPtr buf = Marshal.AllocCoTaskMem(size);
                int retsize = 0;
                while (true)
                {
                    var ret = NtQueryObject(hObj, infoClass, buf, size, out retsize);
                    if (NT_SUCCESS(ret))
                        return buf;
                    if (ret == NT_STATUS.INFO_LENGTH_MISMATCH || ret == NT_STATUS.BUFFER_OVERFLOW)
                    {
                        buf = Marshal.ReAllocCoTaskMem(buf, retsize);
                        size = retsize;
                    }
                    else
                    {
                        Marshal.FreeCoTaskMem(buf);
                        return IntPtr.Zero;
                    }
                }
            }
        }

        static _SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX SystemExtendedHandleFromPtr(IntPtr ptr, int offset)
        {
            unsafe
            {
                var p = (byte*)ptr.ToPointer() + offset;
                return *(_SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX*)p;
            }
        }

        static int lastSizeUsed = 0x10000;

        static IEnumerable<_SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX> EnumHandles(int processId)
        {
            int size = lastSizeUsed;
            IntPtr buffer = Marshal.AllocCoTaskMem(size);
            try
            {
                int required;
                while (NtQuerySystemInformation(SystemExtendedHandleInformation, buffer, size, out required) == NT_STATUS.INFO_LENGTH_MISMATCH)
                {
                    size = required;
                    buffer = Marshal.ReAllocCoTaskMem(buffer, size);
                }

                if (lastSizeUsed < size) lastSizeUsed = size;

                int entrySize = Marshal.SizeOf(typeof(_SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX));
                int offset = Marshal.SizeOf(typeof(IntPtr)) * 2;
                int handleCount = Marshal.ReadInt32(buffer);

                for (int i = 0; i < handleCount; i++)
                {
                    var shi = SystemExtendedHandleFromPtr(buffer, offset + entrySize * i);
                    if (shi.UniqueProcessId != new IntPtr(processId))
                        continue;

                    yield return shi;
                }
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(buffer);
            }
        }

        static bool NT_SUCCESS(NT_STATUS status)
        {
            return ((uint)status & 0x80000000) == 0;
        }

        static readonly string NETWORK_PREFIX = @"\Device\Mup\";

        static string GetRegularFileNameFromDevice(string strRawName)
        {
            if (strRawName.StartsWith(NETWORK_PREFIX))
                return @"\\" + strRawName.Substring(NETWORK_PREFIX.Length);

            string strFileName = strRawName;
            foreach (var drvPath in Environment.GetLogicalDrives())
            {
                var drv = drvPath.Substring(0, 2);
                var sb = new StringBuilder(MAX_PATH);
                if (QueryDosDevice(drv, sb, MAX_PATH) == 0)
                    return strRawName;

                string drvRoot = sb.ToString();
                if (strFileName.StartsWith(drvRoot))
                {
                    strFileName = drv + strFileName.Substring(drvRoot.Length);
                    break;
                }
            }
            return strFileName;
        }
    }
}
