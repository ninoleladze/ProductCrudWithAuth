using HmCrudPLusAuth.Common;

namespace HmCrudPLusAuth.Models
{
    public class Product : Entity
    {
        public string Name { get; set; } 
        public string Description { get; set; }
        public double Price { get; set; }


        public int UserId { get; set; }
        public User User { get; set; }



    }
}
