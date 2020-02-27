using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Collections;
using System.Linq;

/// <summary>
/// src: XCore library
/// http://xcore.codeplex.com/
/// </summary>
public static class XLogger
{
    #region web-app
    /*
    //in global.asax
    void Application_Start(object sender, EventArgs e)
    {
        //VodaLogger.Application = String.Concat("SentimentWeb||", ConfigurationManager.AppSettings["WebsiteVersion"], "||");
        //VodaLogger.Target = ConfigurationManager.AppSettings["EngineRoot"] + @"\Logs\logs.txt";
    }
    */
    //to clear IISExpress cache: System.Reflection.Assembly.GetExecutingAssembly().Location;
    #endregion web-app

    //call this instead of plain throw
    //  ExceptionDispatchInfo.Capture(x).Throw();
    #region timer
    /*
    XLogger.Application = String.Format("SentimentExe||{0}||{1}", Config.ExeVersion, DateTime.Now.ToString("yyyyMMddHHmmss"));
    Stopwatch timer = Stopwatch.StartNew();
    XLogger.Info("BEGIN:\t Scheduled Task Execution");
    TimeSpan ts = timer.Elapsed;
    var elapsed = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
    XLogger.Info($"END:{elapsed}\t Scheduled Task Execution");
    */
    #endregion timer

    #region FLD
    private delegate void LogMessageHandler(LogEntry entry, ILogFormatter formatter);
    private static event LogMessageHandler LogMessage;
    private static ILogFormatter _Formatter;
    #endregion FLD

    #region ENM
    //
    [Flags]
    public enum LogType { File = 1, EventLog = 2, Console = 4, Context = 8, }
    public enum LogStatus { Info, Error, }
    public enum LogFormat { Text, Xml, }
    //
    #endregion ENM

    #region PROP
    public static bool Enabled = true;
    public static bool Split = true;
    public static bool Async = false;
    public static string Application = "";
    public static LogType Type = LogType.File | LogType.Console | LogType.Context;
    public static LogStatus Sensitivity = LogStatus.Info;
    private static string target;
    public static string Target { get { return target; } set { target = value; LogMessage = null; LogMessage += (new FileLog(Target)).Log; } }
    public static LogFormat Format = LogFormat.Text;
    #endregion PROP

    static XLogger()
    {
        //Target = @"c:\X\logs.txt";
        if (String.IsNullOrEmpty(target))
            target = @"Logs\logs.txt";

        // initialize log targets ...
        if ((Type & LogType.File) == LogType.File) LogMessage += (new FileLog(Target)).Log;
        if ((Type & LogType.Console) == LogType.Console) LogMessage += (new ConsoleLog()).Log;
        if ((Type & LogType.EventLog) == LogType.EventLog) LogMessage += (new EventLog()).Log;

        // initialize formatter
        switch (Format)
        {
            case LogFormat.Text:
                _Formatter = new TextFormatter();
                break;
            case LogFormat.Xml:
                _Formatter = new XmlFormatter();
                break;
        }
    }

