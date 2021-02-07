﻿using Microsoft.AspNetCore.Mvc;
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
        public async Task<IEnumerable<SteamGame>> GetGamesInCommon([FromBody] List<string> steamIds)
        {
            SteamOwnedGames ownedGames = null;

            ownedGames = await GetSteamGamesFromId(steamIds[0]);

            return ownedGames.games;
        }

        private async Task<SteamOwnedGames> GetSteamGamesFromId(string id)
        {
            string getOwnedGamesUrl = $"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={ApiKey}&steamid={id}&format=json";

            using (var client = new HttpClient()) {
                HttpResponseMessage httpResponse = await client.GetAsync(getOwnedGamesUrl);
                string responseString = await httpResponse.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SteamOwnedGamesRoot>(responseString).response;
            }
        }

        [HttpGet]
        public SteamProfile GetSteamProfile(string id)
        {
            var steamIdRegex = new Regex(@"\d{17}");

            if (!steamIdRegex.IsMatch(id))
            {
                id = GetSteamIdFromName(id);
            }

            return new SteamProfile()
            {
                Id = id
            };
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
