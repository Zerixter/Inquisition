﻿using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace Inquisition.Data
{
    /*
     * All info is stored locally for now, maybe will end up using azure database
     * at some point if needed, for now it works like this.
     * 
     * To be able to store info in the database you need to have installed the two
     * EntityFramework packages (See Program.cs for info)
     * 
     * Once the packages installed, open up the Package Manager Console and type:
     *      update-database
     *      
     * This will create a local database for the program to use.
     */
    public class InquisitionContext : DbContext
    {
        public DbSet<Game> Games { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<Joke> Jokes { get; set; }
        public DbSet<Meme> Memes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=InquisitionDB;Trusted_Connection=True;");
        }
    }

    public class Game
    {
        [Key]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(10)]
        public string Port { get; set; }

        [StringLength(10)]
        public string Version { get; set; }

        public bool IsOnline { get; set; }

        public string ExeDir { get; set; } = "";

        public string Args { get; set; } = "";
    }

    public class Reminder
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public string Username { get; set; }

        public string Message { get; set; }

        public DateTime CreateDate { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTime DueDate { get; set; }
    }

    public class Joke
    {
        [Key]
        public int Id { get; set; }

        public string Author { get; set; }

        public string Text { get; set; }

        public int PositiveVotes { get; set; } = 0;

        public int NegativeVotes { get; set; } = 0;
    }

    public class Meme
    {
        [Key]
        public int Id { get; set; }

        public string Author { get; set; }

        public string Url { get; set; }
    }
}
