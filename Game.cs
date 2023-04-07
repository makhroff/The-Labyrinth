namespace The_Labyrinth
{
    public enum GameInput
    {
        Null,
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        Interact,
        UseBomb
    }

    public class Game
    {
        private Random random = new();

        private const int fieldDimensionY = 35;
        private const int fieldDimensionX = fieldDimensionY * 2;

        private LabyrinthCoords playerPos = new();
        private LabyrinthCoords playerOldPos = new();
        private LabyrinthCoords finishPos = new();
        private LabyrinthCoords keyPos = new();

        private char[,] field = new char[fieldDimensionX, fieldDimensionY];
        private double wallFrequency = 0.5;
        private double trapFrequencyWithinWalls = 0.3;

        private const char playerChar = '@';
        private const char wallChar = 'O';
        private const char airChar = '.';
        private const char finishChar = 'F';
        private const char keyChar = 'K';
        private const char trapChar = '#';

        private readonly Dictionary<char, ConsoleColor> colorDictionary = new()
        {
            {playerChar, ConsoleColor.Magenta},
            {wallChar, ConsoleColor.Yellow},
            {airChar, ConsoleColor.White},
            {finishChar, ConsoleColor.Cyan},
            {keyChar, ConsoleColor.Green},
            {trapChar, ConsoleColor.Red}
        };

        private readonly Dictionary<ConsoleKey, GameInput> inputDictionary = new()
        {
            { ConsoleKey.UpArrow, GameInput.MoveUp},
            { ConsoleKey.LeftArrow, GameInput.MoveLeft},
            { ConsoleKey.DownArrow, GameInput.MoveDown},
            { ConsoleKey.RightArrow, GameInput.MoveRight},
            { ConsoleKey.Spacebar, GameInput.UseBomb},
            { ConsoleKey.Enter, GameInput.Interact}
        };

        private readonly List<LabyrinthCoords> usedCoordsList = new();

        private const int interactRadious = 1;
        private const int bombExplosionRadious = 2;
        
        private int amountOfBoms = 5;

        private const int amountOfKeysToCollect = 5;
        private int amountOfCollectedKeys;
        private bool allKeysCollected => amountOfCollectedKeys == amountOfKeysToCollect;

        private bool gameIsRunning = true;

        private int hp = 10;
        private const int trapDamage = 1;

        public void StartGameLoop()
        {
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

        private void InitStartPositions()
        {
            playerPos = GetRandomPosition();
            playerOldPos = playerPos;
            finishPos = GetRandomPosition();
            keyPos = GetRandomPosition();
        }

        private void InitField()
        {
            Console.Clear();

            for (int y = 0; y < fieldDimensionY; y++)
            {
                for (int x = 0; x < fieldDimensionX; x++)
                {
                    field[x, y] = TryToCreateWallOrTrap();
                }
            }

            field[playerPos.x, playerPos.y] = playerChar;
            field[finishPos.x, finishPos.y] = finishChar;
            field[keyPos.x, keyPos.y] = keyChar;
        }

        private void GenerateNewKey()
        {
            keyPos = GetRandomPosition();

            field[keyPos.x, keyPos.y] = keyChar;

            UpdateField(keyPos);
        }

        private GameInput CatchGameInput() =>
            inputDictionary.TryGetValue(Console.ReadKey().Key, out var input) ? input : GameInput.Null;

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

                MovePlayer(newCoords);

                UpdatePlayerOnField();


                bool isATrap = AreCoordsATrap(newCoords);
                if (isATrap)
                {
                    TakeDamage(trapDamage);
                }
            }
            else switch (input)
            {
                case GameInput.Interact:
                    TryToInteractInASquareShape();
                    break;
                case GameInput.UseBomb:
                    TryToUseBomb();
                    break;
            }
        }

        private void MovePlayer(LabyrinthCoords newCoords)
        {
            field[playerPos.x, playerPos.y] = airChar;
            playerOldPos = playerPos;
            playerPos = newCoords;
        }

        private static bool AreCoordsWithinField(LabyrinthCoords coords)
        {
            LabyrinthCoords minCoords = new LabyrinthCoords(-1, -1);
            LabyrinthCoords maxCoords = new LabyrinthCoords(fieldDimensionX, fieldDimensionY);

            bool isWithinField = coords > minCoords && coords < maxCoords;
            return isWithinField;
        }

        private bool AreCoordsLegal(LabyrinthCoords coords) => GetCharFromField(coords) is airChar or trapChar;
        private bool AreCoordsATrap(LabyrinthCoords coords) => GetCharFromField(coords) == trapChar;

        private void TryToInteractInASquareShape()
        {
            for (int y = (playerPos.y - interactRadious); y < (playerPos.y + interactRadious + 1); y++)
            {
                for (int x = (playerPos.x - interactRadious); x < (playerPos.x + interactRadious + 1); x++)
                {
                    var intermediateCoords = new LabyrinthCoords(x, y);

                    if (!AreCoordsWithinField(intermediateCoords))
                        continue;

                    switch (field[x, y])
                    {
                        case finishChar:
                            TryToWin();
                            break;
                        
                        case keyChar:
                            CollectKey(x, y, intermediateCoords);
                            break;
                    }
                }
            }
        }

        private void CollectKey(int x, int y, LabyrinthCoords intermediateCoords)
        {
            usedCoordsList.Remove(keyPos);
            
            field[x, y] = airChar;
            UpdateField(intermediateCoords);

            amountOfCollectedKeys++;

            if (amountOfKeysToCollect > amountOfCollectedKeys)
                GenerateNewKey();
        }

        private void TryToUseBomb()
        {
            if (amountOfBoms == 0)
                return;

            for (int y = (playerPos.y - bombExplosionRadious); y < (playerPos.y + bombExplosionRadious + 1); y++)
            {
                for (int x = (playerPos.x - bombExplosionRadious); x < (playerPos.x + bombExplosionRadious + 1); x++)
                {
                    var intermediateCoords = new LabyrinthCoords(x, y);
                    if (!AreCoordsWithinField(intermediateCoords)) continue;

                    if (field[x, y] is wallChar or trapChar)
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

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"YOUR CURRENT HP: {hp}".PadRight(Console.WindowWidth - 1));

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"AMOUNT OF BOMBS: {amountOfBoms}");

            if (amountOfCollectedKeys == amountOfKeysToCollect) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"AMOUNT OF KEYS COLLECTED: {amountOfCollectedKeys} / {amountOfKeysToCollect}");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Press 'Enter' to interact (finishPos)");
            Console.WriteLine("Press the 'Spacebar' to use the BOMB!! " +
                              "\nIt will explode in interactRadious of 2 (in shape of a square)");
        }

        private void UpdateField(LabyrinthCoords coords)
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

        private char TryToCreateWallOrTrap()
        {
            if (random.NextDouble() <= wallFrequency)
            {
                if (random.NextDouble() <= trapFrequencyWithinWalls)
                    return trapChar;
                else
                    return wallChar;
            }
            return airChar;
        }

        private void TakeDamage(int damage)
        {
            hp -= damage;
            if(hp <= 0) 
                Die();
        }
        
        private void TryToWin()
        {
            if(!allKeysCollected) return;
            
            SendConsoleStopMessage("YOU WIN!");
        }
        
        private void Die() => SendConsoleStopMessage("YOU ARE DEAD");

        private void SendConsoleStopMessage(string message)
        {
            gameIsRunning = false;
            Console.Clear();
            Console.WriteLine(message);
        }

        
        private LabyrinthCoords GetRandomPosition()
        {
            LabyrinthCoords position = new LabyrinthCoords
            {
                x = random.Next(0, fieldDimensionX - 1),
                y = random.Next(0, fieldDimensionY - 1)
            };

            if (usedCoordsList.Contains(position))
                return GetRandomPosition();
            else
                usedCoordsList.Add(position);
            
            return position;
        }
        
        private LabyrinthCoords CalculateNewPlayerCoordinates(GameInput input) => input switch
        {
            GameInput.MoveUp => new LabyrinthCoords(playerPos.x, playerPos.y - 1),
            GameInput.MoveDown => new LabyrinthCoords(playerPos.x, playerPos.y + 1),
            GameInput.MoveLeft => new LabyrinthCoords(playerPos.x - 1, playerPos.y),
            GameInput.MoveRight => new LabyrinthCoords(playerPos.x + 1, playerPos.y),
            _ => throw new NotImplementedException("No correct format of player movement found.")
        };
        
        private char GetCharFromField(LabyrinthCoords coords) => field[coords.x, coords.y];
    }
}