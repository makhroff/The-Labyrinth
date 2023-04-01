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

        public static bool operator >=(Vector2 a, Vector2 b) => a.x >= b.x && a.y >= b.y;

        public static bool operator <=(Vector2 a, Vector2 b) => a.x <= b.x && a.y <= b.y;
    }

    static class Program
    {
        public enum GameInput
        {
            Null,
            MoveUp,
            MoveDown,
            MoveLeft,
            MoveRight,
            Interact,
            UseBomb,
        }

        private static Random random = new Random();

        private static int fieldDimensionY = 50;
        private static int fieldDimensionX = fieldDimensionY * 2;

        private static Vector2 playerPos = new Vector2();
        private static Vector2 playerOldPos = new Vector2();
        private static Vector2 finishPos = new Vector2();
        private static Vector2 keyPos = new Vector2();

        private static char[,] field = new char[fieldDimensionX, fieldDimensionY];
        private static double wallFrequency = 0.3;

        private const char playerChar = '@';
        private const char wallChar = 'O';
        private const char airChar = '.';
        private const char finishChar = 'F';
        private const char keyChar = 'K';

        private static Dictionary<char, ConsoleColor> colorDictionary = new();

        private static int bombExplosionRadious = 2;
        private static int amountOfBoms = 3;

        private static int amountOfKeysToCollect = 5;
        private static int keysCollected = 0;

        private static bool gameIsRunning = true;

        static void Main(string[] args)
        {
            SetupConsoleParams();
            SetupColorDictionary();

            GameLoop();
            Console.ReadLine();
        }

        static void SetupConsoleParams()
        {
            Console.SetBufferSize(Console.WindowWidth, Console.WindowWidth);
            Console.CursorVisible = false;
        }
        static void SetupColorDictionary()
        {
            colorDictionary.Add(playerChar, ConsoleColor.Magenta);
            colorDictionary.Add(wallChar, ConsoleColor.Yellow);
            colorDictionary.Add(airChar, ConsoleColor.White);
            colorDictionary.Add(finishChar, ConsoleColor.DarkBlue);
            colorDictionary.Add(keyChar, ConsoleColor.DarkGreen);
        }

        static void GameLoop()
        {
            InitPositions();
            InitField();
            DrawField();

            while (gameIsRunning)
            {
                var input = TryToCatchGameInput();
                ProcessCachedInput(input);
                if (gameIsRunning) DrawPlayer();
            }
        }

        static void InitPositions()
        {
            playerPos = GetRandomPosition();
            playerOldPos = playerPos;
            finishPos = GetRandomPosition();
            keyPos = GetRandomPosition();

            if(keyPos == finishPos || keyPos == playerPos) keyPos = GetRandomPosition();
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

            field[playerPos.x, playerPos.y] = playerChar;
            field[finishPos.x, finishPos.y] = finishChar;
            field[keyPos.x, keyPos.y] = keyChar;
        }

        static void GenerateNewKey()
        {
            keyPos = GetRandomPosition();
            if (keyPos == finishPos || keyPos == playerPos) keyPos = GetRandomPosition();

            Console.ForegroundColor = colorDictionary[keyChar];
            Console.SetCursorPosition(keyPos.x, keyPos.y);
            Console.Write(keyChar);
            field[keyPos.x, keyPos.y] = keyChar;
        }

        static GameInput TryToCatchGameInput()
        {
            while (gameIsRunning)
            {
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                        return GameInput.MoveUp;
                        
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        return GameInput.MoveDown;

                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.A:
                        return GameInput.MoveLeft;

                    case ConsoleKey.RightArrow:
                    case ConsoleKey.D:
                        return GameInput.MoveRight;

                    case ConsoleKey.Enter:
                        return GameInput.Interact;

                    case ConsoleKey.F:
                        return GameInput.UseBomb;
                }
            }

            return GameInput.Null;
        }

        static void ProcessCachedInput(GameInput input)
        {
            bool playerIsMoving = (input == GameInput.MoveUp) || (input == GameInput.MoveDown) || (input == GameInput.MoveLeft) || (input == GameInput.MoveRight);

            if (playerIsMoving)
            {
                var newCoords = CalculateNewPlayerCoordinates(input);

                if (!AreCoordsWithinField(newCoords)) return;
                if (!AreCoordsLegal(newCoords)) return;

                field[playerPos.x, playerPos.y] = airChar;
                playerOldPos = playerPos;
                playerPos = newCoords;
            }
            else if(input == GameInput.Interact)
            {
                TryToInteractInASquareShape(1);
            }
            else if(input == GameInput.UseBomb)
            {
                if(amountOfBoms == 0) return;
                UseBomb();
                amountOfBoms--;

                DrawField();
            }
        }

        static bool AreCoordsWithinField(Vector2 coords)
        {
            if (new Vector2(-1, coords.y) >= coords) return false;
            if (new Vector2(coords.x, -1) >= coords) return false;

            if (new Vector2(fieldDimensionX, coords.y) <= coords) return false;
            if (new Vector2(coords.x, fieldDimensionY) <= coords) return false;

            return true;
        }

        static bool AreCoordsLegal(Vector2 coords) => GetCharFromField(coords) switch
        {
            wallChar => false,
            finishChar => false,
            keyChar => false,
            _ => true
        };

        static void TryToInteractInASquareShape(int radious)
        {
            for(int y = (playerPos.y - radious); y < (playerPos.y + radious + 1); y++)
            {
                for (int x = (playerPos.x - radious); x < (playerPos.x + radious + 1); x++)
                {
                    var intermediateCoords = new Vector2(x, y);
                    if(!AreCoordsWithinField(intermediateCoords)) continue;

                    if (field[x, y] == finishChar)
                    {
                        if (keysCollected != amountOfKeysToCollect) continue;

                        Win();
                        break;
                    }
                    if(field[x, y] == keyChar)
                    {
                        field[x, y] = airChar;
                        keysCollected++;

                        if(amountOfKeysToCollect > keysCollected) GenerateNewKey();

                        DrawField();

                        break;
                    }
                }
            }
        }

        static void UseBomb()
        {
            for (int y = (playerPos.y - bombExplosionRadious); y < (playerPos.y + bombExplosionRadious + 1); y++)
            {
                for (int x = (playerPos.x - bombExplosionRadious); x < (playerPos.x + bombExplosionRadious + 1); x++)
                {
                    var intermediateCoords = new Vector2(x, y);
                    if (!AreCoordsWithinField(intermediateCoords)) continue;

                    if (field[x, y] == wallChar)
                        field[x, y] = airChar;
                }
            }
        }

        static void DrawField()
        {
            Console.Clear();
            field[playerPos.x, playerPos.y] = playerChar;

            for (int y = 0; y < fieldDimensionY; y++)
            {
                for (int x = 0; x < fieldDimensionX; x++)
                {
                    Console.ForegroundColor = colorDictionary[field[x, y]];
                    Console.Write(field[x, y]);
                }
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nAMOUNT OF BOMBS: {amountOfBoms}");

            if(keysCollected == amountOfKeysToCollect) Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"\nAMOUNT OF KEYS COLLECTED: {keysCollected} / {amountOfKeysToCollect}");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n\nPress 'Enter' to interact (finishPos)");
            Console.WriteLine($"\nPress 'F' to use BOMB!! \nIt will explode in radious of 2 (in shape of square)");
        }

        static void DrawPlayer()
        {
            Console.ForegroundColor = colorDictionary[playerChar];
            Console.SetCursorPosition(playerPos.x, playerPos.y);
            Console.Write(playerChar);

            Console.ForegroundColor = colorDictionary[airChar];
            Console.SetCursorPosition(playerOldPos.x, playerOldPos.y);
            Console.Write(airChar);
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
            gameIsRunning = false;
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
            GameInput.MoveUp => new(playerPos.x, playerPos.y - 1),
            GameInput.MoveDown => new(playerPos.x, playerPos.y + 1),
            GameInput.MoveLeft => new(playerPos.x - 1, playerPos.y),
            GameInput.MoveRight => new(playerPos.x + 1, playerPos.y),
            _ => throw new NotImplementedException("No correct format of playerPos movement found.")
        };
        static char GetCharFromField(Vector2 coords) => field[coords.x, coords.y];
    }
}