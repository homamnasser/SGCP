using System.Text.Json.Serialization;

namespace SGCP.Models
{
    public class Role
    {

        [JsonIgnore]
        public int Id { get; set; }
        public string Name { get; set; }


        public ICollection<User> Users { get; set; }


    }
}
