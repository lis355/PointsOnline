using System.IO;
using System.Windows.Forms;

namespace PointsOnline
{
    class SaveDataManager
    {
        public static SaveDataManager Instance { get; private set; }

        static SaveDataManager()
        {
            Instance = new SaveDataManager();
        }

        private SaveDataManager()
        {
        }

        public void Load()
        {
            if (File.Exists(SaveFilePath))
            {
                var file = File.ReadAllLines(SaveFilePath);
            }
        }

        private string SaveFilePath { get { return Application.UserAppDataPath + "\\save.json"; } }
    }
}
