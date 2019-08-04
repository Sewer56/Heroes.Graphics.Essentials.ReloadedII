using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Heroes.Graphics.Essentials.Heroes.Structures;
using Heroes.Graphics.Essentials.Utility;
using Heroes.Graphics.Essentials.Utility.Structs;
using Reloaded.Hooks;
using Reloaded.Hooks.Internal;
using Reloaded.Hooks.X86;
using Reloaded.Memory.Sources;
using Vanara.PInvoke;
using static Reloaded.Hooks.X86.FunctionAttribute;

namespace Heroes.Graphics.Essentials.Heroes
{
    public unsafe class ResolutionVariablePatcher
    {
        // Used to re-project the orthographic projection of some elements..
        private const int EventObjectLocationchange = 0x800B;
        private const float GameCanvasWidth  = 640;
        private const float GameCanvasHeight = 480;

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

        // Cached
        private int   _currentHeight;
        private int   _currentWidth;
        private float _actualAspectRatio;
        private float _relativeAspectRatio;

        // Game's
        private RwEngineInstance* _engineInstance = (RwEngineInstance*)0x008E0A4C;
        
        // Ours
        private Config.Config _config;
        private User32.WINEVENTPROC OnLocationChangeEventHandler;
        private IHook<sub_422AF0> _draw2PViewPortHook;
        private IHook<sub_5263C0> _drawSpecialStageGaugeHook;
        private IHook<sub_526280> _drawSpecialStageBarHook;
        private IHook<sub_458920> _drawSpecialStageEmeraldIndicatorHook;
        private IHook<sub_422A70> _draw2PStatusHook;
        private IHook<_rwD3D8Im2DRenderPrimitive> _renderPrimitiveHook;
        private IHook<sub_644450> _renderVideoHook;
        private IHook<DrawFullVideoFrame> _drawFullVideoFrameHook;
        private IHook<DrawSmallFrame> _drawSmallVideoFrameHook;
        private IHook<sub_442850> _drawTitlecardElementsHook;
        private IHook<sub_526F60> _drawSpecialStageLinkHook;
        private IHook<sub_44EAC0> _drawNowLoadingHook;
        private IHook<sub_4545F0> _executeCreditsHook;
        private sub_651E20 _getVertexBufferSubmission;

        private Queue<bool> _shiftOrthographicProjection = new Queue<bool>();
        private bool        _shiftProjectionFlag;

        public ResolutionVariablePatcher(Config.Config config)
        {
            _config = new Config.Config();
            OnLocationChangeEventHandler = OnLocationChange;
            User32.SetWinEventHook(EventObjectLocationchange, EventObjectLocationchange, HINSTANCE.NULL, OnLocationChangeEventHandler, (uint) Process.GetCurrentProcess().Id, 0, User32.WINEVENT.WINEVENT_OUTOFCONTEXT);
            _draw2PViewPortHook = new Hook<sub_422AF0>(Draw2PViewportHook, 0x422AF0).Activate();
            _drawSpecialStageGaugeHook = new Hook<sub_5263C0>(DrawSpecialStageGaugeImpl, 0x5263C0).Activate();
            _drawSpecialStageBarHook = new Hook<sub_526280>(DrawSpecialStageBarImpl, 0x526280, 0xD).Activate();
            _drawSpecialStageEmeraldIndicatorHook = new Hook<sub_458920>(DrawSpecialStageEmeraldImpl, 0x458920).Activate();
            _draw2PStatusHook = new Hook<sub_422A70>(Draw2pStatusImpl, 0x422A70).Activate();
            _renderPrimitiveHook = new Hook<_rwD3D8Im2DRenderPrimitive>(RenderPrimitiveImpl, 0x00662B00).Activate();
            _renderVideoHook = new Hook<sub_644450>(RenderVideoHookImpl, 0x644450).Activate();
            _drawFullVideoFrameHook = new Hook<DrawFullVideoFrame>(DrawFullVideoFrameHookImpl, 0x0042A100).Activate();
            _drawSmallVideoFrameHook = new Hook<DrawSmallFrame>(DrawSmallFrameImpl, 0x00429F80).Activate();
            _drawTitlecardElementsHook = new Hook<sub_442850>(DrawTitlecardElementsImpl, 0x442850).Activate();
            _drawSpecialStageLinkHook = new Hook<sub_526F60>(DrawSpecialStageLinkImpl, 0x526F60).Activate();
            _getVertexBufferSubmission = Wrapper.Create<sub_651E20>(0x651E20);
            _drawNowLoadingHook = new Hook<sub_44EAC0>(DrawNowLoadingImpl, 0x44EAC0).Activate();
            _executeCreditsHook = new Hook<sub_4545F0>(ExecuteCredits, 0x4545F0).Activate();
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
            return ExecuteWithScaleResolution(() => _drawTitlecardElementsHook.OriginalFunction(thisPtr));
        }

