using SysCol = System.Collections.Generic;

namespace ChaosFramework.Input
{
    public enum KeyboardLayouts : byte
    {
        QWERTZ_GER
    }

    public static class KeyboardLayout
    {
        public const KeyboardLayouts DEFAULT_KEYBOARD_LAYOUT = KeyboardLayouts.QWERTZ_GER;

        static readonly SysCol.Dictionary<int, KeyboardLayouts> keyboardLayouts = new SysCol.Dictionary<int, KeyboardLayouts>();
        static readonly SysCol.Dictionary<int, string> languageNames = new SysCol.Dictionary<int, string>();

        static KeyboardLayout()
        {
            foreach (string line in Properties.Resources.KeyboardLayoutIDs.Split('\n'))
            {
                string[] split = line.Trim('\r').Split(';');

                int langInd;
                KeyboardLayouts layout;
                if (split.Length >= 2)
                    if (int.TryParse(split[1], out langInd))
                    {
                        languageNames[langInd] = split[0].Trim();
                        if (split.Length > 2 && System.Enum.TryParse(split[2], out layout))
                            keyboardLayouts[langInd] = layout;
                        else
                            keyboardLayouts[langInd] = DEFAULT_KEYBOARD_LAYOUT;
                    }
            }
        }

        public static KeyboardLayouts GetLayout(int layoutID)
        {
            KeyboardLayouts layout;
            if (keyboardLayouts.TryGetValue(layoutID, out layout))
                return layout;

            return DEFAULT_KEYBOARD_LAYOUT;
        }
    }
}
