namespace The_Labyrinth
{
    static class Program
    {
        static Game game = new Game();

        static void Main()
        {
            SetupConsoleParams();

            game.StartGameLoop();
            Console.ReadLine();
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "This game is only for Windows users")]
        static void SetupConsoleParams()
        {
            Console.SetBufferSize(Console.WindowWidth, Console.WindowWidth);
            Console.CursorVisible = false;
        }
    }
}