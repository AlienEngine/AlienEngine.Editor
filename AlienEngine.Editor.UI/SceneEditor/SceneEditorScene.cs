using System.Collections.Generic;
using AlienEngine.Core.Game;
using AlienEngine.Core.Graphics;
using AlienEngine.Core.Rendering;
using AlienEngine.Core.Resources;
using AlienEngine.Editor.UI.SceneEditor.Components;
using AlienEngine.Editor.UI.SceneEditor.Scripts;
using AlienEngine.Imaging;
using AlienEngine.Shaders;

namespace AlienEngine.Editor.UI.SceneEditor
{
    internal class SceneEditorScene : Scene
    {
        #region Selected object

        private static GameElement _selectedElement;

        public static GameElement SelectedElement
        {
            get
            {
                return _selectedElement;
            }

            set
            {
                _selectedElement = value;
                SceneEditorGameElementsTree.SelectGameElement(value);
            }
        }

        #endregion

        #region Scene Camera

        private GameElement _sceneEditorCamera;
        private SceneEditorCameraControl _cameraControl;

        #endregion

        #region Bounding Boxes

        private Dictionary<GameElement, BoundingBox> _boundingBoxes;

        public Dictionary<GameElement, BoundingBox> BoundingBoxes => _boundingBoxes;

        #endregion

        public SceneEditorScene() : base("SceneEditorScene")
        {
            _boundingBoxes = new Dictionary<GameElement, BoundingBox>();
        }

        public override void Load()
        {
            _sceneEditorCamera = new GameElement("camera__sceneEditorElement");
            _cameraControl = new SceneEditorCameraControl();

            var camera = new GameElement("Main Camera");

            camera.AttachComponent(new Camera()
            {
                ClearColor = Color4.White,
                ClearScreenType = ClearScreenTypes.Cubemap,
                Cubemap = ResourcesManager.LoadResource<Cubemap>(ResourceType.CubeMap, "Assets/Skybox/skyX/posx.png Assets/Skybox/skyX/negx.png Assets/Skybox/skyX/posy.png Assets/Skybox/skyX/negy.png Assets/Skybox/skyX/posz.png Assets/Skybox/skyX/negz.png"),
                Far = 3000,
                FieldOfView = 45,
                Forward = VectorHelper.Forward,
                IsPrimary = true,
                Near = 0.25f,
                ProjectionType = ProjectionTypes.Perspective,
                Viewport = RendererManager.Viewport
            });

            _sceneEditorCamera.AttachComponent(camera.GetComponent<Camera>());

            _sceneEditorCamera.AttachComponent(_cameraControl);

            var box = new GameElement("Box");

            box.AttachComponents(
                new Material()
                {
                    ColorAmbient = Color4.Crimson,
                    ColorDiffuse = Color4.Crimson,
                    ColorSpecular = Color4.White,
                    HasColorAmbient = true,
                    HasColorDiffuse = true,
                    HasColorSpecular = true,
                    ShaderProgram = new DiffuseShaderProgram()
                },
                new MeshRenderer()
                {
                    MeshFilter = MeshFactory.CreateCube(-Vector3f.One * 5, Vector3f.One * 5),
                    Visible = true
                }
            );

            box.LocalTransform.SetTranslation(0, 0, 50);

            GameElement light = new GameElement("Sun");

            light.AttachComponents(
                new Light()
                {
                    AmbientColor = new Color4(255, 244, 214, 255),
                    DiffuseColor = new Color4(255, 244, 214, 255),
                    // DiffuseColor = new Color4(202, 198, 162, 255),
                    SpecularColor = Color4.White,
                    Intensity = 1f,
                    Type = LightType.Directional
                }
            );

            GameElement plane = new GameElement("Plane");

            plane.AttachComponents(
                new MeshRenderer()
                {
                    MeshFilter = MeshFactory.CreatePlane(Vector2f.One * -1000, Sizef.One * 2000),
                    Visible = true
                },
                new Material()
                {
                    TextureTilling = 50,
                    ColorAmbient = Color4.White,
                    HasColorAmbient = true,
                    ColorDiffuse = Color4.White,
                    HasColorDiffuse = true,
                    ShaderProgram = new DiffuseShaderProgram()
                }
            );

            plane.LocalTransform.SetTranslation(VectorHelper.Down * 20);

            AddGameElement(light);
            AddGameElement(plane);
            AddGameElement(box);
            AddGameElement(_sceneEditorCamera);

            AddRenderScript(new GizmoDrawer());

            base.Load();
        }

        public void SetCameraRotating(bool rotating = true)
        {
            _cameraControl.IsRotating = rotating;
        }

        public void SetCameraPanning(bool panning = true)
        {
            _cameraControl.IsPanning = panning;
        }

        public void SetCameraZomming(bool zomming = true)
        {
            _cameraControl.IsZomming = zomming;
        }

        public void ZoomCamera(bool forward = true)
        {
            _cameraControl.Zoom(forward);
        }

        public GameElement SelectGameElement(string name)
        {
            if (GameElement.Is(name))
            {
                _selectedElement = GameElement.Get(name);
                return _selectedElement;
            }

            return null;
        }

        protected override void OnAddGameElement(GameElement gameElement)
        {
            if (!gameElement.Name.EndsWith("__sceneEditorElement") && gameElement.HasComponent(out MeshRenderer renderer))
            {
                var points = new Vector3f[renderer.MeshFilter.Entry.NumVertices];

                for (int i = 0; i < points.Length; i++)
                    points[i] = renderer.MeshFilter.Mesh.MeshData.Positions[renderer.MeshFilter.Entry.BaseVertex + i];

                var aabb = BoundingBox.CreateFromPoints(points);
                _boundingBoxes.Add(gameElement, new BoundingBox(aabb.Min * 1.05f, aabb.Max * 1.05f));

                var pickable = new PickableObject();
                pickable.Picking += (p) =>
                {
                    SelectedElement = gameElement;
                };

                gameElement.AttachComponent(pickable);
            }

            base.OnAddGameElement(gameElement);
        }
    }
}