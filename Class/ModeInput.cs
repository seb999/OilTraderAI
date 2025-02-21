using Microsoft.ML.Data;

namespace OilTraderAI.Class
{
    public class ModelInput
    {
        [LoadColumn(0)]
        public float Price { get; set; }
        [LoadColumn(1)]
        public float Rsi { get; set; }
        [LoadColumn(2)]
        public float Macd { get; set; }
        [LoadColumn(3)]
        public float MacdSign { get; set; }
        [LoadColumn(4)]
        public float MacdHist { get; set; }
        [LoadColumn(5)]
        public float Future { get; set; }
    }
}