using Newtonsoft.Json;

namespace CobbleBuild.BedrockClasses {
   public class ServerEntityJson : BedrockJson {
      [JsonProperty("minecraft:entity")]
      public ServerEntity server_entity;
      public ServerEntityJson(ServerEntity serverEntity) : base("1.20.40") {
         server_entity = serverEntity;
      }
   }
   public class ServerEntity {
      public Description description { get; set; }
      public Components? components { get; set; } = new Components();
      public Dictionary<string, Components> component_groups = new Dictionary<string, Components>();
      public Dictionary<string, Event> events = new Dictionary<string, Event>();
      public ServerEntity(string identifier) {
         description = new Description(identifier);
      }
      public class Description {
         public string identifier { get; set; }
         public Dictionary<string, Property>? properties { get; set; }

         //There are few situations where I would change these
         public bool is_spawnable = true;
         public bool is_summonable = true;
         public bool is_experimental = false;
         public string? runtime_identifier; //Deprecated but still useful in certain scenarios

         public Description(string Identifier) {
            identifier = Identifier;
         }
      }
      /// <summary>
      /// Giant class representing all components.
      /// Meant to replace the dictionary of Components before I refactor the behavior code.
      /// The property names are written in camel case, but an underscore symbolizes a . in the component name.
      /// </summary>
      public class Components {
         //Normal Components
         [JsonProperty("minecraft:breathable")]
         public Component.Breathable? breathable;
         [JsonProperty("minecraft:can_fly")]
         public Component? canFly;
         [JsonProperty("minecraft:collision_box")]
         public widthAndHeight? collisionBox;
         [JsonProperty("minecraft:custom_hit_test")]
         public Component.CustomHitTest? customHitTest;
         [JsonProperty("minecraft:damage_sensor")]
         public Component.DamageSensor? damageSensor;
         [JsonProperty("minecraft:despawn")]
         public Component.Despawn? despawn;
         [JsonProperty("minecraft:type_family")]
         public Component.Family? family;
         [JsonProperty("minecraft:flying_speed")]
         public Component.BasicValue<float>? flyingSpeed;
         [JsonProperty("minecraft:follow_range")]
         public Component.FollowRange? followRange;
         [JsonProperty("minecraft:health")]
         public Component.Health? health;
         [JsonProperty("minecraft:instant_despawn")]
         public Component.InstantDespawn? instantDespawn;
         [JsonProperty("minecraft:is_tamed")]
         public Component? isTamed;
         [JsonProperty("minecraft:jump.dynamic")]
         public Component? jumpDynamic;
         [JsonProperty("minecraft:jump.static")]
         public Component.JumpStatic? jumpStatic;
         [JsonProperty("minecraft:interact")]
         public Component.Interact? interact;
         [JsonProperty("minecraft:inventory")]
         public Component.Inventory? inventory;
         [JsonProperty("minecraft:loot")]
         public Component.Loot? loot;
         [JsonProperty("minecraft:leashable")]
         public Component.Leashable? leashable;
         [JsonProperty("minecraft:movement")]
         public Component.BasicValue<float>? movement;
         [JsonProperty("minecraft:permute_type")]
         public Component.PermuteType? permuteType;
         [JsonProperty("minecraft:physics")]
         public Component? physics;
         [JsonProperty("minecraft:projectile")]
         public Component.Projectile? projectile;
         [JsonProperty("minecraft:pushable")]
         public Component.Pushable? pushable;
         [JsonProperty("minecraft:scale")]
         public Component.BasicValue<float>? scale;
         [JsonProperty("minecraft:tameable")]
         public Component.Tameable? tameable;
         [JsonProperty("minecraft:transformation")]
         public Component.Transformation? transformation;
         [JsonProperty("minecraft:underwater_movement")]
         public Component.BasicValue<float>? underwaterMovement;
         [JsonProperty("minecraft:variant")]
         public Component.BasicValue<int>? variant;

