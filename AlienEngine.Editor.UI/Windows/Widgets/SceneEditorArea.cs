using System;
using System.ComponentModel;
using System.Diagnostics;
using AlienEngine.Core.Graphics;
using AlienEngine.Core.Graphics.OpenGL;
using AlienEngine.Core.Graphics.OpenGL.Windows;
using AlienEngine.Core.Utils;
using AlienEngine.Editor.UI.Core;
using AlienEngine.Editor.UI.Windows.Events;
using Cairo;
using GLib;

namespace AlienEngine.Editor.UI.Windows.Widgets
{
    [ToolboxItem(true)]
    public class SceneEditorArea : Gtk.DrawingArea
    {
        #region Device Pixel Format

        /// <summary>
        /// Get or set the OpenGL minimum color buffer bits.
        /// </summary>
        [Property("color-bits")]
        public uint ColorBits
        {
            get { return (_colorBits); }
            set { _colorBits = value; }
        }

        /// <summary>
        /// The OpenGL color buffer bits.
        /// </summary>
        private uint _colorBits = 24;

        /// <summary>
        /// Get or set the OpenGL minimum depth buffer bits.
        /// </summary>
        [Property("depth-bits")]
        public uint DepthBits
        {
            get { return (_depthBits); }
            set { _depthBits = value; }
        }

        /// <summary>
        /// The OpenGL depth buffer bits.
        /// </summary>
        private uint _depthBits;

        /// <summary>
        /// Get or set the OpenGL minimum stencil buffer bits.
        /// </summary>
        [Property("stencil-bits")]
        public uint StencilBits
        {
            get { return (_stencilBits); }
            set { _stencilBits = value; }
        }

        /// <summary>
        /// The OpenGL stencil buffer bits.
        /// </summary>
        private uint _stencilBits;

        /// <summary>
        /// Get or set the OpenGL minimum multisample buffer "bits".
        /// </summary>
        [Property("multisample-bits")]
        public uint MultisampleBits
        {
            get { return (_multisampleBits); }
            set { _multisampleBits = value; }
        }

        /// <summary>
        /// The OpenGL multisample buffer bits.
        /// </summary>
        private uint _multisampleBits;

        /// <summary>
        /// Get or set the OpenGL swap buffers interval.
        /// </summary>
        [Property("swap-interval")]
        public int SwapInterval
        {
            get { return (_swapInterval); }
            set { _swapInterval = value; }
        }

        /// <summary>
        /// The OpenGL swap buffers interval.
        /// </summary>
        private int _swapInterval = 1;

        /// <summary>
        /// The <see cref="DevicePixelFormat"/> describing the minimum pixel format required by this control.
        /// </summary>
        private DevicePixelFormat ControlPixelFormat
        {
            get
            {
                DevicePixelFormat pixelFormat = new DevicePixelFormat();

                pixelFormat.RgbaUnsigned = true;
                pixelFormat.RenderWindow = true;

                pixelFormat.ColorBits = (int) ColorBits;
                pixelFormat.DepthBits = (int) DepthBits;
                pixelFormat.StencilBits = (int) StencilBits;
                pixelFormat.MultisampleBits = (int) MultisampleBits;
                pixelFormat.DoubleBuffer = true;

                return (pixelFormat);
            }
        }

        #endregion

        #region Device Context

        /// <summary>
        /// The graphics context used by this widget.
        /// </summary>
        private GraphicsContext _graphicsContext;

        /// <summary>
        /// The render context used by this widget.
        /// </summary>
        private ContextHandle _renderContext;

        /// <summary>
        /// The graphics context used by this widget.
        /// </summary>
        public GraphicsContext GraphicsContext => _graphicsContext;

