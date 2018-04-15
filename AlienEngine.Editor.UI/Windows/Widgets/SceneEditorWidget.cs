using System;
using System.Threading;
using AlienEngine.Core.Game;
using AlienEngine.Core.Graphics.OpenGL;
using AlienEngine.Core.Rendering;
using AlienEngine.Editor.UI.SceneEditor;
using AlienEngine.Editor.UI.SceneEditor.Inputs;
using AlienEngine.Editor.UI.Windows.Managers;

namespace AlienEngine.Editor.UI.Windows.Widgets
{
    public class SceneEditorWidget : Gtk.GLArea
    {
        /// <summary>
        /// The game which run in the <see cref="SceneEditorWidget"/>.
        /// </summary>
        private SceneEditorGame _editorGame;

        /// <summary>
        /// The <see cref="Scene"/> used by the <see cref="SceneEditorGame"/>.
        /// </summary>
        private SceneEditorScene _editorScene;

        /// <summary>
        /// The number of frames passed in one second.
        /// </summary>
        private int _frames;

        /// <summary>
        /// The current passed time between each FPS count.
        /// </summary>
        private double _frameCounter;

        /// <summary>
        /// The frame start time.
        /// </summary>
        private double _lastTime;

        /// <summary>
        /// The remaining time before start a new frame.
        /// </summary>
        private double _unprocessedTime;

        /// <summary>
        /// The time elapsed between each frames.
        /// </summary>
        public readonly double FrameTime;

        /// <summary>
        /// The number of frames per second.
        /// </summary>
        public int FPS => _frames;

        #region Events

        /// <summary>
        /// Event handler for each FPS count change.
        /// </summary>
        public event EventHandler FpsChange;

        #endregion

        public SceneEditorWidget()
        {
            CanFocus = true;
            Hexpand = true;
            Vexpand = true;

            FocusOnClick = true;

            HasAlpha = false;
            HasDepthBuffer = true;
            HasStencilBuffer = true;

            Events |= Gdk.EventMask.AllEventsMask;

            SetRequiredVersion(3, 3);

            FrameTime = Time.SECOND / GameSettings.GameFps;

            SceneEditorGameElementsTree.Initialize();
        }

        protected override Gdk.GLContext OnCreateContext()
        {
            // Create a new GLContext for the window
            Gdk.GLContext context = Window.CreateGlContext();

            // Configure the context
            context.DebugEnabled = true;
            context.ForwardCompatible = false;

            // We are done
            return context;
        }

        protected override void OnRealized()
        {
            // Base implementation
            base.OnRealized();

            // Make the context current
            MakeCurrent();

            // Load OpenGL functions
            GL.LoadOpenGL();

            // Set the size of the viewport
            ResizeViewport();

            // Create the scene
            _editorScene = new SceneEditorScene();

            // Initialize events
            _editorScene.GameElementAdded += _editorSceneGameElementAdded;
            _editorScene.GameElementRemoved += _editorSceneGameElementRemoved;

            // Create the game and add the scene
            _editorGame = new SceneEditorGame(_editorScene);

            // Enable depth testing
            RendererManager.DepthTest();

            // Enable blending
            RendererManager.Blending();

            // Enable face culling
            RendererManager.FaceCulling();

            // Enable Multi Samples
            RendererManager.MultiSample();

            // Enable depth mask
            RendererManager.DepthMask();

            // Start the editor game
            _editorGame.Start();

            // Initialize the renderer manager
            RendererManager.Initialize();

            // Focus on the editor
            GrabFocus();

            // Start updating the scene
            Gdk.Threads.AddIdle(120, OnUpdateFrameClock);

            // Refresh the game elements tree
            ((MainWindow) Toplevel).RefreshGameElementsTree();
        }

        protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
        {
            // Update the keyboard state
            Keyboard.State = evnt.State;

            // Check for pressed keys
            Keyboard.Down(evnt.Key);

            return true;
        }

        protected override bool OnKeyReleaseEvent(Gdk.EventKey evnt)
        {
            // Update the keyboard state
            Keyboard.State = evnt.State;

            // Check for released keys
            Keyboard.Release(evnt.Key);

            return true;
        }

        protected override bool OnButtonPressEvent(Gdk.EventButton evnt)
        {
            if (evnt.Type != Gdk.EventType.ButtonPress) return false;

            switch (evnt.Button)
            {
                case 1:
                    Mouse.Down(MouseButton.Left);
                    break;

                case 2:
                    Mouse.Down(MouseButton.Middle);
                    _editorScene.SetCameraPanning();
                    break;

                case 3:
                    Mouse.Down(MouseButton.Right);
                    if (evnt.State.HasFlag(Gdk.ModifierType.ControlMask))
                    {
                        Mouse.StartScrollPosition = new Point2d(evnt.X, evnt.Y);
                        _editorScene.SetCameraZomming();
                    }
                    else if (evnt.State.HasFlag(Gdk.ModifierType.ShiftMask))
                    {
                        _editorScene.SetCameraPanning();
                    }
                    else
                    {
                        _editorScene.SetCameraRotating();
                    }

                    break;
            }

            return true;
        }

