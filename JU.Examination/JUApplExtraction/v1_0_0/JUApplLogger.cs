using System;
using System.Runtime.CompilerServices;
using JU.ApplicationSupport;
using JU.ApplicationSupportExtraction.v1_0_0.Interface;

namespace JU.Examination.JUApplExtraction.v1_0_0
{
    /// <summary>
    /// Application implementation of IJUApplLogger.
    /// 
    /// This is the version 1.0.0 of the JU.ApplicationSupport Logger extraction implementation, if multiple packages 
    /// uses the same extraction, only one implementation is needed.
    /// </summary>
    public class JUApplLogger : IJUApplLogger
    {
        private string systemName;
        private Log logger;
        private LogToFile fileLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JUApplLogger"/> class.
        /// </summary>
        /// <param name="systemName">Name of the system.</param>
        public JUApplLogger(string systemName)
        {
            logger = new Log();
            fileLogger = LogToFile.Instance;
            this.systemName = systemName;
        }

        #region IJUApplLogger Members
        /// <summary>
        /// Logs an exception.
        /// </summary>
        /// <param name="errorMessage">A string containing a readable error message.</param>
        /// <param name="e">The exception.</param>
        /// <param name="parameters">The parameter string.</param>
        /// <param name="serializedObject">Any object useful to be logged, in a serialized format.</param>
        /// <param name="memberName">Automatic parameter, should not be set. Ends up as the method name.</param>
        /// <param name="sourceFilePath">Automatic parameter, should not be set. Ends up as the source file
        /// path.</param>
        /// <param name="sourceLineNumber">Automatic parameter, should not be set. Ends up as the source line 
        /// number.</param>
        /// <example>
        /// <code>
        /// LogException("Unable to add new Student to the system.", e, String.Format("Id = {0}"), 
        ///     student.SerializeObject());
        /// </code>
        /// </example>
        public void LogException(string errorMessage, Exception e, string parameters = "", string serializedObject = "",
            [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (null == e) { return; }
            if (null == errorMessage) { errorMessage = ""; }
            if (null == parameters) { parameters = ""; }
            if (null == serializedObject) { serializedObject = ""; }
            if (null == memberName) { memberName = ""; }
            if (null == sourceFilePath) { sourceFilePath = ""; }

            string filename = sourceFilePath.Substring(sourceFilePath.LastIndexOf('\\') + 1);
            string compiledMessage = String.Format("EXCEPTION: {0}::{1} ({2}) -> {3}, {4}\r\nmsg = {5}.",
                filename, memberName, sourceLineNumber, errorMessage, parameters, e.Message);

            LogErrorEntry(JUApplLoggerTypes.LogType.Exception, "Exception", compiledMessage, serializedObject);
        }

        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="errorMessage">A string containing a readable error message.</param>
        /// <param name="parameters">The parameter string.</param>
        /// <param name="serializedObject">Any object useful to be logged, in a serialized format.</param>
        /// <param name="memberName">Automatic parameter, should not be set. Ends up as the method name.</param>
        /// <param name="sourceFilePath">Automatic parameter, should not be set. Ends up as the source file 
        /// path.</param>
        /// <param name="sourceLineNumber">Automatic parameter, should not be set. Ends up as the source line 
        /// number.</param>
        /// <example>
        /// <code>
        /// LogError("Search returned unexpected multiple results", 
        ///     String.Format("UserIdentification = {0}", userDN), 
        ///     handlers.SerializeObject());
        /// </code>
        /// </example>
        public void LogError(string errorMessage, string parameters = "", string serializedObject = "",
            [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (null == errorMessage) { errorMessage = ""; }
            if (null == parameters) { parameters = ""; }
            if (null == serializedObject) { serializedObject = ""; }
            if (null == memberName) { memberName = ""; }
            if (null == sourceFilePath) { sourceFilePath = ""; }

            string filename = sourceFilePath.Substring(sourceFilePath.LastIndexOf('\\') + 1);
            string compiledMessage = String.Format("ERROR: {0}::{1} ({2}) -> {3}. {4}.", filename, memberName,
                sourceLineNumber, errorMessage, parameters);

            LogErrorEntry(JUApplLoggerTypes.LogType.Error, "Error", compiledMessage, serializedObject);
        }

        /// <summary>
        /// Logs a warning.
        /// </summary>                                            
        /// <param name="warningMessage">A string containing a readable warning message.</param>
        /// <param name="parameters">The parameter string.</param>
        /// <param name="serializedObject">Any object useful to be logged, in a serialized format.</param>
        public void LogWarning(string warningMessage, string parameters = "", string serializedObject = "")
        {
            if (null == warningMessage) { warningMessage = ""; }
            if (null == parameters) { parameters = ""; }
            if (null == serializedObject) { serializedObject = ""; }

            string compiledMessage = String.Format("WARNING: {0}. {1}.", warningMessage, parameters);
            LogErrorEntry(JUApplLoggerTypes.LogType.Warning, "Warning", compiledMessage, serializedObject);
        }

        private void LogErrorEntry(JUApplLoggerTypes.LogType type, string typeString, string logEntry,
            string serializedObject = "")
        {
            AddLog(type, typeString, "", JUApplLoggerTypes.ExternalIdType.NoRef, logEntry,
                "", "", "", "", "", "", "", serializedObject);
        }


        /// <summary>
        /// Adds the log post to the database.
        /// </summary>
        /// <param name="type">The type of log post (one of values in enum LogType).</param>
        /// <param name="text">A short generic log text, this shouldn't be parameterized.</param>
        public void AddLog(JUApplLoggerTypes.LogType type, string text)
        {
            AddLog(type, text, "", 0, "", "", "", "", "", "", "", "", "");
        }

        /// <summary>
        /// Adds the log post to the database.
        /// </summary>
        /// <param name="type">The type of log post (one of values in enum LogType).</param>
        /// <param name="text">A short generic log text, this shouldn't be parameterized.</param>
        /// <param name="description">A detailed log post description, this may pe parameterized.</param>
        public void AddLog(JUApplLoggerTypes.LogType type, string text, string description)
        {
            AddLog(type, text, "", JUApplLoggerTypes.ExternalIdType.NoRef, description, "", "", "",
                "", "", "", "", "");
        }

        /// <summary>
        /// Adds the log post to the database.
        /// </summary>
        /// <param name="type">The type of log post (one of values in enum LogType).</param>
        /// <param name="text">A short generic log text, this shouldn't be parameterized.</param>
        /// <param name="externalId">The external id used as identification.</param>
        /// <param name="externalIdType">Type of the external id (one of the values in enum ExternalIdType).</param>
        public void AddLog(JUApplLoggerTypes.LogType type, string text, string externalId,
            JUApplLoggerTypes.ExternalIdType externalIdType)
        {
            AddLog(type, text, externalId, externalIdType, "", "", "", "", "", "", "", "", "");
        }

        /// <summary>
        /// Adds the log post to the database.
        /// </summary>
        /// <param name="type">The type of log post (one of values in enum LogType).</param>
        /// <param name="text">A short generic log text, this shouldn't be parameterized.</param>
        /// <param name="externalId">The external id used as identification.</param>
        /// <param name="externalIdType">Type of the external id (one of the values in enum ExternalIdType).</param>
        /// <param name="description">A detailed log post description, this may pe parameterized.</param>
        /// <param name="loggedInUser">The logged in user.</param>
        public void AddLog(JUApplLoggerTypes.LogType type, string text, string externalId,
            JUApplLoggerTypes.ExternalIdType externalIdType, string description, string loggedInUser)
        {
            AddLog(type, text, externalId, externalIdType, description, loggedInUser, "", "", "", "", "", "", "");
        }

        /// <summary>
        /// Adds the log post to the database.
        /// </summary>
        /// <param name="type">The type of log post (one of values in enum LogType).</param>
        /// <param name="text">A short generic log text, this shouldn't be parameterized.</param>
        /// <param name="externalId">The external id used as identification.</param>
        /// <param name="externalIdType">Type of the external id (one of the values in enum ExternalIdType).</param>
        /// <param name="description">A detailed log post description, this may pe parameterized.</param>
        /// <param name="loggedInUser">The logged in user (Optional).</param>
        /// <param name="loggedInUserAccessType">The user access level (Optional).</param>
        /// <param name="externalIdInstitution">The institution the external id is tied to (Optional).</param>
        /// <param name="subSystem">The subsystem, compared to the system this may be e.g. WEB, SERVICE, 
        /// MOBILE etc.</param>
        /// <param name="executionTime">The execution time, may be used for performance logging (Optional).</param>
        /// <param name="extraId">A generic id (Optional).</param>
        /// <param name="device">The device, this may be used to identify e.g. iPhone, Android (Optional).</param>
        /// <param name="data">Generic data, may be used as debugging information (Optional).</param>
        public void AddLog(JUApplLoggerTypes.LogType type, string text, string externalId = "",
            JUApplLoggerTypes.ExternalIdType externalIdType = JUApplLoggerTypes.ExternalIdType.NoRef,
            string description = "", string loggedInUser = "", string loggedInUserAccessType = "",
            string externalIdInstitution = "", string subSystem = "", string executionTime = "", string extraId = "",
            string device = "", string data = "")
        {
            Log.LogType juasLogType = ConvertLogType(type);
            Log.ExternalIdType juasExternalIdType = ConvertExternalIdType(externalIdType);

            // Add the log post
            // Failsafe try/catch, we don't want the error logging to crash
            try
            {
                logger.AddLog(systemName, juasLogType, text, externalId, juasExternalIdType, description, loggedInUser,
                    loggedInUserAccessType, externalIdInstitution, subSystem, executionTime, extraId, device, data);
            }
            catch (System.IO.IOException)
            {
                // Do nothing, procede
                return;
            }
            catch (LogToFile.WriteToFileException)
            {
                // Do nothing, procede
                return;
            }
        }

        /// <summary>
        /// Gets the log post from the database.
        /// </summary>
        /// <param name="sqlFilter">Create your own sql filter.</param>
        /// <returns>A DataTable with the result, on error <c>null</c> is returned.</returns>
        public System.Data.DataTable GetLog(string sqlFilter)
        {
            return GetLog(JUApplLoggerTypes.LogType.UseOfSystem, "", "", 0, "", "", "", "", "", "", "", "", "",
                sqlFilter);
        }

        /// <summary>
        /// Gets the log post from the database.
        /// </summary>
        /// <param name="type">The type of log post (one of values in enum LogType).</param>
        /// <param name="text">A short generic log text, this shouldn't be parameterized.</param>
        /// <returns>A DataTable with the result, on error <c>null</c> is returned.</returns>
        public System.Data.DataTable GetLog(JUApplLoggerTypes.LogType type, string text)
        {
            return GetLog(type, text, "", 0, "", "", "", "", "", "", "", "", "", "");
        }

        /// <summary>
        /// Gets the log post from the database.
        /// </summary>
        /// <param name="type">The type of log post (one of values in enum LogType).</param>
        /// <param name="text">A short generic log text, this shouldn't be parameterized.</param>
        /// <param name="description">A detailed log post description, this may pe parameterized.</param>
        /// <returns>A DataTable with the result, on error <c>null</c> is returned.</returns>
        public System.Data.DataTable GetLog(JUApplLoggerTypes.LogType type, string text, string description)
        {
            return GetLog(type, text, "", 0, description, "", "", "", "", "", "", "", "", "");
        }

        /// <summary>
        /// Gets the log post from the database.
        /// </summary>
        /// <param name="type">The type of log post (one of values in enum LogType).</param>
        /// <param name="text">A short generic log text, this shouldn't be parameterized.</param>
        /// <param name="externalId">The external id used as identification.</param>
        /// <param name="externalIdType">Type of the external id (one of the values in enum ExternalIdType).</param>
        /// <returns>A DataTable with the result, on error <c>null</c> is returned.</returns>
        public System.Data.DataTable GetLog(JUApplLoggerTypes.LogType type, string text, string externalId,
            JUApplLoggerTypes.ExternalIdType externalIdType)
        {
            return GetLog(type, text, externalId, externalIdType, "", "", "", "", "", "", "", "", "", "");
        }

        /// <summary>
        /// Gets the log post from the database.
        /// </summary>
        /// <param name="type">The type of log post (one of values in enum LogType).</param>
        /// <param name="text">A short generic log text, this shouldn't be parameterized.</param>
        /// <param name="externalId">The external id used as identification.</param>
        /// <param name="externalIdType">Type of the external id (one of the values in enum ExternalIdType).</param>
        /// <param name="description">A detailed log post description, this may pe parameterized.</param>
        /// <param name="loggedInUser">The logged in user.</param>
        /// <returns>A DataTable with the result, on error <c>null</c> is returned.</returns>
        public System.Data.DataTable GetLog(JUApplLoggerTypes.LogType type, string text, string externalId,
            JUApplLoggerTypes.ExternalIdType externalIdType, string description, string loggedInUser)
        {
            return GetLog(type, text, externalId, externalIdType, description, loggedInUser, "", "",
                "", "", "", "", "", "");
        }

        /// <summary>
        /// Gets the log post from the database.
        /// </summary>
        /// <param name="type">The type of log post (one of values in enum LogType).</param>
        /// <param name="text">A short generic log text, this shouldn't be parameterized.</param>
        /// <param name="externalId">The external id used as identification.</param>
        /// <param name="externalIdType">Type of the external id (one of the values in enum ExternalIdType).</param>
        /// <param name="description">A detailed log post description, this may pe parameterized.</param>
        /// <param name="loggedInUser">The logged in user.</param>
        /// <param name="loggedInUserAccessType">The user access level.</param>
        /// <param name="externalIdInstitution">The institution the external id is tied to.</param>
        /// <param name="subSystem">The subsystem, compared to the system this may be e.g. WEB, SERVICE, 
        /// MOBILE etc.</param>
        /// <param name="executionTime">The execution time, may be used for performance logging.</param>
        /// <param name="extraId">A generic id.</param>
        /// <param name="device">The device, this may be used to identify e.g. iPhone, Android.</param>
        /// <param name="data">Generic data, may be used as debugging information.</param>
        /// <param name="sqlFilter">Create your own sql filter.</param>
        /// <returns>A DataTable with the result, on error <c>null</c> is returned.</returns>
        public System.Data.DataTable GetLog(JUApplLoggerTypes.LogType type, string text, string externalId = "",
            JUApplLoggerTypes.ExternalIdType externalIdType = JUApplLoggerTypes.ExternalIdType.NoRef,
            string description = "", string loggedInUser = "", string loggedInUserAccessType = "",
            string externalIdInstitution = "", string subSystem = "", string executionTime = "", string extraId = "",
            string device = "", string data = "", string sqlFilter = "")
        {
            // Failsafe try/catch, we don't want the log fetching to crash, return null instead.
            try
            {
                return logger.GetLog(
                    systemName, ConvertLogType(type), text, externalId, ConvertExternalIdType(externalIdType),
                    description, loggedInUser, loggedInUserAccessType, externalIdInstitution, subSystem, executionTime,
                    extraId, device, data, sqlFilter);
            }
            catch (System.InvalidOperationException e)
            {
                LogException("Unable to fetch log posts", e);
                return null;
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                LogException("Unable to fetch log posts", e);
                return null;
            }
            catch (System.Configuration.ConfigurationException e)
            {
                LogException("Unable to fetch log posts", e);
                return null;
            }
        }

        /// <summary>
        /// Writes the message to a file, exceptions are handled internally.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The message source.</param>
        /// <param name="logLevel">The log level.</param>
        /// <returns><c>true</c> on success otherwise <c>false</c></returns>
        public bool FileLoggerWriteMsg(string message, string source, JUApplLoggerTypes.LogLevel logLevel)
        {
            LogLevel juasLogLevel = ConvertLogLevel(logLevel);
            bool success = fileLogger.WriteMsgSafe(message, source, juasLogLevel);
            return success;
        }

        #endregion

        #region Enum convert functions
        /// <summary>
        /// Convert from IJUApplLogger interface type to JUApplicationSupport type of ExternalIdType.
        /// </summary>
        /// <param name="fromType">The type value to convert.</param>
        /// <returns>A converted ExternalIdType value if matched, otherwise NO_REF.</returns>
        private Log.ExternalIdType ConvertExternalIdType(JUApplLoggerTypes.ExternalIdType fromType)
        {
            switch (fromType)
            {
                // DBID
                case JUApplLoggerTypes.ExternalIdType.DBID:
                {
                    return Log.ExternalIdType.DBID;
                }
                // DN
                case JUApplLoggerTypes.ExternalIdType.DN:
                {
                    return Log.ExternalIdType.DN;
                }
                // SETTING_NAME
                case JUApplLoggerTypes.ExternalIdType.SettingName:
                {
                    return Log.ExternalIdType.SETTING_NAME;
                }
                // USERNAME
                case JUApplLoggerTypes.ExternalIdType.Username:
                {
                    return Log.ExternalIdType.USERNAME;
                }
                // Default
                default:
                {
                    return Log.ExternalIdType.NO_REF;
                }
            }
        }

        /// <summary>
        /// Convert from IJUApplLogger interface type to JUApplicationSupport type of LogType.
        /// </summary>
        /// <param name="fromType">The type value to convert.</param>
        /// <returns>A converted LogType value if matched, otherwise USE_UF_SYSTEM.</returns>
        private Log.LogType ConvertLogType(JUApplLoggerTypes.LogType fromType)
        {
            switch (fromType)
            {
                // DEBUG
                case JUApplLoggerTypes.LogType.Debug:
                {
                    return Log.LogType.DEBUG;
                }
                // WARNING
                case JUApplLoggerTypes.LogType.Warning:
                {
                    return Log.LogType.WARNING;
                }
                // ERROR
                case JUApplLoggerTypes.LogType.Error:
                {
                    return Log.LogType.ERROR;
                }
                // EXCEPTION
                case JUApplLoggerTypes.LogType.Exception:
                {
                    return Log.LogType.EXCEPTION;
                }
                // INFO
                case JUApplLoggerTypes.LogType.Info:
                {
                    return Log.LogType.INFO;
                }
                // SEARCH
                case JUApplLoggerTypes.LogType.Search:
                {
                    return Log.LogType.SEARCH;
                }
                // VIEW
                case JUApplLoggerTypes.LogType.View:
                {
                    return Log.LogType.VIEW;
                }
                // DEFAULT
                default:
                {
                    return Log.LogType.USE_OF_SYSTEM;
                }
            }
        }

        /// <summary>
        /// Convert from IJUApplLogger interface type to JUApplicationSupport type of LogLevel.
        /// </summary>
        /// <param name="fromType">The type value to convert.</param>
        /// <returns>A converted LogLevel value if matched, otherwise Normal.</returns>
        private LogLevel ConvertLogLevel(JUApplLoggerTypes.LogLevel fromType)
        {
            switch (fromType)
            {
                // None
                case JUApplLoggerTypes.LogLevel.None:
                {
                    return LogLevel.Debug;
                }
                // Normal
                case JUApplLoggerTypes.LogLevel.Normal:
                {
                    return LogLevel.Normal;
                }
                // Debug
                case JUApplLoggerTypes.LogLevel.Debug:
                {
                    return LogLevel.Debug;
                }
                // Default
                default:
                {
                    return LogLevel.Normal;
                }
            }
        }
        #endregion
    }
}