using System.IO;
using System.Windows.Forms;
using GJson;

namespace PointsOnline
{
    [Name("SaveData")]
    public class SaveData
    {
        [Name("data")]
        public string test = "543gfdgd";
    }

    class SaveDataManager<T>
    {
        protected SaveDataManager()
        {
        }

        private string SaveFilePath { get { return Application.UserAppDataPath + "\\save.json"; } }

        public T Load()
        {
            T data = default(T);

            if (File.Exists(SaveFilePath))
            {
                var json = JsonValue.TryParse(File.ReadAllText(SaveFilePath));
                var jsonData = json["data"];

                string md5 = json["h"];
                var dataMd5 = Utils.GetMd5(jsonData.ToStringIdent());

                if (md5 == dataMd5)
                {
                    data = Serializator.TryDeserialize<T>(jsonData);
                }
            }

            return data;
        }

        public void Save( T data )
        {
            var json = new JsonValue();
            var dataJson = Serializator.Serialize(data);
            var dataString = dataJson.ToStringIdent();
            json["h"] = Utils.GetMd5( dataString );
            json["data"] = dataJson;
            dataString = json.ToStringIdent();

            File.WriteAllText(SaveFilePath, dataString);
        }
    }

    class SaveDataManager : SaveDataManager<SaveData>
    {
        public static SaveDataManager<SaveData> Instance { get; private set; }

        static SaveDataManager()
        {
            Instance = new SaveDataManager();
        }
    }
}