         //Navigation Components
         [JsonProperty("minecraft:navigation.climb")]
         public Component.Navigation? navigation_climb;
         [JsonProperty("minecraft:navigation.float")]
         public Component.Navigation? navigation_float;
         [JsonProperty("minecraft:navigation.fly")]
         public Component.Navigation? navigation_fly;
         [JsonProperty("minecraft:navigation.generic")]
         public Component.Navigation? navigation_generic;
         [JsonProperty("minecraft:navigation.hover")]
         public Component.Navigation? navigation_hover;
         [JsonProperty("minecraft:navigation.swim")]
         public Component.Navigation? navigation_swim;
         [JsonProperty("minecraft:navigation.walk")]
         public Component.Navigation? navigation_walk;

         //Behavior Components
         [JsonProperty("minecraft:behavior.eat_block")]
         public Component.Behavior.eatBlock? behavior_eatBlock;
         [JsonProperty("minecraft:behavior.float")]
         public Component.Behavior.Float? behavior_float;
         [JsonProperty("minecraft:behavior.follow_owner")]
         public Component.Behavior.FollowOwner? behavior_followOwner;
         [JsonProperty("minecraft:behavior.look_at_player")]
         public Component.Behavior.LookAtPlayer? behavior_lookAtPlayer;
         [JsonProperty("minecraft:behavior.melee_attack")]
         public Component.Behavior.MeleeAttack? behavior_meleeeAttack;
         [JsonProperty("minecraft:behavior.nearest_attackable_target")]
         public Component.Behavior.NearestAttackableTarget? behavior_nearestAttackableTarget;
         [JsonProperty("minecraft:behavior.random_look_around")]
         public Component.Behavior.RandomLook? behavior_randomLook;
         [JsonProperty("minecraft:behavior.random_stroll")]
         public Component.Behavior.RandomStroll? behavior_randomStroll;
         [JsonProperty("minecraft:behavior.random_swim")]
         public Component.Behavior.RandomSwim? behavior_randomSwim;
         [JsonProperty("minecraft:behavior.swim_idle")]
         public Component.Behavior.SwimIdle? behavior_swimIdle;
         [JsonProperty("minecraft:behavior.swim_wander")]
         public Component.Behavior.SwimWander? behavior_swimWander;

         //Movement Components
         [JsonProperty("minecraft:movement.amphibious")]
         public Component.Movement.Basic? movement_amphibious;
         [JsonProperty("minecraft:movement.basic")]
         public Component.Movement.Basic? movement_basic;
         [JsonProperty("minecraft:movement.dolphin")]
         public Component? movement_dolphin;
         [JsonProperty("minecraft:movement.fly")]
         public Component.Movement.Fly? movement_fly;
         [JsonProperty("minecraft:movement.generic")]
         public Component.Movement.Basic? movement_generic;
         [JsonProperty("minecraft:movement.glide")]
         public Component.Movement.Fly? movement_glide;
         [JsonProperty("minecraft:movement.hover")]
         public Component.Movement.Basic? movement_hover;
         [JsonProperty("minecraft:movement.jump")]
         public Component.Movement.Jump? movement_jump;
         [JsonProperty("minecraft:movement.skip")]
         public Component.Movement.Basic? movement_skip;
         [JsonProperty("minecraft:movement.sway")]
         public Component.Movement.Sway? movement_sway;

      }

      //When did they add entity variables?
      public class Property {
         public string? type { get; set; }

         //Only use for int and float (2 values)
         public object[]? range { get; set; }

         //Only use for enum
         public string[]? values { get; set; }
         [JsonProperty("default")]
         public object? @default { get; set; }
         public bool? client_sync { get; set; }
         public Property() { }

