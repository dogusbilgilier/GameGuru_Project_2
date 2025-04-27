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
        
    }
}
