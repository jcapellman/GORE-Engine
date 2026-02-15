using System;
using System.Numerics;

namespace GORE.Engine
{
    public class RaycastEngine
    {
        private readonly int[,] _worldMap;
        private readonly int _mapWidth;
        private readonly int _mapHeight;

        public Vector2 PlayerPosition { get; set; }
        public Vector2 PlayerDirection { get; set; }
        public Vector2 CameraPlane { get; set; }

        public RaycastEngine(int[,] worldMap)
        {
            _worldMap = worldMap;
            _mapHeight = worldMap.GetLength(0);
            _mapWidth = worldMap.GetLength(1);

            // Default player position and direction
            PlayerPosition = new Vector2(2.5f, 2.5f);
            PlayerDirection = new Vector2(-1, 0);
            CameraPlane = new Vector2(0, 0.66f);
        }

        public RaycastHit CastRay(int screenX, int screenWidth)
        {
            // Calculate ray position and direction
            float cameraX = 2 * screenX / (float)screenWidth - 1;
            Vector2 rayDir = PlayerDirection + CameraPlane * cameraX;

            // Which box of the map we're in
            int mapX = (int)PlayerPosition.X;
            int mapY = (int)PlayerPosition.Y;

            // Length of ray from current position to next x or y-side
            float sideDistX;
            float sideDistY;

            // Length of ray from one x or y-side to next x or y-side
            float deltaDistX = (rayDir.X == 0) ? float.MaxValue : MathF.Abs(1 / rayDir.X);
            float deltaDistY = (rayDir.Y == 0) ? float.MaxValue : MathF.Abs(1 / rayDir.Y);

            // What direction to step in x or y-direction (either +1 or -1)
            int stepX;
            int stepY;

            bool hit = false;
            int side = 0; // 0 = NS wall hit, 1 = EW wall hit

            // Calculate step and initial sideDist
            if (rayDir.X < 0)
            {
                stepX = -1;
                sideDistX = (PlayerPosition.X - mapX) * deltaDistX;
            }
            else
            {
                stepX = 1;
                sideDistX = (mapX + 1.0f - PlayerPosition.X) * deltaDistX;
            }

            if (rayDir.Y < 0)
            {
                stepY = -1;
                sideDistY = (PlayerPosition.Y - mapY) * deltaDistY;
            }
            else
            {
                stepY = 1;
                sideDistY = (mapY + 1.0f - PlayerPosition.Y) * deltaDistY;
            }

            // Perform DDA
            while (!hit)
            {
                // Jump to next map square in x or y direction
                if (sideDistX < sideDistY)
                {
                    sideDistX += deltaDistX;
                    mapX += stepX;
                    side = 0;
                }
                else
                {
                    sideDistY += deltaDistY;
                    mapY += stepY;
                    side = 1;
                }

                // Check if ray has hit a wall
                if (mapX < 0 || mapX >= _mapWidth || mapY < 0 || mapY >= _mapHeight)
                    break;

                if (_worldMap[mapY, mapX] > 0)
                    hit = true;
            }

            // Calculate distance to the wall
            float perpWallDist;
            if (side == 0)
                perpWallDist = (mapX - PlayerPosition.X + (1 - stepX) / 2) / rayDir.X;
            else
                perpWallDist = (mapY - PlayerPosition.Y + (1 - stepY) / 2) / rayDir.Y;

            // Calculate exact wall hit position for texture mapping
            float wallX;
            if (side == 0)
                wallX = PlayerPosition.Y + perpWallDist * rayDir.Y;
            else
                wallX = PlayerPosition.X + perpWallDist * rayDir.X;
            wallX -= MathF.Floor(wallX);

            return new RaycastHit
            {
                Distance = perpWallDist,
                Side = side,
                MapX = mapX,
                MapY = mapY,
                WallType = (mapX >= 0 && mapX < _mapWidth && mapY >= 0 && mapY < _mapHeight) 
                    ? _worldMap[mapY, mapX] : 0,
                WallX = wallX,
                RayDirX = rayDir.X,
                RayDirY = rayDir.Y
            };
        }

        public void MovePlayer(Vector2 movement, float deltaTime)
        {
            float moveSpeed = 3.0f * deltaTime;
            Vector2 newPos = PlayerPosition + movement * moveSpeed;

            // Collision detection
            int mapX = (int)newPos.X;
            int mapY = (int)newPos.Y;

            if (mapX >= 0 && mapX < _mapWidth && mapY >= 0 && mapY < _mapHeight)
            {
                if (_worldMap[mapY, mapX] == 0)
                {
                    PlayerPosition = newPos;
                }
            }
        }

        public void RotatePlayer(float angle)
        {
            // Rotate direction vector
            float oldDirX = PlayerDirection.X;
            PlayerDirection = new Vector2(
                PlayerDirection.X * MathF.Cos(angle) - PlayerDirection.Y * MathF.Sin(angle),
                oldDirX * MathF.Sin(angle) + PlayerDirection.Y * MathF.Cos(angle)
            );

            // Rotate camera plane
            float oldPlaneX = CameraPlane.X;
            CameraPlane = new Vector2(
                CameraPlane.X * MathF.Cos(angle) - CameraPlane.Y * MathF.Sin(angle),
                oldPlaneX * MathF.Sin(angle) + CameraPlane.Y * MathF.Cos(angle)
            );
        }
    }

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
    }
}
