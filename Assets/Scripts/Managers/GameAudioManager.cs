using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip gameIntro;
    [SerializeField] private AudioClip ghostNormalState;
    [SerializeField] private float introPlaySeconds;

    private AudioSource _audioSource;
    private bool _gameIntroPlaying = false;
    private float _introPlayTimer = 0.0f;

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

        if ((!_audioSource.isPlaying || _introPlayTimer >= introPlaySeconds) && _gameIntroPlaying)
        {
            _gameIntroPlaying = false;
            _audioSource.clip = ghostNormalState;
            _audioSource.loop = true;
            _audioSource.Play();
        }
    }
}
