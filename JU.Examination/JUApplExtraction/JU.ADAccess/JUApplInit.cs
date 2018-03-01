using JU.Examination.JUApplExtraction.v1_0_0;
using JU.ADAccess;

namespace JU.Examination.JUApplExtraction.JU.ADAccess
{
    internal static class JUApplInit
    {
        public static readonly string SystemName = "ADAccess_LIB";

        /// <summary>
        /// Connects the applications implementation of IJUApplSettings and IJUApplLogger.
        /// </summary>
        /// <param name="useSettings">If set to <c>true</c> the settings implementation shall be connected.</param>
        /// <param name="useLogger">If set to <c>true</c> the logger implementation shall be connected.</param>
        /// <returns><c>true</c> on success otherwise false.</returns>
        public static bool ConnectJUApplComponents(bool useSettings = true, bool useLogger = true)
        {
            if (useSettings)
            {
                JUApplConnection.Settings = new JUApplSettings(SystemName);
            }

            if (useLogger)
            {
                JUApplConnection.Logger = new JUApplLogger(SystemName);
            }

            // Check if needed components where connected
            if (null == JUApplConnection.Settings || null == JUApplConnection.Logger)
            {
                return false;
            }

            return true;
        }
    }
}