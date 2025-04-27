using Game;
using UnityEngine;
using Zenject;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanel : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                    _canvasGroup = GetComponent<CanvasGroup>();
                return _canvasGroup;
            }
        }

        protected GameManager GameManager;
        protected SignalBus SignalBus;
        public bool IsInitialized { get; private set; }

        public void Initialize(SignalBus signalBus, GameManager gameManager)
        {
            this.GameManager = gameManager;
            this.SignalBus = signalBus;

            IsInitialized = true;
        }

        protected virtual void OnPanelShown()
        {
        }

        protected virtual void OnPanelHidden()
        {
        }

        public void ShowPanel()
        {
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.alpha = 1;
            OnPanelShown();
        }

        public void HidePanel()
        {
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.alpha = 0;
            OnPanelHidden();
        }
    }
}