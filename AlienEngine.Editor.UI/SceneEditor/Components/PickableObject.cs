using System;
using System.Collections.Generic;
using AlienEngine.Core.Graphics;
using AlienEngine.Editor.UI.SceneEditor.Inputs;

namespace AlienEngine.Editor.UI.SceneEditor.Components
{
    public class PickableObject : Component
    {
        public static PickableObject LockOn;
        
        private static Dictionary<PickableObject, float> _pickList;
        
        private bool _isHover;
        private bool _isPicked;

        private Camera _camera;
        private MeshRenderer _renderer;

        private BoundingBox _aabb;

        public event Action<PickableObject> Unpick; 

        public event Action<PickableObject> Picking;

        public event Action<PickableObject> Hover;

        public event Action<PickableObject> LostHover;

        public PickableObject()
        {
            OnAttach += OnOnAttach;
        }

        static PickableObject()
        {
            _pickList = new Dictionary<PickableObject, float>();
            LockOn = null;
        }
        
        private void OnOnAttach()
        {
        }

        public override void Start()
        {
            _camera = gameElement.ParentScene.PrimaryCamera.GetComponent<Camera>();
            _renderer = GetComponent<MeshRenderer>();

            var points = new Vector3f[_renderer.MeshFilter.Entry.NumVertices];

            for (int i = 0; i < points.Length; i++)
                points[i] = _renderer.MeshFilter.Mesh.MeshData.Positions[_renderer.MeshFilter.Entry.BaseVertex + i];

            _aabb = BoundingBox.CreateFromPoints(points);

            base.Start();
        }

        public override void BeforeUpdate()
        {
            _pickList.Clear();
        }

        public override void Update()
        {
            Matrix4f t = gameElement.WorldTransform.Transformation;

            Ray mouseRay = _camera.Ray(new Point2f((float) Mouse.Position.X, (float) Mouse.Position.Y));
            bool hovered  = mouseRay.Intersects(new BoundingBox(Vector3f.TransformPosition(_aabb.Min, t), Vector3f.TransformPosition(_aabb.Max, t)), out float p);

            var dist = (gameElement.WorldTransform.Translation - mouseRay.Position).Length;
            var scale = mouseRay.Direction * dist;
            dist = scale.Length;
            
            if (_isHover && !hovered)
                _onLostHover();

            if (LockOn != null)
            {
                if (LockOn == this && hovered)
                {
                    _onHover(dist);
                }
            }
            else if (hovered)
            {
                _onHover(dist);
            }

            var picked = _isPicked;
            if (picked && Mouse.Released(MouseButton.Left))
                picked = false;
            
            if (picked)
                _onPicking(dist);

            if (!picked && _isPicked)
                _onUnpick();
            
            _isHover = LockOn != null ? LockOn == this && hovered : hovered;
            _isPicked = picked;
        }

        private void _onPicking(float p)
        {
            bool min = true;

            foreach (var transform in _pickList)
            {
                if (transform.Value < p)
                {
                    min = false;
                    break;
                }
            }
            
            if (min)
                Picking?.Invoke(this);
        }

        private void _onHover(float p)
        {
            _pickList[this] = p;
            
            Hover?.Invoke(this);

            if (Mouse.Pressed(MouseButton.Left))
                _isPicked = true;
        }

        private void _onLostHover()
        {
            LostHover?.Invoke(this);
        }

        private void _onUnpick()
        {
            Unpick?.Invoke(this);
        }
    }
}