         //These make all the different types of properties
         public static Property makeBool(bool @default, bool? clientSync) {
            Property output = new Property();
            output.@default = @default;
            output.type = "bool";
            output.client_sync = clientSync;
            return output;
         }
         /// <summary>
         /// Creates an Enum Entity Property
         /// Default can either be int or molang expression (string)
         /// </summary>
         /// <param name="values">Has a maximum of 16 different enum values</param>
         /// <param name="default">Either a string (molang expression) or int</param>
         /// <param name="clientSync">Whether or not the property can be accessed by the client</param>
         /// <returns></returns>
         public static Property makeEnum(string[] values, string @default, bool? clientSync) {
            Property output = new Property();
            output.@default = @default;
            output.type = "enum";
            output.values = values;
            output.client_sync = clientSync;
            return output;
         }
         /// <summary>
         /// Creates an int Entity Property
         /// Default can either be int or molang expression (string)
         /// </summary>
         /// <param name="rangeMin"></param>
         /// <param name="rangeMax"></param>
         /// <param name="default">Either a string (molang expression) or int</param>
         /// <param name="clientSync">Whether or not the property can be accessed by the client</param>
         /// <returns></returns>
         public static Property makeInt(int rangeMin, int rangeMax, object @default, bool? clientSync) {
            Property output = new Property();
            output.@default = @default;
            output.type = "int";
            output.range = new object[] { rangeMin, rangeMax };
            output.client_sync = clientSync;
            return output;
         }
         /// <summary>
         /// Creates a Float Entity Property
         /// Default can either be float or molang expression (string)
         /// </summary>
         /// <param name="rangeMin"></param>
         /// <param name="rangeMax"></param>
         /// <param name="default">Either a string (molang expression) or float</param>
         /// <param name="clientSync">Whether or not the property can be accessed by the cleint</param>
         /// <returns></returns>
         public static Property makeFloat(float rangeMin, float rangeMax, object @default, bool? clientSync) {
            Property output = new Property();
            output.@default = @default;
            output.type = "float";
            output.range = new object[] { rangeMin, rangeMax };
            output.client_sync = clientSync;
            return output;
         }
      }
      /// <summary>
      /// Not all event nodes are in here, but the basic cones are
      /// </summary>
      public class Event {
         public AddClass? add { get; set; }
         public RemoveClass? remove { get; set; }
         public List<RandomObject>? randomize { get; set; }
         public List<SequenceObject>? sequence { get; set; }
         public string? trigger { get; set; }
         public Dictionary<string, object>? set_property { get; set; }

         ///// <summary>
         ///// Deprecated in minecraft for queue_command now
         ///// </summary>
         //[Obsolete]
         //public CommandClass? run_command { get; set; }
         /// <summary>
         /// same as run command but executes at end of game tick (At least I assume)
         /// </summary>
         public CommandClass? queue_command { get; set; }
         public Event() { }
         public class AddClass {
            [JsonProperty("component_groups")]
            public string[] componentGroups { get; set; }
            public AddClass(string[] ComponentGroups) {
               componentGroups = ComponentGroups;
            }
         }
         public class RemoveClass {
            [JsonProperty("component_groups")]
            public string[] componentGroups { get; set; }
            public RemoveClass(string[] ComponentGroups) {
               componentGroups = ComponentGroups;
            }
         }
         public class CommandClass {
            public objectOrObjectArray command { get; set; }
            public string target { get; set; }
            public CommandClass(string Command, string Target) {
               command = new objectOrObjectArray(Command);
               target = Target;
            }
            public CommandClass(string[] commands, string Target) {
               command = new objectOrObjectArray(commands);
               target = Target;
            }
         }
         public class SequenceObject : Event {
            public Filter? filters { get; set; }

         }
         public class RandomObject : Event {
            public Filter? filters { get; set; }
            public float weight { get; set; }
            public RandomObject(float Weight) {
               weight = Weight;
            }

         }

      }
      /// <summary>
      /// All sub classes are components for server entities
      /// Used to extend all components purely for type safety
      /// </summary>
      public class Component {
         //https://learn.microsoft.com/en-us/minecraft/creator/reference/content/entityreference/examples/entitycomponents/minecraftcomponent_transformation?view=minecraft-bedrock-stable
         public class Transformation : Component {
            public string into; //Format minecraft:entity<spawn_event>
            public static string? transformation_sound;
            public bool? drop_equipment;
            public bool? drop_inventory;
            public Dictionary<string, Component>? add;
            public bool? keep_level;
            public bool? keep_owner;
            public bool? preserve_equipment;
            public Delay? delay;

