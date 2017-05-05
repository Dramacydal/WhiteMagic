using System;
using System.Windows.Forms;
using WhiteMagic.WinAPI.Structures.Input;

namespace WhiteMagic.Input
{
    public class ModifierToggler : IDisposable
    {
        private IKeyboardInput KeyboardInput { get; }
        private Modifiers Mask { get; set; }

        public ModifierToggler(IKeyboardInput KeyboardInput, Modifiers Mask)
        {
            this.KeyboardInput = KeyboardInput;
            this.Mask = Mask;

            if (Mask.HasFlag(Modifiers.Alt))
                KeyboardInput.SendKey(Keys.Menu, false);
            if (Mask.HasFlag(Modifiers.Ctrl))
                KeyboardInput.SendKey(Keys.ControlKey, false);
            if (Mask.HasFlag(Modifiers.Shift))
                KeyboardInput.SendKey(Keys.ShiftKey, false);
        }

        public void Reset()
        {
            if (Mask.HasFlag(Modifiers.Alt))
                KeyboardInput.SendKey(Keys.Menu, true);
            if (Mask.HasFlag(Modifiers.Ctrl))
                KeyboardInput.SendKey(Keys.ControlKey, true);
            if (Mask.HasFlag(Modifiers.Shift))
                KeyboardInput.SendKey(Keys.ShiftKey, true);

            Mask = Modifiers.None;
        }

        public void Dispose() => Reset();
    }
}
