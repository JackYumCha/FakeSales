using System;
using System.Collections.Generic;
using System.Text;

namespace FakeSales.Generator
{
    public class SaleRecord
    {
        public string StoreNo { get; set; }
        public DateTime TimeStamp { get; set; }
        public decimal TotalValueEclGST { get; set; }
        public int NumberOfChicken { get; set; }
        public int NumberApple { get; set; }
        public int NumberOfBanana { get; set; }
        public int NumberOfBeer { get; set; }
        public int HourOfDay { get; set; }
        public int MinuteOfHour { get; set; }
    }
}
