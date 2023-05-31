namespace morrowNet.Contants;

public static class FloraAndKelpConstants
{
    public static List<string> FloraAndKelpNames => new()
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

    public static Dictionary<string, int> StaticMeshLowerCaseToId => new Dictionary<string, int>()
    {
        { @"grass\\flora_ash_grass_b_01.nif", 0 },
        { @"grass\\flora_ash_grass_r_01.nif", 1 },
        { @"grass\\flora_ash_grass_w_01.nif", 2 },
        { @"grass\\flora_bc_fern_02.nif", 3 },
        { @"grass\\flora_bc_fern_03.nif", 4 },
        { @"grass\\flora_bc_fern_04.nif", 5 },
        { @"grass\\flora_bc_grass_01.nif", 6 },
        { @"grass\\flora_bc_grass_02.nif", 7 },
        { @"grass\\flora_bc_lilypad_01.nif", 8 },
        { @"grass\\flora_bc_lilypad_02.nif", 9 },
        { @"grass\\flora_bc_lilypad_03.nif", 10 },
        { @"grass\\flora_grass_01.nif", 11 },
        { @"grass\\flora_grass_02.nif", 12 },
        { @"grass\\flora_grass_03.nif", 13 },
        { @"grass\\flora_grass_04.nif", 14 },
        { @"grass\\flora_grass_05.nif", 15 },
        { @"grass\\flora_grass_06.nif", 16 },
        { @"grass\\flora_grass_07.nif", 17 },
        { @"grass\\flora_kelp_01.nif", 18 },
        { @"grass\\flora_kelp_02.nif", 19 },
        { @"grass\\flora_kelp_03.nif", 20 },
        { @"grass\\flora_kelp_04.nif", 21 },
        { @"grass\\in_cave_plant00.nif", 22 },
        { @"grass\\in_cave_plant10.nif", 23 }
    };
    
    public static Dictionary<int, string> StaticMeshIdToCapitalisedMesh => new Dictionary<int, string>()
    {
        { 0, "grass\\flora_ash_grass_b_01.nif" },
        { 1, "grass\\flora_ash_grass_r_01.nif"},
        { 2, "grass\\flora_ash_grass_w_01.nif" },
        { 3, "grass\\flora_bc_fern_02.nif" },
        { 4, "grass\\flora_bc_fern_03.nif" },
        { 5, "grass\\flora_bc_fern_04.nif" },
        { 6, "grass\\flora_bc_grass_01.nif" },
        { 7, "grass\\flora_bc_grass_02.nif" },
        { 8, "grass\\flora_bc_lilypad_01.nif" },
        { 9, "grass\\flora_bc_lilypad_02.nif" },
        { 10, "grass\\flora_bc_lilypad_03.nif" },
        { 11, "grass\\flora_grass_01.nif" },
        { 12, "grass\\flora_grass_02.nif" },
        { 13, "grass\\flora_grass_03.nif" },
        { 14, "grass\\flora_grass_04.nif" },
        { 15, "grass\\Flora_grass_05.nif" },
        { 16, "grass\\Flora_grass_06.nif" },
        { 17, "grass\\Flora_grass_07.nif" },
        { 18, "grass\\flora_kelp_01.nif" },
        { 19, "grass\\flora_kelp_02.nif" },
        { 20, "grass\\flora_kelp_03.nif" },
        { 21, "grass\\flora_kelp_04.nif" },
        { 22, "grass\\in_cave_plant00.nif" },
        { 23, "grass\\in_cave_plant10.nif" }
    };
    
    public static Dictionary<int, string> StaticMeshIdToIdString => new Dictionary<int, string>()
    {
        { 0, "flora_ash_grass_b_01" },
        { 1, "Flora_Ash_Grass_R_01" },
        { 2, "flora_ash_grass_w_01" },
        { 3, "flora_bc_fern_02" },
        { 4, "flora_bc_fern_03" },
        { 5, "flora_bc_fern_03" },
        { 6, "flora_bc_grass_01" },
        { 7, "flora_bc_grass_02" },
        { 8, "Flora_BC_Lilypad" },
        { 9, "flora_bc_lilypad_02" },
        { 10, "flora_bc_lilypad_02" },
        { 11, "flora_grass_01" },
        { 12, "flora_grass_02" },
        { 13, "flora_grass_03" },
        { 14, "flora_grass_04" },
        { 15, "Flora_grass_05" },
        { 16, "Flora_grass_06" },
        { 17, "Flora_grass_07" },
        { 18, "Flora_kelp_01" },
        { 19, "Flora_kelp_02" },
        { 20, "Flora_kelp_03" },
        { 21, "Flora_kelp_04" },
        { 22, "in_cave_plant00" },
        { 23, "in_cave_plant00" }
    };
}