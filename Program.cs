namespace The_Labyrinth
{
    public class Vector2
    {
        public int x;
        public int y;

        public Vector2()
        {
            x = 0;
            y = 0;
        }

        public Vector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public void Deconstruct(out int X, out int Y)
        {
            X = x;
            Y = y;
        }
    }

    static class Program
    {
        public enum GameInput
        {
            MoveUp,
            MoveDown,
            MoveLeft,
            MoveRight,
        }

        private static Random random = new Random();

        private static int fieldDimensionY = 20;
        private static int fieldDimensionX = fieldDimensionY * 2;

        private static Vector2 player = new Vector2();
        private static Vector2 finish = new Vector2();
        private static char[,] field = new char[fieldDimensionX, fieldDimensionY];

        private const char playerChar = '@';
        private const char wallChar = 'O';
        private const char airChar = ' ';
        private const char finishChar = 'F';

        private static double wallFrequency = 0.3;

        private static bool isGameRunning = true;

        static void Main(string[] args)
        {
            GameLoop();
        }

        static void GameLoop()
        {
            InitPositions();
            InitField();
            DrawField();
            while (isGameRunning)
            {
                var input = TryToCatchGameInput();
                ProcessCachedInput(input);
            }
            Console.ReadKey();
        }

        static void InitPositions()
        {
            player = GetRandomPosition();
            finish = GetRandomPosition();
        }
        static void InitField()
        {
            Console.Clear();

            for (int y = 0; y < fieldDimensionY; y++)
            {
                for (int x = 0; x < fieldDimensionX; x++)
                {
                    field[x, y] = TryToCreateWall();
                }
            }

            field[player.x, player.y] = playerChar;
            field[finish.x, finish.y] = finishChar;
        }

        static GameInput TryToCatchGameInput()
        {
            while (true)
            {
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.UpArrow:
                        return GameInput.MoveUp;
                        
                    case ConsoleKey.DownArrow:
                        return GameInput.MoveDown;

                    case ConsoleKey.LeftArrow:
                        return GameInput.MoveLeft;

                    case ConsoleKey.RightArrow:
                        return GameInput.MoveRight;
                }
            }
        }

        static void ProcessCachedInput(GameInput input)
        {
            bool playerIsMoving = (input == GameInput.MoveUp) || (input == GameInput.MoveDown) || (input == GameInput.MoveLeft) || (input == GameInput.MoveRight);

            if (playerIsMoving)
            {
                var newCoords = CalculateNewPlayerCoordinates(input);

                if (!CoordsWithinField(newCoords)) return;
                if (!CoordsLegal(newCoords)) return;

                field[player.x, player.y] = airChar;
                player = newCoords;

                DrawField();
            }
        }


        static bool CoordsWithinField(Vector2 coords)
        {
            if ((-1, coords.y) == (coords.x, coords.y)) return false;
            if ((coords.x, -1) == (coords.x, coords.y)) return false;

            if ((fieldDimensionX, coords.y) == (coords.x, coords.y)) return false;
            if ((coords.x, fieldDimensionY) == (coords.x, coords.y)) return false;

            return true;
        }
        static bool CoordsLegal(Vector2 coords) => GetCharFromField(coords) switch
        {
            wallChar => false,
            finishChar => false,
            _ => true
        };

        static void DrawField()
        {
            Console.Clear();

            field[player.x, player.y] = playerChar;

            for (int y = 0; y < fieldDimensionY; y++)
            {
                for (int x = 0; x < fieldDimensionX; x++)
                {
                    Console.Write(field[x, y]);
                }
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("player y: " + player.y);
            Console.WriteLine("player x: " + player.x);
            Console.WriteLine();
            Console.WriteLine("finish y:" + finish.y);
            Console.WriteLine("finish x:" + finish.x);
        }
        static char TryToCreateWall()
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



        static Vector2 GetRandomPosition()
        {
            Vector2 position = new Vector2();
            position.x = random.Next(0, fieldDimensionX);
            position.y = random.Next(0, fieldDimensionY);
            return position;
        }
        static Vector2 CalculateNewPlayerCoordinates(GameInput input) => input switch
        {
            GameInput.MoveUp => new(player.x, player.y - 1),
            GameInput.MoveDown => new(player.x, player.y + 1),
            GameInput.MoveLeft => new(player.x - 1, player.y),
            GameInput.MoveRight => new(player.x + 1, player.y),
            _ => throw new NotImplementedException("No correct format of player movement found.")
        };
        static char GetCharFromField(Vector2 coords) => field[coords.x, coords.y];
    }
}