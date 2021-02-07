using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SteamShip.Shared;
using System.Text.RegularExpressions;
using System.Xml.Linq;

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
