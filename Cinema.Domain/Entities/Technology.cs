using Cinema.Domain.Common;
using Cinema.Domain.Enums;

namespace Cinema.Domain.Entities;

public class Technology
{
    public EntityId<Technology> Id { get; }
    public string Name { get; private set; }
    public TechnologyType Type { get; private set; }

    private Technology(
        EntityId<Technology> id,
        string name,
        TechnologyType type)
    {
        Id = id;
        Name = name;
        Type = type;
    }

    public static Technology New(EntityId<Technology> id, string name, TechnologyType type)
        => new(id, name, type);
}