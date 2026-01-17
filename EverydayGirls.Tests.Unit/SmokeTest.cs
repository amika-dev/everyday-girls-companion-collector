using Xunit;
using FluentAssertions;

namespace EverydayGirls.Tests.Unit;

public class SmokeTest
{
    [Fact]
    public void Environment_Is_Configured_Correctly()
    {
        true.Should().BeTrue();
    }
}