            public Transformation(string intoEntity, string? spawnEvent = null) {
               into = intoEntity;
               if (spawnEvent != null) {
                  into += $"<{spawnEvent}>";
               }
            }

            public class Delay {
               public float? block_assist_chance;
               public float? block_chance;
               public float? block_max;
               public float? block_radius;
               public List<string>? block_types;
               public float? range_max;
               public float? range_min;
               public float value;
               public Delay(float timeInSeconds) {
                  value = timeInSeconds;
               }
            }
         }
         /// <summary>
         /// Represnets components that just have one property called value
         /// </summary>
         /// <typeparam name="T"></typeparam>
         public class BasicValue<T> : Component {
            public T value { get; set; }
            public BasicValue(T Value) {
               value = Value;
            }
         }
         public class CustomHitTest : Component {
            public List<Hitbox> hitboxes = new List<Hitbox>();
            public CustomHitTest(List<Hitbox> hitboxes) {
               this.hitboxes = hitboxes;
            }
            public CustomHitTest(float width, float height, Vector3 pivot) {
               hitboxes = [new Hitbox(width, height, pivot)];
            }
            public class Hitbox : widthAndHeight {
               public Vector3 pivot;
               public Hitbox(float width, float height, Vector3 pivot) : base(width, height) {
                  this.pivot = pivot;
               }
            }
         }
         public class Loot : Component {
            [JsonProperty("table")]
            public string value;
            public Loot(string table) {
               this.value = table;
            }
         }
         public class JumpStatic(float? jumpPower = null) {
            public float? jump_power = jumpPower;
         }
         public class Leashable {
            public bool? can_be_stolen;
            public float? hard_distance;
            public float? max_distance;
            public EventTrigger? on_leash;
            public EventTrigger? on_unleash;
         }

         public class Pushable : Component {
            public bool? is_pushable { get; set; }
            public bool? is_pushable_by_piston { get; set; }
            public Pushable() { }
         }
         /// <summary>
         /// Usable for all types of navigation component (navigation.generic, navigation.fly, etc.)
         /// </summary>
         public class Navigation : Component {
            public bool? can_path_over_water; //Default: False
            public bool? can_path_over_lava; //Default: False
            public bool? avoid_water; //Default: False
            public bool? avoid_damage_blocks; //Default: False
            public bool? avoid_portals; //Default: false
            public bool? avoid_sun; //Default: False
            public bool? can_breach; //Default: False
            public bool? can_break_doors; //Default: False
            public bool? can_open_doors; //Default: False
            public bool? can_open_iron_doors; //Default: False
            public bool? can_walk; //Default: True
            public bool? can_swim; //Default: False
            public bool? is_amphibious; //Default: False (Determines if they can walk on ground underwater)
            public bool? can_sink; //Default:True
            public bool? can_jump; //Default: True
            public bool? can_walk_in_lava; //Default: False
            public bool? can_path_from_air; //Default: False\
            public bool? can_pass_doors; //Default: True
            public string[]? blocks_to_avoid;
            public Navigation() { }
            public static Navigation GetDefaults() {
               Navigation output = new Navigation();
               output.can_path_from_air = true;
               output.can_swim = true;
               output.can_path_over_water = true;
               output.avoid_damage_blocks = true;
               return output;
            }
         }
         public class Breathable : Component {
            public List<string>? breathe_blocks;
            public List<string>? non_breathe_blocks;
            public bool? breathes_air;
            public bool? breathes_lava;
            public bool? breathes_water;
            public bool? breathes_solids;
            public bool? generates_bubbles;
            public float? inhale_time;
            public int? suffocate_time;
            public int? total_supply;
            public Breathable() { }
         }
         public class Despawn : Component {
            public bool? despawn_from_chance; //Default:true
            public int? min_range_random_chance; //Random chance between 1 and this value Default:800

            public bool? despawn_from_inactivity; //Default:True
            public int? min_range_inactivity_timer; //Seconds mob must be inactive Default:30

            public bool? despawn_from_simulation_edge; //Default:true

