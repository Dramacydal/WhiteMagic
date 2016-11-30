using System;
using System.Collections.Generic;
using System.Linq;
using WhiteMagic.WinAPI.Structures;

namespace WhiteMagic.Windows
{
    public class RemoteWindow : IEquatable<RemoteWindow>, IWindow
    {
        public RemoteWindow(IntPtr handle)
        {
            Handle = handle;
            Keyboard = new MessageKeyboard(this);
            Mouse = new SendInputMouse(this);
        }

        protected IEnumerable<IntPtr> ChildrenHandles => WindowHelper.EnumChildWindows(Handle);

        public bool Equals(RemoteWindow other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || Handle.Equals(other.Handle);
        }

        public IEnumerable<IWindow> Children
        {
            get { return ChildrenHandles.Select(handle => new RemoteWindow(handle)); }
        }

        public string ClassName => WindowHelper.GetClassName(Handle);

        public IntPtr Handle { get; }

        public int Height
        {
            get { return Placement.NormalPosition.Height; }
            set
            {
                var p = Placement;
                p.NormalPosition.Height = value;
                Placement = p;
            }
        }

        public bool IsActivated => WindowHelper.GetForegroundWindow() == Handle;

        public IKeyboard Keyboard { get; set; }

        public IMouse Mouse { get; set; }

        public WindowPlacement Placement
        {
            get { return WindowHelper.GetWindowPlacement(Handle); }
            set { WindowHelper.SetWindowPlacement(Handle, value); }
        }

        public WindowStates State
        {
            get { return Placement.ShowCmd; }
            set { WindowHelper.ShowWindow(Handle, value); }
        }

        public string Title
        {
            get { return WindowHelper.GetWindowText(Handle); }
            set { WindowHelper.SetWindowText(Handle, value); }
        }

        public int Width
        {
            get { return Placement.NormalPosition.Width; }
            set
            {
                var p = Placement;
                p.NormalPosition.Width = value;
                Placement = p;
            }
        }

        public int X
        {
            get { return Placement.NormalPosition.Left; }
            set
            {
                var p = Placement;
                p.NormalPosition.Right = value + p.NormalPosition.Width;
                p.NormalPosition.Left = value;
                Placement = p;
            }
        }

        public int Y
        {
            get { return Placement.NormalPosition.Top; }
            set
            {
                var p = Placement;
                p.NormalPosition.Bottom = value + p.NormalPosition.Height;
                p.NormalPosition.Top = value;
                Placement = p;
            }
        }

        public void Activate()
        {
            WindowHelper.SetForegroundWindow(Handle);
        }

        public void Close()
        {
            PostMessage(WM.CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        public void Flash()
        {
            WindowHelper.FlashWindow(Handle);
        }

        public void Flash(int count, TimeSpan timeout, FlashWindowFlags flags = FlashWindowFlags.All)
        {
            WindowHelper.FlashWindowEx(Handle, flags, count, timeout);
        }

        public virtual void Dispose()
        {
        }
        public void PostMessage(WM message, UIntPtr wParam, UIntPtr lParam)
        {
            WindowHelper.PostMessage(Handle,(int) message, wParam, lParam);
        }

        public void PostMessage(int message, UIntPtr wParam, UIntPtr lParam)
        {
            WindowHelper.PostMessage(Handle, message, wParam, lParam);
        }

        public void PostMessage(WM message, IntPtr wParam, IntPtr lParam)
        {
            WindowHelper.PostMessage(Handle, message, wParam, lParam);
        }

        public void PostMessage(int message, IntPtr wParam, IntPtr lParam)
        {
            WindowHelper.PostMessage(Handle, message, wParam, lParam);
        }

        public IntPtr SendMessage(WM message, IntPtr wParam, IntPtr lParam)
        {
            return WindowHelper.SendMessage(Handle, message, wParam, lParam);
        }

        public IntPtr SendMessage(int message, IntPtr wParam, IntPtr lParam)
        {
            return WindowHelper.SendMessage(Handle, message, wParam, lParam);
        }

        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return (obj.GetType() == GetType()) && Equals((RemoteWindow) obj);
        }

        public static bool operator ==(RemoteWindow left, RemoteWindow right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RemoteWindow left, RemoteWindow right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"Title = {Title} ClassName = {ClassName}";
        }
    }
}