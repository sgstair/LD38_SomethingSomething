using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD38
{
    // Keep as many of the constants and game definition ideas up here at the top, out of the engine code itself
    class GameConstants
    {
        public readonly PlayerResources InitialResources = new PlayerResources() { Wood = 50, Meat = 10, Stone = 10, Metal = 0 };

        public readonly ResearchDescription[] Research = new ResearchDescription[]
        {
            new ResearchDescription()
            {
                Name ="Archers", Description="A ranged unit", ProgressText = "Constructing targets",
                Origin = TileType.Center,
                ResearchCost = new Requirements() { Ticks = 30*30, Wood = 20, Stone = 10 }
            },
        };

        public readonly TrainingDescription[] Training = new TrainingDescription[]
        {
            new TrainingDescription()
            {
                UnitName="Worker", Description="Anything from resource harvesting to constructing buildings", ProgressText="Brainwashing",
                Origin = TileType.Center, CreatedUnit = UnitType.Worker,
                TrainingCost = new Requirements() { Ticks = 30*8, Wood = 5, Meat = 1 }
            },
            new TrainingDescription()
            {
                UnitName="Soldier", Description="Shiny Sword not just for show", ProgressText="Sharpening the blade",
                Origin = TileType.Center, CreatedUnit = UnitType.Soldier,
                TrainingCost = new Requirements() { Ticks = 30*15, Meat = 5, Metal = 3 }
            },
            new TrainingDescription()
            {
                UnitName="Archer", Description="Precisely Pelting Pointy Poles", ProgressText="Trying to hit the target",
                Origin = TileType.Center, CreatedUnit = UnitType.Archer, RequiredResearch = new string[] { "Archers" },
                TrainingCost = new Requirements() { Ticks = 30*17, Wood = 5, Meat = 3, Metal = 1 }
            },
        };

        public readonly BuildingDescription[] Building = new BuildingDescription[]
        {
            new BuildingDescription()
            {
                BuildingName="Storage Yard", Description="Store resources closer to where they're being harvested", CreatedBuilding= TileType.Storage,
                Origin = UnitType.Worker, ProgressText = "Laying out a perfect circle",
                BuildingCost = new Requirements() { Wood = 10, Stone = 10, Ticks = 30*25 }
            },
            new BuildingDescription()
            {
                BuildingName = "House", Description="Home for your people", ProgressText="Framing and squaring", CreatedBuilding= TileType.House,
                Origin = UnitType.Worker,
                BuildingCost = new Requirements() { Wood = 20, Stone = 10, Ticks = 30*20 }
            }
        };

        public readonly ResourceHarvestingDescription[] ResourceHarvesting = new ResourceHarvestingDescription[]
        {
            new ResourceHarvestingDescription()
            {

            }
        };

    }

    class ResearchDescription
    {
        public string Name;
        public string Description;
        public string ProgressText;
        public Requirements ResearchCost;
        public TileType Origin;
    }
    class TrainingDescription
    {
        public string UnitName;
        public string Description;
        public string ProgressText;
        public Requirements TrainingCost;
        public TileType Origin;
        public UnitType CreatedUnit;
        public string[] RequiredResearch;
    }
    class BuildingDescription
    {
        public string BuildingName;
        public string Description;
        public string ProgressText;
        public Requirements BuildingCost;
        public UnitType Origin;
        public TileType CreatedBuilding;
        public string[] RequiredResearch;
    }
    class ResourceHarvestingDescription
    {
        public string ResourceName;
        public string ProgressText;
        public UnitType HarvestUnit;
        public TileType HarvestBuilding;
        public int HarvestTicks;
        public ResourceType PrimaryResource;
        public ResourceType ScarceResource;
        public int ScarcityFactor; // Adjust how often the Scarce resource is returned based on how many resources remain.
                                   // Return scarce resource after 1 + Math.Floor(ScarcityFactor / RemainingResources) primary resources.
        public int MaxScarcity; // Maximum number of primary resources between scarce resources.
    }
    class UnitProperties
    {
        public string Name;
        public UnitType Unit;
        public int HP;
        public int Range; // in 1/256 tiles
        public int Attack; // raw HP
        public int Defense; // Basic damage reduction
        public int AttackTicks; // Duration of attack
        public int Speed; // 1/256 tiles per second
        public int LoadedSpeed; // Go slower when encumbered? Zero = ignored.
    }
    class BuildingProperties
    {
        public string Name;
        public TileType Building;
        public int HP;
        public int Range; // Turrets can attack, others 0. In 1/256 tiles
        public int Rangeboost; // Future: if higher off the ground
        public int Attack;
        public int AttackTicks;
        public int BoostPopulation; // Some buildings increase the population limit
        public PlayerResources BoostStockpile; // Some buildings boost the amount of resources that can be held.
    }



    /// <summary>
    /// Game engine
    /// </summary>
    class GameEngine
    {
        GameConstants Constants;
        GameMap Map;
        public readonly int PlayerCount;
        public PlayerResources[] Resources;

        public GameEngine(GameMap mapContext, int numPlayers)
        {
            Constants = new GameConstants();
            Map = mapContext;
            PlayerCount = numPlayers;
            Resources = new PlayerResources[PlayerCount];

            PrepareMap();
        }

        const int TickRate = 30;
        long Tick = 0;
        double timeOverflow;

        public void UpdateTime(double secondsPassed)
        {
            secondsPassed += timeOverflow;
            int ticks = (int)Math.Floor(secondsPassed * TickRate);
            timeOverflow = secondsPassed - (double)ticks / TickRate;
            for(int i=0;i< ticks;i++)
            {
                AdvanceTick();
            }
        }



        public Vector3 SuggestCameraStartLocation;

        void PrepareMap()
        {
            // We expect to see one town center per player, and they probably won't be assigned.
            int centers = 0;
            foreach(Point p in Map.EnumerateMap())
            {
                if(Map[p].Content == TileType.Center)
                {
                    if (centers == 0) SuggestCameraStartLocation = Map.CenterPoint(p);
                    Map[p].SetOwner(centers);
                    centers++;
                }
            }
            if (centers != 2) throw new Exception("Unusable map, needs 2 town centers");
        }




        void AdvanceTick()
        {
            // Do all the things that need to happen this tick
        }


        //Interaction functions - Request actions from the game.



    }

    struct PlayerResources
    {
        public int Wood, Meat, Stone, Metal;
    }

    struct Requirements
    {
        public int Wood, Meat, Stone, Metal;
        public int Ticks;
    }

    enum ResourceType
    {
        Wood,
        Meat,
        Stone,
        Metal
    }

    enum UnitType
    {
        Worker,
        Soldier,
        Archer,
        Siege
    }
    enum UnitTask
    {
        Idle,
        Gather,
        Build,
        Move,
        Attack
    }

    class GameUnit
    {
        public int integerX, integerY;
        public UnitType Unit;
        public UnitTask Task;
        public Point TargetLocation;
    }

    enum QueuedWorkType
    {
        CreateUnit,
        HarvestResource,
        ReturnResource,
        Research
    }

    class GameQueuedWork
    {
        public long CompletionTick;
        public QueuedWorkType WorkType;
        public Point QueueLocation;
        public GameUnit InvolvedUnit;

    }

}
