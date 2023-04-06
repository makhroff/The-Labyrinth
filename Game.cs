namespace The_Labyrinth
{
    public class Game
    {
        private Random random = new Random();

        private const int fieldDimensionY = 50;
        private const int fieldDimensionX = fieldDimensionY * 2;

        private Vector2 playerPos = new Vector2();
        private Vector2 playerOldPos = new Vector2();
        private Vector2 finishPos = new Vector2();
        private Vector2 keyPos = new Vector2();

        private char[,] field = new char[fieldDimensionX, fieldDimensionY];
        private double wallFrequency = 0.3;

        private const char playerChar = '@';
        private const char wallChar = 'O';
        private const char airChar = '.';
        private const char finishChar = 'F';
        private const char keyChar = 'K';

        private Dictionary<char, ConsoleColor> colorDictionary = new();

        private int bombExplosionRadious = 2;
        private int amountOfBoms = 5;

        private int amountOfKeysToCollect = 5;
        private int amountOfCollectedKeys = 0;
        private bool areAllKeysCollected => amountOfCollectedKeys == amountOfKeysToCollect;

        private bool gameIsRunning = true;

        public void StartGameLoop()
        {
            SetupColorDictionary();
            InitStartPositions();
            InitField();
            DrawField();

            while (gameIsRunning)
            {
                UpdateUi();

                var input = CatchGameInput();
                ProcessCachedInput(input);
            }
        }

        private void SetupColorDictionary()
        {
            colorDictionary.Add(playerChar, ConsoleColor.Magenta);
            colorDictionary.Add(wallChar, ConsoleColor.Yellow);
            colorDictionary.Add(airChar, ConsoleColor.White);
            colorDictionary.Add(finishChar, ConsoleColor.DarkBlue);
            colorDictionary.Add(keyChar, ConsoleColor.DarkGreen);
        }

        private void InitStartPositions()
        {
            playerPos = GetRandomPosition();
            playerOldPos = playerPos;
            finishPos = GetRandomPosition();
            keyPos = GetRandomPosition();

            if (keyPos == finishPos || keyPos == playerPos) keyPos = GetRandomPosition();
        }

        private void InitField()
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

        private void GenerateNewKey()
        {
            keyPos = GetRandomPosition();
            if (keyPos == finishPos || keyPos == playerPos) keyPos = GetRandomPosition();

            field[keyPos.x, keyPos.y] = keyChar;

            UpdateField(keyPos);
        }

        private GameInput CatchGameInput() => Console.ReadKey().Key switch
        {
            ConsoleKey.UpArrow => GameInput.MoveUp,
            ConsoleKey.DownArrow => GameInput.MoveDown,
            ConsoleKey.RightArrow => GameInput.MoveRight,
            ConsoleKey.LeftArrow => GameInput.MoveLeft,
            ConsoleKey.Spacebar => GameInput.UseBomb,
            ConsoleKey.Enter => GameInput.Interact,
            _ => GameInput.Null
        };

        private void ProcessCachedInput(GameInput input)
        {
            var playerIsMoving = input.IsMoving();

            if (playerIsMoving)
            {
                var newCoords = CalculateNewPlayerCoordinates(input);

                if (!AreCoordsWithinField(newCoords))
                    return;
                if (!AreCoordsLegal(newCoords))
                    return;

                field[playerPos.x, playerPos.y] = airChar;
                playerOldPos = playerPos;
                playerPos = newCoords;

                UpdatePlayerOnField();
            }
            else switch (input)
            {
                case GameInput.Interact:
                    TryToInteractInASquareShape(1);
                    break;
                case GameInput.UseBomb when amountOfBoms == 0:
                    return;
                case GameInput.UseBomb:
                    UseBomb();
                    break;
            }
        }

        private static bool AreCoordsWithinField(Vector2 coords)
        {
            if (new Vector2(-1, coords.y) >= coords) return false;
            if (new Vector2(coords.x, -1) >= coords) return false;

            if (new Vector2(fieldDimensionX, coords.y) <= coords) return false;
            if (new Vector2(coords.x, fieldDimensionY) <= coords) return false;

            return true;
        }

        private bool AreCoordsLegal(Vector2 coords) => GetCharFromField(coords) switch
        {
            airChar => true,
            _ => false
        };

        private void TryToInteractInASquareShape(int radious)
        {
            for (int y = (playerPos.y - radious); y < (playerPos.y + radious + 1); y++)
            {
                for (int x = (playerPos.x - radious); x < (playerPos.x + radious + 1); x++)
                {
                    var intermediateCoords = new Vector2(x, y);

                    if (!AreCoordsWithinField(intermediateCoords))
                        continue;

                    if (field[x, y] == finishChar && areAllKeysCollected)
                    {
                        Win();
                    }
                    if (field[x, y] == keyChar)
                    {
                        field[x, y] = airChar;
                        UpdateField(intermediateCoords);

                        amountOfCollectedKeys++;

                        if (amountOfKeysToCollect > amountOfCollectedKeys)
                            GenerateNewKey();

                        break;
                    }
                }
            }
        }

        private void UseBomb()
        {
            for (int y = (playerPos.y - bombExplosionRadious); y < (playerPos.y + bombExplosionRadious + 1); y++)
            {
                for (int x = (playerPos.x - bombExplosionRadious); x < (playerPos.x + bombExplosionRadious + 1); x++)
                {
                    var intermediateCoords = new Vector2(x, y);
                    if (!AreCoordsWithinField(intermediateCoords)) continue;

                    if (field[x, y] == wallChar)
                    {
                        field[x, y] = airChar;
                        UpdateField(intermediateCoords);
                    }
                }
            }

            amountOfBoms--;
        }

        private void DrawField()
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
        }

        private void UpdateUi()
        {
            Console.SetCursorPosition(0, fieldDimensionY + 1);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nAMOUNT OF BOMBS: {amountOfBoms}");

            if (amountOfCollectedKeys == amountOfKeysToCollect) Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"\nAMOUNT OF KEYS COLLECTED: {amountOfCollectedKeys} / {amountOfKeysToCollect}");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nPress 'Enter' to interact (finishPos)");
            Console.WriteLine($"Press the 'Spacebar' to use the BOMB!! \nIt will explode in radious of 2 (in shape of a square)");
        }

        private void UpdateField(Vector2 coords)
        {
            Console.ForegroundColor = colorDictionary[field[coords.x, coords.y]];
            Console.SetCursorPosition(coords.x, coords.y);
            Console.Write(field[coords.x, coords.y]);
        }

        private void UpdatePlayerOnField()
        {
            Console.ForegroundColor = colorDictionary[playerChar];
            Console.SetCursorPosition(playerPos.x, playerPos.y);
            Console.Write(playerChar);

            Console.ForegroundColor = colorDictionary[airChar];
            Console.SetCursorPosition(playerOldPos.x, playerOldPos.y);
            Console.Write(airChar);
        }

        private char TryToCreateWall()
        {
            if (random.NextDouble() <= wallFrequency)
            {
                return wallChar;
            }
            return airChar;
        }


        private void Win()
        {
            gameIsRunning = false;
            Console.Clear();
            Console.WriteLine("YOU WIN!");
        }



        private Vector2 GetRandomPosition()
        {
            Vector2 position = new Vector2();
            position.x = random.Next(0, fieldDimensionX);
            position.y = random.Next(0, fieldDimensionY);
            return position;
        }
        private Vector2 CalculateNewPlayerCoordinates(GameInput input) => input switch
        {
            GameInput.MoveUp => new Vector2(playerPos.x, playerPos.y - 1),
            GameInput.MoveDown => new Vector2(playerPos.x, playerPos.y + 1),
            GameInput.MoveLeft => new Vector2(playerPos.x - 1, playerPos.y),
            GameInput.MoveRight => new Vector2(playerPos.x + 1, playerPos.y),
            _ => throw new NotImplementedException("No correct format of player movement found.")
        };
        private char GetCharFromField(Vector2 coords) => field[coords.x, coords.y];
    }
}