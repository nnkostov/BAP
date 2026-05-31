using BAP.Core.Models;

namespace BAP.Tests.Models;

public class PathModelTests
{
    [Fact]
    public void FromString_ParsesCommaSeparatedIndices()
    {
        var path = new PathModel();

        Assert.True(path.FromString("0, 1, 2, 3"));
        Assert.Equal([0, 1, 2, 3], path.Sections);
    }

    [Fact]
    public void FromString_ReturnsFalse_ForInvalidInput()
    {
        var path = new PathModel();

        Assert.False(path.FromString("a, b, c"));
    }

    [Fact]
    public void SectionsToString_ReturnsCommaSeparated()
    {
        var path = new PathModel { Sections = [5, 10, 15] };

        Assert.Equal("5, 10, 15", path.SectionsToString());
    }

    [Fact]
    public void SectionsToString_ReturnsEmpty_WhenNoSections()
    {
        var path = new PathModel { Sections = [] };

        Assert.Equal(string.Empty, path.SectionsToString());
    }
}
