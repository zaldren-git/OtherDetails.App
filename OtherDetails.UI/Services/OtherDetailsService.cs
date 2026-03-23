// Services/OtherDetailsService.cs
using System.Net.Http.Json;
using OthersDetails.App.Models;

namespace OthersDetails.App.Services
{
    public class OtherDetailsService
    {
        private readonly HttpClient _http;

        public OtherDetailsService(HttpClient http)
        {
            _http = http;
        }

        // ── OthersDetails ─────────────────────────────────────────────────
        public async Task<List<OthersDetailsModel>> GetAllOthersDetailsAsync()
        {
            try { return await _http.GetFromJsonAsync<List<OthersDetailsModel>>("api/otherdetails") ?? new(); }
            catch { return new(); }
        }

        public async Task<OthersDetailsModel?> GetOthersDetailsByIdAsync(int id)
        {
            try { return await _http.GetFromJsonAsync<OthersDetailsModel>($"api/otherdetails/{id}"); }
            catch { return null; }
        }

        public async Task<bool> UpdateOthersDetailsAsync(int id, OthersDetailsModel details)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/otherdetails/{id}", details);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // ── Meals ─────────────────────────────────────────────────────────
        public async Task<List<OthersMealModel>> GetAllMealsAsync()
        {
            try { return await _http.GetFromJsonAsync<List<OthersMealModel>>("api/otherdetails/meals") ?? new(); }
            catch { return new(); }
        }

        // ── Seats ─────────────────────────────────────────────────────────
        public async Task<List<OthersSeatModel>> GetAllSeatsAsync()
        {
            try { return await _http.GetFromJsonAsync<List<OthersSeatModel>>("api/otherdetails/seats") ?? new(); }
            catch { return new(); }
        }

        // ── Insurance ─────────────────────────────────────────────────────
        public async Task<List<OthersInsuranceModel>> GetAllInsuranceAsync()
        {
            try { return await _http.GetFromJsonAsync<List<OthersInsuranceModel>>("api/otherdetails/insurance") ?? new(); }
            catch { return new(); }
        }

        // ── Baggage ───────────────────────────────────────────────────────
        public async Task<List<OthersBaggageModel>> GetAllBaggageAsync()
        {
            try { return await _http.GetFromJsonAsync<List<OthersBaggageModel>>("api/otherdetails/baggage") ?? new(); }
            catch { return new(); }
        }

        // ── Flights ───────────────────────────────────────────────────────
        // GET api/flights
        // Returns Flight[] — each item already has OriginCode, OriginName,
        // DestinationCode, DestinationName, AirlineName embedded (from FlightRepository JOIN)
        public async Task<List<FlightApiModel>> GetAllFlightsAsync()
        {
            try { return await _http.GetFromJsonAsync<List<FlightApiModel>>("api/flights") ?? new(); }
            catch { return new(); }
        }

        // ── Booking Details ───────────────────────────────────────────────
        // GET api/bookings/details/{bookingId}
        // Returns List<BookingDetail> — each item has:
        //   BookingID, PaxID, FullName, Contact, FlightID, TravelDate, TravelTime
        public async Task<List<BookingDetailApiModel>> GetBookingDetailsAsync(int bookingId)
        {
            try { return await _http.GetFromJsonAsync<List<BookingDetailApiModel>>($"api/bookings/details/{bookingId}") ?? new(); }
            catch { return new(); }
        }
    }

    // ── Exact match to API's Flight model (FlightRepository.MapFlight) ────
    public class FlightApiModel
    {
        public int FlightID { get; set; }
        public int OriginID { get; set; }
        public int DestinationID { get; set; }
        public int AirlineID { get; set; }
        public string TravelDate { get; set; } = string.Empty;
        public string TravelTime { get; set; } = string.Empty;
        public int MaxPax { get; set; }
        public int CurrentPax { get; set; }

        // These come from the JOIN in FlightRepository — use them directly!
        public string AirlineCode { get; set; } = string.Empty;
        public string AirlineName { get; set; } = string.Empty;
        public string OriginCode { get; set; } = string.Empty;
        public string OriginName { get; set; } = string.Empty;
        public string DestinationCode { get; set; } = string.Empty;
        public string DestinationName { get; set; } = string.Empty;

        // Convenience helpers
        public string OriginLabel => $"{OriginCode} – {OriginName}";
        public string DestinationLabel => $"{DestinationCode} – {DestinationName}";
    }

    // ── Exact match to API's BookingDetail model ──────────────────────────
    // GET api/bookings/details/{bookingId} returns List<BookingDetail>
    public class BookingDetailApiModel
    {
        public int BookingID { get; set; }
        public int PaxID { get; set; }
        public string FullName { get; set; } = string.Empty;  // "FirstName LastName"
        public string Contact { get; set; } = string.Empty;
        public int FlightID { get; set; }
        public string TravelDate { get; set; } = string.Empty;
        public string TravelTime { get; set; } = string.Empty;
        // Note: Birthdate and Age are NOT returned by this endpoint
        // They are in Booking_Pax but the BookingDetail query doesn't select them
    }
}