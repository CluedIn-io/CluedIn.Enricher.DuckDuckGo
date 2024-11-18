using System;
using System.Threading.Tasks;
using CluedIn.Core;
using CluedIn.Core.Data.Vocabularies.Models;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.DuckDuckgo.Services
{
    public interface IVocabularyRepository
    {
        Task<Guid> AddVocabulary(ExecutionContext context, AddVocabularyModel model, string createdById, Guid organizationId);
        Task<Guid> AddVocabularyKey(AddVocabularyKeyModel model, ExecutionContext executionContext, string ownerId, bool doReinitialize = true);
        IVocabulary? GetVocabularyByKeyPrefix(ExecutionContext context, string keyPrefix, bool caseInsensitive = false);
        VocabularyKey? GetVocabularyKeyByFullName(ExecutionContext context, string keyName);
        Task ActivateVocabulary(ExecutionContext context, Guid vocabularyId);
        Task ActivateVocabularyKey(ExecutionContext context, Guid vocabularyKeyId);
    }

    public class VocabularyRepository : IVocabularyRepository
    {
        public Task<Guid> AddVocabulary(ExecutionContext context, AddVocabularyModel model, string createdById, Guid organizationId)
        {
            var vocabRepository = GetVocabularyRepository(context);
            var addVocabMethodInfo = vocabRepository.GetType().GetMethod("AddVocabulary");
            return (Task<Guid>)addVocabMethodInfo.Invoke(vocabRepository, new object[] { model, createdById, organizationId });
        }

        public Task<Guid> AddVocabularyKey(AddVocabularyKeyModel model, ExecutionContext executionContext, string ownerId, bool doReinitialize = true)
        {
            var vocabRepository = GetVocabularyRepository(executionContext);
            var addVocabKeyMethodInfo = vocabRepository.GetType().GetMethod("AddVocabularyKey");
            return (Task<Guid>)addVocabKeyMethodInfo.Invoke(vocabRepository, new object[] { model, executionContext, Guid.Empty.ToString(), true });
        }

        public IVocabulary GetVocabularyByKeyPrefix(ExecutionContext context, string keyPrefix, bool caseInsensitive = false)
        {
            var vocabRepository = GetVocabularyRepository(context);
            var getVocabMethodInfo = vocabRepository.GetType().GetMethod("GetVocabularyByKeyPrefix");
            return (IVocabulary)getVocabMethodInfo.Invoke(vocabRepository, new object[] { keyPrefix, caseInsensitive });
        }

        public VocabularyKey GetVocabularyKeyByFullName(ExecutionContext context, string keyName)
        {
            var vocabRepository = GetVocabularyRepository(context);
            var getVocabKeyMethodInfo = vocabRepository.GetType().GetMethod("GetVocabularyKeyByFullName");
            return (VocabularyKey)getVocabKeyMethodInfo.Invoke(vocabRepository, new object[] { keyName });
        }

        public Task ActivateVocabulary(ExecutionContext context, Guid vocabularyId) 
        {
            var vocabRepository = GetVocabularyRepository(context);
            var activateVocab = vocabRepository.GetType().GetMethod("ActivateVocabulary");
            return (Task)activateVocab.Invoke(vocabRepository, new object[] { vocabularyId });
        }

        public Task ActivateVocabularyKey(ExecutionContext context, Guid vocabularyKeyId)
        {
            var vocabRepository = GetVocabularyRepository(context);
            var activateVocabKey = vocabRepository.GetType().GetMethod("ActivateVocabularyKey");
            return (Task)activateVocabKey.Invoke(vocabRepository, new object[] { vocabularyKeyId });
        }

        private object GetVocabularyRepository(ExecutionContext context)
        {
            var vocabularyRepositoryType = typeof(Integration.PrivateServices.PrivateServicesComponent).Assembly.GetType("CluedIn.Integration.PrivateServices.Vocabularies.IPrivateVocabularyRepository");
            var vocabRepository = context.ApplicationContext.Container.Resolve(vocabularyRepositoryType);

            return vocabRepository;
        }
    }
}
