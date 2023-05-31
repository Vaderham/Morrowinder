using Newtonsoft.Json.Linq;

namespace morrowNet;

public class GroundCoverGenerator
{
    static List<dynamic> GenerateGroundCover(dynamic parsedJson, List<string> refs)
{
    var newList = new List<dynamic>();

    foreach (var espType in parsedJson)
    {
        // If it's the header, keep it
        if (espType.type.ToString() != string.Empty && espType.type.ToString() == "Header")
        {
            newList.Add(espType);
            continue;
        }

        // If it's static and it's ID is in references list, keep it
        // Update it's path to have grass folder as parent
        if (espType.type.ToString() != string.Empty && espType.type.ToString() == "Static")
            if (refs.Contains(espType.id.ToString()))
            {
                string[] splitPath = espType.mesh.ToString().Split("\\");
                espType.mesh = "grass\\" + splitPath[1];

                newList.Add(espType);
                continue;
            }

        // If it's a cell ref, and exterior, and if it has something in it's references array, keep it and it's grass and kelp references
        if (espType.data != null && espType.data.flags != null && espType.type.ToString() == "Cell")
        {
            List<dynamic> filteredReferences = FilterReferencesToOnlyGrassAndKelp(espType.references, refs);

            if (!CellMethods.CellMethods.IsInterior(espType) && filteredReferences.Count > 0)
            {
                espType.references = JArray.FromObject(filteredReferences);

                newList.Add(espType);
            }
        }
    }

    // Scan through our refs list and if we don't have a static object for it, add one in
    var staticItemIds = newList.Where(espType => espType.type.ToString() != string.Empty && espType.type == "Static")
        .Select(item => item.id.ToString())
        .ToList();

    foreach (var refString in refs)
        if (!staticItemIds.Contains(refString))
            newList.Add(new
            {
                type = "Static",
                flags = new[] { 0, 0 },
                id = refString,
                mesh = $"grass\\{refString}.NIF"
            });

    return newList;
}
    static List<dynamic> FilterReferencesToOnlyGrassAndKelp(dynamic referencesList, ICollection<string> grassAndKelpList)
    {
        var filteredReferences = new List<dynamic>();

        foreach (var reference in referencesList)
            if (grassAndKelpList.Contains(reference.id.ToString()))
                filteredReferences.Add(reference);

        return filteredReferences;
    }
}