            public minMaxDistance? despawn_from_distance; //Default 32;128

            public Filter? filters;
            public Despawn() { }
            public Despawn(minMaxDistance despawnFromDistance) {
               despawn_from_distance = despawnFromDistance;
               despawn_from_chance = false;
               despawn_from_inactivity = false;
               despawn_from_simulation_edge = true;
            }
            public Despawn(int inactivityTimer) {
               min_range_inactivity_timer = inactivityTimer;
               despawn_from_chance = false;
               despawn_from_inactivity = true;
               despawn_from_simulation_edge = false;
               despawn_from_distance = new minMaxDistance(128, 256); //Basically false
            }
         }
         public class Family : Component {
            public List<string> family;
            public Family(List<string> families) {
               family = families;
            }
            public Family(params string[] families) {
               family = [.. families];
            }
         }
         /// <summary>
         /// Class containing all movement. components
         /// </summary>
         public class Movement {
            public class Basic(float? maxTurn = null) {
               public float? max_turn = maxTurn;
            }
            public class Dolphin { }
            public class Fly(float? startSpeed = null, float? speedWhenTurning = null) {
               public float? start_speed = startSpeed;
               public float? speed_when_turning = speedWhenTurning;
            }
            public class Jump(float? maxTurn = null, Vector2? jumpDelay = null) {
               public float? max_turn = maxTurn;
               public Vector2? jump_delay = jumpDelay;
            }
            public class Sway(float? maxTurn = null, float? swayAmplitude = null, float? swayFrequency = null) {
               public float? max_turn = maxTurn;
               public float? sway_amplitude = swayAmplitude;
               public float? sway_frequency = swayFrequency;
            }
         }

         public class Behavior : Component {
            public int priority;
            public Behavior(int Priority) {
               priority = Priority;
            }
            public class RandomStroll : Behavior {
               public int? xz_dist;
               public int? y_dist;
               public float? speed_multiplier;
               public int? interval;
               public RandomStroll(int Priority) : base(Priority) { }
               public RandomStroll(int Priority, int XZLookDistance, int YLookDistance) : base(Priority) {
                  xz_dist = XZLookDistance;
                  y_dist = YLookDistance;
               }
            }
            public class RandomLook : Behavior {
               public int? angle_of_view_horizontal;
               public int? angle_of_view_vertical;
               public float? look_distance;
               public int[]? look_time; //Two Values Only (Range)
               public float? probability;
               public RandomLook(int Priority) : base(Priority) { }
            }

            public class RandomSwim : Behavior {
               public int? speed_multiplier;
               public int? interval;
               public int? xz_dist;
               public int? y_dist;
               public bool? avoid_surface;
               public RandomSwim(int Priority) : base(Priority) { }
            }

            public class SwimWander : Behavior {
               public float? speed_multiplier;
               public float? interval;
               public float? wander_time;
               public float? look_ahead;
               public SwimWander(int Priority) : base(Priority) { }
            }

            public class SwimIdle : Behavior {
               public float? idle_time;
               public float? sucess_rate;
               public SwimIdle(int Priority) : base(Priority) { }
            }

