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

using System.Diagnostics;
using System.Text.RegularExpressions;
using morrowNet;
using morrowNet.Contants;
using morrowNet.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

Console.WriteLine("Reading in Vardenfell...");
var streamReader1 = new StreamReader("C:/vardenfell.json");
var vardenfellStream = streamReader1.ReadToEnd();
dynamic vardenfell = JsonConvert.DeserializeObject(vardenfellStream);

Console.WriteLine("Reading in Groundcover...");
var streamReader2 = new StreamReader("C:/fileTwoV4.json");
var groundcoverStream = streamReader2.ReadToEnd();
dynamic groundcover = JsonConvert.DeserializeObject(groundcoverStream);

Console.WriteLine("Reading in morrowind...");
var streamReader3 = new StreamReader("C:/morrowind.json");
var morrowindStream = streamReader3.ReadToEnd();
dynamic morrowind = JsonConvert.DeserializeObject(morrowindStream);

Console.WriteLine("Reading in bcom...");
var streamReader4 = new StreamReader("C:/morrowind.json");
var bcomStream = streamReader4.ReadToEnd();
dynamic bcom = JsonConvert.DeserializeObject(bcomStream);

var initialMergedGroundCover = GenerateInitialMergedGroundCover(vardenfell, groundcover);
var parentReference = new ParentCellGenerator().CreateParentCellsList(morrowind, bcom);

var processedFile = ProcessBcomReferenceArrays(groundcover, vardenfell, initialMergedGroundCover, parentReference);

var fileName = "processedReferenceArrays.json";

Console.WriteLine($"Writing to file {fileName}");

JsonFileUtils.SimpleWrite(JsonConvert.SerializeObject(processedFile), fileName);

//ProcessBCOMReferenceArrays(groundcover, vardenfell, initialMergedGroundCover);

