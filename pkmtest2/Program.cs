using System;
using PKHeX.Core;

class Program
{
    static void Main()
    {
        var strings = GameInfo.GetStrings(""en"");
        Console.WriteLine(""Ball: "" + strings.ball[4]);
        Console.WriteLine(""Nature: "" + strings.Natures[0]);
        Console.WriteLine(""Ability: "" + strings.Ability[1]);
        Console.WriteLine(""Move: "" + strings.Move[1]);
        Console.WriteLine(""Location: "" + strings.Location[4]);
        
        var pk = new PK9();
        pk.Species = 1;
        pk.Ball = 4;
        pk.Nature = 0;
        pk.Ability = 1;
        pk.Move1 = 1;
        pk.MetDate = new DateOnly(2026, 6, 14);
        pk.Met_Location = 4;
        
        Console.WriteLine($""Language array? "" + strings.GetLanguage(1)); 
    }
}
