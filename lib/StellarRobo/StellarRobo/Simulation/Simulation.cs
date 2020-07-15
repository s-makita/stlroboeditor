using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarLink.Windows.Simulation
{
    public static class InputSimulator
    {
        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "MapVirtualKeyA")]
            public extern static int MapVirtualKey(int wCode, int wMapType);
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public extern static void SendInput(int nInputs, Input[] pInputs, int cbsize);
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct MouseInput
        {
            public int X;
            public int Y;
            public int Data;
            public int Flags;
            public int Time;
            public int ExtraInfo;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct KeyboardInput
        {
            public short VirtualKey;
            public short ScanCode;
            public int Flags;
            public int Time;
            public int ExtraInfo;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct HardwareInput
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        public struct Input
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            public int Type;

            [System.Runtime.InteropServices.FieldOffset(4)]
            public MouseInput Mouse;

            [System.Runtime.InteropServices.FieldOffset(4)]
            public KeyboardInput Keyboard;

            [System.Runtime.InteropServices.FieldOffset(4)]
            public HardwareInput Hardware;
        }

        public enum MouseStroke
        {
            MOVE = 0x0001,
            LEFT_DOWN = 0x0002,
            LEFT_UP = 0x0004,
            RIGHT_DOWN = 0x0008,
            RIGHT_UP = 0x0010,
            MIDDLE_DOWN = 0x0020,
            MIDDLE_UP = 0x0040,
            X_DOWN = 0x0080,
            X_UP = 0x0100,
            WHEEL = 0x0800
        }

        public enum KeyboardStroke
        {
            KEY_DOWN = 0x0000,
            KEY_UP = 0x0002
        }

        private const int KBD_UNICODE = 0x0004;

        public static void AddMouseInput(ref System.Collections.Generic.List<Input> inputs, MouseStroke flag, int data, bool absolute, int x, int y)
        {
            AddMouseInput(ref inputs, new System.Collections.Generic.List<MouseStroke> { flag }, data, absolute, x, y);
        }

        public static void AddMouseInput(ref System.Collections.Generic.List<Input> inputs, System.Collections.Generic.List<MouseStroke> flags, int data, bool absolute, int x, int y)
        {
            if (flags == null)
            {
                return;
            }

            int mouseFlags = 0;

            foreach (MouseStroke f in flags)
            {
                mouseFlags |= (int)f;
            }

            if (absolute)
            {
                // ABSOLUTE = 0x8000
                mouseFlags |= 0x8000;

                x *= (65535 / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width);
                y *= (65535 / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
            }

            AddMouseInput(ref inputs, mouseFlags, data, x, y, 0, 0);
        }

        public static void AddMouseInput(ref System.Collections.Generic.List<Input> inputs, int flags, int data, int x, int y, int time, int extraInfo)
        {
            Input input = new Input();
            input.Type = 0; // MOUSE = 0
            input.Mouse.Flags = flags;
            input.Mouse.Data = data;
            input.Mouse.X = x;
            input.Mouse.Y = y;
            input.Mouse.Time = time;
            input.Mouse.ExtraInfo = extraInfo;

            inputs.Add(input);
        }

        public static void AddKeyboardInput(ref System.Collections.Generic.List<Input> inputs, string srcStr)
        {
            if (System.String.IsNullOrEmpty(srcStr))
            {
                return;
            }

            foreach (char s in srcStr)
            {
                AddKeyboardInput(ref inputs, (int)KeyboardStroke.KEY_DOWN | KBD_UNICODE, 0, (short)s, 0, 0);
                AddKeyboardInput(ref inputs, (int)KeyboardStroke.KEY_UP | KBD_UNICODE, 0, (short)s, 0, 0);
            }
        }

        public static void AddKeyboardInput(ref System.Collections.Generic.List<Input> inputs, KeyboardStroke flags, System.Windows.Forms.Keys key)
        {
            int keyboardFlags = (int)flags | KBD_UNICODE;
            short virtualKey = (short)key;
            short scanCode = (short)NativeMethods.MapVirtualKey(virtualKey, 0);

            AddKeyboardInput(ref inputs, keyboardFlags, virtualKey, scanCode, 0, 0);
        }

        public static void AddKeyboardInput(ref System.Collections.Generic.List<Input> inputs, int flags, short virtualKey, short scanCode, int time, int extraInfo)
        {
            Input input = new Input();
            input.Type = 1; // KEYBOARD = 1
            input.Keyboard.Flags = flags;
            input.Keyboard.VirtualKey = virtualKey;
            input.Keyboard.ScanCode = scanCode;
            input.Keyboard.Time = time;
            input.Keyboard.ExtraInfo = extraInfo;

            inputs.Add(input);
        }

        public static void SendInput(System.Collections.Generic.List<Input> inputs)
        {
            Input[] inputArray = inputs.ToArray();
            SendInput(inputArray);
        }

        public static void SendInput(Input[] inputs)
        {
            NativeMethods.SendInput(inputs.Length, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        }
    }
}