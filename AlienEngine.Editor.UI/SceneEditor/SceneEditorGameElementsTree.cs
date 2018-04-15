using AlienEngine.Editor.UI.Core;
using System.Collections.Generic;

namespace AlienEngine.Editor.UI.SceneEditor
{
    public static class SceneEditorGameElementsTree
    {
        #region Icons

        private static Gdk.Pixbuf _cameraIcon;
        private static Gdk.Pixbuf _meshIcon;
        private static Gdk.Pixbuf _lightDirectional;

        #endregion

        #region Tree Implementation

        private static Dictionary<GameElement, Gtk.TreeIter> _gameElementsPaths;

        private static Gtk.TreeStore _treeStore;
        private static Gtk.TreeView _treeView;

        #endregion Tree Implementation

        public static void Initialize()
        {
            _meshIcon = new Gdk.Pixbuf(typeof(Program).Assembly, "Windows.Resources.Images.GameElementsIcons.mesh");
            _cameraIcon = new Gdk.Pixbuf(typeof(Program).Assembly, "Windows.Resources.Images.GameElementsIcons.camera");
            _lightDirectional = new Gdk.Pixbuf(typeof(Program).Assembly, "Windows.Resources.Images.GameElementsIcons.lightdirectional");

            _gameElementsPaths = new Dictionary<GameElement, Gtk.TreeIter>();
        }

        public static void SetTree(ref Gtk.TreeStore treeStore, ref Gtk.TreeView treeView)
        {
            _treeStore = treeStore;
            _treeView = treeView;
        }

        public static void SelectGameElement(GameElement gameElement)
        {
            if (_gameElementsPaths.ContainsKey(gameElement))
            {
                var path = _treeStore.GetPath(_gameElementsPaths[gameElement]);
                _treeView.ActivateRow(path, _treeView.Columns[0]);
            }
        }

        public static void Populate()
        {
            _treeStore.Clear();
            _gameElementsPaths.Clear();

            var elements = GameElement.GameElements;

            foreach (var element in elements)
            {
                if (!element.Value.IsSceneEditorGameElement())
                {
                    Gdk.Pixbuf icon;

                    if (element.Value.HasComponent<Camera>())
                    {
                        icon = _cameraIcon;
                    }
                    else if (element.Value.HasComponent(out Light light))
                    {
                        switch (light.Type)
                        {
                            case LightType.Spot:
                                icon = _lightDirectional;
                                break;
                            case LightType.Directional:
                                icon = _lightDirectional;
                                break;
                            case LightType.Point:
                                icon = _lightDirectional;
                                break;
                            default:
                                icon = _lightDirectional;
                                break;
                        }
                    }
                    else
                    {
                        icon = _meshIcon;
                    }

                    var iter = _treeStore.AppendValues(icon, element.Key);
                    _gameElementsPaths.Add(element.Value, iter);

                    if (element.Value.HasChilds)
                    {
                        _populateInternal(ref iter, element.Value);
                    }
                }
            }

            _treeView.Model = _treeStore;
        }
        
        private static void _populateInternal(ref Gtk.TreeIter treeIter, GameElement gameElement)
        {
            foreach (var element in gameElement.Childs)
            {
                if (!element.IsSceneEditorGameElement())
                {
                    Gdk.Pixbuf icon;

                    if (element.HasComponent<Camera>())
                        icon = _cameraIcon;
                    else
                        icon = _meshIcon;

                    var iter = _treeStore.AppendValues(treeIter, icon, element.Name);
                    _gameElementsPaths.Add(element, iter);

                    if (element.HasChilds)
                    {
                        _populateInternal(ref iter, element);
                    }
                }
            }
        }
    }
}
