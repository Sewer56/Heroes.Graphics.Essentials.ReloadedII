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
        private const float DefaultMissionDescriptionX      = 0.6499670148F;
        private const float DefaultMissionDescriptionY      = 0.8666300178F;
        private const float DefaultMissionDescriptionWidth  = 0.400000006F;
        private const float DefaultMissionDescriptionHeight = 0.06666664779F;

        private const float DefaultResultScreenDotsPercentageX = 0.45F;
        private const float DefaultResultScreenDotsPercentageY = 0.2F;
        private const float DefaultResultScreenDotsHorizontalSeparation = 0.03400000185F;
        private const float DefaultResultScreenDotsVerticalSeparation = 0.06700000167F;
        private const float DefaultResultScreenDotsHeight = 0.01669999957F;
        private const float DefaultResultScreenDotsWidth  = 0.01250000019F;

        // Ours
        private AspectConverter _aspectConverter;

        public ResolutionVariablePatcher(float aspectRatioLimit)
        {
            _aspectConverter = new AspectConverter(aspectRatioLimit);
        }

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
            float relativeAspectRatio = sender.RelativeAspectRatio;
            float actualAspectRatio   = sender.ActualAspectRatio;

            Variables.ResolutionX.SetValue(ref sender.CurrentWidth);
            Variables.ResolutionY.SetValue(ref sender.CurrentHeight);

            Variables.MaestroResolutionX.SetValue(ref floatWidth);
            Variables.MaestroResolutionY.SetValue(ref floatHeight);

            // Pickup boxes.
            float* pickupBoxSize = (float*)0x78A240;
            float* pickupBoxSeparation = (float*)0x78A618;
            Memory.CurrentProcess.SafeWrite((IntPtr)pickupBoxSize, _aspectConverter.ScaleByRelativeAspectX(0.03750000149F, relativeAspectRatio, actualAspectRatio));
            Memory.CurrentProcess.SafeWrite((IntPtr)pickupBoxSeparation, _aspectConverter.ScaleByRelativeAspectX(52F, relativeAspectRatio, actualAspectRatio));

            // Stage description.
            float* descriptionX = (float*)0x78A4EC;
            float* descriptionY = (float*)0x78A4E8;
            float* descriptionWidth = (float*)0x745F3C;
            float* descriptionHeight = (float*)0x78A4E4;

            Memory.CurrentProcess.SafeWrite((IntPtr)descriptionWidth, _aspectConverter.ScaleByRelativeAspectX(DefaultMissionDescriptionWidth, relativeAspectRatio, actualAspectRatio));
            Memory.CurrentProcess.SafeWrite((IntPtr)descriptionHeight, _aspectConverter.ScaleByRelativeAspectY(DefaultMissionDescriptionHeight, relativeAspectRatio, actualAspectRatio));

            // => This item is not adjusted horizontally/vertically because the function that
            // draws it cannot be made to set _shiftOrthographicProjection.Enqueue(true)
            // Reason it cannot is because Last story for some reason calls the function in the BG.
            float gameScaleX = DefaultMissionDescriptionX * AspectConverter.GameCanvasWidth;
            float gameScaleY = DefaultMissionDescriptionY * AspectConverter.GameCanvasHeight;
            gameScaleX = _aspectConverter.ProjectFromOldToNewCanvasX(gameScaleX, relativeAspectRatio, actualAspectRatio) / AspectConverter.GameCanvasWidth;
            gameScaleY = _aspectConverter.ProjectFromOldToNewCanvasY(gameScaleY, relativeAspectRatio, actualAspectRatio) / AspectConverter.GameCanvasHeight;

            Memory.CurrentProcess.SafeWrite((IntPtr)descriptionX, gameScaleX);
            Memory.CurrentProcess.SafeWrite((IntPtr)descriptionY, gameScaleY);

            // Result screen
            float* dotsPercentageX = (float*)0x745BA8;
            float* dotsPercentageY  = (float*)0x745F40;
            float* dotsVertSeparation = (float*)0x78A504;
            float* dotsHorzSeparation = (float*)0x78A248;
            float* dotsHeight = (float*)0x78A508;
            float* dotsWidth = (float*)0x78A31C;

            float percentOffsetXFromCenter  = 0.5F - DefaultResultScreenDotsPercentageX;
            float newPercentageX            = 0.5F - _aspectConverter.ScaleByRelativeAspectX(percentOffsetXFromCenter, relativeAspectRatio, actualAspectRatio);

            float percentOffsetYFromCenter  = 0.5F - DefaultResultScreenDotsPercentageY;
            float newPercentageY            = 0.5F - _aspectConverter.ScaleByRelativeAspectY(percentOffsetYFromCenter, relativeAspectRatio, actualAspectRatio);

            Memory.CurrentProcess.SafeWrite((IntPtr)dotsPercentageX, newPercentageX);
            Memory.CurrentProcess.SafeWrite((IntPtr)dotsPercentageY, newPercentageY);
            Memory.CurrentProcess.SafeWrite((IntPtr)dotsVertSeparation, _aspectConverter.ScaleByRelativeAspectY(DefaultResultScreenDotsVerticalSeparation, relativeAspectRatio, actualAspectRatio));
            Memory.CurrentProcess.SafeWrite((IntPtr)dotsHorzSeparation, _aspectConverter.ScaleByRelativeAspectX(DefaultResultScreenDotsHorizontalSeparation, relativeAspectRatio, actualAspectRatio));
            Memory.CurrentProcess.SafeWrite((IntPtr)dotsHeight, _aspectConverter.ScaleByRelativeAspectY(DefaultResultScreenDotsHeight, relativeAspectRatio, actualAspectRatio));
            Memory.CurrentProcess.SafeWrite((IntPtr)dotsWidth, _aspectConverter.ScaleByRelativeAspectX(DefaultResultScreenDotsWidth, relativeAspectRatio, actualAspectRatio));

        }
    }
}
