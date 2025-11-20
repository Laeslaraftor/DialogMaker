using Newtonsoft.Json;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectLanguage : JsonData
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        #region Управление

        public override string ToString()
        {
            return $"{Id}/{Name}";
        }

        #endregion
    }
}
