using Microsoft.AspNetCore.Mvc;
using Cwiczenia_9.Models;
using Cwiczenia_9.Services;
using System.Threading.Tasks;

namespace Cwiczenia_9.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        /// <summary>
        /// Endpoint realizujący scenariusz wykorzystujący logikę C# i klasy SqlConnection/SqlCommand.
        /// Przykładowe żądanie JSON:
        /// {
        ///   "IdProduct": 1,
        ///   "IdWarehouse": 2,
        ///   "Amount": 20,
        ///   "CreatedAt": "2012-04-23T18:25:43.511Z"
        /// }
        /// </summary>
        [HttpPost("add")]
        public async Task<IActionResult> AddProductToWarehouse([FromBody] ProductWarehouseRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                int newId = await _warehouseService.AddProductToWarehouseAsync(request);
                return Ok(new { NewId = newId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (System.Exception ex)
            {
                // W rzeczywistej aplikacji warto logować wyjątki oraz nie ujawniać szczegółów błędu.
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint realizujący tę samą logikę przy użyciu procedury składowanej.
        /// </summary>
        [HttpPost("add-sp")]
        public async Task<IActionResult> AddProductToWarehouseUsingSP([FromBody] ProductWarehouseRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                int newId = await _warehouseService.AddProductToWarehouseUsingSPAsync(request);
                return Ok(new { NewId = newId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
