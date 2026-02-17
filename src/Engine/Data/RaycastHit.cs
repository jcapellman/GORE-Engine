namespace GORE.Engine
{
    public struct RaycastHit
    {
        public float Distance;
        public int Side;
        public int MapX;
        public int MapY;
        public int WallType;
        public float WallX;
        public float RayDirX;
        public float RayDirY;
        public bool IsDoor => WallType == 5; // Doors use texture ID 5
    }
}
