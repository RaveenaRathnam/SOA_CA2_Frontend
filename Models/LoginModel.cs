﻿namespace SOA_CA2_Frontend.Models
{
    public class LoginModel
    {
        public string jwtToken { get; set; }
        public string refreshToken { get; set; }
        public string apiKey { get; set; }

        public int userId { get; set; }
    }

}
