using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PaxDeiBot.Data;
using PaxDeiBot.Models;
using System.Linq;

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
    [HttpGet]
    public Task<List<Item>> GetItems() => 
        _dbContext.Items.AsNoTracking().ToListAsync();
    
    /// <summary>
    /// Поиск предмета по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор предмета</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<Item> GetItem(int id)
    {

        var item = await _dbContext.Items.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
        
        if (item == null)
            throw new Exception("Не найдено"); // Исключение, если задача не найдена

        return item;
    }
    
    // POST: api/items
    //[Route("api/items")]
    [HttpPost]
    public async Task PostTask([FromBody] Item item)
    {
        // Добавляем новый предмет в контекст данных
        _dbContext.Items.Add(item);
        await _dbContext.SaveChangesAsync();
        // после создания в клиенте необходимо актуализировать информацию
    }
    
    // PUT: api/items
    //[Route("api/items")]
    [HttpPut("{id}")]
    public async Task PutItem(int id, [FromBody] Item item)
    {
        var entity = await _dbContext.Items.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);

        if (entity == null)
            throw new Exception("Предмет по указанному идентификатору не найден");

        if (entity.Id != item.Id)
            // Исключение, если идентификатор не совпадает с идентификатором предмета
            throw new Exception("Идентификаторы предмета и заменяемого предмета не совпадают");

        _dbContext.Items.Update(item);
        await _dbContext.SaveChangesAsync();
    }
    
    // DELETE: api/items
    //[Route("api/items")]
    [HttpDelete("{id}")]
    public async Task DeleteItem(int id)
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
}