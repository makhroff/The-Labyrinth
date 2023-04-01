namespace The_Labyrinth
{
    public struct Vector2
    {
        public int y;
        public int x;
    }

    static class Program
    {
        private static Random random = new Random();

        private static int fieldDimensionX = 20;
        private static int fieldDimensionY = fieldDimensionX * 2;

        private static Vector2 player = new Vector2();
        private static Vector2 finish = new Vector2();
        private static char[,] field = new char[fieldDimensionY, fieldDimensionX];

        private static char playerChar = '@';
        private static char wallChar = 'O';
        private static char airChar = ' ';
        private static char finishChar = 'F';

        private static double wallFrequency = 0.3;

        private static bool isGameRunning = true;

        static void Main(string[] args)
        {
            InitPositions();
            InitField();
            DrawField();
            while (isGameRunning)
            {
                TryToMovePlayer();
            }
            Console.ReadKey();
        }

        static void InitPositions()
        {
            player = GetRandomPosition();
            finish = GetRandomPosition();
        }

        static Vector2 GetRandomPosition()
        {
            Vector2 position = new Vector2();
            position.x = random.Next(0, fieldDimensionY);
            position.y = random.Next(0, fieldDimensionX);
            return position;
        }

        static void InitField()
        {
            Console.Clear();

            for (int i = 0; i < fieldDimensionX; i++)
            {
                for (int j = 0; j < fieldDimensionY; j++)
                {
                    field[j, i] = CreateLabirynthCell();
                }
            }

            field[player.x, player.y] = playerChar;
            field[finish.x, finish.y] = finishChar;
        }

        static void TryToMovePlayer()
        {
            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.UpArrow:

                    if (field[player.x, player.y - 1] == ' ')
                    {
                        field[player.x, player.y] = ' ';
                        player.y -= 1;

                        DrawField();
                    }
                    else if (field[player.x, player.y - 1] == 'F')
                    {
                        Win();
                    }

                    break;
                case ConsoleKey.DownArrow:

                    if (field[player.x, player.y + 1] == ' ')
                    {
                        field[player.x, player.y] = ' ';
                        player.y += 1;

                        DrawField();
                    }
                    else if (field[player.x, player.y + 1] == 'F')
                    {
                        Win();
                    }

                    break;
                case ConsoleKey.LeftArrow:

                    if (field[player.x - 1, player.y] == ' ')
                    {
                        field[player.x, player.y] = ' ';
                        player.x -= 1;

                        DrawField();
                    }
                    else if (field[player.x - 1, player.y] == 'F')
                    {
                        Win();
                    }

                    break;
                case ConsoleKey.RightArrow:

                    if (field[player.x + 1, player.y] == ' ')
                    {
                        field[player.x, player.y] = ' ';
                        player.x += 1;

                        DrawField();
                    }
                    else if (field[player.x + 1, player.y] == 'F')
                    {
                        Win();
                    }

                    break;
            }
        }

        static void DrawField()
        {
            Console.Clear();

            field[player.x, player.y] = playerChar;

            for (int i = 0; i < fieldDimensionX; i++)
            {
                for (int j = 0; j < fieldDimensionY; j++)
                {
                    Console.Write(field[j, i]);
                }
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("player y: " + player.y);
            Console.WriteLine("player x: " + player.x);
            Console.WriteLine();
            Console.WriteLine("finish y:" + finish.y);
            Console.WriteLine("finish x:" + finish.x);
        }
        static char CreateLabirynthCell()
        {
            if (random.NextDouble() <= wallFrequency)
            {
                return wallChar;
            }
            return airChar;
        }

        static void Win()
        {
            isGameRunning = false;
            Console.Clear();
            Console.WriteLine("YOU WIN!");
        }
    }
}