using System;
using System.Collections.Generic;
using System.Linq;
using Internal.System.Runtime;

public class Program
{
    public enum ProgramType
    {
        Default,
        MegaGovno
    }

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

        IPlayer player = new ValuePlayer("zeWhite");

        for (int i = 0; i < 5; i++)
        {
            player.PrintMessage();
        }


        var playerType = player.GetType();
        Console.WriteLine(playerType.ToString());
        var enumType = typeof(ProgramType);
        Console.WriteLine(enumType.ToString());
        Console.WriteLine(ObjectType.Class);
        Console.WriteLine(ObjectType.Class == ObjectType.Class);
        Console.WriteLine(ObjectType.Class == ObjectType.Enum);
        Console.WriteLine();

        if (player is ValuePlayer valuePlayer)
        {
            Console.WriteLine("player is value player: " + valuePlayer.Name);
        }
        else
        {
            Console.WriteLine("player is not value player");
        }

        Console.WriteLine("Selection 1: " + Selector<Enemy>(player, "Гавёшка"));
        Console.WriteLine("Selection 2: " + Selector<IPlayer>(player, "Гавёшка"));

        Console.WriteLine();

        TestArray();

        Console.WriteLine();

        try
        {
            Activator.CreateInstance(typeof(Program));
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }

        Console.WriteLine();

        TestPlayersArray("zeBlack", 2);

        Console.WriteLine("Text: " + "example" + Numbers.Int64ToString(12));
        Console.WriteLine("Int size: " + GetSize<int>());
        Console.WriteLine("Long size: " + GetGenericObject<long>().Size);
        Console.WriteLine();

        TestOutputs();

        // last exception should be unhandled
        TestExceptionHandling();
    }

    private static object Selector<T>(object? value1, object value2)
    {
        return (value1 as T) ?? value2;
    }
    private static void TestArray()
    {
        string[] values = new string[] { "value", "value" };
        string[] values2 = new string[] { "clay", "sand" };
        int i = 0;

        foreach (var value in values)
        {
            Console.WriteLine(value + i);
            i++;
        }

        var firstValue = Enumerator.FirstOrDefault(values);

        if (firstValue != null)
        {
            Console.WriteLine("first value: " + firstValue);
        }
        else
        {
            Console.WriteLine("first value not found");
        }

        Console.WriteLine("Union:");

        foreach (var unionValue in values.Union(values2))
        {
            Console.WriteLine(unionValue);
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
    private static void TestOutputs()
    {
        if (TryGetZeBlack("govno", out var ze))
        {
            Console.WriteLine("zeBlack ответил: " + ze);
        }
        else
        {
            Console.WriteLine("zeBlack не ответил");
        }
        if (TryGetZeBlack("zeBlack", out ze))
        {
            Console.WriteLine("zeBlack ответил: " + ze);
        }
        else
        {
            Console.WriteLine("zeBlack не ответил");
        }
    }

    private static bool TryGetZeBlack(string value, out string? result)
    {
        if (value == "zeBlack")
        {
            result = "zeWhite";
            return true;
        }

        result = null;
        return false;
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

    public override string ToString()
    {
        return Name;
    }
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
    public override string ToString()
    {
        return Name;
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