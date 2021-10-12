using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_Shooter.Models
{
    public class RPG
    {
        public Origin Origin { get; set; }
        public Bounds Bounds { get; set; }
        public Player Player { get; set; }
        public RPG(Player player)
        {
            this.Player = player;

            this.Origin = player.Origin;
            this.Bounds = new Bounds(20, 20);
        }

    }
}
