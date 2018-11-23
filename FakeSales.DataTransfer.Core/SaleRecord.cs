using System;
using System.Collections.Generic;
using System.Text;

namespace FakeSales.DataTransfer.Core
{
    public class SaleRecord
    {
        public int StoreNo { get; set; }
        public DateTime TimeStamp { get; set; }
        public double TotalValueEclGST { get; set; }
        public int NumberOfChicken { get; set; }
        public int NumberApple { get; set; }
        public int NumberOfBanana { get; set; }
        public int NumberOfBeer { get; set; }
        public int HourOfDay { get; set; }
        public int MinuteOfHour { get; set; }
    }
}
