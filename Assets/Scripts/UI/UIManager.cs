using System.Collections.Generic;
using Game;
using UnityEngine;
using Zenject;

namespace UI
{
    public class UIManager : MonoBehaviour, IInitializable
    {
        [Inject] private GameManager _gameManager;
        [Inject] private SignalBus _signalBus;

        [Header("References")]
        [SerializeField] private MenuPanel _menuPanel;
        [SerializeField] private GameplayPanel _gameplayPanel;

        private List<UIPanel> _allPanels;

        public void Initialize()
        {
            _allPanels = new List<UIPanel> { _menuPanel, _gameplayPanel };
            
            foreach (UIPanel panel in _allPanels)
                panel.Initialize(_signalBus, _gameManager);
        }
    }
}