            public class RandomHover : Behavior {
               public float? speed_multiplier;
               public int? interval;
               public float[]? hover_height;
               public int? xz_dist;
               public int? y_dist;
               public int? y_offset;
               public RandomHover(int Priority) : base(Priority) { }
            }
            /// <summary>
            /// https://learn.microsoft.com/en-us/minecraft/creator/reference/content/entityreference/examples/entitygoals/minecraftbehavior_follow_owner?view=minecraft-bedrock-stable
            /// </summary>
            public class FollowOwner : Behavior {
               public bool? can_teleport;
               public bool? ignore_vibration;
               public float? max_distance;
               public float? speed_multiplier;
               public float? start_distance;
               public float? stop_distance;
               public FollowOwner(int Priority) : base(Priority) { }
            }
            /// <summary>
            /// https://learn.microsoft.com/en-us/minecraft/creator/reference/content/entityreference/examples/entitygoals/minecraftbehavior_nearest_attackable_target?view=minecraft-bedrock-stable
            /// </summary>
            public class NearestAttackableTarget : Behavior {
               public int? attack_interval;
               public int? attack_interval_min;
               public bool? attack_owner;
               public List<EntityType>? entity_types;
               public bool? must_reach;
               public bool? must_see;
               public float? must_see_forget_duration; //Seconds
               public float? persist_time;
               public bool? reevaluate_description;
               public int? scan_interval; //Every amount of ticks
               public bool? set_persistent;
               public float? target_invisible_multiplier;
               public float? target_search_height;
               public float? target_sneak_visibility_multiplier;
               public float? within_radius;
               public NearestAttackableTarget(int Priority, Filter targetFilter) : base(Priority) {
                  entity_types = [new EntityType(targetFilter)];
               }
               /// <summary>
               /// You shouldn't use this constructor, A filter should be specified.
               /// </summary>
               public NearestAttackableTarget(int Priority) : base(Priority) { }
               public class EntityType {
                  public Filter filters;
                  public bool? must_see;
                  public float? max_dist;
                  public float? must_see_forget_duration;
                  public float? walk_speed_multiplier;
                  public float? sprint_speed_multiplier;
                  public EntityType(Filter entityFilter) {
                     filters = entityFilter;
                  }
               }
            }
            public class MeleeAttack : Behavior {
               public bool? attack_once;
               public string? attack_types;
               public float? cooldown_time;
               public float? inner_boundary_time_increase;
               public float? max_path_time;
               public float? min_path_time;
               public float? melee_fov;
               public EventTrigger? on_attack;
               public EventTrigger? on_kill;
               public float? outer_boundary_time_increase;
               public float? path_fail_time_increase;
               public float? path_inner_boundary;
               public float? path_outer_boundary;
               public int? random_stop_interval;
               public float? reach_multiplier;
               public bool? require_complete_path;
               public bool? set_persistent;
               public float? speed_multiplier;
               public bool? track_target;
               public float? x_max_rotation;
               public float? y_mmax_head_rotation;
               public MeleeAttack(int Priority) : base(Priority) { }
            }
            /// <summary>
            /// https://learn.microsoft.com/en-us/minecraft/creator/reference/content/entityreference/examples/entitygoals/minecraftbehavior_look_at_player?view=minecraft-bedrock-stable
            /// </summary>
            public class LookAtPlayer(int Priority) : Behavior(Priority) {
               public int? angle_of_view_horizontal;
               public int? angle_of_view_vertical;
               public float? look_distance;
               public float? probability;
               /// <summary>
               /// Time range to look at the entity
               /// </summary>
               public Vector2? look_time;
            }
            /// <summary>
            /// Whether the mob can float on water.
            /// https://learn.microsoft.com/en-us/minecraft/creator/reference/content/entityreference/examples/entitygoals/minecraftbehavior_float?view=minecraft-bedrock-stable
            /// </summary>
            public class Float(int Priority) : Behavior(Priority) {
               public bool? sink_with_passengers;
            }
            /// <summary>
            /// Whether the entity can eat blocks.
            /// https://learn.microsoft.com/en-us/minecraft/creator/reference/content/entityreference/examples/entitygoals/minecraftbehavior_eat_block?view=minecraft-bedrock-stable
            /// </summary>
            public class eatBlock : Behavior {
               public List<BlockPair> eat_and_replace_block_pairs;
               public EventTrigger? on_eat;
               public Molang? sucess_chance;
               public float? time_until_eat;
               /// <param name="replacePairs">First string is block to eat, second is block to replace.</param>
               public eatBlock(int Priority, List<(string, string)> replacePairs, string? eventName = null) : base(Priority) {
                  eat_and_replace_block_pairs = replacePairs.Select(x => new BlockPair(x.Item1, x.Item2)).ToList();
                  if (eventName != null)
                     on_eat = new EventTrigger(eventName);
               }
               public struct BlockPair(string eatBlock, string replaceBlock) {
                  public string eat_block = eatBlock;
                  public string replace_block = replaceBlock;
               }
            }
         }
         public class Interact : Component {
            public List<Interaction> interactions;
            public Interact(List<Interaction> Interactions) {
               interactions = Interactions;
            }
            public class Interaction {
               public Loot? add_items;
               public float? cooldown;
               public float? cooldown_after_being_attacked;
               public string? drop_item_slot;
               public int? equip_item_slot; //Equips item after sucessful interaction
               public int? health_amount; //Heals or hurts after interact
               public int? hurt_item; //Duribility item suffers
               public string? interact_text;//Button for mobile
               public string? play_sounds;
               public string? spawn_entites;
               public Loot? spawn_items;
               public bool? swing;
               public string? transform_to_item;
               public bool? use_item;
               public string? vibration;
               public EventTrigger? on_interact;
               public Interaction(EventTrigger onInteract, string interactText) {
                  on_interact = onInteract;
                  interact_text = interactText;
               }
               public Interaction(string Event, string interactText) {
                  on_interact = new EventTrigger(Event);
                  interact_text = interactText;
               }
            }
            public static Interaction getSheepInteraction(string eventName, string lootTable = "loot_tables/entities/sheep_shear.json") {
               return new Interaction(eventName, "action.interact.shear") {
                  cooldown = 2.5f,
                  use_item = false,
                  hurt_item = 1,
                  spawn_items = new Loot(lootTable),
                  vibration = "shear",
                  on_interact = new EventTrigger(eventName) {
                     filters = new Filter() {
                        all_of = new List<Filter> {
                                    new Filter("has_equipment") { subject="other", domain="hand", value="shears" },
                                    new Filter("bool_property") { subject="self", domain="cobblemon:has_been_sheared", value=false}
                                }
                     }
                  }
               };
            }
         }

