using TMModels;

namespace TMDraftsGenerator
{
    internal class DraftOptions : DraftScoreWeights
    {
        public int Players { get; set; } = 3;
        public int Houses { get; set; } = 6;
        public string ResultsPath { get; set; } = "results";
    }
}
