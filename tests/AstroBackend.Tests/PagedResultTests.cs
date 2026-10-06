using AstroBackend.Application.DTOs;
using Xunit;

namespace AstroBackend.Tests;

/// <summary>PagedResult&lt;T&gt;.TotalPages hesablamasının sərhəd hallarını yoxlayan testlər.</summary>
public class PagedResultTests
{
    [Theory]
    [InlineData(25, 10, 3)]   // tam bölünməyən say yuxarı yuvarlaqlaşır
    [InlineData(20, 10, 2)]   // tam bölünən say
    [InlineData(0, 10, 0)]    // heç bir element yoxdur
    [InlineData(1, 10, 1)]    // bir element belə bir səhifə tələb edir
    public void TotalPages_ComputesCeilingDivision(int totalCount, int pageSize, int expected)
    {
        var result = new PagedResult<string>(new List<string>(), Page: 1, PageSize: pageSize, TotalCount: totalCount);

        Assert.Equal(expected, result.TotalPages);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void TotalPages_IsZero_WhenPageSizeIsNotPositive(int pageSize)
    {
        var result = new PagedResult<string>(new List<string>(), Page: 1, PageSize: pageSize, TotalCount: 100);

        Assert.Equal(0, result.TotalPages);
    }
}
