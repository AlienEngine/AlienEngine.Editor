using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace AlienEngine.Editor.UI.Core
{
    public static class Utils
    {
        /// <summary>
        /// GDK method for getting the GTK window handle on Windows platform.
        /// </summary>
        /// <param name="window">The <see cref="Gdk.Window.Handle">GDK window handle</see>.</param>
        /// <returns>A pointer to the native window handle.</returns>
        [SuppressUnmanagedCodeSecurity, DllImport("libgdk-3-0.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "gdk_win32_window_get_handle")]
        public static extern IntPtr GetGDKWindowHandle(IntPtr window);

        public static byte[] ReadAllBytes(this BinaryReader reader)
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }
        }

        public static bool IsSceneEditorGameElement(this GameElement gameElement)
        {
            return gameElement.Name.EndsWith("__sceneEditorElement");
        }
    }
}