         //Unfinished
         public class Projectile : Component //https://learn.microsoft.com/en-us/minecraft/creator/reference/content/entityreference/examples/entitycomponents/minecraftcomponent_projectile?view=minecraft-bedrock-stable#definition_event-parameters
         {
            public int? anchor;
            public float? angle_offset;
            public bool? catch_fire;
            public bool? crit_particle_on_hit;
            public bool? destroy_on_hurt;
            public bool? fire_affected_by_greifing;
            public float? gravity;
            public string? hit_sound;
            public string? hit_ground_sound;
            public bool? homing;
            public float? inertia;
            public bool? is_dangerous;
            public bool? knockback;
            public bool? lightning;
            public float? liquid_inertia;
            public bool? multiple_targets;
            public Vector3? offset;
            public float? on_fire_time;
            public string? particle;
            public float? power;
            public bool? reflect_on_hurt;
            public string? shoot_sound;
            public bool? shoot_target;
            public bool? should_bounce;
            public bool? splash_potion;
            public float? splash_range;
            public bool? stop_on_hurt;
            public float? uncertainty_base;
            public float? uncertainty_multiplier;
            public OnHit? on_hit;
            public string? filter; //Specified mob is invunreble
            public Projectile() { }

            public class OnHit {
               public bool? catch_fire;
               public bool? douse_fire;
               public bool? ignite;
               public float? on_fire_time;
               public int? potion_effect;
               public bool? spawn_aoe_cloud;
               public bool? teleport_owner;
               public EventDefinition? definition_event;
               public ImpactDamage? impact_damage;

               public OnHit() { }
               public OnHit(float Damage) {
                  impact_damage = new ImpactDamage() { damage = Damage };
               }
               public OnHit(string Event, string target = "self", bool affectShooter = false, bool affectTarget = true, bool affectProjectile = false) {
                  EventTrigger evTrigger = new EventTrigger(Event, target);
                  definition_event = new EventDefinition(evTrigger);
                  definition_event.affect_shooter = affectShooter;
                  definition_event.affect_target = affectTarget;
                  definition_event.affect_projectile = affectProjectile;
               }
            }

            public class EventDefinition {
               public bool? affect_projectile;
               public bool? affect_shooter;
               public bool? affect_splash_area;
               public float? splash_area;
               public bool? affect_target;
               public EventTrigger event_trigger;
               public EventDefinition(EventTrigger event_trigger) {
                  this.event_trigger = event_trigger;
               }
            }

