Dictionary<int, string> khDayString = new();

for (int i = 1; i <= 30; i++)
{
    string type = i <= 15 ? "កើត" : "រោច";
    int day = i % (15 + 1);
    int index = i <= 15 ? day : day + 1;
    khDayString.Add(i, $"{index}{type}");
}

foreach (var item in khDayString)
{
    Console.WriteLine(item);
}

Console.ReadKey();