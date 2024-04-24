using System.ComponentModel.Design;
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
    [HttpGet("page")]
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
                (_, item1) => item1);

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
        var parents = GetParentsChain(new LinkedListItem<Guid>()
        {
            Value = id,
            Dependencies = new List<LinkedListItem<Guid>>()
        }, 1);

        if (parents.Dependencies.Any())
        {
            throw new Exception("Сначала удали из рецепта другого предмета этот перед его удалением.");
        }

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
    public Task<Guid> AppendChild(Guid parent, Guid child, [FromBody] long count)
    {
        if (count == 0)
        {
            throw new Exception("4e eblan, количество меньше нуля не бывает чел.. Чеееееееееееелллл...");
        }


        var childrenTree = GetChildrenTreeChain(new LinkedListItem<Guid>() {Value = child});
        var dependencies = DependenciesToList(childrenTree);

        if (dependencies.Any(f => f == parent))
            throw new Exception("Циклическая зависимость");


        var parentTree = GetChildrenTreeChain(new LinkedListItem<Guid>() {Value = parent});
        var parentChildren = DependenciesToList(parentTree);

        if (parentChildren.Any(f => f == child))
        {
            throw new Exception("Уже есть такой предмет в рецепте");
        }

        var component = new ItemComponent()
        {
            ParentId = parent,
            Id = Guid.NewGuid(),
            ChildId = child,
            Count = count
        };
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

    [HttpGet("dependencies/{id:guid}/depth/{depth:int}")]
    public Task<DependenciesModel> GetDependencies(Guid id, int depth)
    {
        var item = _dbContext.Items.FirstOrDefault(f => f.Id == id);
        var children = GetChildrenTreeChain(new LinkedListItem<Guid>()
        {
            Value = id
        });
        var parents = GetParentsChain(new LinkedListItem<Guid>()
        {
            Value = id
        });

        var listIds = DependenciesToList(parents).Concat(DependenciesToList(children)).ToList();

        var items = _dbContext.Items.Where(f => listIds.Any(v => v == f.Id)).ToList();

        return Task.FromResult(MakeDependencies(item, parents.Dependencies, children.Dependencies, items));
    }

    private DependenciesModel MakeDependencies(Item item, List<LinkedListItem<Guid>> parents,
        List<LinkedListItem<Guid>> children,
        List<Item> items)
    {
        var model = new DependenciesModel()
        {
            Id = item.Id,
            Description = item.Description,
            Name = item.Name,
            NeedForParents = new List<DependenciesModel>(),
            Components = new List<DependenciesModel>()
        };

        foreach (var parent in parents)
        {
            item = items.FirstOrDefault(v => v.Id == parent.Value);
            if (item == null)
                continue;
            model.NeedForParents.Add(MakeDependencies(item, parent.Dependencies, new List<LinkedListItem<Guid>>(),
                items));
        }

        foreach (var child in children)
        {
            item = items.FirstOrDefault(v => v.Id == child.Value);
            if (item == null)
                continue;

            model.Components.Add(MakeDependencies(item,
                new List<LinkedListItem<Guid>>(),
                child.Dependencies,
                items));
        }

        return model;
    }

    private LinkedListItem<Guid> GetChildrenTreeChain(LinkedListItem<Guid> parent)
    {
        parent.Dependencies.AddRange(
            _dbContext
                .ItemComponents
                .Where(v => v.ParentId == parent.Value)
                .ToList()
                .Select(f =>
                    new LinkedListItem<Guid>()
                    {
                        Value = f.ChildId,
                        Dependencies = new List<LinkedListItem<Guid>>()
                    }));

        foreach (var component in parent.Dependencies)
            GetChildrenTreeChain(component);

        return parent;
    }

    private LinkedListItem<Guid> GetParentsChain(LinkedListItem<Guid> rootForItem, long depth = -1L)
    {
        rootForItem.Dependencies = _dbContext
            .ItemComponents
            .Where(f => f.ChildId == rootForItem.Value)
            .ToList()
            .Select(f =>
                new LinkedListItem<Guid>()
                {
                    Value = f.ParentId,
                    Dependencies = new List<LinkedListItem<Guid>>() {rootForItem}
                }).ToList();

        if (depth == 0)
        {
            return rootForItem;
        }

        foreach (var parent in rootForItem.Dependencies)
            GetParentsChain(parent, --depth);

        return rootForItem;
    }

    private List<Guid> DependenciesToList(LinkedListItem<Guid> item, long depth = -1L)
    {
        var list = new List<Guid>();

        list.AddRange(item.Dependencies.Select(f => f.Value));

        if (depth == 0)
            return list;

        foreach (var dependency in item.Dependencies)
        {
            // list.Add(dependency.Value);
            list.AddRange(DependenciesToList(dependency, --depth));
        }

        return list;
    }

    private Guid GetRoot(Guid rootForItem)
    {
        var item = _dbContext.ItemComponents.FirstOrDefault(f => f.ChildId == rootForItem);
        return item == null
            ? rootForItem
            : GetRoot(item.ParentId);
    }
}