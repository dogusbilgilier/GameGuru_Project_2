using UnityEngine;
using Zenject;

#region Game

public class SoundManager : MonoBehaviour
{
    private readonly int[] majorScaleSemitones = { 1, 2, 2, 1, 2, 2, 2 }; //starts from si and plays do major

    [SerializeField] private AudioClip[] _platformSoundClips;
    [SerializeField] AudioSource _audioSource;
    private SignalBus _signalBus;

    public void Initialize(SignalBus signalBus)
    {
        _signalBus = signalBus;
        _signalBus.Subscribe<PlatformPlacedSignal>(OnPlatformPlaced);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _audioSource.Play();
        }
    }

    private void OnPlatformPlaced(PlatformPlacedSignal args)
    {
        int clipIndex = (args.StreakCount > 0 ? args.StreakCount - 1 : 0) % _platformSoundClips.Length;
        _audioSource.clip = _platformSoundClips[clipIndex];

        if (args.StreakCount > _platformSoundClips.Length)
        {
            _audioSource.clip = _platformSoundClips[^1];

            int extraNotes = args.StreakCount - _platformSoundClips.Length;
            int noteIndex = extraNotes % majorScaleSemitones.Length;
            int octaveShift = extraNotes / majorScaleSemitones.Length;

            int totalSemitones = 0;

            for (int i = 0; i < noteIndex; i++)
            {
                totalSemitones += majorScaleSemitones[i];
            }

            totalSemitones += octaveShift * 12;

            _audioSource.pitch = Mathf.Pow(2f, totalSemitones / 12f);
        }
        else
        {
            _audioSource.pitch = 1.0f;
        }

        _audioSource.Play();
    }
}

#endregion