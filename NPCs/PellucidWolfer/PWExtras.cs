using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GloryMod.Systems;

namespace GloryMod.NPCs.PellucidWolfer
{
    partial class WolferHead : WormHead
    {
        public override int BodyType => NPCType<WolferBody>();

        public override int TailType => NPCType<WolferTail>();

        public override void Init()
        {
            // Set the segment variance
            // If you want the segment length to be constant, set these two properties to the same value
            MinSegmentLength = 20;
            MaxSegmentLength = 20;
            CanFly = true;

            CustomBehavior = false;
            NumberBodySegments = true;

            CommonWormInit(this);
        }

        // This method is invoked from ExampleWormHead, ExampleWormBody and ExampleWormTail
        internal static void CommonWormInit(Worm worm)
        {
            worm.Acceleration = 0.075f;
            worm.MoveSpeed = 15;
        }
    }
}
