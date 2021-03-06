﻿using System.Threading.Tasks;

using Xenko.Core;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Input;
using Xenko.Physics;

namespace XenkoByteSized {
    public class DoorScript : AsyncScript {

        [DataMember(100)]
        [Display("Model")]
        public ModelComponent model;

        [DataMember(200)]
        [Display("Trigger Collider")]
        public IInlineColliderShapeDesc triggerShape;

        [DataMember(300)]
        [Display("Trigger Group")]
        public CollisionFilterGroups triggerGroup;

        [DataMember(400)]
        [Display("Physical Collider")]
        public IInlineColliderShapeDesc colliderShape;

        [DataMember(500)]
        [Display("Physical Group")]
        public CollisionFilterGroups colliderGroup;

        private StaticColliderComponent collider;
        private StaticColliderComponent trigger;

        public override async Task Execute() {

            collider = new StaticColliderComponent();
            collider.ColliderShapes.Add(colliderShape);
            collider.CollisionGroup = colliderGroup;

            trigger = new StaticColliderComponent();
            trigger.ColliderShapes.Add(triggerShape);
            trigger.CollisionGroup = triggerGroup;
            trigger.IsTrigger = true;

            Entity.Add(collider);
            Entity.Add(trigger);

            while (Game.IsRunning) {

                Collision coll = await trigger.NewCollision();
                collider.Enabled = false;
                model.Enabled = false;

                await coll.Ended();
                collider.Enabled = true;
                model.Enabled = true;

            }

        }
    }
}
