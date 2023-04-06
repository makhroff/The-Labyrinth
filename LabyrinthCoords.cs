namespace The_Labyrinth
{
    public class LabyrinthCoords
    {
        public int x;
        public int y;

        public LabyrinthCoords()
        {
            x = 0;
            y = 0;
        }

        public LabyrinthCoords(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool operator >(LabyrinthCoords a, LabyrinthCoords b) => a.x > b.x && a.y > b.y;

        public static bool operator <(LabyrinthCoords a, LabyrinthCoords b) => a.x < b.x && a.y < b.y;
    }
}
