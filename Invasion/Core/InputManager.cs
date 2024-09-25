using Invasion.Math;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

using System.Windows.Forms;

namespace Invasion.Core
{
    public enum KeyCode
    {
        A = Keys.A,
        B = Keys.B,
        C = Keys.C,
        D = Keys.D,
        E = Keys.E,
        F = Keys.F,
        G = Keys.G,
        H = Keys.H,
        I = Keys.I,
        J = Keys.J,
        K = Keys.K,
        L = Keys.L,
        M = Keys.M,
        N = Keys.N,
        O = Keys.O,
        P = Keys.P,
        Q = Keys.Q,
        R = Keys.R,
        S = Keys.S,
        T = Keys.T,
        U = Keys.U,
        V = Keys.V,
        W = Keys.W,
        X = Keys.X,
        Y = Keys.Y,
        Z = Keys.Z,

        Zero = Keys.D0,
        One = Keys.D1,
        Two = Keys.D2,
        Three = Keys.D3,
        Four = Keys.D4,
        Five = Keys.D5,
        Six = Keys.D6,
        Seven = Keys.D7,
        Eight = Keys.D8,
        Nine = Keys.D9,

        F1 = Keys.F1,
        F2 = Keys.F2,
        F3 = Keys.F3,
        F4 = Keys.F4,
        F5 = Keys.F5,
        F6 = Keys.F6,
        F7 = Keys.F7,
        F8 = Keys.F8,
        F9 = Keys.F9,
        F10 = Keys.F10,
        F11 = Keys.F11,
        F12 = Keys.F12,
        F13 = Keys.F13,
        F14 = Keys.F14,
        F15 = Keys.F15,
        F16 = Keys.F16,
        F17 = Keys.F17,
        F18 = Keys.F18,
        F19 = Keys.F19,
        F20 = Keys.F20,
        F21 = Keys.F21,
        F22 = Keys.F22,
        F23 = Keys.F23,
        F24 = Keys.F24,

        Up = Keys.Up,
        Down = Keys.Down,
        Left = Keys.Left,
        Right = Keys.Right,

        Enter = Keys.Enter,
        Space = Keys.Space,
        Backspace = Keys.Back,
        Tab = Keys.Tab,
        CapsLock = Keys.CapsLock,
        Escape = Keys.Escape,
        Shift = Keys.ShiftKey,
        Control = Keys.ControlKey,
        Alt = Keys.Menu,
        LeftShift = Keys.LShiftKey,
        RightShift = Keys.RShiftKey,
        LeftControl = Keys.LControlKey,
        RightControl = Keys.RControlKey,
        LeftAlt = Keys.LMenu,
        RightAlt = Keys.RMenu,

        Semicolon = Keys.OemSemicolon,
        Comma = Keys.Oemcomma,
        Minus = Keys.OemMinus,
        Period = Keys.OemPeriod,
        Question = Keys.OemQuestion,
        Tilde = Keys.Oemtilde,
        OpenBrackets = Keys.OemOpenBrackets,
        Pipe = Keys.OemPipe,
        CloseBrackets = Keys.OemCloseBrackets,
        Quotes = Keys.OemQuotes,
        Plus = Keys.Oemplus,

        NumPad_Zero = Keys.NumPad0,
        NumPad_One = Keys.NumPad1,
        NumPad_Two = Keys.NumPad2,
        NumPad_Three = Keys.NumPad3,
        NumPad_Four = Keys.NumPad4,
        NumPad_Five = Keys.NumPad5,
        NumPad_Six = Keys.NumPad6,
        NumPad_Seven = Keys.NumPad7,
        NumPad_Eight = Keys.NumPad8,
        NumPad_Nine = Keys.NumPad9,
        Decimal = Keys.Decimal,
        Add = Keys.Add,
        Subtract = Keys.Subtract,
        Multiply = Keys.Multiply,
        Divide = Keys.Divide,

        NumLock = Keys.NumLock,
        ScrollLock = Keys.Scroll,

        PrintScreen = Keys.PrintScreen,
        Pause = Keys.Pause,
        Insert = Keys.Insert,
        Delete = Keys.Delete,
        Home = Keys.Home,
        End = Keys.End,
        PageUp = Keys.PageUp,
        PageDown = Keys.PageDown,

        LeftWindows = Keys.LWin,
        RightWindows = Keys.RWin,
        Apps = Keys.Apps,

        BrowserBack = Keys.BrowserBack,
        BrowserForward = Keys.BrowserForward,
        BrowserRefresh = Keys.BrowserRefresh,
        BrowserStop = Keys.BrowserStop,
        BrowserSearch = Keys.BrowserSearch,
        BrowserFavorites = Keys.BrowserFavorites,
        BrowserHome = Keys.BrowserHome,

        VolumeMute = Keys.VolumeMute,
        VolumeDown = Keys.VolumeDown,
        VolumeUp = Keys.VolumeUp,
        MediaNextTrack = Keys.MediaNextTrack,
        MediaPreviousTrack = Keys.MediaPreviousTrack,
        MediaStop = Keys.MediaStop,
        MediaPlayPause = Keys.MediaPlayPause,

        LaunchMail = Keys.LaunchMail,
        LaunchApplication1 = Keys.LaunchApplication1,
        LaunchApplication2 = Keys.LaunchApplication2,
    }

    public enum MouseCode
    {
        Left,
        Right,
        Middle,
    }

    public enum KeyState
    {
        Pressed,
        Released,
    }

