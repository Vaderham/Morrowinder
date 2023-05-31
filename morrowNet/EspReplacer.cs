using Newtonsoft.Json.Linq;

namespace morrowNet;

public class EspReplacer
{
    static List<dynamic> GenerateEspReplacer(dynamic parsedJson, List<string> listOfReferenceStrings)
    {
        var newEsp = new List<dynamic>();

        foreach (var item in parsedJson)
        {
            // If it's not a cell type, keep it.
            if (item.type.ToString() != string.Empty && item.type.ToString() != "Cell")
            {
                newEsp.Add(item);
                continue;
            }

            // If it's interior, keep it
            if (item.data != null && item.data.flags != null)
                if (CellMethods.CellMethods.IsInterior(item))
                {
                    newEsp.Add(item);
                    continue;
                }

            // Otherwise, go through the cells references, and if any id's are in our refs list, remove them.
            var references = new List<dynamic>();

            foreach (var reference in item.references)
                if (!listOfReferenceStrings.Contains(reference.id.ToString()))
                    references.Add(reference);

            var newRefsArray = JArray.FromObject(references);
            item.references = newRefsArray;

            // Only add the cell back to the file if it has references remaining.
            if (newRefsArray.Count > 0) newEsp.Add(item);
        }

        return newEsp;
    }

}