using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace SitesToClassroom
{
    public sealed class Logger
    {
        public ObservableCollection<LogEntry> LogEntries { get; set; } = new ObservableCollection<LogEntry>();
        private int messageCounter = 0;

        #region Singleton
        static Logger instance = null;
        static readonly object mylock = new object();

        private Logger()
        {
        }

        public static Logger Instance
        {
            get
            {
                lock (mylock)
                {
                    if (instance == null)
                    {
                        instance = new Logger();
                    }
                    return instance;
                }
            }
        }
        #endregion


        public string Log(string message)
        {
            LogEntries.Add(new LogEntry { DateTime = DateTime.Now, Message = message, Index = ++messageCounter });
#if DEBUG 
            Console.WriteLine(message);
#endif
            return message;
        }

        public string Log(Exception exception, string message)
        {
            LogEntries.Add(new LogEntry { DateTime = DateTime.Now, Message = exception.Message, Index = ++messageCounter });
            LogEntries.Add(new LogEntry { DateTime = DateTime.Now, Message = message, Index = ++messageCounter });
#if DEBUG
            Console.WriteLine(exception);
            Console.WriteLine(message);
#endif
            return message;
        }
    }

    public class LogEntry : PropertyChangedBase
    {
        public DateTime DateTime { get; set; }

        public int Index { get; set; }

        public string Message { get; set; }
    }

    public class CollapsibleLogEntry : LogEntry
    {
        public List<LogEntry> Contents { get; set; }
    }

    public class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        protected virtual void OnPropertyChanged(string propertyName)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }));
        }
    }
}