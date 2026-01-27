namespace Services.Helpers
{
    public class Sm2Result
    {
        public int Repetitions { get; set; }
        public double EasinessFactor { get; set; }
        public int IntervalDays { get; set; }
        public DateTime NextReviewAt { get; set; }
    }

    public static class Sm2Calc
    {
        public static Sm2Result CalculatNewState (
            int grade,
            int repetitions,
            double easinessFactor,
            int intervalDays
            )
        {
            int newRepetitions, newIntervalDays;
            double newEasinessFactor;

            if ( grade >= 3)
            {
                newRepetitions = repetitions + 1;

                if (newRepetitions == 1)
                {
                    newIntervalDays = 1;
                }
                else if ( newRepetitions == 2 )
                {
                    newIntervalDays = 6;
                }
                else 
                {
                    newIntervalDays = (int)Math.Round(intervalDays * easinessFactor);
                }

                // EF' = EF + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.02))
                // q = grade
                newEasinessFactor = easinessFactor + (0.1 - (5 - grade) * (0.08 + (5 - grade) * 0.02));

                if (newEasinessFactor < 1.3) newEasinessFactor = 1.3;
            }
            else
            {
                newRepetitions = 0;
                newIntervalDays = 1;
                newEasinessFactor = easinessFactor;
            }

            return new Sm2Result
            {
                Repetitions = newRepetitions,
                EasinessFactor = newEasinessFactor,
                IntervalDays = newIntervalDays,
                NextReviewAt = DateTime.UtcNow.AddDays(newIntervalDays)
            };
        }
    }
}
