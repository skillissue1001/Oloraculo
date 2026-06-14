namespace Oloraculo.Web.Probability
{
    public class ScorelineDistribution
    {
        public int MaxGoals { get; init; }
        public double[,] Matrix { get; init; } = new double[0, 0];

        public double Probability(int home, int away) =>
            home <= MaxGoals && away <= MaxGoals ? Matrix[home, away] : 0;

        public double ExpectedTotalGoals()
        {
            double expected = 0;
            for (var h = 0; h <= MaxGoals; h++)
                for (var a = 0; a <= MaxGoals; a++)
                    expected += (h + a) * Matrix[h, a];

            return expected;
        }

        public double ProbabilityTotalGoalsAtLeast(int total)
        {
            if (total <= 0)
                return 1.0;

            double probability = 0;
            for (var h = 0; h <= MaxGoals; h++)
                for (var a = 0; a <= MaxGoals; a++)
                    if (h + a >= total)
                        probability += Matrix[h, a];

            return probability;
        }

        public (int Home, int Away) RepresentativeScoreline()
        {
            var targetTotal = (int)Math.Round(ExpectedTotalGoals(), MidpointRounding.AwayFromZero);
            if (targetTotal < 0 || targetTotal > MaxGoals * 2)
                return MostLikelyScoreline();

            var best = (Home: 0, Away: 0, Probability: -1.0);
            for (var h = 0; h <= MaxGoals; h++)
            {
                for (var a = 0; a <= MaxGoals; a++)
                {
                    if (h + a == targetTotal && Matrix[h, a] > best.Probability)
                        best = (h, a, Matrix[h, a]);
                }
            }

            return best.Probability >= 0 ? (best.Home, best.Away) : MostLikelyScoreline();
        }

        public OutcomeProbabilities ToOutcome()
        {
            double homeWin = 0, draw = 0, awayWin = 0;

            for (var h = 0; h <= MaxGoals; h++)
            {
                for (var a = 0; a <= MaxGoals; a++)
                {
                    if (h > a)
                    {
                        homeWin += Matrix[h, a];
                    }
                    else if (h == a)
                    {
                        draw += Matrix[h, a];
                    }
                    else
                    {
                        awayWin += Matrix[h, a];
                    }
                }
            }
            return new OutcomeProbabilities(homeWin, draw, awayWin).Normalize();
        }

        public (int Home, int Away) MostLikelyScoreline()
        {
            var best = (Home: 0, Away: 0, Probability: -1.0);
            for (var h = 0; h <= MaxGoals; h++)
            {
                for (var a = 0; a <= MaxGoals; a++)
                {
                    if (Matrix[h, a] > best.Probability)
                    {
                        best = (h, a, Matrix[h, a]);
                    }
                }
            }
            return (best.Home, best.Away);
        }
    }
}
