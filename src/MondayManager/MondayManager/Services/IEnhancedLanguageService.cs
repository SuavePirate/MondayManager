using ServiceResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voicify.Sdk.Core.Models.Model;
using Voicify.Sdk.Webhooks.Services.Definitions;

namespace MondayManager.Services
{
    public interface IEnhancedLanguageService : IPatternMatchingLanguageService
    {
        Task<Result<ProcessedLanguage>> Process(string input, InteractionModel languageModel);
    }
}
