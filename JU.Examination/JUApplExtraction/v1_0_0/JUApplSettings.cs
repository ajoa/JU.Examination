using JU.ApplicationSupport;
using JU.ApplicationSupportExtraction.v1_0_0.Interface;

namespace JU.Examination.JUApplExtraction.v1_0_0
{
    /// <summary>
    /// Application implementation of IJUApplSettings.
    /// 
    /// This is the version 1.0.0 of the JU.ApplicationSupport Settings extraction implementation, if multiple packages 
    /// uses the same extraction, only one implementation is needed.
    /// </summary>
    public class JUApplSettings : IJUApplSettings
    {
        private string systemName;
        private UseSettings useSettings;
        private EditSettings editSettings;


        /// <summary>
        /// Initializes a new instance of the <see cref="JUApplSettings"/> class.
        /// </summary>
        /// <param name="systemName">Name of the system.</param>
        public JUApplSettings(string systemName)
        {
            useSettings = UseSettings.Instance;
            editSettings = new EditSettings();

            this.systemName = systemName;
        }
        #region IJUApplSettings Members

        /// <summary>
        /// Inserts the a setting in the database.
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <returns><c>true</c> on success, otherwise <c>false</c>.</returns>
        public bool InsertSettings(string keyName, string value, string name = "", string description = "")
        {
            return editSettings.InsertSettings(systemName, keyName, value, name, description);
        }

        /// <summary>
        /// Updates the setting  identified by the key.
        /// </summary>
        /// <param name="keyname">The keyname.</param>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <returns><c>true</c> on success, otherwise <c>false</c>.</returns>
        public bool UpdateSettings(string keyname, string value, string name, string description)
        {
            return editSettings.UpdateSettings(systemName, keyname, value, name, description);
        }

        /// <summary>
        /// Updates the setting value identified by the key.
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> on success, otherwise <c>false</c>.</returns>
        public bool UpdateSettingsValue(string keyName, string value)
        {
            return editSettings.UpdateSettingsValue(systemName, keyName, value);
        }

        /// <summary>
        /// Deletes the setting identified by the key.
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        /// <returns><c>true</c> on success, otherwise <c>false</c>.</returns>
        public bool DeleteSettings(string keyName)
        {
            return editSettings.DeleteSettings(systemName, keyName);
        }

        /// <summary>
        /// Gets the setting value from the database.
        /// </summary>
        /// <param name="keyName">The key name you want to get.</param>
        /// <returns>The value of the setting.</returns>
        public string GetSettingsValue(string keyName)
        {
            return editSettings.GetSettingsValue(systemName, keyName);
        }

        /// <summary>
        /// Returns a value from the database that corresponds to the Key. If not found, the default string is returned.
        /// </summary>
        /// <param name="keyName">The Key to get the value for.</param>
        /// <param name="defaultValue">If no key is found, this value will be returned.</param>
        /// <returns>The value for the key.</returns>
        public string GetKeyValue(string keyName, string defaultValue)
        {
            return useSettings.GetKeyValue(systemName, keyName, defaultValue);
        }

        /// <summary>
        /// Gets the setting description from the database.
        /// </summary>
        /// <param name="keyName">The key name you want to get.</param>
        /// <returns>The description of the setting.</returns>
        public string GetSettingsDescription(string keyName)
        {
            return editSettings.GetSettingsDescription(systemName, keyName);
        }

        /// <summary>
        /// Gets the setting name from the database.
        /// </summary>
        /// <param name="keyName">The key name you want to get.</param>
        /// <returns>The name of the setting.</returns>
        public string GetSettingsName(string keyName)
        {
            return editSettings.GetSettingsName(systemName, keyName);
        }

        /// <summary>
        /// Returns the mode which is currently used in JUApplicationSupport.
        /// </summary>
        /// <returns>Returns use settings mode => (NotUsed), Debug, Test, Release.</returns>
        public JUApplSettingsTypes.Mode GetCurrentMode()
        {
            UseSettings.Mode juasMode = useSettings.GetCurrentMode();
            JUApplSettingsTypes.Mode mode = ConvertMode(juasMode);
            return mode;
        }

        #endregion

        #region Enum convert functions
        /// <summary>
        /// Convert to IJUApplSettings interface type from JUApplicationSupport type of Mode.
        /// </summary>
        /// <param name="fromType">The type value to convert.</param>
        /// <returns>A converted Mode value if matched, otherwise Debug.</returns>
        private JUApplSettingsTypes.Mode ConvertMode(UseSettings.Mode fromType)
        {
            switch (fromType)
            {
                // DEBUG
                case UseSettings.Mode.DEBUG:
                {
                    return JUApplSettingsTypes.Mode.Debug;
                }
                // TEST
                case UseSettings.Mode.TEST:
                {
                    return JUApplSettingsTypes.Mode.Test;
                }
                // RELEASE
                case UseSettings.Mode.RELEASE:
                {
                    return JUApplSettingsTypes.Mode.Release;
                }
                // Default
                default:
                {
                    return JUApplSettingsTypes.Mode.Debug;
                }
            }
        }
        #endregion
    }
}