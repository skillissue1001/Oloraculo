using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Oloraculo.Web;
using Oloraculo.Web.DAL;
using Oloraculo.Web.Helpers;
using Oloraculo.Web.Models;
using Oloraculo.Web.Models.ApiFootballModels;
using Oloraculo.Web.Models.CsvModels;
using Oloraculo.Web.Predictors;
using Oloraculo.Web.Probability;
using Oloraculo.Web.Services;
using Oloraculo.Web.Services.Simulation;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace Oloraculo.Web.Tests;

public class ProbabilityTests : TestFixtures
{
    [Fact]
    public void OutcomeProbabilities_NormalizesAndUsesOutcomeLabels()
    {
        var p = new OutcomeProbabilities(2, 1, 1).Normalize();

        Assert.True(p.IsValid);
        Assert.Equal(0.5, p.HomeWin, 3);
        Assert.Equal("Home", p.TopPick);
    }

    [Fact]
    public void OutcomeFromExpectation_TreatsEqualMagnitudeGapsSymmetrically()
    {
        var strongerHome = ProbabilityHelper.OutcomeFromExpectation(.78, 400);
        var strongerAway = ProbabilityHelper.OutcomeFromExpectation(.22, -400);

        Assert.Equal(strongerHome.Draw, strongerAway.Draw, 6);
    }

    [Fact]
    public void PoissonScoreline_ProducesARealProbabilityGrid()
    {
        var dist = ProbabilityHelper.PoissonScoreline(2.2, .7);
        var sum = 0.0;
        for (var h = 0; h <= dist.MaxGoals; h++)
            for (var a = 0; a <= dist.MaxGoals; a++)
                sum += dist.Probability(h, a);

        Assert.Equal(1.0, sum, 6);
        Assert.True(dist.ToOutcome().HomeWin > dist.ToOutcome().AwayWin);
        Assert.NotEqual((0, 0), dist.MostLikelyScoreline());
    }

    [Fact]
    public void ScorelineDistribution_SummarizesGoalTotals()
    {
        var dist = ProbabilityHelper.PoissonScoreline(1.34, 1.28);
        var representative = dist.RepresentativeScoreline();

        Assert.InRange(dist.ExpectedTotalGoals(), 2.5, 2.7);
        Assert.Equal(1.0, dist.ProbabilityTotalGoalsAtLeast(0), 6);
        Assert.Equal(0.0, dist.ProbabilityTotalGoalsAtLeast((dist.MaxGoals * 2) + 1), 6);
        Assert.Equal(
            (int)Math.Round(dist.ExpectedTotalGoals(), MidpointRounding.AwayFromZero),
            representative.Home + representative.Away);
        Assert.NotEqual(dist.MostLikelyScoreline(), representative);
    }

}
