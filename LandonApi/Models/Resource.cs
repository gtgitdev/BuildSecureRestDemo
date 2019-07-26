using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LandonApi.Models
{
    public abstract class Resource :Link
    {
        [JsonIgnore]
        public Link Self { get; set; }
    }
}
