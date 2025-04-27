using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour, IInitializable
{
    [Inject] SignalBus _signalBus;
    
    [Header("References")]
    [SerializeField] private GameplayController _gameplayController;
    public GameplayController GameplayController => _gameplayController;
    
    public void Initialize()
    {
        _gameplayController.Initialize(_signalBus);
    }
}