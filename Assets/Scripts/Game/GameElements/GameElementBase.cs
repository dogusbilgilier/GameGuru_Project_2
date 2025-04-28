using DG.Tweening;
using UnityEngine;

namespace Game.GameElements
{
    public class GameElementBase : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ParticleSystem _particles;


        public void OnCollected()
        {
            _particles.Play();
        }

        protected void DoHover()
        {
            transform.DOMoveY(2f, 1f).SetRelative(true).SetLoops(-1, LoopType.Yoyo).SetLink(this.gameObject).SetAutoKill(true);
        }

        protected void DoTurn()
        {
            transform.DORotate(Vector3.up, 1f).SetRelative(true).SetLoops(-1, LoopType.Incremental).SetLink(this.gameObject).SetAutoKill(true);
        }
    }
}