using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip gameIntro;
    [SerializeField] private AudioClip ghostNormalState;
    [SerializeField] private AudioClip ghostScaredState;
    [SerializeField] private float introPlaySeconds;

    private AudioSource _audioSource;
    private bool _gameIntroPlaying = false;
    private float _introPlayTimer = 0.0f;
    private float _ghostScaredTimer = 0.0f;
    private bool _ghostsScared = false;
    private float _ghostScaredDuration;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _audioSource.clip = gameIntro;
        _audioSource.Play();
        _gameIntroPlaying = true;
    }

    private void Update()
    {
        if (_gameIntroPlaying) _introPlayTimer += Time.deltaTime;

        if ((!_audioSource.isPlaying || _introPlayTimer >= introPlaySeconds) && _gameIntroPlaying && !_ghostsScared)
        {
            _gameIntroPlaying = false;
            PlayNormalGameMusic();
        }

        if (_ghostsScared)
        {
            _ghostScaredTimer += Time.deltaTime;

            if (_ghostScaredTimer >= _ghostScaredDuration)
            {
                _ghostsScared = false;
                _ghostScaredTimer = 0.0f;
                PlayNormalGameMusic();
            }
        }
    }

    public void PlayScaredGhostMusic(float duration)
    {
        _ghostsScared = true;
        _ghostScaredTimer = 0.0f;
        _ghostScaredDuration = duration;
        _audioSource.clip = ghostScaredState;
        _audioSource.loop = true;
        _audioSource.Play();
    }

    private void PlayNormalGameMusic()
    {
        _audioSource.clip = ghostNormalState;
        _audioSource.loop = true;
        _audioSource.Play();
    }
}
