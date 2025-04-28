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
        [SerializeField] private FailPanel _failPanel;
        [SerializeField] private SuccessPanel _successPanel;

        private List<UIPanel> _allPanels;

        public void Initialize()
        {
            _allPanels = new List<UIPanel>
            {
                _menuPanel,
                _gameplayPanel,
                _failPanel,
                _successPanel
            };

            foreach (UIPanel panel in _allPanels)
                panel.Initialize(_signalBus, _gameManager);
            
            _signalBus.Subscribe<GameplayStateChangedSignal>(OnGameplayStateChanged);
        }

        private void OnGameplayStateChanged(GameplayStateChangedSignal args)
        {
            foreach (UIPanel panel in _allPanels)
                panel.HidePanel();

            if (args.CurrenyGameplayState == GameplayState.Menu)
            {
                _menuPanel.ShowPanel();
            }
            else if (args.CurrenyGameplayState == GameplayState.Game)
            {
                _gameplayPanel.ShowPanel();
            }
            else if (args.CurrenyGameplayState == GameplayState.Fail)
            {
                _failPanel.ShowPanel();
            }
            else if (args.CurrenyGameplayState == GameplayState.Win)
            {
                _successPanel.ShowPanel();
            }
        }
    }
}