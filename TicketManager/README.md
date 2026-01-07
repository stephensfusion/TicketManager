# ServiceTemplate

Ticket Manager service provides a comprehensive,
API-driven platform for handling the lifecycle of support tickets, from their creation, 
retrieval, updating to deletion. The system supports core CRUD operations, advanced file management, 
cache integration for performance, flexible database connectivity, and comprehensive configuration options.
***


***
# Features
- Create ticket and upload files
- Manage tickets and retrieve ticket lists
- Update ticket statuses
- Delete tickets and files
- Search and filter through tickets
- Plug-in architecture supporting PostgreSQL, MySQL, and SQL Server
- Built-in base controller for immediate API usage
- Supports custom controller extensions with advanced logic

***


***
# Installation

Set up the TicketManager service in your project using a structured flow for build, reference, and use.
## Step 1: Register Repository in ```repos.xml```
First, make sure your project recognizes the TicketManager repository. Update your ```repos.xml``` file to include the following entry:

```bash
<Repositories>
  <Repository>
    <Name>TicketManager</Name>
    <Branch>main</Branch>
  </Repository>
</Repositories>
```


## Step 2: Build the Service
Navigate to the build directory that contains the build.ps1 script and run the following PowerShell command:
```powershell
.\build.ps1
```

This will:

Compile the TicketManager service

Copy the output DLL to build\NsStore\TicketManager.

## Step 3: Reference the DLL in Your .csproj
***
<ItemGroup>
  <Reference Include="TicketManager">
    <HintPath>build\NsStore\TicketManager.dll</HintPath>
  </Reference>
</ItemGroup>

***
***

# Getting Started with Controllers

TicketManager includes a generic RESTful controller that you can inherit and extend.

***
## Inheriting the Built-in Controller

To use the default logic and endpoints, simply extend the base controller:

`````csharp
[Route("api/[controller]")]
[ApiController]
public class InheritTicketController : TicketControllerBase<TicketStatus>
{
    public InheritTicketController(ITicketInterface<TicketStatus> context) : base(context)
    {
    }
}
***
***
## Defining Custom Endpoints

You can extend the base controller with your own logic:

````csharp
[Route("api/[controller]")]
[ApiController]
public class InheritTicketController : TicketControllerBase<TicketStatus>
{
    private readonly ITicketInterface<TicketStatus> _ticket;
     public InheritTicketController(ITicketInterface<TicketStatus> context) : base(context)
    {
         _ticket = context ?? throw new ArgumentNullException(nameof(context));
    }

    [HttpGet("GetAllTicketsCustomLogic")]
    public async Task<IActionResult> GetAllTicketsCustomLogic()
    {
        var tickets = await _ticket.GetAllTicketsAsync();
        return Ok(tickets);
    }
}
***


***
# Service Registration
In Program.cs, register the service using the provided extension:

```csharp
// SQL Server
builder.Services.AddTicketManager(IntiationModels._sqlModel);

// MySQL Server
builder.Services.AddTicketManager(IntiationModels._mysqlModel);

// PostgreSQL
builder.Services.AddTicketManager(IntiationModels._psqlModel);
`````

**_IntiationModels.\_psqlModel_**,
**_IntiationModels.\_sqlModel_**,
and **_IntiationModels.\_mySqlModel_**
must implement IDatabaseConfig and determine the correct database provider.

***
