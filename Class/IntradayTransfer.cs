using System;
using Microsoft.ML.Data;

namespace OilTraderAI.Class
{
    public class IntradayTransfer
    {
        public double Price { get; set; }
        public double Rsi { get; set; }
        public double Macd { get; set; }
        public double MacdSign { get; set; }
        public double MacdHist { get; set; }
        public double Future { get; set; }
    }
}
