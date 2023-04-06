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

        public static bool operator >=(Vector2 a, Vector2 b) => a.x >= b.x && a.y >= b.y;

        public static bool operator <=(Vector2 a, Vector2 b) => a.x <= b.x && a.y <= b.y;
    }
}
