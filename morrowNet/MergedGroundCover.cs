using morrowNet.Contants;
using morrowNet.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace morrowNet;

public class MergedGroundCover
{
    public static IEnumerable<dynamic> GenerateInitialMergedGroundCover(IEnumerable<dynamic> vardenfell,
        IEnumerable<dynamic> BCOMGroundCover)
    {
        Console.WriteLine("Generating initial merged file...");

        var newFile = new List<dynamic>();

        // Add header
        newFile.Add(new Header
        {
            type = "Header",
            flags = new[]
            {
                0, 0
            },
            version = 1.0,
            file_type = "Esp",
            author = "",
            description = "BCOM Groundcover file",
            num_objects = 1,
            masters = new[]
            {
                new List<dynamic>
                {
                    "Morrowind.esm",
                    79837557
                },
                new List<dynamic>
                {
                    "Tribunal.esm",
                    4565686
                },
                new List<dynamic>
                {
                    "Bloodmoon.esm",
                    9631798
                }
            }
        });

        //Add static entries from BCOM

        //If mesh contains a key from dictionary, replace ID with value from dictionary
        var BcomGroundCover = BCOMGroundCover.ToList();
        var staticFiles = BcomGroundCover.Where(entry => entry.type == "Static");
        foreach (var entry in staticFiles)
        {
            var meshString = (string)Convert.ToString(entry.mesh).Replace(@"\", @"\\");
            var meshLowered = meshString.ToLower();
            var meshDictionaryId = FloraAndKelpConstants.StaticMeshLowerCaseToId[meshLowered];

            entry.mesh = FloraAndKelpConstants.StaticMeshIdToCapitalisedMesh[meshDictionaryId];
            entry.id = FloraAndKelpConstants.StaticMeshIdToIdString[meshDictionaryId];

            newFile.Add(entry);
        }

        // Add cell records from vardenfell as base
        foreach (var entry in vardenfell)
        {
            if (entry.type != "Cell") continue;

            // Find the same record in BCOM (Based on data.grid), if it exists, update to BCOM parent deets
            var matchingFromBcom = BcomGroundCover.Where(bce => bce.type == "Cell"
                                                                && bce.data.grid == entry.data.grid);

            //Construct new cell with everything from bcom
            newFile.Add(matchingFromBcom.Any() ? matchingFromBcom : entry);
        }

        foreach (var entry in BCOMGroundCover)
        {
            if (entry.type != "Cell") continue;

            // Find any matching cells in Vardenfell
            var matchingFromVardenfell = vardenfell.Where(vf => vf.type == "Cell"
                                                                && vf.data.grid == entry.data.grid);

            if (!matchingFromVardenfell.Any())
            {
                newFile.Add(entry);
            }
        }

        return newFile;
    }

    public static List<dynamic> AddReferenceToMergedFileBasedOnGrid(IEnumerable<dynamic> mergedFile,
        dynamic gridToMatch, dynamic referenceToAdd)
    {
        var copy = mergedFile.ToList();
        foreach (var parentItem in copy)
        {
            if (parentItem.type != "Cell") continue;

            if (gridToMatch != parentItem.data.grid) continue;

            var refs = JArray.FromObject(parentItem.references);
            refs.Add(referenceToAdd);

            parentItem.references = refs;
            return copy;
        }

        return copy;
    }

    public static List<dynamic> RemoveReferenceFromMergedFile(IEnumerable<dynamic> mergedFile,
        dynamic referenceToRemove)
    {
        var copy = mergedFile.ToList();

        foreach (var parentItem in copy)
        {
            if (parentItem.type != "Cell") continue;

            var newReferenceList = new List<dynamic>();

            foreach (var entry in parentItem.references)
            {
                if (entry.mast_index == referenceToRemove.mast_index
                    && entry.refr_index == referenceToRemove.refr_index
                    && entry.id == referenceToRemove.id
                    && entry.temporary == referenceToRemove.temporary
                    && entry.translation == referenceToRemove.translation
                    && entry.rotation == referenceToRemove.rotation) continue;

                newReferenceList.Add(entry);
            }

            parentItem.references = JArray.FromObject(newReferenceList);
        }

        return copy;
    }

    public static List<dynamic> CreateFlatDynamicList(IEnumerable<dynamic> listofLists)
    {
        var flatList = new List<dynamic>();

        foreach (var list in listofLists)
        {
            flatList.AddRange(list);
        }

        return flatList;
    }
}