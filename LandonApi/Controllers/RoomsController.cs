using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LandonApi.Models;
using LandonApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LandonApi.Controllers
{

    [Route("/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService roomService;
        private readonly IOpeningService openingService;

        public RoomsController(IRoomService roomService, IOpeningService openingService)
        {
            this.roomService = roomService;
            this.openingService = openingService;
        }

        [HttpGet(Name = nameof(GetAllRooms))]
        [ProducesResponseType(200)]
        public async Task<ActionResult<Collection<Room>>> GetAllRooms()
        {
            var rooms = await roomService.GetRoomsAsync();
            var collection = new Collection<Room>()
            {
                Self = Link.ToCollection(nameof(GetAllRooms)),
                Value = rooms.ToArray()
            };

            return collection;
        }

        // GET /rooms/openings
        [HttpGet("openings", Name = nameof(GetAllRoomOpenings))]
        [ProducesResponseType(200)]
        public async Task<ActionResult<PagedCollection<Opening>>> GetAllRoomOpenings([FromQuery] PagingOptions pagingOptions = null)
        {
            var openings = await openingService.GetOpeningsAsync(pagingOptions);

            var collection = new PagedCollection<Opening>()
            {
                Self = Link.ToCollection(nameof(GetAllRoomOpenings)),
                Value = openings.Items.ToArray(),
                Size = openings.TotalSize,
                Offset = pagingOptions?.Offset ?? 0,
                Limit = pagingOptions?.Limit ?? 0

            };

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

    }
}
