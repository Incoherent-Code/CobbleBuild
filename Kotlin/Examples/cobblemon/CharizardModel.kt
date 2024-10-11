/*
 * Copyright (C) 2023 Cobblemon Contributors
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

package com.cobblemon.mod.common.client.render.models.blockbench.pokemon.gen1

import com.cobblemon.mod.common.client.render.models.blockbench.PoseableEntityState
import com.cobblemon.mod.common.client.render.models.blockbench.asTransformed
import com.cobblemon.mod.common.client.render.models.blockbench.frame.BiWingedFrame
import com.cobblemon.mod.common.client.render.models.blockbench.frame.BimanualFrame
import com.cobblemon.mod.common.client.render.models.blockbench.frame.BipedFrame
import com.cobblemon.mod.common.client.render.models.blockbench.frame.HeadedFrame
import com.cobblemon.mod.common.client.render.models.blockbench.pokemon.CryProvider
import com.cobblemon.mod.common.client.render.models.blockbench.pokemon.PokemonPose
import com.cobblemon.mod.common.client.render.models.blockbench.pokemon.PokemonPoseableModel
import com.cobblemon.mod.common.client.render.models.blockbench.pose.TransformedModelPart.Companion.Y_AXIS
import com.cobblemon.mod.common.entity.PoseType
import com.cobblemon.mod.common.entity.PoseType.Companion.MOVING_POSES
import com.cobblemon.mod.common.entity.PoseType.Companion.STATIONARY_POSES
import com.cobblemon.mod.common.entity.PoseType.Companion.UI_POSES
import com.cobblemon.mod.common.entity.pokemon.PokemonEntity
import net.minecraft.client.model.ModelPart
import net.minecraft.util.math.Vec3d
class CharizardModel(root: ModelPart) : PokemonPoseableModel(), HeadedFrame, BipedFrame, BimanualFrame, BiWingedFrame {
    override val rootPart = root.registerChildWithAllChildren("charizard")
    override val head = getPart("head_ai")
    override val rightArm = getPart("arm_right")
    override val leftArm = getPart("arm_left")
    override val rightLeg = getPart("leg_right")
    override val leftLeg = getPart("leg_left")
    override val leftWing = getPart("wing_left")
    override val rightWing = getPart("wing_right")

    override val portraitScale = 1.9F
    override val portraitTranslation = Vec3d(-0.5, 1.4, 0.0)

    override val profileScale = 0.55F
    override val profileTranslation = Vec3d(0.05, 0.93, 0.0)

    lateinit var sleep: PokemonPose
    lateinit var standing: PokemonPose
    lateinit var walk: PokemonPose
    lateinit var flyIdle: PokemonPose
    lateinit var fly: PokemonPose

    override val cryAnimation = CryProvider { _, _ -> bedrockStateful("charizard", "cry").setPreventsIdle(false) }

    override fun registerPoses() {
//        val blink = quirk("blink") { bedrockStateful("charizard", "blink").setPreventsIdle(false)}
        sleep = registerPose(
            poseType = PoseType.SLEEP,
            idleAnimations = arrayOf(bedrock("charizard", "sleep"))
        )

        standing = registerPose(
            poseName = "standing",
            poseTypes = STATIONARY_POSES - PoseType.HOVER + UI_POSES,
//            quirks = arrayOf(blink),
            idleAnimations = arrayOf(
                singleBoneLook(),
                bedrock("charizard", "ground_idle")
            )
        )

        walk = registerPose(
            poseName = "walk",
            poseTypes = MOVING_POSES - PoseType.FLY,
//            quirks = arrayOf(blink),
            idleAnimations = arrayOf(
                singleBoneLook(),
                bedrock("charizard", "ground_idle"),
                bedrock("charizard", "ground_walk")
            )
        )

        flyIdle = registerPose(
            poseName = "hover",
            poseType = PoseType.HOVER,
//            quirks = arrayOf(blink),
            idleAnimations = arrayOf(
                singleBoneLook(),
                bedrock("charizard", "air_idle")
            ),
            transformedParts = arrayOf(rootPart.asTransformed().addPosition(Y_AXIS, -2F))
        )

        fly = registerPose(
            poseName = "fly",
            poseType = PoseType.FLY,
//            quirks = arrayOf(blink),
            idleAnimations = arrayOf(
                singleBoneLook(),
                bedrock("charizard", "air_fly")
            ),
            transformedParts = arrayOf(rootPart.asTransformed().addPosition(Y_AXIS, 6F))
        )
    }

    override fun getFaintAnimation(
        pokemonEntity: PokemonEntity,
        state: PoseableEntityState<PokemonEntity>
    ) = if (state.isPosedIn(standing, walk, sleep)) bedrockStateful("charizard", "faint") else null
}