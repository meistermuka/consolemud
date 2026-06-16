using ConsoleMud.Entities;

namespace ConsoleMud.Core.Commands;

public class WeatherCommand : ICommand
{
    public string Description => "Check the current weather (only visible outdoors).";
    public string Usage => "weather";
    public string Example => "weather";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (!world.Rooms.TryGetValue(player.CurrentRoomId, out var room) || !room.IsOutside)
        {
            Console.WriteLine("You can't see the sky from in here.");
            return;
        }

        string line = world.CurrentWeather switch
        {
            Enums.Weather.Clear => "The sky is clear.",
            Enums.Weather.Cloudy => "The sky is overcast.",
            Enums.Weather.Raining => "Rain is falling steadily.",
            Enums.Weather.Storming => "A storm rages overhead.",
            Enums.Weather.Snowing => "Snow is falling.",
            _ => $"The weather is {world.CurrentWeather}."
        };
        Helpers.ColorConsole.WriteLine(line, ConsoleColor.Blue);
    }
}
