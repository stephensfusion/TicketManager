using System;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSCatch.Interfaces;
using NSCore.DatabaseContext;
using NSCore.DatabaseProviders;
using TicketManager.Data;
using TicketManager.DbImplementations;
using TicketManager.Enumerations;

namespace TicketManager.Tests;

public class ManageTicketsTests
{
    private readonly Mock<IDatabaseConfig> _psqlConfig = new();
    private readonly Mock<IDatabaseConfig> _mySqlConfig = new();
    private readonly Mock<IDatabaseConfig> _sqlConfig = new();
    private readonly Mock<IDbContextFactory<AppDbContext>> _contextFactory = new();
    private readonly Mock<ICacheManager> _cacheManager = new();
    private readonly Mock<ICacheKeyBuilder> _cacheKeyBuilder = new();

    public ManageTicketsTests()
    {
        _psqlConfig.Setup(c => c.GetType()).Returns(typeof(PSQLDb));
        _mySqlConfig.Setup(c => c.GetType()).Returns(typeof(MySQLDb));
        _sqlConfig.Setup(c => c.GetType()).Returns(typeof(SQLDb));
    }

    [Fact]
    public void Constructor_WithPSQLDbConfig_InitializesPostgreSQLConnection()
    {
        var manageTickets = new ManageTickets(_sqlConfig.Object, _contextFactory.Object, _cacheManager.Object, _cacheKeyBuilder.Object);

        Assert.IsType<PostgreSQL<TicketStatus, Priority, Tags>>(GetPrivateField(manageTickets, "_connection"));
    }

    private object GetPrivateField(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field!.GetValue(obj)!;
    }
}
