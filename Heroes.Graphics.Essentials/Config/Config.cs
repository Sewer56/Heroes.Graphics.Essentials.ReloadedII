using System.ComponentModel;
using Heroes.Graphics.Essentials.Configuration;
using Heroes.Graphics.Essentials.Heroes;

namespace Heroes.Graphics.Essentials.Config;

public class Config : Configurable<Config>
{
    [DisplayName("Width")]
    [Description("Width of the game window, in pixels")]
    [DefaultValue(1280)]
    public int  Width                       { get; set; } = 1280;

    [DisplayName("Height")]
    [Description("Height of the game window, in pixels")]
    [DefaultValue(720)]
    public int  Height                      { get; set; } = 720;

    [DisplayName("Borderless Windowed")]
    [Description("If set to true and game is in windowed mode, the window will have no border.")]
    [DefaultValue(true)]
    public bool BorderlessWindowed          { get; set; } = true;

    [DisplayName("Resizable Window")]
    [Description("If set to true, the game window can be resized.")]
    [DefaultValue(false)]
    public bool ResizableWindowed           { get; set; } = false;

    [DisplayName("High Aspect Ratio Crashfix")]
    [Description("Workaround for borderless windowed that prevents the game crashing on titlecards if the aspect ratio of the game is ridiculous.")]
    [DefaultValue(true)]
    public bool HighAspectRatioCrashFix     { get; set; } = true;

    [DisplayName("Aspect Ratio Limit")]
    [Description("If the game window is below this aspect ratio, the widescreen hack scales the window vertically instead of horizontally.")]
    [DefaultValue((float)(4.0F / 3.0F))]
    public float AspectRatioLimit           { get; set; } = 4 / 3F;

    [DisplayName("Disable 2P Frameskip")]
    [Description("Disables the frame skipping behaviour in 2 player mode, allowing you to play at 60 FPS.")]
    [DefaultValue(true)]
    public bool Disable2PFrameskip          { get; set; } = true;

    [DisplayName("Fast Stage Load Times")]
    [Description("A hack that skips waiting in titlecards, causing levels to load instantly.")]
    [DefaultValue(true)]
    public bool StupidlyFastLoadTimes       { get; set; } = true;

    [DisplayName("Do Not Slowdown on Focus Loss")]
    [Description("Stops the game from slowing down when focus is lost.")]
    [DefaultValue(true)]
    public bool DontSlowdownOnFocusLost { get; set; } = true;

    [DisplayName("Center Window to Screen")]
    [Description("Moves the window to the center of the screen.")]
    [DefaultValue(true)]
    public bool CenterWindow { get; set; } = true;
    
    [DisplayName("Remove Subtitles")]
    [Description("Removes the in-game subtitles.")]
    [DefaultValue(false)]
    public bool NoSubtitles { get; set; } = false;

    [DisplayName("Default Settings")]
    [Description("The regular settings from the Sonic Heroes launcher.")]
    public DefaultSettings DefaultSettings  { get; set; } = new DefaultSettings();
}