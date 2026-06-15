using System;
using PKHeX.Core;

class Program {
    static void Main() {
        foreach(var name in Enum.GetNames(typeof(EntityContext))) {
            Console.WriteLine(name);
        }
        Console.WriteLine("---");
        foreach(var name in Enum.GetNames(typeof(GameVersion))) {
            if (name.Contains("Z") || name.Contains("A") || name.Contains("PL"))
                Console.WriteLine(name);
        }
    }
}
