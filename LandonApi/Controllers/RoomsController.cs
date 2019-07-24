using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LandonApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LandonApi.Controllers
{
   
    [Route("/[controller]")]
    [ApiController]
    public class RoomsController: ControllerBase
    {
        private readonly HotelApiDbContext context;

        public RoomsController(HotelApiDbContext context)
        {
            this.context = context;
        }

        [HttpGet(Name = nameof(GetRooms))]
        public IActionResult GetRooms()
        {
            throw new NotImplementedException();
        }

        [HttpGet("{roomId}", Name = nameof(GetRoomById) ) ]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Room>> GetRoomById(Guid roomId)
        {
            var entity = await context.Rooms.SingleOrDefaultAsync(room => room.Id == roomId);

            if (entity == null)
            {
                return NotFound();
            }

            var resource = new Room
            {
                Href = Url.Link(nameof(GetRoomById), new {roomId = entity.Id}),
                Name = entity.Name,
                Rate = entity.Rate / 100.0m
            };

            return resource;
        }

    }
}
