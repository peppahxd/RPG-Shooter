using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_Shooter.Models
{
    public enum IMPACT 
    {
        MISS,
        HIT,
        FATALHIT
    }
    public class Bullet
    {
        public int speed { get; set; }
        public int damage { get; set; }
        public Origin Origin { get; set; }
        public Bounds Bounds { get; set; }

        public string sDirection { get; set; }
        public int playerId { get; set; }

        public Bullet(Player player)
        {
            this.playerId = player.id;
            this.damage = 38;
            this.speed = 10;
            this.sDirection = player.sDirection;
            this.Bounds = new Bounds(60, 20);



            if (player.sDirection == "LEFT")
                this.Origin = new Origin(player.Origin.iX - 15, player.Origin.iY + (player.Bounds.iHeight / 5));
            else
                this.Origin = new Origin(player.Origin.iX + 15, player.Origin.iY + (player.Bounds.iHeight / 5));
        }

        public void Fire(Player player)
        {
            if (this.Origin.iX < player.Origin.iX)
                this.Origin = new Origin(this.Origin.iX - speed, this.Origin.iY);
            else
                this.Origin = new Origin(this.Origin.iX + speed, this.Origin.iY);
        }

        public IMPACT HandleImpact(List<Player> players)
        {
            Player hitPlayer = null;
            if (this.sDirection == "RIGHT")
            {
                players.ForEach(x =>
                {
                    if (this.Origin.iX + this.Bounds.iWidth > x.Origin.iX &&
                        this.Origin.iX + this.Bounds.iWidth < x.Origin.iX + x.Bounds.iWidth)
                        hitPlayer = x;
                    
                });
            }

            else
            {
                players.ForEach(x =>
                {
                    if (this.Origin.iX > x.Origin.iX &&
                        this.Origin.iX < x.Origin.iX + x.Bounds.iWidth)
                        hitPlayer = x;
                });
            }


            if (hitPlayer != null)
            {
                if (hitPlayer.iHealth > 0)
                {
                    hitPlayer.iHealth -= this.damage;
                    if (hitPlayer.iHealth <= 0) //fatal 
                        return IMPACT.FATALHIT;
                }
                return IMPACT.HIT;
            }

            return IMPACT.MISS;

        }
    }
}
