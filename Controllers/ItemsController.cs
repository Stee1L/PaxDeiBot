using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PaxDeiBot.Controllers.InputModels;
using PaxDeiBot.Controllers.ViewModels;
using PaxDeiBot.Data;
using PaxDeiBot.Models;

namespace PaxDeiBot.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly ItemDbContext _dbContext;

    public ItemsController(ItemDbContext context)
    {
        _dbContext = context;
    }

    /// <summary>
    /// Получаем все предметы из базы данных и передаем их пользователю через API
    /// </summary>
    /// <returns>Предметы из БД</returns>
    [HttpGet("components")]
    public Task<List<ItemShortViewModel>> GetItemsWithComponents() =>
        _dbContext
            .Items
            .AsNoTracking()
            .Select(f => f.ToShortViewModel())
            .ToListAsync();

    // [HttpGet("test")]
    // public async Task<List<Item>> GetItems2()
    // {
    //     var items = _dbContext.Items.GroupJoin(_dbContext.Items, item => item.ComponentOfItemIds, item => item.Id, (item, enumerable) => new);
    // }

    /// <summary>
    /// Поиск предмета по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор предмета</param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    public async Task<ItemFullViewModel> GetItem(Guid id)
    {
        var item = await _dbContext
            .Items
            .AsNoTracking()
            .Where(v => v.Id == id)
            .FirstOrDefaultAsync();

        if (item == null)
            throw new Exception("Не найдено"); // Исключение, если задача не найдена

        var children = _dbContext
            .ItemComponents
            .AsNoTracking()
            .Where(v => v.ParentId == id)
            .Join(_dbContext.Items, 
                component => component.ChildId, 
                item1 => item1.Id, 
                (_, item1) => item);

        return item.ToFullViewModel(children);
    }

    // POST: api/items
    //[Route("api/items")]
    [HttpPost]
    public async Task<Guid> Initialize([FromBody] ItemInputModel item)
    {
        var itemEntity = new Item()
        {
            Name = item.Name,
            Id = Guid.NewGuid(),
            Description = item.Description
        };
        // Добавляем новый предмет в контекст данных
        _dbContext.Items.Add(itemEntity);
        await _dbContext.SaveChangesAsync();
        // после создания в клиенте необходимо актуализировать информацию

        return itemEntity.Id;
    }

    // PUT: api/items
    //[Route("api/items")]
    [HttpPut("{id:guid}")]
    public async Task PutItem(Guid id, [FromBody] ItemInputModel item)
    {
        var entity = await _dbContext.Items.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);

        if (entity == null)
            throw new Exception("Предмет по указанному идентификатору не найден");

        entity.Description = item.Description;
        entity.Name = item.Name;
        
        _dbContext.Items.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    // DELETE: api/items
    //[Route("api/items")]
    [HttpDelete("{id:guid}")]
    public async Task DeleteItem(Guid id)
    {
        // Поиск по идентификатору
        var entity = await _dbContext.Items.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);

        if (entity == null)
            // Исключение если не найдено
            throw new Exception("Предмет по указанному идентификатору не найден");

        // Удаляем предмет из контекста данных
        _dbContext.Items.Remove(entity);
        // Cохраняем изменения
        await _dbContext.SaveChangesAsync();
    }

    [HttpPost("{parent:guid}/child/{child:guid}")]
    public Task<Guid> AppendChild(Guid parent, Guid child)
    {
        var allowStatusRequest = _dbContext.Items.Any(v => v.Id == parent) &&
                                 _dbContext.Items.Any(f => f.Id == child) &&
                                 _dbContext.ItemComponents.All(f => f.ParentId != child && f.ChildId != parent);
        if (!allowStatusRequest)
            throw new Exception("tyt sms");

        var component = new ItemComponent()
        {
            ParentId = parent,
            Id = Guid.NewGuid(),
            ChildId = child
        };

        //155e87ed-adb1-47e6-a6e8-6546da4dc5a6
        
        _dbContext.ItemComponents.Add(component);
        _dbContext.SaveChanges();
        return Task.FromResult(component.Id);
    }

    [HttpDelete("{parent:guid}/child/{child:guid}")]
    public async Task Remove(Guid parent, Guid child)
    {
        var itemComponent = await _dbContext
            .ItemComponents
            .FirstOrDefaultAsync(f => f.ParentId == parent && f.ChildId == child);
        
        if (itemComponent == null)
            throw new Exception("tyt sms");

        _dbContext.ItemComponents.Remove(itemComponent);
    }
}