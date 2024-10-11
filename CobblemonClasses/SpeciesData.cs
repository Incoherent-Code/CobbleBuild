using Newtonsoft.Json;

namespace CobbleBuild.CobblemonClasses {
   //Dang brits and their weird spelling of things
   //I make alot of typos as is and now I have to put random U's everywhere and spell defense like a thrid grader
   /// <summary>
   /// Species Data for all cobblemon deserializes to this. Now with defaults.
   /// </summary>

   public class SpeciesData {
      public string name = "Bulbasaur";
      public int nationalPokedexNumber = 1;
      public StatSet baseStats = new StatSet();
      public float maleRatio = 0.5f;
      public int catchRate = 45;
      public float baseScale = 1f;
      public int baseExperienceYeild = 10;
      public int baseFriendship = 0;
      public StatSet evYeild = new StatSet();
      public string experienceGroup = "erratic";
      public HitboxEntry hitbox = new HitboxEntry() { width = 1, height = 1, @fixed = false };
      public string primaryType = "grass";
      public string? secondaryType = null;
      public string[] abilities = [];
      public bool shoulderMountable = false;
      //TODO: ShoulderEffects
      public string[] moves = [];
      public EvolutionEntry[] evolutions = [];
      public string[] features = [];
      public float? standingEyeHeight = null;
      public float? swimmingEyeHeight = null;
      public float? flyingEyeHeight = null;
      public BehaviorClass behaviour = new BehaviorClass();
      public string[] pokedex = [];
      public DropTable drops = new DropTable();
      public int eggCycles = 120;
      public string[] eggGroups = [];
      public bool cannotDynamax = false;
      public bool implemented = false;
      public float height = 1f;
      public float weight = 1f;

      //These weren't in speciesData.kt but I Observed them
      public FormData[]? forms = null;
   }
   public class FormData {
      public string name;
      public StatSet? baseStats;
      public float? maleRatio;
      public HitboxEntry? hitbox;
      public int? catchRate;
      public string? experienceGroup;
      public int? baseExperienceYield;
      public int? baseFriendship;
      public StatSet? evYeild;
      public string? primaryType;
      public string? secondaryType;
      public bool? shoulderMountable;
      public string[]? moves;
      public EvolutionEntry[]? evolutions;
      public string[]? abilities;
      public DropTable? drops;
      public string[]? pokedex;
      public string? preEvolution;
      public float? standingEyeHeight;
      public float? swimmingEyeHeight;
      public float? flyingEyeHeight;
      public string[]? labels;
      /// <summary>
      /// FormData.kt has it as dynamaxBlocked but all instances I know have it as cannotDynamax;
      /// </summary>
      public bool? cannotDynamax;
      public string[]? eggGroups;
      public float? height;
      public float? weight;
      public string? requiredMove;
      public string? requiredItem;
      public string? requiredItems;
      //TODO: gigantamaxMove
      public string? battleTheme;

      //These weren't in form data but I observed them being used
      public bool? battleOnly;
   }
   public class StatSet {
      public int hp;
      public int attack;
      public int defence;
      public int special_attack;
      public int special_defence;
      public int speed;
   }
   public class DropTable {
      public int? amount;
      public DropEntry[]? entries;
   }
   public class DropEntry {
      public string item;
      public float? percentage;
      public string? quantityRange;
   }

   public class HitboxEntry {
      public float width;
      public float height;
      [JsonProperty("fixed")]
      public bool @fixed;
   }

   public class EvolutionEntry {
      public string id = "id";
      public string variant = "passive";
      public string result = "unown";
      public bool optional = true;
      public bool consumeHeldItem = true;
      public string[] learnableMoves = [];
      public object? requiredContext;
      public Requirement[] requirements = [];
      public class Requirement {
         /// <summary>
         /// Indicates what kind of requirement this is.
         /// </summary>
         public string? variant;
         public int? minLevel;
         public int? maxLevel;
         public int? amount;
         public string? range;
         public string? type;
         public bool? isRaining;
         public bool? isThundering;
         /// <summary>
         /// Using a specfic move allows evolution. Uses amount to determine amount of moves.
         /// </summary>
         public string? move;
         /// <summary>
         /// Used with dimension requirement. Ex("minecraft:the_overworld")
         /// </summary>
         public string? identifier;
         //Unsure about how range (time range requirement) is handled.
         /// <summary>
         /// Used with any reqirement
         /// </summary>
         public Requirement[]? possibilities;
         /// <summary>
         /// Used with Moon Phase Requirement
         /// </summary>
         public string? moonPhase;
         /// <summary>
         /// Used with attack defense ratio requirement.
         /// </summary>
         public string? ratio;
         /// <summary>
         /// Used with held item requirement.
         /// </summary>
         public string? itemCondition;
         public string? target;
         /// <summary>
         /// Used with Party Requirement
         /// </summary>
         public bool? contains;
      }
   }

   public class BehaviorClass {
      public MoveData moving = new MoveData();
      public RestData resting = new RestData();
      public IdleData idle = new IdleData();

      public class MoveData {
         public Walk walk = new Walk();
         public Fly fly = new Fly();
         public Swim swim = new Swim();
         public bool canLook = true;
         public float wanderSpeed = 1f;
         public float wanderChance = 120f;
         public bool lookAtEntities = true;
         public class Walk {
            public bool canWalk = true;
            public bool avoidsLand = false;
            public float walkSpeed = 0.35f;
         }
         public class Fly {
            public bool canFly = false;
            public float flySpeedHorizontal = 0.3f;
         }
         public class Swim {
            public bool avoidsWater = false;
            public float swimSpeed = 0.3f;
            public bool canSwimInWater = true;
            public bool canSwimInLava = true;
            public bool canBreatheUnderwater = false;
            public bool canBreathUnderlava = false;
            public bool hurtByLava = true;
            public bool canWalkOnWater = false;
            public bool canWalkOnLava = false;
         }
      }
      public class RestData {
         public bool canSleep = false;
         public bool willSleepOnBed = false;
         public string depth = "normal";
         /// <summary>
         /// Value will be like "0-4" as a range
         /// </summary>
         public string light = "0-15";
         public float sleepChance = 1f / 600f;
         public string times = "night";
         //Biomes and blocks are unused and im not sure how they serialize.
      }
      public class IdleData {
         public bool pointAtSpawn = false;
      }
   }
   //[Obsolete]
   //public class SpeciesDataOld {
   //    public bool? implemented;
   //    public string name;
   //    public int nationalPokedexNumber;
   //    public string primaryType;
   //    public string? secondaryType;
   //    public string[] abilities;
   //    public StatSet baseStats;
   //    public string[] moves;
   //    public string experienceGroup;
   //    public int catchRate;
   //    public float maleRatio;
   //    public int baseExperienceYeild;
   //    public DropData? drops;
   //    public float? baseScale;
   //    public StatSet evYeild;
   //    public int baseFriendship;
   //    public int height;
   //    public int weight;
   //    public bool cannotDynamax;
   //    public HitboxEntry? hitbox;
   //    public EvolutionEntry[]? evolutions;
   //    public BehaviorClass behaviour = new BehaviorClass();
   //    public bool? shoulderMountable;
   //    //Unknown type: shoulderEffects
   //    public string[]? pokedex;
   //    public string? preEvolution;
   //    public float? standingEyeHeight;
   //    public float? swimmingEyeHeight;
   //    public float? flyingEyeHeight;
   //    public string[]? labels;
   //    public bool? dynamaxBlocked;
   //    public string[]? eggGroups;
   //    public string? requiredMove;
   //    public string? requiredItem;
   //    public string? requiredItems;

   //}
}