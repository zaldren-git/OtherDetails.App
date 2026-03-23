   public class OthersMeal
    {
        public int ID { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class OthersSeat
    {
        public int ID { get; set; }
        public string Seat { get; set; } = string.Empty;
    }

    public class OthersInsurance
    {
        public int ID { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class OthersBaggage
    {
        public int ID { get; set; }
        public decimal Weight { get; set; }
    }

    public class OtherDetails
    {
        public int OthersID { get; set; }
        public int BookingID { get; set; }
        public int MealID { get; set; }
        public int InsuranceID { get; set; }
        public int SeatID { get; set; }
        public int BaggageID { get; set; }
        public int PaxID { get; set; }
    }