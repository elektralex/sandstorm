using System;

namespace Sandstorm
{
#if WINDOWS || XBOX
    static class Program
    {
        static Sandstorm game;
        static Kinect kinectSystem;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            SandstormEditor editor = new SandstormEditor();
            editor.Disposed += new EventHandler(form_Disposed);

            SandstormBeamer beamer = new SandstormBeamer();
            beamer.Disposed += new EventHandler(form_Disposed);

            kinectSystem = new Kinect();

            using (game = new Sandstorm(editor, beamer, kinectSystem))
            {
                game.Run();
            }
        }

        static void form_Disposed (object sender, EventArgs e)
        {
            kinectSystem.StopKinect();
            game.Exit();
        }
    }
#endif
}

