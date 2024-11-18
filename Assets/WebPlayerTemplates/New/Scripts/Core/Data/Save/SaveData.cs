using System;

namespace Core.Data.Save
{
    [Serializable]
    public class SaveData
    {
        public string key;
        public string data;
        public int version;
        public DateTime Timestamp;

        public SaveData(string key, string data, int version = 1)
        {
            this.key = key;
            this.data = data;
            Timestamp = DateTime.UtcNow;
            this.version = version;
        }
    }
}