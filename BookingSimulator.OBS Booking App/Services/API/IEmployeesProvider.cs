using OBS_Booking_App.Models;
using System.Collections.Generic;

namespace OBS_Booking_App.Services.API
{
    /// <summary>
    /// Definiert einen generischen Vertrag für Datenquellen, die Mitarbeiterinformationen (Employee) bereitstellen.
    /// 
    /// Dieses Interface wird von mehreren Implementierungen verwendet, z. B.:
    /// - <see cref="EmployeesApiConfiguration"/>: Lädt Daten dynamisch über die OBS-API.
    /// - <see cref="EmployeesAppsettingsConfiguration"/>: Lädt Daten statisch aus der appsettings.json.
    /// 
    /// Verwendet im zentralen EmployeeStore zur dynamischen Auswahl der besten Datenquelle basierend auf Datenqualität.
    /// </summary>
    public interface IEmployeesProvider
    {
        List<Employee> Employees { get; }
    }
}
