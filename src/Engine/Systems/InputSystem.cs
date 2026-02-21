using Silk.NET.Input;

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

        public void HandleKey(Key key, bool isPressed)
        {
            switch (key)
            {
                case Key.W: MoveForward = isPressed; break;
                case Key.S: MoveBackward = isPressed; break;
                case Key.A: StrafeLeft = isPressed; break;
                case Key.D: StrafeRight = isPressed; break;
                case Key.Left: TurnLeft = isPressed; break;
                case Key.Right: TurnRight = isPressed; break;
                case Key.ControlLeft:
                case Key.ControlRight:
                    FireTriggerHeld = isPressed; break;
                case Key.Q: if (isPressed) PreviousWeaponRequested?.Invoke(); break;
                case Key.E: if (isPressed) NextWeaponRequested?.Invoke(); break;
                case Key.Number1:
                case Key.Number2:
                case Key.Number3:
                case Key.Number4:
                case Key.Number5:
                case Key.Number6:
                case Key.Number7:
                case Key.Number8:
                    if (isPressed) WeaponNumberKeyPressed?.Invoke((int)key - (int)Key.Number1);
                    break;
            }
        }

        public System.Action PreviousWeaponRequested;
        public System.Action NextWeaponRequested;
        public System.Action<int> WeaponNumberKeyPressed;
    }
}