using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RPG_Shooter.Models
{

    public struct Origin
    {
        public Origin(int iX, int iY)
        {
            this.iX = iX;
            this.iY = iY;
        }

        public int iX { get; set; }
        public int iY { get; set; }
    }

    public struct Bounds
    {
        public Bounds(int iWidth, int iHeight)
        {
            this.iWidth = iWidth;
            this.iHeight = iHeight;
        }

        public int iWidth { get; set; }
        public int iHeight { get; set; }
    }

    public struct Velocity
    {
        public Velocity(int xSpeed, int ySpeed)
        {
            this.xSpeed = xSpeed;
            this.ySpeed = ySpeed;
        }

        public int xSpeed { get; set; }
        public int ySpeed { get; set; }
    }

    public enum AnimState
    {
        STAND_LEFT,
        STAND_RIGHT,
        WALK_LEFT,
        WALK_RIGHT,
        DUCK_LEFT,
        DUCK_RIGHT,
        DEAD_LEFT,
        DEAD_RIGHT
    }

    public class Player
    {
        public int id { get; set; }
        public int iHealth { get; set; }
        public int iLives { get; set; }
        public string sDirection { get; set; }
        public string sAnimation { get; set; }
        public bool isStanding { get; set; }
        public bool isStopping { get; set; }
        public bool isMovingLeft { get; set; }
        public bool isMovingRight { get; set; }
        public bool isCrouching { get; set; }
        public bool isDead { get; set; }
        public WebSocket socket { get; set; }

        public Bounds Bounds { get; set; }
        public Bounds OriginalBounds { get; set; }
        public Origin Origin { get; set; }
        public Origin OriginalOrigin { get; set; }
        [JsonIgnore]
        public Jump Jump { get; set; }
        [JsonIgnore]
        public Dictionary<AnimState, string> Animations { get; set; }
        public List<Bullet> Bullets { get; set; }

        public Velocity Velocity { get; set; }
        public Velocity VelocityMax { get; set; }


        public Player(int id, WebSocket socket)
        {
            #region anims
            this.Animations = new Dictionary<AnimState, string>();
            this.Animations.Add(AnimState.STAND_LEFT, "left.png");
            this.Animations.Add(AnimState.STAND_RIGHT, "right.png");
            this.Animations.Add(AnimState.DUCK_LEFT, "leftbend.png");
            this.Animations.Add(AnimState.DUCK_RIGHT, "rightbend.png");
            this.Animations.Add(AnimState.DEAD_LEFT, "leftdead.png");
            this.Animations.Add(AnimState.DEAD_RIGHT, "rightdead.png");
            this.Animations.Add(AnimState.WALK_LEFT, "leftwalk.png");
            this.Animations.Add(AnimState.WALK_RIGHT, "rightwalk.png");
            #endregion

            this.id = id;
            this.iHealth = 100;
            this.iLives = 3;
            this.socket = socket;
            this.sDirection = "RIGHT";
            this.sAnimation = id == 0 ? Animations[AnimState.STAND_RIGHT] : Animations[AnimState.STAND_LEFT];
            this.isStanding = false;
            this.isStopping = true;
            this.isCrouching = false;
            this.isDead = false;
 
            this.Bounds = new Bounds(80, 120);
            this.OriginalBounds = this.Bounds;
            this.Origin = new Origin(0, 0);
            this.OriginalOrigin = new Origin(0,0);
            


            this.Velocity = new Velocity(0, 0);
            this.VelocityMax = new Velocity(3, 3);
            this.Bullets = new List<Bullet>();



            Jump = new Jump(this);
            Init();
        }

        public void Init()
        {
            if (this.id == 0)
            {
                this.Origin = new Origin(-1000,-1000);
            }

            else
            {
                this.Origin = new Origin(1000, 1000);
            }
        }

        public void Run(List<Platform> platforms)
        {
            if (Jump.JUMP == JUMP.NULL || Jump.JUMP == JUMP.LANDING)
                this.Velocity = new Velocity(this.Velocity.xSpeed, this.Velocity.ySpeed + 1);
            else
                this.Velocity = Jump.JumpVelocity;

            this.Origin = new Origin(this.Origin.iX + this.Velocity.xSpeed, isStanding == false ? Origin.iY + Velocity.ySpeed : Origin.iY);

            HandleVelocity();
            HandlePlatforms(platforms);
            HandleStopMovement();
            HandleMovement();
            HandleHealth();

            Jump.Handle();
        }

        public void HandleGeneratedObject(GeneratedObject generatedObject)
        {
            if (generatedObject.Object == OBJECT.TANK)
            {
                Console.WriteLine("will create a tank");
                Task.Run(async () =>
                {
                    await generatedObject.Platform.Server.SendMessage(null, "DELETEOBJECT", true);
                    generatedObject = null;
                });
                
            }

            

        }
        public void HandleHealth()
        {
            if (iHealth <= 0)
            {
                iLives--;
                isDead = true;
            }
        }
        public void HandleMovement()
        {
            if (this.isDead)
            {
                this.sAnimation = this.sDirection == "LEFT" ? this.Animations[AnimState.DEAD_LEFT] : this.Animations[AnimState.DEAD_RIGHT];
                return;
            }

            if (this.isCrouching)
            {
                this.sAnimation = this.sDirection == "LEFT" ? this.Animations[AnimState.DUCK_LEFT] : this.Animations[AnimState.DUCK_RIGHT];
                this.Bounds = new Bounds(50, 40);
            }

            if (this.isMovingLeft)
            {
                this.sDirection = "LEFT";
                this.sAnimation = this.Animations[AnimState.STAND_LEFT];
                if (this.Velocity.xSpeed < -this.VelocityMax.xSpeed)
                {
                    this.Velocity = new Velocity(-this.VelocityMax.xSpeed, this.Velocity.ySpeed);
                    return;
                }

                if (this.Velocity.xSpeed < this.VelocityMax.xSpeed)
                    this.Velocity = new Velocity(this.Velocity.xSpeed - 1, this.Velocity.ySpeed);
            }

            else if (this.isMovingRight)
            {
                this.sDirection = "RIGHT";
                this.sAnimation = this.Animations[AnimState.STAND_RIGHT];
                this.Velocity = new Velocity(this.Velocity.xSpeed + 1, this.Velocity.ySpeed);
            }
        }
        public void HandleStopMovement()
        {
            if (this.isDead)
                return;

            if (!this.isStopping)
                return;

            if (!this.Bounds.Equals(this.OriginalBounds))
            {
                this.Origin = new Origin(this.Origin.iX, this.Origin.iY - (this.OriginalBounds.iHeight - this.Bounds.iHeight));
                this.Bounds = this.OriginalBounds;
            }

            if (!this.Velocity.Equals(new Velocity(0, this.Velocity.ySpeed)))
            {
                this.isMovingLeft = false;
                this.isMovingRight = false;
                this.isCrouching = false;

                if (this.Velocity.xSpeed < 0)
                    this.Velocity = new Velocity(this.Velocity.xSpeed + 1, this.Velocity.ySpeed);
                else
                    this.Velocity = new Velocity(this.Velocity.xSpeed - 1, this.Velocity.ySpeed);
            }

            else
                this.isStopping = false;

        }
        private void HandlePlatforms(List<Platform> platforms)
        {
            var x = FindNearestPlatform(platforms);
            if (x == null)
                return;

            if (x.iX < this.Origin.iX &&
                x.iX + x.iWidth > this.Origin.iX &&
                x.iY - this.Bounds.iHeight < this.Origin.iY &&
                x.iY > this.Origin.iY)
            {

                this.Jump.Reset();
                this.isStanding = true;


                if (Jump.JUMP == JUMP.JUMPING)
                    this.isStanding = false;

            }

            else
                this.isStanding = false;
        }

        private Platform FindNearestPlatform(List<Platform> platforms)
        {
            int closestDistance = 9999;
            Platform closestPlatform = null;
            platforms.ForEach(x =>
            {
                var distance = Math.Abs((x.iX + x.iWidth / 2) - (this.Origin.iX));
                if (x.iY > this.Origin.iY)
                {
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPlatform = x;
                    }
                }
            });


            if (closestPlatform != null)
                return closestPlatform;

            return null;
        }

        private void HandleVelocity()
        {
            //HORIZONTAL
            if (this.Velocity.xSpeed > this.VelocityMax.xSpeed)
                this.Velocity = new Velocity(this.VelocityMax.xSpeed, this.Velocity.ySpeed);
                

            //VERTICAL
            if (this.Velocity.ySpeed > this.VelocityMax.ySpeed)
                this.Velocity = new Velocity(this.Velocity.xSpeed, this.VelocityMax.ySpeed);

        }

        
    }
}
