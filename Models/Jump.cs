using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_Shooter.Models
{

    public enum JUMP
    {
        NULL,
        STARTJUMPING,
        JUMPING,
        LANDING,
        DUBBLEJUMPING
    }
    public class Jump
    {
        public JUMP JUMP { get; set; }
        public Player Player { get; set; }
        public int iJumpAmount { get; set; }
        public bool canDubbleJump { get; set; }
        public bool hasReset { get; set; }
        public Velocity JumpVelocity { get; set; }
        public Velocity MaxJumpVelocity { get; set; }

        public Jump(Player player)
        {
            this.JUMP = JUMP.NULL;
            this.Player = player;
            this.iJumpAmount = 0;
            this.canDubbleJump = false;
            this.JumpVelocity = new Velocity(0, 0);
            this.MaxJumpVelocity = new Velocity(0, 0);
            this.hasReset = true;
        }

        public void StartJump()
        {
            this.Player.isStanding = false;
            this.iJumpAmount++;

            

            if (this.iJumpAmount == 1)
            {
                this.JUMP = JUMP.STARTJUMPING;
                this.JUMP = JUMP.JUMPING;
            }

            if (this.canDubbleJump && this.iJumpAmount == 2)
                this.JUMP = JUMP.DUBBLEJUMPING;

        }

        public void Reset()
        {
            if (this.JUMP != JUMP.JUMPING)
            {
                this.JUMP = JUMP.NULL;
                this.canDubbleJump = false;
                this.iJumpAmount = 0;
                this.hasReset = true;
            }
        }

        public void Handle()
        {
            if (this.JUMP == JUMP.NULL)
            {
                this.MaxJumpVelocity = new Velocity(this.Player.Velocity.xSpeed, this.Player.Velocity.ySpeed - 20);
                return;
            }
                

            if (this.JUMP == JUMP.JUMPING || this.JUMP == JUMP.DUBBLEJUMPING)
            {
                Player.isStanding = false;

                if (JumpVelocity.ySpeed < MaxJumpVelocity.ySpeed)
                {
                    this.JUMP = JUMP.LANDING;
                }

                else
                    JumpVelocity = new Velocity(this.Player.Velocity.xSpeed, this.JumpVelocity.ySpeed - 5);
            }

            if (this.JUMP == JUMP.LANDING)
            {
                if (this.iJumpAmount == 1)
                    this.canDubbleJump = true;
            }
        }
    }
}
