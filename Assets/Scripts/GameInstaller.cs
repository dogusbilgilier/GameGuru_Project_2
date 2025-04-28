using Game;
using UI;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [Header("References")]
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private UIManager _uiManager;

    /// <summary>
    /// Registers all necessary services and signals for the game.
    /// Called automatically by Zenject during scene initialization.
    /// </summary>
    public override void InstallBindings()
    {
        SignalBusInstaller.Install(Container);

        Container.BindInterfacesAndSelfTo<UIManager>().FromInstance(_uiManager).AsSingle();
        Container.BindInterfacesAndSelfTo<GameManager>().FromInstance(_gameManager).AsSingle();
        Container.Bind<LevelManager>().FromComponentInHierarchy().AsSingle();

        Container.DeclareSignal<PlatformPlacedSignal>();
        Container.DeclareSignal<GameplayStateChangedSignal>();
        Container.DeclareSignal<PlacePlatformRequestSignal>();
        Container.DeclareSignal<MainPlayerCreatedSignal>();
        
        Container.DeclareSignal<LevelFinishSuccessSignal>();
        Container.DeclareSignal<PlayerReachFinalPlatformSignal>();

        Container.DeclareSignal<FirstPlatformPlacedInLevelSignal>();
        Container.DeclareSignal<PlatformCenterChangedSignal>();
        Container.DeclareSignal<PlayerFallSignal>();
        Container.DeclareSignal<LevelFailedAndWaitingPlayerToFall>();
        
        
        Container.DeclareSignal<ClearPlatformsSignal>();
        Container.DeclareSignal<LevelCompletelyFailed>();
    }
}