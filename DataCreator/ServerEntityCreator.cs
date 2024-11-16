using CobbleBuild.BedrockClasses;
using CobbleBuild.CobblemonClasses;
using static CobbleBuild.BedrockClasses.ServerEntity;

namespace CobbleBuild.DataCreator {
   public static class ServerEntityCreator {
      private static readonly float walkSpeedMultiplier = 0.6f;
      private static readonly float swimSpeedMultiplier = 0.6f;
      public static ServerEntityJson Create(Pokemon pokemon) {
         ServerEntity output = new ServerEntity(pokemon.identifier);
         output.description.is_spawnable = false;
         //Instantly kill any cobblemon entity
         output.component_groups.Add("cobblemon:instant_kill", new Components() {
            instantDespawn = new Component.InstantDespawn()
         });
         output.events.Add("cobblemon:instant_kill", new Event() { add = new Event.AddClass(["cobblemon:instant_kill"]) });

         //Setting all of the default components
         output.components = new Components() {
            physics = new Component(),
            pushable = new Component.Pushable(),
            movement_generic = new Component.Movement.Basic(),
            jumpStatic = new Component.JumpStatic(),
            movement = new Component.BasicValue<float>(pokemon.data.behaviour.moving.walk.walkSpeed * walkSpeedMultiplier),
            family = new Component.Family("cobblemon", "pokemon"),
            despawn = new Component.Despawn(400) { despawn_from_simulation_edge = true },
            behavior_nearestAttackableTarget = new Component.Behavior.NearestAttackableTarget(1, new Filter() {
               all_of = [
                  //new Filter() { test = "bool_property", subject="self", domain = "cobblemon:in_battle"},
                  new Filter() { test = "has_tag", domain = "targetedBy:" + pokemon.identifier, subject = "other"}
               ]
            }),
            followRange = new Component.FollowRange(4, 10),
            //behavior_meleeeAttack = new Component.Behavior.MeleeAttack(3) {
            //   speed_multiplier = 2f,
            //   track_target = true,
            //   require_complete_path = true,
            //   reach_multiplier = 0.5f
            //},
            leashable = new Component.Leashable(),
            interact = new Component.Interact([new Component.Interact.Interaction("cobblemon:interacted", "Interact")]),
            tameable = new Component.Tameable("minecraft:on_tame"),
            inventory = new Component.Inventory(Component.ContainerType.inventory, 1) { restrict_to_owner = true },
         };
         output.components.collisionBox = new widthAndHeight(pokemon.data.hitbox.width, pokemon.data.hitbox.height);
         output.components.scale = new Component.BasicValue<float>(pokemon.data.baseScale);

         //Adds Properties
         output.description.properties = new Dictionary<string, Property>()
         {
                {"cobblemon:in_battle", Property.makeBool(false, true) },
                {"cobblemon:initialized", Property.makeBool(false, true) },
                {"cobblemon:spawn_condition_used", Property.makeEnum(["none", "spawn_0"], "none", false) }, // -1 indicates not spawned using spawn file
                {"cobblemon:wild", Property.makeBool(true, true) },
                {"cobblemon:busy", Property.makeBool(false, true) }
            };

         // Setup for different variations
         if (pokemon.spawnData != null && pokemon.spawnData.spawns.Length > 0) {
            output.description.properties["cobblemon:spawn_condition_used"] = GetSpawnConditionEnum(pokemon.spawnData.spawns);
            for (int i = 0; i < pokemon.spawnData.spawns.Length; i++) {
               var spawn = pokemon.spawnData.spawns[i];
               var @event = new Event() { sequence = new List<Event.SequenceObject>() };
               var setPropertyEvent = new Event.SequenceObject() {
                  set_property = new Dictionary<string, object> {
                            {"cobblemon:spawn_condition_used", "spawn_" + i.ToString() }
                        }
               };
               @event.sequence.Add(setPropertyEvent);
               var randomizeEvent = new Event.SequenceObject() { randomize = new List<Event.RandomObject>() };
               //@event.trigger = "minecraft:entity_spawned";
               var variationAspect = spawn.pokemon.Split(" ").Select(x => x.Replace("=", "-").Replace("_", "-")).ToArray();
               var validVariations = new List<Variation>();
               if (variationAspect.Length > 1) {
                  validVariations = pokemon.Variations
                      .Where(x => x.aspects.Contains(variationAspect[1]))
                      .ToList();
               }
               else {
                  validVariations = pokemon.Variations
                      //If the variation is a base variation
                      .Where(x => x.aspects.RemoveIfInList("male", "female", "shiny").Count() == 0)
                      .ToList();
               }
               //Appearently there are spawn conditions for variants that dont exist yet
               if (validVariations.Count > 0) {
                  foreach (var variation in validVariations) {
                     var weight = Variation.calcWeight(variation.aspects, pokemon);
                     var index = pokemon.Variations.FindIndex(x => x == variation);
                     if (index == -1)
                        continue;
                     randomizeEvent.randomize.Add(new Event.RandomObject(weight) { trigger = $"cobblemon:spawn_as_variant_{index}" });
                  }
               }
               else {
                  Misc.warn($"Variation {variationAspect[1]} was not found in pokemon {pokemon.shortName}.");
                  //Disable broken spawn events
                  randomizeEvent.randomize.Add(new Event.RandomObject(100f) { trigger = "cobblemon:instant_kill" });
               }

               @event.sequence.Add(randomizeEvent);
               output.events.Add("cobblemon:spawn_condition_" + i, @event);

               if (spawn.weightMultiplier != null && spawn.weightMultiplier.multiplier != null) {
                  var multipliedEvent = @event.Clone();
                  //Should line up with the property setter on the original
                  multipliedEvent.sequence![0] = new Event.SequenceObject() {
                     set_property = new Dictionary<string, object> {
                        { "cobblemon:spawn_condition_used", "spawn_" + i.ToString() + "m" }
                     }
                  };
                  output.events.Add("cobblemon:spawn_condition_" + i.ToString() + "m", multipliedEvent);
               }
            }
         }

         var moveData = pokemon.data.behaviour.moving;
         output.components.navigation_generic = new Component.Navigation() {
            avoid_damage_blocks = true,
            avoid_water = moveData.swim.avoidsWater,
            can_path_from_air = true,
            can_walk = true, //Even pokemon like abra that !canWalk still basically walk around but just dont animate it.
            can_jump = true,
            can_pass_doors = true,
            can_walk_in_lava = moveData.swim.canWalkOnLava || moveData.swim.canSwimInLava,
            can_path_over_water = !moveData.swim.avoidsWater,
            can_path_over_lava = !moveData.swim.hurtByLava,
            can_swim = moveData.swim.canSwimInWater || moveData.swim.canSwimInLava
         };
         output.components.breathable = new Component.Breathable() {
            suffocate_time = 0,
            breathes_air = true,
            breathes_lava = moveData.swim.canBreathUnderlava,
            breathes_water = moveData.swim.canBreatheUnderwater,
            generates_bubbles = !moveData.swim.canBreatheUnderwater,
            inhale_time = 2
         };

         if (moveData.canLook) {
            output.components.behavior_randomLook = new Component.Behavior.RandomLook(4);
            output.components.behavior_lookAtPlayer = new Component.Behavior.LookAtPlayer(6);
         }

         if (!moveData.walk.avoidsLand) {
            output.components.behavior_randomStroll = new Component.Behavior.RandomStroll(3);
         }

         if (moveData.swim.canSwimInWater || moveData.swim.canSwimInLava) {
            output.components.underwaterMovement = new Component.BasicValue<float>(moveData.swim.swimSpeed * swimSpeedMultiplier);
            //If actually a fish
            if (moveData.walk.avoidsLand) {
               output.components.behavior_swimWander = new Component.Behavior.SwimWander(7);
               output.components.behavior_swimIdle = new Component.Behavior.SwimIdle(8);
               output.components.behavior_randomSwim = new Component.Behavior.RandomSwim(5);
            }
            else {
               output.components.behavior_float = new Component.Behavior.Float(9);
            }
         }
         //Come back to later
         if (moveData.fly.canFly) {
            output.components.canFly = new Component();
         }

         if (!moveData.swim.hurtByLava) {
            output.components.damageSensor ??= new Component.DamageSensor();
            output.components.damageSensor.triggers.Add(new Component.DamageSensor.Trigger() {
               cause = "lava",
               deals_damage = false
            });
         }

         //Adds Loot Componenet
         if (pokemon.hasLootTable) {
            output.components.loot = new Component.Loot("loot_tables/cobblemon/" + pokemon.shortName + ".json");
         }

         var notInitializedFilter = new Filter() { test = "bool_property", domain = "cobblemon:initialized", @operator = "equals", subject = "self", value = false };

         //Sets up spawning events and odds for different variants (including shinies)
         List<Variation> variationList = pokemon.Variations;
         Event spawnedEvent = new Event();
         spawnedEvent.sequence = new List<Event.SequenceObject>();
         Event.SequenceObject variantSequence = new Event.SequenceObject();
         variantSequence.randomize = new List<Event.RandomObject>();
         variantSequence.filters = notInitializedFilter;
         for (int i = 0; i < variationList.Count; i++) {
            output.component_groups.Add($"cobblemon:variant_{i}", new Components() { variant = new Component.BasicValue<int>(i) });
            //Come back to weight later
            Event.RandomObject randObj = new Event.RandomObject(Variation.calcWeight(variationList[i].aspects, pokemon, 4096));
            randObj.add = new Event.AddClass(new string[] { $"cobblemon:variant_{i}" });
            variantSequence.randomize.Add(randObj);

            //Add event to let code set variant
            Event setVariant = new Event() {
               add = new Event.AddClass([$"cobblemon:variant_{i}"])
            };
            output.events.Add($"cobblemon:set_variant_{i}", setVariant);

            //Add specific spawn event for each variant
            Event spawnAsVariant = new Event() {
               sequence = [
                    new Event.SequenceObject() {add = new Event.AddClass([$"cobblemon:variant_{i}"])},
                        new Event.SequenceObject() {trigger = "cobblemon:callInitFromServer", filters=notInitializedFilter}
                ]
            };
            output.events.Add($"cobblemon:spawn_as_variant_{i}", spawnAsVariant);

         }
         spawnedEvent.sequence.Add(variantSequence);

         //Tells script to set up cobblemon
         Event setupEvent = new Event();
         setupEvent.queue_command = new Event.CommandClass("scriptevent cobblemon:setup", "self");
         //spawnedEvent.sequence.Add(setupSequenceObject);
         output.events.Add("cobblemon:callInitFromServer", setupEvent);
         spawnedEvent.sequence.Add(new Event.SequenceObject() {
            trigger = "cobblemon:callInitFromServer",
            filters = notInitializedFilter
         });

         output.events.Add("minecraft:entity_spawned", spawnedEvent);

         //Intract Event Added
         Event interactedEvent = new Event();
         interactedEvent.sequence = new List<Event.SequenceObject>();
         //Tags Attacker
         Event.SequenceObject tagInteracter = new Event.SequenceObject();
         tagInteracter.queue_command = new Event.CommandClass("tag @s add interacter", "other");
         interactedEvent.sequence.Add(tagInteracter);
         //Invokes Server Script
         Event.SequenceObject invokeServerScriptInteract = new Event.SequenceObject();
         invokeServerScriptInteract.queue_command = new Event.CommandClass("scriptevent cobblemon:interacted", "self");
         interactedEvent.sequence.Add(invokeServerScriptInteract);
         output.events.Add("cobblemon:interacted", interactedEvent);

         //Tamed Component Group and Event
         output.component_groups["cobblemon:tamed"] = new Components() {
            behavior_followOwner = new Component.Behavior.FollowOwner(0) { max_distance = 14, start_distance = 11, stop_distance = 3, speed_multiplier = 1.4f },
            isTamed = new Component()
         };
         output.events["minecraft:on_tame"] = new Event() {
            add = new Event.AddClass(["cobblemon:tamed"])
         };

         //Rare candy implimentation
         output.events["cobblemon:use_experience_candy"] = new Event() { queue_command = new Event.CommandClass("scriptevent cobblemon:gain_level", "self") };
         output.components.interact.interactions.Insert(0, new Component.Interact.Interaction(
             new EventTrigger("cobblemon:use_experience_candy") {
                filters = new Filter("has_equipment") {
                   subject = "other",
                   domain = "hand",
                   value = "cobblemon:rare_candy"
                }
             },
             "cobblemon.ui.interact.exp_candy"
             ) { use_item = true });

         //Feature implimentations
         if (pokemon.data.features.Contains("sheared")) {
            output.description.properties.Add("cobblemon:has_been_sheared", Property.makeBool(false, true));
            output.events.Add("cobblemon:eat_grass", new Event() {
               set_property = new Dictionary<string, object>() { { "cobblemon:has_been_sheared", false } },
               queue_command = new Event.CommandClass("scriptevent cobblemon:update_self")
            });
            output.events.Add("cobblemon:shear", new Event() {
               set_property = new Dictionary<string, object>() { { "cobblemon:has_been_sheared", true } },
               queue_command = new Event.CommandClass("scriptevent cobblemon:update_self")
            });
            output.components.behavior_eatBlock = new Component.Behavior.eatBlock(2, [("grass_block", "dirt"), ("grass", "dirt"), ("tallgrass", "air")], "cobblemon:eat_grass");
            output.components.interact.interactions.Insert(0, Component.Interact.getSheepInteraction("cobblemon:shear"));
         }

         //Item Evolution Interactions
         foreach (var evolution in pokemon.data.evolutions) {
            if (evolution.variant == "item_interact" && evolution.requiredContext != null && evolution.requiredContext.GetType() == typeof(string)) {
               var evolveID = "cobblemon:evolution_" + evolution.id;
               output.events.Add(evolveID, new Event() { queue_command = new Event.CommandClass($"scriptevent cobblemon:interact_evolution {evolution.id}", "self") });
               output.components.interact.interactions.Insert(0, new Component.Interact.Interaction(new EventTrigger(evolveID, "self", new Filter("has_equipment") {
                  subject = "other",
                  value = evolution.requiredContext,
                  domain = "hand"
               }), "cobblemon.ui.evolve"));
            }
         }

         return new ServerEntityJson(output);
      }

