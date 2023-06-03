// See https://aka.ms/new-console-template for more information

/*
 * Compare CELL parent records from Vvardenfell Groundcover with BCOM Groundcover (fileTwoV4) CELL parent records:
• if "grid" coordinates match (e.g. '18, 4'), overwrite the Vvardenfell CELL parent record in its entirety with data from the corresponding BCOM Groundcover CELL parent record (I.e. "id", "data", "flags", "region", "map colour"). 

Compare Vvardenfell Groundcover CELL reference subrecords with BCOM groundcover CELL reference subrecords:
1) check mast_index of BCOM Groundcover CELL reference subrecord.
1a) if "mast_index in BCOM groundcover = 0 (meaning it is a brand new cell reference added by BCOM, not a modification of an existing Morrowind.esm cell reference),
        insert cell reference subrecord under corresponding CELL parent record in merged file.
1b)  if "mast_index" number in BCOM Groundcover cell reference subrecord corresponds to Morrowind.esm, this is a match. Move on to next step

2) if 1b is a match, check if the refr_index in BCOM Groundcover matches a cell reference subrecord in Vvardenfell Groundcover.
2a) if no identical refr_index number can be found in Vvardenfell Groundcover, throw an error message with the following format: "ERROR: Unable to merge files: 
    CELL reference refr_index [xxx] in [file name (in this case, BCOM Groundcover)] from master file [master file name (in this case, Morrowind.esm)] Groundcover plugin 
    is missing from master file. Process aborted"
2b) if refr_index matches corresponding refr_index in Vvardenfell Groundcover, this is a match. Move on to next step

3) if 1b and 2b are matches, check if CELL reference subrecord in BCOM Groundcover has "moved cell" appended to it
3a) if "moved cell", check what grid coordinates are entered after "moved cell". If grid coordinates match another CELL parent record in Vvardenfell Groundcover, 
    omit Vvardenfell Groundcover cell reference subrecord matched in steps 1b & 2b from merged file, and insert whole BCOM subrecord as-is to new CELL parent 
    subrecord in merged file
3b) if "moved cell" grid coordinates do not match another CELL parent record in Vvardenfell Groundcover, locate corresponding grid coordinates in 'Cell Name Parent' 
    reference list and insert new CELL parent record. Then, omit Vvardenfell Groundcover cell reference subrecord matched in steps 1b & 2b from merged file, 
    and move whole BCOM subrecord as-is to new CELL parent subrecord in merged file.

4) if 1b and 2 are matches, and cell reference subrecord does not have "moved cell" appended, check if subrecord has "deleted" appended to it.
4a) if "deleted", omit whole cell reference subrecord from merged file
 */

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