    public enum MouseState
    {
        Pressed,
        Released,
    }

    public static class InputManager
    {
        public static Vector2f MousePosition { get; private set; } = Vector2f.Zero;
        public static bool MouseLeftPressed { get; private set; }
        public static bool MouseRightPressed { get; private set; }
        public static bool MouseLeftClick { get; private set; }
        public static bool MouseRightClick { get; private set; }
        public static int MouseWheelDelta { get; private set; }
        public static Vector2f MouseDelta { get; private set; } = Vector2f.Zero;
        public static float DeltaTime { get; private set; }

        private static readonly float smoothingFactor = 0.1f;
        private static Vector2f smoothedMouseDelta = Vector2f.Zero;

        private static Vector2f lastMousePosition = Vector2f.Zero;
        private static bool lastMouseLeftState;
        private static bool lastMouseRightState;

        private static Form Form { get; set; } = null!;
        public static bool CursorMode { get; private set; } = true;

        private static readonly HashSet<KeyCode> CurrentPressedKeys = [];
        private static readonly HashSet<KeyCode> NewlyPressedKeys = [];
        private static readonly HashSet<KeyCode> NewlyReleasedKeys = [];

        private static bool cursorIsHidden = false;
        private static readonly Stopwatch Stopwatch = new();

        public static void Initialize(Form form)
        {
            Form = form;

            form.KeyDown += (sender, arguments) =>
            {
                if (!CurrentPressedKeys.Contains((KeyCode)arguments.KeyCode))
                    NewlyPressedKeys.Add((KeyCode)arguments.KeyCode);

                CurrentPressedKeys.Add((KeyCode)arguments.KeyCode);
            };

            form.KeyUp += (sender, arguments) =>
            {
                CurrentPressedKeys.Remove((KeyCode)arguments.KeyCode);
                NewlyReleasedKeys.Add((KeyCode)arguments.KeyCode);
            };

            form.MouseMove += (sender, arguments) =>
            {
                Vector2i newMousePosition = new(arguments.X, arguments.Y);
                Vector2f delta = (Vector2f)newMousePosition - lastMousePosition;

                if (!CursorMode)
                {
                    MouseDelta = delta;
                    smoothedMouseDelta = Vector2f.Lerp(smoothedMouseDelta, delta, smoothingFactor);
                    lastMousePosition = (Vector2f)newMousePosition;

                    Cursor.Position = Form.PointToScreen(new Point(Form.ClientSize.Width / 2, Form.ClientSize.Height / 2));
                    lastMousePosition = new(Form.ClientSize.Width / 2, Form.ClientSize.Height / 2);
                    MouseDelta = Vector2f.Zero;
                }
            };

            form.MouseDown += (sender, arguments) =>
            {
                if (arguments.Button == MouseButtons.Left)
                    MouseLeftPressed = true;
                else if (arguments.Button == MouseButtons.Right)
                    MouseRightPressed = true;
            };

            form.MouseUp += (sender, arguments) =>
            {
                if (arguments.Button == MouseButtons.Left)
                {
                    MouseLeftPressed = false;
                    MouseLeftClick = !lastMouseLeftState;
                }
                else if (arguments.Button == MouseButtons.Right)
                {
                    MouseRightPressed = false;
                    MouseRightClick = !lastMouseRightState;
                }
            };

            form.MouseWheel += (sender, arguments) =>
            {
                MouseWheelDelta += arguments.Delta;
            };

            Stopwatch.Start();
        }

        public static void SetCursorMode(bool hidden)
        {
            if (hidden && !cursorIsHidden)
            {
                Cursor.Hide();
                cursorIsHidden = true;
                Cursor.Position = Form.PointToScreen(new Point(Form.ClientSize.Width / 2, Form.ClientSize.Height / 2));
                lastMousePosition = new(Form.ClientSize.Width / 2, Form.ClientSize.Height / 2);
            }
            else if (!hidden && cursorIsHidden)
            {
                Cursor.Show();
                cursorIsHidden = false;
            }

            CursorMode = !hidden;
        }

        public static bool GetKeyHeld(KeyCode key)
        {
            return CurrentPressedKeys.Contains(key);
        }

        public static bool GetKey(KeyCode key, KeyState state)
        {
            if (state == KeyState.Pressed)
                return NewlyPressedKeys.Contains(key);
            else
                return NewlyReleasedKeys.Contains(key);
        }

        public static bool IsCursorWithinBounds(Vector3f min, Vector3f max)
        {
            Point cursorPosition = Cursor.Position;

            return (cursorPosition.X >= min.X && cursorPosition.X <= max.X &&
                    cursorPosition.Y >= min.Y && cursorPosition.Y <= max.Y);
        }

        public static Vector2f GetMouseMovementOffsets()
        {
            return smoothedMouseDelta;
        }

        public static void ResetMouseDelta()
        {
            MouseDelta = Vector2f.Zero;
            smoothedMouseDelta = Vector2f.Zero;
        }

        public static void Update()
        {
            NewlyPressedKeys.Clear();
            NewlyReleasedKeys.Clear();

            MouseWheelDelta = 0;
            lastMouseLeftState = MouseLeftPressed;
            lastMouseRightState = MouseRightPressed;
            MouseLeftClick = false;
            MouseRightClick = false;

            DeltaTime = (float)Stopwatch.Elapsed.TotalSeconds;
            Stopwatch.Restart();
        }
    }
}
