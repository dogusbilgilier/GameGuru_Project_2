using DefaultNamespace;
using UnityEngine;
using Zenject;

namespace Game
{
    public class GameManager : MonoBehaviour, IInitializable
    {
        public static GameManager Instance;
        [Inject] SignalBus _signalBus;
        public GameConfigs GameConfigs;

        [Header("References")]
        [SerializeField] private GameplayController _gameplayController;
        [SerializeField] private LevelManager _levelManager;

        public GameplayController GameplayController => _gameplayController;
        public LevelManager LevelManager => _levelManager;

        public void Initialize()
        {
            Debug.Assert(Instance == null);
            Instance = this;

            _levelManager.Initialize(_signalBus);
            _gameplayController.Initialize(_signalBus);

            _signalBus.Subscribe<LevelFinishedSignal>(OnLevelFinished);
        }

        private void PrepareGameplay()
        {
            _levelManager.PrepareLevel();
            _gameplayController.PrepareGameplay();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (_gameplayController.CurrentGameplayState == GameplayState.Game)
                {
                    _signalBus.Fire(new PlacePlatformRequestSignal());
                }
                else
                {
                    PrepareGameplay();
                }
            }
        }

        private void OnLevelFinished(LevelFinishedSignal args)
        {
            if (args.IsSuccess)
            {
                _gameplayController.OnLevelCompleted();
            }
            else
            {
                _gameplayController.OnLevelFailed();
            }
        }
    }
}