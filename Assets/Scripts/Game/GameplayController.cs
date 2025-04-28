using System;
using UnityEngine;
using Zenject;

namespace Game
{
    public class GameplayController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlatformManager _platformManager;
        [SerializeField] private SoundManager _soundManager;


        private SignalBus _signalBus;
        private GameplayState _currentGameplayState = GameplayState.None;
        public GameplayState CurrentGameplayState => _currentGameplayState;
        public bool IsInGameplayMode => _currentGameplayState == GameplayState.Game;

        public void Initialize(SignalBus signalBus)
        {
            _signalBus = signalBus;
            _platformManager.Initialize(_signalBus);
            _soundManager.Initialize(_signalBus);


            SetGameplayState(GameplayState.Initial);
        }

        public void PrepareGameplay()
        {
            SetGameplayState(GameplayState.Game);
            _platformManager.PrepareForLevel();
        }

        private void SetGameplayState(GameplayState state)
        {
            _signalBus.Fire(new GameplayStateChangedSignal
            {
                PreviousGameplayState = _currentGameplayState,
                CurrenyGameplayState = state
            });
            _currentGameplayState = state;
        }

        public void OnLevelCompleted()
        {
            SetGameplayState(GameplayState.Win);
        }

        public void OnLevelFailed()
        {
            SetGameplayState(GameplayState.Fail);
        }
    }
}

public enum GameplayState
{
    None,
    Initial,
    Game,
    Fail,
    Win
}

public struct GameplayStateChangedSignal
{
    public GameplayState CurrenyGameplayState;
    public GameplayState PreviousGameplayState;
}

public struct PlacePlatformRequestSignal
{
}