using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using System;
using System.Xml.Linq;

namespace Barotrauma.Items.Components
{
    partial class PowerContainer : Powered, IDrawableComponent, IServerSerializable, IClientSerializable
    {
        private float lastLogUserCooldown;
        private string lastLogUser;

        public void ServerEventRead(IReadMessage msg, Client c)
        {
            bool newUseStep = msg.ReadBoolean();
            float newRechargeSpeed = msg.ReadRangedSingle(0.0f, 100.0f, 8) / 100.0f * maxRechargeSpeed;

            if (item.CanClientAccess(c))
            {
                string currentUser = GameServer.CharacterLogName(c.Character);
                UseStep = newUseStep;
                RechargeSpeed = newRechargeSpeed;

                if (lastLogUser != currentUser)
                {
                    lastLogUserCooldown = 0f;
                }
                if (lastLogUserCooldown - (float)Timing.TotalTime <= 0f)
                {
                    lastLogUser = currentUser;
                    GameServer.Log(lastLogUser + " set the recharge speed of " + item.Name + " to " + (int)((rechargeSpeed / maxRechargeSpeed) * 100.0f) + " %", ServerLog.MessageType.ItemInteraction);
                    lastLogUserCooldown = (float)Timing.TotalTime + 1.0f;
                }
            }

            item.CreateServerEvent(this);
        }

        public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
        {
            msg.Write(UseStep);
            msg.WriteRangedSingle(rechargeSpeed / MaxRechargeSpeed * 100.0f, 0.0f, 100.0f, 8);

            float chargeRatio = MathHelper.Clamp(charge / capacity, 0.0f, 1.0f);
            msg.WriteRangedSingle(chargeRatio, 0.0f, 1.0f, 8);
        }
    }
}
