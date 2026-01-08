using System;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSCore.DatabaseContext;
using TicketManager.Data;
using TicketManager.Models;

namespace TicketManager.Tests;

public class TicketServiceTests
{
    private readonly ManageTickets _service;
    private readonly AppDbContext _context;

    public TicketServiceTests()
    {
        // In-memory EF Core database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        // Seed test data
        SeedTickets(_context);

        // Mock DbContextFactory
        var contextFactoryMock = new Mock<IDbContextFactory<AppDbContext>>();
        contextFactoryMock
            .Setup(f => f.CreateDbContext())
            .Returns(_context);

    }

    private static void SeedTickets(AppDbContext context)
    {
        var tickets = Enumerable.Range(1, 10)
            .Select(i => new Ticket
            {
                TicketId = i,
                Title = $"user{i}@test.com",
                TicketStatus = "Open",
                Priority = "Low",
                Tag = "General",
                CreatedDate = DateTimeOffset.UtcNow.AddDays(-i),
                UpdatedDate = DateTimeOffset.UtcNow,
                Attachments = new List<string>(),
                Metadata = new List<TicketMetadata>(),
                CCS = new List<string>()
            });

            context.Tickets.AddRange(tickets);
            context.SaveChanges();
    }
}
