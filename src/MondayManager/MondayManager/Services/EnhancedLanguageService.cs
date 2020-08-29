using ServiceResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voicify.Sdk.Core.Models.Model;
using Voicify.Sdk.Webhooks.Services;
using Voicify.Sdk.Webhooks.Services.Definitions;

namespace MondayManager.Services
{
    public class EnhancedLanguageService : IEnhancedLanguageService
    {
        private IPhraseParserService _phraseParserService;
        public EnhancedLanguageService(IPhraseParserService phraseParserService)
        {
            _phraseParserService = phraseParserService;
        }
        public Task<Result<List<ProcessedLanguage>>> ProcessAll(string input, InteractionModel languageModel)
        {
            try
            {
                var intentMatches = new List<ProcessedLanguage>();
                foreach (var intent in languageModel.Intents)
                {
                    foreach (var utterance in intent.Utterances)
                    {
                        var slots = MatchSlots(input, utterance, intent);
                        if (slots != null && HasAllRequiredSlots(intent, slots))
                        {
                            intentMatches.Add(new ProcessedLanguage
                            {
                                Intent = intent.Name["voicify"],
                                IntentDisplayName = intent.DisplayName,
                                Slots = slots,
                                UtteranceMatched = utterance
                            });
                        }
                    }
                }

                return Task.FromResult<Result<List<ProcessedLanguage>>>(new SuccessResult<List<ProcessedLanguage>>(intentMatches));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Task.FromResult<Result<List<ProcessedLanguage>>>(new UnexpectedResult<List<ProcessedLanguage>>());
            }
        }
        /// <summary>
        /// Gets the matched slots. returns null if not a match
        /// </summary>
        /// <param name="input"></param>
        /// <param name="utterance"></param>
        /// <returns></returns>
        private Dictionary<string, string> MatchSlots(string input, string utterance, Intent intent)
        {
            // ASSUMPTIONS: No back to back slots - does not require exact match - only supports english
            input = CleanForNLU(input);
            utterance = CleanForNLU(utterance);
            var utteranceParts = _phraseParserService.SplitPhraseIntoParts(utterance).Where(part => !string.IsNullOrEmpty(part)).ToList();
            utteranceParts = utteranceParts.Where(u => !string.IsNullOrEmpty(u)).ToList();
            var remainingString = input.ToLower();
            var slots = new Dictionary<string, string>();
            var hasCarrier = false;
            var slotCount = 0;
            for (var i = 0; i < utteranceParts.Count; i++)
            {
                var part = utteranceParts[i]?.ToLower();
                var originalPart = utteranceParts[i];

                var isSlot = part.Contains("{") && part.Contains("}");
                if (isSlot)
                {
                    slotCount++;
                    var slot = originalPart.Replace("{", string.Empty).Replace("}", string.Empty);
                    var slotName = (slot.Contains('|') ? slot.Split('|')[0] : slot).Trim();
                    if (i == utteranceParts.Count - 1)
                    {
                        // ends on slot
                            slots.Add(slotName, remainingString);
                        remainingString = string.Empty; // we did it! clear it out
                    }
                    else
                    {
                        // slot mid-phrase check the next part and pull the slot out.
                        var nextPart = utteranceParts[i + 1]?.ToLower();
                        // check if the rest of the phrase has the next part 
                        if (!remainingString.Contains(nextPart))
                            return null; // doesn't match the rest of the carriers
                        var slotValue = remainingString.Substring(0, remainingString.IndexOf(nextPart));
                        slots.Add(slotName, slotValue);

                        remainingString = remainingString.Remove(0, slotValue.Length);
                    }
                    continue;
                }
                else if (!string.IsNullOrEmpty(part) && remainingString.StartsWith(part))
                {
                    if (string.IsNullOrEmpty(part)) continue;
                    hasCarrier = true;
                    var index = remainingString.IndexOf(part);
                    remainingString = remainingString.Remove(index, part.Length);
                }
                else if (!hasCarrier)
                {
                    return null;
                }
            }

            // we matched with some but not all of the phrase
            if (!string.IsNullOrEmpty(remainingString) || slots.Count < slotCount)
                return null;

            return slots;
        }

        private string CleanForNLU(string input)
        {
            return input
                .Trim()
                .Trim('.')
                .Trim('-')
                .Replace("!", "")
                .Replace("?", "")
                .Replace(":", "")
                .Replace(";", "");
        }

        private bool HasAllRequiredSlots(Intent intent, Dictionary<string, string> slots)
        {
            var requiredSlots = intent?.Slots?.Where(s => s.Required == true) ?? new List<Slot>();
            foreach (var requiredSlot in requiredSlots)
            {
                if (!slots.Any(kvp => kvp.Key.ToLower() == requiredSlot.Name.ToLower()))
                    return false;
            }

            return true;
        }


        public async Task<Result<ProcessedLanguage>> Process(string input, InteractionModel languageModel)
        {
            try
            {
                var intentMatchesResult = await ProcessAll(input, languageModel);

                if (intentMatchesResult?.ResultType != ResultType.Ok)
                    return new InvalidResult<ProcessedLanguage>(intentMatchesResult.Errors?.FirstOrDefault());

                var matches = intentMatchesResult.Data;

                if (matches.Count <= 1)
                {
                    return new SuccessResult<ProcessedLanguage>(matches.FirstOrDefault());
                }


                var best = matches.FirstOrDefault();
                foreach (var match in matches)
                {
                    // exact match with no slots is best
                    if (match.Slots.Count == 0)
                    {
                        best = match;
                        break;
                    }

                    // with slots - more slots is better
                    if (match.Slots.Count > best.Slots.Count)
                    {
                        best = match;
                        continue;
                    }
                    else if (match.Slots.Count == best.Slots.Count)
                    {
                        // shorter phrases mean closer match
                        var matchLength = string.Join("|", match.Slots.Select(kvp => kvp.Value).ToArray()).Length;
                        var bestMatchLength = string.Join("|", best.Slots.Select(kvp => kvp.Value).ToArray()).Length;
                        if (matchLength < bestMatchLength)
                        {
                            best = match;
                            continue;
                        }
                    }
                }
                return new SuccessResult<ProcessedLanguage>(best);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new UnexpectedResult<ProcessedLanguage>();
            }
        }
    }
}
