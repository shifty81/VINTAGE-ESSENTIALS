using System;
using System.Collections.Generic;
using System.Text.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.MathTools;

namespace VintageEssentials
{
    public class VintageEssentialsModSystem : ModSystem
    {
        private ICoreServerAPI serverApi;
        private Dictionary<string, Vec3d> playerHomes = new Dictionary<string, Vec3d>();
        private const string HOMES_DATA_KEY = "vintageessentials:homes";
        private LockedSlotsManager lockedSlotsManager;
        private ModConfig config;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            
            // Register block and block entity classes
            api.RegisterBlockClass("VintageEssentials.BlockPortableCraftingTable", typeof(BlockPortableCraftingTable));
            api.RegisterBlockEntityClass("VintageEssentials.BlockEntityPortableCraftingTable", typeof(BlockEntityPortableCraftingTable));
            
            // Patch all collectibles to have increased stack sizes
            api.World.Logger.Event("VintageEssentials: Applying stack size patches...");
        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            // Constants for stack size limits
            const int MIN_STACK_THRESHOLD = 16;    // Items that stack under 16 are excluded
            const int VANILLA_CAP = 1000;          // Max stack for items already high
            const int MAX_ALLOWED_STACK = 10000;   // Absolute maximum stack size
            
            // Load configuration
            config = ModConfig.Load(api);
            
            int patchedCount = 0;
            
            // Patch stack sizes for all collectibles (items and blocks)
            foreach (var collectible in api.World.Collectibles)
            {
                if (collectible != null)
                {
                    // Get the original/current max stack size
                    int originalMaxStack = collectible.MaxStackSize;
                    
                    // Skip items that stack under 16 (tools, weapons, armor, etc.)
                    if (originalMaxStack < MIN_STACK_THRESHOLD)
                    {
                        continue;
                    }
                    
                    // Apply multiplier to items that stack 16 or more, capped at MAX_ALLOWED_STACK
                    if (originalMaxStack < VANILLA_CAP)
                    {
                        int newMaxStack = Math.Min(originalMaxStack * config.StackSizeMultiplier, MAX_ALLOWED_STACK);
                        collectible.MaxStackSize = newMaxStack;
                        patchedCount++;
                    }
                }
            }
            
            api.World.Logger.Event($"VintageEssentials: Patched stack sizes for {patchedCount} collectibles (multiplier: {config.StackSizeMultiplier}x)");
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            this.serverApi = api;

            // Load configuration
            config = ModConfig.Load(api);

            // Create locked slots manager
            lockedSlotsManager = new LockedSlotsManager(api, config);

            // Load saved data
            LoadHomes();
            lockedSlotsManager.Load(serverApi);

            // Register chat commands
            api.ChatCommands.Create("sethome")
                .WithDescription("Sets your home location")
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(OnSetHome);

            api.ChatCommands.Create("home")
                .WithDescription("Warps you to your home location")
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(OnHome);

            api.ChatCommands.Create("rtp")
                .WithDescription("Randomly teleports you in a direction")
                .WithArgs(api.ChatCommands.Parsers.Word("direction"))
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(OnRtp);

            // Save data periodically and on shutdown
            api.Event.SaveGameLoaded += OnSaveGameLoaded;
            api.Event.GameWorldSave += OnGameWorldSave;
        }

        private void OnSaveGameLoaded()
        {
            LoadHomes();
            lockedSlotsManager.Load(serverApi);
        }

        private void OnGameWorldSave()
        {
            SaveHomes();
            lockedSlotsManager.Save(serverApi);
        }

        private void LoadHomes()
        {
            byte[] data = serverApi.WorldManager.SaveGame.GetData(HOMES_DATA_KEY);
            if (data != null)
            {
                try
                {
                    string json = System.Text.Encoding.UTF8.GetString(data);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, Vec3d>>(json);
                    if (dict != null)
                    {
                        playerHomes = dict;
                    }
                }
                catch (Exception e)
                {
                    serverApi.Logger.Error("VintageEssentials: Failed to load homes data: " + e.Message);
                }
            }
        }

        private void SaveHomes()
        {
            try
            {
                string json = JsonSerializer.Serialize(playerHomes);
                byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
                serverApi.WorldManager.SaveGame.StoreData(HOMES_DATA_KEY, data);
            }
            catch (Exception e)
            {
                serverApi.Logger.Error("VintageEssentials: Failed to save homes data: " + e.Message);
            }
        }

        private TextCommandResult OnSetHome(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (player == null)
            {
                return TextCommandResult.Error("Only players can use this command.");
            }

            Vec3d pos = player.Entity.Pos.XYZ;
            playerHomes[player.PlayerUID] = pos;
            SaveHomes();

            return TextCommandResult.Success($"Home set at {pos.X:F0}, {pos.Y:F0}, {pos.Z:F0}");
        }

