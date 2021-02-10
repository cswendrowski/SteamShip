using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SteamShip.Shared;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SteamShip.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SteamController : ControllerBase
    {

        private const string ApiKey = "B809FE9D19152246D16A66E7ECE22ADF";
        private readonly ILogger<SteamController> _logger;

        public SteamController(ILogger<SteamController> logger)
        {
            _logger = logger;
        }



        [HttpPost("[action]")]
        public async Task<IEnumerable<SteamGame>> GetGamesInCommon([FromBody] List<SteamProfile> steamProfiles)
        {
            SteamOwnedGames ownedGames = null;
            SteamGame[] sharedGames = null;

            foreach(var steamProfile in steamProfiles) {
                ownedGames = await GetSteamGamesFromId(steamProfile.Id);

                if(sharedGames == null) {
                    sharedGames = ownedGames.games;
                } else {
                    IEnumerable<SteamGame> shared = sharedGames.Intersect(ownedGames.games, new SteamGameComparer());
                    sharedGames = shared.ToArray();
                }

            }

            //At this point the hours played is added up from each player. Now we have to average them. 
            foreach(var game in sharedGames) {
                game.playtime_forever /= steamProfiles.Count;
            }

            //Now we sort by hours played!
            Array.Sort(sharedGames, new SteamGameComparer());

            
            return sharedGames;
        }

        private async Task<SteamOwnedGames> GetSteamGamesFromId(string id)
        {
            string getOwnedGamesUrl = $"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={ApiKey}&steamid={id}&include_appinfo=true&include_played_free_games=true&format=json";

            using (var client = new HttpClient()) {
                var httpResponse = await client.GetAsync(getOwnedGamesUrl);
                string responseString = await httpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SteamOwnedGamesRoot>(responseString).response;
            }
        }

        [HttpGet]
        public async Task<SteamProfile> GetSteamProfile(string query)
        {

            var steamIdRegex = new Regex(@"\d{17}");

            var profile = new SteamProfile();

            if (!steamIdRegex.IsMatch(query))
            {
                profile.Id = GetSteamIdFromName(query);
            }
            else
            {
                profile.Id = query;
            }

            var fullProfile = await GetFullSteamProfile(profile.Id);

            profile.Name = fullProfile.Realname;
            profile.Persona = fullProfile.Personaname;
            profile.AvatarUrl = fullProfile.Avatar.AbsoluteUri;

            return profile;
        }

        private async Task<Player> GetFullSteamProfile(string id)
        {
            var fullProfileUrl = $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={ApiKey}&steamids={id}";

            using (var client = new HttpClient())
            {
                var httpResponse = await client.GetAsync(fullProfileUrl);
                string responseString = await httpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<FullProfile>(responseString).Response.Players.First();
            }
        }

        private string GetSteamIdFromName(string name)
        {
            var steamCommunityProfileUrl = $"https://steamcommunity.com/id/{name}/?xml=1";

            try
            {
                var xml = XDocument.Load(steamCommunityProfileUrl);
                return xml.Element("profile").Element("steamID64").Value;
            }
            catch
            {
                return null;
            }
        }
    }
}
