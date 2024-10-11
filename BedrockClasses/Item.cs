using Newtonsoft.Json;

namespace CobbleBuild.BedrockClasses {
   public class ItemJson : BedrockJson {
      [JsonProperty("minecraft:item")]
      public ItemData item { get; set; }
      public ItemJson(ItemData item) : base("1.20.50") {
         this.item = item;
      }
   }
   public class ItemData {
      public Description description;
      public Dictionary<string, Component>? components;
      public class Description {
         public string identifier { get; set; }
         public string? category;
         public Description(string Identfier) {
            identifier = Identfier;
         }
      }
      public ItemData(string Identfier) {
         description = new Description(Identfier);
      }
      public ItemData(string Identifier, Dictionary<string, Component>? Components) {
         description = new Description(Identifier);
         components = Components;
      }

      public class Component {
         //Value for any objects who have just 1 value
         public class BasicValue : Component {
            public object value { get; set; }
            public BasicValue(object Value) {
               value = Value;
            }
         }

         public class Texuture : Component {
            public string texture;
            public Texuture(string texture) {
               this.texture = texture;
            }
         }
         public class DisplayName : BasicValue {
            public DisplayName(string name) : base(name) { }
         }
         public class Throwable : Component {
            public bool? do_swing_animation;
            public float? launch_power_scale;
            public float? max_draw_duration;
            public float? min_draw_duration;
            public float? max_launch_power;
            public bool? scale_power_by_draw_duration;
            public Throwable() { }
         }
         public class Projectile : Component {
            public float? minimum_critical_power;
            public string? projectile_entity;
            public Projectile(string ProjectileEntity, float? minimumCriticalPower = null) {
               projectile_entity = ProjectileEntity;
               minimum_critical_power = minimumCriticalPower;
            }
         }
         public class Shooter : Component {
            public bool? chrage_on_draw;
            public float? max_draw_duration;
            public bool? scale_power_by_draw_duration;
            public List<Ammunition> ammunition = new List<Ammunition>();
            public Shooter(string Ammunition) {
               ammunition.Add(new Ammunition(Ammunition));
            }

            public class Ammunition {
               public string item;
               public bool? use_offhand;
               public bool? search_inventory;
               public bool? use_in_creative;
               public Ammunition(string item, bool searchInventory = true, bool useInCreative = true, bool useOffhand = false) {
                  this.item = item;
                  this.use_in_creative = useInCreative;
                  this.search_inventory = searchInventory;
                  this.use_offhand = useOffhand;
               }
            }
         }
         public class UseModifiers : Component //Brand New, Literally Undocumented
         {
            public float? use_duration;
            public float? movement_modifier;
            public UseModifiers(float useDuration = 0.1f, float movementModifier = 1) {
               use_duration = useDuration;
               movement_modifier = movementModifier;
            }
         }
         public class BlockPlacer : Component {
            public string block;
            public List<string> use_on;
            public BlockPlacer(string block, List<string> use_on) {
               this.block = block;
               this.use_on = use_on;
            }
         }
         public class Tags : Component {
            public string[] tags;
            public Tags(params string[] tags) {
               this.tags = tags;
            }
         }
      }
   }
}
