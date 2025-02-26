namespace Hawaii.Test.Models;

public class Feature
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Name { get; set; }

    public Transform Transform { get; set; }
}