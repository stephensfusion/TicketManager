using LogHandler.Core;
using LogHandler.Writers;
using Scalar.AspNetCore;
using ServicesUnitTest.Implementation;
using ServicesUnitTest.Models;
using TicketManager.Enumerations;
using TicketManager.Extentions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register services
//builder.Services.AddSingleton<DemoOne>(provider => new DemoOne("Manual Testing"));
//builder.Services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<DemoOne>());

// 1. LOGHANDLER

builder.Services.Configure<LogOptions>(options =>
{
    options.DateFormat = "yyyy-MM-dd";
    options.FlowDirection = LogFlow.TopToBottom;
    options.ArchivePath = "Logs"; // Make sure to set this
    options.ArchiveThreshold = TimeSpan.FromDays(1); // Example threshold
});


builder.Services.AddSingleton<ILogWriter, FileLogWriter>();

// Register logger as singleton (or scoped if needed)
// builder.Services.AddSingleton<ILogWriter>(new FileLogWriter(IntiationModels._log));


// // 2. MANAGEFILES
// builder.Services.AddFileStateManager<FileProcessStatus>(IntiationModels._psqlModel);

// // 3. USERMANAGER
// builder.Services.AddUserManager<UserProcessRole, UserProcessStatus, UserProcessPermission, UserProcessTheme>(IntiationModels._psqlModel);

// builder.Services.AddUserManager<UserProcessRole, UserProcessStatus, UserProcessPermission, UserProcessTheme>(
//     IntiationModels._psqlModel,
//     true, // enable logging
//     new ActionRecordSettings
//     {
//         LogRoles = true,
//         LogPermission = true,
//         LogStatuses = true,
//         IncludeActorInfo = true,
//         IncludeTimeStamp = true,
//         IncludePayloadDetails = true
//     });


// // 4. GROUPSMANAGER
// builder.Services.AddGroupManager(IntiationModels._psqlModel);

// // 5. FIELDOPTIONSMANAGER
// builder.Services.AddFieldOptionsManager(IntiationModels._psqlModel);

// builder.Services.AddActionManager(IntiationModels._psqlModel);
builder.Services.AddTicketManager<TicketStatus, Priority,Tags>(IntiationModels._psqlModel, new NSCore.Models.DbSetupOptions{EnablePooling=true,PoolSize=50,EnableDatabaseLogging=false});

var app = builder.Build();
ServiceLocator.SetLocator(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opt =>
    {
        opt.WithTitle("Available API's");
        opt.WithTheme(ScalarTheme.Mars);
        opt.WithSidebar(true);

    });
}

app.UseAuthorization();

app.MapControllers();

app.MapRazorPages();

app.UseStaticFiles();

app.Run();
