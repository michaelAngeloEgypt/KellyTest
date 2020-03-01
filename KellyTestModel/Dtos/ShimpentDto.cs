using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KellyTestModel.Dtos
{
    public class ShimpentDto
    {
        public int ShimpentId { get; set; }
        public int FirstName { get; set; }
        public int LastName { get; set; }
        public int Address { get; set; }
        public int City { get; set; }
        public int State { get; set; }
        public int Country { get; set; }
        public List<ShipmentProductDto> Products{ get; set; }

        public ShimpentDto()
        {
            Products = new List<ShipmentProductDto>();
        }
    }
}
