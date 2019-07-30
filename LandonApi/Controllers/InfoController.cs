using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LandonApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LandonApi.Controllers
{
    [Route("/[Controller]")]
    [ApiController]
    public class InfoController:ControllerBase
    {
        private readonly HotelInfo hotelInfo;

        public InfoController(IOptions<HotelInfo> hotelInfoWrapper)
        {
            hotelInfo = hotelInfoWrapper.Value;
        }

        [HttpGet(Name = nameof(GetInfo))]
        [ProducesResponseType(200)]
        [ResponseCache(CacheProfileName = "Static")]
        public ActionResult<HotelInfo> GetInfo()
        {
            hotelInfo.Href = Url.Link(nameof(GetInfo), null);
            return hotelInfo;
        }
    }
}
