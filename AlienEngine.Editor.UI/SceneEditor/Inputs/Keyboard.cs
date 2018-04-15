using System.Collections.Generic;

namespace AlienEngine.Editor.UI.SceneEditor.Inputs
{
    public static class Keyboard
    {
        private static List<Gdk.Key> _pressedKeys;

        private static List<Gdk.Key> _holdedKeys;

        private static List<Gdk.Key> _releasedKeys;

        public static Gdk.ModifierType State;
        public static List<Gdk.Key> PressedKeys => _pressedKeys;

        static Keyboard()
        {
            _pressedKeys = new List<Gdk.Key>();
            _holdedKeys = new List<Gdk.Key>();
            _releasedKeys = new List<Gdk.Key>();
        }
        
        public static bool Pressed(Gdk.Key key)
        {
            return _pressedKeys.Contains(key);
        }

        public static bool Holding(Gdk.Key key)
        {
            return _holdedKeys.Contains(key);
        }

        public static bool Released(Gdk.Key key)
        {
            return _releasedKeys.Contains(key);
        }

        public static void Down(Gdk.Key key)
        {
            _releasedKeys.Remove(key);
            
            _pressedKeys.Add(key);
            _holdedKeys.Add(key);
        }

        public static void Release(Gdk.Key key)
        {
            _pressedKeys.Remove(key);
            _holdedKeys.Remove(key);
            
            _releasedKeys.Add(key);
        }

        public static void Sync()
        {
            _pressedKeys.Clear();
            _releasedKeys.Clear();
        }
    }
}