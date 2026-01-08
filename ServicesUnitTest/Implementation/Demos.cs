namespace ServicesUnitTest;
using Microsoft.Extensions.Hosting;

public class DemoOne : BackgroundService
{
    private readonly string _name;
    public DemoOne(string name)
    {
        Console.WriteLine(name);
        _name = name;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("DemoOne is running.");
        // Implemnt on and off logic
    }

    public string GetName()
    {
        return _name;
    }
}

public class ServiceOne : BackgroundService
{
    private readonly string _name;
    public ServiceOne(string name)
    {
        _name = name;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        DemoOne demoOne = new DemoOne(_name);
    }
}

public class DemoTwo
{
    public readonly int Id;
    public DemoTwo(int id)
    {
        Console.WriteLine(id);
    }
    public int GetId()
    {
        return Id;
    }
}

public class ServiceTwo : BackgroundService
{
    private readonly int _id;
    public ServiceTwo(int id)
    {
        _id = id;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        DemoTwo demoTwo = new DemoTwo(_id);
    }
}

public class DemoThree
{
    public DemoThree(DateTime date)
    {
        Console.WriteLine(date);
    }
}



public class ServiceThree : BackgroundService
{
    private readonly DateTime _date;
    public ServiceThree(DateTime date)
    {
        _date = date;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        DemoThree demoThree = new DemoThree(_date);
    }
}


