using Game;
using UnityEngine;
using Zenject;

namespace Game
{
    public class GameManager : MonoBehaviour, IInitializable
    {
        [Inject] private DiContainer _diContainer;
        public static GameManager Instance;

        [Inject] SignalBus _signalBus;
        public GameConfigs GameConfigs;

        [Header("References")]
        [SerializeField] private GameplayController _gameplayController;
        [SerializeField] private LevelManager _levelManager;
        [SerializeField] private MainPlayerController _mainPlayerControllerPrefab;
        [SerializeField] private CameraController _cameraController;

        public void Initialize()
        {
            Debug.Assert(Instance == null);
            Instance = this;

            _levelManager.Initialize(_signalBus);
            _gameplayController.Initialize(_signalBus);
            
            _signalBus.Subscribe<PlayerReachFinalPlatformSignal>(OnPlayerReachFinalPlatform);
            _signalBus.Subscribe<LevelCompletelyFailed>(OnLevelCompletelyFailed);

            CreateMainPlayer();
        }

        private void OnLevelCompletelyFailed()
        {
            _gameplayController.OnLevelFailed();
        }

        private void CreateMainPlayer()
        {
            MainPlayerController mainPlayerController = Instantiate(_mainPlayerControllerPrefab, transform);

            mainPlayerController.Initialize(this,_signalBus);
            _diContainer.Inject(mainPlayerController);
            _cameraController.Initialize(_signalBus);

            _signalBus.Fire(new MainPlayerCreatedSignal
            {
                MainPlayerController = mainPlayerController
            });
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
                else if (_gameplayController.CurrentGameplayState == GameplayState.Menu)
                {
                    PrepareGameplay();
                }
                else 
                {
                    _gameplayController.SetGameplayState(GameplayState.Menu);
                }
            }
        }

        private void OnPlayerReachFinalPlatform()
        {
            _gameplayController.OnLevelCompleted();
        }
    }
}

public struct LevelStartedSignal
{
}

public struct MainPlayerCreatedSignal
{
    public MainPlayerController MainPlayerController;
}