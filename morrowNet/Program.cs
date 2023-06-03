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

//Read in files
dynamic vardenfell = JsonFileUtils.ReadInFile("C:/vardenfell.json", "Vardenfell");
dynamic groundcover = JsonFileUtils.ReadInFile("C:/fileTwoV4.json", "GroundCover");
dynamic morrowind = JsonFileUtils.ReadInFile("C:/morrowind.json", "Morrowind");
dynamic bcom = JsonFileUtils.ReadInFile("C:/bcom.json", "BCOM");

// Generate initial merged groundCover and parent cell list
var initialMergedGroundCover = MergedGroundCover.GenerateInitialMergedGroundCover(vardenfell, groundcover);
var parentReference = new ParentCellGenerator().CreateParentCellsList(morrowind, bcom);

// Process reference arrays
var processedFile = ProcessBcomReferenceArrays(groundcover, vardenfell, initialMergedGroundCover, parentReference);

// Write to file
var fileName = "processedFile.json";
Console.WriteLine($"Writing to file {fileName}");
JsonFileUtils.SimpleWrite(JsonConvert.SerializeObject(processedFile), fileName);

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
    

    var vardenfellReferences = MergedGroundCover.CreateFlatDynamicList(vardenfell.Where(vf => vf.type.ToString() == "Cell").Select(vf => vf.references));
    
    foreach (var bcomParentEntry in BCOMGroundCover)
    {
        Console.WriteLine($"{bcomCompletedCount} bcom parent entries completed out of {bcomEntryCount}");
        
        if (bcomParentEntry.type != "Cell")
        {
            bcomCompletedCount++;
            continue;
        }

        foreach (var bcomReference in bcomParentEntry.references)
        {
            if (bcomReference.mast_index == 0)
            {
                mergedCopy = MergedGroundCover.AddReferenceToMergedFileBasedOnGrid(mergedCopy, bcomParentEntry.data.grid, bcomReference);
                continue;
            }
                
            //match on master index from BCOM. MW = 1
            if (bcomReference.mast_index == 1)
            {
                var matchingVFRE = vardenfellReferences.Where(vfr => vfr.refr_index == bcomReference.refr_index);
                //check if the refr_index in BCOM Groundcover matches a cell reference subrecord in Vvardenfell Groundcover.
                if (!matchingVFRE.Any())
                {
                    throw new Exception(
                        $"ERROR: Unable to merge files: CELL reference refr_index [{bcomReference.refr_index}] is missing from vardenfell");
                }

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
                        mergedCopy = MergedGroundCover.RemoveReferenceFromMergedFile(mergedCopy, matchingFromVardenfell);
                        //Add BCOM refernec to parent reference array that matches moved_cell
                        mergedCopy = MergedGroundCover.AddReferenceToMergedFileBasedOnGrid(mergedCopy, bcomReference.moved_cell, bcomReference);
                        continue;
                    }

                    /*
                      if "moved cell" grid coordinates do not match another CELL parent record in Vvardenfell Groundcover, locate corresponding grid coordinates in 'Cell Name Parent' 
                      reference list and insert new CELL parent record. Then, omit Vvardenfell Groundcover cell reference subrecord matched in steps 1b & 2b from merged file, 
                      and move whole BCOM subrecord as-is to new CELL parent subrecord in merged file.
                     */
                    var matchFromParentCellList = parentCellList.First(pcl => pcl.data.grid.ToString() == bcomReference.moved_cell.ToString());
                    mergedCopy.Add(matchFromParentCellList);
                        
                    mergedCopy = MergedGroundCover.RemoveReferenceFromMergedFile(mergedCopy, matchingVFRE.First());
                    mergedCopy = MergedGroundCover.AddReferenceToMergedFileBasedOnGrid(mergedCopy, bcomReference.moved_cell, bcomReference);
                    
                    continue;
                }

                if (bcomReference.deleted != null)
                {
                    mergedCopy = MergedGroundCover.RemoveReferenceFromMergedFile(mergedCopy, bcomReference);
                }
            }
        }

        bcomCompletedCount++;
    }
    Console.WriteLine("Processing reference arrays complete.");
    return mergedCopy;
}

