namespace Gilad.PowerToys.Run.Plugin.DuckDuckGo.UriHelper
{
    public class DuckDuckGoUriParser : ExtendedUriParser
    {
        public new bool TryParse(string input, out System.Uri result)
        {
            if (!base.TryParse("duckduckgo.com", out var duckDuckGoUri))
            {
                result = default;
                return false;
            }

            // https://duckduckgo.com/?q=test
            var duckDuckGoSearchUriBuilder = new System.UriBuilder(duckDuckGoUri);
            duckDuckGoSearchUriBuilder.Query = $"q={input}";

            result = duckDuckGoSearchUriBuilder.Uri;
            return true;
        }
    }
}
