using HGV.Basilius;
using HGV.Nullifier.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGV.Nullifier.Tools.Export
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new MetaClient();
            var heroes = client.GetHeroes();

            var draft_pool = heroes
                .Where(h => h.Enabled == true)
                .Select(h => new
                {
                    id = h.Id,
                    enabled = h.AbilityDraftEnabled,
                    name = h.Name,
                    img = h.ImageBanner,
                    abilities = h.Abilities.Where(_ => _.Id != Ability.GENERIC).Select(a => new {
                        id = a.Id,
                        hero_id = a.HeroId,
                        name = a.Name,
                        img = a.Image,
                        is_ultimate = a.IsUltimate,
                        has_upgrade = a.HasScepterUpgrade,
                        enabled = a.AbilityDraftEnabled,
                    }).ToList(),
                })
                .OrderBy(_ => _.name)
                .ToList();

            var json_draft_pool = Newtonsoft.Json.JsonConvert.SerializeObject(draft_pool, Newtonsoft.Json.Formatting.Indented);
            System.IO.File.WriteAllText("DraftPool.json", json_draft_pool);

            // var context = new DataContext();
        }
    }
}
