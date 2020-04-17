using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using OilTraderAI.Class;
using OilTraderAI.Model;
using OilTradeAI.Misc;
using System.IO;
using System.Reflection;
using System.Text;

namespace OilTraderAI.Misc
{
    public static class Helper
    {
        public static void CreateCsv()
        {
            //0 - Create a StringBuilder output
            var csv = new StringBuilder();

            //1 - Add header to output csv
            PropertyInfo[] propertyInfos;
            propertyInfos = typeof(IntradayTransfer).GetProperties();
            var headerLine = "";
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                headerLine += propertyInfo.Name + ",";
            }
            headerLine = headerLine.Substring(0, headerLine.Length - 1);
            csv.Append(headerLine);
            csv.AppendLine();


            //2-Get data from db and push to output
            List<IntradayTransfer> priceList = GetDataFromDb();

            foreach (var item in priceList)
            {
                csv.Append(item.Price.ToString().Replace(",", ".") + "," +
                item.Rsi.ToString().Replace(",", ".") + "," +
                item.Macd.ToString().Replace(",", ".") + "," +
                item.MacdSign.ToString().Replace(",", ".") + "," +
                item.MacdHist.ToString().Replace(",", ".") + "," +
                item.Future.ToString().Replace(",", "."));
                csv.AppendLine();
            }

            //3 - Create output file name
            string resultFileName = string.Format("Oil-TrainData.csv");

            //4 - save file to drive
            var resultFilePath = string.Format("{0}/Csv/{1}", Environment.CurrentDirectory, resultFileName);
            File.WriteAllText(resultFilePath, csv.ToString());

            //3-save csv and print the file name
            Console.WriteLine(resultFileName);
        }
        
        ///We remove identical lines
        public static List<IntradayTransfer> GetDataFromDb()
        {
            List<IntradayTransfer> intradayList = new List<IntradayTransfer>();
            double previousPrice = -999;
            using (var db = new ApplicationDbContext())
            {
                //3 - We add each item to our final list (26 first doesn;t contain RSI neither MACD calulation)
                foreach (var item in db.Intraday)
                {
                    if(item.P == previousPrice) continue;
                    IntradayTransfer newIntraday = new IntradayTransfer()
                    {
                        Price = item.P,
                    };
                    intradayList.Add(newIntraday);
                    previousPrice = item.P;
                }
            }

            //Calculate change from next day to current day
            intradayList.Where((p, index) => CalculateFuture(p, index, intradayList)).ToList();

            //Add RSI calculation to the list
            TradeIndicator.CalculateRsiList(14, ref intradayList);
            TradeIndicator.CalculateMacdList(ref intradayList);

            return intradayList.Skip(26).ToList();
        }

        private static bool CalculateFuture(IntradayTransfer p, int index, List<IntradayTransfer> intradayList)
        {
            ///Used to do mlti-classification alo / not working well
            // if (index > intradayList.Count - 4) return true;
            // int b1 = 0;
            // int b2 = 0;
            // if (intradayList[index + 1].Price - p.Price > 0) {b1 = 1;} else {b1 = -1;};
            // if (intradayList[index + 2].Price - intradayList[index + 1].Price > 0) {b2 = 1;} else {b2 = -1;};

            // if (b1 + b2 == 2)
            // {
            //     p.Future = 1;
            //     return true;
            // }

            // if (b1 + b2 == -2)
            // {
            //     p.Future = -1;
            //     return true;
            // }

            // p.Future = 0;
            // return true;

            // if (index > intradayList.Count - 4) return true;
            // int b1 = 0;
            // int b2 = 0;
            // if (intradayList[index + 1].Price - p.Price > 0) {b1 = 1;} else {b1 = -1;};
            // if (intradayList[index + 2].Price - intradayList[index + 1].Price > 0) {b2 = 1;} else {b2 = -1;};

            // if (b1 + b2 == 2)
            // {
            //     p.Future = 1;
            //     return true;
            // }

            // if (b1 + b2 == -2)
            // {
            //     p.Future = -1;
            //     return true;
            // }
            if (index < 1) return true;
            if (index > intradayList.Count - 2) return true;

            p.Future = intradayList[index -1].Price - p.Price;
            return true;
        }
    }
}