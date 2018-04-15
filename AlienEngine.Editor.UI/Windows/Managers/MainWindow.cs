using AlienEngine.Core;
using AlienEngine.Core.Graphics;
using AlienEngine.Core.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.Threading;
using AlienEngine.Core.Game;
using AlienEngine.Core.Graphics.OpenGL.Windows;
using AlienEngine.Core.Rendering;
using AlienEngine.Editor.UI.SceneEditor;
using AlienEngine.Editor.UI.Windows.Events;
using AlienEngine.Editor.UI.Windows.Widgets;
using Gtk;
using GtkUI = Gtk.Builder.ObjectAttribute;

namespace AlienEngine.Editor.UI.Windows.Managers
{
    class MainWindow : Gtk.Window
    {
        [GtkUI] private Gtk.Image _alienEngineLogoSquare;
        [GtkUI] private Gtk.Image _commandButtonMoveImage;
        [GtkUI] private Gtk.Image _commandButtonRotateImage;
        [GtkUI] private Gtk.Image _commandButtonScaleImage;
        [GtkUI] private Gtk.Image _commandButtonGlobalViewImage;
        [GtkUI] private Gtk.Image _commandButtonLocalViewImage;

        [GtkUI] private Gtk.Image _commandButtonUndoImage;
        [GtkUI] private Gtk.Image _commandButtonRedoImage;

        [GtkUI] private Gtk.Image _optionButtonHelpImage;
        [GtkUI] private Gtk.Image _optionButtonSettingsImage;
        [GtkUI] private Gtk.Image _optionButtonProjectsImage;

        [GtkUI] private Gtk.Alignment _sceneGLAlignement;

        [GtkUI] private Gtk.Statusbar _appStatusbar;

        [GtkUI] private Gtk.TreeStore _sceneEditorGameElementsTreeStore;
        [GtkUI] private Gtk.TreeView _sceneEditorGameElementsTreeView;

        [GtkUI] private Gtk.Button _sceneEditorTransformComponentRevealerSwitch;
        [GtkUI] private Gtk.Image _sceneEditorTransformComponentRevealerSwitchImage;
        [GtkUI] private Gtk.Revealer _sceneEditorTransformComponentRevealer;
        [GtkUI] private Gtk.Image _sceneEditorTransformComponentTranslateImage;
        [GtkUI] private Gtk.Image _sceneEditorTransformComponentRotateImage;
        [GtkUI] private Gtk.Image _sceneEditorTransformComponentScaleImage;

        [GtkUI] private Gtk.SpinButton _sceneEditorTransformComponentTranstateSpinX;
        [GtkUI] private Gtk.SpinButton _sceneEditorTransformComponentTranstateSpinY;
        [GtkUI] private Gtk.SpinButton _sceneEditorTransformComponentTranstateSpinZ;
        [GtkUI] private Gtk.SpinButton _sceneEditorTransformComponentRotateSpinX;
        [GtkUI] private Gtk.SpinButton _sceneEditorTransformComponentRotateSpinY;
        [GtkUI] private Gtk.SpinButton _sceneEditorTransformComponentRotateSpinZ;
        [GtkUI] private Gtk.SpinButton _sceneEditorTransformComponentScaleSpinX;
        [GtkUI] private Gtk.SpinButton _sceneEditorTransformComponentScaleSpinY;
        [GtkUI] private Gtk.SpinButton _sceneEditorTransformComponentScaleSpinZ;

        [GtkUI] private SceneEditorWidget _sceneGLWidget;

        private uint _fpsStatusbarContextID;

        private bool _lockTransformComponentEvents;

        public MainWindow() : this(new Gtk.Builder("Windows.Designers.MainWindow"))
        {
        }

