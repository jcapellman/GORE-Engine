using System;

namespace GORE.Engine
{
    public class DoorState
    {
        public int MapX { get; set; }
        public int MapY { get; set; }
        public float OpenAmount { get; set; } = 0f; // 0 = closed, 1 = fully open
        public bool IsOpening { get; set; } = false;
        public bool IsClosing { get; set; } = false;
        public float CloseTimer { get; set; } = 0f;
    }
}
