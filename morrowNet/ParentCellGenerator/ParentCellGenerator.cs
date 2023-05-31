namespace morrowNet;

public class ParentCellGenerator : IParentCellGenerator
{
    public IEnumerable<dynamic> CreateParentCellsList(IEnumerable<dynamic> morrowind, IEnumerable<dynamic> bcom)
    {
        Console.WriteLine("Creating Parent interior cell list...");
        
        var list = new List<dynamic>();

        var mwExternalParents = GetExteriorCells(morrowind).ToList();
        var bcomExternalParents = GetExteriorCells(bcom).ToList();

        foreach (var entry in mwExternalParents)
        {
            var listCount = list.Count;

            list.AddRange(bcomExternalParents.Where(bcomEntry => entry.data.grid == bcomEntry.data.grid));

            if (list.Count > listCount)
            {
                continue;
            }
            
            list.Add(entry);
        }

        return list;
    }
 
    public IEnumerable<dynamic> GetExteriorCells(IEnumerable<dynamic> file)
    {
        return file.Where(entry => entry.type.ToString() == "Cell")
            .Where(entry => !CellMethods.CellMethods.IsInterior(entry))
            .ToList();
    }
}