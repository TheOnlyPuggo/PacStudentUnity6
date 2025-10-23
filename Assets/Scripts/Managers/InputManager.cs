using UnityEngine;

public class InputManager : MonoBehaviour
{
    private GameInput _gameInput;
    public GameInput GameInput => _gameInput;

    private void Awake()
    {
        _gameInput = new GameInput();
    }

    private void OnEnable()
    {
        _gameInput.Enable();
    }

    private void OnDisable()
    {
        _gameInput.Disable();
    }
}
