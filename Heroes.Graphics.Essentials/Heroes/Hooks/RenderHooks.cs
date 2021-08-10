using System;
using System.Collections.Generic;
using Heroes.SDK.API;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Sources;
using Heroes.Graphics.Essentials.Math;
using Heroes.SDK.Classes;
using Heroes.SDK.Definitions.Structures.Media.Video;
using Heroes.SDK.Definitions.Structures.RenderWare.Arbitrary;
using static Heroes.SDK.Classes.PseudoNativeClasses.RenderWareFunctions;
using static Heroes.SDK.Classes.Uncategorized;
using static Reloaded.Memory.Kernel32.Kernel32;

namespace Heroes.Graphics.Essentials.Heroes.Hooks
{
    public unsafe class RenderHooks
    {
        public AspectConverter AspectConverter { get; private set; }

        /* Sizes Received From Events */
        private float ActualAspectRatio { get; set; }
        private float RelativeAspectRatio { get; set; }
        private int CurrentHeight { get; set; }
        private int CurrentWidth { get; set; }

        // Game's
        // Note: Float values are shared between multiple pieces of game code, must only modify them during when relevant code is executed.
        const float DefaultPickupBoxSeparation = 52.0F;
        const float DefaultMissionDescriptionX = 0.6499670148F;
        const float DefaultMissionDescriptionY = 0.8666300178F;
        const float DefaultMissionDescriptionWidth = 0.400000006F;
        const float DefaultMissionDescriptionHeight = 0.06666664779F;
        const float DefaultResultScreenDotsHorizontalSeparation = 0.03400000185F;
        const float DefaultResultScreenDotsVerticalSeparation = 0.06700000167F;
        const float DefaultResultScreenDotsHeight = 0.01669999957F;
        const float DefaultResultScreenDotsWidth = 0.01250000019F;

        private readonly float* _descriptionX = (float*)0x78A4EC;
        private readonly float* _descriptionY = (float*)0x78A4E8;
        private readonly float* _descriptionWidth = (float*)0x745F3C;
        private readonly float* _descriptionHeight = (float*)0x78A4E4;
        private readonly float* _pickupBoxSeparation = (float*)0x78A618;
        private readonly float* _dotsVertSeparation = (float*)0x78A504;
        private readonly float* _dotsHorzSeparation = (float*)0x78A248;
        private readonly float* _dotsHeight = (float*)0x78A508;
        private readonly float* _dotsWidth = (float*)0x78A31C;

        // Replace constants used in calculating titlecard text position.

        /* Ours */
        private Memory _memory;
        private IHook<DrawViewPorts> _draw2PViewPortHook;
        private IHook<DrawSpecialStageGauge> _drawSpecialStageGaugeHook;
        private IHook<DrawSpecialStageBar> _drawSpecialStageBarHook;
        private IHook<DrawTwoPlayerStatusBar> _draw2PStatusHook;
        private IHook<Native_rwD3D8Im2DRenderPrimitive> _renderPrimitiveHook;
        private IHook<RenderVideoFrame> _renderVideoHook;
        private IAsmHook _drawSpecialStageEmeraldIndicatorHook;
        private IHook<DrawFullVideoFrame> _drawFullVideoFrameHook;
        private IHook<DrawSmallFrame> _drawSmallVideoFrameHook;
        private IHook<DrawTitlecardElements> _drawTitlecardElementsHook;
        private IHook<Calls_DrawSpecialStageLinkText> _drawSpecialStageLinkHook;
        private IHook<DrawNowLoading> _drawNowLoadingHook;
        private IHook<TObjCreditsExecute> _executeCreditsHook;
        private IHook<DrawResultScreenLevelupDotsAndSomeOtherElements> _drawResultScreenDotsHook;
        private IHook<DrawPowerupBox> _drawPowerupBoxHook;
        private IHook<DrawSpecialStageEmeraldAndResultScreenGauge> _drawSpecialStageEmeraldHook;

        private Queue<bool> _shiftOrthographicProjection = new Queue<bool>();
        private bool _shiftProjectionFlag;

