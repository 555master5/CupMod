using CupMod.Entities;
using CupMod.Blocks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace CupMod
{
    public class CupModModSystem : ModSystem
    {

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            Mod.Logger.Notification("Hello from Cup Mod: " + api.Side);
            api.RegisterBlockClass(Mod.Info.ModID + ".cup", typeof(BlockCup));
            api.RegisterEntity(Mod.Info.ModID + ".throwncup", typeof(EntityThrownCup));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            Mod.Logger.Notification("Hello from Cup Mod server side: " + Lang.Get("cupmod:hello"));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            Mod.Logger.Notification("Hello from Cup Mod client side: " + Lang.Get("cupmod:hello"));
        }

    }
}