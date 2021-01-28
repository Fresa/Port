using System;
using Spdy.Logging.Annotations;

namespace Spdy.Logging
{
    public interface ILogger
    {
        [StringFormatMethod("template")]
        void Fatal(Exception ex, IFormatProvider formatProvider, string template, params object[] args);
        [StringFormatMethod("template")]
        void Fatal(Exception ex, string template, params object[] args);    
        void Fatal<TMessage>(IFormatProvider formatProvider, TMessage msg);
        void Fatal(IFormatProvider formatProvider, object msg);
        void Fatal(Exception ex, string msg);
        [StringFormatMethod("template")]
        void Fatal(string template, params object[] args);

        [StringFormatMethod("template")]
        void Error(Exception ex, IFormatProvider formatProvider, string template, params object[] args);
        [StringFormatMethod("template")]
        void Error(Exception ex, string template, params object[] args);
        void Error<TMessage>(IFormatProvider formatProvider, TMessage msg);
        void Error(IFormatProvider formatProvider, object msg);
        void Error(Exception ex, string msg);
        [StringFormatMethod("template")]
        void Error(string template, params object[] args);

        [StringFormatMethod("template")]
        void Warning(Exception ex, IFormatProvider formatProvider, string template, params object[] args);
        [StringFormatMethod("template")]
        void Warning(Exception ex, string template, params object[] args);
        void Warning<TMessage>(IFormatProvider formatProvider, TMessage msg);
        void Warning(IFormatProvider formatProvider, object msg);
        void Warning(Exception ex, string msg);
        [StringFormatMethod("template")]
        void Warning(string template, params object[] args);

        [StringFormatMethod("template")]
        void Info(Exception ex, IFormatProvider formatProvider, string template, params object[] args);
        [StringFormatMethod("template")]
        void Info(Exception ex, string template, params object[] args);
        void Info<TMessage>(IFormatProvider formatProvider, TMessage msg);
        void Info(IFormatProvider formatProvider, object msg);
        void Info(Exception ex, string msg);
        [StringFormatMethod("template")]
        void Info(string template, params object[] args);

        [StringFormatMethod("template")]
        void Debug(Exception ex, IFormatProvider formatProvider, string template, params object[] args);
        [StringFormatMethod("template")]
        void Debug(Exception ex, string template, params object[] args);
        void Debug<TMessage>(IFormatProvider formatProvider, TMessage msg);
        void Debug(IFormatProvider formatProvider, object msg);
        void Debug(Exception ex, string msg);
        [StringFormatMethod("template")]
        void Debug(string template, params object[] args);

        [StringFormatMethod("template")]
        void Trace(Exception ex, IFormatProvider formatProvider, string template, params object[] args);
        [StringFormatMethod("template")]
        void Trace(Exception ex, string template, params object[] args);
        void Trace<TMessage>(IFormatProvider formatProvider, TMessage msg);
        void Trace(IFormatProvider formatProvider, object msg);
        void Trace(Exception ex, string msg);
        [StringFormatMethod("template")]
        void Trace(string template, params object[] args);
        
        ILogicalThreadContext LogicalThreadContext { get; }
    }
}