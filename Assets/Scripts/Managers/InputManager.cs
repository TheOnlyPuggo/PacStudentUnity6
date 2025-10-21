using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    private GameInput _gameInput;
    public GameInput GameInput => _gameInput;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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
