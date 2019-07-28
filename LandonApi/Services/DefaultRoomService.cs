using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using LandonApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LandonApi.Services
{
    public class DefaultRoomService : IRoomService
    {
        private readonly HotelApiDbContext context;
        private readonly IConfigurationProvider mappingConfiguration;


        public DefaultRoomService(HotelApiDbContext context, IConfigurationProvider mappingConfiguration)
        {
            this.context = context;
            this.mappingConfiguration = mappingConfiguration;
        }

       public async Task<Room> GetRoomAsync(Guid id)
        {
            var entity = await context.Rooms.SingleOrDefaultAsync(room => room.Id == id);

            if (entity == null)
            {
                return null;
            }

            var mapper = mappingConfiguration.CreateMapper();
            return mapper.Map<Room>(entity);
        }

        public async Task<PagedResults<Room>> GetRoomsAsync(
            PagingOptions pagingOptions,
            SortOptions<Room, RoomEntity> sortOptions,
            SearchOptions<Room, RoomEntity> searchOptions)
        {
            IQueryable<RoomEntity> query = context.Rooms;
            query = searchOptions.Apply(query);
            query = sortOptions.Apply(query);

            var size = await query.CountAsync();

            var items = await query
                .Skip(pagingOptions.Offset ?? 0)
                .Take(pagingOptions.Limit ?? 0)
                .ProjectTo<Room>(mappingConfiguration)
                .ToArrayAsync();

            return new PagedResults<Room>
            {
                Items = items,
                TotalSize = size
            };

        }
    }
}