        private TextCommandResult OnHome(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (player == null)
            {
                return TextCommandResult.Error("Only players can use this command.");
            }

            if (playerHomes.TryGetValue(player.PlayerUID, out Vec3d homePos))
            {
                player.Entity.TeleportTo(homePos);
                return TextCommandResult.Success("Welcome home!");
            }
            else
            {
                return TextCommandResult.Error("You haven't set a home yet! Use /sethome first.");
            }
        }

        private TextCommandResult OnRtp(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (player == null)
            {
                return TextCommandResult.Error("Only players can use this command.");
            }

            string direction = args[0] as string;
            if (string.IsNullOrEmpty(direction))
            {
                return TextCommandResult.Error("Usage: /rtp <north|south|east|west>");
            }

            direction = direction.ToLower();

            // Accept short direction aliases
            switch (direction)
            {
                case "n": direction = "north"; break;
                case "s": direction = "south"; break;
                case "e": direction = "east"; break;
                case "w": direction = "west"; break;
            }

            if (direction != "north" && direction != "south" && direction != "east" && direction != "west")
            {
                return TextCommandResult.Error("Invalid direction. Use: north, south, east, or west");
            }

            player.SendMessage(GlobalConstants.GeneralChatGroup, "Searching for a safe location...", EnumChatType.Notification);

            // Start async search with retry logic
            TryRtpAttempt(player, direction, 0);

            return TextCommandResult.Success();
        }

