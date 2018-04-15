using System.Collections.Generic;

namespace AlienEngine.Editor.UI.SceneEditor.Inputs
{
    public static class Mouse
    {
        private static List<MouseButton> _holdedButtons;

        private static List<MouseButton> _currentButtons;

        private static Point2d _lastPosition;

        private static Point2d _position;

        private static Point2d _startScrollPosition;

        public static Point2d LastPosition => _lastPosition;

        public static Point2d Position
        {
            get => _position;
            set
            {
                _lastPosition = _position;
                _position = value;
            }
        }

        public static Point2d StartScrollPosition
        {
            get => _startScrollPosition;
            set => _startScrollPosition = value;
        }

        static Mouse()
        {
            _holdedButtons = new List<MouseButton>();
            _currentButtons = new List<MouseButton>();
        }

        public static bool Pressed(MouseButton key)
        {
            return Holding(key) && !_currentButtons.Contains(key);
        }

        public static bool Holding(MouseButton key)
        {
            return _holdedButtons.Contains(key);
        }

        public static bool Released(MouseButton key)
        {
            return !Holding(key) && _currentButtons.Contains(key);
        }

        public static void Down(MouseButton key)
        {
            _holdedButtons.Add(key);
        }

        public static void Release(MouseButton key)
        {
            _holdedButtons.Remove(key);
        }

        public static void Sync()
        {
            Position = Position;
            StartScrollPosition = Position;

            _currentButtons.Clear();
            _currentButtons.AddRange(_holdedButtons);
        }
    }
}