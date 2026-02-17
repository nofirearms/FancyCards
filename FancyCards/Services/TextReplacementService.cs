using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Services
{

    public class TextReplacementService
    {
        private readonly DataService _dataService;

        public TextReplacementService(DataService dataService)
        {
            _dataService = dataService;
        }

        public async Task<string> ReplaceWithReplacementRules(string text)
        {
            var rules = await _dataService.GetTextReplacementRules();

            var result = text;

            foreach (var rule in rules)
            {
                result.ToLower().Replace(rule.Original, rule.Replacement);
            }

            return result;
        }

        public string RemoveSpaces(string text)
        {
            return text.Replace(" ", "");
        }

        public async Task<bool> ProcessAndCompareAsync(string text1, string text2)
        {
            var processed_text1 = RemoveSpaces(await ReplaceWithReplacementRules(text1.ToLower()));
            var processed_text2 = RemoveSpaces(await ReplaceWithReplacementRules(text2.ToLower()));

            return string.Equals(processed_text1, processed_text2);
        }
    }
}
