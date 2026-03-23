// Controllers/OtherDetailsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OthersDetails.App.Models;
using OthersDetails.App.Services;

namespace OthersDetails.App.Controllers
{
    public class OtherDetailsController : Controller
    {
        private readonly OtherDetailsService _service;

        public OtherDetailsController(OtherDetailsService service)
        {
            _service = service;
        }

        // ─────────────────────────────────────────────────────────────────
        // Shared builder — called by both Index and List
        // Key insight from the API source code:
        //   - api/flights returns Flight objects with OriginCode, OriginName,
        //     DestinationCode, DestinationName, AirlineName already joined
        //   - api/bookings/details/{id} returns List<BookingDetail> with
        //     FlightID, PaxID, FullName, Contact, TravelDate, TravelTime
        // So we: fetch all flights into a dict by FlightID,
        //        fetch booking details per unique BookingID (cached),
        //        use bookingDetail.FlightID to look up the flight directly.
        // ─────────────────────────────────────────────────────────────────
        private async Task<(List<OtherDetailsDashboardRow> rows, OtherDetailsSummary summary)> BuildRowsAsync()
        {
            // Fetch add-on lookups + all flights in parallel
            var odTask = _service.GetAllOthersDetailsAsync();
            var mealsTask = _service.GetAllMealsAsync();
            var seatsTask = _service.GetAllSeatsAsync();
            var insTask = _service.GetAllInsuranceAsync();
            var bagTask = _service.GetAllBaggageAsync();
            var flightTask = _service.GetAllFlightsAsync();

            await Task.WhenAll(odTask, mealsTask, seatsTask, insTask, bagTask, flightTask);

            var allOD = odTask.Result;
            var meals = mealsTask.Result.ToDictionary(m => m.ID);
            var seats = seatsTask.Result.ToDictionary(s => s.ID);
            var insurances = insTask.Result.ToDictionary(i => i.ID);
            var baggages = bagTask.Result.ToDictionary(b => b.ID);

            // Key: FlightID → FlightApiModel (has OriginCode, OriginName etc already)
            var flights = flightTask.Result.ToDictionary(f => f.FlightID);

            var rows = new List<OtherDetailsDashboardRow>();
            var summary = new OtherDetailsSummary { TotalRecords = allOD.Count };

            // Cache: BookingID → List<BookingDetailApiModel>
            var bookingCache = new Dictionary<int, List<BookingDetailApiModel>>();

            foreach (var od in allOD)
            {
                // ── Booking details (cached per bookingId) ────────────────
                if (!bookingCache.ContainsKey(od.BookingID))
                    bookingCache[od.BookingID] = await _service.GetBookingDetailsAsync(od.BookingID);

                var bookingRows = bookingCache[od.BookingID];

                // Find the specific pax row for this OthersDetails record
                var paxRow = od.PaxID > 0
                    ? bookingRows.FirstOrDefault(r => r.PaxID == od.PaxID)
                    : bookingRows.FirstOrDefault();

                // ── Route — from the flight the booking is on ─────────────
                string origin = "—", destination = "—";
                int flightId = paxRow?.FlightID ?? 0;

                if (flightId > 0 && flights.TryGetValue(flightId, out var flight))
                {
                    // These come directly from the JOIN in FlightRepository
                    origin = flight.OriginLabel;       // "MNL – Ninoy Aquino..."
                    destination = flight.DestinationLabel;  // "CEB – Mactan-Cebu..."
                }

                // ── Pax name ──────────────────────────────────────────────
                string paxName = paxRow?.FullName ?? (od.PaxID > 0 ? $"Pax #{od.PaxID}" : "—");

                // ── Add-on labels ─────────────────────────────────────────
                string mealType = od.MealID > 0 && meals.TryGetValue(od.MealID, out var m) ? m.Type : string.Empty;
                string seatLabel = od.SeatID > 0 && seats.TryGetValue(od.SeatID, out var s) ? s.Seat : string.Empty;
                string insuranceType = od.InsuranceID > 0 && insurances.TryGetValue(od.InsuranceID, out var ins) ? ins.Type : string.Empty;
                string baggageWeight = od.BaggageID > 0 && baggages.TryGetValue(od.BaggageID, out var bag) ? $"{bag.Weight} kg" : string.Empty;

                rows.Add(new OtherDetailsDashboardRow
                {
                    OthersID = od.OthersID,
                    BookingID = od.BookingID,
                    Origin = origin,
                    Destination = destination,
                    PaxID = od.PaxID,
                    PaxName = paxName,
                    MealType = mealType,
                    SeatLabel = seatLabel,
                    InsuranceType = insuranceType,
                    BaggageWeight = baggageWeight,
                });

                // ── Summary counters ──────────────────────────────────────
                bool hasMeal = !string.IsNullOrEmpty(mealType);
                bool hasSeat = !string.IsNullOrEmpty(seatLabel);
                bool hasInsurance = !string.IsNullOrEmpty(insuranceType);
                bool hasBaggage = !string.IsNullOrEmpty(baggageWeight);

                if (hasMeal)
                {
                    summary.WithMeal++;
                    summary.MealBreakdown[mealType] = summary.MealBreakdown.GetValueOrDefault(mealType) + 1;
                }
                if (hasSeat) summary.WithSeat++;
                if (hasInsurance)
                {
                    summary.WithInsurance++;
                    summary.InsuranceBreakdown[insuranceType] = summary.InsuranceBreakdown.GetValueOrDefault(insuranceType) + 1;
                }
                if (hasBaggage)
                {
                    summary.WithBaggage++;
                    summary.BaggageBreakdown[baggageWeight] = summary.BaggageBreakdown.GetValueOrDefault(baggageWeight) + 1;
                }
                if (!hasMeal && !hasSeat && !hasInsurance && !hasBaggage)
                    summary.NoAddOns++;
            }

            return (rows, summary);
        }

