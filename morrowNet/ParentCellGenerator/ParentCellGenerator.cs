namespace morrowNet;

public class ParentCellGenerator : IParentCellGenerator
{
    public IEnumerable<dynamic> GetExteriorCells(IEnumerable<dynamic> file)
    {
        return file.Where(entry => entry.type.ToString() == "Cell")
            .Where(entry => !CellMethods.CellMethods.IsInterior(entry))
            .ToList();
    }
}