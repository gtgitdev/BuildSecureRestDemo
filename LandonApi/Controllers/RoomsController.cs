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
    public class RoomsController: ControllerBase
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
        public async Task<ActionResult<Collection<Opening>>> GetAllRoomOpenings()
        {
            var openings = await openingService.GetOpeningsAsync();

            var collection = new Collection<Opening>()
            {
                Self = Link.ToCollection(nameof(GetAllRoomOpenings)),
                Value = openings.ToArray()
            };

            return collection;
        }

        [HttpGet("{roomId}", Name = nameof(GetRoomById) ) ]
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