      private static ServerEntity.Property GetSpawnConditionEnum(CobblemonSpawn[] conditions) {
         List<string> enums = [];
         for (int i = 0; i < conditions.Length; i++) {
            enums.Add("spawn_" + i.ToString());
            if (conditions[i].weightMultiplier != null && conditions[i].weightMultiplier!.multiplier != null) {
               enums.Add("spawn_" + i.ToString() + 'm');
            }
         }
         if (enums.Count > 15) {
            Misc.warn($"Pokemon ${conditions[0].pokemon} has too many spawn conditions.");
            enums = enums.Slice(0, 15);
         }
         return Property.makeEnum(["none", .. enums], "none", false);
      }

      //Create Pokeball
      public static ServerEntityJson Create(PokeballResourceData pokeball) {
         ServerEntity output = new ServerEntity(pokeball.pokeball);
         output.description = new Description(pokeball.pokeball) {
            is_spawnable = false,
            properties = new Dictionary<string, Property>()
             {
                    {"cobblemon:can_catch_pokemon", Property.makeBool(true, true) },
                    {"cobblemon:disabled", Property.makeBool(false, true)}
                }
         };
         output.events = new Dictionary<string, Event>()
         {
                {"cobblemon:thrown", new Event() { queue_command = new Event.CommandClass("scriptevent cobblemon:pokeball_thrown", "self") } },
                //{"cobblemon:catching", new Event() { sequence = [new Event.SequenceObject() {
                //    filters = new Filter() { test = "bool_property", domain="cobblemon:can_catch_pokemon"},
                //    sequence = [
                //        new Event.SequenceObject() {set_property = new Dictionary<string, object>() {{"cobblemon:can_catch_pokemon", false}}},
                //        new Event.SequenceObject() {queue_command = new Event.CommandClass("scriptevent cobblemon:pokeball_catching", "self")}
                //        ]
                //}] } },
                {"minecraft:entity_spawned", new Event() { trigger = "cobblemon:thrown" } }
            };

         //Instantly kill any cobblemon entity
         output.component_groups.Add("cobblemon:instant_kill", new Components() {
            instantDespawn = new Component.InstantDespawn()
         });
         output.events.Add("cobblemon:instant_kill", new Event() { add = new Event.AddClass(["cobblemon:instant_kill"]) });

         output.component_groups.Add("cobblemon:enabled", new Components() {
            physics = new Component(),
            pushable = new Component.Pushable()
         });
         output.component_groups.Add("cobblemon:disabled", new Components() {
            customHitTest = new Component.CustomHitTest(0, 0, new Vector3(0, 0, 0))
         });
         output.events.Add("cobblemon:disable", new Event() {
            sequence = new List<Event.SequenceObject>() {
                new Event.SequenceObject() { remove = new Event.RemoveClass(["cobblemon:enabled"]) },
                new Event.SequenceObject() { add = new Event.AddClass(["cobblemon:disabled"]) },
                new Event.SequenceObject() { set_property = new Dictionary<string, object>() {{"cobblemon:disabled", true}}}
            }
         });


         output.components = new Components() {
            physics = new Component(),
            pushable = new Component.Pushable(),
            collisionBox = new widthAndHeight(0.25f, 0.25f),
            health = new Component.Health(1, 1),
            damageSensor = Component.DamageSensor.getImmuneToFallDamage(),
            family = new Component.Family("cobblemon", "pokeball")
         };
         Component.Projectile projectile = new Component.Projectile() {
            stop_on_hurt = true,
            //on_hit = new Component.Projectile.OnHit("cobblemon:catching", "self", true), //{ impact_damage = new Component.Projectile.ImpactDamage() },
            anchor = 1,
            gravity = 0.1f,
            power = 1.5f,
            //Dive ball is faster
            liquid_inertia = (pokeball.pokeball == "cobblemon:dive_ball") ? 0.99f : 0.6f
         };
         output.components.projectile = projectile;
         return new ServerEntityJson(output);
      }
      /// <summary>
      /// Creates a smaller pokeball used for the healing machine
      /// </summary>
      public static ServerEntityJson CreatePokeballDummy(PokeballResourceData pokeball) {
         ServerEntity output = new ServerEntity(pokeball.pokeball + "_dummy");
         output.description.is_summonable = true;
         output.description.is_spawnable = false;
         output.components = new Components() {
            collisionBox = new widthAndHeight(0.01f, 0.01f),
            scale = new Component.BasicValue<float>(0.4f),
            family = new Component.Family("cobblemon", "pokeball_dummy")
         };
         //Instantly kill any cobblemon entity
         output.component_groups.Add("cobblemon:instant_kill", new Components() {
            instantDespawn = new Component.InstantDespawn()
         });
         output.events.Add("cobblemon:instant_kill", new Event() { add = new Event.AddClass(["cobblemon:instant_kill"]) });
         return new ServerEntityJson(output);
      }
   }
}
