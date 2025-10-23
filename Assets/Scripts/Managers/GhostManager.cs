using TMPro;
using UnityEngine;

public class GhostManager : MonoBehaviour
{
    public bool GhostsAreScared { get; private set; } = false;

    [SerializeField] private float recoveryInterval;
    [SerializeField] private GameObject ghostTimerObj;
    [SerializeField] private TMP_Text ghostTimerText;

    private float _scaredTimer = 0.0f;
    private float _recoveryIntervalTimer = 0.0f;
    private bool _recoveryGhostAnimNormal = false;
    private float _scaredLength;
    private float _recoveryLength;

    public enum GhostAnimState
    {
        Normal,
        Scared,
    }

    [SerializeField] private GhostController[] ghostControllers;

    private void Start()
    {
        ghostTimerObj.SetActive(false);
    }

    private void Update()
    {
        GhostScaredHandle();
    }

    private void GhostScaredHandle()
    {
        if (!GhostsAreScared || !GameManager.Instance.GameStarted) return;

        if (!ghostTimerObj.activeSelf) ghostTimerObj.SetActive(true);

        _scaredTimer += Time.deltaTime;

        if (_scaredTimer >= _scaredLength - _recoveryLength)
        {
            _recoveryIntervalTimer += Time.deltaTime;

            if (_recoveryIntervalTimer >= recoveryInterval)
            {
                if (!_recoveryGhostAnimNormal)
                {
                    foreach (var controller in ghostControllers)
                    {
                        if (!controller.GhostIsDead) SetGhostAnimState(controller, GhostAnimState.Normal);
                    }
                    _recoveryGhostAnimNormal = true;
                }
                else
                {
                    foreach (var controller in ghostControllers)
                    {
                        if (!controller.GhostIsDead) SetGhostAnimState(controller, GhostAnimState.Scared);
                    }
                    _recoveryGhostAnimNormal = false;
                }
                _recoveryIntervalTimer = 0.0f;
            }
        }

        ghostTimerText.text = ((int)Mathf.Ceil(_scaredLength - _scaredTimer)).ToString();

        if (_scaredTimer >= _scaredLength)
        {
            GhostsAreScared = false;
            _scaredTimer = 0.0f;
            _recoveryIntervalTimer = 0.0f;

            foreach (var controller in ghostControllers)
            {
                if (!controller.GhostIsDead) SetGhostAnimState(controller, GhostAnimState.Normal);
            }

            ghostTimerObj.SetActive(false);
        }
    }

    public void TriggerScared(float scaredLength, float recoveryLength)
    {
        _scaredTimer = 0.0f;
        _scaredLength = scaredLength;
        _recoveryLength = recoveryLength;
        GhostsAreScared = true;

        foreach (var controller in ghostControllers)
        {
            SetGhostAnimState(controller, GhostAnimState.Scared);
        }

        GameManager.Instance.GameAudioManager.PlayScaredGhostMusic(scaredLength);
    }

    public void SetGhostAnimState(GhostController controller, GhostAnimState state)
    {
        if (state == GhostAnimState.Scared)
        {
            switch (controller.CurrentAnimation)
            {
                case "NormalForward":
                    controller.TriggerGhostAnimation("ScaredForward");
                    break;
                case "NormalBackward":
                    controller.TriggerGhostAnimation("ScaredBackward");
                    break;
                case "NormalLeft":
                    controller.TriggerGhostAnimation("ScaredLeft");
                    break;
                case "NormalRight":
                    controller.TriggerGhostAnimation("ScaredRight");
                    break;
                default:
                    controller.TriggerGhostAnimation("ScaredForward");
                    break;
            }
        } else if (state == GhostAnimState.Normal)
        {
            switch (controller.CurrentAnimation)
            {
                case "ScaredForward":
                    controller.TriggerGhostAnimation("NormalForward");
                    break;
                case "ScaredBackward":
                    controller.TriggerGhostAnimation("NormalBackward");
                    break;
                case "ScaredLeft":
                    controller.TriggerGhostAnimation("NormalLeft");
                    break;
                case "ScaredRight":
                    controller.TriggerGhostAnimation("NormalRight");
                    break;
                default:
                    controller.TriggerGhostAnimation("NormalForward");
                    break;
            }
        }
    }
}
