using System;
using System.Runtime.InteropServices;
using AlienEngine.Core.Graphics;
using AlienEngine.Core.Graphics.OpenGL;
using AlienEngine.Core.Graphics.OpenGL.Windows;
using AlienEngine.Editor.UI.Core;

namespace AlienEngine.Editor.UI.Windows.Managers
{
    public class DummyGLWindow : Gtk.Window
    {
        public DummyGLWindow() : this(new Gtk.Builder("Windows.Designers.DummyGLWindow"))
        {
        }

        private DummyGLWindow(Gtk.Builder builder) : base(builder.GetObject("DummyGLWindow").Handle)
        {
            Realized += OnRealized;

            ShowAll();
        }

        private void OnRealized(object sender, EventArgs eventArgs)
        {
            WGL.GetCurrentDC();

            IntPtr wh = Utils.GetGDKWindowHandle(Window.Handle);

            if (wh == IntPtr.Zero)
                throw new NotSupportedException("Failed to get the native window handle.");

            IntPtr dc = GraphicsManager.GetDeviceContext(wh);

            if (dc == IntPtr.Zero)
                throw new NotSupportedException("Failed to get the device context. OpenGL not supported");

            try
            {
                WGL.PixelFormatDescriptor pfd = WGL.PixelFormatDescriptor.Default;

                int pf = GraphicsManager.ChoosePixelFormat(dc, ref pfd);

                if (pf == 0)
                    throw new NotSupportedException("OpenGL not supported. Can't find a pixel format.");

                if (GraphicsManager.DescribePixelFormat(dc, pf, (uint) pfd.nSize, out pfd) == 0) throw new NotSupportedException("OpenGL not supported. Couldn't retrieve informations about a pixel format.");
                if (pfd.dwFlags.HasFlag(WGL.PixelFormatDescriptorFlags.GenericFormat))
                {
                    WGL.PixelFormatDescriptor checkpfd;
                    // First pixel format should be a ICD. Generic implentation comes later.
                    if (GraphicsManager.DescribePixelFormat(dc, 1, (uint) pfd.nSize, out checkpfd) == 0) throw new NotSupportedException("OpenGL not supported. Couldn't retrieve informations about a pixel format.");
                    if (checkpfd.dwFlags.HasFlag(WGL.PixelFormatDescriptorFlags.GenericFormat)) throw new NotSupportedException("OpenGL not supported. Not ICD (hardware renderer) found.");

                    pf = 1;
                    pfd = checkpfd;
                }

                if (!GraphicsManager.SetPixelFormat(dc, pf, ref pfd)) throw new NotSupportedException("OpenGL not supported. Couldn't initialize pixel format.");

                IntPtr rc = GraphicsManager.CreateContext(dc);

                if (rc == IntPtr.Zero)
                    throw new NotSupportedException($"Failed to create a render context. OpenGL not supported. {Marshal.GetLastWin32Error()}");

                try
                {
                    if (!GraphicsManager.MakeCurrent(dc, rc))
                        throw new NotSupportedException("Failed to make the render context current. OpenGL not supported");

                    GL.LoadOpenGL();

                    if (!GraphicsManager.ClearCurrentContext())
                        throw new NotSupportedException("Can't clear the current context. OpenGL not supported");
                }
                finally
                {
                    if (!GraphicsManager.DeleteContext(rc))
                        throw new NotSupportedException("Failed to delete the render context. OpenGL not supported.");
                }
            }
            finally
            {
                if (!GraphicsManager.ReleaseDeviceContext(wh, dc))
                    throw new NotSupportedException("Failed to release the device context. OpenGL not supported.");

                Close();
            }
        }
    }
}