using System;
using PKHeX.Core;

class Program
{
    static void Main()
    {
        var pk = new PK9();
        Console.WriteLine(pk.Met_Year);
        Console.WriteLine(pk.Met_Month);
        Console.WriteLine(pk.Met_Day);
    }
}
