using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game
{
    public class LevelManager : MonoBehaviour
    {
        [Inject] DiContainer _diContainer;
        [Header("Levels")]
        public Level[] _levelPrefabs;

        public Level CurrentLevelInstance { get; private set; }

        private SignalBus _signalBus;

        private int _completedLevelsInSession = 0;
        public int CompletedLevelsInSession => _completedLevelsInSession;

        private List<Platform> _platformsToRelease = new List<Platform>();
        public int LinearLevelIndex
        {
            get => PlayerPrefs.GetInt("LinearLevelIndex", 0);
            set => PlayerPrefs.SetInt("LinearLevelIndex", value);
        }

        public void Initialize(SignalBus signalBus)
        {
            _signalBus = signalBus;
            _signalBus.Subscribe<LevelFinishSuccessSignal>(OnLevelFinished);
            _signalBus.Subscribe<LevelCompletelyFailed>(OnLevelCompletelyFailed);
        }


        private void OnLevelFinished(LevelFinishSuccessSignal args)
        {
            _completedLevelsInSession++;
            LinearLevelIndex++;

            ReleasablePlatformCountsInTheBack();
        }

        private void OnLevelCompletelyFailed()
        {
            UnloadLevel();
        }

        private void UnloadLevel()
        {
            Destroy(CurrentLevelInstance.gameObject);
        }

        private void ReleasablePlatformCountsInTheBack()
        {
            if (_completedLevelsInSession > 2)
            {
                int platformCountToRelease = _levelPrefabs[(LinearLevelIndex - 2) % _levelPrefabs.Length].platformCount;
                _signalBus.Fire<ClearPlatformsSignal>(new ClearPlatformsSignal
                {
                    Count = platformCountToRelease
                });
            }
        }

        public void PrepareLevel()
        {
            int index = LinearLevelIndex % _levelPrefabs.Length;
            CurrentLevelInstance = Instantiate(_levelPrefabs[index], transform);
            _diContainer.Inject(CurrentLevelInstance);

            CurrentLevelInstance.transform.position = Vector3.forward * GetLevelsStartDistance(_completedLevelsInSession);
            CurrentLevelInstance.Initialize();
        }

        public float GetLevelsStartDistance(int levelIndex)
        {
            if (levelIndex == 0)
                return 0;

            float levelZPos = 0;

            for (int i = 0; i < levelIndex; i++)
            {
                levelZPos += _levelPrefabs[i % _levelPrefabs.Length].GetLevelLength();
            }

            return levelZPos;
        }

        public float GetFinishPlatformLength() => CurrentLevelInstance.GetFinishPlatformLength();

        public float GetLastLevelsStartDistance()
        {
            return GetLevelsStartDistance(_completedLevelsInSession);
        }
    }
}

public struct ClearPlatformsSignal
{
    public int Count;
}