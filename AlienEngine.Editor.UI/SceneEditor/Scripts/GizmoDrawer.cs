using AlienEngine.Core.Game;
using AlienEngine.Core.Graphics;
using AlienEngine.Core.Graphics.OpenGL;
using AlienEngine.Core.Rendering;
using AlienEngine.Editor.UI.SceneEditor.Components;
using AlienEngine.Editor.UI.SceneEditor.Inputs;
using AlienEngine.Editor.UI.SceneEditor.Shaders;
using AlienEngine.Shaders;

namespace AlienEngine.Editor.UI.SceneEditor.Scripts
{
    public class GizmoDrawer : RenderScript
    {
        private bool _started;

        private GameElement _aabbGameElement;

        #region Axis Mover

        private Material _axisXMoverMaterial;
        private Material _axisYMoverMaterial;
        private Material _axisZMoverMaterial;

        private MeshRenderer _axisXMoverMeshRenderer;
        private MeshRenderer _axisYMoverMeshRenderer;
        private MeshRenderer _axisZMoverMeshRenderer;

        private GameElement _sceneEditorObjectAxisMoverX;
        private GameElement _sceneEditorObjectAxisMoverY;
        private GameElement _sceneEditorObjectAxisMoverZ;
        private GameElement _sceneEditorObjectAxisMover;

        #endregion

        public override void Load()
        {
            _aabbGameElement = new GameElement("aabb__sceneEditorElement");
            _aabbGameElement.AttachComponent(new Material()
            {
                HasColorAmbient = true,
                ColorAmbient = Color4.White,
                HasColorDiffuse = true,
                ColorDiffuse = Color4.White,
                ShaderProgram = new DiffuseShaderProgram()
            });
            _aabbGameElement.AttachComponent(new MeshRenderer());

            _initMoverAxis();
        }

        public override void BeforeRender()
        {
            if (!_started)
            {
                _aabbGameElement.Start();

                _started = true;
            }

            _axisXMoverMeshRenderer.Visible = false;
            _axisYMoverMeshRenderer.Visible = false;
            _axisZMoverMeshRenderer.Visible = false;
        }

        public override void AfterRender()
        {
            var selectedElement = SceneEditorScene.SelectedElement;
            if (selectedElement != null)
            {
                RendererManager.BackupState(RendererBackupMode.DepthTest);
                RendererManager.DepthTest(true, DepthFunction.Always);

                _applyMoverAxisTransform(selectedElement.WorldTransform);
                
                _renderMoverAxis();

                RendererManager.RestoreState(RendererBackupMode.DepthTest);

                if (((SceneEditorScene)ParentScene).BoundingBoxes.ContainsKey(selectedElement))
                    _renderSelectedElementBoundingBox();
            }
        }

        public override void Unload()
        {
            _aabbGameElement.Stop();
            _sceneEditorObjectAxisMover.Stop();
        }

