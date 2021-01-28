using System;
using Spdy.Logging;

namespace Spdy.UnitTests.Logging
{
    public class FakeLogger : ILogger
    {
        public string Name { get; }

        public FakeLogger(string name)
        {
            Name = name;
        }

        public void Fatal(Exception ex, IFormatProvider formatProvider, string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Fatal(Exception ex, string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Fatal<TMessage>(IFormatProvider formatProvider, TMessage msg)
        {
            throw new NotImplementedException();
        }

        public void Fatal(IFormatProvider formatProvider, object msg)
        {
            throw new NotImplementedException();
        }

        public void Fatal(Exception ex, string msg)
        {
            throw new NotImplementedException();
        }

        public void Fatal(string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Error(Exception ex, IFormatProvider formatProvider, string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Error(Exception ex, string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Error<TMessage>(IFormatProvider formatProvider, TMessage msg)
        {
            throw new NotImplementedException();
        }

        public void Error(IFormatProvider formatProvider, object msg)
        {
            throw new NotImplementedException();
        }

        public void Error(Exception ex, string msg)
        {
            throw new NotImplementedException();
        }

        public void Error(string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Warning(Exception ex, IFormatProvider formatProvider, string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Warning(Exception ex, string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Warning<TMessage>(IFormatProvider formatProvider, TMessage msg)
        {
            throw new NotImplementedException();
        }

        public void Warning(IFormatProvider formatProvider, object msg)
        {
            throw new NotImplementedException();
        }

        public void Warning(Exception ex, string msg)
        {
            throw new NotImplementedException();
        }

        public void Warning(string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Info(Exception ex, IFormatProvider formatProvider, string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Info(Exception ex, string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Info<TMessage>(IFormatProvider formatProvider, TMessage msg)
        {
            throw new NotImplementedException();
        }

        public void Info(IFormatProvider formatProvider, object msg)
        {
            throw new NotImplementedException();
        }

        public void Info(Exception ex, string msg)
        {
            throw new NotImplementedException();
        }

        public void Info(string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Debug(Exception ex, IFormatProvider formatProvider, string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Debug(Exception ex, string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Debug<TMessage>(IFormatProvider formatProvider, TMessage msg)
        {
            throw new NotImplementedException();
        }

        public void Debug(IFormatProvider formatProvider, object msg)
        {
            throw new NotImplementedException();
        }

        public void Debug(Exception ex, string msg)
        {
            throw new NotImplementedException();
        }

        public void Debug(string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Trace(Exception ex, IFormatProvider formatProvider, string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Trace(Exception ex, string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Trace<TMessage>(IFormatProvider formatProvider, TMessage msg)
        {
            throw new NotImplementedException();
        }

        public void Trace(IFormatProvider formatProvider, object msg)
        {
            throw new NotImplementedException();
        }

        public void Trace(Exception ex, string msg)
        {
            throw new NotImplementedException();
        }

        public void Trace(string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public ILogicalThreadContext LogicalThreadContext { get; }
    }
}