using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace P4G_Encounter_Randomizer
{
    class Program
    {
        private struct PatchEdit
        {
            public string comment { get; set; }
            public string tbl { get; set; }
            public int section { get; set; }
            public int offset { get; set; }
            public string data { get; set; }
        }

        private struct Patch
        {
            public int Version { get; set; }
            public PatchEdit[] Patches { get; set; }
        }

        private struct ConfigStruct
        {
            public bool BossShuffle { get; set; }
            public bool MiniBossShuffle { get; set; }
            public bool OptionalBossShuffle { get; set; }
            public int MinEnemies { get; set; }
            public int MaxEnemies { get; set; }
            public ushort[] DisabledRandomEncounterEnemies { get; set; }
        }

        private struct Encounter
        {
            public ushort[] Enemies { get; set; }
        }

        private static Dictionary<int, ushort[]> bossBattles = new Dictionary<int, ushort[]>
        {
            { (int)P4G_Battle.ShadowYosuke, new ushort[5] { (ushort)P4_EnemiesID.ShadowYosuke, 0, 0, 0, 0 } },
            { (int)P4G_Battle.ShadowChie, new ushort[5] { (ushort)P4_EnemiesID.ShadowChie, 0, 0, 0, 0 } },
            { (int)P4G_Battle.ShadowYukiko, new ushort[5] { (ushort)P4_EnemiesID.ShadowYukiko, 0, 0, 0, 0 } },
            { (int)P4G_Battle.ShadowKanji, new ushort[5] { (ushort)P4_EnemiesID.ShadowKanji, (ushort)P4_EnemiesID.NiceGuy, (ushort)P4_EnemiesID.ToughGuy, 0, 0 } },
            { (int)P4G_Battle.ShadowRise, new ushort[5] { (ushort)P4_EnemiesID.ShadowRise, 0, 0, 0, 0 } },
            { (int)P4G_Battle.ShadowTeddie, new ushort[5] { (ushort)P4_EnemiesID.ShadowTeddie, 0, 0, 0, 0 } },
            { (int)P4G_Battle.ShadowMitsuo, new ushort[5] { (ushort)P4_EnemiesID.ShadowMitsuo, (ushort)P4_EnemiesID.MitsuotheHero, 0, 0, 0 } },
            { (int)P4G_Battle.ShadowNaoto, new ushort[5] { (ushort)P4_EnemiesID.ShadowNaoto, 0, 0, 0, 0 } },

            { (int)P4G_Battle.Namatame, new ushort[5] { (ushort)P4_EnemiesID.KusuminoOkami, 0, 0, 0, 0 } },
            { (int)P4G_Battle.Adachi, new ushort[5] { (ushort)P4_EnemiesID.Adachi, 0, 0, 0, 0 } },
            { (int)P4G_Battle.Amenosagiri, new ushort[5] { (ushort)P4_EnemiesID.Amenosagiri, 0, 0, 0, 0 } },
            { (int)P4G_Battle.Izanami, new ushort[5] { (ushort)P4_EnemiesID.Izanami, 0, 0, 0, 0 } },
            { (int)P4G_Battle.IzanamiNoOkami, new ushort[5] { (ushort)P4_EnemiesID.IzanaminoOkami, 0, 0, 0, 0 } },
            { (int)P4G_Battle.Marie, new ushort[5] { (ushort)P4_EnemiesID.Marie, 0, 0, 0, 0 } },
            { (int)P4G_Battle.KusumiNoOkami, new ushort[5] { (ushort)P4_EnemiesID.KusuminoOkami, 0, 0, 0, 0 } },
            { (int)P4G_Battle.Margaret, new ushort[5] { (ushort)P4_EnemiesID.Margaret, 0, 0, 0, 0 } }
        };

        private static Dictionary<int, ushort[]> mbossBattles = new Dictionary<int, ushort[]>
        {
            { (int)P4G_Battle.MBossCastle, new ushort[5] { (ushort)P4_EnemiesID.AvengerKnight310, 0, 0, 0, 0} },
            { (int)P4G_Battle.MBossSauna, new ushort[5] { (ushort)P4_EnemiesID.DaringGigas311, 0, 0, 0, 0} },
            { (int)P4G_Battle.MBossMaru, new ushort[5] { (ushort)P4_EnemiesID.AmorousSnake312, 0, 0, 0, 0} },
            { (int)P4G_Battle.MBossVoid, new ushort[5] { (ushort)P4_EnemiesID.KillingHand313, 0, 0, 0, 0} },
            { (int)P4G_Battle.MBossLab, new ushort[5] { (ushort)P4_EnemiesID.DominatingMachine314, 0, 0, 0, 0} },
            { (int)P4G_Battle.MBossHeaven, new ushort[5] { (ushort)P4_EnemiesID.WorldBalance315, 0, 0, 0, 0} },
            { (int)P4G_Battle.MBossMagatsu1, new ushort[5] { (ushort)P4_EnemiesID.ChaosFuzz316, 0, 0, 0, 0} },
            { (int)P4G_Battle.MBossMagatsu2, new ushort[5] { (ushort)P4_EnemiesID.EnviousGiant, 0, 0, 0, 0} },
            { (int)P4G_Battle.MBossHollow1, new ushort[5] { (ushort)P4_EnemiesID.GorgeousKing, 0, 0, 0, 0} },
            { (int)P4G_Battle.MBossHollow2, new ushort[5] { (ushort)P4_EnemiesID.HeavensGiant, 0, 0, 0, 0} },
            { (int)P4G_Battle.MBossYomotsu1, new ushort[5] { (ushort)P4_EnemiesID.NeoMinotaur318, 0, 0, 0, 0} },
            { (int)P4G_Battle.MBossYomotsu2, new ushort[5] { (ushort)P4_EnemiesID.SleepingTable319, 0, 0, 0, 0} }
        };

        private static Dictionary<int, ushort[]> obossBattles = new Dictionary<int, ushort[]>
        {
            { (int)P4G_Battle.OBossCastle, new ushort[5] { (ushort)P4_EnemiesID.ContrarianKing, 0, 0, 0, 0} },
            { (int)P4G_Battle.OBossSauna, new ushort[5] { (ushort)P4_EnemiesID.IntolerantOfficer, 0, 0, 0, 0} },
            { (int)P4G_Battle.OBossMaru, new ushort[5] { (ushort)P4_EnemiesID.MomentaryChild, 0, 0, 0, 0} },
            { (int)P4G_Battle.OBossVoid, new ushort[5] { (ushort)P4_EnemiesID.EscapistSoldier, 0, 0, 0, 0} },
            { (int)P4G_Battle.OBossLab, new ushort[5] { (ushort)P4_EnemiesID.ExtremeVessel, 0, 0, 0, 0} },
            { (int)P4G_Battle.OBossHeaven, new ushort[5] { (ushort)P4_EnemiesID.LostOkina, 0, 0, 0, 0} }
        };

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("P4G Encounter Randomizer");
            Console.ResetColor();

            RandomizeEncounters();

            Console.WriteLine("Enter any key to exit...");
            Console.ReadLine();
        }

        private static void RandomizeEncounters()
        {
            // load config file
            string configFile = $@"{Directory.GetCurrentDirectory()}\config.json";
            ConfigStruct config = new ConfigStruct();
            if (!LoadConfig(ref config, configFile))
                return;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Config");
            Console.ResetColor();
            Console.WriteLine($"Boss Shuffle: {config.BossShuffle}");
            Console.WriteLine($"Optional Boss Shuffle: {config.OptionalBossShuffle}");
            Console.WriteLine($"Mini Boss Shuffle: {config.MiniBossShuffle}");
            Console.WriteLine($"Min Enemies in Encounter: {config.MinEnemies}");
            Console.WriteLine($"Max Enemies in Encounter: {config.MaxEnemies}");
            Console.WriteLine($"Enemies Disabled: {config.DisabledRandomEncounterEnemies.Length}");

            Encounter[] encounters = new Encounter[944];
            List<int> usedBattles = new List<int>();

            Random rand = new Random();

            for (int encounterIndex = 0, totalEncounters = encounters.Length; encounterIndex < totalEncounters; encounterIndex++)
            {
                // shuffle boss battles
                if (bossBattles.ContainsKey(encounterIndex))
                {
                    if (config.BossShuffle)
                    {
                        ushort[] battleEnemies = null;

                        while (battleEnemies == null)
                        {
                            int randomBattleIndex = rand.Next(bossBattles.Count);
                            KeyValuePair<int, ushort[]> randomBattle = bossBattles.ElementAt(randomBattleIndex);
                            if (!usedBattles.Contains(randomBattle.Key))
                            {
                                battleEnemies = randomBattle.Value;
                                usedBattles.Add(randomBattle.Key);
                            }
                        }

                        encounters[encounterIndex].Enemies = battleEnemies;
                    }
                    else
                    {
                        encounters[encounterIndex].Enemies = null;
                    }
                }
                // shuffle miniboss battles
                else if (mbossBattles.ContainsKey(encounterIndex))
                {
                    if (config.MiniBossShuffle)
                    {
                        ushort[] battleEnemies = null;

                        while (battleEnemies == null)
                        {
                            int randomBattleIndex = rand.Next(mbossBattles.Count);
                            KeyValuePair<int, ushort[]> randomBossBattle = mbossBattles.ElementAt(randomBattleIndex);
                            if (!usedBattles.Contains(randomBossBattle.Key))
                            {
                                battleEnemies = randomBossBattle.Value;
                                usedBattles.Add(randomBossBattle.Key);
                            }
                        }

                        encounters[encounterIndex].Enemies = battleEnemies;
                    }
                    else
                    {
                        encounters[encounterIndex].Enemies = null;
                    }
                }
                // shuffle optional battles
                else if (obossBattles.ContainsKey(encounterIndex))
                {
                    if (config.OptionalBossShuffle)
                    {
                        ushort[] battleEnemies = null;

                        while (battleEnemies == null)
                        {
                            int randomBattleIndex = rand.Next(obossBattles.Count);
                            KeyValuePair<int, ushort[]> randomBossBattle = obossBattles.ElementAt(randomBattleIndex);
                            if (!usedBattles.Contains(randomBossBattle.Key))
                            {
                                battleEnemies = randomBossBattle.Value;
                                usedBattles.Add(randomBossBattle.Key);
                            }
                        }

                        encounters[encounterIndex].Enemies = battleEnemies;
                    }
                    else
                    {
                        encounters[encounterIndex].Enemies = null;
                    }
                }
                // randomize regular encounters
                else
                {
                    int randNumEnemies = rand.Next(config.MinEnemies, config.MaxEnemies + 1);

                    encounters[encounterIndex].Enemies = new ushort[]
                    {
                        (randNumEnemies > 0) ? (ushort)GetValidRandomEnemy(config.DisabledRandomEncounterEnemies) : (ushort)P4_EnemiesID.h000,
                        (randNumEnemies > 1) ? (ushort)GetValidRandomEnemy(config.DisabledRandomEncounterEnemies) : (ushort)P4_EnemiesID.h000,
                        (randNumEnemies > 2) ? (ushort)GetValidRandomEnemy(config.DisabledRandomEncounterEnemies) : (ushort)P4_EnemiesID.h000,
                        (randNumEnemies > 3) ? (ushort)GetValidRandomEnemy(config.DisabledRandomEncounterEnemies) : (ushort)P4_EnemiesID.h000,
                        (randNumEnemies > 4) ? (ushort)GetValidRandomEnemy(config.DisabledRandomEncounterEnemies) : (ushort)P4_EnemiesID.h000,
                    };
                }
            }

            try
            {
                string packageFolder = $@"{Directory.GetCurrentDirectory()}\Randomized Encounters";
                if (!Directory.Exists(packageFolder))
                    Directory.CreateDirectory(packageFolder);

                string tblpatchesFolder = $@"{packageFolder}\tblpatches";
                if (!Directory.Exists(tblpatchesFolder))
                    Directory.CreateDirectory(tblpatchesFolder);

                string encounterPatchFile = $@"{tblpatchesFolder}\Randomized Encounters.tbp";

                OutputTBLPatch(encounterPatchFile, encounters);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Package succesfully created: {packageFolder}");
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Problem building output package!");
            }
        }

        private static void OutputTBLPatch(string outputPath, Encounter[] encounters)
        {
            List<PatchEdit> encounterPatches = new List<PatchEdit>();

            int startOffset = 8;
            int entrySize = 24;

            for (int i = 0, total = encounters.Length; i < total; i++)
            {
                if (encounters[i].Enemies != null)
                {
                    int encounterOffset = (entrySize * i) + startOffset;
                    encounterPatches.Add(new PatchEdit()
                    {
                        comment = $@"Encounter Index: {i}",
                        tbl = "ENCOUNT",
                        section = 0,
                        offset = encounterOffset,
                        data = PatchDataFormatter.ByteArrayToHexText(encounters[i].Enemies.SelectMany(BitConverter.GetBytes).ToArray())
                    });
                }
            }

            Patch thePatch = new Patch()
            {
                Version = 1,
                Patches = encounterPatches.ToArray()
            };

            File.WriteAllText(outputPath, JsonSerializer.Serialize(thePatch, new JsonSerializerOptions() { WriteIndented = true }));
        }

        private static P4_EnemiesID GetValidRandomEnemy(ushort[] disabledEnemies)
        {
            P4_EnemiesID temp = P4_EnemiesID.h000;

            Random rand = new Random();
            Array values = Enum.GetValues(typeof(P4_EnemiesID));

            while (temp == P4_EnemiesID.h000)
            {
                P4_EnemiesID randomEnemy = (P4_EnemiesID)values.GetValue(rand.Next(values.Length));

                if (!randomEnemy.ToString().ToLower().Contains("blank") && randomEnemy != P4_EnemiesID.h000 && !disabledEnemies.Contains((ushort)randomEnemy))
                {
                    temp = randomEnemy;
                }
            }

            return temp;
        }

        private static bool LoadConfig(ref ConfigStruct config, string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Missing config file! Creating new one...");
                try
                {
                    List<ushort> disabledEnemies = new List<ushort>();

                    // add enemies used in special battles to disabled list
                    foreach (var battle in bossBattles)
                    {
                        foreach (var enemy in battle.Value)
                        {
                            if (enemy != (ushort)P4_EnemiesID.h000)
                                disabledEnemies.Add(enemy);
                        }
                    }

                    foreach (var battle in obossBattles)
                    {
                        foreach (var enemy in battle.Value)
                        {
                            if (enemy != (ushort)P4_EnemiesID.h000)
                                disabledEnemies.Add(enemy);
                        }
                    }

                    foreach (var battle in mbossBattles)
                    {
                        foreach (var enemy in battle.Value)
                        {
                            if (enemy != (ushort)P4_EnemiesID.h000)
                                disabledEnemies.Add(enemy);
                        }
                    }

                    disabledEnemies.AddRange(new ushort[]
                    {
                        (ushort)P4_EnemiesID.TheReaper288,
                        (ushort)P4_EnemiesID.TheReaper289,
                        (ushort)P4_EnemiesID.TheReaper290,
                        (ushort)P4_EnemiesID.TheReaper291,
                        (ushort)P4_EnemiesID.TheReaper292,
                        (ushort)P4_EnemiesID.TheReaper293,
                        (ushort)P4_EnemiesID.TheReaper294,
                        (ushort)P4_EnemiesID.TheReaper295,
                        (ushort)P4_EnemiesID.TheReaper296,
                        (ushort)P4_EnemiesID.TheReaper297
                    });

                    config = new ConfigStruct()
                    {
                        MinEnemies = 1,
                        MaxEnemies = 5,
                        DisabledRandomEncounterEnemies = disabledEnemies.ToArray()
                    };

                    File.WriteAllText(filePath, JsonSerializer.Serialize(config, new JsonSerializerOptions() { WriteIndented = true }));
                    Console.WriteLine($"New config created! File: {filePath}");
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine($"Problem creating new config! File: {filePath}");
                }
            }
            else
            {
                try
                {
                    config = JsonSerializer.Deserialize<ConfigStruct>(File.ReadAllText(filePath));

                    if (config.MinEnemies < 1 || config.MaxEnemies > 5 || config.MinEnemies > config.MaxEnemies)
                    {
                        Console.WriteLine("Invalid Min and Max Enemies values!");
                        return false;
                    }
                    else
                    {
                        Console.WriteLine("Config loaded successfully!");
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Problem parsing config file!");
                }
            }

            return false;
        }
    }
}
