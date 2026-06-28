using PayrollDashboard.Models;

namespace PayrollDashboard.Repositories;

public interface IPayrollRepository
{
    void Save(PayrollSlip slip);

    IEnumerable<PayrollSlip> GetAll();

    PayrollSlip? GetById(int id);

    bool ExistsByFileName(string fileName);
}
