namespace The_Labyrinth
{
    public enum GameInput
    {
        Null = 0,
        MoveUp = 1,
        MoveDown = 2,
        MoveLeft = 3,
        MoveRight = 4,
        Interact = 5,
        UseBomb = 6,
    }

    static class Program
    {
        static Game game = new Game();

        static void Main(string[] args)
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