using System;

public class Program
{
    public static void Main()
    {
        while (true)
        {
            Console.Write("> ");
            var value = Console.ReadLine();
            
            if (value == "malinovka")
            {
                Console.WriteLine("Малиновка шоколадного кольца");
            }
            else if (value == "zeWhite")
            {
                Console.WriteLine("zeWhite on the beat");
            }
            else if (value == "exit")
            {
                break;
            }
            else
            {
                Console.WriteLine("Неизвестная команда");
            }

            Console.WriteLine();
        }

        Console.WriteLine();

        IPlayer player = new Enemy("zeWhite");

        for (int i = 0; i < 5; i++)
        {
            player.PrintMessage();
        }

        var playerType = player.GetType();
        Console.WriteLine();

        TestArray();

        Console.WriteLine();

        TestPlayersArray("zeBlack", 2);

        Console.WriteLine("Text: " + "example" + Numbers.Int64ToString(12));
        Console.WriteLine("Int size: " + GetSize<int>());
        Console.WriteLine("Long size: " + GetGenericObject<long>().Size);


        // last exception should be unhandled
        TestExceptionHandling();
    }

    private static void TestArray()
    {
        string[] values = new string[] { "value", "value" };
        int i = 0;

        foreach (var value in values)
        {
            Console.WriteLine(value + i);
            i++;
        }
    }
    private static int GetSize<T>() => sizeof(T);
    private static GenericObject<T> GetGenericObject<T>() => new GenericObject<T>();
    private static void TestPlayersArray(string name, int count)
    {
        ValuePlayer[] players = new ValuePlayer[count];

        for (int i = 0; i < players.Length; i++)
        {
            players[i] = new(name);
            players[i].PrintMessage();
        }
    }
    private static void TestExceptionHandling()
    {
        try
        {
            throw new Exception("Random exception");
        }
        catch
        {
            Console.WriteLine("An random exception was catched");
        }
        try
        {
            throw new InvalidOperationException("Invalid operation exception");
            Console.WriteLine("Этого никогда не было");
        }
        catch (InvalidOperationException exception)
        {
            Console.WriteLine(exception.Message);
        }
        finally
        {
            Console.WriteLine("\"TestExceptionHandling\" completed");
        }

        throw new NotImplementedException("Сказал как с лестницы упал");
    }
}

public struct GenericObject<T>
{
    public GenericObject()
    {
        Size = sizeof(T);
    }

    public int Size;
}
public interface IPlayer
{
    public string Name { get; }

    public void PrintMessage();
}
public struct ValuePlayer : IPlayer
{
    public ValuePlayer(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public void PrintMessage() => Console.WriteLine(Name);
}
public class Player : IPlayer
{
    public Player(string name)
    {
        Name = name;
    }

    public virtual string Name { get; }

    public virtual void PrintMessage()
    {
        Console.WriteLine(Name);
    }
}
public class Enemy : Player
{
    public Enemy(string name) : base(name)
    {
    }

    public override string Name => "Поедатель миров";

    public override void PrintMessage()
    {
        base.PrintMessage();
        Console.WriteLine("Фигня №1");
    }
}