static IEnumerable<dynamic> GenerateInitialMergedGroundCover(IEnumerable<dynamic> vardenfell, IEnumerable<dynamic> BCOMGroundCover)
{
    Console.WriteLine("Generating initial merged file...");
    
    var newFile = new List<dynamic>();
    
    // Add header
    newFile.Add(new Header
    {
        type = "Header",
        flags = new []
        {
            0, 0
        },
        version = 1.0,
        file_type = "Esp",
        author = "",
        description = "BCOM Groundcover file",
        num_objects = 1,
        masters = new []
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

static IEnumerable<dynamic> ProcessBcomReferenceArrays(IEnumerable<dynamic> BCOMGroundCover, 
    IEnumerable<dynamic> vardenfell, 
    IEnumerable<dynamic> initialMerged, 
    IEnumerable<dynamic> parentCellList)
{
    Console.WriteLine("Processing reference arrays...");
    
    var mergedCopy = initialMerged.ToList();
    var vardenfellEntryCount = vardenfell.Count();
    var bcomEntryCount = BCOMGroundCover.Count();
    var bcomCompletedCount = 0;
    var mergedCopies = 0;
    
    var countCompleted = 0;

    var vardenfellReferences = CreateFlatDynamicList(vardenfell.Where(vf => vf.type == "Cell").Select(vf => vf.references));

    foreach (var vardenfellParentEntry in vardenfell)
    {
        Console.WriteLine($"Completed {countCompleted} out of {vardenfellEntryCount}");
        
        if (vardenfellParentEntry.type != "Cell")
        {
            countCompleted++;
            continue;
        }

        foreach (var bcomParentEntry in BCOMGroundCover)
        {
            Console.WriteLine($"{bcomCompletedCount} bcom parent entries completed out of {bcomEntryCount}");
            
            if (bcomParentEntry.type != "Cell")
            {
                bcomCompletedCount++;
                continue;
            }
            
            foreach (var vardenfellReference in vardenfellParentEntry.references)
            {
                foreach (var bcomReference in bcomParentEntry.references)
                {
                    if (bcomReference.mast_index == 0)
                    {
                        mergedCopies++;
                        Console.WriteLine($"Merged {mergedCopies} copies.");
                        mergedCopy = AddReferenceToMergedFileBasedOnGrid(mergedCopy, bcomParentEntry.data.grid, bcomReference);
                        continue;
                    }
                    
                    //match on master index from BCOM. MW = 1
                    if (bcomReference.mast_index == 1)
                    {
                        //check if the refr_index in BCOM Groundcover matches a cell reference subrecord in Vvardenfell Groundcover.
                        if (vardenfellReferences.Any(vfr => vfr.refr_index == bcomReference.refr_index))
                        {
                            throw new Exception(
                                $"ERROR: Unable to merge files: CELL reference refr_index [{bcomReference.refr_index}] is missing from vardenfell");
                        }

                        if (bcomReference.refr_index != vardenfellReference.refr_index) continue;

                        if (bcomReference.moved_cell != null)
                        {
                            /*check what grid coordinates are entered after "moved cell". If grid coordinates match another CELL parent record in Vvardenfell Groundcover, 
                            omit Vvardenfell Groundcover cell reference subrecord matched in steps 1b & 2b from merged file, and insert whole BCOM subrecord as-is to new CELL parent 
                            subrecord in merged file */
                            var matchingFromVardenfell = vardenfell.Where(vf => vf.type.ToString() == "Cell" 
                                                                                && vf.data.grid == bcomReference.moved_cell);

                            if (matchingFromVardenfell.Any())
                            {
                                //Remove matching vardenfell reference 
                                mergedCopy = RemoveReferenceFromMergedFile(mergedCopy, vardenfellReference);
                                //Add BCOM refernec to parent reference array that matches moved_cell
                                mergedCopy = AddReferenceToMergedFileBasedOnGrid(mergedCopy, bcomReference.moved_cell, bcomReference);
                                continue;
                            }

                            /*
                              if "moved cell" grid coordinates do not match another CELL parent record in Vvardenfell Groundcover, locate corresponding grid coordinates in 'Cell Name Parent' 
                              reference list and insert new CELL parent record. Then, omit Vvardenfell Groundcover cell reference subrecord matched in steps 1b & 2b from merged file, 
                              and move whole BCOM subrecord as-is to new CELL parent subrecord in merged file.
                             */
                            var matchFromParentCellList = parentCellList.First(pcl => pcl.data.grid == bcomReference.moved_cell);
                            mergedCopy.Add(matchFromParentCellList);
                            
                            mergedCopy = RemoveReferenceFromMergedFile(mergedCopy, vardenfellReference);
                            mergedCopy = AddReferenceToMergedFileBasedOnGrid(mergedCopy, bcomReference.moved_cell, bcomReference);
                        }
                    }
                }
            }
            bcomCompletedCount++;
        }

        countCompleted++;
    }
    
    Console.WriteLine("Processing reference arrays complete.");
    return mergedCopy;
}

static List<dynamic> AddReferenceToMergedFileBasedOnGrid(IEnumerable<dynamic> mergedFile, dynamic gridToMatch, dynamic referenceToAdd)
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

static List<dynamic> RemoveReferenceFromMergedFile(IEnumerable<dynamic> mergedFile, dynamic referenceToRemove)
{
    var copy = mergedFile.ToList();
    
    foreach (var parentItem in copy)
    {
        if (parentItem.type != "Cell") continue;

        var referenceList = (List<dynamic>) parentItem.references.ToList();
        var match = referenceList.Any(r => r.mast_index == referenceToRemove.mast_index
                                           && r.refr_index == referenceToRemove.refr_index
                                           && r.id == referenceToRemove.id
                                           && r.temporary == referenceToRemove.temporary
                                           && r.translation == referenceToRemove.translation
                                           && r.rotation == referenceToRemove.rotation);

        if (!match) continue;

        var newReferenceList = new List<dynamic>();

        foreach (var entry in parentItem.references)
        {
            if(entry.mast_index == referenceToRemove.mast_index
               && entry.refr_index == referenceToRemove.refr_index
               && entry.id == referenceToRemove.id
               && entry.temporary == referenceToRemove.temporary
               && entry.translation == referenceToRemove.translation
               && entry.rotation == referenceToRemove.rotation) continue;
            
            newReferenceList.Add(entry);
        }
        parentItem.references = newReferenceList;
    }
    return copy;
}

static List<dynamic> CreateFlatDynamicList(IEnumerable<dynamic> listofLists)
{
    var flatList = new List<dynamic>();
    
    foreach (var list in listofLists)
    {
        flatList.AddRange(list);
    }
    return flatList;
}