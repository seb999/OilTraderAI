using Microsoft.ML.Data;

namespace OilTraderAI.Class
{
    public class ModelOutput
    {
        [ColumnName("Score")]
        public float Future { get; set; }
    }
}