using TMPro;
using UnityEngine;

public class GhostManager : MonoBehaviour
{
    public bool GhostsAreScared { get; private set; } = false;
    public bool PlayerIsDead { get; private set; } = false;

    [SerializeField] private float recoveryInterval;
    [SerializeField] private GameObject ghostTimerObj;
    [SerializeField] private TMP_Text ghostTimerText;

    private float _scaredTimer = 0.0f;
    private float _recoveryIntervalTimer = 0.0f;
    private bool _recoveryGhostAnimNormal = false;
    private float _scaredLength;
    private float _recoveryLength;
    private float _playerDeadTimer = 0.0f;
    private float _ghostPauseLength;

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

        if (PlayerIsDead)
        {
            _playerDeadTimer += Time.deltaTime;

            if (_playerDeadTimer > _ghostPauseLength)
            {
                PlayerIsDead = false;

                foreach (var controller in ghostControllers)
                {
                    controller.TriggerGhostAnimation("NormalForward");
                    controller.ResetGhostToStart();
                }
            }
        }
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
            if (!controller.GhostIsDead) SetGhostAnimState(controller, GhostAnimState.Scared);
        }

        GameAudioManager gameAudioManager = GameManager.Instance.GameAudioManager;
        gameAudioManager.PlayScaredGhostMusic(scaredLength);
    }

    public void PlayerDeathEvent(float ghostPauseLength)
    {
        PlayerIsDead = true;
        _ghostPauseLength = ghostPauseLength;

        foreach (var controller in ghostControllers)
        {
            controller.TriggerGhostAnimation("NormalForward");
        }
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
                case "ScaredForward":
                    controller.TriggerGhostAnimation("ScaredForward");
                    break;
                case "ScaredBackward":
                    controller.TriggerGhostAnimation("ScaredBackward");
                    break;
                case "ScaredLeft":
                    controller.TriggerGhostAnimation("ScaredLeft");
                    break;
                case "ScaredRight":
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
                case "NormalForward":
                    controller.TriggerGhostAnimation("NormalForward");
                    break;
                case "NormalBackward":
                    controller.TriggerGhostAnimation("NormalBackward");
                    break;
                case "NormalLeft":
                    controller.TriggerGhostAnimation("NormalLeft");
                    break;
                case "NormalRight":
                    controller.TriggerGhostAnimation("NormalRight");
                    break;
                default:
                    controller.TriggerGhostAnimation("NormalForward");
                    break;
            }
        }
    }

    public bool SeeIfAnyGhostIsDead()
    {
        foreach (var controller in ghostControllers)
        {
            if (controller.GhostIsDead) return true;
        }

        return false;
    }
}
