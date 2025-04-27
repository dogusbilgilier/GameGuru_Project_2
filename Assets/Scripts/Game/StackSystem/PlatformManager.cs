using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using Zenject;

namespace Game
{
    public class PlatformManager : MonoBehaviour
    {
        [Inject] private DiContainer _container;

        [Header("References")]
        [SerializeField] private Platform _platformPrefab;
        [SerializeField] private FallingPiece _fallingPiecePrefab;

        [SerializeField] private Transform _platformContainer;

        [Header("Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float _tolerancePercent;

        [Header("Settings - Platform")]
        [SerializeField] Material[] _platformMaterials;
        [SerializeField] private float _platformWidth;
        [SerializeField] private float _platformLength;
        [SerializeField] private float _platformDepth;

        [SerializeField] private float _movementOverflowAmount;
        [SerializeField] private float _moveSpeed;

        private SignalBus _signalBus;

        private IObjectPool<Platform> _platformPool;
        private IObjectPool<FallingPiece> _fallingPiecePool;

        private Platform _lastPlatform;
        private Platform _currentMovingPlatform;

        private int _createdPlatformCount = 0;
        private int _perfectTimingStreakCount = 0;

        private float CurrentPlatformWidth { get; set; }

        public void Initialize(SignalBus signalBus)
        {
            _signalBus = signalBus;
            _platformPool = new ObjectPool<Platform>(OnCreatePlatform, OnGetPlatform, OnReleasePlatform, OnDestroyPlatform);
            _fallingPiecePool = new ObjectPool<FallingPiece>(OnCreateFallingPiece, OnGetFallingPiece, OnReleaseFallingPiece, OnDestroyFallingPiece);

            CurrentPlatformWidth = _platformWidth;
            SpawnInitialPlatform();
            SpawnNewMovingPlatform();
        }

        private void SpawnInitialPlatform()
        {
            _lastPlatform = _platformPool.Get();
            _lastPlatform.StopMovement();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnScreenClicked();
            }
        }

        private void OnScreenClicked()
        {
            if (_currentMovingPlatform != null)
            {
                _currentMovingPlatform.StopMovement();

                CutPlatform();

                _lastPlatform = _currentMovingPlatform;
                _currentMovingPlatform = null;

                SpawnNewMovingPlatform();
            }
        }

        private void CutPlatform()
        {
            float lastLeft = _lastPlatform.transform.position.x - (_lastPlatform.Width * 0.5f);
            float lastRight = _lastPlatform.transform.position.x + (_lastPlatform.Width * 0.5f);

            float currentLeft = _currentMovingPlatform.transform.position.x - (_currentMovingPlatform.Width * 0.5f);
            float currentRight = _currentMovingPlatform.transform.position.x + (_currentMovingPlatform.Width * 0.5f);

            float overlapLeft = Mathf.Max(lastLeft, currentLeft);
            float overlapRight = Mathf.Min(lastRight, currentRight);

            if (overlapRight > overlapLeft)
            {
                float offset = Mathf.Abs(_lastPlatform.transform.position.x - _currentMovingPlatform.transform.position.x);

                float tolerance = CurrentPlatformWidth * _tolerancePercent;
                if (offset <= tolerance)
                {
                    CurrentPlatformWidth = _lastPlatform.Width;

                    Vector3 curentPlatformPos = _currentMovingPlatform.transform.position;
                    curentPlatformPos.x = _lastPlatform.transform.position.x;

                    _currentMovingPlatform.transform.position = new Vector3(
                        _lastPlatform.transform.position.x,
                        _currentMovingPlatform.transform.position.y,
                        curentPlatformPos.z);

                    PerfectTiming();
                }
                else
                {
                    float newWidth = overlapRight - overlapLeft;
                    CurrentPlatformWidth = newWidth;

                    float newCenterX = (overlapLeft + overlapRight) * 0.5f;

                    _currentMovingPlatform.transform.position = new Vector3(
                        newCenterX,
                        _currentMovingPlatform.transform.position.y,
                        _currentMovingPlatform.transform.position.z);

                    _currentMovingPlatform.transform.localScale = new Vector3(CurrentPlatformWidth, _platformDepth, _platformLength);

                    CreateFallingPiece(currentLeft, currentRight, overlapLeft, overlapRight);

                    PerfectTimingStreakBroken();
                }
            }
            else
            {
                MissWholePlatform();
                Debug.Log("No overlap!");
            }
        }

        private void MissWholePlatform()
        {
            _perfectTimingStreakCount = 0;
            _signalBus.Fire<PlatformPlacedSignal>(new PlatformPlacedSignal
            {
                StreakCount = _perfectTimingStreakCount,
                IsMissedCompletely = true
            });
        }

        private void PerfectTimingStreakBroken()
        {
            _perfectTimingStreakCount = 0;
            _signalBus.Fire<PlatformPlacedSignal>(new PlatformPlacedSignal
            {
                StreakCount = _perfectTimingStreakCount,
                IsMissedCompletely = false
            });
        }

        private void PerfectTiming()
        {
            _perfectTimingStreakCount++;
            _signalBus.Fire<PlatformPlacedSignal>(new PlatformPlacedSignal
            {
                StreakCount = _perfectTimingStreakCount,
                IsMissedCompletely = false
            });
        }

        private void CreateFallingPiece(float currentLeft, float currentRight, float overlapLeft, float overlapRight)
        {
            if (currentLeft < overlapLeft)
            {
                float fallWidth = overlapLeft - currentLeft;
                float centerX = (currentLeft + overlapLeft) / 2f;

                CreateFallingBlock(centerX, fallWidth);
            }

            if (currentRight > overlapRight)
            {
                float fallWidth = currentRight - overlapRight;
                float centerX = (currentRight + overlapRight) / 2f;

                CreateFallingBlock(centerX, fallWidth);
            }
        }

        private void CreateFallingBlock(float centerX, float width)
        {
            FallingPiece fallingBlock = _fallingPiecePool.Get();

            Vector3 fallingPiecePos = new Vector3(centerX, -(_platformDepth * 0.5f), _lastPlatform.transform.position.z + _platformLength);
            Vector3 fallingPieceScale = new Vector3(width, _platformDepth, _platformLength);
            fallingBlock.Prepare(fallingPiecePos, fallingPieceScale, _platformMaterials[_createdPlatformCount - 1]);
        }


        private void SpawnNewMovingPlatform()
        {
            Platform platform = _platformPool.Get();
            platform.Initialize();
            platform.StartMovement(_moveSpeed, _movementOverflowAmount + _platformWidth);
            _currentMovingPlatform = platform;
        }

        private float CalculateSpawnDistance()
        {
            return (_createdPlatformCount * _platformLength) + (_platformLength * 0.5f);
        }


        #region Platform Pool

        private void OnGetPlatform(Platform platform)
        {
            platform.gameObject.SetActive(true);
            int materialIndex = (_createdPlatformCount) % _platformMaterials.Length;
            platform.Prepare(new Vector3(0f, -(_platformDepth * 0.5f), CalculateSpawnDistance()), _platformMaterials[materialIndex]);
            _createdPlatformCount++;
        }

        private Platform OnCreatePlatform()
        {
            Platform platform = Instantiate(_platformPrefab, _platformContainer);
            platform.transform.localScale = new Vector3(CurrentPlatformWidth, _platformDepth, _platformLength);
            platform.AssignToPool(_platformPool);

            return platform;
        }

        private void OnReleasePlatform(Platform platform)
        {
            platform.gameObject.SetActive(false);
        }

        private void OnDestroyPlatform(Platform platform)
        {
            platform.ReleaseFromPool();
        }

        #endregion

        #region Falling Piece Pool

        private FallingPiece OnCreateFallingPiece()
        {
            var fallingPiece = Instantiate(_fallingPiecePrefab, _platformContainer);
            fallingPiece.AssignToPool(_fallingPiecePool);
            return fallingPiece;
        }

        private void OnGetFallingPiece(FallingPiece fallingPiece)
        {
            fallingPiece.gameObject.SetActive(true);
        }

        private void OnReleaseFallingPiece(FallingPiece fallingPiece)
        {
            fallingPiece.gameObject.SetActive(false);
        }

        private void OnDestroyFallingPiece(FallingPiece fallingPiece)
        {
            fallingPiece.ReleaseFromPool();
        }

        #endregion
    }
}

public struct PlatformPlacedSignal
{
    public int StreakCount;
    public bool IsMissedCompletely;
}