        public RenderHooks(float aspectRatioLimit, Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks hooks)
        {
            _memory = Memory.CurrentProcess;
            AspectConverter = new AspectConverter(aspectRatioLimit);
            
            _draw2PViewPortHook = Fun_DrawViewPorts.Hook(Draw2PViewportHook).Activate();
            _drawSpecialStageGaugeHook = Fun_DrawSpecialStageGauge.Hook(DrawSpecialStageGaugeImpl).Activate();
            _drawSpecialStageBarHook = Fun_DrawSpecialStageBar.Hook(DrawSpecialStageBarImpl).Activate();
            _draw2PStatusHook = Fun_DrawTwoPlayerStatusBar.Hook(Draw2pStatusImpl).Activate();
            _renderPrimitiveHook = Fun_D3D8Im2DRenderPrimitive.Hook(RenderPrimitiveImpl).Activate();
            _renderVideoHook = Fun_RenderVideoFrame.Hook(RenderVideoHookImpl).Activate();
            _drawFullVideoFrameHook = Fun_DrawFullVideoFrame.Hook(DrawFullVideoFrameHookImpl).Activate();
            _drawSmallVideoFrameHook = Fun_DrawSmallFrame.Hook(DrawSmallFrameImpl).Activate();
            _drawTitlecardElementsHook = Fun_DrawTitlecardElements.Hook(DrawTitlecardElementsImpl).Activate();
            _drawSpecialStageLinkHook = Fun_DrawSpecialStageLinkText.Hook(DrawSpecialStageLinkImpl).Activate();
            _drawNowLoadingHook = Fun_DrawNowLoading.Hook(DrawNowLoadingImpl).Activate();
            _executeCreditsHook = Fun_TObjCreditsExecute.Hook(ExecuteCredits).Activate();
            _drawResultScreenDotsHook = Fun_DrawResultScreenLevelupDotsAndSomeOtherElements.Hook(DrawResultScreenDotsImpl).Activate();
            _drawPowerupBoxHook = Fun_DrawPowerupBox.Hook(DrawPowerupBoxImpl).Activate();
            _drawSpecialStageEmeraldHook = Fun_DrawSpecialStageEmeraldAndResultScreenGauge.Hook(DrawSpecialStageEmeraldImpl).Activate();

            // Change permissions for game code regions.
            _memory.ChangePermission((IntPtr) _descriptionX, sizeof(void*), MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
            _memory.ChangePermission((IntPtr) _descriptionY, sizeof(void*), MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
            _memory.ChangePermission((IntPtr) _descriptionWidth, sizeof(void*), MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
            _memory.ChangePermission((IntPtr) _descriptionHeight, sizeof(void*), MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
            _memory.ChangePermission((IntPtr) _pickupBoxSeparation, sizeof(void*), MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
            _memory.ChangePermission((IntPtr) _dotsVertSeparation, sizeof(void*), MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
            _memory.ChangePermission((IntPtr) _dotsHorzSeparation, sizeof(void*), MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
            _memory.ChangePermission((IntPtr) _dotsHeight, sizeof(void*), MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
            _memory.ChangePermission((IntPtr) _dotsWidth, sizeof(void*), MEM_PROTECTION.PAGE_EXECUTE_READWRITE);


            _memory.ChangePermission((IntPtr)_dotsVertSeparation, sizeof(float), MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
            _memory.ChangePermission((IntPtr)_dotsHorzSeparation, sizeof(float), MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
            _memory.ChangePermission((IntPtr)_dotsHeight, sizeof(float), MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
            _memory.ChangePermission((IntPtr)_dotsWidth, sizeof(float), MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
        }

        public void SubscribeToResizeEventHook(ResizeEventHook hook)
        {
            hook.Resized += (in ResizeEventHook.ResizeEventHookData sender) =>
            {
                ActualAspectRatio = sender.ActualAspectRatio;
                RelativeAspectRatio = sender.RelativeAspectRatio;
                CurrentWidth = sender.CurrentWidth;
                CurrentHeight = sender.CurrentHeight;
            };
        }

        /* Patching resolutions in functions section */
        private int ExecuteCredits(int thisPtr)
        {
            _shiftProjectionFlag = true;
            PatchViewport();
            var result = _executeCreditsHook.OriginalFunction(thisPtr);
            _shiftProjectionFlag = false;
            return result;
        }

        private int* DrawNowLoadingImpl(int a1, char* a2, float* a3)
        {
            _shiftOrthographicProjection.Enqueue(true);
            return _drawNowLoadingHook.OriginalFunction(a1, a2, a3);
        }

        private int DrawSpecialStageLinkImpl(int preserveEax, int a1, float a2, float a3, float a4, float a5, int a6, int a7, int a8, int a9, int a10)
        {
            _shiftOrthographicProjection.Enqueue(true);
            return ExecuteWithScaleResolution(() => _drawSpecialStageLinkHook.OriginalFunction(preserveEax, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10));
        }

        private int DrawTitlecardElementsImpl(int thisPtr)
        {
            *_descriptionWidth  =  AspectConverter.ScaleByRelativeAspectX(DefaultMissionDescriptionWidth, RelativeAspectRatio, ActualAspectRatio);
            *_descriptionHeight =  AspectConverter.ScaleByRelativeAspectY(DefaultMissionDescriptionHeight, RelativeAspectRatio, ActualAspectRatio);
            var additionalBorderX = AspectConverter.GetBorderWidthX (ActualAspectRatio, AspectConverter.GameCanvasHeight) / 2F / AspectConverter.GameCanvasWidth;
            var additionalBorderY = AspectConverter.GetBorderHeightY(ActualAspectRatio, AspectConverter.GameCanvasWidth) / 2F / AspectConverter.GameCanvasHeight;

            *_descriptionX = AspectConverter.ScaleByRelativeAspectX(DefaultMissionDescriptionX, RelativeAspectRatio, ActualAspectRatio) + AspectConverter.ScaleByRelativeAspectX(additionalBorderX, RelativeAspectRatio, ActualAspectRatio);
            *_descriptionY = AspectConverter.ScaleByRelativeAspectY(DefaultMissionDescriptionY, RelativeAspectRatio, ActualAspectRatio) + AspectConverter.ScaleByRelativeAspectY(additionalBorderY, RelativeAspectRatio, ActualAspectRatio); ;
            
            var retVal = ExecuteWithScaleResolution(() => _drawTitlecardElementsHook.OriginalFunction(thisPtr));

            *_descriptionWidth = DefaultMissionDescriptionWidth;
            *_descriptionHeight = DefaultMissionDescriptionHeight;
            *_descriptionX = DefaultMissionDescriptionX;
            *_descriptionY = DefaultMissionDescriptionY;

            return retVal;
        }

        private int DrawSmallFrameImpl(int ebx, float x, float y, float width, float height, int a5)
        {
            x += AspectConverter.GetBorderWidthX(ActualAspectRatio, CurrentHeight) / 2;
            y += AspectConverter.GetBorderHeightY(ActualAspectRatio, CurrentWidth) / 2;

            return _drawSmallVideoFrameHook.OriginalFunction(ebx, x, y, width, height, a5);
        }

        private int DrawFullVideoFrameHookImpl(int ebx, float x, float y, float width, float height, int a5, float a6, float a7)
        {
            x = AspectConverter.GetBorderWidthX(ActualAspectRatio, CurrentHeight) / 2;
            y = AspectConverter.GetBorderHeightY(ActualAspectRatio, CurrentWidth) / 2;

            return _drawFullVideoFrameHook.OriginalFunction(ebx, x, y, width, height, a5, a6, a7);
        }

        private int RenderVideoHookImpl(VideoRenderProperties* a1, int a2)
        {
            // A1: 00AA4934 (Constant)
            return ExecuteWithScaleResolution(() => _renderVideoHook.OriginalFunction(a1, a2));
        }

        private bool RenderPrimitiveImpl(int a1, char* a2, int a3)
        {
            // Get vertex buffer.
            bool shift = false;

            if (_shiftOrthographicProjection.Count > 0)
                shift = _shiftOrthographicProjection.Dequeue();

            if (shift || _shiftProjectionFlag)
            {
                var vertexBufferPtr = *(VertexBufferSubmission**) 0xAA5048;
                var vertexBufferSubmissionThing = vertexBufferPtr->SubmissionThing != (void*) 0
                                                ? (*vertexBufferPtr).SubmissionThing
                                                : (VertexBufferSubmissionDetails*) Fun_GetVertexBufferSubmission.GetWrapper()();

                // Convert.ToInt32 performs rounding!
                var extraLeftBorder = (short)(Convert.ToInt32(AspectConverter.GetBorderWidthX(ActualAspectRatio, CurrentHeight) / 2));
                var extraTopBorder = (short)(Convert.ToInt32(AspectConverter.GetBorderHeightY(ActualAspectRatio, CurrentWidth) / 2));

                vertexBufferSubmissionThing->X += extraLeftBorder;
                vertexBufferSubmissionThing->Y += extraTopBorder;

                // Execute
                var result = _renderPrimitiveHook.OriginalFunction(a1, a2, a3);

                vertexBufferSubmissionThing->X -= extraLeftBorder;
                vertexBufferSubmissionThing->Y -= extraTopBorder;
                return result;
            }

            return _renderPrimitiveHook.OriginalFunction(a1, a2, a3);
        }

        private int Draw2pStatusImpl(int preserveEax, float x, float y, float width, float height)
        {
            _shiftOrthographicProjection.Enqueue(true);
            return _draw2PStatusHook.OriginalFunction(preserveEax,
                AspectConverter.ScaleByRelativeAspectX(x, RelativeAspectRatio, ActualAspectRatio),
                AspectConverter.ScaleByRelativeAspectY(y, RelativeAspectRatio, ActualAspectRatio),
                AspectConverter.ScaleByRelativeAspectX(width, RelativeAspectRatio, ActualAspectRatio),
                AspectConverter.ScaleByRelativeAspectY(height, RelativeAspectRatio, ActualAspectRatio));
        }

        private int DrawSpecialStageBarImpl(int preserveEax, float x, float y, float width, float height)
        {
            return _drawSpecialStageBarHook.OriginalFunction(preserveEax,
                AspectConverter.ProjectFromOldToNewCanvasX(x, RelativeAspectRatio, ActualAspectRatio),
                AspectConverter.ProjectFromOldToNewCanvasY(y, RelativeAspectRatio, ActualAspectRatio),
                AspectConverter.ScaleByRelativeAspectX(width, RelativeAspectRatio, ActualAspectRatio),
                AspectConverter.ScaleByRelativeAspectY(height, RelativeAspectRatio, ActualAspectRatio));
        }

        private int DrawSpecialStageGaugeImpl(int preserveEax, float x, float y, float width, float height, int a5, int a6, int a7, float a8, float a9)
        {
            return _drawSpecialStageGaugeHook.OriginalFunction(preserveEax,
                AspectConverter.ProjectFromOldToNewCanvasX(x, RelativeAspectRatio, ActualAspectRatio),
                AspectConverter.ProjectFromOldToNewCanvasY(y, RelativeAspectRatio, ActualAspectRatio),
                AspectConverter.ScaleByRelativeAspectX(width, RelativeAspectRatio, ActualAspectRatio),
                AspectConverter.ScaleByRelativeAspectY(height, RelativeAspectRatio, ActualAspectRatio),
                a5, a6, a7, a8, a9);
        }

        private int Draw2PViewportHook()
        {
            PatchViewport();
            return _draw2PViewPortHook.OriginalFunction();
        }

        private void* DrawSpecialStageEmeraldImpl(void* preserveEax, void* preserveEsi, float x, float y, float width, float height)
        {
            x = AspectConverter.ProjectFromOldToNewCanvasX(x, RelativeAspectRatio, ActualAspectRatio);
            y = AspectConverter.ProjectFromOldToNewCanvasY(y, RelativeAspectRatio, ActualAspectRatio);
            width = AspectConverter.ScaleByRelativeAspectX(width, RelativeAspectRatio, ActualAspectRatio);
            height = AspectConverter.ScaleByRelativeAspectY(height, RelativeAspectRatio, ActualAspectRatio);
            return _drawSpecialStageEmeraldHook.OriginalFunction(preserveEax, preserveEsi, x, y, width, height);
        }

        private void PatchViewport()
        {
            // Patch camera values for 2P Mode
            ref var engineInstance = ref State.EngineInstance.TryDereference(out bool success);
            if (success)
            {
                var screenViewPort = engineInstance.Graphics->ScreenViewport;
                var allViewPorts = engineInstance.Graphics->AllViewPorts;

                int halfHeight = CurrentHeight / 2;
                int halfWidth = CurrentWidth / 2;

                screenViewPort->Height = CurrentHeight;
                screenViewPort->Width = CurrentWidth;

                int cameraCount = *(int*)0xA60BE4;

                // Split camera horizontally.
                if (cameraCount <= 2)
                {
                    // 2P, Horizontal Splitscreen
                    allViewPorts->P1Viewport.Height = CurrentHeight;
                    allViewPorts->P1Viewport.Width = halfWidth;
                    allViewPorts->P1Viewport.OffsetX = 0;
                    allViewPorts->P1Viewport.OffsetY = 0;

                    allViewPorts->P2Viewport.Height = CurrentHeight;
                    allViewPorts->P2Viewport.Width = halfWidth;
                    allViewPorts->P2Viewport.OffsetX = (short)halfWidth;
                    allViewPorts->P2Viewport.OffsetY = 0;
                }
                else
                {
                    // 4P: Four quadrants
                    allViewPorts->P1Viewport.Height = halfHeight;
                    allViewPorts->P1Viewport.Width = halfWidth;
                    allViewPorts->P1Viewport.OffsetX = 0;
                    allViewPorts->P1Viewport.OffsetY = 0;

                    allViewPorts->P2Viewport.Height = halfHeight;
                    allViewPorts->P2Viewport.Width = halfWidth;
                    allViewPorts->P2Viewport.OffsetX = (short)halfWidth;
                    allViewPorts->P2Viewport.OffsetY = 0;

                    allViewPorts->P3Viewport.Height = halfHeight;
                    allViewPorts->P3Viewport.Width = halfWidth;
                    allViewPorts->P3Viewport.OffsetX = 0;
                    allViewPorts->P3Viewport.OffsetY = (short)halfHeight;

                    allViewPorts->P4Viewport.Height = halfHeight;
                    allViewPorts->P4Viewport.Width = halfWidth;
                    allViewPorts->P4Viewport.OffsetX = (short)(halfWidth);
                    allViewPorts->P4Viewport.OffsetY = (short)(halfHeight);
                }
            }
        }

        private void* DrawResultScreenDotsImpl()
        {
            _memory.Write((IntPtr)_dotsVertSeparation, AspectConverter.ScaleByRelativeAspectY(DefaultResultScreenDotsVerticalSeparation, RelativeAspectRatio, ActualAspectRatio));
            _memory.Write((IntPtr)_dotsHorzSeparation, AspectConverter.ScaleByRelativeAspectX(DefaultResultScreenDotsHorizontalSeparation, RelativeAspectRatio, ActualAspectRatio));
            _memory.Write((IntPtr)_dotsHeight, AspectConverter.ScaleByRelativeAspectY(DefaultResultScreenDotsHeight, RelativeAspectRatio, ActualAspectRatio));
            _memory.Write((IntPtr)_dotsWidth, AspectConverter.ScaleByRelativeAspectX(DefaultResultScreenDotsWidth, RelativeAspectRatio, ActualAspectRatio));

            var returnValue = _drawResultScreenDotsHook.OriginalFunction();

            _memory.Write((IntPtr)_dotsVertSeparation, DefaultResultScreenDotsVerticalSeparation);
            _memory.Write((IntPtr)_dotsHorzSeparation, DefaultResultScreenDotsHorizontalSeparation);
            _memory.Write((IntPtr)_dotsHeight, DefaultResultScreenDotsHeight);
            _memory.Write((IntPtr)_dotsWidth, DefaultResultScreenDotsWidth);

            return returnValue;
        }

        /// <summary>
        /// Executes a function, changing the resolution variables before and after execution.
        /// </summary>
        private T ExecuteWithScaleResolution<T>(Func<T> func)
        {
            Variables.ResolutionX.GetValue(out var backupX);
            Variables.ResolutionY.GetValue(out var backupY);

            int newX = (int)AspectConverter.ScaleByRelativeAspectX(backupX, RelativeAspectRatio, ActualAspectRatio);
            int newY = (int)AspectConverter.ScaleByRelativeAspectY(backupY, RelativeAspectRatio, ActualAspectRatio);
            Variables.ResolutionX.SetValue(ref newX);
            Variables.ResolutionY.SetValue(ref newY);

            var result = func();

            Variables.ResolutionX.SetValue(ref backupX);
            Variables.ResolutionY.SetValue(ref backupY);

            return result;
        }

        private void* DrawPowerupBoxImpl(void* preserveEax, float probablyX, float probablyY, float size)
        {
            *_pickupBoxSeparation = AspectConverter.ScaleByRelativeAspectX(DefaultPickupBoxSeparation, RelativeAspectRatio, ActualAspectRatio);
            var retVal = _drawPowerupBoxHook.OriginalFunction(preserveEax, probablyX, probablyY, AspectConverter.ScaleByRelativeAspectX(size, RelativeAspectRatio, ActualAspectRatio));
            *_pickupBoxSeparation = AspectConverter.ScaleByRelativeAspectX(DefaultPickupBoxSeparation, RelativeAspectRatio, ActualAspectRatio);

            return retVal;
        }
    }
}
