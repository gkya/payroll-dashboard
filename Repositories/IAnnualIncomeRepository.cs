using PayrollDashboard.Models;

namespace PayrollDashboard.Repositories;

public interface IAnnualIncomeRepository
{
    void Save(AnnualIncomeSlip slip);
    IEnumerable<AnnualIncomeSlip> GetAll();
    bool ExistsByHash(string hash);
}