        private int DrawSmallFrameImpl(int ebx, float x, float y, float width, float height, int a5)
        {
            x += GetBorderWidthX(_actualAspectRatio, _currentHeight) / 2;
            y += GetBorderHeightY(_actualAspectRatio, _currentWidth) / 2;
            
            return _drawSmallVideoFrameHook.OriginalFunction(ebx, x, y, width, height, a5);
        }

        private int DrawFullVideoFrameHookImpl(int ebx, float x, float y, float width, float height, int a5, float a6, float a7)
        {
            x = GetBorderWidthX(_actualAspectRatio, _currentHeight) / 2;
            y = GetBorderHeightY(_actualAspectRatio, _currentWidth) / 2;

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
                var extraLeftBorder = (short)(Convert.ToInt32(GetBorderWidthX(_actualAspectRatio, _currentHeight) / 2));
                var extraTopBorder = (short)(Convert.ToInt32(GetBorderHeightY(_actualAspectRatio, _currentWidth) / 2));

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
                ScaleByRelativeAspectX(x, _relativeAspectRatio, _actualAspectRatio),
                ScaleByRelativeAspectY(y, _relativeAspectRatio, _actualAspectRatio),
                ScaleByRelativeAspectX(width, _relativeAspectRatio, _actualAspectRatio),
                ScaleByRelativeAspectY(height, _relativeAspectRatio, _actualAspectRatio));
        }

        private int DrawSpecialStageEmeraldImpl(int preserveEax, float x, float y, float width, float height)
        {
            return _drawSpecialStageEmeraldIndicatorHook.OriginalFunction(preserveEax,
                ProjectFromOldToNewCanvasX(x, _relativeAspectRatio, _actualAspectRatio),
                ProjectFromOldToNewCanvasY(y, _relativeAspectRatio, _actualAspectRatio),
                ScaleByRelativeAspectX(width, _relativeAspectRatio, _actualAspectRatio),
                ScaleByRelativeAspectY(height, _relativeAspectRatio, _actualAspectRatio));
        }

        private int DrawSpecialStageBarImpl(int preserveEax, float x, float y, float width, float height)
        {
            return _drawSpecialStageBarHook.OriginalFunction(preserveEax,
                ProjectFromOldToNewCanvasX(x, _relativeAspectRatio, _actualAspectRatio),
                ProjectFromOldToNewCanvasY(y, _relativeAspectRatio, _actualAspectRatio),
                ScaleByRelativeAspectX(width, _relativeAspectRatio, _actualAspectRatio),
                ScaleByRelativeAspectY(height, _relativeAspectRatio, _actualAspectRatio));
        }

        private int DrawSpecialStageGaugeImpl(int preserveEax, float x, float y, float width, float height, int a5, int a6, int a7, float a8, float a9)
        {
            return _drawSpecialStageGaugeHook.OriginalFunction(preserveEax, 
                ProjectFromOldToNewCanvasX(x, _relativeAspectRatio, _actualAspectRatio), 
                ProjectFromOldToNewCanvasY(y, _relativeAspectRatio, _actualAspectRatio),
                ScaleByRelativeAspectX(width, _relativeAspectRatio, _actualAspectRatio), 
                ScaleByRelativeAspectY(height, _relativeAspectRatio, _actualAspectRatio), 
                a5, a6, a7, a8, a9);
        }

        private byte Draw2PViewportHook()
        {
            PatchViewport();
            return _draw2PViewPortHook.OriginalFunction();
        }

        private void PatchViewport()
        {
            // Patch camera values for 1P & 2P.
            var cameraController = _engineInstance->Engine->MultiplayerRender->MultiplayerCameraController;

            cameraController->ScreenHeight = _currentHeight;
            cameraController->ScreenWidth = _currentWidth;
            cameraController->UnknownDefaultScreenWidth = _currentWidth;
            cameraController->UnknownDefaultScreenHeight = _currentHeight;
            cameraController->UnknownScreenHeight = _currentHeight;
            cameraController->UnknownScreenWidth = _currentWidth;

            cameraController->PlayerOneViewport.PlayerOneViewPortHeight = _currentHeight;
            cameraController->PlayerOneViewport.PlayerOneViewPortWidth = _currentWidth / 2;
            cameraController->PlayerThreeViewport.OffsetX = 0;

            cameraController->PlayerThreeViewport.PlayerOneViewPortHeight = _currentHeight;
            cameraController->PlayerThreeViewport.PlayerOneViewPortWidth = _currentWidth / 2;
            cameraController->PlayerThreeViewport.OffsetX = _currentWidth / 2;
        }

