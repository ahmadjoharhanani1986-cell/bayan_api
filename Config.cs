using System.IO;
using Newtonsoft.Json;

namespace SHLAPI
{
    public class Config
    {
        private const string FILE_NAME = "api.config.json";
        public string ConnectionString { get; set; }

        private static Config _config = null;

        private Config()
        {

        }

        public static Config GetInstance()
        {
            if (_config == null)
            {
                var json = File.ReadAllText(FILE_NAME);
                Serilog.Log.Information("json " + json);
                _config = JsonConvert.DeserializeObject<Config>(json);
            }
            //_config = new Config();
            //_config.ShamelConnectionString = "server=localhost;Port=3306;database=TMS;user=root;password=P@ssw0rd;";
            //_config.ShamelConnectionString = "server=192.168.222.223;Port=3306;database=TMS;user=ahmad;password=P@ssw0rd;";
            return _config;
        }
    }
}