        private void _initMoverAxis()
        {
            _axisXMoverMaterial = new Material()
            {
                HasColorAmbient = true,
                ColorAmbient = Color4.Red,
                HasColorDiffuse = true,
                ColorDiffuse = Color4.Red,
                ShaderProgram = new ObjectAxisShader()
            };

            _axisYMoverMaterial = new Material()
            {
                HasColorAmbient = true,
                ColorAmbient = Color4.Green,
                HasColorDiffuse = true,
                ColorDiffuse = Color4.Green,
                ShaderProgram = new ObjectAxisShader()
            };

            _axisZMoverMaterial = new Material()
            {
                HasColorAmbient = true,
                ColorAmbient = Color4.Blue,
                HasColorDiffuse = true,
                ColorDiffuse = Color4.Blue,
                ShaderProgram = new ObjectAxisShader()
            };

            var arrow = MeshImporter.ImportMesh("Assets/alienEngineSceneEditorObjectMover.obj");

            _axisXMoverMeshRenderer = new MeshRenderer()
            {
                Visible = false,
                MeshFilter = new MeshFilter(arrow, arrow.RootNode.Meshes[0])
            };

            _axisYMoverMeshRenderer = new MeshRenderer()
            {
                Visible = false,
                MeshFilter = new MeshFilter(arrow, arrow.RootNode.Meshes[0])
            };

            _axisZMoverMeshRenderer = new MeshRenderer()
            {
                Visible = false,
                MeshFilter = new MeshFilter(arrow, arrow.RootNode.Meshes[0])
            };

            _sceneEditorObjectAxisMoverX = new GameElement("axisMoverX__sceneEditorElement");
            _sceneEditorObjectAxisMoverY = new GameElement("axisMoverY__sceneEditorElement");
            _sceneEditorObjectAxisMoverZ = new GameElement("axisMoverZ__sceneEditorElement");

            PickableObject xPicker = new PickableObject();
            xPicker.Hover += _xPickerOnHover;
            xPicker.LostHover += _xPickerOnLostHover;
            xPicker.Picking += _xPickerOnPicking;
            xPicker.Unpick += _xPickerOnUnpick;

            PickableObject yPicker = new PickableObject();
            yPicker.Hover += _yPickerOnHover;
            yPicker.LostHover += _yPickerOnLostHover;
            yPicker.Picking += _yPickerOnPicking;
            yPicker.Unpick += _yPickerOnUnpick;

            PickableObject zPicker = new PickableObject();
            zPicker.Hover += _zPickerOnHover;
            zPicker.LostHover += _zPickerOnLostHover;
            zPicker.Picking += _zPickerOnPicking;
            zPicker.Unpick += _zPickerOnUnpick;

            _sceneEditorObjectAxisMoverX.AttachComponent(xPicker);
            _sceneEditorObjectAxisMoverX.AttachComponent(_axisXMoverMaterial);
            _sceneEditorObjectAxisMoverX.AttachComponent(_axisXMoverMeshRenderer);

            _sceneEditorObjectAxisMoverY.AttachComponent(yPicker);
            _sceneEditorObjectAxisMoverY.AttachComponent(_axisYMoverMaterial);
            _sceneEditorObjectAxisMoverY.AttachComponent(_axisYMoverMeshRenderer);

            _sceneEditorObjectAxisMoverZ.AttachComponent(zPicker);
            _sceneEditorObjectAxisMoverZ.AttachComponent(_axisZMoverMaterial);
            _sceneEditorObjectAxisMoverZ.AttachComponent(_axisZMoverMeshRenderer);
            
            _sceneEditorObjectAxisMoverX.LocalTransform.SetRotationZ(-90.0f);
            _sceneEditorObjectAxisMoverZ.LocalTransform.SetRotationX(+90.0f);

            _sceneEditorObjectAxisMover = new GameElement("axisMover__sceneEditorElement");
            _sceneEditorObjectAxisMover.AddChild(_sceneEditorObjectAxisMoverX);
            _sceneEditorObjectAxisMover.AddChild(_sceneEditorObjectAxisMoverY);
            _sceneEditorObjectAxisMover.AddChild(_sceneEditorObjectAxisMoverZ);

            ParentScene.AddGameElement(_sceneEditorObjectAxisMover);
        }

        private void _xPickerOnLostHover(PickableObject pickable)
        {
            _axisXMoverMaterial.ColorDiffuse = Color4.Red;
            PickableObject.LockOn = null;
        }

        private void _xPickerOnUnpick(PickableObject pickable)
        {
            _axisXMoverMaterial.ColorDiffuse = Color4.Red;
            PickableObject.LockOn = null;
        }

        private void _xPickerOnHover(PickableObject pickable)
        {
            _axisXMoverMaterial.ColorDiffuse = Color4.Yellow;
            PickableObject.LockOn = pickable;
        }

        private void _xPickerOnPicking(PickableObject pickable)
        {
            if (_axisXMoverMeshRenderer.Visible && Mouse.Holding(MouseButton.Left))
            {
                _axisXMoverMaterial.ColorDiffuse = Color4.Yellow;
                PickableObject.LockOn = pickable;

                var editorScene = ((SceneEditorScene) ParentScene);
                editorScene.SetCameraPanning(false);
                editorScene.SetCameraZomming(false);

                var camera = editorScene.PrimaryCamera.GetComponent<Camera>();
                
                var speed = (float) (Mouse.Position.X - Mouse.LastPosition.X) * (float) Time.DeltaTime * 5.0f;
                
                SceneEditorScene.SelectedElement.LocalTransform.Move(SceneEditorScene.SelectedElement.LocalTransform.RightVector, camera.Forward.Z <= 0 ? speed : -speed);
            }
        }

        private void _yPickerOnLostHover(PickableObject pickable)
        {
            _axisYMoverMaterial.ColorDiffuse = Color4.Green;
            PickableObject.LockOn = null;
        }

        private void _yPickerOnUnpick(PickableObject pickable)
        {
            _axisYMoverMaterial.ColorDiffuse = Color4.Green;
            PickableObject.LockOn = null;
        }

