using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LandonApi.Models;
using LandonApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LandonApi.Controllers
{
    // TODO: Authentication
    [Route("/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService roomService;
        private readonly IOpeningService openingService;
        private readonly IDateLogicService dateLogicService;
        private readonly IBookingService bookingService;
        private readonly PagingOptions defaultPagingOptions;

        public RoomsController(
            IRoomService roomService,
            IOpeningService openingService,
            IOptions<PagingOptions> defaultPagingOptions,
            IDateLogicService dateLogicService,
            IBookingService bookingService)
        {
            this.roomService = roomService;
            this.openingService = openingService;
            this.dateLogicService = dateLogicService;
            this.bookingService = bookingService;
            this.defaultPagingOptions = defaultPagingOptions.Value;
        }

        [HttpGet(Name = nameof(GetAllRooms))]
        [ProducesResponseType(200)]
        public async Task<ActionResult<Collection<Room>>> GetAllRooms(
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Room, RoomEntity> sortOptions,
            [FromQuery] SearchOptions<Room, RoomEntity> searchOptions)
        {
            pagingOptions = pagingOptions ?? new PagingOptions();

            pagingOptions.Offset = pagingOptions.Offset ?? defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? defaultPagingOptions.Limit;

            var rooms = await roomService.GetRoomsAsync(pagingOptions, sortOptions, searchOptions);

            var collection = PagedCollection<Room>.Create<RoomsResponse>(
                Link.ToCollection(nameof(GetAllRooms)),
                rooms.Items.ToArray(),
                rooms.TotalSize,
                pagingOptions);
            collection.Openings = Link.ToCollection(nameof(GetAllRoomOpenings));

            return collection;
        }

        // GET /rooms/openings
        [HttpGet("openings", Name = nameof(GetAllRoomOpenings))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedCollection<Opening>>> GetAllRoomOpenings(
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Opening, OpeningEntity> sortOptions,
            [FromQuery] SearchOptions<Opening, OpeningEntity> searchOptions)
        {

            pagingOptions.Offset = pagingOptions.Offset ?? defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? defaultPagingOptions.Limit;

            var openings = await openingService.GetOpeningsAsync(pagingOptions, sortOptions, searchOptions);

            var collection = PagedCollection<Opening>.Create(
                Link.ToCollection(nameof(GetAllRoomOpenings)),
                openings.Items.ToArray(),
                openings.TotalSize,
                pagingOptions);

            return collection;
        }

        [HttpGet("{roomId}", Name = nameof(GetRoomById))]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public async Task<ActionResult<Room>> GetRoomById(Guid roomId)
        {
            var room = await roomService.GetRoomAsync(roomId);

            if (room == null) return NotFound();

            return room;
        }

        // Post /rooms/{roomId}/bookings
        [HttpPost("{roomId}/bookings", Name = nameof(CreateBookingForRoom))]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(201)]
        public async Task<ActionResult> CreateBookingForRoom(Guid roomId, [FromBody] BookingForm bookingForm)
        {
            var room = await roomService.GetRoomAsync(roomId);
            if (room == null) return NotFound();

            var minimumStay = dateLogicService.GetMinimumStay();
            bool tooShort = (bookingForm.EndAt.Value - bookingForm.StartAt.Value) <
                            minimumStay;
            if (tooShort)
                return BadRequest(new ApiError($"The minimum booking duration is {minimumStay.TotalHours} hours"));

            var conflictedSlots =
                await openingService.GetConflictingSlots(roomId, bookingForm.StartAt.Value, bookingForm.EndAt.Value);
            if (conflictedSlots.Any()) return BadRequest(new ApiError("This time conflicts with existing booking."));

            // TODO: Get the current user
            var userId = Guid.NewGuid();

            var bookingId = await bookingService.CreateBookingAsync(
                userId, roomId, bookingForm.StartAt.Value, bookingForm.EndAt.Value);

            return Created(
                Url.Link(nameof(BookingsController.GetBookingById),
                    new {bookingId}), null);

        }
    }
}
