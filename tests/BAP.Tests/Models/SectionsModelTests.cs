using BAP.Core.Models;

namespace BAP.Tests.Models;

public class SectionsModelTests
{
    [Fact]
    public void FindSection_ReturnsSectionByName()
    {
        var model = new SectionsModel
        {
            Sections =
            [
                new SectionModel { Name = "Station" },
                new SectionModel { Name = "Bridge" }
            ]
        };

        var found = model.FindSection("Bridge");

        Assert.NotNull(found);
        Assert.Equal("Bridge", found.Name);
    }

    [Fact]
    public void FindSection_ReturnsNull_WhenNotFound()
    {
        var model = new SectionsModel
        {
            Sections = [new SectionModel { Name = "Station" }]
        };

        Assert.Null(model.FindSection("Tunnel"));
    }

    [Fact]
    public void FindSectionIndex_ReturnsCorrectIndex()
    {
        var model = new SectionsModel
        {
            Sections =
            [
                new SectionModel { Name = "A" },
                new SectionModel { Name = "B" },
                new SectionModel { Name = "C" }
            ]
        };

        Assert.Equal(1, model.FindSectionIndex("B"));
        Assert.Equal(-1, model.FindSectionIndex("Z"));
    }
}
