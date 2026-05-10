using System;

namespace LumStoreAPI.Application.DTOs.TreeNodeManageDTO;

public class TreeLinked
{
    public string Name { get; set; } = default!;
    public int NodeId { get; set; }
    public int ParentNodeId { get; set; }
}
