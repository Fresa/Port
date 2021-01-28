using System;

namespace Spdy.Logging
{
    internal sealed class NoopLogger : ILogger
    {
        public void Fatal(
            Exception ex,
            IFormatProvider formatProvider,
            string template,
            params object[] args)
        {
        }

        public void Fatal(
            Exception ex,
            string template,
            params object[] args)
        {
        }

        public void Fatal<TMessage>(
            IFormatProvider formatProvider,
            TMessage msg)
        {
        }

        public void Fatal(
            IFormatProvider formatProvider,
            object msg)
        {
        }

        public void Fatal(
            Exception ex,
            string msg)
        {
        }

        public void Fatal(
            string template,
            params object[] args)
        {
        }

        public void Error(
            Exception ex,
            IFormatProvider formatProvider,
            string template,
            params object[] args)
        {
        }

        public void Error(
            Exception ex,
            string template,
            params object[] args)
        {
        }

        public void Error<TMessage>(
            IFormatProvider formatProvider,
            TMessage msg)
        {
        }

        public void Error(
            IFormatProvider formatProvider,
            object msg)
        {
        }

        public void Error(
            Exception ex,
            string msg)
        {
        }

        public void Error(
            string template,
            params object[] args)
        {
        }

        public void Warning(
            Exception ex,
            IFormatProvider formatProvider,
            string template,
            params object[] args)
        {
        }

        public void Warning(
            Exception ex,
            string template,
            params object[] args)
        {
        }

        public void Warning<TMessage>(
            IFormatProvider formatProvider,
            TMessage msg)
        {
        }

        public void Warning(
            IFormatProvider formatProvider,
            object msg)
        {
        }

        public void Warning(
            Exception ex,
            string msg)
        {
        }

        public void Warning(
            string template,
            params object[] args)
        {
        }

        public void Info(
            Exception ex,
            IFormatProvider formatProvider,
            string template,
            params object[] args)
        {
        }

        public void Info(
            Exception ex,
            string template,
            params object[] args)
        {
        }

        public void Info<TMessage>(
            IFormatProvider formatProvider,
            TMessage msg)
        {
        }

        public void Info(
            IFormatProvider formatProvider,
            object msg)
        {
        }

        public void Info(
            Exception ex,
            string msg)
        {
        }

        public void Info(
            string template,
            params object[] args)
        {
        }

        public void Debug(
            Exception ex,
            IFormatProvider formatProvider,
            string template,
            params object[] args)
        {
        }

        public void Debug(
            Exception ex,
            string template,
            params object[] args)
        {
        }

        public void Debug<TMessage>(
            IFormatProvider formatProvider,
            TMessage msg)
        {
        }

        public void Debug(
            IFormatProvider formatProvider,
            object msg)
        {
        }

        public void Debug(
            Exception ex,
            string msg)
        {
        }

        public void Debug(
            string template,
            params object[] args)
        {
        }

        public void Trace(
            Exception ex,
            IFormatProvider formatProvider,
            string template,
            params object[] args)
        {
        }

        public void Trace(
            Exception ex,
            string template,
            params object[] args)
        {
        }

        public void Trace<TMessage>(
            IFormatProvider formatProvider,
            TMessage msg)
        {
        }

        public void Trace(
            IFormatProvider formatProvider,
            object msg)
        {
        }

        public void Trace(
            Exception ex,
            string msg)
        {
        }

        public void Trace(
            string template,
            params object[] args)
        {
        }

        public ILogicalThreadContext LogicalThreadContext { get; } =
            new NoopLogicalThreadContext();
    }
}