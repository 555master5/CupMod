using CupMod.Blocks;
using CupMod.Entities;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace CupMod
{
    public class CupModModSystem : ModSystem
    {
        public static CupModConfigData config;

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            if (api.Side == EnumAppSide.Server)
            {
                TryToLoadServerConfig(api);
                foreach (var breakChance in config.breakChances)
                {
                    api.World.Config.SetFloat(breakChance.Key + "BreakChance", breakChance.Value);
                    //Console.WriteLine("[Cupmod] Break chance is being set as " + breakChance.Value.ToString() + " for cup " + breakChance.Key);
                }
                api.World.Config.SetBool("IsThrowingEnabled", config.throwingEnabled);
            }
            api.RegisterBlockClass(Mod.Info.ModID + ".cup", typeof(BlockCup));
            api.RegisterEntity(Mod.Info.ModID + ".throwncup", typeof(EntityThrownCup));
        }

        private void TryToLoadServerConfig(ICoreAPI api)
        {
            //It is important to surround the LoadModConfig function in a try-catch. 
            //If loading the file goes wrong, then the 'catch' block is run.
            try
            {
                config = api.LoadModConfig<CupModConfigData>("CupModConfig.json");
                if (config == null) //if the 'CupModServerConfigData.json' file isn't found...
                {
                    config = new CupModConfigData();
                }
                //Save a copy of the mod config.
                api.StoreModConfig<CupModConfigData>(config, "CupModConfig.json");
            }
            catch (Exception e)
            {
                //Couldn't load the mod config... Create a new one with default settings, but don't save it.
                Mod.Logger.Error("Could not load config for Daymare's Cup Mod! Loading default settings instead.");
                Mod.Logger.Error(e);
                config = new CupModConfigData();
            }
        }
    }
}