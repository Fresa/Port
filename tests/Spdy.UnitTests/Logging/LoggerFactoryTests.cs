using FluentAssertions;
using Spdy.Logging;
using Test.It.With.XUnit;
using Xunit;

namespace Spdy.UnitTests.Logging
{
    public class When_creating_a_logger_specifying_a_class : XUnit2Specification
    {
        private ILogger _createdLogger;
        
        protected override void When()
        {
            _createdLogger = FakeLoggerFactory.Create()
                                              .Create<FakeLoggerFactory>();
        }
        
        [Fact]
        public void It_should_create_a_fake_logger()
        {
            _createdLogger.Should().BeOfType<FakeLogger>();
        }
    }

    public class When_creating_a_logger_specifying_a_name : XUnit2Specification
    {
        private ILogger _createdLogger;

        protected override void When()
        {
            _createdLogger = FakeLoggerFactory.Create()
                                              .Create(
                                                  "Given_a_app_configured_logger_factory.When_creating_a_logger");
        }

        [Fact]
        public void It_should_create_a_fake_logger()
        {
            _createdLogger.Should().BeOfType<FakeLogger>();
        }

        [Fact]
        public void It_should_create_a_logger_with_the_name_of_the_creating_class()
        {
            ((FakeLogger) _createdLogger).Name.Should().Be("Given_a_app_configured_logger_factory.When_creating_a_logger");
        }
    }
}