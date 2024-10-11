using CobbleBuild.BedrockClasses;
using static CobbleBuild.BedrockClasses.Animation;

namespace CobbleBuild {
   public static class PostProcessor {
      //Prevents z-fighting in models 
      //Configurable
      public static readonly float minInflateNumber = 0f;
      public static readonly float minThicknessSize = 0.02f;
      public static void PostProcess(ref GeometryJson geoJson) {
         foreach (Geometry geo in geoJson.geometry) {
            foreach (Geometry.Bone bone in geo.bones) {
               if (bone.cubes != null) {
                  foreach (Geometry.Cube cube in bone.cubes) {
                     if (cube.inflate != null && cube.inflate > 0 && cube.inflate < minInflateNumber) {
                        cube.inflate = minInflateNumber;
                     }
                     if (cube.size != null && cube.size.x < minThicknessSize) {
                        cube.size.x = minThicknessSize;
                     }
                     if (cube.size != null && cube.size.y < minThicknessSize) {
                        cube.size.y = minThicknessSize;
                     }
                     if (cube.size != null && cube.size.z < minThicknessSize) {
                        cube.size.z = minThicknessSize;
                     }
                  }
               }
            }
         }
      }
      public static void PostProcess(ref AnimationJson animJson) {
         foreach (var animation in animJson.animations) {
            //This newAnimation and newBone stuff should be redundant because its refrence values but im not 100% sure
            //So I do it anyways.
            var newAnimation = animation.Value;
            if (newAnimation.bones != null) {
               foreach (var bone in newAnimation.bones) {
                  var newBone = bone.Value;
                  ProcessMolangVector3(ref newBone.ScaleArray);
                  ProcessMolangVector3(ref newBone.Rotation);
                  ProcessMolangVector3(ref newBone.Position);
                  ProcessKeyframeData(ref newBone.KeyframeScale);
                  ProcessKeyframeData(ref newBone.KeyframeRotation);
                  ProcessKeyframeData(ref newBone.KeyframePosition);
                  newAnimation.bones[bone.Key] = newBone;
               }
            }
            animJson.animations[animation.Key] = newAnimation;
         }
      }
      private static void ProcessMolangVector3(ref MolangVector3? vector) {
         if (vector == null) {
            return;
         }
         if (vector.x.stringValue != null) {
            vector.x.stringValue = vector.x.stringValue.ToLower().Replace("nan", "0");
         }
         if (vector.y.stringValue != null) {
            vector.y.stringValue = vector.y.stringValue.ToLower().Replace("nan", "0");
         }
         if (vector.z.stringValue != null) {
            vector.z.stringValue = vector.z.stringValue.ToLower().Replace("nan", "0");
         }
      }
      private static void ProcessKeyframeData(ref Dictionary<string, KeyframeData>? keyframeData) {
         if (keyframeData == null) {
            return;
         }
         foreach (var keyframe in keyframeData) {
            var newKeyFrame = keyframe.Value;
            ProcessMolangVector3(ref newKeyFrame.standardArray);
            if (newKeyFrame.extraData != null) {
               ProcessMolangVector3(ref newKeyFrame.extraData.pre);
               ProcessMolangVector3(ref newKeyFrame.extraData.post);
            }
            keyframeData[keyframe.Key] = newKeyFrame;
         }
      }
   }
}
