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
                Name ="Archers", Description="Research: A ranged unit", ProgressText = "Constructing targets",
                Origin = TileType.Center,
                Cost = new Requirements() { Ticks = 30*30, Wood = 20, Stone = 10 }
            },
        };

        public readonly TrainingDescription[] Training = new TrainingDescription[]
        {
            new TrainingDescription()
            {
                Name="Worker", Description="Resource harvesting and Construction", ProgressText="Brainwashing",
                Origin = TileType.Center, CreatedUnit = UnitType.Worker,
                Cost = new Requirements() { Ticks = 30*8, Wood = 5, Meat = 1 }
            },
            new TrainingDescription()
            {
                Name="Soldier", Description="Shiny Sword not just for show", ProgressText="Sharpening the blade",
                Origin = TileType.Center, CreatedUnit = UnitType.Soldier,
                Cost = new Requirements() { Ticks = 30*15, Meat = 5, Metal = 3 }
            },
            new TrainingDescription()
            {
                Name="Archer", Description="Precisely Pelting Pointy Poles", ProgressText="Trying to hit the target",
                Origin = TileType.Center, CreatedUnit = UnitType.Archer, RequiredResearch = new string[] { "Archers" },
                Cost = new Requirements() { Ticks = 30*17, Wood = 5, Meat = 3, Metal = 1 }
            },
        };

        public readonly BuildingDescription[] Building = new BuildingDescription[]
        {
            new BuildingDescription()
            {
                Name="Storage Yard", Description="Store more resources", CreatedBuilding= TileType.Storage,
                Origin = UnitType.Worker, ProgressText = "Laying out a perfect circle",
                Cost = new Requirements() { Wood = 10, Stone = 10, Ticks = 30*25 }
            },
            new BuildingDescription()
            {
                Name = "House", Description="Home for your people", ProgressText="Framing and squaring", CreatedBuilding= TileType.House,
                Origin = UnitType.Worker,
                Cost = new Requirements() { Wood = 20, Stone = 10, Ticks = 30*20 }
            }
        };

        public readonly ResourceHarvestingDescription[] ResourceHarvesting = new ResourceHarvestingDescription[]
        {
            new ResourceHarvestingDescription()
            {
                ResourceName = "Forest", ProgressText = "Ravaging the local flora and fauna", HarvestBuilding = TileType.Forest, HarvestUnit = UnitType.Worker,
                PrimaryResource = ResourceType.Wood, ScarceResource = ResourceType.Meat, ScarcityFactor = 1000, MaxScarcity = 10,
                HarvestTicks = 75
            },
            new ResourceHarvestingDescription()
            {
                ResourceName = "Mine", ProgressText = "Tink... tink... tink", HarvestBuilding = TileType.Mine, HarvestUnit = UnitType.Worker,
                PrimaryResource = ResourceType.Stone, ScarceResource = ResourceType.Metal, ScarcityFactor= 1000, MaxScarcity = 10,
                HarvestTicks = 30*4
            }
        };

        public readonly UnitProperties[] UnitDetails = new UnitProperties[]
        {
            new UnitProperties()
            {
                 Name = "Worker", Unit = UnitType.Worker,
                 HP = 50, Attack = 8, Defense = 2, Range = 100, AttackTicks = 30*2,
                 Speed = 200, LoadedSpeed = 140
            },
            new UnitProperties()
            {
                Name = "Soldier", Unit = UnitType.Soldier,
                HP = 150, Attack = 25, Defense = 5, Range = 180, AttackTicks = 30,
                Speed = 160
            },
            new UnitProperties()
            {
                Name="Archer", Unit = UnitType.Archer,
                HP = 80, Attack = 45, Defense = 2, Range = 700, AttackTicks = 50,
                Speed = 180
            }
        };

        public readonly BuildingProperties[] BuildingDetails = new BuildingProperties[]
        {
            new BuildingProperties()
            {
                Name = "Town Center", Building = TileType.Center,
                HP = 1000, Defense = 4,
                BoostPopulation = 3, BoostStockpile = new PlayerResources() { Meat=100, Metal=100, Stone=100, Wood=100 }
            },
            new BuildingProperties()
            {
                Name = "Storage Yard", Building = TileType.Storage,
                HP = 200, Defense = 2,
                BoostStockpile = new PlayerResources() { Meat = 200, Metal = 200, Stone=200, Wood=200 }
            },
            new BuildingProperties()
            {
                Name = "House", Building = TileType.House,
                HP = 500, Defense = 0,
                BoostPopulation = 5
            }
        };


        public readonly float HPPercentWhilebuilding = 0.2f;
        public readonly float DestroyBuildingRefund = 0.3f;
        public readonly float CancelOperationRefund = 0.5f;
    }

    class ActionDescription
    {
        public string Name;
        public string Description;
        public string ProgressText;
        public Requirements Cost;
        public string[] RequiredResearch;
        public override string ToString()
        {
            return string.Format($"{this.GetType().Name}({Name})");
        }
    }

    class ResearchDescription : ActionDescription
    {
        public TileType Origin;
    }
    class TrainingDescription : ActionDescription
    {
        public TileType Origin;
        public UnitType CreatedUnit;
    }
    class BuildingDescription : ActionDescription
    {
        public UnitType Origin;
        public TileType CreatedBuilding;
        public TileType SuitableLocation = TileType.Land;
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

        public override string ToString()
        {
            return string.Format($"ResourceHarvestingDescription({ResourceName})");
        }
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

        public override string ToString()
        {
            return string.Format($"UnitProperties({Name})");
        }
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
        public int Defense; // Damage reduction
        public int BoostPopulation; // Some buildings increase the population limit
        public PlayerResources BoostStockpile; // Some buildings boost the amount of resources that can be held.

        public override string ToString()
        {
            return string.Format($"BuildingProperties({Name})");
        }
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
        public PlayerResources[] ResourceLimit;

        public int[] PlayerUnitLimit;

        public GameEngine(GameMap mapContext, int numPlayers)
        {
            Constants = new GameConstants();
            GenerateConstantLookup();
            Map = mapContext;
            PlayerCount = numPlayers;
            Resources = new PlayerResources[PlayerCount];
            ResourceLimit = new PlayerResources[PlayerCount];
            PlayerUnitLimit = new int[PlayerCount];

            PrepareMap();
        }

        public const int TickRate = 30;
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

        Dictionary<TileType, BuildingProperties> BuildingLookup;
        Dictionary<UnitType, UnitProperties> UnitLookup;
        void GenerateConstantLookup()
        {
            BuildingLookup = new Dictionary<TileType, BuildingProperties>();
            UnitLookup = new Dictionary<UnitType, UnitProperties>();

            foreach(var p in Constants.BuildingDetails)
            {
                BuildingLookup[p.Building] = p;
            }
            foreach(var u in Constants.UnitDetails)
            {
                UnitLookup[u.Unit] = u;
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
                    GameMapTile t = Map[p];
                    t.Owner = (byte)centers;
                    Map[p] = t;
                    centers++;
                }

                // Promote all existing buildings to "Built", and complete 
                BuildingProperties prop;
                if(BuildingLookup.TryGetValue(Map[p].Content, out prop))
                {
                    CompleteBuilding(p);
                }

            }
            if (centers != PlayerCount) throw new Exception("Unusable map, needs one town center per player");
            
            // Give players initial resources
            for(int i=0;i<PlayerCount;i++)
            {
                Resources[i] = Constants.InitialResources;
            }
        }




        void AdvanceTick()
        {
            Tick++;

            List<GameQueuedWork> CompletedItems = new List<GameQueuedWork>();
            // Complete any work items that complete on this tick
            foreach(var work in ActiveQueue)
            {
                work.CompletedTicks++;
                if(work.CompletedTicks >= work.RequiredTicks)
                {
                    CompletedItems.Add(work);
                    BusyLocations.Remove(work.QueueLocation);
                    CompleteWorkItem(work);
                }
            }
            foreach(var work in CompletedItems) { ActiveQueue.Remove(work); }

            // Promote any queued work items that can be promoted, and complete any finished work we find (buildings)
            CompletedItems.Clear();
            foreach(var work in StoppedQueue)
            {
                if(work.Active)
                {
                    // Only building tasks are active in stopped queue currently. They don't receive ticks, just receive updates from workers.
                    if(work.CompletedTicks >= work.RequiredTicks)
                    {
                        CompleteWorkItem(work);
                        CompletedItems.Add(work);
                    }
                }
                else
                {
                    // Can we enqueue this work item?
                    if(!BusyLocations.Contains(work.QueueLocation))
                    {
                        // yes.
                        work.Active = true;
                        ActiveQueue.Add(work);
                        BusyLocations.Add(work.QueueLocation);
                        CompletedItems.Add(work); // Tracking items to remove from StoppedQueue
                    }
                }
            }
            foreach (var work in CompletedItems) { StoppedQueue.Remove(work); }

            // Process all the unit actions

        }

        const int MaxQueueLength = 3;
        List<GameQueuedWork> ActiveQueue = new List<GameQueuedWork>();
        List<GameQueuedWork> StoppedQueue = new List<GameQueuedWork>();

        HashSet<string> CompletedResearch = new HashSet<string>();
        HashSet<Point> BusyLocations = new HashSet<Point>();

        public List<GameUnit> Units = new List<GameUnit>();


        //
        // Internal functions - Perform actions on the current tick
        // (Future, refactor to support replay)
        //

        bool IsResourceTile(Point tile, GameUnit unit)
        {
            TileType t = Map[tile].Content;
            foreach(var r in Constants.ResourceHarvesting)
            {
                if (r.HarvestUnit == unit.Unit && r.HarvestBuilding == t) return true;
            }
            return false;
        }
        bool IsResourceTile(Point tile)
        {
            TileType t = Map[tile].Content;
            foreach (var r in Constants.ResourceHarvesting)
            {
                if (r.HarvestBuilding == t) return true;
            }
            return false;
        }

        bool IsPartiallyBuilt(Point tile)
        {
            foreach (var q in StoppedQueue)
            {
                if (q.QueueLocation == tile && q.WorkType == QueuedWorkType.Construct) return true;
            }
            return false;
        }

        // Synonomous with "Is Building", I guess
        public bool IsAttackable(Point tile)
        {
            return BuildingLookup.ContainsKey(Map[tile].Content);
        }

        /// <summary>
        /// Informational call - made available to the game
        /// </summary>
        public bool IsStructure(Point tile)
        {
            if (IsAttackable(tile)) return true;
            if (IsResourceTile(tile)) return true;
            return false;
        }

        bool UnitCanBuild(Point tile, GameUnit unit)
        {
            TileType t = Map[tile].Content;
            foreach(var r in Constants.Building)
            {
                if (r.Origin == unit.Unit && r.CreatedBuilding == t) return true;
            }
            return false;
        }


        int TileQueueLength(Point tile)
        {
            int count = 0;
            foreach(var q in StoppedQueue)
            {
                if (q.QueueLocation == tile) count++;
            }
            return count;
        }

        bool PlayerDeductResources(int player, ref Requirements Cost)
        {
            // Confirm we have enough resources
            if(Resources[player].SufficientResources(Cost))
            {
                Resources[player].RemoveResources(Cost);
                return true;
            }
            return false;
        }
        void PlayerRefundResources(int player, ref Requirements Cost)
        {
            Resources[player].AddResources(Cost);
        }


        void CompleteResearch(GameQueuedWork work)
        {
            ResearchDescription desc = (ResearchDescription)work.TaskDescription;
            CompletedResearch.Add(desc.Name);
        }

        bool HaveRequiredResearch(ActionDescription action)
        {
            if (action.RequiredResearch == null) return true; // No required research
            foreach(string research in action.RequiredResearch)
            {
                if (!CompletedResearch.Contains(research)) return false; // Missing research.
            }
            return true;
        }

        BuildingDescription FindBuildingDescriptionForTile(Point location)
        {
            TileType t = Map[location].Content;
            foreach(var d in Constants.Building)
            {
                if (d.CreatedBuilding == t) return d;
            }
            return null;
        }


        void DestroyBuilding(Point location)
        {
            // Unapply building bonuses

        }
        void CancelBuilding(Point location)
        {
            BuildingDescription desc = FindBuildingDescriptionForTile(location);
            GameMapTile t = Map[location];
            t.Content = desc.SuitableLocation;
            Map[location] = t;
        }
        void CompleteBuilding(Point location)
        {
            BuildingProperties prop = BuildingLookup[Map[location].Content];
            GameMapTile t = Map[location];
            t.Built = true;
            t.HP = prop.HP;
            Map[location] = t;

            // Apply building effects
            int playerOwner = t.Owner;
            PlayerUnitLimit[playerOwner] += prop.BoostPopulation;
            ResourceLimit[playerOwner].AddResources(prop.BoostStockpile);
        }
        void StartBuilding(GameQueuedWork work)
        {
            Point location = work.QueueLocation;
            GameMapTile t = Map[location];
            BuildingDescription desc = (BuildingDescription)work.TaskDescription;
            BuildingProperties prop = BuildingLookup[desc.CreatedBuilding];
            t.Content = desc.CreatedBuilding;
            t.Owner = (byte)(work.Owner);
            t.HP = (int)Math.Ceiling(prop.HP * Constants.HPPercentWhilebuilding);
            t.Built = false;
            Map[location] = t;
            work.Active = true;
        }

        void CompleteTraining(GameQueuedWork work)
        {
            TrainingDescription td = (TrainingDescription)work.TaskDescription;
            UnitProperties up = UnitLookup[td.CreatedUnit];
            GameUnit u = new GameUnit()
            {
                Unit = td.CreatedUnit,
                Active = true,
                Owner = work.Owner,
                HP = up.HP
            };
            u.SetLocation(Map.StructureExitPoint(work.QueueLocation));
            u.SetTask(UnitTask.Idle);
            Units.Add(u);
        }



        void UnitReturnToStorage(GameUnit unit)
        {
            // Find nearest storage and path to it (ask pathfinder to find nearest from a list)
        }

        void UnitPathToLocation(GameUnit unit, Vector2 mapLocation)
        {
            unit.TargetLocation = mapLocation;

        }

        void UnitPathToTile(GameUnit unit, Point tileLocation)
        {
            unit.TargetTile = tileLocation;
            if(IsStructure(tileLocation))
            {
                UnitPathToLocation(unit, Map.StructureExitPoint(tileLocation));
            }
            else
            {
                Vector3 loc = Map.CenterPoint(tileLocation);
                UnitPathToLocation(unit, new Vector2(loc.X, loc.Y));
            }
        }


        void UnitHarvestTile(GameUnit unit, Point tileLocation)
        {
            unit.SetTask(UnitTask.Gather);
            unit.GatherTile = tileLocation;
            UnitPathToTile(unit, tileLocation);
        }
        
        void UnitConstructTile(GameUnit unit, Point tileLocation)
        {
            unit.SetTask(UnitTask.Build);
            UnitPathToTile(unit, tileLocation);
        }

        void UnitAttackTile(GameUnit unit, Point tileLocation)
        {
            unit.SetTask(UnitTask.AttackBuilding);
            UnitPathToTile(unit, tileLocation);
        }
        void UnitAttackUnit(GameUnit unit, GameUnit otherUnit)
        {
            unit.SetTask(UnitTask.AttackUnit);
            unit.TargetUnit = otherUnit;
            UnitPathToLocation(unit, otherUnit.Location);
        }

        void UnitMoveCommand(GameUnit unit, Vector2 targetLocation)
        {
            unit.SetTask(UnitTask.Move);
            UnitPathToLocation(unit, targetLocation);
        }


        void CompleteHarvest(GameQueuedWork work, bool giveResource = true)
        {
            // Generate resource for unit
            if(giveResource) // If not, collection was canceled.
            {

            }

            // Return unit to outside world
            work.InvolvedUnit.Active = true;
            work.InvolvedUnit.SetLocation(Map.StructureExitPoint(work.QueueLocation));

            // Instruct unit to return to the nearest storage facility
            UnitReturnToStorage(work.InvolvedUnit);
        }

        void CompleteDeposit(GameQueuedWork work)
        {
            // Return unit to outside world
            work.InvolvedUnit.Active = true;
            work.InvolvedUnit.SetLocation(Map.StructureExitPoint(work.QueueLocation));

            if(work.InvolvedUnit.Task == UnitTask.Gather)
            {
                UnitHarvestTile(work.InvolvedUnit, work.InvolvedUnit.GatherTile);
            }
            else
            {
                work.InvolvedUnit.SetTask(UnitTask.Idle);
            }
        }


        /// <summary>
        /// Queue work and take any important final steps related to the work type.
        /// </summary>
        void EnqueueWork(GameQueuedWork work)
        {
            switch(work.WorkType)
            {
                case QueuedWorkType.Construct:
                    StartBuilding(work);
                    break;
            }
            StoppedQueue.Add(work);
        }

        /// <summary>
        /// Called when the work item has timed out and expired, or otherwise been completed (e.g. buildings)
        /// </summary>
        void CompleteWorkItem(GameQueuedWork work)
        {
            work.Completed = true;
            switch(work.WorkType)
            {
                case QueuedWorkType.Construct:
                    CompleteBuilding(work.QueueLocation);
                    break;
                case QueuedWorkType.Harvest:
                    CompleteHarvest(work);
                    break;
                case QueuedWorkType.Research:
                    CompleteResearch(work);
                    break;
                case QueuedWorkType.ReturnResource:
                    CompleteDeposit(work);
                    break;
                case QueuedWorkType.Train:
                    CompleteTraining(work);
                    break;
            }
        }


        void CancelApplyRefund(int player, Requirements req)
        {
            PlayerResources res = PlayerResources.FromRequirements(req);
            res.DiscountResources(Constants.CancelOperationRefund);
            Resources[player].AddResources(res);
        }

        /// <summary>
        /// Cancel a pending work item and remove it from any queues.
        /// </summary>
        void CancelWorkItem(GameQueuedWork work)
        {
            switch (work.WorkType)
            {
                case QueuedWorkType.Construct:
                    // Issue partial refund
                    CancelApplyRefund(work.Owner, ((BuildingDescription)work.TaskDescription).Cost);
                    CancelBuilding(work.QueueLocation);
                    break;
                case QueuedWorkType.Harvest:
                    CompleteHarvest(work, false);
                    break;
                case QueuedWorkType.Research:
                    // Issue partial refund
                    CancelApplyRefund(work.Owner, ((ResearchDescription)work.TaskDescription).Cost);
                    break;
                case QueuedWorkType.ReturnResource:
                    CompleteDeposit(work); // Ok to cancel early, minor exploit case
                    break;
                case QueuedWorkType.Train:
                    // Issue partial refund
                    CancelApplyRefund(work.Owner, ((TrainingDescription)work.TaskDescription).Cost);
                    break;
            }

            work.Completed = true;
            ActiveQueue.Remove(work);
            StoppedQueue.Remove(work);
            if (work.Active)
            {
                // Remove busy building block
                BusyLocations.Remove(work.QueueLocation);
            }
        }


        GameQueuedWork CreateWorkFromAction(ActionDescription action)
        {
            GameQueuedWork w = new GameQueuedWork()
            {
                TaskDescription = action,
                RequiredTicks = action.Cost.Ticks,
                TaskProgressText = action.ProgressText + "..."
            };

            if(action is ResearchDescription)
            {
                w.WorkType = QueuedWorkType.Research;
            }
            else if(action is TrainingDescription)
            {
                w.WorkType = QueuedWorkType.Train;
            }
            else if(action is BuildingDescription)
            {
                w.WorkType = QueuedWorkType.Construct;
            }
            w.TaskName = w.WorkType.ToString() + " " + action.Name;
            return w;
        }

        void QueuePaidWork(int playerId, Point tileLocation, ActionDescription action)
        {
            GameQueuedWork work = CreateWorkFromAction(action);
            work.Owner = playerId;
            work.QueueLocation = tileLocation;
            EnqueueWork(work);
        }










        //
        // Interaction functions - Request actions from the game.
        // Below this point mostly functions do minimal interaction with state, sanity check, and call into internal functions to take action.
        //

        public ResourceHarvestingDescription ResourceDetailsForTile(Point tileLocation)
        {
            TileType t = Map[tileLocation].Content;
            foreach(var d in Constants.ResourceHarvesting)
            {
                if (d.HarvestBuilding == t) return d;
            }
            return null;
        }

        public BuildingProperties StructureDetailsForTile(Point tileLocation)
        {
            TileType t = Map[tileLocation].Content;
            foreach (var d in Constants.BuildingDetails)
            {
                if (d.Building == t) return d;
            }
            return null;
        }

        public UnitProperties GetUnitDetails(GameUnit unit)
        {
            UnitType t = unit.Unit;
            foreach(var d in Constants.UnitDetails)
            {
                if (d.Unit == t) return d;
            }
            return null;
        }


        public ActionDescription[] EnumerateActionsForTile(int playerId, Point tileLocation)
        {
            List<ActionDescription> actions = new List<ActionDescription>();
            if (Map[tileLocation].Built && Map[tileLocation].Owner == playerId)
            {
                TileType t = Map[tileLocation].Content;

                foreach (var action in Constants.Research)
                {
                    if (action.Origin == t)
                    {
                        if (CompletedResearch.Contains(action.Name)) continue; // don't allow queueing completed research.
                        if (!HaveRequiredResearch(action)) continue; // Don't include unresearched items
                        actions.Add(action);
                    }
                }
                foreach (var action in Constants.Training)
                {
                    if (!HaveRequiredResearch(action)) continue; // Don't include unresearched items
                    if (action.Origin == t) actions.Add(action);
                }
            }
            return actions.ToArray();
        }

        public ActionDescription[] EnumerateActionsForUnit(GameUnit unit)
        {
            List<ActionDescription> actions = new List<ActionDescription>();
            foreach(var action in Constants.Building)
            {
                if (action.Origin == unit.Unit) actions.Add(action);
            }
            return actions.ToArray();
        }

        public GameQueuedWork[] EnumerateQueueForTile(int playerId, Point tileLocation)
        {
            List<GameQueuedWork> queueOut = new List<GameQueuedWork>();
            foreach(var q in ActiveQueue)
            {
                if (q.QueueLocation == tileLocation && q.Owner == playerId) queueOut.Add(q);
            }
            foreach(var q in StoppedQueue)
            {
                if (q.QueueLocation == tileLocation && q.Owner == playerId) queueOut.Add(q);
            }
            return queueOut.ToArray();
        }

        public bool CanInteractWithTile(GameUnit unit, Point targetLocation)
        {
            if (IsResourceTile(targetLocation, unit)) return true;
            if (UnitCanBuild(targetLocation, unit) && IsPartiallyBuilt(targetLocation)) return true;
            if (IsAttackable(targetLocation) && unit.Owner != Map[targetLocation].Owner) return true;
            return false;
        }

        public EngineRequestStatus QueueActionForTile(int playerId, Point tileLocation, ActionDescription action)
        {
            GameMapTile t = Map[tileLocation];
            if (playerId != t.Owner)
            {
                System.Diagnostics.Debug.Print("Attempt to queue action for unowned tile. id {0}, location {1}, action {2}, tile {3}", playerId, tileLocation, action, t);
                return EngineRequestStatus.FailWrongPlayer;
            }
            if (!t.Built)
            {
                System.Diagnostics.Debug.Print("Attempt to queue action for unbuilt structure. id {0}, location {1}, action {2}, tile {3}", playerId, tileLocation, action, t);
                return EngineRequestStatus.FailUnspecified;
            }
            if (!HaveRequiredResearch(action))
            {
                System.Diagnostics.Debug.Print("Attempt to queue unresearched action. id {0}, location {1}, action {2}, tile {3}", playerId, tileLocation, action, t);
                return EngineRequestStatus.FailUnspecified;
            }
            int queueLength = TileQueueLength(tileLocation);
            if (queueLength == MaxQueueLength) return EngineRequestStatus.FailQueueFull;
            if (!PlayerDeductResources(playerId, ref action.Cost))
            {
                return EngineRequestStatus.FailNoResources;
            }
            QueuePaidWork(playerId, tileLocation, action);
            return EngineRequestStatus.Completed;
        }

        /// <summary>
        /// just "Build" for the time being. This will be like queuing a build task for the tile, then telling the unit to go build it.
        /// </summary>
        public EngineRequestStatus QueueActionForUnit(int playerId, GameUnit unit, Point targetLocation, ActionDescription action)
        {
            if (playerId != unit.Owner)
            {
                System.Diagnostics.Debug.Print("Attempt to queue action for unowned unit. id {0}, unit {1}, location {2} {3}, action {4}",
                    playerId, unit, targetLocation, Map[targetLocation], action);
                return EngineRequestStatus.FailWrongPlayer;
            }
            if (!HaveRequiredResearch(action))
            {
                System.Diagnostics.Debug.Print("Attempt to queue unresearched action. id {0}, unit {1}, location {2} {3}, action {4}",
                    playerId, unit, targetLocation, Map[targetLocation], action);
                return EngineRequestStatus.FailUnspecified;
            }
            if (action is BuildingDescription)
            {
                BuildingDescription desc = (BuildingDescription)action;
                if(Map[targetLocation].Content != desc.SuitableLocation)
                {
                    System.Diagnostics.Debug.Print("Attempt to build structure on unsuitable terrain. id {0}, unit {1}, location {2} {3}, action {4}",
                        playerId, unit, targetLocation, Map[targetLocation], action);

                    return EngineRequestStatus.FailUnspecified;
                }
            }
            if (!PlayerDeductResources(playerId, ref action.Cost))
            {
                return EngineRequestStatus.FailNoResources;
            }
            QueuePaidWork(playerId, targetLocation, action);
            UnitConstructTile(unit, targetLocation);

            return EngineRequestStatus.Completed;
        }

        public EngineRequestStatus CancelQueueElement(int playerId, GameQueuedWork work)
        {
            if (playerId != work.Owner)
            {
                System.Diagnostics.Debug.Print("Attempt to cancel unowned queue item. id {0}, queue {1}",
                    playerId, work);
                return EngineRequestStatus.FailWrongPlayer;
            }
            if(work.Completed)
            {
                System.Diagnostics.Debug.Print("Attempt to cancel completed queue item. id {0}, queue {1}",
                    playerId, work);
                return EngineRequestStatus.FailUnspecified;
            }
            CancelWorkItem(work);
            return EngineRequestStatus.Completed;
        }

        /// <summary>
        /// Interactions: Harvest resources, resume building, future: repair, Attack enemy building, 
        /// </summary>
        public EngineRequestStatus RequestInteractWithTile(int playerId, GameUnit unit, Point targetLocation)
        {
            if (playerId != unit.Owner)
            {
                System.Diagnostics.Debug.Print("Attempt to request interaction for unowned unit. id {0}, unit {1}, location {2} {3}",
                    playerId, unit, targetLocation, Map[targetLocation]);
                return EngineRequestStatus.FailWrongPlayer;
            }
            if (IsResourceTile(targetLocation, unit))
            {
                UnitHarvestTile(unit, targetLocation);
            }
            return EngineRequestStatus.FailUnspecified;
        }

        public EngineRequestStatus MoveUnit(int playerId, GameUnit unit, Vector2 targetLocation)
        {
            if (playerId != unit.Owner)
            {
                System.Diagnostics.Debug.Print("Attempt to move unowned unit. id {0}, unit {1}, location {2} ",
                    playerId, unit, targetLocation);
                return EngineRequestStatus.FailWrongPlayer;
            }
            UnitMoveCommand(unit, targetLocation);
            return EngineRequestStatus.Completed;
        }
        public EngineRequestStatus AttackUnit(int playerId, GameUnit unit, GameUnit targetUnit)
        {
            if (playerId != unit.Owner)
            {
                System.Diagnostics.Debug.Print("Attempt to request attack using unowned unit. id {0}, unit {1}, targetUnit {2} ",
                    playerId, unit, targetUnit);
                return EngineRequestStatus.FailWrongPlayer;
            }
            if(unit.Owner == targetUnit.Owner)
            {
                // For now, disable this case. Make it possible in the future.
                System.Diagnostics.Debug.Print("Attempt to request attack on friendly unit. id {0}, unit {1}, targetUnit {2} ",
                    playerId, unit, targetUnit);
                return EngineRequestStatus.FailWrongPlayer;
            }
            UnitAttackUnit(unit, targetUnit);
            return EngineRequestStatus.Completed;
        }

    }

    enum EngineRequestStatus
    {
        Completed,
        FailUnspecified,
        FailWrongPlayer,
        FailNoResources,
        FailQueueFull
    }


    struct PlayerResources
    {
        public int Wood, Meat, Stone, Metal;
        public void AddResources(PlayerResources res)
        {
            Wood += res.Wood;
            Meat += res.Meat;
            Stone += res.Stone;
            Metal += res.Metal;
        }
        public void RemoveResources(PlayerResources res)
        {
            Wood -= res.Wood;
            Meat -= res.Meat;
            Stone -= res.Stone;
            Metal -= res.Metal;
        }
        public void AddResources(Requirements res)
        {
            Wood += res.Wood;
            Meat += res.Meat;
            Stone += res.Stone;
            Metal += res.Metal;
        }

        public void RemoveResources(Requirements res)
        {
            Wood -= res.Wood;
            Meat -= res.Meat;
            Stone -= res.Stone;
            Metal -= res.Metal;
        }
        public bool SufficientResources(Requirements res)
        {
            if (Wood < res.Wood) return false;
            if (Meat < res.Meat) return false;
            if (Stone < res.Stone) return false;
            if (Metal < res.Metal) return false;
            return true;
        }

        /// <summary>
        /// Use on a copy of a cost/requirements to e.g. refund reduced amounts of resources.
        /// </summary>
        public void DiscountResources(float multiplyPercent)
        {
            Wood = (int)Math.Ceiling(Wood * multiplyPercent);
            Meat = (int)Math.Ceiling(Meat * multiplyPercent);
            Stone = (int)Math.Ceiling(Stone * multiplyPercent);
            Metal = (int)Math.Ceiling(Metal * multiplyPercent);
        }


        public void AdjustResource(ResourceType resType, int adjust)
        {
            switch(resType)
            {
                case ResourceType.Wood: Wood += adjust; break;
                case ResourceType.Meat: Meat += adjust; break;
                case ResourceType.Stone: Stone += adjust; break;
                case ResourceType.Metal: Metal += adjust; break;
                default: throw new Exception("Invalid resource type");
            }
        }
        public int GetResource(ResourceType resType)
        {
            switch (resType)
            {
                case ResourceType.Wood: return Wood;
                case ResourceType.Meat: return Meat;
                case ResourceType.Stone: return Stone;
                case ResourceType.Metal: return Metal;
                default: throw new Exception("Invalid resource type");
            }
        }

        public static PlayerResources FromRequirements(Requirements r)
        {
            PlayerResources res = new PlayerResources();
            res.AddResources(r);
            return res;
        }
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
        AttackBuilding, // Fixed target
        AttackUnit // May need to re-path for moving target
    }

    class GameUnit
    {
        public int Owner;
        public int integerX, integerY; // integer representation of x/y, in units of 1/256 tile.
        public float X { get { return integerX / 256.0f; } }
        public float Y { get { return integerY / 256.0f; } }
        public Vector2 Location {  get { return new Vector2(X, Y); } }
        public UnitType Unit;
        public UnitTask Task;
        public Vector2 TargetLocation;
        public Point TargetTile;
        public Point GatherTile;
        public GameUnit TargetUnit;

        public int HP;

        public bool Active;
        public bool HaveLoad;
        public ResourceType ResourceLoad;

        public int IdleCounter;

        public override string ToString()
        {
            return $"GameUnit(({X},{Y}) {Unit} {Task} Active {Active},Owner {Owner},Target {TargetTile})";
        }

        public void SetLocation(Vector2 loc)
        {
            integerX = (int)Math.Round(loc.X * 256);
            integerY = (int)Math.Round(loc.Y * 256);
        }
        public void SetTask(UnitTask newTask)
        {
            Task = newTask;
            IdleCounter = 0;
        }
    }

    enum QueuedWorkType
    {
        Train,
        Harvest,
        ReturnResource,
        Construct, // construction work is not time based so always stays in the stopped queue.
        Research
    }

    class GameQueuedWork
    {
        public int Owner;
        public string TaskName, TaskProgressText;

        public bool Active;
        public bool Completed;
        public int CompletedTicks;
        public int RequiredTicks;
        public QueuedWorkType WorkType;
        public Point QueueLocation;
        public GameUnit InvolvedUnit;
        public object TaskDescription;

        public override string ToString()
        {
            return $"GameQueuedWork({TaskName},Owner {Owner},Active {Active},Completed/Required {CompletedTicks}/{RequiredTicks},{QueueLocation},{InvolvedUnit})";
        }
    }

}
