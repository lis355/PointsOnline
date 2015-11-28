using System.Collections.Generic;

namespace PointsOnline
{
    class PointsSaveDataManager
    {
        public static PointsSaveDataManager Instance { get; private set; }

        static PointsSaveDataManager()
        {
            Instance = new PointsSaveDataManager();
        }

        SaveDataManager<GameSaveData> _manager;
        GameSaveData _data;
        //bool _dataIsDirty;

        public GameSaveData Data
        {
            get
            {
                //_dataIsDirty = true;
                return _data;
            }
            set
            {
                //_dataIsDirty = true;
                _data = value;
            }
        }

        private PointsSaveDataManager()
        {
            _manager = new SaveDataManager<GameSaveData>();

            //_dataIsDirty = true;
        }

        public void Initialize()
        {
            Data = _manager.Load();

            if (Data == null
                || Data.BluePlayer == null
                || Data.RedPlayer == null )
            {
                InitNewGame();
            }
        }

        public void Save()
        {
            _manager.Save(_data);
        }

        public void InitNewGame()
        {
            Data = new GameSaveData();

            Data.IsGameStarted = false;
            Data.CurrentMovingPlayer = GameSaveData.PlayerSide.Blue;

            Data.BluePlayer = new GameSaveData.PlayerStats();
            Data.BluePlayer.TimeInSeconds = 0;
            Data.BluePlayer.Score = 0;
            Data.BluePlayer.ActivePoints = new List<IntPoint>();
            Data.BluePlayer.CapturedPoints = new List<IntPoint>();
            Data.BluePlayer.Regions = new List<Region>();

            Data.RedPlayer = new GameSaveData.PlayerStats();
            Data.RedPlayer.TimeInSeconds = 0;
            Data.RedPlayer.Score = 0;
            Data.RedPlayer.ActivePoints = new List<IntPoint>();
            Data.RedPlayer.CapturedPoints = new List<IntPoint>();
            Data.RedPlayer.Regions = new List<Region>();
        }
    }
}
