string[]? filters = ["png", "jpg", "gif", "bmp", "webp"];
filters = filters.Select(f => "*." + f).ToArray();
string filterNames = string.Join(";", filters ?? []);
string label = $"Images ({filterNames})|{filterNames}";

Console.WriteLine(label);

Console.ReadKey();