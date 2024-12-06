namespace SOA_CA2_Frontend.Models
{
    public class UserModel
    {
        public int User_Id { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public int Role { get; set; }
        public string Address { get; set; }

        public string ApiKey { get; set; }

        public string RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiration { get; set; }

        public DateTime? ApiKeyExpiration { get; set; }
        public DateTime CreatedAt { get; set; }


        public string roleName
        {
            get
            {
                return Role switch
                {
                    0 => "Admin",
                    1 => "Customer",
                    _ => "Unknown"
                };
            }
        }
    }
}