    #region API
    //
    /// <summary>
    /// <code>
    /// _dbContext.Database.Log = XLogger.EF;
    /// _dbContext.SaveChanges();
    /// </code>
    /// used to Log messages sent by EF to the database
    /// </summary>
    /// <param name="message"></param>
    public static void EF(string message)
    {
        var toSkip = new List<string>() { "connection", "transaction", "Executing" };
        if (!toSkip.Any(s => message.Contains(s)))
            XLogger.Info("EF mes: " + message);
    }
    /// <summary>
    /// 
    /// </summary>
    public static void Sep()
    {
        string sep = "----------------------------------------------------------------------------------------------";
        Log(new LogEntry(DateTime.Now, sep, Application, LogStatus.Info));
    }
    /// <summary>
    /// 
    /// </summary>
    public static void ShortSep()
    {
        string sep = "---------------------------------------------";
        Log(new LogEntry(DateTime.Now, sep, Application, LogStatus.Info));
    }
    /// <summary>
    /// non-fatatl exception
    /// </summary>
    /// <param name="ex">usually ApplicationException</param>
    /// <returns></returns>
    public static bool Info(Exception ex)
    {
        return LogException(ex, LogStatus.Info);
    }
    /// <summary>
    /// Infoes the specified message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns></returns>
    public static bool Info(string message)
    {
        Log(new LogEntry(DateTime.Now, message, Application, LogStatus.Info));
        return true;
    }
    /// <summary>
    /// Infoes the specified message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns></returns>
    public static bool Info(string message, params string[] parameters)
    {
        try
        {
            if (parameters != null && parameters.Length != 0)
                message = string.Format(message, parameters);
        }
        catch { }

        return Info(message);
    }
    //
    /// <summary>
    /// Errors the specified ex.
    /// </summary>
    /// <param name="ex">The ex.</param>
    /// <returns></returns>
    public static bool Error(Exception ex)
    {
        return LogException(ex, LogStatus.Error);
    }
    /// <summary>
    /// Errors the specified message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns></returns>
    public static bool Error(string message)
    {
        Log(new LogEntry(DateTime.Now, message, Application, LogStatus.Error));
        return false;
    }
    /// <summary>
    /// Errors the specified message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns></returns>
    public static bool Error(string message, params string[] parameters)
    {
        try
        {
            if (parameters != null && parameters.Length != 0)
                message = string.Format(message, parameters);
        }
        catch { }

        return Error(message);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="parametersTemplate"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static bool Error(Exception ex, string parametersTemplate, params string[] parameters)
    {
        StringBuilder sb = new StringBuilder();
        try
        {
            sb.AppendFormat("{0}:\n", ex.Source);
            if (parameters != null && parameters.Length != 0)
            {
                sb.AppendFormat(parametersTemplate, parameters);
                sb.AppendLine();
            }
            sb.Append(ex.ToString());
        }
        catch { }

        return Error(sb.ToString());
    }
    //
    #endregion API

    #region UTL
    //
    /// <summary>
    /// Logs the specified log entries.
    /// </summary>
    /// <param name="logEntries">The log entries.</param>
    /// <returns></returns>
    private static bool Log(IList<LogEntry> logEntries)
    {
        if (logEntries == null) return true;

        for (int i = 0; i < logEntries.Count; i++)
        {
            Log(new LogEntry(logEntries[i].Time, logEntries[i].Message, Application, logEntries[i].Status));
        }

        return true;
    }
    /// <summary>
    /// Logs the specified log entry.
    /// </summary>
    /// <param name="logEntry">The log entry.</param>
    private static void Log(LogEntry logEntry)
    {
        if (!Enabled || logEntry.Status < Sensitivity || LogMessage == null) return;
        LogMessage(logEntry, _Formatter);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    private static bool LogException(Exception ex, LogStatus status)
    {
        string data = SerializeException(ex);
        Log(new LogEntry(DateTime.Now, data, Application, status));
        return false;
    }

    private static string SerializeException_old(Exception ex)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("PARAMS:");
        foreach (DictionaryEntry kvp in ex.Data)
            sb.AppendFormat("\t[{0}, {1}]\n", kvp.Key, kvp.Value);

        sb.AppendLine("TRACE:");
        var trace = ex.ToString();
        trace = trace.Replace("--->", "\n--->");
        string data = string.Format("{0}{1}", sb.ToString(), trace);
        return data;
    }

    private static string SerializeException(Exception ex)
    {
        string res = null;
        if ((ex != null) && (ex.Data != null))
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Parameters:");
            foreach (DictionaryEntry L_V_Scalar_DictionaryEntry_KeyValuePair in ex.Data)
                sb.AppendFormat($"\t[{L_V_Scalar_DictionaryEntry_KeyValuePair.Key.ToString().PadRight(70)}, {L_V_Scalar_DictionaryEntry_KeyValuePair.Value}]{System.Environment.NewLine}");
            sb.AppendLine("Trace:");
            string trace = ex.ToString();
			      //This is needed for the case of System.ServiceModel.FaultException, since the calling context is only stored in .StackTrace and not in .ToString.
            if (!(ex is ApplicationException) && !trace.Contains(ex.StackTrace))
                trace += $"{System.Environment.NewLine}==>" + ex.StackTrace;
            trace = trace.Replace("--->", $"{System.Environment.NewLine}--->");
            List<string> rawStackLines = trace.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            rawStackLines.RemoveAll(l=>l.Contains("ExceptionDispatchInfo") || l.StartsWith("--- End"));
            trace = string.Join(Environment.NewLine, rawStackLines);
            res = $"{sb.ToString()}{trace}";
        }
        return res;
    }
    //
    #endregion UTL

