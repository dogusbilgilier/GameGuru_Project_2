using DefaultNamespace;
using Game;
using Game.GameElements;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using Zenject;

namespace Game
{
    public class PlatformManager : MonoBehaviour
    {
        [Inject] private DiContainer _container;
        [Inject] private LevelManager _levelManager;
        [Inject] private GameManager _gameManager;

        [Header("References")]
        [SerializeField] private Platform _platformPrefab;
        [SerializeField] private FallingPiece _fallingPiecePrefab;
        [SerializeField] private Transform _platformContainer;

        private SignalBus _signalBus;

        private IObjectPool<Platform> _platformPool;
        private IObjectPool<FallingPiece> _fallingPiecePool;

        private Platform _lastPlatform;
        private Platform _currentMovingPlatform;
        private Material[] _platformMaterials;

        private int _createdPlatformCount;

        private int _perfectTimingStreakCount;
        private int _successfullyPlacedPlatformCount;

        private int _targetPlatformCountForLevel;
        private bool _isLevelCompleted;


        private float _platformLength;
        private float _platformDepth;
        private float _tolerancePercent;
        private float _moveSpeed;
        private float _movementOverflowAmount;

        private bool _isLevelFailed;
        private float CurrentPlatformWidth { get; set; }
        public bool IsInitialized { get; private set; }
        public bool IsPrepared { get; private set; }


        public void Initialize(SignalBus signalBus)
        {
            _signalBus = signalBus;
            _platformPool = new ObjectPool<Platform>(OnCreatePlatform, OnGetPlatform, OnReleasePlatform, OnDestroyPlatform);
            _fallingPiecePool = new ObjectPool<FallingPiece>(OnCreateFallingPiece, OnGetFallingPiece, OnReleaseFallingPiece, OnDestroyFallingPiece);
            _signalBus.Subscribe<PlacePlatformRequestSignal>(OnPlacePlatformRequest);
            GetDataFromConfig();
            SpawnInitialPlatform();
            IsInitialized = true;
        }

        public void PrepareForLevel()
        {
            _successfullyPlacedPlatformCount = 0;
            _targetPlatformCountForLevel = _levelManager.CurrentLevelInstance.platformCount;
            _isLevelCompleted = false;
            _isLevelFailed = false;
            SpawnNewMovingPlatform();

            IsPrepared = true;
        }

        private void GetDataFromConfig()
        {
            GameConfigs gameConfigs = _gameManager.GameConfigs;

            _platformLength = gameConfigs.PlatformLength;
            CurrentPlatformWidth = gameConfigs.PlatformWidth;
            _platformDepth = gameConfigs.PlatformDepth;
            _platformMaterials = gameConfigs.PlatformMaterials;
            _tolerancePercent = gameConfigs.TolerancePercent;
            _moveSpeed = gameConfigs.MoveSpeed;
            _movementOverflowAmount = gameConfigs.MovementOverflowAmount;
        }

        private void OnPlacePlatformRequest(PlacePlatformRequestSignal args)
        {
            _currentMovingPlatform.StopMovement();

            PlacePlatform();

            if (_isLevelCompleted || _isLevelFailed)
                return;

            _lastPlatform = _currentMovingPlatform;
            _currentMovingPlatform = null;

            SpawnNewMovingPlatform();
        }

        private void SpawnInitialPlatform()
        {
            _lastPlatform = _platformPool.Get();
            _lastPlatform.StopMovement();
        }

        private void PlacePlatform()
        {
            float lastLeft = _lastPlatform.transform.position.x - (_lastPlatform.Width * 0.5f);
            float lastRight = _lastPlatform.transform.position.x + (_lastPlatform.Width * 0.5f);

            float currentLeft = _currentMovingPlatform.transform.position.x - (_currentMovingPlatform.Width * 0.5f);
            float currentRight = _currentMovingPlatform.transform.position.x + (_currentMovingPlatform.Width * 0.5f);

            float overlapLeft = Mathf.Max(lastLeft, currentLeft);
            float overlapRight = Mathf.Min(lastRight, currentRight);

            if (overlapRight > overlapLeft)
            {
                _successfullyPlacedPlatformCount++;
                float offset = Mathf.Abs(_lastPlatform.transform.position.x - _currentMovingPlatform.transform.position.x);
                float tolerance = CurrentPlatformWidth * _tolerancePercent;

                if (offset <= tolerance)
                {
                    PerfectTiming();
                }
                else
                {
                    CreateFallingPiece(currentLeft, currentRight, overlapLeft, overlapRight);
                }

                CheckIsLevelFinished();
            }
            else
            {
                _isLevelCompleted = true;
                MissWholePlatform();
            }
        }

