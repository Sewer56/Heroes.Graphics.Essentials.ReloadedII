using System;
using Heroes.Graphics.Essentials.Shared.Heroes.Enums;

namespace Heroes.Graphics.Essentials.Shared.Heroes
{
    /// <summary>
    /// Default Sonic Heroes configurations normally accessible from the launcher.
    /// </summary>
    public unsafe class DefaultSettings
    {
        // Configuration fields.
        public bool Fullscreen              { get; set; } = false;
        public Language Language            { get; set; } = Language.English;
        public byte SFXVolume               { get; set; } = 100;
        public byte BGMVolume               { get; set; } = 100;
        public bool ThreeDimensionalSound   { get; set; } = true;
        public bool SFXOn                   { get; set; } = true;
        public bool BGMOn                   { get; set; } = true;
        public bool SoftShadows             { get; set; } = true;
        public int  MouseControl            { get; set; } = -1;
        public bool CharmyShutup            { get; set; } = false;
        public bool Fog                     { get; set; } = true;

        // Memory addresses.
        // ReSharper disable InconsistentNaming
        private static readonly int*  _fullScreen   = (int*)0x008CAEDC;
        private static readonly byte* _language     = (byte*)0x008CAEE1;
        private static readonly byte* _sfxVolume    = (byte*)0x008CAEE2;
        private static readonly byte* _bgmVolume    = (byte*)0x008CAEE3;
        private static readonly bool* _3dSound      = (bool*)0x008CAEE4;
        private static readonly bool* _sfxOn        = (bool*)0x008CAEE8;
        private static readonly bool* _bgmOn        = (bool*)0x008CAEEC;
        private static readonly bool* _cheapShadow  = (bool*)0x008CAEF0;
        private static readonly int*  _mouseControl = (int*)0x008CAEF4;
        private static readonly bool* _charmyShutup = (bool*)0x008CAEF8;
        private static readonly bool* _fog          = (bool*)0x008CAEB8;
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Applies this set of default settings to game memory.
        /// </summary>
        public void Apply()
        {
            *_fullScreen = Convert.ToInt32(Fullscreen);

            *_language  = (byte) Language;
            *_sfxVolume = SFXVolume;
            *_bgmVolume = BGMVolume;
            *_3dSound   = ThreeDimensionalSound;
            *_sfxOn     = SFXOn;
            *_bgmOn     = BGMOn;
            *_cheapShadow = !SoftShadows;
            *_mouseControl = MouseControl;
            *_charmyShutup = CharmyShutup;
            *_fog = Fog;
        }
    }
}
