using Heroes.Graphics.Essentials.Heroes.Hooks;

namespace Heroes.Graphics.Essentials.Heroes.Patches
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
        private void HookOnResized(in ResizeEventHook.ResizeEventHookData sender)
        {
            // Set game resolution variables.
            float floatWidth  = sender.CurrentWidth;
            float floatHeight = sender.CurrentHeight;

            Variables.ResolutionX.SetValue(sender.CurrentWidth);
            Variables.ResolutionY.SetValue(sender.CurrentHeight);

            Variables.MaestroResolutionX.SetValue(ref floatWidth);
            Variables.MaestroResolutionY.SetValue(ref floatHeight);
        }
    }
}