        /// <summary>
        /// Creates a new graphics context.
        /// </summary>
        private void _createGraphicsContext(DevicePixelFormat pixelFormat)
        {
            _graphicsContext = GraphicsManager.CreateGraphicsContext(_getWindowHandle());

            #region Set Pixel Format

            DevicePixelFormatCollection pixelFormats = _graphicsContext.PixelsFormats;
            System.Collections.Generic.List<DevicePixelFormat> matchingPixelFormats = pixelFormats.Choose(pixelFormat);

            if ((matchingPixelFormats.Count == 0) && pixelFormat.MultisampleBits > 0)
            {
                // Try to select the maximum multisample configuration
                int multisampleBits = 0;

                pixelFormats.ForEach(delegate(DevicePixelFormat item) { multisampleBits = Math.Max(multisampleBits, item.MultisampleBits); });

                pixelFormat.MultisampleBits = multisampleBits;

                matchingPixelFormats = pixelFormats.Choose(pixelFormat);
            }

            if ((matchingPixelFormats.Count == 0) && pixelFormat.DoubleBuffer)
            {
                // Try single buffered pixel formats
                pixelFormat.DoubleBuffer = false;

                matchingPixelFormats = pixelFormats.Choose(pixelFormat);
                if (matchingPixelFormats.Count == 0)
                    throw new InvalidOperationException(String.Format("unable to find a suitable pixel format: {0}", pixelFormats.GuessChooseError(pixelFormat)));
            }
            else if (matchingPixelFormats.Count == 0)
                throw new InvalidOperationException(String.Format("unable to find a suitable pixel format: {0}", pixelFormats.GuessChooseError(pixelFormat)));

            _graphicsContext.SetPixelFormat(matchingPixelFormats[0]);

            #endregion

            // TODO: Platform specific extension checker
            if (WGL.IsExtensionSupported(_graphicsContext.DeviceHandle.Handle, WGL.EXT.SwapControl))
            {
                // TODO: Handle tear
                WGL.SwapIntervalEXT(SwapInterval);
            }
        }

        /// <summary>
        /// Creates a new render context.
        /// </summary>
        private void _createContext()
        {
            if (_renderContext != ContextHandle.Zero)
                return;

            if (GL.PlatformExtensions.IsSupported(_graphicsContext.DeviceHandle.Handle, GL.PlatformExtensions.CreateContextARB))
            {
                System.Collections.Generic.List<int> attributes = new System.Collections.Generic.List<int>();
                uint contextProfile = 0, contextFlags = 0;
                bool debuggerAttached = Debugger.IsAttached;

                #region WGL_ARB_create_context|GLX_ARB_create_context

                #endregion

                #region WGL_ARB_create_context_profile|GLX_ARB_create_context_profile

                if (GL.PlatformExtensions.IsSupported(_graphicsContext.DeviceHandle.Handle, GL.PlatformExtensions.CreateContextProfileARB))
                {
                }

                #endregion

                #region WGL_ARB_create_context_robustness|GLX_ARB_create_context_robustness

                if (GL.PlatformExtensions.IsSupported(_graphicsContext.DeviceHandle.Handle, GL.PlatformExtensions.CreateContextRobustnessARB))
                {
                }

                #endregion

                if (contextFlags != 0)
                    attributes.AddRange(new int[] {(int) WGL.ContextAttributeARB.ContextFlagsARB, unchecked((int) contextFlags)});

                if (contextProfile != 0)
                    attributes.AddRange(new int[] {(int) WGL.ContextAttributeARB.ContextProfileMaskARB, unchecked((int) contextProfile)});

                attributes.Add(0);

                if ((_renderContext = (ContextHandle) _graphicsContext.CreateContextAttrib(IntPtr.Zero, attributes.ToArray())) == ContextHandle.Zero)
                    throw new InvalidOperationException($"unable to create render context ({GL.GetError()})");
            }
            else
            {
                // TODO: CreateContextAttribARB
                _renderContext = (ContextHandle) _graphicsContext.CreateContext(IntPtr.Zero);
            }
        }

        /// <summary>
        /// Makes the render context of this widget current.
        /// </summary>
        /// <exception cref="InvalidOperationException">When it is unable to set the render context current.</exception>
        private void _makeContextCurrent()
        {
            if (!_graphicsContext.MakeCurrent(_renderContext.Handle))
            {
                throw new InvalidOperationException("Unable to make the scene context current");
            }
        }

