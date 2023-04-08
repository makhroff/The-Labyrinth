namespace The_Labyrinth
{
    public static class GameInputExtensions
    {
        public static bool IsMoving(this GameInput input) => 
            input is GameInput.MoveUp or GameInput.MoveDown or GameInput.MoveLeft or GameInput.MoveRight;
    }
}