    #region CLS
    //
    /// <summary>
    /// 
    /// </summary>
    private class LogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntry"/> class.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <param name="message">The message.</param>
        /// <param name="status">The status.</param>
        public LogEntry(DateTime time, string message, string application, LogStatus status)
        {
            _Time = time;
            _Message = message;
            _Application = application;
            _Status = status;
        }

        private DateTime _Time;
        /// <summary>
        /// Gets or sets the time.
        /// </summary>
        /// <value>The time.</value>
        public DateTime Time { get { return _Time; } set { _Time = value; } }

        private string _Message;
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get { return _Message; } set { _Message = value; } }

        private string _Application;
        /// <summary>
        /// Gets or sets the application.
        /// used to group a set of log entries as belonging to a specific application
        /// </summary>
        /// <value>
        /// The application.
        /// </value>
        public string Application { get { return _Application; } set { _Application = value; } }

        private LogStatus _Status;
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>The status.</value>
        public LogStatus Status { get { return _Status; } set { _Status = value; } }
    }
    /// <summary>
    /// 
    /// </summary>
    private class LogContext
    {
        //private static List<string> _Logs = new List<string>();
    }

    /// <summary>
    /// 
    /// </summary>
    private interface ILogTarget
    {
        /// <summary>
        /// Logs the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="formatter">The formatter.</param>
        void Log(LogEntry entry, ILogFormatter formatter);
    }
    /// <summary>
    /// 
    /// </summary>
    private interface ILogFormatter
    {
        /// <summary>
        /// Formats the specified log entry.
        /// </summary>
        /// <param name="logEntry">The log entry.</param>
        /// <returns></returns>
        string Format(LogEntry logEntry);
    }
    //
    /// <summary>
    /// 
    /// </summary>
    private class FileLog : ILogTarget
    {
        #region Fields

        private bool _Enabled = true;
        private string _Location;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLog"/> class.
        /// </summary>
        public FileLog(string userLocation = null)
        {
            _Enabled = Initialize(userLocation);
        }


        /// <summary>
        /// Logs the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="formatter">The formatter.</param>
        public void Log(LogEntry entry, ILogFormatter formatter)
        {
            if (!_Enabled) return;
            SplitLog();

            try
            {
                string logLine = formatter.Format(entry) + "\r\n";
                File.AppendAllText(_Location, logLine);
            }
            catch { return; }
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns></returns>
        private bool Initialize(string userLocation = null)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(userLocation))
                    _Location = userLocation;
                FileInfo _File = new FileInfo(_Location);
                if (!_File.Directory.Exists) _File.Directory.Create();
                if (!_File.Exists) using (_File.Create()) { }

                return true;
            }
            catch { return false; }
        }
        /// <summary>
        /// Splits the log.
        /// </summary>
        private void SplitLog()
        {
            try
            {
                FileInfo _File = new FileInfo(_Location);
                if (_Enabled && _File.Exists && _File.CreationTime.Date.CompareTo(DateTime.Now.Date) != 0)
                {
                    string backupFileName = string.Format(@"{0}\logs_{1}.txt", _File.DirectoryName, _File.CreationTime.Date.ToString("yyyyMMdd"));
                    _File.MoveTo(backupFileName);

                    Initialize();
                }
            }
            catch { return; }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    private class EventLog : ILogTarget
    {
        #region Fields

        private System.Diagnostics.EventLog _EventLog;
        private const string _SourceName = "XLogger";
        private const string _LogName = "XLogger";

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLog"/> class.
        /// </summary>
        public EventLog()
        {
            Initialize(_SourceName, _LogName);
        }
        //public EventLog( string sourceName , string logName )
        //{
        //    Initialize( sourceName , logName );
        //}

        /// <summary>
        /// Logs the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="formatter">The formatter.</param>
        public void Log(LogEntry entry, ILogFormatter formatter)
        {
            switch (entry.Status)
            {
                case LogStatus.Info:
                    Log(entry, EventLogEntryType.Information, formatter);
                    break;
                case LogStatus.Error:
                    Log(entry, EventLogEntryType.Error, formatter);
                    break;
                default:
                    Log(entry, EventLogEntryType.Warning, formatter);
                    break;
            }
        }

        #region Helpers

        private bool Initialize(string sourceName, string logName)
        {
            if (string.IsNullOrEmpty(sourceName) || string.IsNullOrEmpty(logName)) return false;

            try
            {
                EventSourceCreationData eventSourceCreationData = new EventSourceCreationData(sourceName, logName);
                if (!System.Diagnostics.EventLog.SourceExists(sourceName)) System.Diagnostics.EventLog.CreateEventSource(eventSourceCreationData);

                _EventLog = new System.Diagnostics.EventLog("Application", ".", sourceName);

                return true;
            }
            catch (Exception x)
            {
                Console.WriteLine("XLogger.EventLog.Initialize ... Exception: " + x);
                return false;
            }
        }
        private void Log(LogEntry entry, EventLogEntryType logType, ILogFormatter formatter)
        {
            string logLine = formatter.Format(entry);

            try { _EventLog.WriteEntry(logLine, logType); }
            catch { return; }
        }

        #endregion
    }
    /// <summary>
    /// 
    /// </summary>
    private class ConsoleLog : ILogTarget
    {
        /// <summary>
        /// Logs the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="formatter">The formatter.</param>
        public void Log(LogEntry entry, ILogFormatter formatter)
        {
            Console.WriteLine(formatter.Format(entry));
        }
    }
    //private class ContextLog : ILogTarget
    //{
    //    [ThreadStaticAttribute]
    //    private static LogContext _LogContext;

    //    public void Log( LogEntry entry , ILogFormatter formatter )
    //    {
    //        Console.WriteLine( formatter.Format( entry ) );
    //    }
    //}

    /// <summary>
    /// 
    /// </summary>
    private class TextFormatter : ILogFormatter
    {
        /// <summary>
        /// Formats the specified log entry.
        /// </summary>
        /// <param name="logEntry">The log entry.</param>
        /// <returns></returns>
        public string Format(LogEntry logEntry)
        {
            if (logEntry == null) return "";

            return string.Format(CultureInfo.InvariantCulture, "{0}[{1}] {2}[{3}]:{4}{5}{0}"
                , (logEntry.Status == LogStatus.Error) ? Environment.NewLine : String.Empty                                     //0
                , logEntry.Time.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)                                   //1
                , !String.IsNullOrEmpty(logEntry.Application) ? string.Format("[{0}].", logEntry.Application) : String.Empty    //2
                , logEntry.Status                                                                                               //3
                , logEntry.Status == LogStatus.Error ? Environment.NewLine : " "                                                 //4
                , logEntry.Message                                                                                              //5
                , (logEntry.Status == LogStatus.Error) ? Environment.NewLine : String.Empty);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    private class XmlFormatter : ILogFormatter
    {
        /// <summary>
        /// Formats the specified log entry.
        /// </summary>
        /// <param name="logEntry">The log entry.</param>
        /// <returns></returns>
        public string Format(LogEntry logEntry)
        {
            throw new NotImplementedException();
        }
    }
    //
    #endregion CLS

}

