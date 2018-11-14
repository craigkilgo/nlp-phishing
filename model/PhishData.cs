using Microsoft.ML.Runtime.Api;


namespace model
{
    public class PhishData
    {
        [Column(ordinal: "0", name: "Label")]
        public float Phish;

        [Column(ordinal: "1")]
        public string UrlText;
    }

    public class PhishPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        [ColumnName("Probability")]
        public float Probability { get; set; }

        [ColumnName("Score")]
        public float Score { get; set; }
    }

}