﻿using Microsoft.EntityFrameworkCore;
using Stargate.Server.Data.Models;
using System.Data;

namespace Stargate.Server.Data
{
    public class StargateContext : DbContext
    {
        public IDbConnection Connection => Database.GetDbConnection();
        public virtual DbSet<Person> People { get; set; }
        public virtual DbSet<AstronautDetail> AstronautDetails { get; set; }
        public virtual DbSet<AstronautDuty> AstronautDuties { get; set; }
        public virtual DbSet<PersonAstronaut> PersonAstronauts { get; set; }
        public virtual DbSet<LogEntry> LogEntries { get; set; }

        public StargateContext() { /* For testing purposes with moq */}

        public StargateContext(DbContextOptions<StargateContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StargateContext).Assembly);

            // not sure if this is wanted so I'm leaving in case
            //SeedData(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            //add seed data
            modelBuilder.Entity<Person>()
                .HasData(
                    new Person
                    {
                        Id = 1,
                        Name = "John Doe"
                    },
                    new Person
                    {
                        Id = 2,
                        Name = "Jane Doe"
                    }
                );

            modelBuilder.Entity<AstronautDetail>()
                .HasData(
                    new AstronautDetail
                    {
                        Id = 1,
                        PersonId = 1,
                        CurrentRank = "1LT",
                        CurrentDutyTitle = "Commander",
                        CareerStartDate = DateTime.Now
                    }
                );

            modelBuilder.Entity<AstronautDuty>()
                .HasData(
                    new AstronautDuty
                    {
                        Id = 1,
                        PersonId = 1,
                        DutyStartDate = DateTime.Now,
                        DutyTitle = "Commander",
                        Rank = "1LT"
                    }
                );
        }
    }
}
