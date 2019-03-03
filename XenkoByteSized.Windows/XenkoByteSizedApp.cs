using Xenko.Engine;

namespace XenkoByteSized.Windows
{
    class XenkoByteSizedApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}