        private void CheckIsLevelFinished()
        {
            if (_successfullyPlacedPlatformCount >= _targetPlatformCountForLevel)
            {
                _isLevelCompleted = true;
                _signalBus.Fire(new LevelFinishedSignal
                {
                    IsSuccess = true
                });
            }
        }

        private void MissWholePlatform()
        {
            _perfectTimingStreakCount = 0;
            _signalBus.Fire<PlatformPlacedSignal>(new PlatformPlacedSignal
            {
                PlatformManager = this,
                StreakCount = _perfectTimingStreakCount,
                IsMissedCompletely = true,
                SuccessfullyPlacedPlatformCount = _successfullyPlacedPlatformCount
            });
            _signalBus.Fire(new LevelFinishedSignal
            {
                IsSuccess = false
            });
        }

        private void PerfectTimingStreakBroken()
        {
            _perfectTimingStreakCount = 0;
            _signalBus.Fire<PlatformPlacedSignal>(new PlatformPlacedSignal
            {
                PlatformManager = this,
                StreakCount = _perfectTimingStreakCount,
                IsMissedCompletely = false,
                SuccessfullyPlacedPlatformCount = _successfullyPlacedPlatformCount
            });
        }

        private void PerfectTiming()
        {
            CurrentPlatformWidth = _lastPlatform.Width;

            Vector3 currentPlatformPos = _currentMovingPlatform.transform.position;
            currentPlatformPos.x = _lastPlatform.transform.position.x;

            _currentMovingPlatform.transform.position = new Vector3(
                _lastPlatform.transform.position.x,
                _currentMovingPlatform.transform.position.y,
                currentPlatformPos.z);

            _perfectTimingStreakCount++;
            _signalBus.Fire<PlatformPlacedSignal>(new PlatformPlacedSignal
            {
                PlatformManager = this,
                StreakCount = _perfectTimingStreakCount,
                IsMissedCompletely = false,
                SuccessfullyPlacedPlatformCount = _successfullyPlacedPlatformCount
            });
        }

        private void CreateFallingPiece(float currentLeft, float currentRight, float overlapLeft, float overlapRight)
        {
            float newWidth = overlapRight - overlapLeft;
            CurrentPlatformWidth = newWidth;

            float newCenterX = (overlapLeft + overlapRight) * 0.5f;

            _currentMovingPlatform.transform.position = new Vector3(
                newCenterX,
                _currentMovingPlatform.transform.position.y,
                _currentMovingPlatform.transform.position.z);

            _currentMovingPlatform.transform.localScale = new Vector3(CurrentPlatformWidth, _platformDepth, _platformLength);

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

            PerfectTimingStreakBroken();
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
            platform.StartMovement(_moveSpeed, _movementOverflowAmount + _gameManager.GameConfigs.PlatformWidth);
            _currentMovingPlatform = platform;
        }

        private float CalculateSpawnDistance()
        {
            if (_createdPlatformCount <= 0)
                return _platformLength * 0.5f;

            float finishPlatformOffset = (_levelManager.CompletedLevelsInSession * _levelManager.CurrentLevelInstance.GetFinishPlatformLength());
            float totalDistanceMinusHalfLength = (_createdPlatformCount * _platformLength) + (_platformLength * 0.5f);
            
            return totalDistanceMinusHalfLength + finishPlatformOffset;
        }

        #region Platform Pool

        private void OnGetPlatform(Platform platform)
        {
            platform.gameObject.SetActive(true);
            platform.transform.localScale = new Vector3(CurrentPlatformWidth, _platformDepth, _platformLength);

            int materialIndex = (_createdPlatformCount) % _platformMaterials.Length;
            float zPos = CalculateSpawnDistance();
            Vector3 position = new Vector3(0f, -(_platformDepth * 0.5f), zPos);
            platform.Prepare(position, _platformMaterials[materialIndex]);

            _createdPlatformCount++;
        }

        private Platform OnCreatePlatform()
        {
            Platform platform = Instantiate(_platformPrefab, _platformContainer);
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
    public PlatformManager PlatformManager;
    public int StreakCount;
    public bool IsMissedCompletely;
    public int SuccessfullyPlacedPlatformCount;
}

public struct LevelFinishedSignal
{
    public bool IsSuccess;
}