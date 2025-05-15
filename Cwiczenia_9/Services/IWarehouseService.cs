using Cwiczenia_9.Models;
using System.Threading.Tasks;

namespace Cwiczenia_9.Services
{
    public interface IWarehouseService
    {
        /// <summary>
        /// Dodaje produkt do magazynu przy użyciu logiki C# i klas SqlConnection/SqlCommand.
        /// Zwraca identyfikator nowego rekordu w tabeli Product_Warehouse.
        /// </summary>
        Task<int> AddProductToWarehouseAsync(ProductWarehouseRequest request);

        /// <summary>
        /// Dodaje produkt przy użyciu procedury składowanej AddProductToWarehouse.
        /// Zwraca identyfikator nowego rekordu.
        /// </summary>
        Task<int> AddProductToWarehouseUsingSPAsync(ProductWarehouseRequest request);
    }
}