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
using morrowNet;
using Newtonsoft.Json;

using var r = new StreamReader("C:/bcom.json");
string json = r.ReadToEnd();

var refs = new List<string>()
{
    "flora_ash_grass_b_01",
    "bcom_MM_flora_ash_grass_b_01",
    "flora_ash_grass_r_01",
    "bcom_MM_flora_ash_grass_r_01",
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
    "flora_grass_06",
    "flora_grass_07",
    "flora_kelp_01",
    "flora_kelp_02",
    "flora_kelp_03",
    "flora_kelp_04",
    "in_cave_plant00",
    "in_cave_plant10"
};

dynamic items = JsonConvert.DeserializeObject(json);

Console.Write("BCOM contains " + items.length + " items");



// JsonFileUtils.SimpleWrite(JsonConvert.SerializeObject(newList), "newFile.json");

static List<dynamic> fileOne(dynamic parsedJSON, List<string> refs)
{
    var newList = new List<dynamic>();
    
    foreach (var espType in parsedJSON)
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
    
        // If it's ID is in the refs list, don't keep it.
        if (espType.references.)
        {
            newList.Add(espType);
        }
    }

    return newList;
}

static List<dynamic> fileTwo(dynamic parsedJSON, List<string> refs)
{
    var newList = new List<dynamic>();

    foreach (var obj in parsedJSON)
    {
        // If it's the header, keep it
        if (obj.tpye != null && obj.type.ToString() == "Header")
        {
            newList.Add(obj);
            continue;
        }
    
        // If it's static and contains ID's keep it
        if (obj.tpye != null && (obj.type.ToString() == "Static" && refs.Contains(obj.id)))
        {
            newList.Add(obj);
            continue;
        }
    
        // If it's a cell ref, and contains Id from refs, keep it
        if (obj.tpye != null && (obj.type.ToString() == "Cell" && refs.Contains(obj.id)))
        {
            newList.Add(obj);
        }
    }

    return newList;
}