            public class ImpactDamage {
               public bool? catch_fire;
               public bool? channeling;
               public float? damage;
               public bool? destroy_on_hit;
               public bool? destroy_on_hit_requires_damage;
               public string? filter; //Entity that can be hit
               public bool? knockback;
               public int? max_critical_damage;
               public int? min_critical_damage;
               public float? power_multiplier;
               public bool? semi_random_diff_damage;
               public bool? set_last_hurt_requires_damage;
            }
         }
         public class Health : Component {
            public int value;
            public int max;
            public Health(int value, int max) {
               this.value = value;
               this.max = max;
            }
         }
         public class Tameable : Component {
            public float? probability;
            public EventTrigger? tame_event;
            public List<string>? tame_items;
            [JsonConstructor]
            public Tameable() { }
            public Tameable(string @event, List<string>? tame_items = null, float probability = 1) {
               this.probability = probability;
               this.tame_items = tame_items;
               this.tame_event = new EventTrigger(@event, "self");
            }
         }
         public class DamageSensor : Component {
            public static DamageSensor getImmuneToFallDamage() {
               DamageSensor dmg = new DamageSensor();
               dmg.triggers = new List<Trigger>();
               dmg.triggers.Add(new Trigger() {
                  cause = "fall",
                  deals_damage = false
               });
               return dmg;
            }
            public List<Trigger> triggers;
            public class Trigger {
               public string? cause;
               public float? damage_modifier;
               public float? damage_multiplier;
               public bool? deals_damage;
               public string? on_damage_sound_event;
               public OnDamage? on_damage;
               public Trigger() { }
            }
            public class OnDamage {
               public Filter? filters;
               [JsonProperty("event")]
               public string? Event;
               public OnDamage(string Event, Filter? filters = null) {
                  this.Event = Event;
                  this.filters = filters;
               }
            }
         }
         //https://learn.microsoft.com/en-us/minecraft/creator/reference/content/entityreference/examples/entitycomponents/minecraftcomponent_instant_despawn?view=minecraft-bedrock-stable
         public class InstantDespawn : Component {
            public bool? remove_child_entities;
         }
         //https://learn.microsoft.com/en-us/minecraft/creator/reference/content/entityreference/examples/entitycomponents/minecraftcomponent_follow_range?view=minecraft-bedrock-stable
         public class FollowRange : Component {
            public int value;
            public int? max;
            public FollowRange(int value, int? max = null) {
               this.value = value;
               this.max = max;
            }
         }
         // https://learn.microsoft.com/en-us/minecraft/creator/reference/content/entityreference/examples/definitions/nestedtables/permute_type?view=minecraft-bedrock-stable
         public class PermuteType : Component {
            public int weight;
            public string entity_type;
            public int? garunteed_count;
            public PermuteType(int weight, string entity_type, int? garunteed_count = null) {
               this.weight = weight;
               this.entity_type = entity_type;
               this.garunteed_count = garunteed_count;
            }
         }
         [JsonConverter(typeof(EnumToStringConverter<ContainerType>))]
         public enum ContainerType {
            horse,
            minecart_chest,
            chest_boat,
            minecart_hopper,
            inventory,
            container,
            hopper
         }
         /// <summary>
         /// https://learn.microsoft.com/en-us/minecraft/creator/reference/content/entityreference/examples/entitycomponents/minecraftcomponent_inventory?view=minecraft-bedrock-stable
         /// </summary>
         public class Inventory : Component {
            public int? additional_slots_per_strength;
            public bool? can_be_siphoned_from;
            public ContainerType container_type;
            public int? inventory_size;
            /// <summary>
            /// If true, the entity will not drop its inventory on death.
            /// Defaults to false on null.
            /// </summary>
            [JsonProperty("private")]
            public bool? @private;
            public bool? restrict_to_owner;

            public Inventory(ContainerType type, int size = 5) {
               this.container_type = type;
               this.inventory_size = size;
            }
         }
      }


   }

}
