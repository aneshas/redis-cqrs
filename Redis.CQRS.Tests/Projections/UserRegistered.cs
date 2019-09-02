using System;
using Tactical.DDD;

namespace Redis.CQRS.Tests.Projections
{
    public class UserRegistered : IDomainEvent
    {
        public DateTime CreatedAt { get; set; }
        
        public string Email { get; }
        
        public string Username { get; }
        
        public int Age { get; }

        public UserRegistered(string email, string username, int age)
        {
            Email = email;
            Username = username;
            Age = age;
        }
    }
}