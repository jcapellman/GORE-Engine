using Windows.System;

namespace GORE.Engine
{
    public class InputSystem
    {
        // Simple input state abstraction used by GameWindow
        public bool MoveForward { get; set; }
        public bool MoveBackward { get; set; }
        public bool StrafeLeft { get; set; }
        public bool StrafeRight { get; set; }
        public bool TurnLeft { get; set; }
        public bool TurnRight { get; set; }
        public bool FireTriggerHeld { get; set; }

        public void HandleKey(VirtualKey key, bool isPressed)
        {
            switch (key)
            {
                case VirtualKey.W: MoveForward = isPressed; break;
                case VirtualKey.S: MoveBackward = isPressed; break;
                case VirtualKey.A: StrafeLeft = isPressed; break;
                case VirtualKey.D: StrafeRight = isPressed; break;
                case VirtualKey.Left: TurnLeft = isPressed; break;
                case VirtualKey.Right: TurnRight = isPressed; break;
                case VirtualKey.Control: FireTriggerHeld = isPressed; break;
                case VirtualKey.Q:
                    if (isPressed) PreviousWeaponRequested?.Invoke();
                    break;
                case VirtualKey.E:
                    if (isPressed) NextWeaponRequested?.Invoke();
                    break;
                case VirtualKey.Number1:
                case VirtualKey.Number2:
                case VirtualKey.Number3:
                case VirtualKey.Number4:
                case VirtualKey.Number5:
                case VirtualKey.Number6:
                case VirtualKey.Number7:
                case VirtualKey.Number8:
                    if (isPressed) WeaponNumberKeyPressed?.Invoke((int)key - (int)VirtualKey.Number1);
                    break;
            }
        }

        public System.Action PreviousWeaponRequested;
        public System.Action NextWeaponRequested;
        public System.Action<int> WeaponNumberKeyPressed;
    }
}