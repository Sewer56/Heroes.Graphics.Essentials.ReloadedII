using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Heroes.Graphics.Essentials.Shared;
using Heroes.Graphics.Essentials.Shared.Heroes;
using Heroes.Graphics.Essentials.Shared.Heroes.Structures;
using Heroes.Graphics.Essentials.Shared.Math;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory.Kernel32;
using Reloaded.Memory.Sources;

namespace Heroes.Graphics.Essentials.Hooks
{
    public unsafe class RenderHooks
    {
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

        private RwEngineInstance* _engineInstance = (RwEngineInstance*)0x008E0A4C;

        // Replace constants used in calculating titlecard text position.

        /* Ours */
        private Memory _memory;
        private IHook<sub_422AF0> _draw2PViewPortHook;
        private IHook<sub_5263C0> _drawSpecialStageGaugeHook;
        private IHook<sub_526280> _drawSpecialStageBarHook;
        private IHook<sub_422A70> _draw2PStatusHook;
        private IHook<_rwD3D8Im2DRenderPrimitive> _renderPrimitiveHook;
        private IHook<sub_644450> _renderVideoHook;
        private IHook<sub_458920> _drawSpecialStageEmeraldIndicatorHook;
        private IHook<DrawFullVideoFrame> _drawFullVideoFrameHook;
        private IHook<DrawSmallFrame> _drawSmallVideoFrameHook;
        private IHook<sub_442850> _drawTitlecardElementsHook;
        private IHook<sub_526F60> _drawSpecialStageLinkHook;
        private IHook<sub_44EAC0> _drawNowLoadingHook;
        private IHook<sub_4545F0> _executeCreditsHook;
        private IHook<sub_438A90> _drawResultScreenDotsHook;
        private IHook<DrawPowerupBox> _drawPowerupBoxHook;
        private sub_651E20        _getVertexBufferSubmission;

        private Queue<bool> _shiftOrthographicProjection = new Queue<bool>();
        private bool _shiftProjectionFlag;

        private AspectConverter _aspectConverter;

