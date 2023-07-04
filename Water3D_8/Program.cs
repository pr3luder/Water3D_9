using System;

namespace Water3D
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            using (var game = new GraphicsSystem())
            {
                game.Run();
            }
        }
    }
#endif
}

