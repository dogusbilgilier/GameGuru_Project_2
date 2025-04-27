using UnityEngine;
using Zenject;

public class UIManager : MonoBehaviour, IInitializable
{
    [Inject] private GameManager _gameManager;
    [Inject] SignalBus _signalBus;

    public void Initialize()
    {
    }
}