        private MainWindow(Gtk.Builder builder) : base(builder.GetObject("MainWindow").Handle)
        {
            builder.Autoconnect(this);
            DeleteEvent += Window_DeleteEvent;

            // Create a status bar context
            _fpsStatusbarContextID = _appStatusbar.GetContextId("FPS");

            // Create the scene editor
            _sceneGLWidget = new SceneEditorWidget();
            _sceneGLWidget.FpsChange += SceneGLWidgetOnFpsChange;

            // Add the GL widget to the UI
            _sceneGLAlignement.Add(_sceneGLWidget);

            // AlienEngine logo
            _alienEngineLogoSquare.Pixbuf = new Gdk.Pixbuf(typeof(Program).Assembly, "Windows.Resources.Images.AlienEngineLogo");
            _alienEngineLogoSquare.Pixbuf = _alienEngineLogoSquare.Pixbuf.ScaleSimple(36, 36, Gdk.InterpType.Bilinear);

            // Command buttons
            _commandButtonMoveImage.Pixbuf = new Gdk.Pixbuf(typeof(Program).Assembly, "Windows.Resources.Images.commandButtonMove");
            _commandButtonRotateImage.Pixbuf = new Gdk.Pixbuf(typeof(Program).Assembly, "Windows.Resources.Images.commandButtonRotate");
            _commandButtonScaleImage.Pixbuf = new Gdk.Pixbuf(typeof(Program).Assembly, "Windows.Resources.Images.commandButtonScale");
            _commandButtonGlobalViewImage.Pixbuf = new Gdk.Pixbuf(typeof(Program).Assembly, "Windows.Resources.Images.commandButtonGlobal");
            _commandButtonLocalViewImage.Pixbuf = new Gdk.Pixbuf(typeof(Program).Assembly, "Windows.Resources.Images.commandButtonLocal");

            // Toolbar buttons
            _commandButtonUndoImage.Pixbuf = new Gdk.Pixbuf(typeof(Program).Assembly, "Windows.Resources.Images.commandButtonUndo");
            _commandButtonRedoImage.Pixbuf = new Gdk.Pixbuf(typeof(Program).Assembly, "Windows.Resources.Images.commandButtonRedo");

            // Options buttons
            _optionButtonHelpImage.Pixbuf = new Gdk.Pixbuf(typeof(Program).Assembly, "Windows.Resources.Images.commandButtonHelp");
            _optionButtonSettingsImage.Pixbuf = new Gdk.Pixbuf(typeof(Program).Assembly, "Windows.Resources.Images.commandButtonSettings");
            _optionButtonProjectsImage.Pixbuf = new Gdk.Pixbuf(typeof(Program).Assembly, "Windows.Resources.Images.commandButtonProjects");

            _sceneEditorGameElementsTreeView.RowActivated += _sceneEditorGameElementsTreeView_RowActivated;

            _sceneEditorTransformComponentTranstateSpinX.SetRange(double.MinValue, double.MaxValue);
            _sceneEditorTransformComponentTranstateSpinY.SetRange(double.MinValue, double.MaxValue);
            _sceneEditorTransformComponentTranstateSpinZ.SetRange(double.MinValue, double.MaxValue);

            _sceneEditorTransformComponentRotateSpinX.SetRange(double.MinValue, double.MaxValue);
            _sceneEditorTransformComponentRotateSpinY.SetRange(double.MinValue, double.MaxValue);
            _sceneEditorTransformComponentRotateSpinZ.SetRange(double.MinValue, double.MaxValue);

            _sceneEditorTransformComponentScaleSpinX.SetRange(double.MinValue, double.MaxValue);
            _sceneEditorTransformComponentScaleSpinY.SetRange(double.MinValue, double.MaxValue);
            _sceneEditorTransformComponentScaleSpinZ.SetRange(double.MinValue, double.MaxValue);

            _sceneEditorTransformComponentRevealerSwitchImage.Pixbuf = new Gdk.Pixbuf(typeof(Program).Assembly, "Windows.Resources.Images.Components.Transform");
            _sceneEditorTransformComponentRevealerSwitch.Clicked += (sender, args) => _sceneEditorTransformComponentRevealer.RevealChild = !_sceneEditorTransformComponentRevealer.RevealChild;

            _sceneEditorTransformComponentTranslateImage.Pixbuf = new Gdk.Pixbuf(typeof(Program).Assembly, "Windows.Resources.Images.Icons.move");
            _sceneEditorTransformComponentRotateImage.Pixbuf = new Gdk.Pixbuf(typeof(Program).Assembly, "Windows.Resources.Images.Icons.rotate");
            _sceneEditorTransformComponentScaleImage.Pixbuf = new Gdk.Pixbuf(typeof(Program).Assembly, "Windows.Resources.Images.Icons.scale");

            _sceneEditorTransformComponentTranstateSpinX.ValueChanged += _sceneEditorTransformComponentChanged;
            _sceneEditorTransformComponentTranstateSpinY.ValueChanged += _sceneEditorTransformComponentChanged;
            _sceneEditorTransformComponentTranstateSpinZ.ValueChanged += _sceneEditorTransformComponentChanged;

            _sceneEditorTransformComponentRotateSpinX.ValueChanged += _sceneEditorTransformComponentChanged;
            _sceneEditorTransformComponentRotateSpinY.ValueChanged += _sceneEditorTransformComponentChanged;
            _sceneEditorTransformComponentRotateSpinZ.ValueChanged += _sceneEditorTransformComponentChanged;

            _sceneEditorTransformComponentScaleSpinX.ValueChanged += _sceneEditorTransformComponentChanged;
            _sceneEditorTransformComponentScaleSpinY.ValueChanged += _sceneEditorTransformComponentChanged;
            _sceneEditorTransformComponentScaleSpinZ.ValueChanged += _sceneEditorTransformComponentChanged;

            SceneEditorGameElementsTree.SetTree(ref _sceneEditorGameElementsTreeStore, ref _sceneEditorGameElementsTreeView);
        }

