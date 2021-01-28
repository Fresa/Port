using FluentAssertions;
using Spdy.Primitives;
using Test.It.With.XUnit;
using Xunit;

namespace Spdy.UnitTests.Primitives
{
    public class Given_a_uint24
    {
        public class When_getting_value : XUnit2Specification
        {
            private UInt24 _uint24;
            private uint _value;

            protected override void Given()
            {
                // 0100 1100 0100 1011 0100 0000
                _uint24 = UInt24.From(5000000);
            }

            protected override void When()
            {
                _value = _uint24.Value;
            }

            [Fact]
            public void Value_should_be_same_as_input()
            {
                _value.Should()
                    .Be(5000000);
            }

            [Fact]
            public void First_byte_should_be_least_significant()
            {
                _uint24.One.Should()
                    .Be(64);
            }

            [Fact]
            public void Second_byte_should_be_middle_significant()
            {
                _uint24.Two.Should()
                    .Be(75);
            }

            [Fact]
            public void Third_byte_should_be_most_significant()
            {
                _uint24.Three.Should()
                    .Be(76);
            }
        }
    }
}