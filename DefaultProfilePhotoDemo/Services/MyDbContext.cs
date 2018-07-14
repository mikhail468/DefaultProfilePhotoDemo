using DefaultProfilePhotoDemo.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DefaultProfilePhotoDemo.Services
{

    public class MyDbContext : IdentityDbContext
    {
        public DbSet<Profile> TblCustomerProfile { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder option)
        {
            option.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB ; Database=DefaultProfilePhotoDatabase; Trusted_Connection=True");
        }
    }
    
}
