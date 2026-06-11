using TreasuryToolkit.Core.Models;

namespace TreasuryToolkit.Core.Contracts
{
    public interface ICompanyService
    {
        IReadOnlyList<CompanyModel> GetCompanyNames();
    }
}