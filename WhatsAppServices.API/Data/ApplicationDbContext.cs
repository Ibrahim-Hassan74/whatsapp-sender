using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WhatsAppServices.API.Entities;
using WhatsAppServices.API.IdentityEntities;

namespace WhatsAppServices.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<WhatsAppSession> WhatsAppSessions { get; set; }
        public virtual DbSet<MessageAttempt> MessageAttempts { get; set; }
    }
}
