using System.Threading;
using WhiteMagic.WinAPI;
using WhiteMagic.WinAPI.Structures;

namespace WhiteMagic.Windows
{
    public class SendInputMouse : IMouse
    {
        public SendInputMouse(RemoteWindow window)
        {
            Window = window;
        }

        protected RemoteWindow Window { get; set; }

        public void ClickLeft()
        {
            PressLeft();
            ReleaseLeft();
        }

        public void ClickMiddle()
        {
            PressMiddle();
            ReleaseMiddle();
        }

        public void ClickRight()
        {
            PressRight();
            ReleaseRight();
        }

        public void DoubleClickLeft()
        {
            ClickLeft();
            Thread.Sleep(10);
            ClickLeft();
        }

        public void MoveTo(int x, int y)
        {
            MoveToAbsolute(Window.X + x, Window.Y + y);
        }

        public void PressLeft()
        {
            var input = CreateInput();
            input.Mouse.Flags = MouseFlags.LeftDown;
            WindowHelper.SendInput(input);
        }


        public void PressMiddle()
        {
            var input = CreateInput();
            input.Mouse.Flags = MouseFlags.MiddleDown;
            WindowHelper.SendInput(input);
        }

        public void PressRight()
        {
            var input = CreateInput();
            input.Mouse.Flags = MouseFlags.RightDown;
            WindowHelper.SendInput(input);
        }

        public void ReleaseLeft()
        {
            var input = CreateInput();
            input.Mouse.Flags = MouseFlags.LeftUp;
            WindowHelper.SendInput(input);
        }

        public void ReleaseMiddle()
        {
            var input = CreateInput();
            input.Mouse.Flags = MouseFlags.MiddleUp;
            WindowHelper.SendInput(input);
        }

        public void ReleaseRight()
        {
            var input = CreateInput();
            input.Mouse.Flags = MouseFlags.RightUp;
            WindowHelper.SendInput(input);
        }

        public void ScrollHorizontally(int delta = 120)
        {
            var input = CreateInput();
            input.Mouse.Flags = MouseFlags.HWheel;
            input.Mouse.MouseData = delta;
            WindowHelper.SendInput(input);
        }

        public void ScrollVertically(int delta = 120)
        {
            var input = CreateInput();
            input.Mouse.Flags = MouseFlags.Wheel;
            input.Mouse.MouseData = delta;
            WindowHelper.SendInput(input);
        }

        protected void MoveToAbsolute(int x, int y)
        {
            var input = CreateInput();
            input.Mouse.DeltaX = CalculateAbsoluteCoordinateX(x);
            input.Mouse.DeltaY = CalculateAbsoluteCoordinateY(y);
            input.Mouse.Flags = MouseFlags.Move | MouseFlags.Absolute;
            input.Mouse.MouseData = 0;
            WindowHelper.SendInput(input);
        }

        static int CalculateAbsoluteCoordinateX(int x)
        {
            return x*65536/User32.GetSystemMetrics(SystemMetrics.CxScreen);
        }

        static int CalculateAbsoluteCoordinateY(int y)
        {
            return y*65536/User32.GetSystemMetrics(SystemMetrics.CyScreen);
        }

        static Input CreateInput()
        {
            return new Input(InputTypes.Mouse);
        }
    }
}