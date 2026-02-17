using System;
using System.Collections.Generic;
using System.Numerics;

namespace GORE.Engine
{
    // DoorState moved to src/Engine/Data/DoorState.cs

    public class RaycastEngine
    {
        private readonly int[,] _worldMap;
        private readonly int _mapWidth;
        private readonly int _mapHeight;
        private readonly Dictionary<(int, int), DoorState> _doors;
        private const int DOOR_TEXTURE_ID = 5; // Doors use texture ID 5
        private const float DOOR_OPEN_SPEED = 2.0f;
        private const float DOOR_CLOSE_DELAY = 3.0f; // Delay before door starts closing

        public Vector2 PlayerPosition { get; set; }
        public Vector2 PlayerDirection { get; set; }
        public Vector2 CameraPlane { get; set; }

        private readonly EventSystem _eventSystem;

        public RaycastEngine(int[,] worldMap, EventSystem eventSystem = null)
        {
            _worldMap = worldMap;
            _mapHeight = worldMap.GetLength(0);
            _mapWidth = worldMap.GetLength(1);
            _doors = new Dictionary<(int, int), DoorState>();
            _eventSystem = eventSystem;

            // Find all doors in the map and initialize their state
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    if (_worldMap[y, x] == DOOR_TEXTURE_ID)
                    {
                        _doors[(x, y)] = new DoorState
                        {
                            MapX = x,
                            MapY = y,
                            OpenAmount = 0f
                        };
                        System.Diagnostics.Debug.WriteLine($"Door found at ({x}, {y})");
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"Total doors initialized: {_doors.Count}");

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

            // Perform DDA with max distance limit
            const int maxRaySteps = 20; // Limit ray distance for performance
            int steps = 0;

            while (!hit && steps < maxRaySteps)
            {
                steps++;

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

                int cellValue = _worldMap[mapY, mapX];

                // Check if it's a door
                if (cellValue == DOOR_TEXTURE_ID && _doors.TryGetValue((mapX, mapY), out var doorState))
                {
                    // If door is mostly open (>90%), treat as passable
                    if (doorState.OpenAmount < 0.9f)
                    {
                        // Door is closed or partially open, register as hit
                        hit = true;
                    }
                }
                else if (cellValue > 0)
                {
                    hit = true;
                }
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
                int cellValue = _worldMap[mapY, mapX];

                // Allow movement through empty cells
                if (cellValue == 0)
                {
                    PlayerPosition = newPos;
                }
                // Allow movement through open doors (90% or more open)
                else if (cellValue == DOOR_TEXTURE_ID && 
                         _doors.TryGetValue((mapX, mapY), out var doorState) &&
                         doorState.OpenAmount >= 0.9f)
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

        /// <summary>
        /// Update door states (opening/closing animations)
        /// </summary>
        public void UpdateDoors(float deltaTime)
        {
            foreach (var door in _doors.Values)
            {
                if (door.IsOpening)
                {
                    door.OpenAmount += DOOR_OPEN_SPEED * deltaTime;
                    if (door.OpenAmount >= 1.0f)
                    {
                        door.OpenAmount = 1.0f;
                        door.IsOpening = false;
                        door.CloseTimer = DOOR_CLOSE_DELAY; // Start close timer
                    }
                }
                else if (door.OpenAmount > 0f && !door.IsOpening)
                {
                    // Door is open, wait before closing
                    door.CloseTimer -= deltaTime;

                    if (door.CloseTimer <= 0f)
                    {
                        door.IsClosing = true;
                    }
                }

                if (door.IsClosing)
                {
                    door.OpenAmount -= DOOR_OPEN_SPEED * deltaTime;
                    if (door.OpenAmount <= 0f)
                    {
                        door.OpenAmount = 0f;
                        door.IsClosing = false;
                    }
                }
            }
        }

        /// <summary>
        /// Try to interact with a door in front of the player
        /// </summary>
        public bool TryInteractWithDoor()
        {
            // Check cells around the player within reach distance
            float reachDistance = 1.5f;

            // First, check the cell directly in front of the player
            Vector2 checkPos = PlayerPosition + PlayerDirection * reachDistance;
            int checkX = (int)checkPos.X;
            int checkY = (int)checkPos.Y;

            // Check the cell in front
            if (TryActivateDoorAtPosition(checkX, checkY))
            {
                _eventSystem?.Publish(new DoorInteractedEvent());
                return true;
            }

            // Also check adjacent cells in case player isn't perfectly aligned
            // Check player's current cell
            int playerX = (int)PlayerPosition.X;
            int playerY = (int)PlayerPosition.Y;

            if (TryActivateDoorAtPosition(playerX, playerY))
            {
                _eventSystem?.Publish(new DoorInteractedEvent());
                return true;
            }

            // Check cells in a 3x3 grid around player
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int testX = playerX + dx;
                    int testY = playerY + dy;

                    // Calculate distance to this cell center
                    Vector2 cellCenter = new Vector2(testX + 0.5f, testY + 0.5f);
                    float dist = Vector2.Distance(PlayerPosition, cellCenter);

                    if (dist <= reachDistance)
                    {
                        if (TryActivateDoorAtPosition(testX, testY))
                        {
                            _eventSystem?.Publish(new DoorInteractedEvent());
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Helper method to try activating a door at a specific position
        /// </summary>
        private bool TryActivateDoorAtPosition(int x, int y)
        {
            // Check bounds
            if (x < 0 || x >= _mapWidth || y < 0 || y >= _mapHeight)
                return false;

            // Check if there's a door at this position
            if (_worldMap[y, x] == DOOR_TEXTURE_ID &&
                _doors.TryGetValue((x, y), out var doorState))
            {
                // Toggle door state
                if (doorState.OpenAmount < 0.5f && !doorState.IsOpening)
                {
                    // Door is closed or closing, open it
                    doorState.IsOpening = true;
                    doorState.IsClosing = false;
                    System.Diagnostics.Debug.WriteLine($"Door activated at ({x}, {y})");
                    return true;
                }
                // If already open or opening, still return true to indicate a door was found
                System.Diagnostics.Debug.WriteLine($"Door at ({x}, {y}) already open/opening");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get door state for a specific cell (for rendering)
        /// </summary>
        public DoorState GetDoorState(int mapX, int mapY)
        {
            _doors.TryGetValue((mapX, mapY), out var doorState);
            return doorState;
        }
    }

    // RaycastHit moved to src/Engine/Data/RaycastHit.cs
}
