using AstroBackend.Application.Astrology;
using Xunit;

namespace AstroBackend.Tests;

/// <summary>
/// NumerologyEngine saf (xarici asılılığı olmayan) hesablama məntiqi üçün testlər.
/// Dəyərlər əl ilə yoxlanılıb (bax: komment-lərdə izah) ki, test gözləntiləri də səhv olmasın.
/// </summary>
public class NumerologyEngineTests
{
    [Theory]
    [InlineData(5, 5)]      // artıq birrəqəmli — dəyişməz
    [InlineData(16, 7)]     // 1+6=7
    [InlineData(38, 11)]    // 3+8=11 → master ədəd, keepMaster=true (defolt) ilə saxlanılır
    [InlineData(29, 11)]    // 2+9=11 → master ədəd saxlanılır
    public void ReduceNumber_KeepsMasterNumbers_ByDefault(int input, int expected)
    {
        Assert.Equal(expected, NumerologyEngine.ReduceNumber(input));
    }

    [Theory]
    [InlineData(38, 2)]     // 3+8=11 → keepMaster=false → 1+1=2
    [InlineData(29, 2)]     // 2+9=11 → keepMaster=false → 1+1=2
    public void ReduceNumber_ReducesMasterNumbers_WhenKeepMasterIsFalse(int input, int expected)
    {
        Assert.Equal(expected, NumerologyEngine.ReduceNumber(input, keepMaster: false));
    }

    [Fact]
    public void Compute_ReturnsExpectedValues_ForKnownNameAndDate()
    {
        // "Ali" + "1990-05-15" üçün dəyərlər əl ilə hesablanıb:
        // LifePath: rəqəmlərin cəmi (1+9+9+0+0+5+1+5=30) → 3+0=3
        // Destiny:  a(1)+l(3)+i(9)=13 → 1+3=4
        // SoulUrge: sait hərflər a(1)+i(9)=10 → 1+0=1
        // Personality: samit hərf l(3) → 3
        // Birthday: gün 15 → 1+5=6
        var result = NumerologyEngine.Compute("Ali", "1990-05-15");

        Assert.Equal(3, result.LifePath);
        Assert.Equal(4, result.Destiny);
        Assert.Equal(1, result.SoulUrge);
        Assert.Equal(3, result.Personality);
        Assert.Equal(6, result.Birthday);
        Assert.Equal(5, result.Details.Count);
    }

    [Fact]
    public void Compute_AlwaysReturnsValuesThatAreSingleDigitOrMasterNumbers()
    {
        var result = NumerologyEngine.Compute("Shirin Alizada", "1995-11-22");

        int[] values = { result.LifePath, result.Destiny, result.SoulUrge, result.Personality, result.Birthday };
        foreach (var value in values)
        {
            Assert.True(value is >= 1 and <= 9 or 11 or 22 or 33,
                $"Dəyər ({value}) birrəqəmli (1-9) və ya master ədəd (11/22/33) olmalıdır.");
        }
    }
}
