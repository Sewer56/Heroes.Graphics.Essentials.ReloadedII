using System;
using System.Diagnostics;
using Heroes.Graphics.Essentials.Shared;
using Heroes.Graphics.Essentials.Shared.Heroes;
using Heroes.Graphics.Essentials.Shared.Math;
using Reloaded.Memory.Sources;
using Vanara.PInvoke;

namespace Heroes.Graphics.Essentials.Heroes
{
    public unsafe class ResolutionVariablePatcher
    {
        public void SubscribeToResizeEventHook(ResizeEventHook hook)
        {
            hook.Resized += HookOnResized;
        }

        /// <summary>
        /// Patches hardcoded variables unattainable without hooking.
        /// </summary>
        private void HookOnResized(ResizeEventHook sender)
        {
            // Set game resolution variables.
            float floatWidth  = sender.CurrentWidth;
            float floatHeight = sender.CurrentHeight;

            Variables.ResolutionX.SetValue(ref sender.CurrentWidth);
            Variables.ResolutionY.SetValue(ref sender.CurrentHeight);

            Variables.MaestroResolutionX.SetValue(ref floatWidth);
            Variables.MaestroResolutionY.SetValue(ref floatHeight);
        }
    }
}
