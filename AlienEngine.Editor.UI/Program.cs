using System;
using AlienEngine.Editor.UI.Windows.Managers;

namespace AlienEngine.Editor.UI
{
    public class Program
    {
        private static SplashScreen _splashScreen;
        
        [STAThread]
        public static void Main(string[] args)
        {
            Gtk.Application.Init();
            ApplyTheme();

            var app = EditorApplication.Instance;
            app.Register(GLib.Cancellable.Current);

            _splashScreen = new SplashScreen();

            while (Gtk.Application.EventsPending())
            {
                Gtk.Application.RunIteration();
            }

            var dgl = new DummyGLWindow();

            dgl.Destroyed += _onDummyContextCreated;

            Gtk.Application.Run();
        }

        private static void _onDummyContextCreated(object sender, EventArgs eventArgs)
        {
            var win = new MainWindow();
            EditorApplication.Instance.AddWindow(win);

            win.Shown += (o, args) => { _splashScreen?.Destroy(); };
            
            win.ShowAll();
        }

        public static void ApplyTheme()
        {
            // Based on this Link http://awesome.naquadah.org/wiki/Better_Font_Rendering

            // Get the Global Settings
            var setts = Gtk.Settings.Default;
            // This enables clear text on Win32, makes the text look a lot less crappy
            setts.XftRgba = "rgb";
            // This enlarges the size of the controls based on the dpi
            setts.XftDpi = 96;
            // By Default Anti-aliasing is enabled, if you want to disable it for any reason set this value to 0
            //setts.XftAntialias = 0
            // Enable text hinting
            setts.XftHinting = 1;
            //setts.XftHintstyle = "hintslight"
            setts.XftHintstyle = "hintfull";

            // Load the Theme
            Gtk.CssProvider css_provider = new Gtk.CssProvider();
            
            //css_provider.LoadFromPath("themes/DeLorean-Dark-3.14/gtk-3.0/gtk.css");
            css_provider.LoadFromPath("themes/AlienEngine/gtk.css");

            Gtk.StyleContext.AddProviderForScreen(Gdk.Screen.Default, css_provider, 800);
        }

    }
}
