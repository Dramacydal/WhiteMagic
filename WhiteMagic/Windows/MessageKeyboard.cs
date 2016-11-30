using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhiteMagic.WinAPI;
using WhiteMagic.WinAPI.Structures;

namespace WhiteMagic.Windows
{
    public class MessageKeyboard : IKeyboard
    {
        protected static readonly List<Tuple<IntPtr, Keys>> PressedKeys = new List<Tuple<IntPtr, Keys>>();

        public MessageKeyboard(RemoteWindow window)
        {
            Window = window;
        }

        protected RemoteWindow Window { get; set; }

        public void Press(Keys key, TimeSpan interval)
        {
            var tuple = Tuple.Create(Window.Handle, key);
            if (PressedKeys.Contains(tuple))
                return;

            PressedKeys.Add(tuple);
            Task.Run(async () =>
            {
                while (PressedKeys.Contains(tuple))
                {
                    Press(key);
                    await Task.Delay(interval);
                }
            });
        }

        public void PressRelease(Keys key)
        {
            Press(key);
            Thread.Sleep(10);
            Release(key);
        }

        public void Write(string text, params object[] args)
        {
            foreach (var character in string.Format(text, args))
                Write(character);
        }

        public void Press(Keys key)
        {
            Window.PostMessage((int) WM.KEYDOWN, new UIntPtr((uint) key), MakeKeyParameter(key, false));
        }

        public void Write(char character)
        {
            Window.PostMessage((int) WM.CHAR, new IntPtr(character), IntPtr.Zero);
        }

        public virtual void Release(Keys key)
        {
            var tuple = Tuple.Create(Window.Handle, key);
            if (PressedKeys.Contains(tuple))
                PressedKeys.Remove(tuple);
            Window.PostMessage(WM.KEYUP, (UIntPtr)key, MakeKeyParameter(key, true));
        }

        static UIntPtr MakeKeyParameter(Keys key, bool keyUp, bool fRepeat, uint cRepeat, bool altDown, bool fExtended)
        {
            // Create the result and assign it with the repeat count
            var result = cRepeat;
            // Add the scan code with a left shift operation
            result |= WindowHelper.MapVirtualKey(key, TranslationTypes.VirtualKeyToScanCode) << 16;
            // Does we need to set the extended flag ?
            if (fExtended)
                result |= 0x1000000;
            // Does we need to set the alt flag ?
            if (altDown)
                result |= 0x20000000;
            // Does we need to set the repeat flag ?
            if (fRepeat)
                result |= 0x40000000;
            // Does we need to set the keyUp flag ?
            if (keyUp)
                result |= 0x80000000;

            return new UIntPtr(result);
        }

        static UIntPtr MakeKeyParameter(Keys key, bool keyUp)
        {
            return MakeKeyParameter(key, keyUp, keyUp, 1, false, false);
        }
    }
}