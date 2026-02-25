namespace Domain.Interfaces.Repository
{
    public interface ICodeChallengeRepository
    {
        Task Add(CodeChallenge codeChallenge);
        Task Update(CodeChallenge codeChallenge);
        Task<CodeChallenge?> GetByReferenceAndCodeValid(string referenceId, string code);
    }
}
