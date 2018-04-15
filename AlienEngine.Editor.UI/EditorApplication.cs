using System;
using AlienEngine.Core;
using GLib;

namespace AlienEngine.Editor.UI
{
    public static class EditorApplication
    {
        #region Unique Instance Pattern

        /// <summary>
        /// The application instance.
        /// </summary>
        private static Gtk.Application _instance;

        /// <summary>
        /// The application instance.
        /// </summary>
        public static Gtk.Application Instance => _instance ?? (_instance = _createApplication());

        #endregion

        #region Private Members

        /// <summary>
        /// Creates a new application instance.
        /// </summary>
        /// <returns>The application instance.</returns>
        private static Gtk.Application _createApplication()
        {
            var app = new Gtk.Application("cm.aliengames.alienengine", GLib.ApplicationFlags.None);
            
            app.Startup += _appOnStartup;
            app.Shutdown += _appOnShutdown;

            return app;
        }

        private static void _appOnStartup(object sender, EventArgs eventArgs)
        {
            Engine.Start();
        }

        /// <summary>
        /// Executes events when the application shutdown.
        /// </summary>
        /// <param name="sender">The object who send the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private static void _appOnShutdown(object sender, EventArgs eventArgs)
        {
            Engine.Stop();
        }

        #endregion
    }
}