using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Common;
using Play.Catalog.Contracts;

namespace Play.Catalog.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<Item> itemsRepository;
        private readonly IPublishEndpoint publishEndpoint;

        public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint)
        {
            this.itemsRepository = itemsRepository;
            this.publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
        {
            var items = (await itemsRepository.GetAllAsync())
                        .Select(item => item.AsDto());

            return Ok(items);
        }

        //GET /items/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
        {
            var item = await itemsRepository.GetAsync(id);

            if (item == null)
                return NotFound();

            return item.AsDto();
        }

        [HttpPost]
        public async Task<ActionResult<ItemDto>> Post(CreateItemDto createItemDto)
        {
            var item = new Item
            {
                Name = createItemDto.Name,
                Description = createItemDto.Description,
                Price = createItemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await itemsRepository.CreateAsync(item);

            //Publish Message
            await publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

            //201 //Agreggated Metadata
            return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
            //return Ok(item);
        }

        //PUT /items/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, UpdateItemDto updateItemDto)
        {
            var exisitingItem = await itemsRepository.GetAsync(id);

            if (exisitingItem == null)
                return NotFound();

            exisitingItem.Name = updateItemDto.Name;
            exisitingItem.Description = updateItemDto.Description;
            exisitingItem.Price = updateItemDto.Price;

            await itemsRepository.UpdateAsync(exisitingItem);

            //Publish Message
            await publishEndpoint.Publish(new CatalogItemUpdated(exisitingItem.Id, exisitingItem.Name, exisitingItem.Description));

            return NoContent();
        }

        //DELETE /items/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var item = await itemsRepository.GetAsync(id);

            if (item == null)
                return NotFound();

            await itemsRepository.RemoveAsync(item.Id);

            //Publish Message
            await publishEndpoint.Publish(new CatalogItemDeleted(id));

            return NoContent();
        }
    }
}