﻿using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using Inquisition.Data;
using System.Linq;
using System.Diagnostics;

namespace Inquisition.Modules
{
    [Group("game")]
    public class GameCommands : ModuleBase<SocketCommandContext>
    {
        InquisitionContext db = new InquisitionContext();
        private readonly string Path = "";

        [Command("status")]
        [Summary("Returns if a game server is on")]
        public async Task StatusAsync(string name)
        {
            Data.Game game = db.Games.Where(x => x.Name == name).FirstOrDefault();
            if (game is null)
            {
                await ReplyAsync($"Sorry, {name} not found in the database");
                return;
            }

            bool ProcessRunning = ProcessDictionary.Instance.TryGetValue(game.Name, out Process p);
            bool GameMarkedOnline = game.IsOnline;

            if (!ProcessRunning && !GameMarkedOnline)
            {
                await ReplyAsync($"{game.Name} server is offline");
                return;
            } else if (ProcessRunning && !GameMarkedOnline)
            {
                await ReplyAsync($"{game.Name} has a process running, but is marked as offline in the database, " +
                    $"please let the knobhead who programmed this know abut this error, thanks");
                return;
            } else if (!ProcessRunning && GameMarkedOnline)
            {
                await ReplyAsync($"{game.Name} server is not running, but is marked as online in the database, " +
                    $"please let the knobhead who programmed this know abut this error, thanks");
                return;
            }
        }

        [Command("start")]
        [Summary("Starts up a game server")]
        public async Task StartGameAsync(string name)
        {
            Data.Game game = db.Games.Where(x => x.Name == name).FirstOrDefault();
            if (game is null)
            {
                await ReplyAsync($"Sorry, {name} not found in the database");
                return;
            }

            try
            {
                if (ProcessDictionary.Instance.TryGetValue(game.Name, out Process temp))
                {
                    await ReplyAsync($"{game.Name} is already running, version {game.Version} on port {game.Port}. " +
                        $"If you want to stop it, use the command: game stop {game.Name}.");
                    return;
                }

                Process p = new Process();
                p.StartInfo.FileName = Path + game.ExeDir;
                p.StartInfo.Arguments = game.Args;
                p.Start();

                ProcessDictionary.Instance.Add(game.Name, p);

                game.IsOnline = true;
                await db.SaveChangesAsync();

                await ReplyAsync($"{game.Name} server should be online in a few seconds, version {game.Version} on port {game.Port}");
            }
            catch (System.Exception ex)
            {
                await ReplyAsync($"Somethng went wrong, couldn't start {game.Name} server, please contact an Administrator.");
                System.Console.WriteLine(ex.Message);
                throw;
            }
        }

        [Command("stop")]
        [Summary("Stops a game server")]
        public async Task StopGameAsync(string name)
        {
            Data.Game game = db.Games.Where(x => x.Name == name).FirstOrDefault();
            if (game is null)
            {
                await ReplyAsync($"Sorry, {name} not found in the database");
                return;
            }

            try
            {
                if (ProcessDictionary.Instance.TryGetValue(game.Name, out Process p))
                {
                    p.CloseMainWindow();
                    p.Close();
                    ProcessDictionary.Instance.Remove(game.Name);

                    game.IsOnline = false;
                    await db.SaveChangesAsync();

                    await ReplyAsync($"{game.Name} server is shutting down");
                    return;
                }

                await ReplyAsync($"{Context.Message.Author.Mention}, {game.Name} doesn't seem to be running. " +
                    $"If you wish to start it up, use the command: game start {game.Name}");
            }
            catch (System.Exception ex)
            {
                await ReplyAsync($"Something went wrong, couldn't stop {game.Name} server, please contact the Administrator");
                System.Console.WriteLine(ex.Message);
                throw;
            }
        }

        [Command("list")]
        [Summary("Displays all the game servers with the corresponding port")]
        public async Task ServerListAsync()
        {
            List<Data.Game> Games = db.Games.ToList();
            EmbedBuilder builder = new EmbedBuilder
            {
                Color = Color.Orange,
                Title = "Domain: ffs.game-host.org",
                Description = "List of all game servers with ports"
            };

            foreach (Data.Game game in Games)
            {
                string status = game.IsOnline ? "Online " : "Offline ";
                string port = game.IsOnline ? $"on port {game.Port}" : "";
                builder.AddInlineField(game.Name,status + port);
            }
            await ReplyAsync($"Here's the server list {Context.User.Mention}:", false, builder.Build());
        }
    }
}
