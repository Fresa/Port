using System;
using NLog;
using Spdy.Logging;
using ILogger = Spdy.Logging.ILogger;

namespace Spdy.UnitTests.Logging
{
    public class NLogLogger : ILogger
    {
        private readonly Lazy<Logger> _lazyLoggerResolver;
        private Logger Logger => _lazyLoggerResolver.Value;

        public NLogLogger(string type, ILogicalThreadContext logContext)
        {
            _lazyLoggerResolver = new Lazy<Logger>(() => LogManager.GetLogger(type));
            LogicalThreadContext = logContext;
        }

        public void Fatal(Exception ex, IFormatProvider formatProvider, string template, params object[] args)
        {
            Logger.Fatal(ex, formatProvider, template, args);
        }

        public void Fatal(Exception ex, string template, params object[] args)
        {
            Logger.Fatal(ex, template, args);
        }

        public void Fatal<T>(IFormatProvider formatProvider, T msg)
        {
            Logger.Fatal(formatProvider, msg);
        }

        public void Fatal(IFormatProvider formatProvider, object msg)
        {
            Logger.Fatal(formatProvider, msg);
        }

        public void Fatal(Exception ex, string msg)
        {
            Logger.Fatal(ex, msg);
        }

        public void Fatal(string template, params object[] args)
        {
            Logger.Fatal(template, args);
        }


        public void Error(Exception ex, IFormatProvider formatProvider, string template, params object[] args)
        {
            Logger.Error(ex, formatProvider, template, args);
        }

        public void Error(Exception ex, string template, params object[] args)
        {
            Logger.Error(ex, template, args);
        }

        public void Error<TMessage>(IFormatProvider formatProvider, TMessage msg)
        {
            Logger.Error(formatProvider, msg);
        }

        public void Error(IFormatProvider formatProvider, object msg)
        {
            Logger.Error(formatProvider, msg);
        }

        public void Error(Exception ex, string msg)
        {
            Logger.Error(ex, msg);
        }

        public void Error(string template, params object[] args)
        {
            Logger.Error(template, args);
        }


        public void Warning(Exception ex, IFormatProvider formatProvider, string template, params object[] args)
        {
            Logger.Warn(ex, formatProvider, template, args);
        }

        public void Warning(Exception ex, string template, params object[] args)
        {
            Logger.Warn(ex, template, args);
        }

        public void Warning<TMessage>(IFormatProvider formatProvider, TMessage msg)
        {
            Logger.Warn(formatProvider, msg);
        }

        public void Warning(IFormatProvider formatProvider, object msg)
        {
            Logger.Warn(formatProvider, msg);
        }

        public void Warning(Exception ex, string msg)
        {
            Logger.Warn(ex, msg);
        }

        public void Warning(string template, params object[] args)
        {
            Logger.Warn(template, args);
        }


        public void Info(Exception ex, IFormatProvider formatProvider, string template, params object[] args)
        {
            Logger.Info(ex, formatProvider, template, args);
        }

        public void Info(Exception ex, string template, params object[] args)
        {
            Logger.Info(ex, template, args);
        }

        public void Info<TMessage>(IFormatProvider formatProvider, TMessage msg)
        {
            Logger.Info(formatProvider, msg);
        }

        public void Info(IFormatProvider formatProvider, object msg)
        {
            Logger.Info(formatProvider, msg);
        }

        public void Info(Exception ex, string msg)
        {
            Logger.Info(ex, msg);
        }

        public void Info(string template, params object[] args)
        {
            Logger.Info(template, args);
        }


        public void Debug(Exception ex, IFormatProvider formatProvider, string template, params object[] args)
        {
            Logger.Debug(ex, formatProvider, template, args);
        }

        public void Debug(Exception ex, string template, params object[] args)
        {
            Logger.Debug(ex, template, args);
        }

        public void Debug<TMessage>(IFormatProvider formatProvider, TMessage msg)
        {
            Logger.Debug(formatProvider, msg);
        }

        public void Debug(IFormatProvider formatProvider, object msg)
        {
            Logger.Debug(formatProvider, msg);
        }

        public void Debug(Exception ex, string msg)
        {
            Logger.Debug(ex, msg);
        }

        public void Debug(string template, params object[] args)
        {
            Logger.Debug(template, args);
        }


        public void Trace(Exception ex, IFormatProvider formatProvider, string template, params object[] args)
        {
            Logger.Trace(ex, formatProvider, template, args);
        }

        public void Trace(Exception ex, string template, params object[] args)
        {
            Logger.Trace(ex, template, args);
        }

        public void Trace<TMessage>(IFormatProvider formatProvider, TMessage msg)
        {
            Logger.Trace(formatProvider, msg);
        }

        public void Trace(IFormatProvider formatProvider, object msg)
        {
            Logger.Trace(formatProvider, msg);
        }

        public void Trace(Exception ex, string msg)
        {
            Logger.Trace(ex, msg);
        }

        public void Trace(string template, params object[] args)
        {
            Logger.Trace(template, args);
        }


        public ILogicalThreadContext LogicalThreadContext { get; }
    }
}