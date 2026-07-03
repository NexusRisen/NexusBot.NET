using Discord.Commands;
using System.Threading.Tasks;
using SysBot.Pokemon;
using SysBot.Pokemon.Discord;

namespace SysBot.Pokemon.Discord.Commands.General;

public class PokemonCommand : ModuleBase<SocketCommandContext>
{
    [Command("pokemon")]
    [Summary("Shows the website that lists all available Pokémon in the game.")]
    public async Task PokemonAsync()
    {
        await ReplyAsync("Pick from the list on: https://pokemondb.net/pokedex then copy url into the hub.").ConfigureAwait(false);
    }
}
