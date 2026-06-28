using PayrollDashboard.Models;

namespace PayrollDashboard.Repositories;

public interface IPayrollRepository
{
    void Save(PayrollSlip slip);

    IEnumerable<PayrollSlip> GetAll();

    bool ExistsByFileName(string fileName);
}