        private void TryRtpAttempt(IServerPlayer player, string direction, int attempt)
        {
            int maxAttempts = 10;
            if (attempt >= maxAttempts)
            {
                player.SendMessage(GlobalConstants.GeneralChatGroup, "Failed to find a safe location after multiple attempts. Please try again.", EnumChatType.CommandError);
                return;
            }

            // Notify player of progress on retries so they know it's still working
            if (attempt > 0)
            {
                player.SendMessage(GlobalConstants.GeneralChatGroup,
                    $"Attempt {attempt + 1}/{maxAttempts} — searching a new area...",
                    EnumChatType.Notification);
            }

            // Random distance between 3,000 and 10,000 blocks — reduced from 10k-20k
            // for more reliable chunk generation at unvisited locations
            double minDistance = 3000;
            double maxDistance = 10000;
            Random rand = new Random();
            double distance = rand.NextDouble() * (maxDistance - minDistance) + minDistance;

            // Random perpendicular offset (±500 blocks) so retries explore different terrain
            double lateralOffset = (rand.NextDouble() - 0.5) * 1000;

            Vec3d currentPos = player.Entity.Pos.XYZ;
            double newX = currentPos.X;
            double newZ = currentPos.Z;

            // In Vintage Story, negative Z is North, positive Z is South
            switch (direction)
            {
                case "north": newZ -= distance; break;
                case "south": newZ += distance; break;
                case "east":  newX += distance; break;
                case "west":  newX -= distance; break;
            }

            // Apply perpendicular offset: X-axis for north/south, Z-axis for east/west
            if (direction == "north" || direction == "south")
                newX += lateralOffset;
            else
                newZ += lateralOffset;

            // Force-load a 5x5 grid of chunk columns around the target so terrain
            // generates properly (world gen depends on neighbor chunks).
            int chunkSize = GlobalConstants.ChunkSize;
            int centerChunkX = (int)Math.Floor(newX / chunkSize);
            int centerChunkZ = (int)Math.Floor(newZ / chunkSize);

            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dz = -2; dz <= 2; dz++)
                {
                    serverApi.WorldManager.LoadChunkColumnPriority(centerChunkX + dx, centerChunkZ + dz);
                }
            }

            // Capture values for the closure
            double capturedX = newX;
            double capturedZ = newZ;
            double capturedDistance = distance;
            int capturedAttempt = attempt;

            // Progressive delay: 5s base + 1s per attempt — later attempts give
            // the server more time to generate terrain at far locations.
            int delayMs = 5000 + (attempt * 1000);

            serverApi.Event.RegisterCallback((deltaTime) =>
            {
                try
                {
                    // Verify the player is still connected
                    if (player?.Entity == null || player.ConnectionState != EnumClientState.Playing)
                    {
                        return;
                    }

                    // Search a grid of nearby columns (up to 25 positions) for any safe landing spot.
                    // This greatly increases the chance of finding safe ground even when some columns
                    // have difficult terrain (trees, cliffs, caves).
                    int foundY = -1;
                    double teleX = capturedX;
                    double teleZ = capturedZ;

                    int[] searchOffsets = { 0, -4, 4, -8, 8 };
                    foreach (int dx in searchOffsets)
                    {
                        if (foundY > 0) break;
                        foreach (int dz in searchOffsets)
                        {
                            BlockPos pos = new BlockPos((int)capturedX + dx, 0, (int)capturedZ + dz);
                            foundY = FindSurfaceY(pos);
                            if (foundY > 0)
                            {
                                teleX = capturedX + dx;
                                teleZ = capturedZ + dz;
                                break;
                            }
                        }
                    }

                    if (foundY > 0)
                    {
                        Vec3d teleportPos = new Vec3d(teleX, foundY + 1, teleZ);
                        player.Entity.TeleportTo(teleportPos);
                        player.SendMessage(GlobalConstants.GeneralChatGroup,
                            $"Randomly warped {direction} to {teleX:F0}, {foundY}, {teleZ:F0} ({capturedDistance:F0} blocks away)",
                            EnumChatType.CommandSuccess);
                    }
                    else
                    {
                        serverApi.Logger.Debug($"VintageEssentials: RTP attempt {capturedAttempt + 1}/{maxAttempts} at {capturedX:F0},{capturedZ:F0} — no safe surface found, retrying...");
                        TryRtpAttempt(player, direction, capturedAttempt + 1);
                    }
                }
                catch (Exception ex)
                {
                    // Log the error and ensure retries continue despite exceptions.
                    // Without this catch, a single exception silently aborts all remaining retries.
                    serverApi.Logger.Error($"VintageEssentials: RTP attempt {capturedAttempt + 1} failed with exception: {ex.Message}");
                    if (capturedAttempt + 1 < maxAttempts)
                    {
                        TryRtpAttempt(player, direction, capturedAttempt + 1);
                    }
                    else
                    {
                        player.SendMessage(GlobalConstants.GeneralChatGroup,
                            "An error occurred during teleport. Please try again.",
                            EnumChatType.CommandError);
                    }
                }
            }, delayMs);
        }

        /// <summary>
        /// Checks whether a block is passable (a player can occupy the space).
        /// Returns true for air, plants, snow layers, leaves, and other non-solid blocks.
        /// </summary>
        private bool IsBlockPassable(Block block)
        {
            if (block == null || block.Id == 0) return true;

            var mat = block.BlockMaterial;
            return mat == EnumBlockMaterial.Air
                || mat == EnumBlockMaterial.Plant
                || mat == EnumBlockMaterial.Leaves
                || mat == EnumBlockMaterial.Snow;
        }

        private int FindSurfaceY(BlockPos pos)
        {
            IBlockAccessor blockAccessor = serverApi.World.BlockAccessor;
            int mapHeight = serverApi.World.BlockAccessor.MapSizeY;
            bool foundAnySolidBlock = false;

            // Start from the top and search downward for the first solid block
            for (int y = mapHeight - 1; y > 0; y--)
            {
                BlockPos checkPos = new BlockPos(pos.X, y, pos.Z);
                Block block = blockAccessor.GetBlock(checkPos);

                if (block == null || block.Id == 0) continue;

                var mat = block.BlockMaterial;

                // Skip air, liquid, and lava entirely
                if (mat == EnumBlockMaterial.Air
                    || mat == EnumBlockMaterial.Liquid
                    || mat == EnumBlockMaterial.Lava)
                {
                    continue;
                }

                foundAnySolidBlock = true;

                // Skip passable surface blocks (plants, snow, leaves) — they're not solid ground
                if (IsBlockPassable(block)) continue;

                // Found a solid non-liquid block, check if there's space above for the player (2 blocks)
                BlockPos abovePos1 = new BlockPos(pos.X, y + 1, pos.Z);
                BlockPos abovePos2 = new BlockPos(pos.X, y + 2, pos.Z);
                Block blockAbove1 = blockAccessor.GetBlock(abovePos1);
                Block blockAbove2 = blockAccessor.GetBlock(abovePos2);

                if (IsBlockPassable(blockAbove1) && IsBlockPassable(blockAbove2))
                {
                    return y;
                }
            }

            // If we scanned the entire column and found no solid blocks at all, the chunk
            // is likely not loaded yet (GetBlock returns air for unloaded chunks).
            if (!foundAnySolidBlock)
            {
                serverApi.Logger.Debug($"VintageEssentials: FindSurfaceY at {pos.X},{pos.Z} found no solid blocks — chunk may not be loaded");
            }

            return -1; // No safe location found
        }

        public override void Dispose()
        {
            SaveHomes();
            lockedSlotsManager?.Save(serverApi);
            base.Dispose();
        }
    }
}
