using System;
using GtkUI = Gtk.Builder.ObjectAttribute;

namespace AlienEngine.Editor.UI.Windows.Managers
{
    internal class SplashScreen : Gtk.Window
    {
        [GtkUI] private Gtk.Image _alienEngineSplash = null;

        public SplashScreen() : this(new Gtk.Builder("Windows.Designers.SplashScreen")) { }

        private SplashScreen(Gtk.Builder builder) : base(builder.GetObject("SplashScreen").Handle)
        {
            builder.Autoconnect(this);

            DeleteEvent += Window_DeleteEvent;

            _alienEngineSplash.Pixbuf = new Gdk.Pixbuf(typeof(Program).Assembly, "Windows.Resources.Images.AlienEngineSplash");

            ShowAll();
        }

        private void Window_DeleteEvent(object sender, Gtk.DeleteEventArgs a)
        {
            Gtk.Application.Quit();
        }
    }
}
