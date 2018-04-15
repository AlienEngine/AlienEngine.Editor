using System;
using AlienEngine.Editor.UI.SceneEditor.Inputs;

namespace AlienEngine.Editor.UI.SceneEditor.Components
{
    public class SceneEditorCameraControl : Component
    {
        public bool IsRotating { get; set; }
        public bool IsPanning { get; set; }
        public bool IsZomming { get; set; }

        public float MouseSensitivity = 0.5f;
        public float MoveSpeed = 5.0f;

        private Camera _camera;

        public override void Start()
        {
            _camera = GetComponent<Camera>();

            base.Start();
        }

        public override void Update()
        {
            double moveSpeed = MoveSpeed * Time.DeltaTime;
            double rotateSpeed = MouseSensitivity * Time.DeltaTime;

            double deltaX = (Mouse.LastPosition.X - Mouse.Position.X);
            double deltaY = (Mouse.LastPosition.Y - Mouse.Position.Y);

            if (IsRotating)
            {
                if (Keyboard.State.HasFlag(Gdk.ModifierType.ControlMask))
                {
                    gameElement.LocalTransform.RotateAround(Point3f.Zero, new Vector2f((float) deltaY, (float) deltaX), (float) Time.DeltaTime);
                }
                else
                {
                    _camera.Pitch(deltaX * rotateSpeed);
                    _camera.Roll(deltaY * rotateSpeed);
                }
            }

            if (IsPanning)
            {
                gameElement.LocalTransform.Move(_camera.Down, (float) (deltaY * moveSpeed));
                gameElement.LocalTransform.Move(_camera.Right, (float) (deltaX * moveSpeed));
            }

            if (IsZomming)
            {
                double delta = (Mouse.StartScrollPosition.Y - Mouse.Position.Y);

                if (Math.Abs(delta) > MathHelper.Epsilon)
                    Zoom(delta <= 0.0);
            }

            if (Keyboard.Pressed(Gdk.Key.R) || Keyboard.Pressed(Gdk.Key.r))
            {
                gameElement.LocalTransform.SetTranslation(Vector3f.Zero);
            }
        }

        public void Zoom(bool forward)
        {
            gameElement.LocalTransform.Move(forward ? _camera.Forward : _camera.Backward, 10.0f * MoveSpeed * (float) Time.DeltaTime);
        }
    }
}