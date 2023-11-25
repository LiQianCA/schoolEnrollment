namespace OnlineCourse.Utitlty
{
    /// <summary>
    /// appsettings配置读取辅助类
    /// </summary>
    public class ConfigurationHelper
    {
        #region Filed String

        private static readonly string CONFIGFILELOCK = "CONFIGFILELOCK";
        private static readonly string APPSETTINGS = "AppSettings";
        private static readonly string PROSETTINGS = "ProSettings";
        private static IConfigurationRoot configurationRoot = null;

        #endregion

        #region To DO

        private static ConfigurationHelper _instance = null;

        private ConfigurationHelper()
        {
            configurationRoot = new ConfigurationBuilder()
                    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();
        }

        public static ConfigurationHelper Instance
        {
            get
            {
                lock (CONFIGFILELOCK)
                {
                    if (_instance == null)
                    {
                        _instance = new ConfigurationHelper();
                    }
                    return _instance;
                }
            }
        }

        #endregion

        #region GetConnectionString

        /// <summary>
        /// 获取配置文件中ConnectionStrings中某个配置项的数据库连接字符串
        /// </summary>
        /// <param name="key">ConnectionStrings中某个配置项</param>
        /// <param name="defaultValue">如果没有找到对应的配置项，则返回默认值</param>
        /// <returns>返回配置文件中ConnectionStrings中某个配置项的数据库连接字符串</returns>
        public string GetConnectionString(string key, string defaultValue)
        {
            string result = configurationRoot.GetConnectionString(key);
            if (result == null)
            {
                return defaultValue;
            }
            else
            {
                return result;
            }
        }

        #endregion

        #region GetAppSetting

        /// <summary>
        /// 获取配置文件中AppSettings中某个配置项的值
        /// </summary>
        /// <param name="key">AppSettings中某个配置项</param>
        /// <param name="defaultValue">如果没有找到对应的配置项，则返回默认值</param>
        /// <returns>返回配置文件中AppSettings中某个配置项的值</returns>
        public string GetAppSetting(string key, string defaultValue)
        {
            string result = defaultValue;
            IConfigurationSection configurationSection = configurationRoot.GetSection(APPSETTINGS);
            if (configurationSection != null)
            {
                if (configurationSection.GetSection(key) != null)
                {
                    result = configurationSection.GetSection(key).Value;
                }
            }
            return result;
        }

        /// <summary>
        /// 获取配置文件中AppSettings中某个配置项的值
        /// </summary>
        /// <param name="key">AppSettings中某个配置项</param>
        /// <param name="defaultValue">如果没有找到对应的配置项，则返回默认值</param>
        /// <returns>返回配置文件中AppSettings中某个配置项的值</returns>
        public int GetAppSetting(string key, int defaultValue)
        {
            int result = defaultValue;
            IConfigurationSection configurationSection = configurationRoot.GetSection(APPSETTINGS);
            if (configurationSection != null)
            {
                if (configurationSection.GetSection(key) != null)
                {
                    string strResult = configurationSection.GetSection(key).Value;
                    int.TryParse(strResult, out result);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取配置文件中AppSettings中某个配置项的值
        /// </summary>
        /// <param name="key">AppSettings中某个配置项</param>
        /// <param name="defaultValue">如果没有找到对应的配置项，则返回默认值</param>
        /// <returns>返回配置文件中AppSettings中某个配置项的值</returns>
        public bool GetAppSetting(string key, bool defaultValue)
        {
            bool result = defaultValue;
            IConfigurationSection configurationSection = configurationRoot.GetSection(APPSETTINGS);
            if (configurationSection != null)
            {
                if (configurationSection.GetSection(key) != null)
                {
                    string strResult = configurationSection.GetSection(key).Value;
                    bool.TryParse(strResult, out result);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取配置文件中AppSettings中某个配置项的值
        /// </summary>
        /// <param name="key">AppSettings中某个配置项</param>
        /// <param name="defaultValue">如果没有找到对应的配置项，则返回默认值</param>
        /// <returns>返回配置文件中AppSettings中某个配置项的值</returns>
        public double GetAppSetting(string key, double defaultValue)
        {
            double result = defaultValue;
            IConfigurationSection configurationSection = configurationRoot.GetSection(APPSETTINGS);
            if (configurationSection != null)
            {
                if (configurationSection.GetSection(key) != null)
                {
                    string strResult = configurationSection.GetSection(key).Value;
                    double.TryParse(strResult, out result);
                }
            }
            return result;
        }

        #endregion

        #region GetProSetting

        /// <summary>
        /// 获取配置文件中ProSettings中某个配置项的值
        /// </summary>
        /// <param name="key">ProSettings中某个配置项</param>
        /// <param name="defaultValue">如果没有找到对应的配置项，则返回默认值</param>
        /// <returns>返回配置文件中ProSettings中某个配置项的值</returns>
        public string GetProSetting(string key, string defaultValue)
        {
            string result = defaultValue;
            IConfigurationSection configurationSection = configurationRoot.GetSection(PROSETTINGS);
            if (configurationSection != null)
            {
                if (configurationSection.GetSection(key) != null)
                {
                    result = configurationSection.GetSection(key).Value;
                }
            }
            return result;
        }

        /// <summary>
        /// 获取配置文件中ProSettings中某个配置项的值
        /// </summary>
        /// <param name="key">ProSettings中某个配置项</param>
        /// <param name="defaultValue">如果没有找到对应的配置项，则返回默认值</param>
        /// <returns>返回配置文件中ProSettings中某个配置项的值</returns>
        public int GetProSetting(string key, int defaultValue)
        {
            int result = defaultValue;
            IConfigurationSection configurationSection = configurationRoot.GetSection(PROSETTINGS);
            if (configurationSection != null)
            {
                if (configurationSection.GetSection(key) != null)
                {
                    string strResult = configurationSection.GetSection(key).Value;
                    int.TryParse(strResult, out result);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取配置文件中ProSettings中某个配置项的值
        /// </summary>
        /// <param name="key">ProSettings中某个配置项</param>
        /// <param name="defaultValue">如果没有找到对应的配置项，则返回默认值</param>
        /// <returns>返回配置文件中ProSettings中某个配置项的值</returns>
        public bool GetProSetting(string key, bool defaultValue)
        {
            bool result = defaultValue;
            IConfigurationSection configurationSection = configurationRoot.GetSection(APPSETTINGS);
            if (configurationSection != null)
            {
                if (configurationSection.GetSection(key) != null)
                {
                    string strResult = configurationSection.GetSection(key).Value;
                    bool.TryParse(strResult, out result);
                }
            }
            return result;
        }

        #endregion
    }
}
