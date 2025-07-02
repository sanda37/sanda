using Microsoft.AspNetCore.Mvc;
using sanda.Repositories;


namespace sanda.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceItemsController : ControllerBase
    {
        private readonly IServiceRepository _serviceRepository;

        public ServiceItemsController(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceItem>>> GetAll()
        {
            return Ok(await _serviceRepository.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceItem>> GetById(int id)
        {
            var item = await _serviceRepository.GetByIdAsync(id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceItem>> Create(ServiceItem serviceItem)
        {
            await _serviceRepository.AddAsync(serviceItem);
            return CreatedAtAction(nameof(GetById), new { id = serviceItem.Id }, serviceItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ServiceItem serviceItem)
        {
            if (id != serviceItem.Id)
                return BadRequest();

            await _serviceRepository.UpdateAsync(serviceItem);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _serviceRepository.DeleteAsync(id);
            return NoContent();
        }



        [HttpGet("category/{categoryName}")]
        public async Task<IActionResult> GetByCategory(string categoryName)
        {
            var services = (await _serviceRepository.GetAllAsync()).Where(s => s.Category == categoryName).ToList();
            if (!services.Any())
                return NotFound();

            return Ok(services);
        }

        [HttpPatch("{id}/update-image")]
        public async Task<IActionResult> UpdateImage(int id, [FromBody] string imageUrl)
        {
            try
            {
                await _serviceRepository.UpdateImageAsync(id, imageUrl);
                return Ok(new { message = "Image updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to update image: {ex.Message}" });
            }
        }
    }
}