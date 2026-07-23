using System;

public class Program
{
    public static void Main()
    {
        /*
        IPlayer player = new Enemy("zeWhite");

        for (int i = 0; i < 5; i++)
        {
            player.PrintMessage();
        }
        */

        string[] values = new string[] { "value1", "value2" };
        
        foreach (var value in values)
        {
            Console.WriteLine(value);
        }

        /*
        for (int i = 0; i < values.Length; i++)
        {
            var value = values[i]; 

            Console.WriteLine(value);

            for (int c = 0; c < value.Length; c++)
            {
                Console.WriteLine(value[c]);
            }
        }
        */
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