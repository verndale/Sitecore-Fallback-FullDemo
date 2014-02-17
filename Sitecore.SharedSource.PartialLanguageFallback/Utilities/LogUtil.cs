using System;
using System.Web;

namespace Sitecore.SharedSource.PartialLanguageFallback.Utilities
{
    public class LogUtil
    {
        private static readonly LogUtil _instance = new LogUtil();

        private LogUtil() 
		{
			//Enabled = HttpContext.Current.IsDebuggingEnabled;
		}

        public static LogUtil Instance { get { return _instance; } }

		protected bool? _enabled;
        protected bool Enabled 
		{ 
			get { 
				if (!_enabled.HasValue) 
				{
					try
					{
						_enabled = HttpContext.Current.IsDebuggingEnabled;
					}
					catch
					{
						_enabled = false;
					}
				}
				return _enabled.Value;
			}
		}

        public void Info(string message)
        {
            if (Enabled)
            {
                Diagnostics.Log.Info("FallbackLanguageProvider " + message, this);
            }
        }

        public void Info(string message, params object[] args)
        {
            Info(string.Format(message, args));
        }

        public void Error(string message)
        {
            if (Enabled)
            {
                Diagnostics.Log.Error("FallbackLanguageProvider " + message, this);
            }
        }
        public void Error(string message, Exception exception)
        {
            if (Enabled)
            {
                Diagnostics.Log.Error("FallbackLanguageProvider " + message, exception, this);
            }
        }

        public void Error(string message, params object[] args)
        {
            Error(string.Format(message, args));
        }

        public void Error(string message, Exception exception, params object[] args)
        {
            Error(string.Format(message, args), exception);
        }

        public void Debug(string message)
        {
            if (Enabled)
            {
                Diagnostics.Log.Debug("FallbackLanguageProvider " + message, this);
            }
        }

        public void Debug(string message, params object[] args)
        {
            Debug(string.Format(message, args));
        }

        public void Warn(string message)
        {
            if (Enabled)
            {
                Diagnostics.Log.Warn("FallbackLanguageProvider " + message, this);
            }
        }

        public void Warn(string message, params object[] args)
        {
            Warn(string.Format(message, args));
        }
    }
}