using UnityEngine;
using Zenject;

namespace DefaultNamespace
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
        public int LinearLevelIndex
        {
            get => PlayerPrefs.GetInt("LinearLevelIndex", 0);
            set => PlayerPrefs.SetInt("LinearLevelIndex", value);
        }

        public void Initialize(SignalBus signalBus)
        {
            _signalBus = signalBus;
            _signalBus.Subscribe<LevelFinishedSignal>(OnLevelFinished);
        }

        private void OnLevelFinished(LevelFinishedSignal args)
        {
            if (args.IsSuccess)
            {
                Debug.Log("");
                _completedLevelsInSession++;
                LinearLevelIndex++;
            }
            else
            {
            }
        }


        public void PrepareLevel()
        {
            Debug.Log(_completedLevelsInSession);
            int index = LinearLevelIndex % _levelPrefabs.Length;
            float levelZPos = 0;
            
            for (int i = 0; i < _completedLevelsInSession; i++)
            {
                levelZPos += _levelPrefabs[i % _levelPrefabs.Length].GetLevelLength();
            }

            Debug.Log(levelZPos);
            CurrentLevelInstance = Instantiate(_levelPrefabs[index], transform);
            _diContainer.Inject(CurrentLevelInstance);
            CurrentLevelInstance.transform.position = Vector3.forward * levelZPos;
            CurrentLevelInstance.Initialize();
        }
    }
}