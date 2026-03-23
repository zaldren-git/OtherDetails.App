// Models/OtherDetailsModels.cs
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OthersDetails.App.Models
{
    // ── Add-on lookup models ────────────────────────────────────────────────

    public class OthersMealModel
    {
        public int ID { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class OthersSeatModel
    {
        public int ID { get; set; }
        public string Seat { get; set; } = string.Empty;
    }

    public class OthersInsuranceModel
    {
        public int ID { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class OthersBaggageModel
    {
        public int ID { get; set; }
        public decimal Weight { get; set; }
    }

    public class OthersDetailsModel
    {
        public int OthersID { get; set; }
        public int BookingID { get; set; }
        public int MealID { get; set; }
        public int InsuranceID { get; set; }
        public int SeatID { get; set; }
        public int BaggageID { get; set; }
        public int PaxID { get; set; }
    }

    // ── View Models ─────────────────────────────────────────────────────────

    public class OtherDetailsDashboardRow
    {
        public int OthersID { get; set; }
        public int BookingID { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public int PaxID { get; set; }
        public string PaxName { get; set; } = string.Empty;
        public string MealType { get; set; } = string.Empty;
        public string SeatLabel { get; set; } = string.Empty;
        public string InsuranceType { get; set; } = string.Empty;
        public string BaggageWeight { get; set; } = string.Empty;

        public string ActiveTypes
        {
            get
            {
                var parts = new List<string>();
                if (!string.IsNullOrEmpty(MealType)) parts.Add("Meal");
                if (!string.IsNullOrEmpty(SeatLabel)) parts.Add("Seat");
                if (!string.IsNullOrEmpty(InsuranceType)) parts.Add("Insurance");
                if (!string.IsNullOrEmpty(BaggageWeight)) parts.Add("Baggage");
                return parts.Count > 0 ? string.Join(", ", parts) : "None";
            }
        }
    }

    public class OtherDetailsSummary
    {
        public int TotalRecords { get; set; }
        public int WithMeal { get; set; }
        public int WithSeat { get; set; }
        public int WithInsurance { get; set; }
        public int WithBaggage { get; set; }
        public int NoAddOns { get; set; }

        public Dictionary<string, int> MealBreakdown { get; set; } = new();
        public Dictionary<string, int> InsuranceBreakdown { get; set; } = new();
        public Dictionary<string, int> BaggageBreakdown { get; set; } = new();
    }

    public class OtherDetailsDashboardViewModel
    {
        public OtherDetailsSummary Summary { get; set; } = new();
        public List<OtherDetailsDashboardRow> Rows { get; set; } = new();
    }

    public class OtherDetailsEditViewModel
    {
        public OthersDetailsModel Details { get; set; } = new();

        // Current resolved labels shown before editing
        public string CurrentMeal { get; set; } = string.Empty;
        public string CurrentSeat { get; set; } = string.Empty;
        public string CurrentInsurance { get; set; } = string.Empty;
        public string CurrentBaggage { get; set; } = string.Empty;

        // Passenger info — from BookingDetail.FullName and Contact
        // Birthdate/Age are NOT available from the API (not in the query)
        public string PaxName { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;

        // Flight info — all come directly from FlightApiModel JOIN fields
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string TravelDate { get; set; } = string.Empty;
        public string TravelTime { get; set; } = string.Empty;
        public string Airline { get; set; } = string.Empty;

        // Dropdowns
        public SelectList MealSelectList { get; set; } = new SelectList(Enumerable.Empty<object>());
        public SelectList SeatSelectList { get; set; } = new SelectList(Enumerable.Empty<object>());
        public SelectList InsuranceSelectList { get; set; } = new SelectList(Enumerable.Empty<object>());
        public SelectList BaggageSelectList { get; set; } = new SelectList(Enumerable.Empty<object>());
    }
}