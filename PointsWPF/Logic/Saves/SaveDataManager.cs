using System.IO;
using System.Windows.Forms;
using GJson;

namespace PointsOnline
{
    class SaveDataManager<T>
    {
        bool _isDebug;

        public SaveDataManager()
        {
#if DEVELOP
            _isDebug = true;
#else
            _isDebug = false;
#endif
        }

        private string SaveFilePath
        {
            get
            {
                return (_isDebug) ? "save.json" : Application.UserAppDataPath + "\\save.json";
            }
        }

        public T Load()
        {
            T data = default(T);

            if (File.Exists(SaveFilePath))
            {
                var json = JsonValue.TryParse(File.ReadAllText(SaveFilePath));
                var jsonData = json["data"];

                string md5 = json["h"];
                var dataMd5 = Utils.GetMd5(jsonData.ToStringIdent());

                if (_isDebug
                    || md5 == dataMd5)
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
            var saveDataString = json.ToStringIdent();

            File.WriteAllText(SaveFilePath, saveDataString);
        }
    }
}
