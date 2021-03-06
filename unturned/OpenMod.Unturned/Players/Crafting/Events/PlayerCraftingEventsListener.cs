﻿using OpenMod.API;
using OpenMod.API.Eventing;
using OpenMod.API.Users;
using OpenMod.Unturned.Events;
using SDG.Unturned;

namespace OpenMod.Unturned.Players.Crafting.Events
{
    internal class CraftingEventsListener : UnturnedEventsListener
    {
        public CraftingEventsListener(IOpenModHost openModHost,
            IEventBus eventBus,
            IUserManager userManager) : base(openModHost, eventBus, userManager)
        {

        }

        public override void Subscribe()
        {
            PlayerCrafting.onCraftBlueprintRequested += OnCraftBlueprintRequested;
        }

        public override void Unsubscribe()
        {
            PlayerCrafting.onCraftBlueprintRequested -= OnCraftBlueprintRequested;
        }

        private void OnCraftBlueprintRequested(PlayerCrafting crafting, ref ushort itemId, ref byte blueprintIndex, ref bool shouldAllow)
        {
            UnturnedPlayer player = GetUnturnedPlayer(crafting.player);

            UnturnedPlayerCraftingEvent @event = new UnturnedPlayerCraftingEvent(player, itemId, blueprintIndex);

            Emit(@event);

            itemId = @event.ItemId;
            blueprintIndex = @event.BlueprintIndex;
            shouldAllow = !@event.IsCancelled;
        }
    }
}
