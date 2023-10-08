﻿// Controller for Reservation Model

using TrainTicketApi.Services;
using TrainTicketApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace TrainTicketApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly ReservationService _reservationService;
        private readonly ScheduleService _scheduleService;
        private readonly TrainService _trainService;

        public ReservationController(ReservationService reservationService, ScheduleService scheduleService, TrainService trainService)
        {
            _reservationService = reservationService;
            _scheduleService = scheduleService;
            _trainService = trainService;
        }

        // Get all reservations from db
        [HttpGet]
        public async Task<List<Reservation>> Get() =>
            await _reservationService.GetAsync();

        // Get a reservation by id
        [HttpGet("get")]
        public async Task<ActionResult<Reservation>> Get(string id)
        {
            var reservation = await _reservationService.GetAsync(id);

            if (reservation is null)
            {
                return NotFound();
            }
            return reservation;
        }

        // Create new Reservation
        [HttpPost("create")]
        public async Task<IActionResult> Create(Reservation reservation)
        {
            Schedule selectedSchedule;
            int count = reservation.pax;

            if (reservation.pax < 1)
                return BadRequest("Invalid no.of persons");

            try
            {
                selectedSchedule = await _scheduleService.GetAsync(reservation.ScheduleId);

                if (selectedSchedule is null)
                {
                    return NotFound("Schedule Not found");
                }

                // Check if the number of seats are at max
                List<Reservation> activeResForSchedule = await _reservationService.GetAsyncBySchedule(reservation.ScheduleId);
                foreach (var item in activeResForSchedule)
                {
                    count += item.pax;
                }

                if (count > selectedSchedule.Seats)
                {
                    return BadRequest("Reservation limit exceeded for the train");
                }
            }
            catch (Exception ex)
            {
                return NotFound("Schedule Not found");
            }

            // Check if the schedule date is within 30 days of the current date
            //DateTime currentDate = DateTime.UtcNow;
            //DateTime scheduleDate = selectedSchedule?.Date ?? DateTime.MinValue;
            //TimeSpan dateDifference = scheduleDate - currentDate;

            DateOnly currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
            DateOnly scheduleDate = selectedSchedule?.Date ?? selectedSchedule.Date;

            int dateDifference = scheduleDate.Day - currentDate.Day;

            if (dateDifference > 30)
            {
                return BadRequest("Schedule date is more than 30 days in the future.");
            }

            await _reservationService.CreateAsync(reservation);

            return CreatedAtAction(nameof(Create), new { id = reservation.Id }, reservation);
        }

        // Edit reservation details
        [HttpPost("edit")]
        public async Task<IActionResult> Edit(string id, Reservation reservation)
        {
            Reservation reservation1;
            Schedule selectedSchedule;

            if (reservation.pax < 1)
                return BadRequest("Invalid no.of persons");

            try
            {
                reservation1 = await _reservationService.GetAsync(id);
            }
            catch (Exception ex)
            {
                return NotFound("Invalid reservation");
            }

            if (reservation1 == null)
            {
                return NotFound("Invalid reservation");
            }

            try
            {
                selectedSchedule = await _scheduleService.GetAsync(reservation1.ScheduleId);
            }
            catch (Exception ex)
            {
                return NotFound("Schedule Not found");
            }

            if (id is null)
            {
                return NotFound("Schedule Not found");
            }

            // Check if the schedule date is within 5 days of the current date
            //DateTime currentDate = DateTime.UtcNow;
            //DateTime scheduleDate = selectedSchedule?.Date ?? DateTime.MinValue;
            //TimeSpan dateDifference = scheduleDate - currentDate;

            DateOnly currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
            DateOnly scheduleDate = selectedSchedule?.Date ?? selectedSchedule.Date;

            int dateDifference = scheduleDate.Day - currentDate.Day;

            if (dateDifference <= 5)
            {
                return BadRequest("Reservations can only be edited atleast 5 days before the reservation date");
            }

            reservation.Id = id;
            await _reservationService.UpdateAsync(id, reservation);
            return Ok(reservation);
        }

        // Delete reservation
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) { return NotFound(); }

            Reservation reservation = await _reservationService.GetAsync(id);

            if (reservation is null)
            {
                return NotFound();
            }

            await _reservationService.RemoveAsync(id);
            return Ok(reservation);
        }

        // Get all reservations by user
        [HttpGet("getByUser")]
        public async Task<List<ReservationResponse>> GetByUser(string id) 
        {
            List<Reservation> reservations = await _reservationService.GetUsersResAsync(id);
            List<ReservationResponse> results = new List<ReservationResponse>();

            foreach (var item in reservations)
            {
                var currentSchedule = await _scheduleService.GetAsync(item.ScheduleId);
                if (currentSchedule is not null)
                {
                    ReservationResponse result = new ReservationResponse(item.Id, currentSchedule.TrainName, currentSchedule.Date, currentSchedule.StartTime, item.pax);
                    results.Add(result);
                }
            }
            return results;
        }
    }
}
