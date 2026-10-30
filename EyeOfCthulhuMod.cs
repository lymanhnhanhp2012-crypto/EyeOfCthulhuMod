using System;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Terraria.ID;

namespace EyeOfCthulhuMod
{
    [ApiVersion(2, 1)]
    public class EyeOfCthulhuMod : TerrariaPlugin
    {
        public override string Name => "EyeOfCthulhuMod";
        public override string Author => "Grok";
        public override string Description => "EoC Phase 2: 5 charges liên tục + xuyên giáp";
        public override Version Version => new Version(1, 0, 0);

        public EyeOfCthulhuMod(Main game) : base(game) { }

        public override void Initialize()
        {
            ServerApi.Hooks.NpcAIUpdate.Register(this, OnNpcAIUpdate);
            GetDataHandlers.PlayerDamage += OnPlayerDamage;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NpcAIUpdate.Deregister(this, OnNpcAIUpdate);
                GetDataHandlers.PlayerDamage -= OnPlayerDamage;
            }
            base.Dispose(disposing);
        }

        private void OnNpcAIUpdate(NpcAiUpdateEventArgs args)
        {
            NPC npc = Main.npc[args.NpcId];
            if (npc.type != NPCID.EyeofCthulhu || !npc.active) return;

            bool isPhase2 = npc.life < npc.lifeMax * 0.5f || npc.ai[0] >= 2f;
            if (!isPhase2) return;

            // Đếm số lần đã charge bằng localAI[0]
            if (npc.ai[1] == 0 && npc.localAI[0] < 5)
            {
                npc.localAI[0]++;
                if (npc.localAI[0] < 5)
                {
                    npc.ai[1] = 1f; // ép charge tiếp
                    npc.netUpdate = true;
                }
            }

            // Reset khi boss nghỉ
            if (npc.ai[0] >= 3f)
            {
                npc.localAI[0] = 0;
            }
        }

        private void OnPlayerDamage(object sender, GetDataHandlers.PlayerDamageEventArgs args)
        {
            if (args.Player == null) return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.type == NPCID.EyeofCthulhu &&
                    (npc.life < npc.lifeMax * 0.5f || npc.ai[0] >= 2f))
                {
                    // Xuyên giáp hoàn toàn
                    args.Damage += args.Player.TPlayer.statDefense;
                    break;
                }
            }
        }
    }
}
