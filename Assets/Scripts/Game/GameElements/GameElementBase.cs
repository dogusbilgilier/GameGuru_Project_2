using DG.Tweening;
using UnityEngine;

namespace Game.GameElements
{
    [RequireComponent(typeof(BoxCollider))]
    public class GameElementBase : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ParticleSystem _particles;
        [SerializeField] private GameObject _objectModel;

        private BoxCollider _boxCollider;

        public BoxCollider BoxCollider
        {
            get
            {
                if (_boxCollider == null)
                    _boxCollider = GetComponent<BoxCollider>();
                return _boxCollider;
            }
        }


        private void Awake()
        {
            DoTurn(20f, 1f);
        }

        public virtual void OnCollected()
        {
            _particles.Play();
            _objectModel.gameObject.SetActive(false);
            BoxCollider.enabled = false;
        }

        protected void DoHover(float yValue, float duration)
        {
            transform.DOMoveY(yValue, duration).SetRelative(true).SetLoops(-1, LoopType.Yoyo).SetLink(this.gameObject).SetAutoKill(true);
        }

        protected void DoTurn(float turnAmount, float duration)
        {
            transform.DORotate(Vector3.up * turnAmount, duration).SetRelative(true).SetLoops(-1, LoopType.Incremental).SetLink(this.gameObject).SetAutoKill(true).SetEase(Ease.Linear);
        }
    }
}