using System;

public class Program
{
    public static void Main()
    {
        Player player = new("Ura");
        
        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine(player.Name);
        }
    }
}
public class Player
{
    public Player(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public string GetName() => Name;
}