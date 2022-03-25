using System;
using Heroes.SDK.API;
using Heroes.SDK.Definitions.Enums;

namespace Heroes.Graphics.Essentials.Heroes;

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

    /// <summary>
    /// Applies this set of default settings to game memory.
    /// </summary>
    public void Apply()
    {
        Misc.DefaultSettings.FullScreen = Fullscreen;
        Misc.DefaultSettings.Language = Language;
        Misc.DefaultSettings.SfxVolume = SFXVolume;
        Misc.DefaultSettings.BgmVolume = BGMVolume;
        Misc.DefaultSettings.SurroundSound = ThreeDimensionalSound;
        Misc.DefaultSettings.SfxOn = SFXOn;
        Misc.DefaultSettings.BgmOn = BGMOn;
        Misc.DefaultSettings.CheapShadow = !SoftShadows;
        Misc.DefaultSettings.MouseControl = MouseControl;
        Misc.DefaultSettings.CharmyShutup = CharmyShutup;
        Misc.DefaultSettings.Fog = Fog;
    }
}