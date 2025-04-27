using UnityEngine;
using Zenject;

public class GameplayController : MonoBehaviour
{
    private SignalBus _signalBus;

    public void Initialize(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }
}