using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.Pool;
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
        [SerializeField] private Transform _fallingPieceContainer;

        private SignalBus _signalBus;

        private IObjectPool<Platform> _platformPool;
        private IObjectPool<FallingPiece> _fallingPiecePool;

        private Platform _lastPlatform;
        private Platform _currentMovingPlatform;
        private Material[] _platformMaterials;

        private int _createdPlatformCount;

        private int _perfectTimingStreakCount;
        private int _successfullyPlacedPlatformCountInLevel;

        private int _targetPlatformCountForLevel;
        private bool _isLevelCompleted;

        private float _platformLength;
        private float _platformDepth;
        private float _tolerancePercent;
        private float _moveSpeed;
        private float _movementOverflowAmount;

        private List<Platform> _currentLevelPlatforms = new List<Platform>();

        private float CurrentPlatformWidth { get; set; }
        public bool IsInitialized { get; private set; }
        public bool IsPrepared { get; private set; }


        public void Initialize(SignalBus signalBus)
        {
            _signalBus = signalBus;
            _platformPool = new ObjectPool<Platform>(OnCreatePlatform, OnGetPlatform, OnReleasePlatform, OnDestroyPlatform);
            _fallingPiecePool = new ObjectPool<FallingPiece>(OnCreateFallingPiece, OnGetFallingPiece, OnReleaseFallingPiece, OnDestroyFallingPiece);

            _signalBus.Subscribe<PlacePlatformRequestSignal>(OnPlacePlatformRequest);
            _signalBus.Subscribe<PlayerFallSignal>(OnPlayerFall);
            _signalBus.Subscribe<ClearPlatformsSignal>(OnPlatformClear);

            GetDataFromConfig();
            SpawnInitialPlatform();

            IsInitialized = true;
        }

        public void PrepareForLevel()
        {
            _currentLevelPlatforms.Clear();
            CurrentPlatformWidth = _gameManager.GameConfigs.PlatformWidth;
            _successfullyPlacedPlatformCountInLevel = 0;
            _targetPlatformCountForLevel = _levelManager.CurrentLevelInstance.platformCount;
            _isLevelCompleted = false;
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

            if (_isLevelCompleted)
                return;

            PlacePlatform(_successfullyPlacedPlatformCountInLevel == 0);

            if (_isLevelCompleted)
                return;

            _lastPlatform = _currentMovingPlatform;
            _currentMovingPlatform = null;

            SpawnNewMovingPlatform();
        }

        private void SpawnInitialPlatform()
        {
            _lastPlatform = _platformPool.Get();
            _currentLevelPlatforms.Add(_lastPlatform);
            _lastPlatform.StopMovement();
        }

        private void PlacePlatform(bool isFirst = false)
        {
            float lastLeft = isFirst ? -(_gameManager.GameConfigs.PlatformWidth * 0.5f) : _lastPlatform.transform.position.x - (_lastPlatform.Width * 0.5f);
            float lastRight = isFirst ? (_gameManager.GameConfigs.PlatformWidth * 0.5f) : _lastPlatform.transform.position.x + (_lastPlatform.Width * 0.5f);

            float currentLeft = _currentMovingPlatform.transform.position.x - (_currentMovingPlatform.Width * 0.5f);
            float currentRight = _currentMovingPlatform.transform.position.x + (_currentMovingPlatform.Width * 0.5f);

            float overlapLeft = Mathf.Max(lastLeft, currentLeft);
            float overlapRight = Mathf.Min(lastRight, currentRight);

            if (overlapRight > overlapLeft)
            {
                float offset = Mathf.Abs((isFirst ? 0f : _lastPlatform.transform.position.x) - _currentMovingPlatform.transform.position.x);
                float tolerance = CurrentPlatformWidth * _tolerancePercent;

                if (offset <= tolerance)
                {
                    PerfectTiming(isFirst);
                }
                else
                {
                    float newWidth = overlapRight - overlapLeft;
                    CurrentPlatformWidth = newWidth;

                    float newCenterX = (overlapLeft + overlapRight) * 0.5f;

                    _signalBus.Fire(new PlatformCenterChangedSignal
                    {
                        NewCenterX = newCenterX
                    });

                    _currentMovingPlatform.transform.position = new Vector3(
                        newCenterX,
                        _currentMovingPlatform.transform.position.y,
                        _currentMovingPlatform.transform.position.z);

                    _currentMovingPlatform.transform.localScale = new Vector3(CurrentPlatformWidth, _platformDepth, _platformLength);

                    CreateFallingPiece(currentLeft, currentRight, overlapLeft, overlapRight);
                }

                OnPlatformPlacedSuccessfully();
            }
            else
            {
                MissWholePlatform();
            }
        }

        private void OnPlatformPlacedSuccessfully()
        {
            _successfullyPlacedPlatformCountInLevel++;
            if (_successfullyPlacedPlatformCountInLevel == 1)
            {
                _signalBus.Fire(new FirstPlatformPlacedInLevelSignal());
            }

            CheckIsLevelFinished();
        }

        private void CheckIsLevelFinished()
        {
            if (_successfullyPlacedPlatformCountInLevel >= _targetPlatformCountForLevel)
            {
                _perfectTimingStreakCount = 0;
                _isLevelCompleted = true;
                _signalBus.Fire(new LevelFinishSuccessSignal());
            }
        }

        private void MissWholePlatform()
        {
            _isLevelCompleted = true;
            _currentMovingPlatform.gameObject.SetActive(false);
            CreateFallingBlock(_currentMovingPlatform.transform.position.x,_currentMovingPlatform.transform.localScale.x);
        }

        private void PerfectTimingStreakBroken()
        {
            _perfectTimingStreakCount = 0;
            _signalBus.Fire(new PlatformPlacedSignal
            {
                PlacedPlatform = _currentMovingPlatform,
                StreakCount = _perfectTimingStreakCount,
            });
        }

        private void PerfectTiming(bool isFirst)
        {
            CurrentPlatformWidth = isFirst ? CurrentPlatformWidth : _lastPlatform.Width;

            Vector3 currentPlatformPos = _currentMovingPlatform.transform.position;
            currentPlatformPos.x = isFirst ? 0f : _lastPlatform.transform.position.x;

            _currentMovingPlatform.transform.position = new Vector3(
                isFirst ? 0f : _lastPlatform.transform.position.x,
                _currentMovingPlatform.transform.position.y,
                currentPlatformPos.z);

            _perfectTimingStreakCount++;
            _signalBus.Fire(new PlatformPlacedSignal
            {
                PlacedPlatform = _currentMovingPlatform,
                StreakCount = _perfectTimingStreakCount,
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

            PerfectTimingStreakBroken();
        }

        private void CreateFallingBlock(float centerX, float width)
        {
            FallingPiece fallingBlock = _fallingPiecePool.Get();

            Vector3 fallingPiecePos = new Vector3(centerX, -(_platformDepth * 0.5f), _lastPlatform.transform.position.z + _platformLength);
            Vector3 fallingPieceScale = new Vector3(width, _platformDepth, _platformLength);
            fallingBlock.Prepare(fallingPiecePos, fallingPieceScale, _platformMaterials[(_createdPlatformCount - 1) & _platformMaterials.Length]);
        }

        private void SpawnNewMovingPlatform()
        {
            Platform platform = _platformPool.Get();
            platform.Initialize();
            platform.StartMovement(_moveSpeed, _movementOverflowAmount + _gameManager.GameConfigs.PlatformWidth);
            _currentMovingPlatform = platform;

            _currentLevelPlatforms.Add(_currentMovingPlatform);
        }

        private float CalculateSpawnDistance()
        {
            if (_createdPlatformCount <= 0)
                return _platformLength * 0.5f;

            float finishPlatformOffset = (_levelManager.CompletedLevelsInSession * _levelManager.CurrentLevelInstance.GetFinishPlatformLength());
            float totalDistanceMinusHalfLength = (_createdPlatformCount * _platformLength) + (_platformLength * 0.5f);

            return totalDistanceMinusHalfLength + finishPlatformOffset;
        }

        private void OnPlatformClear(ClearPlatformsSignal args)
        {
            ReleasePlatformToPool(args.Count);
        }

        private void ReleasePlatformToPool(int count)
        {
            int platformCount = _platformContainer.transform.childCount;

            if (count >= platformCount)
                return;

            int index = 0;

            for (int i = platformCount - 1; i >= 0; i--)
            {
                Platform platformToRelease = _platformContainer.transform.GetChild(i).gameObject.GetComponent<Platform>();

                if (platformToRelease.gameObject.activeInHierarchy)
                {
                    platformToRelease.ReleaseFromPool();
                }

                index++;

                if (index == count)
                    break;
            }
        }

        private void OnPlayerFall(PlayerFallSignal args)
        {
            Debug.Log("PlayerFall");
            _perfectTimingStreakCount = 0;
            _isLevelCompleted = true;
            _createdPlatformCount -= _currentLevelPlatforms.Count;

            Debug.Log(_createdPlatformCount);
            _signalBus.Fire(new LevelCompletelyFailed());

            foreach (var platform in _currentLevelPlatforms)
            {
                platform.ReleaseFromPool();
            }

            _currentLevelPlatforms.Clear();
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

            platform.transform.SetSiblingIndex(0);

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
            if (platform != null && platform.gameObject != null)
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
            var fallingPiece = Instantiate(_fallingPiecePrefab, _fallingPieceContainer);
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
    public Platform PlacedPlatform;
    public int StreakCount;
}

public struct LevelFinishSuccessSignal
{
}

public struct LevelFailedAndWaitingPlayerToFall
{
}

public struct LevelCompletelyFailed
{
}

public struct FirstPlatformPlacedInLevelSignal
{
}

public struct PlatformCenterChangedSignal
{
    public float NewCenterX;
}