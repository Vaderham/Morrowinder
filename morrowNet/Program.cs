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


using System.Collections;
using System.Diagnostics;
using morrowNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using var r = new StreamReader("C:/bcom.json");
string json = r.ReadToEnd();

var refs = new List<string>()
{
    "flora_ash_grass_b_01",
    "bcom_MM_flora_ash_grass_b_01",
    "Flora_Ash_Grass_R_01",
    "bcom_MM_Flora_Ash_Grass_R_01",
    "flora_ash_grass_w_01",
    "bcom_MM_flora_ash_grass_w_01",
    "flora_bc_fern_02",
    "flora_bc_fern_03",
    "flora_bc_fern_04",
    "flora_bc_grass_01",
    "flora_bc_grass_02",
    "flora_bc_lilypad_01",
    "flora_bc_lilypad_02",
    "flora_bc_lilypad_03",
    "flora_grass_01",
    "bcom_MM_flora_grass_01",
    "flora_grass_02",
    "flora_grass_03",
    "flora_grass_04",
    "flora_grass_05",
    "Flora_grass_06",
    "Flora_grass_07",
    "Flora_kelp_01",
    "Flora_kelp_02",
    "Flora_kelp_03",
    "Flora_kelp_04",
    "in_cave_plant00",
    "in_cave_plant10"
};

dynamic items = JsonConvert.DeserializeObject(json);

var newList1 = FileOne(items, refs);

JsonFileUtils.SimpleWrite(JsonConvert.SerializeObject(newList1), "fileOne.json");

var newList2 = FileTwo(items, refs);

JsonFileUtils.SimpleWrite(JsonConvert.SerializeObject(newList2), "FileTwo.json");

static List<dynamic> FileOne(dynamic parsedJson, List<string> refs)
{
    var newList = new List<dynamic>();
    
    foreach (var espType in parsedJson)
    {
        // If it's not a cell type, keep it.
        if (espType.type.ToString() != string.Empty && espType.type.ToString() != "Cell")
        {
            newList.Add(espType);
            continue;
        }

        // If it's interior, keep it
        if (espType.data != null && espType.data.flags != null)
        {
            BitArray flagByte = new BitArray(new int[] { espType.data.flags });

            //true = interior
            //false = exterior
            if (flagByte[0])
            {
                newList.Add(espType);
                continue;
            }
        }

        // Otherwise, go through the cells references, and if any id's are in our refs list, remove them.
        var references = new List<dynamic>();

        foreach (var reference in espType.references)
        {
            if (!refs.Contains(reference.id.ToString()))
            {
                references.Add(reference);
            }
        }

        JArray newRefsArray = JArray.FromObject(references);
        espType.references = newRefsArray;
        
        // Only add the cell back to the file if it has references remaining.
        if (newRefsArray.Count > 0)
        {
            newList.Add(espType);   
        }
    }

    return newList;
}

static List<dynamic> FileTwo(dynamic parsedJson, List<string> refs)
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
        {
            if (refs.Contains(espType.id.ToString()))
            {
                string[] splitPath = espType.mesh.ToString().Split("\\");
                espType.mesh = "grass\\" + splitPath[1];
                
                Console.WriteLine(espType.mesh);

                newList.Add(espType);
                continue;
            }
        }
    
        // If it's a cell ref, and references array contains Id from refs, keep those references and cell.
        if (espType.type != null && espType.type.ToString() == "Cell")
        {
            var references = new List<dynamic>();
            
            foreach (var reference in espType.references)
            {
                if (refs.Contains(reference.id.ToString()))
                {
                    references.Add(reference);
                }
            }
            
            JArray newRefsArray = JArray.FromObject(references);
            espType.references = newRefsArray;

            // Only add the cell back to the file if it has references remaining.
            if (newRefsArray.Count > 0)
            {
                newList.Add(espType);   
            }
        }
    }
    
    // Scan through our refs list and if we don't have a static object for it, add one in
    var staticItemIds = newList.Where(espType => espType.type.ToString() != string.Empty && espType.type == "Static")
        .Select(item => item.id)
        .ToList();
    
    foreach (var refString in refs)
    {
        if (!staticItemIds.Contains(refString))
        {
            newList.Add(new 
            {
                type = "Static",
                flags = new [] { 0, 0 },
                id = refString,
                mesh = $"grass\\{refString}.NIF"
            });
        }
    }

    return newList;
}