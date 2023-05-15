namespace morrowNet;

public interface IParentCellGenerator
{
    IEnumerable<dynamic> GetExteriorCells(IEnumerable<dynamic> file);
}