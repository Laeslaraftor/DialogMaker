using System;
using System.Collections.Generic;

public class Program
{
    public static void Main()
    {
        List<Exception> exceptions = new();

        while (true)
        {
            try
            {
                MainImpl();
            }
            catch (Exception error)
            {
                exceptions.Add(error);
            }

            Console.WriteLine();
            Console.WriteLine("Restart?");
            bool restart = false;
            
            while (true)
            {
                Console.Write("y/n > ");
                var value = Console.ReadLine();

                if (value == "y")
                {
                    restart = true;
                    break;
                }
                else if (value == "n")
                {
                    break;
                }
            }

            if (restart)
            {
                continue;
            }

            break;
        }

        Console.WriteLine("Program ended with " + exceptions.Count + " exceptions:");

        foreach (var exception in exceptions)
        {
            Console.WriteLine(exception.Message);
        }
    }
    private static void MainImpl()
    {
        while (true)
        {
            Console.Write("> ");
            var value = Console.ReadLine();
            
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }
            if (value == "malinovka")
            {
                Console.WriteLine("Малиновка шоколадного кольца");
            }
            else if (value == "zeWhite")
            {
                Console.WriteLine("zeWhite on the beat");
            }
            else if (value == "clear")
            {
                Console.Clear();
            }
            else if (value == "using")
            {
                using (DisposableObject disposable = new("first disposable"))
                {
                    Console.WriteLine("Now inside disposable block");
                }

                Console.WriteLine();
                using DisposableObject disposable2 = new("second disposable");
                throw new InvalidOperationException("Exception after creating disposable");
            }
            else if (value == "split")
            {
                Console.Write("Enter value: ");
                var valueToSplit = Console.ReadLine();
                Console.Write("Enter separator: ");
                var separator = Console.ReadLine();
                var parts = valueToSplit.Split(separator[0]);

                Console.WriteLine();
                Console.WriteLine("Parts:");

                foreach (var part in parts)
                {
                    Console.WriteLine(part);
                }
            }
            else if (value == "replace")
            {
                Console.Write("Enter value: ");
                var valueToTestReplacing = Console.ReadLine();
                Console.Write("Enter value that need to replace: ");
                var oldValue = Console.ReadLine();
                Console.Write("Enter new value: ");
                var newValue = Console.ReadLine();
                valueToTestReplacing = valueToTestReplacing.Replace(oldValue, newValue);

                Console.WriteLine();
                Console.WriteLine("New value: " + valueToTestReplacing);
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
        Console.WriteLine(playerType.Name);

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

public class DisposableObject : IDisposable
{
    public DisposableObject(string name)
    {
        _name = name;
        Console.WriteLine("Disposable object \"" + name + "\" created");
    }

    private readonly string _name;

    public void Dispose()
    {
        Console.WriteLine("Object \"" + _name + "\" was successfully disposed!");
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