        private void _yPickerOnHover(PickableObject pickable)
        {
            _axisYMoverMaterial.ColorDiffuse = Color4.Yellow;
            PickableObject.LockOn = pickable;
        }

        private void _yPickerOnPicking(PickableObject pickable)
        {
            if (_axisYMoverMeshRenderer.Visible && Mouse.Holding(MouseButton.Left))
            {
                _axisYMoverMaterial.ColorDiffuse = Color4.Yellow;
                PickableObject.LockOn = pickable;

                var editorScene = ((SceneEditorScene) ParentScene);
                editorScene.SetCameraPanning(false);
                editorScene.SetCameraZomming(false);

                SceneEditorScene.SelectedElement.LocalTransform.Move(SceneEditorScene.SelectedElement.LocalTransform.UpVector, (float) (Mouse.LastPosition.Y - Mouse.Position.Y) * (float) Time.DeltaTime * 5.0f);
            }
        }

        private void _zPickerOnLostHover(PickableObject pickable)
        {
            _axisZMoverMaterial.ColorDiffuse = Color4.Blue;
            PickableObject.LockOn = null;
        }

        private void _zPickerOnUnpick(PickableObject pickable)
        {
            _axisZMoverMaterial.ColorDiffuse = Color4.Blue;
            PickableObject.LockOn = null;
        }

        private void _zPickerOnHover(PickableObject pickable)
        {
            _axisZMoverMaterial.ColorDiffuse = Color4.Yellow;
            PickableObject.LockOn = pickable;
        }

        private void _zPickerOnPicking(PickableObject pickable)
        {
            if (_axisZMoverMeshRenderer.Visible && Mouse.Holding(MouseButton.Left))
            {
                _axisZMoverMaterial.ColorDiffuse = Color4.Yellow;
                PickableObject.LockOn = pickable;

                var editorScene = ((SceneEditorScene) ParentScene);
                editorScene.SetCameraPanning(false);
                editorScene.SetCameraZomming(false);

                var camera = editorScene.PrimaryCamera.GetComponent<Camera>();
                
                var speed = (float) (Mouse.Position.X - Mouse.LastPosition.X) * (float) Time.DeltaTime * 5.0f;
                
                SceneEditorScene.SelectedElement.LocalTransform.Move(SceneEditorScene.SelectedElement.LocalTransform.ForwardVector, camera.Forward.X <= 0 ? -speed : speed);
            }
        }

        private void _applyMoverAxisTransform(Transform transform)
        {
            _sceneEditorObjectAxisMover.LocalTransform.Translation = transform.Translation;
            _sceneEditorObjectAxisMoverX.LocalTransform.SetRotationY(-transform.Rotation.X);
            _sceneEditorObjectAxisMoverX.LocalTransform.SetRotationZ(-90.0f - transform.Rotation.Z);
            _sceneEditorObjectAxisMoverY.LocalTransform.SetRotationX(-transform.Rotation.X);
            _sceneEditorObjectAxisMoverY.LocalTransform.SetRotationZ(-transform.Rotation.Z);
            _sceneEditorObjectAxisMoverZ.LocalTransform.SetRotationX(90.0f - transform.Rotation.X);
            _sceneEditorObjectAxisMoverZ.LocalTransform.SetRotationY(-transform.Rotation.Y);
            _sceneEditorObjectAxisMover.LocalTransform.Scale = new Vector3f((ParentScene.PrimaryCamera.WorldTransform.Translation - transform.Translation).Length / 5.0f);
        }

        private void _renderMoverAxis()
        {
            _axisXMoverMeshRenderer.Visible = true;
            _axisYMoverMeshRenderer.Visible = true;
            _axisZMoverMeshRenderer.Visible = true;

            _axisXMoverMeshRenderer.Render();
            _axisYMoverMeshRenderer.Render();
            _axisZMoverMeshRenderer.Render();
        }

        private void _renderSelectedElementBoundingBox()
        {
            var selectedElement = SceneEditorScene.SelectedElement;

            var transform = selectedElement.WorldTransform.Transformation;
            var boundingBox = ((SceneEditorScene) ParentScene).BoundingBoxes[selectedElement];
            var meshRenderer = _aabbGameElement.GetComponent<MeshRenderer>();

            meshRenderer.MeshFilter = MeshFactory.CreateCube(Vector3f.TransformPosition(boundingBox.Min, transform), Vector3f.TransformPosition(boundingBox.Max, transform));
            meshRenderer.MeshFilter.Mesh.VAO.DrawMode = BeginMode.Lines;

            meshRenderer.Render();
        }
    }
}