        public RenderHooks(float aspectRatioLimit, IReloadedHooks hooks)
        {
            _memory = Memory.CurrentProcess;
            _aspectConverter = new AspectConverter(aspectRatioLimit);
            _draw2PViewPortHook = hooks.CreateHook<sub_422AF0>(Draw2PViewportHook, 0x422AF0).Activate();
            _drawSpecialStageGaugeHook = hooks.CreateHook<sub_5263C0>(DrawSpecialStageGaugeImpl, 0x5263C0).Activate();
            _drawSpecialStageBarHook = hooks.CreateHook<sub_526280>(DrawSpecialStageBarImpl, 0x526280, 0xD).Activate();
            _draw2PStatusHook = hooks.CreateHook<sub_422A70>(Draw2pStatusImpl, 0x422A70).Activate();
            _renderPrimitiveHook = hooks.CreateHook<_rwD3D8Im2DRenderPrimitive>(RenderPrimitiveImpl, 0x00662B00).Activate();
            _renderVideoHook = hooks.CreateHook<sub_644450>(RenderVideoHookImpl, 0x644450).Activate();
            _drawFullVideoFrameHook = hooks.CreateHook<DrawFullVideoFrame>(DrawFullVideoFrameHookImpl, 0x0042A100).Activate();
            _drawSmallVideoFrameHook = hooks.CreateHook<DrawSmallFrame>(DrawSmallFrameImpl, 0x00429F80).Activate();
            _drawTitlecardElementsHook = hooks.CreateHook<sub_442850>(DrawTitlecardElementsImpl, 0x442850).Activate();
            _drawSpecialStageLinkHook = hooks.CreateHook<sub_526F60>(DrawSpecialStageLinkImpl, 0x526F60).Activate();
            _getVertexBufferSubmission = hooks.CreateWrapper<sub_651E20>(0x651E20, out _);
            _drawNowLoadingHook = hooks.CreateHook<sub_44EAC0>(DrawNowLoadingImpl, 0x44EAC0).Activate();
            _executeCreditsHook = hooks.CreateHook<sub_4545F0>(ExecuteCredits, 0x4545F0).Activate();
            _drawSpecialStageEmeraldIndicatorHook = hooks.CreateHook<sub_458920>(DrawSpecialStageEmeraldImpl, 0x458920).Activate();
            _drawResultScreenDotsHook = hooks.CreateHook<sub_438A90>(DrawResultScreenDotsImpl, 0x438A90).Activate();
            _drawPowerupBoxHook = hooks.CreateHook<DrawPowerupBox>(DrawPowerupBoxImpl, 0x479AB0).Activate();

            _memory.ChangePermission((IntPtr) _descriptionX, sizeof(void*), Kernel32.MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
            _memory.ChangePermission((IntPtr) _descriptionY, sizeof(void*), Kernel32.MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
            _memory.ChangePermission((IntPtr) _descriptionWidth, sizeof(void*), Kernel32.MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
            _memory.ChangePermission((IntPtr) _descriptionHeight, sizeof(void*), Kernel32.MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
            _memory.ChangePermission((IntPtr) _pickupBoxSeparation, sizeof(void*), Kernel32.MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
            _memory.ChangePermission((IntPtr) _dotsVertSeparation, sizeof(void*), Kernel32.MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
            _memory.ChangePermission((IntPtr) _dotsHorzSeparation, sizeof(void*), Kernel32.MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
            _memory.ChangePermission((IntPtr) _dotsHeight, sizeof(void*), Kernel32.MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
            _memory.ChangePermission((IntPtr) _dotsWidth, sizeof(void*), Kernel32.MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
        }

        public void SubscribeToResizeEventHook(ResizeEventHook hook)
        {
            hook.Resized += sender =>
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
            *_descriptionWidth  =  _aspectConverter.ScaleByRelativeAspectX(DefaultMissionDescriptionWidth, RelativeAspectRatio, ActualAspectRatio);
            *_descriptionHeight =  _aspectConverter.ScaleByRelativeAspectY(DefaultMissionDescriptionHeight, RelativeAspectRatio, ActualAspectRatio);
            var additionalBorderX = _aspectConverter.GetBorderWidthX (ActualAspectRatio, AspectConverter.GameCanvasHeight) / 2F / AspectConverter.GameCanvasWidth;
            var additionalBorderY = _aspectConverter.GetBorderHeightY(ActualAspectRatio, AspectConverter.GameCanvasWidth) / 2F / AspectConverter.GameCanvasHeight;

            *_descriptionX = _aspectConverter.ScaleByRelativeAspectX(DefaultMissionDescriptionX, RelativeAspectRatio, ActualAspectRatio) + _aspectConverter.ScaleByRelativeAspectX(additionalBorderX, RelativeAspectRatio, ActualAspectRatio);
            *_descriptionY = _aspectConverter.ScaleByRelativeAspectY(DefaultMissionDescriptionY, RelativeAspectRatio, ActualAspectRatio) + _aspectConverter.ScaleByRelativeAspectY(additionalBorderY, RelativeAspectRatio, ActualAspectRatio); ;
            
            var retVal = ExecuteWithScaleResolution(() => _drawTitlecardElementsHook.OriginalFunction(thisPtr));

            *_descriptionWidth = DefaultMissionDescriptionWidth;
            *_descriptionHeight = DefaultMissionDescriptionHeight;
            *_descriptionX = DefaultMissionDescriptionX;
            *_descriptionY = DefaultMissionDescriptionY;

            return retVal;
        }

        private int DrawSmallFrameImpl(int ebx, float x, float y, float width, float height, int a5)
        {
            x += _aspectConverter.GetBorderWidthX(ActualAspectRatio, CurrentHeight) / 2;
            y += _aspectConverter.GetBorderHeightY(ActualAspectRatio, CurrentWidth) / 2;

            return _drawSmallVideoFrameHook.OriginalFunction(ebx, x, y, width, height, a5);
        }

        private int DrawFullVideoFrameHookImpl(int ebx, float x, float y, float width, float height, int a5, float a6, float a7)
        {
            x = _aspectConverter.GetBorderWidthX(ActualAspectRatio, CurrentHeight) / 2;
            y = _aspectConverter.GetBorderHeightY(ActualAspectRatio, CurrentWidth) / 2;

            return _drawFullVideoFrameHook.OriginalFunction(ebx, x, y, width, height, a5, a6, a7);
        }

        private int RenderVideoHookImpl(VideoRenderThing* a1, int a2)
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
                VertexBufferSubmissionPtr* vertexBufferPtr = *(VertexBufferSubmissionPtr**)0xAA5048;
                VertexBufferSubmissionThing* vertexBufferSubmissionThing;
                if (*(int*)vertexBufferPtr != 0)
                    vertexBufferSubmissionThing = (*vertexBufferPtr).SubmissionThing;
                else
                    vertexBufferSubmissionThing = _getVertexBufferSubmission();

                // Convert.ToInt32 performs rounding!
                var extraLeftBorder = (short)(Convert.ToInt32(_aspectConverter.GetBorderWidthX(ActualAspectRatio, CurrentHeight) / 2));
                var extraTopBorder = (short)(Convert.ToInt32(_aspectConverter.GetBorderHeightY(ActualAspectRatio, CurrentWidth) / 2));

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
                _aspectConverter.ScaleByRelativeAspectX(x, RelativeAspectRatio, ActualAspectRatio),
                _aspectConverter.ScaleByRelativeAspectY(y, RelativeAspectRatio, ActualAspectRatio),
                _aspectConverter.ScaleByRelativeAspectX(width, RelativeAspectRatio, ActualAspectRatio),
                _aspectConverter.ScaleByRelativeAspectY(height, RelativeAspectRatio, ActualAspectRatio));
        }



        private int DrawSpecialStageBarImpl(int preserveEax, float x, float y, float width, float height)
        {
            return _drawSpecialStageBarHook.OriginalFunction(preserveEax,
                _aspectConverter.ProjectFromOldToNewCanvasX(x, RelativeAspectRatio, ActualAspectRatio),
                _aspectConverter.ProjectFromOldToNewCanvasY(y, RelativeAspectRatio, ActualAspectRatio),
                _aspectConverter.ScaleByRelativeAspectX(width, RelativeAspectRatio, ActualAspectRatio),
                _aspectConverter.ScaleByRelativeAspectY(height, RelativeAspectRatio, ActualAspectRatio));
        }

        private int DrawSpecialStageGaugeImpl(int preserveEax, float x, float y, float width, float height, int a5, int a6, int a7, float a8, float a9)
        {
            return _drawSpecialStageGaugeHook.OriginalFunction(preserveEax,
                _aspectConverter.ProjectFromOldToNewCanvasX(x, RelativeAspectRatio, ActualAspectRatio),
                _aspectConverter.ProjectFromOldToNewCanvasY(y, RelativeAspectRatio, ActualAspectRatio),
                _aspectConverter.ScaleByRelativeAspectX(width, RelativeAspectRatio, ActualAspectRatio),
                _aspectConverter.ScaleByRelativeAspectY(height, RelativeAspectRatio, ActualAspectRatio),
                a5, a6, a7, a8, a9);
        }

        private int Draw2PViewportHook()
        {
            PatchViewport();
            return _draw2PViewPortHook.OriginalFunction();
        }

        private void* DrawSpecialStageEmeraldImpl(void* preserveEax, void* preserveEsi, float x, float y, float width, float height)
        {
            return _drawSpecialStageEmeraldIndicatorHook.OriginalFunction(preserveEax, preserveEsi,
                _aspectConverter.ProjectFromOldToNewCanvasX(x, RelativeAspectRatio, ActualAspectRatio),
                _aspectConverter.ProjectFromOldToNewCanvasY(y, RelativeAspectRatio, ActualAspectRatio),
                _aspectConverter.ScaleByRelativeAspectX(width, RelativeAspectRatio, ActualAspectRatio),
                _aspectConverter.ScaleByRelativeAspectY(height, RelativeAspectRatio, ActualAspectRatio));
        }

        private void PatchViewport()
        {
            // Patch camera values for 2P Mode
            var screenViewPort = _engineInstance->Engine->ScreenRender->ScreenViewport;
            var allViewPorts   = _engineInstance->Engine->ScreenRender->AllViewPorts;

            int halfHeight = CurrentHeight / 2;
            int halfWidth  = CurrentWidth / 2;

            screenViewPort->Height = CurrentHeight;
            screenViewPort->Width  = CurrentWidth;

            int cameraCount = *(int*)0xA60BE4;

            // Split camera horizontally.
            if (cameraCount <= 2)
            {
                // 2P, Horizontal Splitscreen
                allViewPorts->P1Viewport.Height = CurrentHeight;
                allViewPorts->P1Viewport.Width  = halfWidth;
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

        private void* DrawResultScreenDotsImpl()
        {
            _memory.SafeWrite((IntPtr)_dotsVertSeparation, _aspectConverter.ScaleByRelativeAspectY(DefaultResultScreenDotsVerticalSeparation, RelativeAspectRatio, ActualAspectRatio));
            _memory.SafeWrite((IntPtr)_dotsHorzSeparation, _aspectConverter.ScaleByRelativeAspectX(DefaultResultScreenDotsHorizontalSeparation, RelativeAspectRatio, ActualAspectRatio));
            _memory.SafeWrite((IntPtr)_dotsHeight, _aspectConverter.ScaleByRelativeAspectY(DefaultResultScreenDotsHeight, RelativeAspectRatio, ActualAspectRatio));
            _memory.SafeWrite((IntPtr)_dotsWidth, _aspectConverter.ScaleByRelativeAspectX(DefaultResultScreenDotsWidth, RelativeAspectRatio, ActualAspectRatio));

            var returnValue = _drawResultScreenDotsHook.OriginalFunction();

            _memory.SafeWrite((IntPtr)_dotsVertSeparation, DefaultResultScreenDotsVerticalSeparation);
            _memory.SafeWrite((IntPtr)_dotsHorzSeparation, DefaultResultScreenDotsHorizontalSeparation);
            _memory.SafeWrite((IntPtr)_dotsHeight, DefaultResultScreenDotsHeight);
            _memory.SafeWrite((IntPtr)_dotsWidth, DefaultResultScreenDotsWidth);

            return returnValue;
        }

        /// <summary>
        /// Executes a function, changing the resolution variables before and after execution.
        /// </summary>
        private T ExecuteWithScaleResolution<T>(Func<T> func)
        {
            Variables.ResolutionX.GetValue(out var backupX);
            Variables.ResolutionY.GetValue(out var backupY);

            int newX = (int)_aspectConverter.ScaleByRelativeAspectX(backupX, RelativeAspectRatio, ActualAspectRatio);
            int newY = (int)_aspectConverter.ScaleByRelativeAspectY(backupY, RelativeAspectRatio, ActualAspectRatio);
            Variables.ResolutionX.SetValue(ref newX);
            Variables.ResolutionY.SetValue(ref newY);

            var result = func();

            Variables.ResolutionX.SetValue(ref backupX);
            Variables.ResolutionY.SetValue(ref backupY);

            return result;
        }

        private void* DrawPowerupBoxImpl(void* preserveEax, float probablyX, float probablyY, float size)
        {
            *_pickupBoxSeparation = _aspectConverter.ScaleByRelativeAspectX(DefaultPickupBoxSeparation, RelativeAspectRatio, ActualAspectRatio);
            var retVal = _drawPowerupBoxHook.OriginalFunction(preserveEax, probablyX, probablyY, _aspectConverter.ScaleByRelativeAspectX(size, RelativeAspectRatio, ActualAspectRatio));
            *_pickupBoxSeparation = _aspectConverter.ScaleByRelativeAspectX(DefaultPickupBoxSeparation, RelativeAspectRatio, ActualAspectRatio);

            return retVal;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(FunctionAttribute.Register.eax, FunctionAttribute.Register.eax, FunctionAttribute.StackCleanup.Caller)]
        public delegate int sub_5263C0(int preserveEax, float x, float y, float width, float height, int a5, int a6, int a7, float a8, float a9);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(FunctionAttribute.Register.eax, FunctionAttribute.Register.eax, FunctionAttribute.StackCleanup.Caller)]
        public delegate int sub_526280(int preserveEax, float x, float y, float width, float height);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(FunctionAttribute.Register.eax, FunctionAttribute.Register.eax, FunctionAttribute.StackCleanup.Caller)]
        public delegate int sub_422A70(int preserveEax, float x, float y, float width, float height);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(CallingConventions.Cdecl)]
        public delegate bool _rwD3D8Im2DRenderPrimitive(int a1, char* a2, int a3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(new[] { FunctionAttribute.Register.eax }, FunctionAttribute.Register.eax, FunctionAttribute.StackCleanup.Callee)]
        public delegate int sub_644450(VideoRenderThing* a1, int a2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(new[] { FunctionAttribute.Register.ebx }, FunctionAttribute.Register.eax, FunctionAttribute.StackCleanup.Caller)]
        public delegate int DrawFullVideoFrame(int ebx, float x, float y, float width, float height, int a5, float a6, float a7); // sub_42A100

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(new[] { FunctionAttribute.Register.ebx }, FunctionAttribute.Register.eax, FunctionAttribute.StackCleanup.Caller)]
        public delegate int DrawSmallFrame(int ebx, float x, float y, float width, float height, int a5);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(new[] { FunctionAttribute.Register.eax }, FunctionAttribute.Register.eax, FunctionAttribute.StackCleanup.Callee)]
        public delegate int sub_526F60(int preserveEax, int a1, float a2, float a3, float a4, float a5, int a6, int a7,
            int a8, int a9, int a10);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(new[] { FunctionAttribute.Register.eax, FunctionAttribute.Register.ecx, FunctionAttribute.Register.ebx }, FunctionAttribute.Register.eax, FunctionAttribute.StackCleanup.Caller)]
        public delegate int* sub_44EAC0(int a1, char* a2, float* a3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(CallingConventions.Cdecl)]
        public delegate VertexBufferSubmissionThing* sub_651E20(); // Gets vertex buffer submission ptr

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(CallingConventions.Cdecl)]
        public delegate int sub_422AF0();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(CallingConventions.MicrosoftThiscall)]
        public delegate int sub_442850(int thisPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(CallingConventions.MicrosoftThiscall)]
        public delegate int sub_4545F0(int thisPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(new[] { FunctionAttribute.Register.eax, FunctionAttribute.Register.esi }, FunctionAttribute.Register.eax, FunctionAttribute.StackCleanup.Callee)]
        public delegate void* sub_458920(void* preserveEax, void* preserveEsi, float x, float y, float width, float height);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(CallingConventions.Cdecl)]
        public delegate void* sub_438A90();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(new[] { FunctionAttribute.Register.eax }, FunctionAttribute.Register.eax, FunctionAttribute.StackCleanup.Callee)]
        public delegate void* DrawPowerupBox(void* preseveEax, float probablyX, float probablyY, float size); // 479AB0
    }
}