        protected override bool OnButtonReleaseEvent(Gdk.EventButton evnt)
        {
            if (evnt.Type != Gdk.EventType.ButtonRelease) return false;

            switch (evnt.Button)
            {
                case 1:
                    Mouse.Release(MouseButton.Left);
                    break;

                case 2:
                    Mouse.Release(MouseButton.Middle);
                    _editorScene.SetCameraPanning(false);
                    _editorScene.SetCameraZomming(false);
                    break;

                case 3:
                    Mouse.Release(MouseButton.Right);
                    _editorScene.SetCameraRotating(false);
                    _editorScene.SetCameraZomming(false);
                    _editorScene.SetCameraPanning(false);
                    break;
            }

            return true;
        }

        protected override bool OnScrollEvent(Gdk.EventScroll evnt)
        {
            _editorScene.ZoomCamera(evnt.Direction == Gdk.ScrollDirection.Up);
            return true;
        }

        protected override bool OnMotionNotifyEvent(Gdk.EventMotion evnt)
        {
            // Save the mouse position when moving
            Mouse.Position = new Point2d(evnt.X, evnt.Y);

            return true;
        }

        protected override bool OnLeaveNotifyEvent(Gdk.EventCrossing evnt)
        {
            // Disable all camera actions
            //_editorScene.SetCameraPanning(false);
            //_editorScene.SetCameraRotating(false);
            //_editorScene.SetCameraZomming(false);

            return true;
        }

        protected override bool OnRender(Gdk.GLContext context)
        {
            // Clear the screen
            RendererManager.ClearScreen();

            // Render the game
            _editorGame.Render();

            // We are done
            return true;
        }

        protected override void OnResize(int width, int height)
        {
            // Base implementation
            base.OnResize(width, height);

            // Resize the viewport
            ResizeViewport();
        }

        protected override void OnUnrealized()
        {
            // Base implementation
            base.OnUnrealized();

            // Stop the game
            _editorGame.Stop();

            // Dispose the scene
            _editorScene.Dispose();
        }

        private bool OnUpdateFrameClock()
        {
            if (_editorGame.Running)
            {
                bool rend = false;

                // Save the first frame time
                double startTime = Time.GetTime();

                // The time between the fist and the last time
                double passedTime = startTime - _lastTime;

                // Restore the last frame time
                _lastTime = startTime;

                double deltaTime = passedTime / Time.SECOND;

                _unprocessedTime += passedTime;
                _frameCounter += passedTime;

                _editorGame.BeforeUpdate();

                while (_unprocessedTime >= FrameTime)
                {
                    if (_frameCounter >= Time.SECOND)
                    {
                        FpsChange?.Invoke(this, new EventArgs());
                        _frames = 0;
                        _frameCounter = 0;
                    }

                    // We can render the game
                    rend = true;

                    // Sets the delta time
                    Time.SetDelta(_editorGame.Paused ? 0 : deltaTime);

                    // Update all game elements and components
                    _editorGame.Update();

                    // Update mouse handler
                    Mouse.Sync();

                    // Update keyboard handler
                    Keyboard.Sync();

                    // We have processed some unprocessed time...
                    _unprocessedTime -= FrameTime;
                }

                _editorGame.AfterUpdate();

                if (rend)
                {
                    QueueRender();
                    _frames++;
                }
                else
                {
                    try
                    {
                        Thread.Sleep(1);
                    }
                    catch (ThreadInterruptedException exception)
                    {
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                    }
                }
            }

            if (_editorGame.NeedReload)
            {
                _editorGame.Reload();
                return true;
            }

            return _editorGame.Running;
        }

        private void _editorSceneGameElementAdded(Scene scene, GameElement element)
        {
            // Refresh the game elements tree
            ((MainWindow) Toplevel).RefreshGameElementsTree();
        }

        private void _editorSceneGameElementRemoved(Scene scene, GameElement element)
        {
            if (scene.Started)
            {
                // Refresh the game elements tree
                ((MainWindow) Toplevel).RefreshGameElementsTree();
            }
        }

        public void ResizeViewport()
        {
            ResizeViewport(AllocatedWidth, AllocatedHeight);
        }

        public void ResizeViewport(int width, int heigth)
        {
            Rectangle viewport = new Rectangle(0, 0, width, heigth);

            // TODO: Use EditorSettings instead
            if (GameSettings.GameWindowHasAspectRatio)
                RendererManager.SetViewportWithAspectRatio(viewport.Size);
            else
                RendererManager.SetViewport(viewport);
        }

        public void SelectGameElement(string name)
        {
            if (SceneEditorScene.SelectedElement != null)
            {
                SceneEditorScene.SelectedElement.LocalTransform.OnAllChange -= _selectedElementLocalTransformOnOnAllChange;
            }
            GameElement element = _editorScene.SelectGameElement(name);
            if (element != null)
            {
                ((MainWindow) Toplevel).PopulateTransformData(element.LocalTransform);
                element.LocalTransform.OnAllChange += _selectedElementLocalTransformOnOnAllChange;
            }
        }

        private void _selectedElementLocalTransformOnOnAllChange(Transform old, Transform @new)
        {
            ((MainWindow) Toplevel).PopulateTransformData(@new);
        }
    }
}