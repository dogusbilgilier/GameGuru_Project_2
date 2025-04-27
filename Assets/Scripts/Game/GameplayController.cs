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

        public void Initialize(SignalBus signalBus)
        {
            _signalBus = signalBus;
            _platformManager.Initialize(_signalBus);
            _soundManager.Initialize(_signalBus);
        }
    }
}