        private void _sceneEditorTransformComponentOutputHandler(object o, OutputArgs outputArgs)
        {
            if (SceneEditorScene.SelectedElement == null)
            {
                _sceneEditorTransformComponentTranstateSpinX.Text = "";
                _sceneEditorTransformComponentTranstateSpinY.Text = "";
                _sceneEditorTransformComponentTranstateSpinZ.Text = "";

                _sceneEditorTransformComponentRotateSpinX.Text = "";
                _sceneEditorTransformComponentRotateSpinY.Text = "";
                _sceneEditorTransformComponentRotateSpinZ.Text = "";

                _sceneEditorTransformComponentScaleSpinX.Text = "";
                _sceneEditorTransformComponentScaleSpinY.Text = "";
                _sceneEditorTransformComponentScaleSpinZ.Text = "";

                outputArgs.RetVal = true;
            }

            outputArgs.RetVal = false;
        }

        private void _sceneEditorGameElementsTreeView_RowActivated(object o, Gtk.RowActivatedArgs args)
        {
            if (_sceneEditorGameElementsTreeStore.GetIter(out Gtk.TreeIter it, args.Path))
            {
                string name = (string) _sceneEditorGameElementsTreeStore.GetValue(it, 1);

                _sceneGLWidget.SelectGameElement(name);
            }
        }

        private void _sceneEditorTransformComponentChanged(object o, EventArgs changeValueArgs)
        {
            if (SceneEditorScene.SelectedElement != null && !_lockTransformComponentEvents)
            {
                _lockTransformComponentEvents = true;

                SceneEditorScene.SelectedElement.LocalTransform.SetTranslation(
                    (float) _sceneEditorTransformComponentTranstateSpinX.Value,
                    (float) _sceneEditorTransformComponentTranstateSpinY.Value,
                    (float) _sceneEditorTransformComponentTranstateSpinZ.Value
                );

                SceneEditorScene.SelectedElement.LocalTransform.SetRotation(
                    (float) _sceneEditorTransformComponentRotateSpinX.Value,
                    (float) _sceneEditorTransformComponentRotateSpinY.Value,
                    (float) _sceneEditorTransformComponentRotateSpinZ.Value
                );

                SceneEditorScene.SelectedElement.LocalTransform.SetScale(
                    (float) _sceneEditorTransformComponentScaleSpinX.Value,
                    (float) _sceneEditorTransformComponentScaleSpinY.Value,
                    (float) _sceneEditorTransformComponentScaleSpinZ.Value
                );

                _lockTransformComponentEvents = false;
            }
        }

        public void PopulateTransformData(Transform transform)
        {
            if (!_lockTransformComponentEvents)
            {
                _lockTransformComponentEvents = true;

                _sceneEditorTransformComponentTranstateSpinX.Value = transform.Translation.X;
                _sceneEditorTransformComponentTranstateSpinY.Value = transform.Translation.Y;
                _sceneEditorTransformComponentTranstateSpinZ.Value = transform.Translation.Z;

                _sceneEditorTransformComponentRotateSpinX.Value = transform.Rotation.X;
                _sceneEditorTransformComponentRotateSpinY.Value = transform.Rotation.Y;
                _sceneEditorTransformComponentRotateSpinZ.Value = transform.Rotation.Z;

                _sceneEditorTransformComponentScaleSpinX.Value = transform.Scale.X;
                _sceneEditorTransformComponentScaleSpinY.Value = transform.Scale.Y;
                _sceneEditorTransformComponentScaleSpinZ.Value = transform.Scale.Z;

                _lockTransformComponentEvents = false;
            }
        }

        public void RefreshGameElementsTree()
        {
            SceneEditorGameElementsTree.Populate();
        }

        private void SceneGLWidgetOnFpsChange(object sender, EventArgs eventArgs)
        {
            _appStatusbar.RemoveAll(_fpsStatusbarContextID);
            _appStatusbar.Push(_fpsStatusbarContextID, $"{_sceneGLWidget.FPS} FPS");
        }

        private void Window_DeleteEvent(object sender, Gtk.DeleteEventArgs a)
        {
            // Release all GTK ressources
            Unrealize();

            // Release all AlienEngine resources
            Engine.Stop();

            // Quit the application
            Gtk.Application.Quit();

            // Every things is done
            a.RetVal = true;
        }
    }
}