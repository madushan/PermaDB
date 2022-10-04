﻿using Perma.Engine;

Console.WriteLine("PermaDB");

string dbName = "abcd.data";

try
{
    List<uint> ids = new List<uint>();

    using (Database db = new Database(dbName))
    {
        uint id1 = db.Insert("one");
        ids.Add(id1);
        Console.WriteLine("Inserted 1st row");

        uint id2 = db.Insert("two");
        ids.Add(id2);
        Console.WriteLine("Inserted 2nd row");
    }

    using (Database db = new Database(dbName))
    {
        foreach (uint id in ids)
        {
            string row = db.FindById(id);
            Console.WriteLine(id + " " + row);
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}