        /* Patching Resolution Changes Section */
        private void OnLocationChange(User32.HWINEVENTHOOK hWinEventHook, uint winEvent, HWND hWnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime)
        {
            if (idObject != 0 || idChild != 0)
                return;

            if (winEvent == EventObjectLocationchange)
            {
                // Two things performed here.
                RECT rect = new RECT();
                User32_Gdi.GetClientRect(Variables.WindowHandle, ref rect);

                Patch(rect.Width, rect.Height);
            }
        }

        /// <summary>
        /// Patches, mostly hardcoded resolution variables.
        /// </summary>
        public void Patch(int width, int height)
        {
            float floatWidth = width;
            float floatHeight = height;

            _currentHeight = height;
            _currentWidth = width;
            _actualAspectRatio   = width / (float) height;
            _relativeAspectRatio = AspectConverter.GetRelativeAspect(_actualAspectRatio);

            // Set game resolution variables.
            Variables.ResolutionX.SetValue(ref width);
            Variables.ResolutionY.SetValue(ref height);

            Variables.MaestroResolutionX.SetValue(ref floatWidth);
            Variables.MaestroResolutionY.SetValue(ref floatHeight);

            // Pickup boxes.
            float* pickupBoxSize       = (float*) 0x78A240;
            float* pickupBoxSeparation = (float*) 0x78A618;
            Memory.CurrentProcess.SafeWrite((IntPtr)pickupBoxSize,       ScaleByRelativeAspectX(0.03750000149F, _relativeAspectRatio, _actualAspectRatio));
            Memory.CurrentProcess.SafeWrite((IntPtr)pickupBoxSeparation, ScaleByRelativeAspectX(52F, _relativeAspectRatio, _actualAspectRatio));

            // Stage description.
            float* descriptionX      = (float*)0x78A4EC;
            float* descriptionY      = (float*)0x78A4E8;
            float* descriptionWidth  = (float*)0x745F3C;
            float* descriptionHeight = (float*)0x78A4E4;

            Memory.CurrentProcess.SafeWrite((IntPtr)descriptionWidth , ScaleByRelativeAspectX(DefaultMissionDescriptionWidth, _relativeAspectRatio, _actualAspectRatio));
            Memory.CurrentProcess.SafeWrite((IntPtr)descriptionHeight, ScaleByRelativeAspectY(DefaultMissionDescriptionHeight, _relativeAspectRatio, _actualAspectRatio));

            // => This item is not adjusted horizontally/vertically because the function that
            // draws it cannot be made to set _shiftOrthographicProjection.Enqueue(true)
            // Reason it cannot is because Last story for some reason calls the function in the BG.
            float gameScaleX = DefaultMissionDescriptionX * GameCanvasWidth;
            float gameScaleY = DefaultMissionDescriptionY * GameCanvasHeight;
            gameScaleX       = ProjectFromOldToNewCanvasX(gameScaleX, _relativeAspectRatio, _actualAspectRatio) / GameCanvasWidth;
            gameScaleY       = ProjectFromOldToNewCanvasY(gameScaleY, _relativeAspectRatio, _actualAspectRatio) / GameCanvasHeight;

            Memory.CurrentProcess.SafeWrite((IntPtr)descriptionX     , gameScaleX);
            Memory.CurrentProcess.SafeWrite((IntPtr)descriptionY     , gameScaleY);

            // Result screen
            float* dotsPercentageX = (float*)0x745BA8;
            float* dotsPercentageY = (float*)0x745F40;
            float* dotsVertSeparation = (float*)0x78A504;
            float* dotsHorzSeparation = (float*)0x78A248;
            float* dotsHeight = (float*) 0x78A508;
            float* dotsWidth = (float*) 0x78A31C;

            float percentOffsetXFromCenter = 0.5F - DefaultResultScreenDotsPercentageX;
            float newPercentageX           = 0.5F - ScaleByRelativeAspectX(percentOffsetXFromCenter, _relativeAspectRatio, _actualAspectRatio);

            float percentOffsetYFromCenter = 0.5F - DefaultResultScreenDotsPercentageY;
            float newPercentageY           = 0.5F - ScaleByRelativeAspectY(percentOffsetYFromCenter, _relativeAspectRatio, _actualAspectRatio);

            Memory.CurrentProcess.SafeWrite((IntPtr)dotsPercentageX, newPercentageX);
            Memory.CurrentProcess.SafeWrite((IntPtr)dotsPercentageY, newPercentageY);
            Memory.CurrentProcess.SafeWrite((IntPtr)dotsVertSeparation, ScaleByRelativeAspectY(DefaultResultScreenDotsVerticalSeparation, _relativeAspectRatio, _actualAspectRatio));
            Memory.CurrentProcess.SafeWrite((IntPtr)dotsHorzSeparation, ScaleByRelativeAspectX(DefaultResultScreenDotsHorizontalSeparation, _relativeAspectRatio, _actualAspectRatio));
            Memory.CurrentProcess.SafeWrite((IntPtr)dotsHeight, ScaleByRelativeAspectY(DefaultResultScreenDotsHeight, _relativeAspectRatio, _actualAspectRatio));
            Memory.CurrentProcess.SafeWrite((IntPtr)dotsWidth, ScaleByRelativeAspectX(DefaultResultScreenDotsWidth, _relativeAspectRatio, _actualAspectRatio));

            // Font notes
            // MESSAGE STRUCT SIZE: 0x1B4
            // POINTER TO SINGLETON INSTANCE AT 0xA777C8

            // Since Donut will probably be hogging the disassembly for another year, I can't update anything.
            // Here's the relevant offsets from the old code before complete switch to hooks.

            #region Old Code

            // A. Scaling
            // If Aspect > Aspect Limit, Shrink X
            // If Aspect < Aspect Limit, Shrink Y
            // See: rwCamera Stretch/Unstretch

            // B: Projecting
            // Converting object coordinates of 640x480 orthographic projections
            // to new object coordinates of our resolution.

            // Special stage gauge outline.
            // Memory.CurrentProcess.SafeWrite((IntPtr)0x527407, ProjectFromOldToNewCanvasX(460, relativeAspectRatio, actualAspectRatio)); // X
            // Memory.CurrentProcess.SafeWrite((IntPtr)0x527402, ProjectFromOldToNewCanvasY(38, relativeAspectRatio, actualAspectRatio));  // Y
            // Memory.CurrentProcess.SafeWrite((IntPtr)0x5273FD, ScaleByRelativeAspectX(128, relativeAspectRatio, actualAspectRatio));     // Width
            // Memory.CurrentProcess.SafeWrite((IntPtr)0x5273F8, ScaleByRelativeAspectY(32, relativeAspectRatio, actualAspectRatio));      // Height

            // Special stage gauge outline: Split Screen 1P
            // Memory.CurrentProcess.SafeWrite((IntPtr)0x52743F, ProjectFromOldToNewCanvasX(170, relativeAspectRatio, actualAspectRatio)); // X
            // Memory.CurrentProcess.SafeWrite((IntPtr)0x52743A, ProjectFromOldToNewCanvasY(38, relativeAspectRatio, actualAspectRatio));  // Y
            // Memory.CurrentProcess.SafeWrite((IntPtr)0x527435, ScaleByRelativeAspectX(128, relativeAspectRatio, actualAspectRatio));     // Width
            // Memory.CurrentProcess.SafeWrite((IntPtr)0x527430, ScaleByRelativeAspectY(32, relativeAspectRatio, actualAspectRatio));      // Height

            // Special stage gauge outline: Split Screen 2P
            // Scaling here the character switcher rendered by perspective projection cannot be moved.
            // Memory.CurrentProcess.SafeWrite((IntPtr)0x527467, ScaleByRelativeAspectX(140, relativeAspectRatio, actualAspectRatio));     // X
            // Memory.CurrentProcess.SafeWrite((IntPtr)0x527462, ProjectFromOldToNewCanvasY(38, relativeAspectRatio, actualAspectRatio));  // Y
            // Memory.CurrentProcess.SafeWrite((IntPtr)0x52745D, ScaleByRelativeAspectX(128, relativeAspectRatio, actualAspectRatio));     // Width
            // Memory.CurrentProcess.SafeWrite((IntPtr)0x527458, ScaleByRelativeAspectY(32, relativeAspectRatio, actualAspectRatio));      // Height

            // Special gauge (actual gauge)
            // Memory.CurrentProcess.SafeWrite((IntPtr)0x5274A1, ProjectFromOldToNewCanvasX(640, relativeAspectRatio, actualAspectRatio)); // Calculating X
            // Memory.CurrentProcess.SafeWrite((IntPtr)0x5274AB, ProjectFromOldToNewCanvasX(547, relativeAspectRatio, actualAspectRatio)); // Calculating X
            // Memory.CurrentProcess.SafeWrite((IntPtr)0x5274B5, ProjectFromOldToNewCanvasX(507, relativeAspectRatio, actualAspectRatio)); // Calculating X

            // Memory.CurrentProcess.SafeWrite((IntPtr)0x5274BF, ProjectFromOldToNewCanvasX(467, relativeAspectRatio, actualAspectRatio)); // X 2P
            // Memory.CurrentProcess.SafeWrite((IntPtr)0x745AF8, ProjectFromOldToNewCanvasX(30, relativeAspectRatio, actualAspectRatio));  // X Offset

            #endregion
        }