        /// <summary>
        /// Deletes the render context of this widget.
        /// </summary>
        private void _deleteContext()
        {
            if (_renderContext != ContextHandle.Zero)
            {
                _graphicsContext.DeleteContext(_renderContext.Handle);
                _renderContext = ContextHandle.Zero;
            }
        }

        #endregion

        public SceneEditorArea()
        {
            _renderContext = ContextHandle.Zero;
            DoubleBuffered = false;
            Opacity = 1;
        }

        [Signal("context-created")]
        public event EventHandler<SceneEditorAreaEventArgs> ContextCreated
        {
            add { _contextCreatedEventHandler += value; }
            remove { _contextCreatedEventHandler -= value; }
        }

        [Signal("context-destroying")]
        public event EventHandler<SceneEditorAreaEventArgs> ContextDestroying;

        [Signal("render")]
        public event EventHandler<SceneEditorAreaEventArgs> Render;

        [Signal("context-update")]
        public event EventHandler<SceneEditorAreaEventArgs> ContextUpdate;

        private EventHandler<SceneEditorAreaEventArgs> _contextCreatedEventHandler;

        protected virtual void OnContextCreated()
        {
            if (_contextCreatedEventHandler != null)
            {
                foreach (EventHandler<SceneEditorAreaEventArgs> handler in _contextCreatedEventHandler.GetInvocationList())
                {
                    try
                    {
                        handler(this, new SceneEditorAreaEventArgs(_graphicsContext, _renderContext));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Raise the event <see cref="ContextDestroying"/>.
        /// </summary>
        protected virtual void OnContextDestroying()
        {
            ContextDestroying?.Invoke(this, new SceneEditorAreaEventArgs(_graphicsContext, _renderContext));
        }

        protected virtual void OnRender()
        {
            Render?.Invoke(this, new SceneEditorAreaEventArgs(_graphicsContext, _renderContext));
        }

        protected virtual void OnContextUpdate()
        {
            ContextUpdate?.Invoke(this, new SceneEditorAreaEventArgs(_graphicsContext, _renderContext));
        }

        protected override void OnRealized()
        {
            base.OnRealized();

            _createGraphicsContext(ControlPixelFormat);

            _createContext();

            _makeContextCurrent();

            OnContextCreated();
        }

        protected override void OnUnrealized()
        {
            if (_renderContext != ContextHandle.Zero)
            {
                _makeContextCurrent();

                OnContextDestroying();

                _deleteContext();
            }

            _graphicsContext = null;

            base.OnUnrealized();
        }

        protected override bool OnDrawn(Context cr)
        {
            try
            {
                _makeContextCurrent();

                OnRender();
                //GL.ClearColor(Color4.Red);

                OnContextUpdate();
                
                _graphicsContext.SwapBuffers();
//                int width, height;
//                Gdk.RGBA color;
//                Gtk.StyleContext context;
//
//                context = StyleContext;
//
//                width = AllocatedWidth;
//                height = AllocatedHeight;
//
//                context.RenderBackground(cr, 0, 0, width, height);
//
//                cr.Arc(width / 2.0, height / 2.0,
//                    Math.Min(width, height) / 2.0,
//                    0, 2 * Math.PI);
//
//                color = context.GetColor(context.State);
//
//                cr.SetSourceRGBA(color.Red, color.Green, color.Blue, color.Alpha);
//
//                cr.Fill();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to draw the frame: {e}");
            }

            return true;
        }

        #region Utils

        /// <summary>
        /// Gets the pointer to the display device.
        /// </summary>
        /// <returns></returns>
        private IntPtr _getDisplay()
        {
            return IntPtr.Zero;
        }

        /// <summary>
        /// Get platform independent GTL widget handle.
        /// </summary>
        /// <returns>
        /// It returns an <see cref="IntPtr"/> that is the handle of this GlWidget.
        /// </returns>
        private IntPtr _getWindowHandle()
        {
            return Utils.GetGDKWindowHandle(Window.Handle);
        }

        #endregion
    }
}