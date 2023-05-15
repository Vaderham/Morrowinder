// See https://aka.ms/new-console-template for more information

// 1. Look through all the objects in the file => Find any Cells references, where data.flags != 5
// 2. In each cell ref, find any grass or kelp ID's that appear in our refs list
// 3. If we find any with a matching ID, remove it
// 4. Somehow, print out the result

// step 1: 
// Update to Beautiful cities of morrowind.json
// 1. Look through all the objects in the file => Find any Cells references + where data.flags != 5 (don't touch interior cells!)
// 2. In each cell ref, find any grass or kelp ID's that appear in our refs list
// 3. If we find any with a matching ID, remove it
// 4. Somehow, print out the result
// Convert that back to ESP - Replaces the original BCOM ESP

// WE then need a basic groudcover ESP. 
// Create this by taking original BCOM json and deleting everything except for the ones we removed in step 1
// Keep header, keep all objects of type static that contain id's from refs grass kelp id's list. keep all cell references to the grasses also
// Update all mesh filepaths in static to grass, eg. "mesh": "grass\\Flora_Ash_Grass_B_01"

// new BCOM.json - with all references deleted
// new groundCover file - stripped back to only include meshes from grass and kelp


using morrowNet;
using morrowNet.CellMethods;
using morrowNet.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var streamReader1 = new StreamReader("C:/vardenfell.json");
var vardenfellStream = streamReader1.ReadToEnd();
dynamic vardenfell = JsonConvert.DeserializeObject(vardenfellStream);

var streamReader2 = new StreamReader("C:/fileTwoV4.json");
var groundcoverStream = streamReader2.ReadToEnd();
dynamic groundcover = JsonConvert.DeserializeObject(groundcoverStream);

var initialMergedGroundCover = GenerateInitialMergedGroundCover(vardenfell, groundcover);

ProcessBCOMReferenceArrays(groundcover, vardenfell, initialMergedGroundCover);

/*var parentCellGenerator = new ParentCellGenerator();
var exterior = parentCellGenerator.GetExteriorCells(items);

JsonFileUtils.SimpleWrite(JsonConvert.SerializeObject(exterior), "bcomExt.json");*/

/*var newList1= GenerateEspReplacer(items, FloraAndKelpConstants.FloraAndKelpNames);
JsonFileUtils.SimpleWrite(JsonConvert.SerializeObject(newList1), "fileOne.json");

var newList2 = GenerateGroundCover(items, FloraAndKelpConstants.FloraAndKelpNames);
JsonFileUtils.SimpleWrite(JsonConvert.SerializeObject(newList2), "FileTwo.json");*/

static IEnumerable<dynamic> GenerateInitialMergedGroundCover(IEnumerable<dynamic> vardenfell, IEnumerable<dynamic> BCOMGroundCover)
{
    var newFile = new List<dynamic>();
    
    // Add header
    newFile.Add(new Header
    {
        type = "Header",
        Flags = new []
        {
            0, 0
        },
        Version = 1.0,
        FileType = "Esp",
        Author = "",
        Description = "BCOM Groundcover file",
        NumObjects = 1,
        Masters = new []
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
    var BcomGroundCover = BCOMGroundCover.ToList();
    newFile.AddRange(BcomGroundCover.Where(entry => entry.type == "Static").ToList());

    // Add cell records from vardenfell as base
    foreach (var entry in vardenfell)
    {
        if (entry.type != "Cell") continue;
        
        // Find the same record in BCOM (Based on data.grid), if it exists, update to BCOM parent deets
        // If it doesn't exist in BCOM, just add as is?
        var matchingFromBcom = BcomGroundCover.Where(bce => bce.type == "Cell" 
                                                            && bce.data.grid == entry.data.grid);

        if (matchingFromBcom.Any())
        {
            //Construct new cell with everything from bcom
            var bcomRecord = matchingFromBcom.First();
            bcomRecord.references = Array.Empty<dynamic>();
        }
        
        // If entry doesn't exist in BCOM, keep it as is from vardenfell??
        //newFile.Add(entry);
    }
    
    return newFile;
}

static IEnumerable<dynamic> ProcessBCOMReferenceArrays(IEnumerable<dynamic> BCOMGroundCover, IEnumerable<dynamic> vardenfell, IEnumerable<dynamic> initialMerged)
{
    var bcomCells = BCOMGroundCover.Where(entry => entry.type == "Cell");

    //Create a local copy of initial merged file, we can pass back after making our changes.
    var parentFileCopy = initialMerged.ToList();

    foreach (var bcomCell in bcomCells)
    {
        foreach (var bcomReference in bcomCell.references)
        {
            if (bcomReference.mast_index == 0)
            {
                // Find corresponding parent cell in initialMerged, add it to 
                foreach (var parentItem in parentFileCopy)
                {
                    if (parentItem.type != "Cell" || bcomCell.data.grid != parentItem.data.grid) continue;
                    
                    var refs = new List<dynamic>().AddRange(parentItem.references);
                    refs.Add(bcomReference);

                    parentItem.references = refs;
                }
            }
            
            // If mast index matches morrowind.esm as per bcom header
            if (bcomReference.mast_index == 1)
            {
                 // Find a matching refr_index field in vardenfell
                    // If no matching refr_index, throw an error
                var matchingVFReferences = vardenfell.Select(entry => entry.references)
                .Where(vfreference => vfreference.refr_index == bcomReference.refr_index);

                if (!matchingVFReferences.Any())
                {
                    throw new Exception($"ERROR: Unable to merge files: CELL reference refr_index {bcomReference.refr_index} in [file name] " +
                                        "from master file [master file name (in this case, Morrowind.esm)] Groundcover plugin is missing from master file. Process aborted");
                }

                if (bcomReference.moved_cell)
                {
                    // If moved grid coordinates match another CELL parent record in Vvardenfell Groundcover,
                    // omit Vvardenfell Groundcover cell reference subrecord matched in steps 1b & 2b from merged file, 
                    //          i.e. Remove matchingVFReferences from merged file 
                    // and insert whole BCOM subrecord as-is to new CELL parent subrecord in merged file
                    var matchingVardenfeelParent =
                        vardenfell.Where(entry => entry.data.grid == bcomReference.moved_cell);

                    if (matchingVardenfeelParent.Any())
                    {
                        foreach (var entry in parentFileCopy)
                        {
                            if (entry.type == "Cell")
                            {
                                
                            }
                        }
                    }
                }
            }
        }
    }

    return parentFileCopy;
}

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
            if (CellMethods.IsInterior(item))
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

            if (!CellMethods.IsInterior(espType) && filteredReferences.Count > 0)
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