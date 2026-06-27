using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBoatControllerInput : BoatControllerInputBase
{
    private Vector2 moveInput;

    private void Update()
    {
        if(Gamepad.current != null)
        {
            Debug.Log(Gamepad.current.leftStick.value);
        }
    }

    public override Vector2 GetMoveInput()
    {
        return moveInput;
    }

    private void OnMove(InputValue value)
    {
        moveInput.x = value.Get<Vector2>().x;
    }

    private void OnAccelerate(InputValue value)
    {
        moveInput.y = value.Get<float>();
    }
}