        /// <summary>
        /// Scales a width value by the relative aspect ratio.
        /// </summary>
        private float ScaleByRelativeAspectX(float value, float relativeAspectRatio, float actualAspect)
        {
            if (actualAspect > _config.AspectRatioLimit)
            {
                return value / relativeAspectRatio;
            }

            return value;
        }

        /// <summary>
        /// Scales a height value by the relative aspect ratio.
        /// </summary>
        private float ScaleByRelativeAspectY(float value, float relativeAspectRatio, float actualAspect)
        {
            if (actualAspect < _config.AspectRatioLimit)
            {
                return value * relativeAspectRatio;
            }

            return value;
        }

        /// <summary>
        /// Returns the extra width added by the left and right borders extending beyond the 4:3 aspect.
        /// </summary>
        private float GetBorderWidthX(float actualAspect, float height)
        {
            if (actualAspect > _config.AspectRatioLimit)
            {
                AspectConverter.HeightToResolution((int)height, actualAspect                      , out Resolution resolutionOurAspect);
                AspectConverter.HeightToResolution((int)height, AspectConverter.OriginalGameAspect, out Resolution resolutionGameAspect);

                return resolutionOurAspect.Width - resolutionGameAspect.Width;
            }

            return 0;
        }

        /// <summary>
        /// Returns the extra height added by the top and bottom borders extending before the 4:3 aspect.
        /// </summary>
        private float GetBorderHeightY(float actualAspect, float width)
        {
            if (actualAspect < _config.AspectRatioLimit)
            {
                AspectConverter.WidthToResolution((int)width, actualAspect, out Resolution resolutionOurAspect);
                AspectConverter.WidthToResolution((int)width, AspectConverter.OriginalGameAspect, out Resolution resolutionGameAspect);

                return resolutionOurAspect.Height - resolutionGameAspect.Height;
            }

            return 0;
        }

