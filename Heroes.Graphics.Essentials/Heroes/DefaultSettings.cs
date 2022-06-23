using System;
using System.ComponentModel;
using Heroes.SDK.API;
using Heroes.SDK.Definitions.Enums;

namespace Heroes.Graphics.Essentials.Heroes;

/// <summary>
/// Default Sonic Heroes configurations normally accessible from the launcher.
/// </summary>
public unsafe class DefaultSettings
{
    // Configuration fields.

    [DisplayName("Full Screen")]
    [Description("Makes the game run in exclusive fullscreen mode.")]
    [DefaultValue(false)]
    public bool Fullscreen              { get; set; } = false;

    [DisplayName("Language")]
    [Description("Default language for the game. Overwritten by save file.")]
    [DefaultValue(Language.English)]
    public Language Language            { get; set; } = Language.English;

    [DisplayName("Sound FX Volume")]
    [Description("Volume for sound effects. Between 0 and 100.")]
    [DefaultValue((byte)100)]
    public byte SFXVolume               { get; set; } = 100;

    [DisplayName("Music Volume")]
    [Description("Volume for music. Between 0 and 100.")]
    [DefaultValue((byte)100)]
    public byte BGMVolume               { get; set; } = 100;

    [DisplayName("3D Sound")]
    [Description("Basically Surround Sound.")]
    [DefaultValue(true)]
    public bool ThreeDimensionalSound   { get; set; } = true;

    [DisplayName("Enable Sound Effects")]
    [DefaultValue(true)]
    public bool SFXOn                   { get; set; } = true;

    [DisplayName("Enable Music")]
    [DefaultValue(true)]
    public bool BGMOn                   { get; set; } = true;

    [DisplayName("Soft Shadows")]
    [Description("When enabled, shadow models are accurate to the characters/enemies.\n" +
                 "When disabled, uses circles for shadows.")]
    [DefaultValue(true)]
    public bool SoftShadows             { get; set; } = true;

    [DisplayName("Enable Mouse Control")]
    [Description("Setting intended for Masochists")]
    [DefaultValue(-1)]
    public int  MouseControl            { get; set; } = -1;

    [DisplayName("Disable Character Dialogue")]
    [Description("Disables certain character dialogue.\n" +
                 "Internally known as CharmyShutup in game code.")]
    [DefaultValue(false)]
    public bool CharmyShutup            { get; set; } = false;

    [DefaultValue(true)]
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