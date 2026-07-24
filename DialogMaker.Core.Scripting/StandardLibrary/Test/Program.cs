using System;

public class Program
{
    public static void Main()
    {
        IPlayer player = new Enemy("zeWhite");

        for (int i = 0; i < 5; i++)
        {
            player.PrintMessage();
        }

        Console.WriteLine();

        TestArray();
        TestPlayersArray("zeBlack", 2);
    }

    private static void TestArray()
    {
        string[] values = new string[] { "value1", "value2" };
        
        foreach (var value in values)
        {
            Console.WriteLine(value);
        }
    }
    private static void TestPlayersArray(string name, int count)
    {
        ValuePlayer[] players = new ValuePlayer[count];

        for (int i = 0; i < players.Length; i++)
        {
            players[i] = new(name);
            players[i].PrintMessage();   
        }
    }
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