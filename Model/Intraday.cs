using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OilTraderAI.Model
{
    public class Intraday
    {
        [Key]
        public int Id { get; set; }
        public double P { get; set; }
        public double V { get; set; }
        public string dateAdded { get; set; }
    }
}