        // ─────────────────────────────────────────────────────────────────
        // GET: /OtherDetails  →  Dashboard with charts
        // ─────────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var (rows, summary) = await BuildRowsAsync();
            return View(new OtherDetailsDashboardViewModel { Summary = summary, Rows = rows });
        }

        // ─────────────────────────────────────────────────────────────────
        // GET: /OtherDetails/List  →  Searchable table
        // ─────────────────────────────────────────────────────────────────
        public async Task<IActionResult> List()
        {
            var (rows, _) = await BuildRowsAsync();
            if (TempData["Success"] != null) ViewBag.Success = TempData["Success"];
            if (TempData["Error"] != null) ViewBag.Error = TempData["Error"];
            return View(rows);
        }

        // ─────────────────────────────────────────────────────────────────
        // GET: /OtherDetails/Edit/5
        // ─────────────────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            var details = await _service.GetOthersDetailsByIdAsync(id);
            if (details == null)
            {
                TempData["Error"] = $"Record #{id} not found.";
                return RedirectToAction(nameof(List));
            }

            // Fetch lookups + all flights in parallel
            var mealsTask = _service.GetAllMealsAsync();
            var seatsTask = _service.GetAllSeatsAsync();
            var insTask = _service.GetAllInsuranceAsync();
            var bagTask = _service.GetAllBaggageAsync();
            var flightTask = _service.GetAllFlightsAsync();

            await Task.WhenAll(mealsTask, seatsTask, insTask, bagTask, flightTask);

            var meals = mealsTask.Result;
            var seats = seatsTask.Result;
            var insurances = insTask.Result;
            var baggages = bagTask.Result;
            var flights = flightTask.Result.ToDictionary(f => f.FlightID);

            // ── GET booking details — returns List<BookingDetail> ─────────
            var bookingRows = await _service.GetBookingDetailsAsync(details.BookingID);

            // Find the row matching this specific pax
            var paxRow = details.PaxID > 0
                ? bookingRows.FirstOrDefault(r => r.PaxID == details.PaxID)
                : bookingRows.FirstOrDefault();

            // ── Flight info — from FlightApiModel JOIN fields ──────────────
            string origin = "—", destination = "—", travelDate = "—", travelTime = "—", airline = "—";
            int flightId = paxRow?.FlightID ?? 0;

            if (flightId > 0 && flights.TryGetValue(flightId, out var flight))
            {
                origin = flight.OriginLabel;       // e.g. "MNL – Ninoy Aquino International Airport"
                destination = flight.DestinationLabel;  // e.g. "CEB – Mactan-Cebu International Airport"
                airline = flight.AirlineName;       // e.g. "Cebu Pacific Air"
                travelDate = flight.TravelDate;
                travelTime = flight.TravelTime;
            }

            // ── Pax info — from BookingDetail ─────────────────────────────
            // Note: BookingDetail only has FullName and Contact (no Birthdate/Age in query)
            string paxName = paxRow?.FullName ?? (details.PaxID > 0 ? $"Pax #{details.PaxID}" : "—");
            string contact = paxRow?.Contact ?? string.Empty;

            // ── Build SelectLists with current values pre-selected ─────────
            var noneItem = new SelectListItem { Value = "0", Text = "— None —" };

            var mealItems = meals.Select(m => new SelectListItem
            { Value = m.ID.ToString(), Text = m.Type, Selected = m.ID == details.MealID }).ToList();
            mealItems.Insert(0, noneItem);

            var seatItems = seats.Select(s => new SelectListItem
            { Value = s.ID.ToString(), Text = s.Seat, Selected = s.ID == details.SeatID }).ToList();
            seatItems.Insert(0, noneItem);

            var insItems = insurances.Select(i => new SelectListItem
            { Value = i.ID.ToString(), Text = i.Type, Selected = i.ID == details.InsuranceID }).ToList();
            insItems.Insert(0, noneItem);

            var bagItems = baggages.Select(b => new SelectListItem
            { Value = b.ID.ToString(), Text = $"{b.Weight} kg", Selected = b.ID == details.BaggageID }).ToList();
            bagItems.Insert(0, noneItem);

            var vm = new OtherDetailsEditViewModel
            {
                Details = details,
                PaxName = paxName,
                Contact = contact,
                Origin = origin,
                Destination = destination,
                TravelDate = travelDate,
                TravelTime = travelTime,
                Airline = airline,
                CurrentMeal = meals.FirstOrDefault(m => m.ID == details.MealID)?.Type ?? string.Empty,
                CurrentSeat = seats.FirstOrDefault(s => s.ID == details.SeatID)?.Seat ?? string.Empty,
                CurrentInsurance = insurances.FirstOrDefault(i => i.ID == details.InsuranceID)?.Type ?? string.Empty,
                CurrentBaggage = baggages.FirstOrDefault(b => b.ID == details.BaggageID) is { } foundBag
                                        ? $"{foundBag.Weight} kg" : string.Empty,
                MealSelectList = new SelectList(mealItems, "Value", "Text"),
                SeatSelectList = new SelectList(seatItems, "Value", "Text"),
                InsuranceSelectList = new SelectList(insItems, "Value", "Text"),
                BaggageSelectList = new SelectList(bagItems, "Value", "Text"),
            };

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────────
        // POST: /OtherDetails/Edit/5
        // ─────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OthersDetailsModel details)
        {
            details.OthersID = id;
            bool success = await _service.UpdateOthersDetailsAsync(id, details);

            TempData[success ? "Success" : "Error"] = success
                ? "Add-ons updated successfully."
                : "Update failed. The booking may have been cancelled.";

            return RedirectToAction(nameof(List));
        }
    }
}