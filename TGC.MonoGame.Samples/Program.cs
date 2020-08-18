using System;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            using (var game = new TGCViewer())
            {
                game.Run();
            }
        }
    }
}