        /// <summary>
        /// Returns the extra width added by the left and right borders extending beyond the 4:3 aspect.
        /// Note: Assumes resolution is 640x480.
        /// </summary>
        private float GetGameSizeBorderWidthX(float actualAspect) => GetBorderWidthX(actualAspect, GameCanvasHeight);

        /// <summary>
        /// Returns the extra height added by the top and bottom borders extending before the 4:3 aspect.
        /// Note: Assumes resolution is 640x480.
        /// </summary>
        private float GetGameSizeBorderHeightY(float actualAspect) => GetBorderHeightY(actualAspect, GameCanvasWidth);

        /// <summary>
        /// Used for shifting item locations of an orthographic projection (e.g. special stage HUD)
        /// that are relative to the left edge of the screen.
        /// Note: Assumes resolution is 640x480.
        /// </summary>
        /// <param name="originalPosition">Original position of the object.</param>
        /// <param name="relativeAspectRatio">Relative aspect ratio of the desired aspect compared to game's aspect.</param>
        /// <param name="actualAspect">The desired aspect ratio.</param>
        private float ProjectFromOldToNewCanvasX(float originalPosition, float relativeAspectRatio, float actualAspect)
        {
            if (actualAspect > _config.AspectRatioLimit)
            {
                // Now the projection is the right size, however it is not centered to our screen.
                AspectConverter.HeightToResolution((int) GameCanvasHeight, actualAspect, out Resolution resolution); // Get resolution with our aspect equal to the height. 
                float borderWidth               = resolution.Width - GameCanvasWidth;   // Get the extra width (left and right border)
                float leftBorderOnly            = (borderWidth / 2);                    // We only want left border.
                float originalPlusLeftBorder    = leftBorderOnly + originalPosition;

                return originalPlusLeftBorder / relativeAspectRatio;
            }

            return originalPosition;
        }

