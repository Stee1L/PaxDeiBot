namespace PaxDeiBot.Models;

public class LinkedListItem<T>
{
    public T Value { get; set; }
    public List<LinkedListItem<T>> Dependencies { get; set; } = new List<LinkedListItem<T>>();
}