        /// <summary>
        /// Used for shifting item locations of an orthographic projection (e.g. special stage HUD)
        /// that are relative to the top edge of the screen.
        /// Note: Assumes resolution is 640x480.
        /// </summary>
        /// <param name="originalPosition">Original position of the object.</param>
        /// <param name="relativeAspectRatio">Relative aspect ratio of the desired aspect compared to game's aspect.</param>
        /// <param name="actualAspect">The desired aspect ratio.</param>
        private float ProjectFromOldToNewCanvasY(float originalPosition, float relativeAspectRatio, float actualAspect)
        {
            if (actualAspect < _config.AspectRatioLimit)
            {
                // Now the projection is the right size, however it is not centered to our screen.
                AspectConverter.WidthToResolution((int)GameCanvasWidth, actualAspect, out Resolution resolution); // Get resolution with our aspect equal to the height. 
                float borderHeight = resolution.Height - GameCanvasHeight;   // Get the extra height (top and bottom border)
                float topBorderOnly = (borderHeight / 2);                    // We only want top border.
                float originalPlusTopBorder = topBorderOnly + originalPosition; // Our top border is in the aspect ratio it originated from,
                                                                             // we need to scale it to the new ratio.

                return originalPlusTopBorder * relativeAspectRatio;
            }

            return originalPosition;
        }

        /// <summary>
        /// Executes a function, changing the resolution variables before and after execution.
        /// </summary>
        private T ExecuteWithScaleResolution<T>(Func<T> func)
        {
            Variables.ResolutionX.GetValue(out var backupX);
            Variables.ResolutionY.GetValue(out var backupY);

            int newX = (int) ScaleByRelativeAspectX(backupX, _relativeAspectRatio, _actualAspectRatio);
            int newY = (int) ScaleByRelativeAspectY(backupY, _relativeAspectRatio, _actualAspectRatio);
            Variables.ResolutionX.SetValue(ref newX);
            Variables.ResolutionY.SetValue(ref newY);

            var result = func();

            Variables.ResolutionX.SetValue(ref backupX);
            Variables.ResolutionY.SetValue(ref backupY);

            return result;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(Register.eax, Register.eax, StackCleanup.Caller)]
        public delegate int sub_5263C0(int preserveEax, float x, float y, float width, float height, int a5, int a6, int a7, float a8, float a9);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(Register.eax, Register.eax, StackCleanup.Caller)]
        public delegate int sub_526280(int preserveEax, float x, float y, float width, float height);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(Register.eax, Register.eax, StackCleanup.Callee)]
        public delegate int sub_458920(int preserveEax, float x, float y, float width, float height);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(Register.eax, Register.eax, StackCleanup.Caller)]
        public delegate int sub_422A70(int preserveEax, float x, float y, float width, float height);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(CallingConventions.Cdecl)]
        public delegate bool _rwD3D8Im2DRenderPrimitive(int a1, char* a2, int a3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(new[] {Register.eax}, Register.eax, StackCleanup.Callee)]
        public delegate int sub_644450(VideoRenderThing* a1, int a2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(new []{ Register.ebx }, Register.eax, StackCleanup.Caller)]
        public delegate int DrawFullVideoFrame(int ebx, float x, float y, float width, float height, int a5, float a6, float a7); // sub_42A100

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(new[] { Register.ebx }, Register.eax, StackCleanup.Caller)]
        public delegate int DrawSmallFrame(int ebx, float x, float y, float width, float height, int a5);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(new[] {Register.eax}, Register.eax, StackCleanup.Callee)]
        public delegate int sub_526F60(int preserveEax, int a1, float a2, float a3, float a4, float a5, int a6, int a7,
            int a8, int a9, int a10);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(new[] { Register.eax, Register.ecx, Register.ebx }, Register.eax, StackCleanup.Caller)]
        public delegate int* sub_44EAC0(int a1, char* a2, float* a3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(CallingConventions.Cdecl)]
        public delegate VertexBufferSubmissionThing* sub_651E20(); // Gets vertex buffer submission ptr

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(CallingConventions.Cdecl)]
        public delegate byte sub_422AF0();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(CallingConventions.MicrosoftThiscall)]
        public delegate int sub_442850(int thisPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [Function(CallingConventions.MicrosoftThiscall)]
        public delegate int sub_4545F0(int thisPtr);
    }
}
